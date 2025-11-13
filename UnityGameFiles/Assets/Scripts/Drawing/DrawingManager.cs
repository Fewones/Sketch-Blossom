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
    public PlantResultPanel plantResultPanel; // NEW: Proper result panel

    [Header("Scene Management")]
    public string battleSceneName = "BattleScene";

    private DrawnUnitData unitData;
    private PlantAnalyzer.PlantAnalysisResult lastPlantResult;
    private Color lastDominantColor;

    private void Start()
    {
        Debug.Log("========== DRAWING MANAGER START ==========");

        // Create or get the unit data object
        if (DrawnUnitData.Instance == null)
        {
            GameObject dataObj = new GameObject("DrawnUnitData");
            unitData = dataObj.AddComponent<DrawnUnitData>();
            Debug.Log("✓ Created new DrawnUnitData");
        }
        else
        {
            unitData = DrawnUnitData.Instance;
            unitData.ClearData(); // Clear previous data
            Debug.Log("✓ Using existing DrawnUnitData");
        }

        if (drawingCanvas == null)
        {
            drawingCanvas = FindObjectOfType<DrawingCanvas>();
            if (drawingCanvas != null)
                Debug.Log("✓ Auto-found DrawingCanvas: " + drawingCanvas.gameObject.name);
            else
                Debug.LogError("❌ DrawingCanvas NOT FOUND!");
        }
        else
        {
            Debug.Log("✓ DrawingCanvas assigned: " + drawingCanvas.gameObject.name);
        }

        if (plantAnalyzer == null)
        {
            plantAnalyzer = FindObjectOfType<PlantAnalyzer>();
            if (plantAnalyzer == null)
            {
                // Create PlantAnalyzer if it doesn't exist
                GameObject analyzerObj = new GameObject("PlantAnalyzer");
                plantAnalyzer = analyzerObj.AddComponent<PlantAnalyzer>();
                Debug.Log("✓ Created new PlantAnalyzer");
            }
            else
            {
                Debug.Log("✓ Auto-found PlantAnalyzer: " + plantAnalyzer.gameObject.name);
            }
        }
        else
        {
            Debug.Log("✓ PlantAnalyzer assigned: " + plantAnalyzer.gameObject.name);
        }

        // Auto-find result panel (check inactive too)
        if (plantResultPanel == null)
        {
            plantResultPanel = FindFirstObjectByType<PlantResultPanel>(FindObjectsInactive.Include);
            if (plantResultPanel == null)
            {
                Debug.LogError("❌ PlantResultPanel NOT FOUND!");
                Debug.LogError("❌ Please run: Tools > Sketch Blossom > Setup Plant Result Panel (Complete)");
            }
            else
            {
                Debug.Log("✓ Auto-found PlantResultPanel: " + plantResultPanel.gameObject.name);
            }
        }
        else
        {
            Debug.Log("✓ PlantResultPanel assigned: " + plantResultPanel.gameObject.name);
        }

        // Hook into the existing finish button
        if (drawingCanvas != null && drawingCanvas.finishButton != null)
        {
            // Remove existing listener and add our own
            int oldListeners = drawingCanvas.finishButton.onClick.GetPersistentEventCount();
            drawingCanvas.finishButton.onClick.RemoveAllListeners();
            drawingCanvas.finishButton.onClick.AddListener(OnFinishDrawing);
            Debug.Log($"✓ Finish button hooked up (removed {oldListeners} old listeners, added OnFinishDrawing)");
        }
        else
        {
            if (drawingCanvas == null)
                Debug.LogError("❌ Cannot hook finish button - DrawingCanvas is NULL!");
            else if (drawingCanvas.finishButton == null)
                Debug.LogError("❌ Cannot hook finish button - finishButton is NULL!");
        }

        Debug.Log("========== DRAWING MANAGER START COMPLETE ==========\n");
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

        // Show result panel
        if (plantResultPanel != null && lastPlantResult != null)
        {
            Debug.Log("===== SHOWING RESULT PANEL =====");
            plantResultPanel.ShowResults(lastPlantResult, lastDominantColor, unitData, LoadBattleScene, OnRedrawRequested);
        }
        else
        {
            Debug.LogError("Cannot show results - panel or result is null!");
            Debug.LogError($"Panel: {(plantResultPanel != null ? "OK" : "NULL")}");
            Debug.LogError($"Result: {(lastPlantResult != null ? "OK" : "NULL")}");

            // Try to find the panel
            if (plantResultPanel == null)
            {
                plantResultPanel = FindFirstObjectByType<PlantResultPanel>();
                if (plantResultPanel != null)
                {
                    Debug.Log("Found PlantResultPanel via FindFirstObjectByType, retrying...");
                    plantResultPanel.ShowResults(lastPlantResult, lastDominantColor, unitData, LoadBattleScene, OnRedrawRequested);
                    return;
                }
            }

            // Fallback - just transition after delay
            Debug.LogError("FALLBACK: Transitioning to battle in 3 seconds...");
            Invoke("LoadBattleScene", 3f);
        }
    }

    /// <summary>
    /// Called when player clicks "Redraw" button
    /// </summary>
    private void OnRedrawRequested()
    {
        Debug.Log("DrawingManager: Redraw requested");

        // Clear the canvas
        if (drawingCanvas != null)
        {
            drawingCanvas.ClearCanvas();
            Debug.Log("Canvas cleared for redrawing");
        }

        // Clear stored results
        lastPlantResult = null;
        lastDominantColor = Color.green;

        // Note: The drawing panel should already be visible
        // The player can now draw again
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
        }
        else
        {
            Debug.LogError("Plant analysis failed - no result!");
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
