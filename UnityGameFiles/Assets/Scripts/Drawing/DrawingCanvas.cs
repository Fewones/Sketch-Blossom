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

    public int currentStrokeCount = 0;
    public List<LineRenderer> allStrokes = new List<LineRenderer>();

    private LineRenderer currentLine;
    private List<Vector3> currentStrokePoints = new List<Vector3>();
    private bool isDrawing = false;
    private Camera mainCamera;

    void Start()
    {
        Debug.Log("=== DRAWINGCANVAS START ===");

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

        // Only setup finish button if it exists (for DrawingScene)
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

            bool inside = IsInsideDrawingArea(mousePos);

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
            EndStroke();
        }
    }

    bool IsInsideDrawingArea(Vector2 screenPos)
    {
        if (drawingArea == null || mainCamera == null)
            return false;

        // Simple bounds check
        Vector3[] corners = new Vector3[4];
        drawingArea.GetWorldCorners(corners);

        // Convert world corners to screen space
        Vector2 min = RectTransformUtility.WorldToScreenPoint(mainCamera, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(mainCamera, corners[2]);

        bool inside = screenPos.x >= min.x && screenPos.x <= max.x &&
                      screenPos.y >= min.y && screenPos.y <= max.y;

        return inside;
    }

    void StartNewStroke(Vector2 screenPos)
    {
        if (currentStrokeCount >= maxStrokes)
        {
            Debug.Log("Max strokes reached!");
            return;
        }

        if (strokeContainer == null)
        {
            Debug.LogError("StrokeContainer is null! Cannot create line.");
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
        if (currentLine == null || mainCamera == null)
            return;

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

        // Enable finish button after at least 1 stroke (only if button exists)
        if (currentStrokeCount >= 1 && finishButton != null)
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
        Debug.Log($"DrawingCanvas.OnFinishDrawing called! Total strokes: {currentStrokeCount}");

        // Check which scene we're in - only do analysis in DrawingScene
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"Current Scene: {currentScene}");

        if (currentScene == "DrawingScene")
        {
            // In DrawingScene - analyze and transition
            AnalyzeDrawing();
            // DrawingManager will handle scene transition
        }
        else
        {
            // In battle or other scene - do nothing
            Debug.Log("Not in DrawingScene - ignoring OnFinishDrawing");
            // CombatManager handles attack submission
        }
    }

    void AnalyzeDrawing()
    {
        Debug.Log("=== DRAWING ANALYSIS ===");
        Debug.Log($"Total Strokes: {allStrokes.Count}");

        float totalLength = 0f;
        int totalPoints = 0;

        foreach (var stroke in allStrokes)
        {
            if (stroke == null) continue;

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

    /// <summary>
    /// Clear all drawing data - used when starting new turn in battle
    /// </summary>
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

        // Disable finish button (only if it exists)
        if (finishButton != null)
        {
            finishButton.interactable = false;
        }
    }
}