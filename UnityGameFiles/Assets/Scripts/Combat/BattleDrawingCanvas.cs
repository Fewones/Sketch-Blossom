using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Battle-specific drawing canvas for combat moves
/// Simplified version without color picking - only for battle scene
/// </summary>
public class BattleDrawingCanvas : MonoBehaviour
{
    [Header("Required References")]
    public Camera mainCamera;
    public Transform strokeContainer;
    public LineRenderer lineRendererPrefab;
    public RectTransform drawingArea;

    [Header("Drawing Settings")]
    public float lineWidth = 0.05f;
    public int maxStrokes = 30;
    public float minPointDistance = 0.03f;
    public Color strokeColor = Color.green;

    // All completed strokes
    public List<LineRenderer> allStrokes = new List<LineRenderer>();

    // Current stroke being drawn
    private LineRenderer currentStroke;
    private List<Vector3> currentPoints = new List<Vector3>();
    private bool isDrawing = false;
    private bool hasLoggedBounds = false;

    void Update()
    {
        // Only handle input if this component is enabled
        if (!enabled) return;

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
                // Mouse left the drawing area - end the stroke
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
            return true; // If no area defined, allow anywhere
        }

        // Use RectTransformUtility for proper UI bounds checking (works with Screen Space - Overlay)
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingArea,
            screenPos,
            null, // null for Screen Space - Overlay canvas
            out localPoint
        );

        // Check if local point is within the rect
        Rect rect = drawingArea.rect;
        bool isInside = rect.Contains(localPoint);

        // Debug log on first check
        if (!hasLoggedBounds)
        {
            Debug.Log($"[BattleDrawingCanvas] Drawing area rect: {rect}");
            Debug.Log($"[BattleDrawingCanvas] Screen pos: {screenPos} -> Local pos: {localPoint}");
            Debug.Log($"[BattleDrawingCanvas] Is inside: {isInside}");
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
        currentStroke.startColor = strokeColor;
        currentStroke.endColor = strokeColor;

        // Ensure LineRenderer renders on top of UI
        currentStroke.sortingLayerName = "UI";
        currentStroke.sortingOrder = 1000; // Very high order to appear above all UI elements

        // Set material color
        if (currentStroke.material != null)
        {
            Material mat = new Material(currentStroke.material);
            mat.color = strokeColor;
            currentStroke.material = mat;
        }

        currentPoints.Clear();
        isDrawing = true;

        // Add first point immediately
        Vector3 worldPos = ScreenToWorld(screenPos);
        currentPoints.Add(worldPos);
        UpdateStrokeRenderer();

        Debug.Log($"[BattleDrawingCanvas] Started stroke #{allStrokes.Count + 1}");
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
            Debug.Log($"[BattleDrawingCanvas] Finished stroke #{allStrokes.Count} with {currentPoints.Count} points");
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
        // For Screen Space - Overlay canvas, convert screen pos to local UI coordinates
        // Then position in world space at the drawing area's z-depth with an offset for visibility

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingArea,
            screenPos,
            null, // null for Screen Space - Overlay
            out localPoint
        );

        // Convert local UI point to world position
        // Get the world position of the drawing area center
        Vector3 worldPos = drawingArea.TransformPoint(new Vector3(localPoint.x, localPoint.y, 0));

        // Offset forward from the canvas to ensure visibility above UI elements
        // Use a small z-offset (closer to camera) to draw on top of UI
        worldPos.z = -5f; // Negative z brings it closer to camera in overlay mode

        return worldPos;
    }

    /// <summary>
    /// Force end any in-progress stroke (called before analysis)
    /// </summary>
    public void ForceEndStroke()
    {
        if (isDrawing)
        {
            Debug.Log("[BattleDrawingCanvas] ForceEndStroke: Finishing in-progress stroke");
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
        Debug.Log("[BattleDrawingCanvas] Cleared all strokes");
    }

    /// <summary>
    /// Get stroke count
    /// </summary>
    public int GetStrokeCount()
    {
        return allStrokes.Count;
    }

    void OnValidate()
    {
        // Auto-find camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
}
