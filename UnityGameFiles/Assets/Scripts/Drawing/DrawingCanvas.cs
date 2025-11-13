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

    [Header("Drawing Control")]
    [Tooltip("Set to true to enable drawing. False by default to prevent drawing before Start button")]
    public bool isDrawingEnabled = false;

    [Header("Color Selection")]
    public Color redColor = Color.red;
    public Color greenColor = Color.green;
    public Color blueColor = Color.blue;
    public Color currentDrawingColor = Color.green; // Default to green

    public int currentStrokeCount = 0;
    public List<LineRenderer> allStrokes = new List<LineRenderer>();

    // Color tracking for plant analysis
    public Dictionary<Color, int> colorUsageCount = new Dictionary<Color, int>();
    public Dictionary<Color, float> colorUsageLength = new Dictionary<Color, float>();

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

        // Auto-find DrawingArea if not assigned
        if (drawingArea == null)
        {
            Debug.Log("DrawingArea not assigned, attempting to find it...");

            // Find Canvas first
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                // Search through Canvas children (including inactive ones)
                Transform drawingPanelTransform = canvas.transform.Find("DrawingPanel");
                if (drawingPanelTransform != null)
                {
                    Transform areaTransform = drawingPanelTransform.Find("DrawingArea");
                    if (areaTransform != null)
                    {
                        drawingArea = areaTransform.GetComponent<RectTransform>();
                        Debug.Log("✓ DrawingArea auto-found!");
                    }
                    else
                    {
                        Debug.LogError("ERROR: DrawingArea not found inside DrawingPanel!");
                    }
                }
                else
                {
                    Debug.LogError("ERROR: DrawingPanel not found in Canvas!");
                }
            }
            else
            {
                Debug.LogError("ERROR: Canvas not found in scene!");
            }
        }
        else
        {
            Debug.Log("DrawingArea found");
        }

        // Auto-find StrokeContainer if not assigned
        if (strokeContainer == null)
        {
            Transform container = transform.Find("StrokeContainer");
            if (container != null)
            {
                strokeContainer = container;
                Debug.Log("✓ StrokeContainer auto-found!");
            }
        }

        // Auto-find StrokeCountText if not assigned
        if (strokeCountText == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform drawingPanelTransform = canvas.transform.Find("DrawingPanel");
                if (drawingPanelTransform != null)
                {
                    Transform counterTransform = drawingPanelTransform.Find("StrokeCounter");
                    if (counterTransform != null)
                    {
                        strokeCountText = counterTransform.GetComponent<TextMeshProUGUI>();
                        Debug.Log("✓ StrokeCountText auto-found!");
                    }
                }
            }
        }

        // Auto-find FinishButton if not assigned
        if (finishButton == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform drawingPanelTransform = canvas.transform.Find("DrawingPanel");
                if (drawingPanelTransform != null)
                {
                    Transform buttonTransform = drawingPanelTransform.Find("FinishButton");
                    if (buttonTransform != null)
                    {
                        finishButton = buttonTransform.GetComponent<Button>();
                        Debug.Log("✓ FinishButton auto-found!");
                    }
                }
            }
        }

        if (lineRendererPrefab == null)
            Debug.LogError("ERROR: LineRenderer Prefab is NULL!");
        else
            Debug.Log("LineRenderer Prefab OK");

        UpdateStrokeUI();

        // Only setup finish button if it exists (for DrawingScene)
        // BUT: Don't hook it up if DrawingManager exists - DrawingManager will handle it
        if (finishButton != null)
        {
            DrawingManager manager = FindFirstObjectByType<DrawingManager>();
            if (manager == null)
            {
                // No DrawingManager - use old behavior
                finishButton.onClick.AddListener(OnFinishDrawing);
                Debug.Log("DrawingCanvas: Hooked up finish button (no DrawingManager found)");
            }
            else
            {
                // DrawingManager exists - let it handle the button
                Debug.Log("DrawingCanvas: DrawingManager found, letting it handle finish button");
            }
            finishButton.interactable = false;
        }

        // Initialize color tracking dictionaries
        InitializeColorTracking();

        Debug.Log("=== DRAWINGCANVAS READY ===");
    }

    void InitializeColorTracking()
    {
        colorUsageCount.Clear();
        colorUsageLength.Clear();

        colorUsageCount[redColor] = 0;
        colorUsageCount[greenColor] = 0;
        colorUsageCount[blueColor] = 0;

        colorUsageLength[redColor] = 0f;
        colorUsageLength[greenColor] = 0f;
        colorUsageLength[blueColor] = 0f;

        Debug.Log("Color tracking initialized");
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Don't allow drawing if not enabled
        if (!isDrawingEnabled)
        {
            return;
        }

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

        // Apply current color to the line - Try multiple methods for compatibility
        currentLine.startColor = currentDrawingColor;
        currentLine.endColor = currentDrawingColor;

        // ADD INITIAL POINT IMMEDIATELY - This ensures even quick clicks create valid strokes
        if (mainCamera != null)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            currentStrokePoints.Add(worldPos);
            currentLine.positionCount = 1;
            currentLine.SetPosition(0, worldPos);
        }

        // Create material instance and try multiple ways to set color
        if (currentLine.material != null)
        {
            currentLine.material = new Material(currentLine.material); // Create instance to avoid modifying prefab

            // Try setting color via multiple property names (different shaders use different names)
            if (currentLine.material.HasProperty("_Color"))
                currentLine.material.SetColor("_Color", currentDrawingColor);
            if (currentLine.material.HasProperty("_BaseColor"))
                currentLine.material.SetColor("_BaseColor", currentDrawingColor);
            if (currentLine.material.HasProperty("_MainColor"))
                currentLine.material.SetColor("_MainColor", currentDrawingColor);

            // Also try setting via .color property
            currentLine.material.color = currentDrawingColor;

            // Enable color material property block
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            propBlock.SetColor("_Color", currentDrawingColor);
            propBlock.SetColor("_BaseColor", currentDrawingColor);
            currentLine.SetPropertyBlock(propBlock);
        }

        Debug.Log($"Created new stroke with color: {currentDrawingColor} (R:{currentDrawingColor.r}, G:{currentDrawingColor.g}, B:{currentDrawingColor.b})");
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

        // Track color usage
        TrackColorUsage(currentLine, currentDrawingColor);

        UpdateStrokeUI();

        // Enable finish button after at least 1 stroke (only if button exists)
        if (currentStrokeCount >= 1 && finishButton != null)
        {
            finishButton.interactable = true;
        }
    }

    /// <summary>
    /// Force end any in-progress stroke
    /// Called by DrawingManager before analyzing
    /// </summary>
    public void ForceEndStroke()
    {
        if (isDrawing && currentLine != null)
        {
            Debug.Log("ForceEndStroke: Ending in-progress stroke");
            EndStroke();
        }
        else
        {
            Debug.Log("ForceEndStroke: No stroke in progress");
        }
    }

    void TrackColorUsage(LineRenderer stroke, Color usedColor)
    {
        // Find the matching base color (red, green, or blue)
        Color baseColor = GetClosestBaseColor(usedColor);

        // Increment stroke count for this color
        if (colorUsageCount.ContainsKey(baseColor))
        {
            colorUsageCount[baseColor]++;
        }

        // Calculate and add stroke length
        float strokeLength = 0f;
        Vector3[] positions = new Vector3[stroke.positionCount];
        stroke.GetPositions(positions);

        for (int i = 1; i < positions.Length; i++)
        {
            strokeLength += Vector3.Distance(positions[i - 1], positions[i]);
        }

        if (colorUsageLength.ContainsKey(baseColor))
        {
            colorUsageLength[baseColor] += strokeLength;
        }

        Debug.Log($"Color Usage: {baseColor} - Count: {colorUsageCount[baseColor]}, Length: {colorUsageLength[baseColor]:F2}");
    }

    Color GetClosestBaseColor(Color color)
    {
        // Determine which base color (red, green, blue) is closest
        float redDist = ColorDistance(color, redColor);
        float greenDist = ColorDistance(color, greenColor);
        float blueDist = ColorDistance(color, blueColor);

        if (redDist <= greenDist && redDist <= blueDist)
            return redColor;
        else if (greenDist <= blueDist)
            return greenColor;
        else
            return blueColor;
    }

    float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
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
    /// Set drawing color to red (for UI button)
    /// </summary>
    public void SelectRedColor()
    {
        currentDrawingColor = redColor;
        Debug.Log("Selected RED color for drawing (Sunflower)");
    }

    /// <summary>
    /// Set drawing color to green (for UI button)
    /// </summary>
    public void SelectGreenColor()
    {
        currentDrawingColor = greenColor;
        Debug.Log("Selected GREEN color for drawing (Cactus)");
    }

    /// <summary>
    /// Set drawing color to blue (for UI button)
    /// </summary>
    public void SelectBlueColor()
    {
        currentDrawingColor = blueColor;
        Debug.Log("Selected BLUE color for drawing (Water Lily)");
    }

    /// <summary>
    /// Get the dominant color used in the drawing (by stroke count)
    /// </summary>
    public Color GetDominantColorByCount()
    {
        Color dominant = greenColor;
        int maxCount = 0;

        foreach (var kvp in colorUsageCount)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                dominant = kvp.Key;
            }
        }

        Debug.Log($"Dominant color by count: {dominant} ({maxCount} strokes)");
        return dominant;
    }

    /// <summary>
    /// Get the dominant color used in the drawing (by total length)
    /// </summary>
    public Color GetDominantColorByLength()
    {
        Color dominant = greenColor;
        float maxLength = 0f;

        foreach (var kvp in colorUsageLength)
        {
            if (kvp.Value > maxLength)
            {
                maxLength = kvp.Value;
                dominant = kvp.Key;
            }
        }

        Debug.Log($"Dominant color by length: {dominant} ({maxLength:F2} units)");
        return dominant;
    }

    /// <summary>
    /// Get color usage percentages (by stroke count)
    /// </summary>
    public Dictionary<Color, float> GetColorPercentages()
    {
        Dictionary<Color, float> percentages = new Dictionary<Color, float>();
        int totalStrokes = currentStrokeCount;

        if (totalStrokes == 0)
        {
            percentages[redColor] = 0f;
            percentages[greenColor] = 0f;
            percentages[blueColor] = 0f;
            return percentages;
        }

        foreach (var kvp in colorUsageCount)
        {
            percentages[kvp.Key] = (float)kvp.Value / totalStrokes;
        }

        return percentages;
    }

    /// <summary>
    /// Get all strokes for move detection
    /// </summary>
    public List<LineRenderer> GetAllStrokes()
    {
        return allStrokes;
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

        // Reset color tracking
        InitializeColorTracking();

        // Update UI
        UpdateStrokeUI();

        // Disable finish button (only if it exists)
        if (finishButton != null)
        {
            finishButton.interactable = false;
        }
    }
}