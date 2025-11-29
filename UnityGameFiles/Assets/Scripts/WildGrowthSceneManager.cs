using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using SketchBlossom.Progression; // for PlayerInventory & PlantInventoryEntry

/// <summary>
/// Manages the Wild Growth reward scene:
/// - Player draws on a canvas to power up the selected plant
/// - Drawing quality determines a multiplier (1.3x–1.8x)
/// - Shows stat preview
/// - Applies upgrade to the selected PlantInventoryEntry
/// - Transitions back to WorldMapScene
/// </summary>
public class WildGrowthSceneManager : MonoBehaviour
{
    [Header("Existing UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button continueButton;   // can be used as Confirm if confirmButton is null
    [SerializeField] private GameObject contentPanel;

    [Header("Drawing & Controls")]
    [SerializeField] private SimpleDrawingCanvas drawingCanvas;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button confirmButton;    // optional; if null, we fall back to continueButton

    [Header("Preview UI")]
    [SerializeField] private TextMeshProUGUI qualityText;
    [SerializeField] private TextMeshProUGUI hpPreviewText;
    [SerializeField] private TextMeshProUGUI attackPreviewText;
    [SerializeField] private TextMeshProUGUI defensePreviewText;

    [Header("Transition Settings")]
    [SerializeField] private bool useAutoTransition = false; // usually false now
    [SerializeField] private float autoTransitionDelay = 3f;

    [Header("Multiplier Settings")]
    [SerializeField] private float minMultiplier = 1.3f;
    [SerializeField] private float maxMultiplier = 1.8f;
    [SerializeField] private int minRequiredStrokes = 3;

    // Runtime state
    private PlantInventoryEntry selectedPlant;
    private float currentMultiplier;
    private bool hasAnyDrawing = false;

    private void Start()
    {
        LoadSelectedPlantFromInventory();
        SetupUI();

        currentMultiplier = minMultiplier;
        UpdatePreviewTexts();

        if (useAutoTransition)
        {
            StartCoroutine(AutoTransitionToWorldMap());
        }
    }

    private void Update()
    {
        // Poll drawing stats each frame and update preview
        UpdateQualityAndPreview();
    }

    private void SetupUI()
    {
        if (messageText != null)
        {
            messageText.text = "Draw to help your plant grow stronger!";
        }

        // Clear button
        if (clearButton != null)
        {
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(OnClearClicked);
        }

        // Decide which button acts as "Confirm"
        Button buttonToUse = confirmButton != null ? confirmButton : continueButton;
        if (buttonToUse != null)
        {
            buttonToUse.onClick.RemoveAllListeners();
            buttonToUse.onClick.AddListener(OnConfirmClicked);
            confirmButton = buttonToUse; // ensure confirmButton reference is set
            confirmButton.interactable = false; // until we have a valid drawing
        }

        Debug.Log("WildGrowthScene: Setup complete");
    }

    private void LoadSelectedPlantFromInventory()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("WildGrowthSceneManager: PlayerInventory.Instance is null.");
            return;
        }

        selectedPlant = PlayerInventory.Instance.GetSelectedPlant();

        if (selectedPlant == null)
        {
            Debug.LogError("WildGrowthSceneManager: No selected plant found in PlayerInventory.");
        }
        else
        {
            Debug.Log($"WildGrowthSceneManager: Loaded selected plant {selectedPlant.plantName} (ID: {selectedPlant.plantId})");
        }
    }

    private void UpdateQualityAndPreview()
    {
        if (drawingCanvas == null || selectedPlant == null)
            return;

        // This assumes your SimpleDrawingCanvas has a GetDrawingStats() returning DrawingStats
        DrawingStats stats = drawingCanvas.GetDrawingStats();

        hasAnyDrawing = stats.strokeCount > 0;
        currentMultiplier = ComputeMultiplier(stats);

        if (confirmButton != null)
        {
            confirmButton.interactable = stats.strokeCount >= minRequiredStrokes;
        }

        UpdatePreviewTexts();
    }

    private float ComputeMultiplier(DrawingStats stats)
    {
        if (stats.strokeCount == 0)
            return minMultiplier;

        // Example quality function — tweak thresholds for your game feel

        // Stroke count: 1–15 → 0–1
        float strokeScore = Mathf.InverseLerp(1, 15, stats.strokeCount);

        // Total stroke length: 50–600 → 0–1
        float lengthScore = Mathf.InverseLerp(50f, 600f, stats.totalLength);

        // Canvas coverage
        float coverageScore = 0f;
        if (stats.canvasArea > 0f)
        {
            float rawCoverage = stats.boundingBoxArea / stats.canvasArea;
            coverageScore = Mathf.Clamp01(rawCoverage * 1.5f);
        }

        float quality = (strokeScore + lengthScore + coverageScore) / 3f;
        quality = Mathf.Clamp01(quality);

        return Mathf.Lerp(minMultiplier, maxMultiplier, quality);
    }

    private void UpdatePreviewTexts()
    {
        if (qualityText == null || hpPreviewText == null ||
            attackPreviewText == null || defensePreviewText == null)
            return;

        if (selectedPlant == null)
        {
            qualityText.text = "No plant selected.";
            hpPreviewText.text = "";
            attackPreviewText.text = "";
            defensePreviewText.text = "";
            return;
        }

        int baseHP      = selectedPlant.maxHealth;
        int baseAttack  = selectedPlant.attack;
        int baseDefense = selectedPlant.defense;

        int newHP      = Mathf.RoundToInt(baseHP      * currentMultiplier);
        int newAttack  = Mathf.RoundToInt(baseAttack  * currentMultiplier);
        int newDefense = Mathf.RoundToInt(baseDefense * currentMultiplier);

        float percent = (currentMultiplier - 1f) * 100f;

        if (hasAnyDrawing)
        {
            qualityText.text = $"Quality: {percent:0}% (x{currentMultiplier:0.00})";
        }
        else
        {
            qualityText.text = $"Draw something to power up! (Base x{minMultiplier:0.00})";
        }

        hpPreviewText.text      = $"HP:  {baseHP} → {newHP} (+{percent:0}%)";
        attackPreviewText.text  = $"ATK: {baseAttack} → {newAttack} (+{percent:0}%)";
        defensePreviewText.text = $"DEF: {baseDefense} → {newDefense} (+{percent:0}%)";
    }

    private void OnClearClicked()
    {
        if (drawingCanvas != null)
        {
            // Adjust name if your method is different (e.g. ClearDrawing)
            drawingCanvas.ClearCanvas();
        }

        hasAnyDrawing = false;
        currentMultiplier = minMultiplier;

        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }

        UpdatePreviewTexts();
    }

    private void OnConfirmClicked()
    {
        if (selectedPlant == null)
        {
            Debug.LogWarning("WildGrowthSceneManager: Cannot confirm upgrade, no plant selected.");
            return;
        }

        ApplyDrawingBasedWildGrowth();
        TransitionToWorldMap();
    }

    private void ApplyDrawingBasedWildGrowth()
    {
        // Multiply stats
        int newMaxHealth = Mathf.RoundToInt(selectedPlant.maxHealth * currentMultiplier);
        int newAttack    = Mathf.RoundToInt(selectedPlant.attack    * currentMultiplier);
        int newDefense   = Mathf.RoundToInt(selectedPlant.defense   * currentMultiplier);

        selectedPlant.maxHealth    = newMaxHealth;
        selectedPlant.attack       = newAttack;
        selectedPlant.defense      = newDefense;
        selectedPlant.currentHealth = newMaxHealth; // heal to full

        // Progression bits similar to ApplyWildGrowth()
        selectedPlant.wildGrowthCount++;
        selectedPlant.level++;

        Debug.Log(
            $"Wild Growth (drawing-based) applied to {selectedPlant.plantName}! " +
            $"Now level {selectedPlant.level} with ATK:{selectedPlant.attack} HP:{selectedPlant.maxHealth} DEF:{selectedPlant.defense}"
        );

        // Persist in inventory
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.SaveInventory();
        }
    }

    private IEnumerator AutoTransitionToWorldMap()
    {
        yield return new WaitForSeconds(autoTransitionDelay);
        TransitionToWorldMap();
    }

    private void TransitionToWorldMap()
    {
        Debug.Log("WildGrowthScene: Transitioning to WorldMapScene...");

        // Keep your existing encounter clear logic
        if (EnemyEncounterData.Instance != null)
        {
            EnemyEncounterData.Instance.ClearEncounterData();
        }

        SceneManager.LoadScene("WorldMapScene");
    }

    private void OnDestroy()
    {
        if (clearButton != null)
        {
            clearButton.onClick.RemoveAllListeners();
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
        }

        if (continueButton != null && continueButton != confirmButton)
        {
            continueButton.onClick.RemoveAllListeners();
        }
    }
}