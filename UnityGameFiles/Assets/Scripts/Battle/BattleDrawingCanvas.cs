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
        [SerializeField] private float lineWidth = 0.1f;
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

            // Create default line material if none provided
            if (lineMaterial == null)
            {
                lineMaterial = new Material(Shader.Find("Sprites/Default"));
                lineMaterial.color = Color.white;
                Debug.Log("BattleDrawingCanvas: Created default line material");
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
            currentStrokePoints = new List<Vector2>();

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

            currentLine = lineObj.GetComponent<LineRenderer>();
            if (currentLine == null)
                currentLine = lineObj.AddComponent<LineRenderer>();

            // Configure line renderer
            currentLine.startWidth = lineWidth;
            currentLine.endWidth = lineWidth;
            currentLine.positionCount = 0;
            currentLine.useWorldSpace = false;

            // Set material
            if (lineMaterial != null)
                currentLine.material = lineMaterial;

            // Set colors
            currentLine.startColor = drawingColor;
            currentLine.endColor = drawingColor;

            // Ensure proper 2D rendering
            currentLine.alignment = LineAlignment.TransformZ;
            currentLine.textureMode = LineTextureMode.Tile;

            // Set sorting order to render on top
            currentLine.sortingLayerName = "Default";
            currentLine.sortingOrder = 100;

            // Add first point
            Vector2 localPoint = ScreenToCanvasPoint(screenPosition);
            AddPointToCurrentLine(localPoint);
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

            AddPointToCurrentLine(localPoint);
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

        private void AddPointToCurrentLine(Vector2 point)
        {
            currentStrokePoints.Add(point);

            if (currentLine != null)
            {
                currentLine.positionCount = currentStrokePoints.Count;

                // Convert to Vector3 for LineRenderer
                Vector3[] positions = new Vector3[currentStrokePoints.Count];
                for (int i = 0; i < currentStrokePoints.Count; i++)
                {
                    positions[i] = new Vector3(currentStrokePoints[i].x, currentStrokePoints[i].y, 0);
                }

                currentLine.SetPositions(positions);
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
