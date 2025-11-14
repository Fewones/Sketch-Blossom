using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages drawing system specifically for battle scene
/// This is separate from the main DrawingManager used in DrawingScene
/// </summary>
public class BattleDrawingManager : MonoBehaviour
{
    [Header("Drawing Canvas")]
    public SimpleDrawingCanvas drawingCanvas;
    public DrawingCanvas legacyDrawingCanvas; // Fallback to legacy if SimpleDrawingCanvas not available

    [Header("Drawing UI")]
    public GameObject drawingPanel;
    public RawImage drawingAreaImage; // Visual background for drawing area
    public Button submitButton;
    public Button clearButton;
    public TextMeshProUGUI instructionText;

    [Header("References")]
    public CombatManager combatManager;
    public MovesetDetector movesetDetector;

    [Header("Settings")]
    public Color drawingAreaColor = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark gray, semi-transparent
    public Vector2 drawingAreaSize = new Vector2(400, 300);

    private bool isDrawingEnabled = false;
    private PlantRecognitionSystem.PlantType currentPlantType;

    private void Start()
    {
        Debug.Log("========== BATTLE DRAWING MANAGER START ==========");

        // Only run in Battle scenes
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Battle"))
        {
            Debug.Log($"BattleDrawingManager: Not in a Battle scene (current: {sceneName}), disabling");
            enabled = false;
            return;
        }

        Debug.Log("✓ Confirmed we're in a Battle scene");

        // Auto-find components if not assigned
        if (combatManager == null)
        {
            combatManager = FindObjectOfType<CombatManager>();
            if (combatManager != null)
                Debug.Log("✓ Found CombatManager");
        }

        if (movesetDetector == null)
        {
            movesetDetector = FindObjectOfType<MovesetDetector>();
            if (movesetDetector == null)
            {
                // Create one if it doesn't exist
                movesetDetector = gameObject.AddComponent<MovesetDetector>();
                Debug.Log("✓ Created MovesetDetector");
            }
        }

        // Try to find drawing canvas
        if (drawingCanvas == null)
        {
            drawingCanvas = FindObjectOfType<SimpleDrawingCanvas>();
            if (drawingCanvas != null)
                Debug.Log("✓ Found SimpleDrawingCanvas");
        }

        // Fallback to legacy canvas
        if (drawingCanvas == null && legacyDrawingCanvas == null)
        {
            legacyDrawingCanvas = FindObjectOfType<DrawingCanvas>();
            if (legacyDrawingCanvas != null)
                Debug.Log("✓ Found legacy DrawingCanvas");
        }

        if (drawingCanvas == null && legacyDrawingCanvas == null)
        {
            Debug.LogError("❌ No drawing canvas found! Please add SimpleDrawingCanvas or DrawingCanvas to the scene.");
        }

        // Setup buttons if assigned
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitDrawing);
            Debug.Log("✓ Submit button hooked up");
        }

        if (clearButton != null)
        {
            clearButton.onClick.AddListener(OnClearDrawing);
            Debug.Log("✓ Clear button hooked up");
        }

        // Hide drawing panel initially
        if (drawingPanel != null)
        {
            drawingPanel.SetActive(false);
            Debug.Log("✓ Drawing panel hidden initially");
        }

        Debug.Log("========== BATTLE DRAWING MANAGER READY ==========");
    }

    /// <summary>
    /// Show the drawing panel and enable drawing
    /// Called by CombatManager when it's player's turn
    /// </summary>
    public void ShowDrawingPanel(PlantRecognitionSystem.PlantType plantType)
    {
        Debug.Log($"========== SHOWING DRAWING PANEL FOR {plantType} ==========");

        currentPlantType = plantType;
        isDrawingEnabled = true;

        // Show the panel
        if (drawingPanel != null)
        {
            drawingPanel.SetActive(true);
        }

        // Clear any previous drawing
        ClearCanvas();

        // Update instruction text
        if (instructionText != null)
        {
            MoveData[] availableMoves = MoveData.GetMovesForPlant(plantType);
            string movesText = "Draw your move:\n";
            foreach (var move in availableMoves)
            {
                movesText += $"• {move.moveName}\n";
            }
            instructionText.text = movesText;
        }

        // Disable submit button until something is drawn
        if (submitButton != null)
        {
            submitButton.interactable = false;
        }

        Debug.Log("✓ Drawing panel shown and ready");
    }

    /// <summary>
    /// Hide the drawing panel
    /// </summary>
    public void HideDrawingPanel()
    {
        Debug.Log("Hiding drawing panel");
        isDrawingEnabled = false;

        if (drawingPanel != null)
        {
            drawingPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Check if player has drawn something to enable submit button
    /// </summary>
    private void Update()
    {
        if (!isDrawingEnabled || submitButton == null)
            return;

        // Enable submit button if there are strokes
        int strokeCount = GetStrokeCount();
        bool hasStrokes = strokeCount > 0;

        if (submitButton.interactable != hasStrokes)
        {
            submitButton.interactable = hasStrokes;
        }
    }

    /// <summary>
    /// Get current stroke count from active canvas
    /// </summary>
    private int GetStrokeCount()
    {
        if (drawingCanvas != null)
        {
            return drawingCanvas.allStrokes.Count;
        }
        else if (legacyDrawingCanvas != null)
        {
            return legacyDrawingCanvas.currentStrokeCount;
        }
        return 0;
    }

    /// <summary>
    /// Get all strokes from active canvas
    /// </summary>
    private List<LineRenderer> GetAllStrokes()
    {
        if (drawingCanvas != null)
        {
            return drawingCanvas.allStrokes;
        }
        else if (legacyDrawingCanvas != null)
        {
            return legacyDrawingCanvas.GetAllStrokes();
        }
        return new List<LineRenderer>();
    }

    /// <summary>
    /// Clear the drawing canvas
    /// </summary>
    private void ClearCanvas()
    {
        if (drawingCanvas != null)
        {
            drawingCanvas.ClearAll();
        }
        else if (legacyDrawingCanvas != null)
        {
            legacyDrawingCanvas.ClearCanvas();
        }
    }

    /// <summary>
    /// Called when player clicks Submit button
    /// </summary>
    private void OnSubmitDrawing()
    {
        Debug.Log("========== SUBMIT DRAWING CLICKED ==========");

        List<LineRenderer> strokes = GetAllStrokes();
        int strokeCount = strokes.Count;

        Debug.Log($"Analyzing {strokeCount} strokes for {currentPlantType}");

        if (strokeCount == 0)
        {
            Debug.LogWarning("No strokes to analyze!");
            return;
        }

        // Force end any in-progress stroke
        if (drawingCanvas != null)
        {
            drawingCanvas.ForceEndStroke();
        }
        else if (legacyDrawingCanvas != null)
        {
            legacyDrawingCanvas.ForceEndStroke();
        }

        // Analyze the move using MovesetDetector
        if (movesetDetector != null)
        {
            MovesetDetector.MoveDetectionResult result = movesetDetector.DetectMove(strokes, currentPlantType);

            Debug.Log($"Move detected: {result.detectedMove}");
            Debug.Log($"Confidence: {result.confidence:P0}");
            Debug.Log($"Quality: {result.quality:P0}");
            Debug.Log($"Quality Rating: {result.qualityRating}");
            Debug.Log($"Damage Multiplier: {result.damageMultiplier:F2}x");

            // Hide drawing panel
            HideDrawingPanel();

            // Notify CombatManager that move is ready
            if (combatManager != null)
            {
                combatManager.OnMoveSubmitted(result);
            }
            else
            {
                Debug.LogError("❌ CombatManager is null! Cannot submit move.");
            }
        }
        else
        {
            Debug.LogError("❌ MovesetDetector is null! Cannot analyze move.");
        }
    }

    /// <summary>
    /// Called when player clicks Clear button
    /// </summary>
    private void OnClearDrawing()
    {
        Debug.Log("Clear button clicked - clearing canvas");
        ClearCanvas();

        // Disable submit button
        if (submitButton != null)
        {
            submitButton.interactable = false;
        }
    }

    /// <summary>
    /// Check if drawing panel is currently shown
    /// </summary>
    public bool IsDrawingPanelActive()
    {
        return drawingPanel != null && drawingPanel.activeSelf;
    }
}
