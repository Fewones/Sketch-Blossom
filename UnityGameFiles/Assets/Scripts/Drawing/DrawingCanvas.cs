using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DrawingCanvas : MonoBehaviour
{
    [Header("Drawing Settings")]
    public int maxStrokes = 15;
    public float minPointDistance = 0.05f;
    public LineRenderer lineRendererPrefab;
    public Transform strokeContainer;

    [Header("UI References")]
    public TextMeshProUGUI strokeCountText;
    public Button finishButton;

    [Header("Drawing Area")]
    public RectTransform drawingArea;

    // CHANGED: Made these public
    public int currentStrokeCount = 0;
    public List<LineRenderer> allStrokes = new List<LineRenderer>();

    private LineRenderer currentLine;
    private List<Vector3> currentStrokePoints = new List<Vector3>();
    private bool isDrawing = false;
    private Camera mainCamera;

    void Start()
    {
        Debug.LogError("=== DRAWINGCANVAS START ===");

        mainCamera = Camera.main;

        if (mainCamera == null)
            Debug.LogError("ERROR: Main Camera is NULL!");
        else
            Debug.Log("Main Camera found: " + mainCamera.name);

        if (drawingArea == null)
            Debug.LogError("ERROR: Drawing Area is NULL!");
        else
            Debug.Log("Drawing Area found");

        if (lineRendererPrefab == null)
            Debug.LogError("ERROR: LineRenderer Prefab is NULL!");
        else
            Debug.Log("LineRenderer Prefab OK");

        UpdateStrokeUI();

        if (finishButton != null)
        {
            finishButton.onClick.AddListener(OnFinishDrawing);
            finishButton.interactable = false;
        }

        Debug.Log("=== DRAWINGCANVAS READY ===");
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Mouse/Touch Down
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Debug.Log("MOUSE DOWN at: " + mousePos);

            bool inside = IsInsideDrawingArea(mousePos);
            Debug.Log("Inside drawing area: " + inside);

            if (inside)
            {
                StartNewStroke(mousePos);
            }
        }

        // Mouse/Touch Held
        if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector2 mousePos = Input.mousePosition;
            if (IsInsideDrawingArea(mousePos))
            {
                AddPointToStroke(mousePos);
            }
        }

        // Mouse/Touch Up
        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            Debug.Log("MOUSE UP - Ending stroke");
            EndStroke();
        }
    }

    bool IsInsideDrawingArea(Vector2 screenPos)
    {
        // Simple bounds check
        Vector3[] corners = new Vector3[4];
        drawingArea.GetWorldCorners(corners);

        // Convert world corners to screen space
        Vector2 min = RectTransformUtility.WorldToScreenPoint(mainCamera, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(mainCamera, corners[2]);

        bool inside = screenPos.x >= min.x && screenPos.x <= max.x &&
                      screenPos.y >= min.y && screenPos.y <= max.y;

        Debug.Log($"Screen pos: {screenPos}, Min: {min}, Max: {max}, Inside: {inside}");

        return inside;
    }

    void StartNewStroke(Vector2 screenPos)
    {
        Debug.Log("START NEW STROKE!");

        if (currentStrokeCount >= maxStrokes)
        {
            Debug.Log("Max strokes reached!");
            return;
        }

        isDrawing = true;
        currentStrokePoints.Clear();

        // Create new LineRenderer for this stroke
        currentLine = Instantiate(lineRendererPrefab, strokeContainer);
        currentLine.positionCount = 0;
    }

    void AddPointToStroke(Vector2 screenPos)
    {
        // Convert screen position to world position
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, 10f));

        // Check if point is far enough from last point
        if (currentStrokePoints.Count > 0)
        {
            Vector3 lastPoint = currentStrokePoints[currentStrokePoints.Count - 1];
            if (Vector3.Distance(lastPoint, worldPos) < minPointDistance)
                return;
        }

        currentStrokePoints.Add(worldPos);

        // Update LineRenderer
        currentLine.positionCount = currentStrokePoints.Count;
        currentLine.SetPosition(currentStrokePoints.Count - 1, worldPos);
    }

    void EndStroke()
    {
        if (currentStrokePoints.Count < 2)
        {
            // Stroke too short, delete it
            if (currentLine != null)
                Destroy(currentLine.gameObject);
            isDrawing = false;
            return;
        }

        isDrawing = false;
        currentStrokeCount++;
        allStrokes.Add(currentLine);
        UpdateStrokeUI();

        // Enable finish button after at least 1 stroke
        if (currentStrokeCount >= 1)
        {
            finishButton.interactable = true;
        }
    }

    void UpdateStrokeUI()
    {
        if (strokeCountText != null)
        {
            strokeCountText.text = $"Strokes: {currentStrokeCount}/{maxStrokes}";

            // Change color when reaching max
            if (currentStrokeCount >= maxStrokes)
            {
                strokeCountText.color = Color.red;
            }
            else
            {
                strokeCountText.color = Color.white;
            }
        }
    }

    void OnFinishDrawing()
    {
        Debug.Log($"Drawing finished! Total strokes: {currentStrokeCount}");

        // Here you'll call the analyzer later
        // For now, just show what we have
        AnalyzeDrawing();
    }

    void AnalyzeDrawing()
    {
        Debug.Log("=== DRAWING ANALYSIS ===");
        Debug.Log($"Total Strokes: {allStrokes.Count}");

        float totalLength = 0f;
        int totalPoints = 0;

        foreach (var stroke in allStrokes)
        {
            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            for (int i = 1; i < positions.Length; i++)
            {
                totalLength += Vector3.Distance(positions[i-1], positions[i]);
            }
            totalPoints += positions.Length;
        }

        Debug.Log($"Total Length: {totalLength:F2}");
        Debug.Log($"Total Points: {totalPoints}");
        Debug.Log($"Avg Points per Stroke: {(float)totalPoints / allStrokes.Count:F1}");

        // Simple stats calculation
        int attack = Mathf.Clamp(allStrokes.Count * 5, 5, 30);
        int defense = Mathf.Clamp((int)(totalLength * 10), 5, 25);
        int hp = Mathf.Clamp(totalPoints / 2, 10, 50);

        Debug.Log($"Generated Stats - ATK: {attack}, DEF: {defense}, HP: {hp}");
    }

    // NEW METHOD: Clear canvas for reuse in battle
    public void ClearCanvas()
    {
        Debug.Log("Clearing canvas...");

        // Destroy all existing stroke GameObjects
        foreach (var stroke in allStrokes)
        {
            if (stroke != null)
            {
                Destroy(stroke.gameObject);
            }
        }

        // Clear the current line if still drawing
        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }

        // Reset all tracking variables
        allStrokes.Clear();
        currentStrokePoints.Clear();
        currentStrokeCount = 0;
        isDrawing = false;

        // Update UI
        UpdateStrokeUI();

        // Disable finish button
        if (finishButton != null)
        {
            finishButton.interactable = false;
        }
    }
}