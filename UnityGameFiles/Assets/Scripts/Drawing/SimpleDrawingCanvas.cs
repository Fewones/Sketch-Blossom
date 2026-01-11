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

    // NEW: used to enforce "only one completed stroke" (for Wild Growth)
    private bool strokeFinished = false;

    private bool hasLoggedBounds = false;

    void Update()
    {
        HandleDrawingInput();
    }

    void HandleDrawingInput()
    {
        // Start drawing
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (IsInsideDrawingArea(mousePos))
            {
                StartStroke(mousePos);
            }
        }

        // Continue drawing
        if (Input.GetMouseButton(0) && isDrawing)
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
        if (Input.GetMouseButtonUp(0) && isDrawing)
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
        Vector3 worldPos = ScreenToWorld(screenPos);
        currentPoints.Add(worldPos);
        UpdateStrokeRenderer();

        Debug.Log($"Started stroke #{allStrokes.Count + 1} with color {currentColor}");
    }

    void AddPoint(Vector2 screenPos)
    {
        if (!isDrawing || currentStroke == null) return;

        Vector3 worldPos = ScreenToWorld(screenPos);

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
            strokeFinished = true; // NEW: mark stroke as done
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
    }

    void UpdateStrokeRenderer()
    {
        if (currentStroke == null) return;

        currentStroke.positionCount = currentPoints.Count;
        currentStroke.SetPositions(currentPoints.ToArray());
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        // z-distance should match camera → drawing plane distance; 10f worked for you before
        return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
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
    /// Clear all strokes
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
        strokeFinished = false; // NEW: allow drawing again
        Debug.Log("Cleared all strokes");
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

        Debug.Log($"Dominant color: {dominantColor} (used in {maxCount}/{allStrokes.Count} strokes)");
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
        Debug.Log($"Drawing color changed to: {color}");
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

    /// <summary>
    /// Clears the canvas and resets all strokes, used by WildGrowthSceneManager.
    /// </summary>
    public void ClearCanvas()
    {
        ClearAll();
    }
}
