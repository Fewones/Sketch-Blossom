using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Manages the drawing phase and transitions to battle
/// Works with SimpleDrawingCanvas for drawing functionality
/// </summary>
public class DrawingManager : MonoBehaviour
{
    [Header("References")]
    public SimpleDrawingCanvas simpleCanvas;
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

        // Find SimpleDrawingCanvas
        if (simpleCanvas == null)
        {
            simpleCanvas = FindObjectOfType<SimpleDrawingCanvas>();
            if (simpleCanvas != null)
                Debug.Log("✓ Auto-found SimpleDrawingCanvas: " + simpleCanvas.gameObject.name);
            else
                Debug.LogError("❌ SimpleDrawingCanvas not found! Please add SimpleDrawingCanvas to the scene.");
        }
        else
        {
            Debug.Log("✓ SimpleDrawingCanvas assigned: " + simpleCanvas.gameObject.name);
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
    /// </summary>
    private void OnFinishDrawing()
    {
        Debug.Log("DrawingManager: Finish button pressed!");

        if (simpleCanvas == null)
        {
            Debug.LogError("❌ SimpleDrawingCanvas is null! Cannot process drawing.");
            return;
        }

        // Check if we have any strokes BEFORE hiding anything
        int strokeCount = simpleCanvas.allStrokes.Count;

        // If no strokes, show error and don't proceed
        if (strokeCount == 0)
        {
            Debug.LogError("❌ No strokes drawn! Please draw something before clicking Finish.");
            Debug.LogError("❌ Make sure SimpleDrawingCanvas references are set up correctly.");
            Debug.LogError("❌ Run: Tools > Sketch Blossom > Setup Drawing System (Auto)");
            return;
        }

        // End any in-progress stroke first
        simpleCanvas.ForceEndStroke();
        Debug.Log("Using SimpleDrawingCanvas - forced end stroke");

        // Analyze the drawing BEFORE hiding anything
        AnalyzeAndStoreDrawing();

        // Check if analysis produced a result
        if (lastRecognitionResult == null)
        {
            Debug.LogError("❌ Analysis failed! No result produced.");
            return;
        }

        Debug.Log("===== ANALYSIS SUCCEEDED - SHOWING RESULTS =====");

        // Keep strokes visible by ensuring strokeContainer stays active
        if (simpleCanvas != null && simpleCanvas.strokeContainer != null)
        {
            simpleCanvas.strokeContainer.gameObject.SetActive(true);
            Debug.Log("✓ Ensured strokes remain visible");
        }

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

        // Ensure strokes remain visible even if they were children of hidden panels
        if (simpleCanvas != null && simpleCanvas.strokeContainer != null)
        {
            simpleCanvas.strokeContainer.gameObject.SetActive(true);
            Debug.Log("✓ Strokes kept visible in background");
        }

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

        if (simpleCanvas == null)
        {
            Debug.LogError("❌ SimpleDrawingCanvas is null! Cannot clear canvas.");
            return;
        }

        // Clear the canvas
        simpleCanvas.ClearAll();
        Debug.Log("✓ SimpleCanvas cleared for redrawing");

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
        if (simpleCanvas == null)
        {
            Debug.LogError("SimpleDrawingCanvas reference missing!");
            return;
        }

        if (recognitionSystem == null)
        {
            Debug.LogError("PlantRecognitionSystem reference missing!");
            return;
        }

        Debug.Log("========== ANALYZING DRAWING ==========");

        // Get strokes and dominant color from canvas
        List<LineRenderer> strokes = simpleCanvas.allStrokes;
        Color dominantColor = simpleCanvas.GetDominantColor();
        Debug.Log($"Using SimpleDrawingCanvas - {strokes.Count} strokes");

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
