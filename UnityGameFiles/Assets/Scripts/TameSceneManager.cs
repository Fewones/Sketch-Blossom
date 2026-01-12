using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using SketchBlossom.Progression;

public class TameGrowthManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private TextMeshProUGUI referenceRulesText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [SerializeField] private Button clearButton;
    [SerializeField] private Button submitButton;

    [Header("Drawing")]
    [SerializeField] private SimpleDrawingCanvas drawingCanvas;

    [Header("Systems")]
    [SerializeField] private PlantRecognitionSystem recognitionSystem;
    [SerializeField] private DrawingCaptureHandler captureHandler;

    [Header("Validation")]
    [SerializeField] private float minConfidence = 0.30f;

    [Header("Scene Names")]
    [SerializeField] private string worldMapSceneName = "WorldMapScene";

    [SerializeField] private TextMeshProUGUI strokeCounterText;
    [SerializeField] private int maxStrokesForTame = 15;

    // Runtime
    private PlantRecognitionSystem.PlantType requiredPlantType;
    private bool isSubmitting = false;
    private bool tameCompleted = false; // CRITICAL: prevents double add / re-fire

    private void Awake()
    {
        if (recognitionSystem == null)
            recognitionSystem = FindFirstObjectByType<PlantRecognitionSystem>();

        if (captureHandler == null)
            captureHandler = FindFirstObjectByType<DrawingCaptureHandler>();

        if (drawingCanvas == null)
            drawingCanvas = FindFirstObjectByType<SimpleDrawingCanvas>();
    }

    private void Start()
    {
        if (EnemyUnitData.Instance == null || !EnemyUnitData.Instance.HasData())
        {
            Debug.LogError("[TameGrowth] EnemyUnitData missing. Returning to WorldMap.");
            SceneManager.LoadScene(worldMapSceneName);
            return;
        }

        requiredPlantType = EnemyUnitData.Instance.plantType;

        if (promptText != null)
            promptText.text = $"Draw a {requiredPlantType} to tame it";

        if (referenceRulesText != null)
            referenceRulesText.text = BuildGuideText(requiredPlantType);

        SetFeedback("");

        if (clearButton != null)
        {
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(OnClearClicked);
        }

        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
            submitButton.interactable = true;
        }

        Debug.Log($"[TameGrowth] Ready. Required plant type: {requiredPlantType}");
        if (drawingCanvas != null)
            drawingCanvas.maxStrokes = maxStrokesForTame;

        UpdateStrokeCounter(); // initial

    }

    private void Update()
    {
        UpdateStrokeCounter();
    }

    private void UpdateStrokeCounter()
    {
        if (strokeCounterText == null || drawingCanvas == null)
            return;

        // Includes in-progress stroke so it reacts instantly
        int current = drawingCanvas.GetDrawingStats().strokeCount;
        int max = drawingCanvas.maxStrokes;

        strokeCounterText.text = $"Strokes: {current}/{max}";
    }



    private void OnClearClicked()
    {
        drawingCanvas?.ClearCanvas();
        SetFeedback("");
    }

    private void OnSubmitClicked()
    {
        if (tameCompleted) return;     // already succeeded, do nothing
        if (isSubmitting) return;      // prevent double click
        isSubmitting = true;

        // Step 3: local guards
        if (drawingCanvas == null)
        {
            Debug.LogError("[TameGrowth] drawingCanvas not assigned.");
            SetFeedback("Drawing system not found.");
            isSubmitting = false;
            return;
        }

        if (recognitionSystem == null)
        {
            Debug.LogError("[TameGrowth] recognitionSystem not assigned/found.");
            SetFeedback("Recognition system not found.");
            isSubmitting = false;
            return;
        }

        drawingCanvas.ForceEndStroke();

        if (drawingCanvas.allStrokes == null || drawingCanvas.allStrokes.Count == 0)
        {
            SetFeedback($"Please draw a {requiredPlantType} first.");
            isSubmitting = false;
            return;
        }

        // Step 4: VALIDATION ONLY
        if (!ValidateDrawingAgainstEnemyType(out var recognitionResult, out string failMessage))
        {
            SetFeedback(failMessage);
            isSubmitting = false;
            return;
        }

        // Step 5: SUCCESS ACTIONS
        HandleSuccessfulTame(recognitionResult);

        isSubmitting = false;
    }

    /// <summary>
    /// STEP 4: Validates the player's drawing against the required enemy plant type.
    /// This function does NOT modify inventory or transition scenes.
    /// </summary>
    private bool ValidateDrawingAgainstEnemyType(
        out PlantRecognitionSystem.RecognitionResult recognitionResult,
        out string failureMessage)
    {
        recognitionResult = null;
        failureMessage = "";

        List<LineRenderer> strokes = drawingCanvas.allStrokes;
        Color dominant = drawingCanvas.GetDominantColor();

        var result = recognitionSystem.AnalyzeDrawing("flame tulip", 0.3, strokes, dominant);
        if (result == null)
        {
            failureMessage = "Could not analyze drawing. Try again.";
            return false;
        }

        if (!result.isValidPlant)
        {
            failureMessage = $"Drawing doesn't match {requiredPlantType}. Try again!";
            return false;
        }

        if (result.confidence < minConfidence)
        {
            failureMessage = $"Not confident enough. Draw a clearer {requiredPlantType} and try again.";
            return false;
        }

        // Keep YOUR original line (you said this one is correct for your project)
        PlantRecognitionSystem.PlantType detectedType = result.plantData.type;

        if (detectedType != requiredPlantType)
        {
            failureMessage = $"Drawing doesn't match {requiredPlantType}. Try again!";
            Debug.Log($"[TameGrowth] FAIL: required={requiredPlantType}, detected={detectedType}, conf={result.confidence:0.00}");
            return false;
        }

        Debug.Log($"[TameGrowth] VALIDATED: required={requiredPlantType}, detected={detectedType}, conf={result.confidence:0.00}");
        recognitionResult = result;
        return true;
    }

    /// <summary>
    /// STEP 5: Runs only after validation succeeded.
    /// Captures texture, adds to inventory, saves, clears data, transitions.
    /// </summary>
    private void HandleSuccessfulTame(PlantRecognitionSystem.RecognitionResult recognitionResult)
    {
        tameCompleted = true;

        if (submitButton != null)
            submitButton.interactable = false;

        SetFeedback($"Tamed {EnemyUnitData.Instance.plantDisplayName}!");

        Texture2D tex = CapturePlayerDrawingTexture();
        AddTamedPlantToInventory(recognitionResult, tex);
        TransitionToWorldMap();
    }

    private Texture2D CapturePlayerDrawingTexture()
    {
        if (captureHandler == null)
        {
            Debug.LogWarning("[TameGrowth] No DrawingCaptureHandler found. Drawing texture will be null.");
            return null;
        }

        Camera cam = drawingCanvas != null ? drawingCanvas.mainCamera : null;
        if (cam == null) cam = Camera.main;

        if (cam == null)
        {
            Debug.LogWarning("[TameGrowth] No camera found for capture. Drawing texture will be null.");
            return null;
        }

        return captureHandler.CaptureDrawing(
            drawingCanvas.allStrokes,
            cam,
            drawingCanvas.drawingArea
        );
    }

    private void AddTamedPlantToInventory(
        PlantRecognitionSystem.RecognitionResult recognitionResult,
        Texture2D drawingTexture)
    {
        if (DrawnUnitData.Instance == null)
        {
            GameObject go = new GameObject("DrawnUnitData");
            go.AddComponent<DrawnUnitData>();
        }

        // Use enemy stats/type; store player's drawing texture as the unique sprite
        DrawnUnitData.Instance.plantType = EnemyUnitData.Instance.plantType;
        DrawnUnitData.Instance.element = EnemyUnitData.Instance.element;
        DrawnUnitData.Instance.plantDisplayName = EnemyUnitData.Instance.plantDisplayName;

        DrawnUnitData.Instance.health = EnemyUnitData.Instance.health;
        DrawnUnitData.Instance.attack = EnemyUnitData.Instance.attack;
        DrawnUnitData.Instance.defense = EnemyUnitData.Instance.defense;

        DrawnUnitData.Instance.detectionConfidence = recognitionResult.confidence;
        DrawnUnitData.Instance.drawingTexture = drawingTexture;
        DrawnUnitData.Instance.unitColor = EnemyUnitData.Instance.unitColor;

        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("[TameGrowth] PlayerInventory.Instance is null. Cannot add tamed plant.");
            return;
        }

        PlantInventoryEntry newPlant = PlayerInventory.Instance.AddPlant(DrawnUnitData.Instance);
        if (newPlant != null)
        {
            PlayerInventory.Instance.SelectPlant(newPlant.plantId);
            PlayerInventory.Instance.SaveInventory();
            Debug.Log($"[TameGrowth] Added tamed plant to inventory: {newPlant.plantName} (ID: {newPlant.plantId})");
        }
        else
        {
            Debug.LogWarning("[TameGrowth] AddPlant returned null.");
        }
    }

    private void TransitionToWorldMap()
    {
        if (EnemyEncounterData.Instance != null)
            EnemyEncounterData.Instance.ClearEncounterData();

        if (EnemyUnitData.Instance != null)
            EnemyUnitData.Instance.Clear();

        SceneManager.LoadScene(worldMapSceneName);
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null)
            feedbackText.text = msg;
    }

    private string BuildGuideText(PlantRecognitionSystem.PlantType type)
    {
        switch (type)
        {
            case PlantRecognitionSystem.PlantType.GrassSprout:
                return "Guide:\n\n• Thin stem\n• Small leaves\n• Not too round\n• Keep it clean (no scribbles)";
            default:
                return $"Guide:\n\n• Clear silhouette for {type}\n• 2–4 major parts (not a scribble)\n• Keep it inside the box\n• Draw bigger if it fails";
        }
    }
}
