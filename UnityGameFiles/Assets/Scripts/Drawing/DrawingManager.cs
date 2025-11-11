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
    
    [Header("Scene Management")]
    public string battleSceneName = "BattleScene";

    private DrawnUnitData unitData;

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

        // Transition to battle
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

        // Store in DrawnUnitData
        unitData.SetStatsFromDrawing(strokeCount, totalLength, totalPoints);
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
