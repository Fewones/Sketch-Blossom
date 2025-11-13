using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Manages the drawing phase and transitions to battle
/// Works alongside your existing DrawingCanvas script
/// </summary>
public class DrawingManager : MonoBehaviour
{
    [Header("References")]
    public DrawingCanvas drawingCanvas;  // Legacy canvas (keep for backward compatibility)
    public SimpleDrawingCanvas simpleCanvas;  // New simplified canvas (preferred)
    public PlantRecognitionSystem recognitionSystem;
    public PlantResultPanel plantResultPanel;

    [Header("Scene Management")]
    public string battleSceneName = "BattleScene";

    private DrawnUnitData unitData;
    private PlantRecognitionSystem.RecognitionResult lastRecognitionResult;
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

        // Try to find SimpleDrawingCanvas first (preferred)
        if (simpleCanvas == null)
        {
            simpleCanvas = FindObjectOfType<SimpleDrawingCanvas>();
            if (simpleCanvas != null)
                Debug.Log("✓ Auto-found SimpleDrawingCanvas: " + simpleCanvas.gameObject.name);
        }
        else
        {
            Debug.Log("✓ SimpleDrawingCanvas assigned: " + simpleCanvas.gameObject.name);
        }

        // Fall back to legacy DrawingCanvas if SimpleDrawingCanvas not found
        if (simpleCanvas == null && drawingCanvas == null)
        {
            drawingCanvas = FindObjectOfType<DrawingCanvas>();
            if (drawingCanvas != null)
                Debug.Log("✓ Auto-found legacy DrawingCanvas: " + drawingCanvas.gameObject.name);
            else
                Debug.LogError("❌ No drawing canvas found! Please add SimpleDrawingCanvas or DrawingCanvas to the scene.");
        }
        else if (drawingCanvas != null && simpleCanvas == null)
        {
            Debug.Log("✓ Using legacy DrawingCanvas: " + drawingCanvas.gameObject.name);
        }

        if (recognitionSystem == null)
        {
            recognitionSystem = FindObjectOfType<PlantRecognitionSystem>();
            if (recognitionSystem == null)
            {
                // Create PlantRecognitionSystem if it doesn't exist
                GameObject recognitionObj = new GameObject("PlantRecognitionSystem");
                recognitionSystem = recognitionObj.AddComponent<PlantRecognitionSystem>();
                Debug.Log("✓ Created new PlantRecognitionSystem");
            }
            else
            {
                Debug.Log("✓ Auto-found PlantRecognitionSystem: " + recognitionSystem.gameObject.name);
            }
        }
        else
        {
            Debug.Log("✓ PlantRecognitionSystem assigned: " + recognitionSystem.gameObject.name);
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
        // Try to find it in the Canvas hierarchy
        Canvas canvas = FindFirstObjectByType<Canvas>();
        Button finishButton = null;

        if (canvas != null)
        {
            Transform drawingPanelTransform = canvas.transform.Find("DrawingPanel");
            if (drawingPanelTransform != null)
            {
                Transform buttonTransform = drawingPanelTransform.Find("FinishButton");
                if (buttonTransform != null)
                {
                    finishButton = buttonTransform.GetComponent<Button>();
                    if (finishButton != null)
                    {
                        Debug.Log("✓ Found FinishButton in DrawingPanel");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ FinishButton not found in DrawingPanel!");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ DrawingPanel not found in Canvas!");
            }
        }

        // Hook up the finish button
        if (finishButton != null)
        {
            // Remove existing listeners and add our own
            int oldListeners = finishButton.onClick.GetPersistentEventCount();
            finishButton.onClick.RemoveAllListeners();
            finishButton.onClick.AddListener(OnFinishDrawing);
            Debug.Log($"✓ Finish button hooked up (removed {oldListeners} old listeners, added OnFinishDrawing)");

            // Also assign it to legacy canvas if it exists
            if (drawingCanvas != null)
            {
                drawingCanvas.finishButton = finishButton;
                Debug.Log("✓ Assigned finish button to legacy DrawingCanvas");
            }
        }
        else
        {
            Debug.LogError("❌ Cannot hook finish button - FinishButton not found!");
            Debug.LogError("❌ Make sure 'DrawingPanel/FinishButton' exists in your Canvas hierarchy");
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

        // Determine which canvas system we're using
        bool usingSimpleCanvas = (simpleCanvas != null);
        bool usingLegacyCanvas = (drawingCanvas != null);

        // End any in-progress stroke first
        if (usingSimpleCanvas)
        {
            simpleCanvas.ForceEndStroke();
            Debug.Log("Using SimpleDrawingCanvas - forced end stroke");
        }
        else if (usingLegacyCanvas)
        {
            drawingCanvas.ForceEndStroke();
            Debug.Log("Using legacy DrawingCanvas - forced end stroke");
        }

        // Hide the drawn strokes so they don't render over the result panel
        if (usingSimpleCanvas && simpleCanvas.strokeContainer != null)
        {
            simpleCanvas.strokeContainer.gameObject.SetActive(false);
            Debug.Log("Hidden SimpleCanvas stroke container to show result panel");
        }
        else if (usingLegacyCanvas && drawingCanvas.strokeContainer != null)
        {
            drawingCanvas.strokeContainer.gameObject.SetActive(false);
            Debug.Log("Hidden legacy canvas stroke container to show result panel");
        }

        // Analyze the drawing using the new recognition system
        AnalyzeAndStoreDrawing();

        // Show result panel
        if (plantResultPanel != null && lastRecognitionResult != null)
        {
            Debug.Log("===== SHOWING RESULT PANEL =====");
            // BATTLE SCENE DISABLED FOR TESTING - Pass null instead of LoadBattleScene
            plantResultPanel.ShowResults(lastRecognitionResult, unitData, null, OnRedrawRequested);
        }
        else
        {
            Debug.LogError("Cannot show results - panel or result is null!");
            Debug.LogError($"Panel: {(plantResultPanel != null ? "OK" : "NULL")}");
            Debug.LogError($"Result: {(lastRecognitionResult != null ? "OK" : "NULL")}");

            // Try to find the panel
            if (plantResultPanel == null)
            {
                plantResultPanel = FindFirstObjectByType<PlantResultPanel>();
                if (plantResultPanel != null)
                {
                    Debug.Log("Found PlantResultPanel via FindFirstObjectByType, retrying...");
                    // BATTLE SCENE DISABLED FOR TESTING - Pass null instead of LoadBattleScene
                    plantResultPanel.ShowResults(lastRecognitionResult, unitData, null, OnRedrawRequested);
                    return;
                }
            }

            // BATTLE SCENE DISABLED FOR TESTING
            Debug.LogWarning("BATTLE SCENE TRANSITION DISABLED - Stay in drawing scene for testing");
            // Invoke("LoadBattleScene", 3f);
        }
    }

    /// <summary>
    /// Called when player clicks "Redraw" button
    /// </summary>
    private void OnRedrawRequested()
    {
        Debug.Log("DrawingManager: Redraw requested");

        // Determine which canvas system we're using
        bool usingSimpleCanvas = (simpleCanvas != null);
        bool usingLegacyCanvas = (drawingCanvas != null);

        // Show the stroke container again
        if (usingSimpleCanvas && simpleCanvas.strokeContainer != null)
        {
            simpleCanvas.strokeContainer.gameObject.SetActive(true);
            Debug.Log("Showed SimpleCanvas stroke container for redrawing");
        }
        else if (usingLegacyCanvas && drawingCanvas.strokeContainer != null)
        {
            drawingCanvas.strokeContainer.gameObject.SetActive(true);
            Debug.Log("Showed legacy canvas stroke container for redrawing");
        }

        // Clear the canvas
        if (usingSimpleCanvas)
        {
            simpleCanvas.ClearAll();
            Debug.Log("SimpleCanvas cleared for redrawing");
        }
        else if (usingLegacyCanvas)
        {
            drawingCanvas.ClearCanvas();
            Debug.Log("Legacy canvas cleared for redrawing");
        }

        // Clear stored results
        lastRecognitionResult = null;
        lastDominantColor = Color.green;

        // Note: The drawing panel should already be visible
        // The player can now draw again
    }

    /// <summary>
    /// Analyze the drawing and store stats using the new recognition system
    /// </summary>
    private void AnalyzeAndStoreDrawing()
    {
        // Determine which canvas system we're using
        bool usingSimpleCanvas = (simpleCanvas != null);
        bool usingLegacyCanvas = (drawingCanvas != null);

        if (!usingSimpleCanvas && !usingLegacyCanvas)
        {
            Debug.LogError("No drawing canvas reference found! Please assign either SimpleDrawingCanvas or DrawingCanvas.");
            return;
        }

        if (recognitionSystem == null)
        {
            Debug.LogError("PlantRecognitionSystem reference missing!");
            return;
        }

        Debug.Log("========== ANALYZING DRAWING ==========");

        // Get strokes and dominant color from appropriate canvas
        List<LineRenderer> strokes;
        Color dominantColor;

        if (usingSimpleCanvas)
        {
            strokes = simpleCanvas.allStrokes;
            dominantColor = simpleCanvas.GetDominantColor();
            Debug.Log($"Using SimpleDrawingCanvas - {strokes.Count} strokes");
        }
        else
        {
            strokes = drawingCanvas.allStrokes;
            dominantColor = drawingCanvas.GetDominantColorByCount();
            Debug.Log($"Using legacy DrawingCanvas - {strokes.Count} strokes");
        }

        // Validate stroke count
        int strokeCount = strokes.Count;
        Debug.Log($"Total strokes to analyze: {strokeCount}");

        if (strokeCount == 0)
        {
            Debug.LogWarning("No strokes drawn! Cannot analyze.");
            return;
        }

        lastDominantColor = dominantColor;
        Debug.Log($"Dominant drawing color: {dominantColor}");

        // Use new recognition system
        PlantRecognitionSystem.RecognitionResult result = recognitionSystem.AnalyzeDrawing(
            strokes,
            dominantColor
        );

        // Store result
        lastRecognitionResult = result;

        // Update DrawnUnitData with recognized plant
        unitData.SetPlantData(result.plantData);

        // Get available moves for this plant type
        MoveData[] moves = MoveData.GetMovesForPlant(result.plantType);

        Debug.Log("========== ANALYSIS COMPLETE ==========");
        Debug.Log($"🌱 Recognized: {result.plantData.displayName}");
        Debug.Log($"🔥 Element: {result.element}");
        Debug.Log($"⭐ Confidence: {result.confidence:P0}");
        Debug.Log($"💚 Stats: HP={result.plantData.baseHP}, ATK={result.plantData.baseAttack}, DEF={result.plantData.baseDefense}");
        Debug.Log($"⚔️ Available Moves: {moves.Length}");
        foreach (var move in moves)
        {
            Debug.Log($"   - {move.moveName} (Power: {move.basePower})");
        }
    }

    /// <summary>
    /// Load the battle scene - DISABLED FOR TESTING
    /// </summary>
    private void LoadBattleScene()
    {
        Debug.LogWarning("⚠️ BATTLE SCENE LOADING DISABLED FOR TESTING ⚠️");
        Debug.LogWarning($"Would have loaded: {battleSceneName}");
        Debug.LogWarning("Stay in drawing scene to test plant recognition");
        // SceneManager.LoadScene(battleSceneName);
    }
}
