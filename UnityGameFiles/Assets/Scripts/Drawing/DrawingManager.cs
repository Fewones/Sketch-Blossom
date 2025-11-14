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

    [Header("UI Panels")]
    public GameObject drawingOverlay;
    public GameObject drawingPanel;

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

        // Auto-find drawing UI panels if not assigned
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas != null)
        {
            if (drawingOverlay == null)
            {
                Transform overlayTransform = canvas.transform.Find("DrawingOverlay");
                if (overlayTransform != null)
                {
                    drawingOverlay = overlayTransform.gameObject;
                    Debug.Log("✓ Auto-found DrawingOverlay");
                }
            }

            if (drawingPanel == null)
            {
                Transform panelTransform = canvas.transform.Find("DrawingPanel");
                if (panelTransform != null)
                {
                    drawingPanel = panelTransform.gameObject;
                    Debug.Log("✓ Auto-found DrawingPanel");
                }
            }

            // Note: We don't manage ResultOverlay/ResultPanel - PlantResultPanel handles its own visibility
        }

        // Hook into the existing finish button
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

        // Check if we have any strokes BEFORE hiding anything
        int strokeCount = 0;
        if (usingSimpleCanvas)
        {
            strokeCount = simpleCanvas.allStrokes.Count;
        }
        else if (usingLegacyCanvas)
        {
            strokeCount = drawingCanvas.allStrokes.Count;
        }

        // If no strokes, show error and don't proceed
        if (strokeCount == 0)
        {
            Debug.LogError("❌ No strokes drawn! Please draw something before clicking Finish.");
            Debug.LogError("❌ Make sure SimpleDrawingCanvas references are set up correctly.");
            Debug.LogError("❌ Run: Tools > Sketch Blossom > Setup Drawing System (Auto)");
            return;
        }

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

        // Analyze the drawing BEFORE hiding anything
        AnalyzeAndStoreDrawing();

        // Check if analysis produced a result
        if (lastRecognitionResult == null)
        {
            Debug.LogError("❌ Analysis failed! No result produced.");
            return;
        }

        Debug.Log("===== ANALYSIS SUCCEEDED - SHOWING RESULTS =====");

        // HIDE DRAWING UI (only drawing UI - let PlantResultPanel show itself)
        if (drawingOverlay != null)
        {
            drawingOverlay.SetActive(false);
            Debug.Log("✓ Hidden DrawingOverlay");
        }

        if (drawingPanel != null)
        {
            drawingPanel.SetActive(false);
            Debug.Log("✓ Hidden DrawingPanel");
        }

        // Leave strokes visible - they'll be in the background behind the result panel
        Debug.Log("✓ Keeping strokes visible in background");

        // Show result data - PlantResultPanel will handle showing its own overlay/panel
        if (plantResultPanel != null)
        {
            Debug.Log("✓ Calling PlantResultPanel.ShowResults() - it will show its own UI");
            // BATTLE SCENE DISABLED FOR TESTING - Pass null instead of LoadBattleScene
            plantResultPanel.ShowResults(lastRecognitionResult, unitData, null, OnRedrawRequested);
        }
        else
        {
            Debug.LogError("❌ PlantResultPanel is NULL! Cannot show results.");
            Debug.LogError("❌ Run: Tools > Sketch Blossom > Setup Drawing System (Auto)");
        }
    }

    /// <summary>
    /// Called when player clicks "Redraw" button
    /// PlantResultPanel has already hidden itself before calling this callback
    /// </summary>
    private void OnRedrawRequested()
    {
        Debug.Log("===== REDRAW REQUESTED =====");

        // Determine which canvas system we're using
        bool usingSimpleCanvas = (simpleCanvas != null);
        bool usingLegacyCanvas = (drawingCanvas != null);

        // Clear the canvas
        if (usingSimpleCanvas)
        {
            simpleCanvas.ClearAll();
            Debug.Log("✓ SimpleCanvas cleared for redrawing");
        }
        else if (usingLegacyCanvas)
        {
            drawingCanvas.ClearCanvas();
            Debug.Log("✓ Legacy canvas cleared for redrawing");
        }

        // Clear stored results
        lastRecognitionResult = null;
        lastDominantColor = Color.green;

        // SHOW DRAWING UI
        // Note: PlantResultPanel has already hidden its own UI before calling this callback
        if (drawingOverlay != null)
        {
            drawingOverlay.SetActive(true);
            Debug.Log("✓ Showing DrawingOverlay");
        }

        if (drawingPanel != null)
        {
            drawingPanel.SetActive(true);
            Debug.Log("✓ Showing DrawingPanel");
        }

        Debug.Log("✓ Ready to draw again!");
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
