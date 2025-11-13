using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the drawing phase and transitions to battle
/// Works alongside your existing DrawingCanvas script
/// </summary>
public class DrawingManager : MonoBehaviour
{
    [Header("References")]
    public DrawingCanvas drawingCanvas;
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
        // Try to find it ourselves if DrawingCanvas hasn't found it yet
        if (drawingCanvas != null)
        {
            if (drawingCanvas.finishButton == null)
            {
                Debug.Log("⚠️ DrawingCanvas.finishButton is null, attempting manual search...");

                // Search for FinishButton in the Canvas hierarchy
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    Transform drawingPanelTransform = canvas.transform.Find("DrawingPanel");
                    if (drawingPanelTransform != null)
                    {
                        Transform buttonTransform = drawingPanelTransform.Find("FinishButton");
                        if (buttonTransform != null)
                        {
                            drawingCanvas.finishButton = buttonTransform.GetComponent<Button>();
                            if (drawingCanvas.finishButton != null)
                            {
                                Debug.Log("✓ Found and assigned FinishButton manually!");
                            }
                        }
                        else
                        {
                            Debug.LogError("❌ FinishButton not found in DrawingPanel!");
                        }
                    }
                    else
                    {
                        Debug.LogError("❌ DrawingPanel not found in Canvas!");
                    }
                }
            }

            // Now try to hook it up
            if (drawingCanvas.finishButton != null)
            {
                // Remove existing listener and add our own
                int oldListeners = drawingCanvas.finishButton.onClick.GetPersistentEventCount();
                drawingCanvas.finishButton.onClick.RemoveAllListeners();
                drawingCanvas.finishButton.onClick.AddListener(OnFinishDrawing);
                Debug.Log($"✓ Finish button hooked up (removed {oldListeners} old listeners, added OnFinishDrawing)");
            }
            else
            {
                Debug.LogError("❌ Cannot hook finish button - finishButton still NULL after manual search!");
                Debug.LogError("❌ Make sure 'DrawingPanel/FinishButton' exists in your Canvas hierarchy");
            }
        }
        else
        {
            Debug.LogError("❌ Cannot hook finish button - DrawingCanvas is NULL!");
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

        // End any in-progress stroke first
        if (drawingCanvas != null)
        {
            // Force end current stroke if one is being drawn
            drawingCanvas.ForceEndStroke();
        }

        // Hide the drawn strokes so they don't render over the result panel
        if (drawingCanvas != null && drawingCanvas.strokeContainer != null)
        {
            drawingCanvas.strokeContainer.gameObject.SetActive(false);
            Debug.Log("Hidden stroke container to show result panel");
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

        // Show the stroke container again
        if (drawingCanvas != null && drawingCanvas.strokeContainer != null)
        {
            drawingCanvas.strokeContainer.gameObject.SetActive(true);
            Debug.Log("Showed stroke container for redrawing");
        }

        // Clear the canvas
        if (drawingCanvas != null)
        {
            drawingCanvas.ClearCanvas();
            Debug.Log("Canvas cleared for redrawing");
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
        if (drawingCanvas == null)
        {
            Debug.LogError("DrawingCanvas reference missing!");
            return;
        }

        if (recognitionSystem == null)
        {
            Debug.LogError("PlantRecognitionSystem reference missing!");
            return;
        }

        Debug.Log("========== ANALYZING DRAWING ==========");

        // Get stroke count
        int strokeCount = drawingCanvas.allStrokes.Count;
        Debug.Log($"Total strokes to analyze: {strokeCount}");

        if (strokeCount == 0)
        {
            Debug.LogWarning("No strokes drawn! Cannot analyze.");
            return;
        }

        // Get dominant color from drawing
        Color dominantColor = drawingCanvas.GetDominantColorByCount();
        lastDominantColor = dominantColor;
        Debug.Log($"Dominant drawing color: {dominantColor}");

        // Use new recognition system
        PlantRecognitionSystem.RecognitionResult result = recognitionSystem.AnalyzeDrawing(
            drawingCanvas.allStrokes,
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
