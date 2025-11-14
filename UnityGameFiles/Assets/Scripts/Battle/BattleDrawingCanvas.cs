using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Drawing canvas specifically for battle moves.
    /// Captures player's drawing input during combat turns.
    /// </summary>
    public class BattleDrawingCanvas : MonoBehaviour
    {
        [Header("Canvas Settings")]
        [SerializeField] private RectTransform drawingArea;
        [SerializeField] private Canvas canvas;
        [SerializeField] private float lineWidth = 5f; // Much thicker for canvas local space visibility
        [SerializeField] private Color drawingColor = Color.black;

        [Header("Line Rendering")]
        [SerializeField] private GameObject lineRendererPrefab;
        [SerializeField] private Material lineMaterial;

        [Header("Drawing State")]
        [SerializeField] private bool isDrawingEnabled = false;

        // Current stroke data
        private LineRenderer currentLine;
        private List<Vector2> currentStrokePoints = new List<Vector2>();

        // All strokes in current drawing
        private List<LineRenderer> allLines = new List<LineRenderer>();
        private List<List<Vector2>> allStrokes = new List<List<Vector2>>();

        // Drawing tracking
        private bool isDrawing = false;
        private Camera mainCamera;

        // Events
        public delegate void DrawingCompleted(List<List<Vector2>> strokes, Color dominantColor);
        public event DrawingCompleted OnDrawingCompleted;

        private void Awake()
        {
            mainCamera = Camera.main;

            // Find the parent Canvas (not on this GameObject)
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            // The drawing area is this GameObject's RectTransform
            if (drawingArea == null)
                drawingArea = GetComponent<RectTransform>();

            if (canvas == null)
            {
                Debug.LogError("BattleDrawingCanvas: No Canvas found in parent! Drawing may not work correctly.");
            }

            // Debug camera and canvas info
            if (mainCamera != null)
            {
                Debug.Log($"Camera: Position={mainCamera.transform.position}, Orthographic={mainCamera.orthographic}, " +
                         $"OrthographicSize={mainCamera.orthographicSize}, NearClip={mainCamera.nearClipPlane}, FarClip={mainCamera.farClipPlane}");
            }
            if (canvas != null)
            {
                Debug.Log($"Canvas: RenderMode={canvas.renderMode}, Position={canvas.transform.position}, PlaneDistance={canvas.planeDistance}");
            }
            if (drawingArea != null)
            {
                Debug.Log($"DrawingArea: Position={drawingArea.position}, LocalPosition={drawingArea.localPosition}, " +
                         $"Rect={drawingArea.rect}, Size={drawingArea.rect.size}");
            }

            // Create default line material if none provided
            if (lineMaterial == null)
            {
                // Use Unlit/Color shader which works better for LineRenderer
                Shader lineShader = Shader.Find("Unlit/Color");
                if (lineShader == null)
                {
                    // Fallback to Sprites/Default if Unlit/Color not found
                    lineShader = Shader.Find("Sprites/Default");
                }
                lineMaterial = new Material(lineShader);
                lineMaterial.color = Color.black; // BLACK so lines are visible on white background!
                Debug.Log($"BattleDrawingCanvas: Created default line material (BLACK) with shader: {lineShader.name}");
            }
        }

        private void Update()
        {
            if (!isDrawingEnabled) return;

            // Handle mouse/touch input
            if (Input.GetMouseButtonDown(0))
            {
                StartDrawing(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && isDrawing)
            {
                ContinueDrawing(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && isDrawing)
            {
                EndDrawing();
            }
        }

        /// <summary>
        /// Enable drawing on the canvas
        /// </summary>
        public void EnableDrawing()
        {
            isDrawingEnabled = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Disable drawing on the canvas
        /// </summary>
        public void DisableDrawing()
        {
            isDrawingEnabled = false;
            if (isDrawing)
                EndDrawing();
        }

        /// <summary>
        /// Clear all drawings from the canvas
        /// </summary>
        public void ClearCanvas()
        {
            // Destroy all line renderers
            foreach (var line in allLines)
            {
                if (line != null)
                    Destroy(line.gameObject);
            }

            allLines.Clear();
            allStrokes.Clear();
            currentStrokePoints.Clear();

            if (currentLine != null)
            {
                Destroy(currentLine.gameObject);
                currentLine = null;
            }

            isDrawing = false;
        }

        /// <summary>
        /// Finish drawing and trigger analysis
        /// </summary>
        public void FinishDrawing()
        {
            if (allStrokes.Count == 0)
            {
                Debug.LogWarning("No drawing to finish!");
                return;
            }

            DisableDrawing();

            // Calculate dominant color (for now, use the set drawing color)
            Color dominantColor = drawingColor;

            // Trigger the drawing completed event
            OnDrawingCompleted?.Invoke(allStrokes, dominantColor);
        }

        /// <summary>
        /// Get all stroke data
        /// </summary>
        public List<List<Vector2>> GetAllStrokes()
        {
            return new List<List<Vector2>>(allStrokes);
        }

        /// <summary>
        /// Get all line renderers for move detection
        /// </summary>
        public List<LineRenderer> GetAllLineRenderers()
        {
            return new List<LineRenderer>(allLines);
        }

        /// <summary>
        /// Set the drawing color
        /// </summary>
        public void SetDrawingColor(Color color)
        {
            drawingColor = color;
        }

        private void StartDrawing(Vector2 screenPosition)
        {
            if (!IsPointInDrawingArea(screenPosition))
                return;

            isDrawing = true;
            currentStrokePoints = new List<Vector2>(); // Still track as Vector2 for move detection

            // Create new line renderer
            GameObject lineObj;
            if (lineRendererPrefab != null)
            {
                lineObj = Instantiate(lineRendererPrefab, transform);
            }
            else
            {
                lineObj = new GameObject("BattleLine");
                lineObj.transform.SetParent(transform);
            }

            // CRITICAL: Reset local position to (0,0,0) to prevent offset
            lineObj.transform.localPosition = Vector3.zero;
            lineObj.transform.localRotation = Quaternion.identity;
            lineObj.transform.localScale = Vector3.one;

            currentLine = lineObj.GetComponent<LineRenderer>();
            if (currentLine == null)
                currentLine = lineObj.AddComponent<LineRenderer>();

            // Configure line renderer
            currentLine.startWidth = lineWidth;
            currentLine.endWidth = lineWidth;
            currentLine.positionCount = 0;

            // For ScreenSpaceOverlay, MUST use local space since overlay doesn't exist in true world space
            currentLine.useWorldSpace = false;

            // Set material and ensure it's applied correctly
            if (lineMaterial != null)
            {
                currentLine.material = lineMaterial;
                currentLine.sharedMaterial = lineMaterial;
            }

            // Set colors - CRITICAL for visibility
            currentLine.startColor = drawingColor;
            currentLine.endColor = drawingColor;

            // Ensure proper 2D rendering
            currentLine.alignment = LineAlignment.TransformZ;
            currentLine.textureMode = LineTextureMode.Tile;

            // Enable shadow casting OFF for UI rendering
            currentLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            currentLine.receiveShadows = false;

            // Set sorting order to render on top of the canvas
            currentLine.sortingLayerName = "Default";
            currentLine.sortingOrder = 1000; // High value to ensure it's on top

            Debug.Log($"Created LineRenderer - Color: {currentLine.startColor}, Width: {currentLine.startWidth}, Material: {currentLine.material?.name}, " +
                     $"GameObject: {lineObj.name}, Active: {lineObj.activeInHierarchy}, Parent: {lineObj.transform.parent?.name}, " +
                     $"LocalPos: {lineObj.transform.localPosition}, LocalScale: {lineObj.transform.localScale}");

            // Add first point
            Vector2 localPoint = ScreenToCanvasPoint(screenPosition);
            AddPointToCurrentLine(localPoint, screenPosition);
        }

        private void ContinueDrawing(Vector2 screenPosition)
        {
            if (!IsPointInDrawingArea(screenPosition))
                return;

            Vector2 localPoint = ScreenToCanvasPoint(screenPosition);

            // Only add point if it's far enough from the last point
            if (currentStrokePoints.Count > 0)
            {
                Vector2 lastPoint = currentStrokePoints[currentStrokePoints.Count - 1];
                if (Vector2.Distance(lastPoint, localPoint) < 5f)
                    return;
            }

            AddPointToCurrentLine(localPoint, screenPosition);
        }

        private void EndDrawing()
        {
            if (currentStrokePoints.Count < 2)
            {
                // Remove invalid stroke
                if (currentLine != null)
                    Destroy(currentLine.gameObject);
                currentLine = null;
                currentStrokePoints.Clear();
                isDrawing = false;
                return;
            }

            // Save the stroke
            allLines.Add(currentLine);
            allStrokes.Add(new List<Vector2>(currentStrokePoints));

            currentLine = null;
            currentStrokePoints.Clear();
            isDrawing = false;
        }

        private void AddPointToCurrentLine(Vector2 localPoint, Vector2 screenPosition)
        {
            // Store local point for move detection
            currentStrokePoints.Add(localPoint);

            if (currentLine != null)
            {
                // For ScreenSpaceOverlay, use local canvas coordinates directly (Z=0 in local space)
                Vector3 localPos = new Vector3(localPoint.x, localPoint.y, 0f);

                // Update LineRenderer with the new point
                int newIndex = currentLine.positionCount;
                currentLine.positionCount = newIndex + 1;
                currentLine.SetPosition(newIndex, localPos);

                // Debug first and every 10th point to verify visibility
                if (currentLine.positionCount == 1 || currentLine.positionCount % 10 == 0)
                {
                    Rect bounds = drawingArea != null ? drawingArea.rect : new Rect();
                    bool inBounds = bounds.Contains(localPos);
                    Debug.Log($"Line point {currentLine.positionCount}: Screen({screenPosition.x:F1}, {screenPosition.y:F1}) → Local({localPos.x:F2}, {localPos.y:F2}, {localPos.z:F2}) | " +
                             $"InBounds: {inBounds} | CanvasBounds: {bounds} | " +
                             $"Enabled: {currentLine.enabled} | Active: {currentLine.gameObject.activeInHierarchy} | " +
                             $"Width: {currentLine.startWidth} | Color: {currentLine.startColor}");
                }
            }
        }

        private bool IsPointInDrawingArea(Vector2 screenPosition)
        {
            if (drawingArea == null) return true;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingArea,
                screenPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out localPoint
            );

            return drawingArea.rect.Contains(localPoint);
        }

        private Vector2 ScreenToCanvasPoint(Vector2 screenPosition)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingArea,
                screenPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out localPoint
            );

            return localPoint;
        }

        /// <summary>
        /// Get drawing statistics
        /// </summary>
        public void GetDrawingStats(out int strokeCount, out float totalLength)
        {
            strokeCount = allStrokes.Count;
            totalLength = 0f;

            foreach (var stroke in allStrokes)
            {
                for (int i = 0; i < stroke.Count - 1; i++)
                {
                    totalLength += Vector2.Distance(stroke[i], stroke[i + 1]);
                }
            }
        }
    }
}
