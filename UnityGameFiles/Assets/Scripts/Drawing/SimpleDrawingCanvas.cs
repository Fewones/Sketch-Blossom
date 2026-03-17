using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// SIMPLIFIED drawing canvas - rebuilt from scratch for reliability
/// Handles all drawing input and stroke management
/// </summary>
public class SimpleDrawingCanvas : MonoBehaviour
{
    [Header("Required References")]
    public Camera mainCamera;
    public Transform strokeContainer;
    public LineRenderer lineRendererPrefab;
    public RectTransform drawingArea;

    [Header("Drawing Settings")]
    [SerializeField] private Slider brushSlider;
    [SerializeField] private RectTransform brushSliderHandle;
    public float lineWidth = 0.1f;
    public int maxStrokes = 20;
    public float minPointDistance = 0.05f;

    [Header("Current State")]
    public Color currentColor = Color.green;  // Default to green (Grass/Cactus)

    // All completed strokes
    public List<LineRenderer> allStrokes = new List<LineRenderer>();

    // Current stroke being drawn
    private LineRenderer currentStroke;
    private List<Vector3> currentPoints = new List<Vector3>();
    private bool isDrawing = false;

    public bool visible = true;

    // NEW: used to enforce "only one completed stroke" (for Wild Growth)
    private bool strokeFinished = false;

    public event System.Action<bool> OnStrokeFinished;
    public bool StrokeFinished
    {
        get {return strokeFinished;}
        set
        {
            strokeFinished = value;
            OnStrokeFinished?.Invoke(strokeFinished);
        }
    }

    private bool hasLoggedBounds = false;


    public float currentz = 0;

    // ── Fill Tool ──────────────────────────────────────────────
    private const int FillTexSize = 512;
    private Texture2D fillTexture;
    private Color[] fillPixels;
    private RawImage fillDisplay;
    private bool isFillMode = false;
    public bool IsFillMode => isFillMode;

    void Start()
    {
        InitFillTexture();
    }

    void Update()
    {
        if (visible){
           HandleDrawingInput();
        }
    }

    void HandleDrawingInput()
    {
        // Start drawing or fill
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (IsInsideDrawingArea(mousePos))
            {
                if (isFillMode)
                    PerformFill(ScreenToTextureCoord(mousePos), false);
                else
                    StartStroke(mousePos);
            }
        }

        // Continue drawing (only in draw mode)
        if (!isFillMode && Input.GetMouseButton(0) && isDrawing)
        {
            Vector2 mousePos = Input.mousePosition;
            if (IsInsideDrawingArea(mousePos))
            {
                AddPoint(mousePos);
            }
            else
            {
                // Mouse left the drawing area - end the stroke immediately
                Debug.Log("Mouse left drawing area - ending stroke");
                FinishStroke();
            }
        }

        // End drawing
        if (!isFillMode && Input.GetMouseButtonUp(0) && isDrawing)
        {
            FinishStroke();
        }
    }

    bool IsInsideDrawingArea(Vector2 screenPos)
    {
        if (drawingArea == null)
        {
            Debug.LogWarning("DrawingArea is null! Cannot restrict drawing bounds.");
            return true; // allow anywhere if not set
        }
        if (mainCamera == null)
        {
            Debug.LogWarning("MainCamera is null! Cannot check bounds.");
            return false;
        }

        Vector3[] corners = new Vector3[4];
        drawingArea.GetWorldCorners(corners);

        Vector2 min = mainCamera.WorldToScreenPoint(corners[0]);
        Vector2 max = mainCamera.WorldToScreenPoint(corners[2]);

        bool isInside = screenPos.x >= min.x && screenPos.x <= max.x &&
                        screenPos.y >= min.y && screenPos.y <= max.y;

        if (!hasLoggedBounds)
        {
            Debug.Log($"Drawing area bounds - Min: {min}, Max: {max}, Size: {max - min}");
            hasLoggedBounds = true;
        }

        return isInside;
    }

    void StartStroke(Vector2 screenPos)
    {
        if (allStrokes.Count >= maxStrokes)
        {
            Debug.LogWarning("Maximum strokes reached!");
            return;
        }

        if (strokeContainer == null || lineRendererPrefab == null || mainCamera == null)
        {
            Debug.LogError("Missing required references for drawing!");
            return;
        }

        // Create new stroke
        currentStroke = Instantiate(lineRendererPrefab, strokeContainer);
        currentStroke.startWidth = lineWidth;
        currentStroke.endWidth = lineWidth;
        currentStroke.startColor = currentColor;
        currentStroke.endColor = currentColor;

        // Set material color
        if (currentStroke.material != null)
        {
            Material mat = new Material(currentStroke.material);
            mat.color = currentColor;
            currentStroke.material = mat;
        }

        currentPoints.Clear();
        isDrawing = true;

        // Add first point immediately
        Vector3 worldPos = ScreenToWorld(screenPos, currentz);
        Debug.Log(worldPos);
        currentz += 0.01f;
        currentPoints.Add(worldPos);
        UpdateStrokeRenderer();

        Debug.Log($"Started stroke #{allStrokes.Count + 1} with color {currentColor}");
    }

    void AddPoint(Vector2 screenPos)
    {
        if (!isDrawing || currentStroke == null) return;

        Vector3 worldPos = ScreenToWorld(screenPos, currentz);

        // Check distance from last point
        if (currentPoints.Count > 0)
        {
            float distance = Vector3.Distance(currentPoints[currentPoints.Count - 1], worldPos);
            if (distance < minPointDistance) return;
        }

        currentPoints.Add(worldPos);
        UpdateStrokeRenderer();
    }

    void FinishStroke()
    {
        if (!isDrawing || currentStroke == null) return;

        // Keep stroke even if short (minimum 1 point)
        if (currentPoints.Count >= 1)
        {
            allStrokes.Add(currentStroke);
            StrokeFinished = true; // NEW: mark stroke as done
            Debug.Log($"Finished stroke #{allStrokes.Count} with {currentPoints.Count} points");
        }
        else
        {
            // No points - delete
            Destroy(currentStroke.gameObject);
            Debug.LogWarning("Stroke had no points - deleted");
        }

        isDrawing = false;
        currentStroke = null;
        currentPoints.Clear();
        Debug.Log($"[TG] FinishStroke -> allStrokes.Count = {allStrokes.Count}");

    }

    void UpdateStrokeRenderer()
    {
        if (currentStroke == null) return;

        currentStroke.positionCount = currentPoints.Count;
        currentStroke.SetPositions(currentPoints.ToArray());
    }

    Vector3 ScreenToWorld(Vector2 screenPos, float z)
    {

        return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f - z)); // subtract z so new lines appear over old lines

    }

    /// <summary>
    /// Force end any in-progress stroke (called before analysis)
    /// </summary>
    public void ForceEndStroke()
    {
        if (isDrawing)
        {
            Debug.Log("ForceEndStroke: Finishing in-progress stroke");
            FinishStroke();
        }
    }

    /// <summary>
    /// Clear all strokes and fill layer
    /// </summary>
    public void ClearAll()
    {
        foreach (var stroke in allStrokes)
        {
            if (stroke != null)
            {
                Destroy(stroke.gameObject);
            }
        }
        allStrokes.Clear();

        if (isDrawing && currentStroke != null)
        {
            Destroy(currentStroke.gameObject);
            isDrawing = false;
            currentStroke = null;
        }

        currentPoints.Clear();
        currentz = 0;
        strokeFinished = false; // NEW: allow drawing again

        drawingArea.GetComponent<Image> ().color = new Color(1,1,1,1);

        ClearFillTexture();

        Debug.Log("Cleared all strokes and fill");
    }

    // ── Fill Tool Public API ───────────────────────────────────

    public void ToggleFillMode()
    {
        isFillMode = !isFillMode;
        Debug.Log($"Fill mode: {(isFillMode ? "ON" : "OFF")}");
    }

    public Texture2D GetFillTexture()
    {
        return fillTexture;
    }

    // ── Fill Tool Internals ────────────────────────────────────

    private void InitFillTexture()
    {
        fillTexture = new Texture2D(FillTexSize, FillTexSize, TextureFormat.RGBA32, false);
        fillTexture.filterMode = FilterMode.Bilinear;
        fillPixels = new Color[FillTexSize * FillTexSize];
        ClearFillTexture();

        // Create a RawImage child of the drawing area to display fills behind strokes
        if (drawingArea != null)
        {
            GameObject fillObj = new GameObject("FillLayer");
            fillObj.transform.SetParent(drawingArea, false);
            fillDisplay = fillObj.AddComponent<RawImage>();
            RectTransform rt = fillDisplay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            // Render behind other children (strokes render via LineRenderer in world space,
            // but this keeps fill behind any UI overlays)
            fillObj.transform.SetAsFirstSibling();
            fillDisplay.texture = fillTexture;
            fillDisplay.raycastTarget = false;
        }
    }

    private void ClearFillTexture()
    {
        if (fillPixels == null) return;
        for (int i = 0; i < fillPixels.Length; i++)
            fillPixels[i] = Color.white;
        if (fillTexture != null)
        {
            fillTexture.SetPixels(fillPixels);
            fillTexture.Apply();
        }
    }

    public void PerformFill(Vector2Int texCoord, bool transparent)
    {
        if (texCoord.x < 0) return; // outside bounds

        // Build a snapshot: existing fills + rasterized strokes
        Color[] snapshot = new Color[FillTexSize * FillTexSize];
        System.Array.Copy(fillPixels, snapshot, fillPixels.Length);
        RasterizeStrokesOnto(snapshot);

        int startIdx = texCoord.y * FillTexSize + texCoord.x;
        Color targetColor = snapshot[startIdx];

        Color fillColor = currentColor;
        fillColor.a = transparent ? 0f: 1f;

        // BFS flood fill
        bool[] visited = new bool[FillTexSize * FillTexSize];
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(startIdx);
        visited[startIdx] = true;

        while (queue.Count > 0)
        {
            int idx = queue.Dequeue();
            fillPixels[idx] = fillColor;

            int x = idx % FillTexSize;
            int y = idx / FillTexSize;

            TryEnqueueFill(queue, visited, snapshot, x + 1, y, targetColor);
            TryEnqueueFill(queue, visited, snapshot, x - 1, y, targetColor);
            TryEnqueueFill(queue, visited, snapshot, x, y + 1, targetColor);
            TryEnqueueFill(queue, visited, snapshot, x, y - 1, targetColor);
        }

        fillTexture.SetPixels(fillPixels);
        fillTexture.Apply();
        Debug.Log($"Fill performed at ({texCoord.x},{texCoord.y}) with color {fillColor}");
    }

    private void TryEnqueueFill(Queue<int> queue, bool[] visited, Color[] snapshot, int x, int y, Color previousColor)
    {
        if (x < 0 || x >= FillTexSize || y < 0 || y >= FillTexSize) return;
        int idx = y * FillTexSize + x;
        if (visited[idx]) return;
        if (snapshot[idx] != previousColor) return; // boundary (stroke with a different Color)
        visited[idx] = true;
        queue.Enqueue(idx);
    }

    // ── Stroke Rasterization (for fill boundaries) ─────────────

    private void RasterizeStrokesOnto(Color[] pixels)
    {
        foreach (var stroke in allStrokes)
        {
            if (stroke == null) continue;
            int count = stroke.positionCount;
            if (count < 1) continue;

            Vector3[] positions = new Vector3[count];
            stroke.GetPositions(positions);
            Color c = stroke.startColor;
            c.a = 1f;

            float worldWidth = stroke.startWidth;
            int pixelRadius = Mathf.Max(1, WorldWidthToPixelRadius(worldWidth));

            // Draw circles at each point and lines between consecutive points
            Vector2Int prev = WorldToTextureCoord(positions[0]);
            DrawCircleOnPixels(pixels, prev.x, prev.y, c, pixelRadius);

            for (int i = 1; i < count; i++)
            {
                Vector2Int cur = WorldToTextureCoord(positions[i]);
                DrawThickLine(pixels, prev.x, prev.y, cur.x, cur.y, c, pixelRadius);
                prev = cur;
            }
        }
    }

    private Vector2Int ScreenToTextureCoord(Vector2 screenPos)
    {
        Vector3[] corners = new Vector3[4];
        drawingArea.GetWorldCorners(corners);
        Vector2 min = mainCamera.WorldToScreenPoint(corners[0]);
        Vector2 max = mainCamera.WorldToScreenPoint(corners[2]);

        float u = (screenPos.x - min.x) / (max.x - min.x);
        float v = (screenPos.y - min.y) / (max.y - min.y);

        if (u < 0 || u > 1 || v < 0 || v > 1)
            return new Vector2Int(-1, -1);

        return new Vector2Int(
            Mathf.Clamp((int)(u * FillTexSize), 0, FillTexSize - 1),
            Mathf.Clamp((int)(v * FillTexSize), 0, FillTexSize - 1)
        );
    }

    private Vector2Int WorldToTextureCoord(Vector3 worldPos)
    {
        Vector2 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        return ScreenToTextureCoord(screenPos);
    }

    private int WorldWidthToPixelRadius(float worldWidth)
    {
        // Approximate: convert world-space line width to pixel radius on the fill texture
        Vector3[] corners = new Vector3[4];
        drawingArea.GetWorldCorners(corners);
        Vector2 min = mainCamera.WorldToScreenPoint(corners[0]);
        Vector2 max = mainCamera.WorldToScreenPoint(corners[2]);
        float canvasScreenWidth = max.x - min.x;
        if (canvasScreenWidth <= 0) return 1;

        // World width in screen pixels (orthographic approximation)
        float screenPx = worldWidth * (canvasScreenWidth /
            (mainCamera.orthographicSize * 2f * ((float)Screen.width / Screen.height)));
        float texPx = screenPx / canvasScreenWidth * FillTexSize;
        return Mathf.Max(1, Mathf.RoundToInt(texPx * 0.5f));
    }

    private void DrawCircleOnPixels(Color[] pixels, int cx, int cy, Color color, int radius)
    {
        int r2 = radius * radius;
        for (int oy = -radius; oy <= radius; oy++)
        {
            for (int ox = -radius; ox <= radius; ox++)
            {
                if (ox * ox + oy * oy > r2) continue;
                int px = cx + ox, py = cy + oy;
                if (px >= 0 && px < FillTexSize && py >= 0 && py < FillTexSize)
                    pixels[py * FillTexSize + px] = color;
            }
        }
    }

    private void DrawThickLine(Color[] pixels, int x0, int y0, int x1, int y1, Color color, int radius)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawCircleOnPixels(pixels, x0, y0, color, radius);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    /// <summary>
    /// Get dominant color based on number of strokes
    /// </summary>
    public Color GetDominantColor()
    {
        if (allStrokes.Count == 0) return Color.white;

        Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

        foreach (var stroke in allStrokes)
        {
            Color strokeColor = stroke.startColor;
            Color rounded = RoundToNearestPrimaryColor(strokeColor);

            if (!colorCounts.ContainsKey(rounded))
                colorCounts[rounded] = 0;

            colorCounts[rounded]++;
        }

        Color dominantColor = Color.white;
        int maxCount = 0;

        foreach (var pair in colorCounts)
        {
            if (pair.Value > maxCount)
            {
                maxCount = pair.Value;
                dominantColor = pair.Key;
            }
        }
        return dominantColor;
    }

    Color RoundToNearestPrimaryColor(Color color)
    {
        if (color.r > color.g && color.r > color.b)
            return Color.red;
        else if (color.g > color.r && color.g > color.b)
            return Color.green;
        else if (color.b > color.r && color.b > color.g)
            return Color.blue;
        else
            return Color.white;
    }

    /// <summary>
    /// Get stroke count (finished strokes only)
    /// </summary>
    public int GetStrokeCount()
    {
        return allStrokes.Count;
    }

    /// <summary>
    /// Optional helper if you need finished strokes count explicitly.
    /// Currently identical to GetStrokeCount().
    /// </summary>
    public int GetFinishedStrokeCount()
    {
        return allStrokes.Count;
    }

    /// <summary>
    /// Set drawing color
    /// </summary>
    public void SetColor(Color color)
    {
        currentColor = color;
    }

    void OnValidate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    /// <summary>
    /// Returns summary statistics about the current drawing (finished strokes + in-progress stroke).
    /// </summary>
    public DrawingStats GetDrawingStats()
    {
        DrawingStats stats = new DrawingStats();

        int strokeCount = allStrokes.Count;
        if (isDrawing && currentPoints != null && currentPoints.Count > 0)
        {
            strokeCount += 1;
        }
        stats.strokeCount = strokeCount;

        bool hasAnyPoint = false;
        Vector2 minPoint = Vector2.zero;
        Vector2 maxPoint = Vector2.zero;
        float totalLength = 0f;

        void UpdateBounds(Vector3 worldPos)
        {
            Vector2 p = new Vector2(worldPos.x, worldPos.y);
            if (!hasAnyPoint)
            {
                hasAnyPoint = true;
                minPoint = p;
                maxPoint = p;
            }
            else
            {
                minPoint = Vector2.Min(minPoint, p);
                maxPoint = Vector2.Max(maxPoint, p);
            }
        }

        foreach (var stroke in allStrokes)
        {
            if (stroke == null) continue;
            int count = stroke.positionCount;
            if (count == 0) continue;

            Vector3[] positions = new Vector3[count];
            stroke.GetPositions(positions);

            for (int i = 0; i < count; i++)
            {
                UpdateBounds(positions[i]);
                if (i > 0)
                {
                    totalLength += Vector3.Distance(positions[i - 1], positions[i]);
                }
            }
        }

        if (isDrawing && currentPoints != null && currentPoints.Count > 0)
        {
            for (int i = 0; i < currentPoints.Count; i++)
            {
                UpdateBounds(currentPoints[i]);
                if (i > 0)
                {
                    totalLength += Vector3.Distance(currentPoints[i - 1], currentPoints[i]);
                }
            }
        }

        stats.totalLength = totalLength;

        if (hasAnyPoint)
        {
            Vector2 size = maxPoint - minPoint;
            stats.boundingBoxArea = Mathf.Abs(size.x * size.y);
        }
        else
        {
            stats.boundingBoxArea = 0f;
        }

        float canvasArea = 0f;
        if (drawingArea != null)
        {
            var rect = drawingArea.rect;
            canvasArea = Mathf.Abs(rect.width * rect.height);
        }
        stats.canvasArea = canvasArea;

        return stats;
    }

    // Change the brush width
    public void ChangeBrushWidth()
    {
        lineWidth = brushSlider.value;
        brushSliderHandle.sizeDelta= new Vector2(1 + brushSlider.value*50, 1 + brushSlider.value*50);
    }

    /// <summary>
    /// Clears the canvas and resets all strokes, used by WildGrowthSceneManager.
    /// </summary>
    public void ClearCanvas()
    {
        ClearAll();
    }
}
