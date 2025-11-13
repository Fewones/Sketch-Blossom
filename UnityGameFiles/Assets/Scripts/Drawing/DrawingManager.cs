using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the drawing phase and transitions to battle
/// Works alongside your existing DrawingCanvas script
/// </summary>
public class DrawingManager : MonoBehaviour
{
    [Header("References")]
    public DrawingCanvas drawingCanvas;
    public PlantAnalyzer plantAnalyzer;
    public PlantDetectionFeedback detectionFeedback; // Optional UI feedback
    public PlantAnalysisResultPanel analysisResultPanel; // Main result display
    public SimpleResultDisplay simpleResultDisplay; // Simple text display

    [Header("Scene Management")]
    public string battleSceneName = "BattleScene";
    public bool showDetectionFeedback = true; // Show plant type before battle
    public bool showAnalysisPanel = true; // Show detailed analysis panel
    public float resultDisplayDelay = 3f; // Delay before transitioning to battle

    private DrawnUnitData unitData;
    private PlantAnalyzer.PlantAnalysisResult lastPlantResult;
    private Color lastDominantColor;

    private void Start()
    {
        // Create or get the unit data object
        if (DrawnUnitData.Instance == null)
        {
            GameObject dataObj = new GameObject("DrawnUnitData");
            unitData = dataObj.AddComponent<DrawnUnitData>();
        }
        else
        {
            unitData = DrawnUnitData.Instance;
            unitData.ClearData(); // Clear previous data
        }

        if (drawingCanvas == null)
        {
            drawingCanvas = FindObjectOfType<DrawingCanvas>();
        }

        if (plantAnalyzer == null)
        {
            plantAnalyzer = FindObjectOfType<PlantAnalyzer>();
            if (plantAnalyzer == null)
            {
                // Create PlantAnalyzer if it doesn't exist
                GameObject analyzerObj = new GameObject("PlantAnalyzer");
                plantAnalyzer = analyzerObj.AddComponent<PlantAnalyzer>();
            }
        }

        // Auto-find analysis result panel
        if (analysisResultPanel == null)
        {
            analysisResultPanel = FindObjectOfType<PlantAnalysisResultPanel>();
        }

        // Auto-find simple result display
        if (simpleResultDisplay == null)
        {
            simpleResultDisplay = FindObjectOfType<SimpleResultDisplay>();
        }

        if (analysisResultPanel == null && simpleResultDisplay == null && showAnalysisPanel)
        {
            Debug.LogWarning("No result display found in scene. Analysis results will only show in console.");
        }

        // Hook into the existing finish button
        if (drawingCanvas != null && drawingCanvas.finishButton != null)
        {
            // Remove existing listener and add our own
            drawingCanvas.finishButton.onClick.RemoveAllListeners();
            drawingCanvas.finishButton.onClick.AddListener(OnFinishDrawing);
        }
    }

    /// <summary>
    /// Called when the player finishes drawing their unit
    /// This replaces the OnFinishDrawing in DrawingCanvas
    /// </summary>
    private void OnFinishDrawing()
    {
        Debug.Log("DrawingManager: Finish button pressed!");

        // Analyze the drawing using the existing DrawingCanvas data
        AnalyzeAndStoreDrawing();

        // Show analysis results - try multiple display methods
        bool resultShown = false;

        // Try full panel first
        if (showAnalysisPanel && analysisResultPanel != null && lastPlantResult != null)
        {
            Debug.Log("Showing analysis result panel...");
            analysisResultPanel.ShowResult(lastPlantResult, lastDominantColor, LoadBattleScene);
            resultShown = true;
        }
        // Try simple text display
        else if (simpleResultDisplay != null && lastPlantResult != null)
        {
            Debug.Log("Showing simple result display...");
            simpleResultDisplay.ShowResult(lastPlantResult, lastDominantColor);
            StartCoroutine(DelayedBattleTransition());
            resultShown = true;
        }

        // If nothing worked, just show in console and transition
        if (!resultShown)
        {
            Debug.LogWarning("No UI display available - results shown in console only");
            StartCoroutine(DelayedBattleTransition());
        }
    }

    /// <summary>
    /// Delayed battle transition (gives time to read console logs)
    /// </summary>
    private System.Collections.IEnumerator DelayedBattleTransition()
    {
        yield return new WaitForSeconds(resultDisplayDelay);
        LoadBattleScene();
    }

    /// <summary>
    /// Analyze the drawing and store stats
    /// </summary>
    private void AnalyzeAndStoreDrawing()
    {
        if (drawingCanvas == null)
        {
            Debug.LogError("DrawingCanvas reference missing!");
            return;
        }

        // Get drawing data
        int strokeCount = drawingCanvas.currentStrokeCount;

        // Calculate total length and points from all strokes
        float totalLength = 0f;
        int totalPoints = 0;

        foreach (var stroke in drawingCanvas.allStrokes)
        {
            if (stroke == null) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            // Calculate length
            for (int i = 1; i < positions.Length; i++)
            {
                totalLength += Vector3.Distance(positions[i - 1], positions[i]);
            }

            totalPoints += positions.Length;
        }

        Debug.Log($"Drawing Analysis: {strokeCount} strokes, {totalLength:F2} length, {totalPoints} points");

        // ===== NEW: ANALYZE PLANT TYPE WITH COLOR =====
        PlantAnalyzer.PlantAnalysisResult plantResult = null;
        Color dominantColor = Color.green;

        if (plantAnalyzer != null)
        {
            // Get dominant color from drawing
            dominantColor = drawingCanvas.GetDominantColorByCount();
            Debug.Log($"Passing dominant color to analyzer: {dominantColor}");

            // Analyze with color influence
            plantResult = plantAnalyzer.AnalyzeDrawing(drawingCanvas.allStrokes, dominantColor);
            unitData.SetPlantType(plantResult);

            // Store for panel display
            lastPlantResult = plantResult;
            lastDominantColor = dominantColor;
        }
        else
        {
            Debug.LogWarning("PlantAnalyzer not found! Plant type will be Unknown.");
        }

        // Store in DrawnUnitData
        unitData.SetStatsFromDrawing(strokeCount, totalLength, totalPoints);

        // Log final result
        if (plantResult != null)
        {
            Debug.Log($"🌱 Plant Detected: {plantResult.detectedType} ({plantResult.elementType}) - Confidence: {plantResult.confidence:P0}");
            Debug.Log($"📊 Stats: ATK={unitData.attack}, DEF={unitData.defense}, HP={unitData.health}");

            // Get available moves for this plant type
            MoveData[] moves = MoveData.GetMovesForPlant(plantResult.detectedType);
            Debug.Log($"⚔️ Available Moves:");
            foreach (var move in moves)
            {
                Debug.Log($"  - {move.moveName} ({move.element}, Power: {move.basePower})");
            }

            // Show visual feedback if enabled (old system, still supported)
            if (showDetectionFeedback && !showAnalysisPanel)
            {
                if (detectionFeedback != null)
                {
                    detectionFeedback.ShowDetectionResult(plantResult);
                }
                else
                {
                    // Try to find it in scene
                    PlantDetectionFeedback.ShowResult(plantResult);
                }
            }
        }
    }

    /// <summary>
    /// Load the battle scene
    /// </summary>
    private void LoadBattleScene()
    {
        Debug.Log($"Loading {battleSceneName}...");
        SceneManager.LoadScene(battleSceneName);
    }
}
