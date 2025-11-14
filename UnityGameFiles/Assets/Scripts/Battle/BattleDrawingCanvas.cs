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
        [SerializeField] private float lineWidth = 5f;
        [SerializeField] private Color drawingColor = Color.black;

        [Header("UI Drawing")]
        [SerializeField] private RawImage drawingImage; // UI Image to display the drawing texture

        [Header("Drawing State")]
        [SerializeField] private bool isDrawingEnabled = false;

        // Texture-based drawing (proper for UI)
        private Texture2D drawingTexture;
        private Color[] clearPixels;
        private int textureWidth = 800;
        private int textureHeight = 600;

        // Current stroke data
        private List<Vector2> currentStrokePoints = new List<Vector2>();

        // All strokes in current drawing (for move detection)
        private List<List<Vector2>> allStrokes = new List<List<Vector2>>();
        private List<LineRenderer> allLines = new List<LineRenderer>(); // Keep for compatibility

        // Drawing tracking
        private bool isDrawing = false;
        private Camera mainCamera;
        private Vector2 lastDrawPoint;

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

            // Find or create RawImage for drawing
            if (drawingImage == null)
            {
                drawingImage = GetComponent<RawImage>();
                if (drawingImage == null)
                {
                    drawingImage = gameObject.AddComponent<RawImage>();
                }
            }

            // Initialize texture based on drawing area size
            if (drawingArea != null)
            {
                textureWidth = Mathf.Max(512, (int)drawingArea.rect.width);
                textureHeight = Mathf.Max(512, (int)drawingArea.rect.height);
            }

            // Create drawing texture
            drawingTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            drawingTexture.filterMode = FilterMode.Bilinear;

            // Initialize clear pixels (transparent)
            clearPixels = new Color[textureWidth * textureHeight];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = Color.clear;
            }

            // Clear the texture initially
            drawingTexture.SetPixels(clearPixels);
            drawingTexture.Apply();

            // Assign texture to RawImage
            drawingImage.texture = drawingTexture;
            drawingImage.color = Color.white; // Ensure image is visible

            Debug.Log($"BattleDrawingCanvas: Initialized texture-based drawing ({textureWidth}x{textureHeight}) on UI canvas. RenderMode: {canvas?.renderMode}");
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
            // Clear the texture
            if (drawingTexture != null)
            {
                drawingTexture.SetPixels(clearPixels);
                drawingTexture.Apply();
            }

            allStrokes.Clear();
            currentStrokePoints.Clear();
            isDrawing = false;

            Debug.Log("BattleDrawingCanvas: Canvas cleared");
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
        /// Creates temporary LineRenderers from stroke data for compatibility
        /// </summary>
        public List<LineRenderer> GetAllLineRenderers()
        {
            // Create temporary LineRenderers from our stroke data for move detection
            List<LineRenderer> lineRenderers = new List<LineRenderer>();

            foreach (var stroke in allStrokes)
            {
                if (stroke.Count < 2) continue;

                GameObject tempObj = new GameObject("TempLine");
                tempObj.transform.SetParent(transform);
                LineRenderer lr = tempObj.AddComponent<LineRenderer>();

                lr.positionCount = stroke.Count;
                for (int i = 0; i < stroke.Count; i++)
                {
                    lr.SetPosition(i, new Vector3(stroke[i].x, stroke[i].y, 0));
                }

                lineRenderers.Add(lr);
            }

            return lineRenderers;
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

            // Convert to texture coordinates
            Vector2 texturePoint = ScreenToTexturePoint(screenPosition);
            lastDrawPoint = texturePoint;
            currentStrokePoints.Add(texturePoint);

            // Draw initial point
            DrawPoint(texturePoint);

            Debug.Log($"Started drawing at screen: {screenPosition}, texture: {texturePoint}");
        }

        private void ContinueDrawing(Vector2 screenPosition)
        {
            if (!IsPointInDrawingArea(screenPosition))
                return;

            Vector2 texturePoint = ScreenToTexturePoint(screenPosition);

            // Only add point if it's far enough from the last point (prevents too many points)
            if (Vector2.Distance(lastDrawPoint, texturePoint) < 2f)
                return;

            // Draw line from last point to current point
            DrawLine(lastDrawPoint, texturePoint);

            currentStrokePoints.Add(texturePoint);
            lastDrawPoint = texturePoint;
        }

        private void EndDrawing()
        {
            if (currentStrokePoints.Count < 2)
            {
                // Invalid stroke, just clear
                currentStrokePoints.Clear();
                isDrawing = false;
                return;
            }

            // Save the stroke for move detection
            allStrokes.Add(new List<Vector2>(currentStrokePoints));

            Debug.Log($"Finished stroke with {currentStrokePoints.Count} points. Total strokes: {allStrokes.Count}");

            currentStrokePoints.Clear();
            isDrawing = false;
        }

        /// <summary>
        /// Convert screen position to texture pixel coordinates
        /// </summary>
        private Vector2 ScreenToTexturePoint(Vector2 screenPosition)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingArea,
                screenPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out localPoint
            );

            // Convert local point to texture coordinates
            // Local point is relative to the center of the rect, so offset it
            Rect rect = drawingArea.rect;
            float x = (localPoint.x - rect.xMin) / rect.width * textureWidth;
            float y = (localPoint.y - rect.yMin) / rect.height * textureHeight;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Draw a single point on the texture
        /// </summary>
        private void DrawPoint(Vector2 point)
        {
            int x = Mathf.RoundToInt(point.x);
            int y = Mathf.RoundToInt(point.y);

            int radius = Mathf.RoundToInt(lineWidth);

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    if (i * i + j * j <= radius * radius) // Circle check
                    {
                        int px = x + i;
                        int py = y + j;

                        if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
                        {
                            drawingTexture.SetPixel(px, py, drawingColor);
                        }
                    }
                }
            }

            drawingTexture.Apply();
        }

        /// <summary>
        /// Draw a line between two points using Bresenham's algorithm
        /// </summary>
        private void DrawLine(Vector2 start, Vector2 end)
        {
            int x0 = Mathf.RoundToInt(start.x);
            int y0 = Mathf.RoundToInt(start.y);
            int x1 = Mathf.RoundToInt(end.x);
            int y1 = Mathf.RoundToInt(end.y);

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                DrawPoint(new Vector2(x0, y0));

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
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
