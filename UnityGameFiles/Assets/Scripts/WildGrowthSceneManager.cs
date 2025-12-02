using System; // for Convert.ToBase64String
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
/// - Captures the upgraded drawing and stores it in inventory / DrawnUnitData
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

    [Header("Capture Settings")]
    [Tooltip("RectTransform that defines the drawing area we capture (usually the DrawingArea under WG_DrawingPanel).")]
    [SerializeField] private RectTransform drawingCaptureArea;

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
    private bool isConfirming = false;

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

        // SimpleDrawingCanvas -> DrawingStats
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

        // Stroke score: reward 4–12 strokes most
        float strokeScore;
        if (stats.strokeCount <= 2)
            strokeScore = 0f;
        else if (stats.strokeCount >= 12)
            strokeScore = 1f;
        else
            strokeScore = Mathf.InverseLerp(2, 12, stats.strokeCount);

        // Length score: assume 200–1000 units is "good"
        float lengthScore = Mathf.InverseLerp(200f, 1000f, stats.totalLength);

        // Coverage: 5–40% of canvas is good
        float coverageScore = 0f;
        if (stats.canvasArea > 0f)
        {
            float coverage = stats.boundingBoxArea / stats.canvasArea; // 0–1
            coverageScore = Mathf.InverseLerp(0.05f, 0.4f, coverage);
        }

        // Weighted average (tweak weights if you like)
        float quality =
            0.4f * strokeScore +
            0.3f * lengthScore +
            0.3f * coverageScore;

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

        if (!hasAnyDrawing)
        {
            qualityText.text = "Draw to help your plant grow stronger!";
        }
        else if (confirmButton != null && !confirmButton.interactable)
        {
            qualityText.text = "Keep drawing… (need a bit more detail)";
        }
        else
        {
            qualityText.text = $"Quality: {percent:0}% (x{currentMultiplier:0.00})";
        }

        hpPreviewText.text      = $"HP:  {baseHP} → {newHP} (+{percent:0}%)";
        attackPreviewText.text  = $"ATK: {baseAttack} → {newAttack} (+{percent:0}%)";
        defensePreviewText.text = $"DEF: {baseDefense} → {newDefense} (+{percent:0}%)";
    }

    private void OnClearClicked()
    {
        if (drawingCanvas != null)
        {
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
        if (isConfirming)
            return;

        if (selectedPlant == null)
        {
            Debug.LogWarning("WildGrowthSceneManager: Cannot confirm upgrade, no plant selected.");
            return;
        }

        StartCoroutine(ConfirmAndCaptureRoutine());
    }

    /// <summary>
    /// Confirms the upgrade: finalizes strokes, captures the drawing,
    /// applies stat changes + new texture, saves, then transitions to world map.
    /// </summary>
    private IEnumerator ConfirmAndCaptureRoutine()
    {
        isConfirming = true;

        // Make sure any in-progress stroke is finished
        if (drawingCanvas != null)
        {
            drawingCanvas.ForceEndStroke();
        }

        // Wait for end of frame so drawing is fully rendered
        yield return new WaitForEndOfFrame();

        // Capture the drawing area to a texture
        Texture2D captured = CaptureDrawingToTexture();

        // Apply stat upgrade
        ApplyDrawingBasedWildGrowth();

        // Apply new art to DrawnUnitData + inventory
        ApplyNewDrawingTexture(captured);

        // Transition back to world map
        TransitionToWorldMap();

        isConfirming = false;
    }

    /// <summary>
    /// Captures the drawing area (DrawingArea RectTransform) to a Texture2D.
    /// </summary>
    private Texture2D CaptureDrawingToTexture()
    {
        if (drawingCaptureArea == null || drawingCanvas == null || drawingCanvas.mainCamera == null)
        {
            Debug.LogWarning("WildGrowthSceneManager: Cannot capture drawing - missing camera or capture area.");
            return null;
        }

        Camera cam = drawingCanvas.mainCamera;

        // Get world corners of the drawing area
        Vector3[] corners = new Vector3[4];
        drawingCaptureArea.GetWorldCorners(corners);

        // Convert to screen space
        Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]); // bottom-left
        Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]); // top-right

        int width  = Mathf.RoundToInt(max.x - min.x);
        int height = Mathf.RoundToInt(max.y - min.y);

        if (width <= 0 || height <= 0)
        {
            Debug.LogWarning($"WildGrowthSceneManager: Capture size invalid: {width}x{height}");
            return null;
        }

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Rect rect = new Rect(min.x, min.y, width, height);
        tex.ReadPixels(rect, 0, 0);
        tex.Apply();

        Debug.Log($"WildGrowthSceneManager: Captured drawing texture {width}x{height}");
        return tex;
    }

    /// <summary>
    /// Applies the multiplier-based stat upgrade to the selected plant and saves the inventory.
    /// </summary>
    private void ApplyDrawingBasedWildGrowth()
    {
        // Multiply stats
        int newMaxHealth = Mathf.RoundToInt(selectedPlant.maxHealth * currentMultiplier);
        int newAttack    = Mathf.RoundToInt(selectedPlant.attack    * currentMultiplier);
        int newDefense   = Mathf.RoundToInt(selectedPlant.defense   * currentMultiplier);

        selectedPlant.maxHealth     = newMaxHealth;
        selectedPlant.attack        = newAttack;
        selectedPlant.defense       = newDefense;
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

    /// <summary>
    /// Writes the captured texture into DrawnUnitData and the inventory entry (base64).
    /// </summary>
    private void ApplyNewDrawingTexture(Texture2D tex)
    {
        if (tex == null || selectedPlant == null)
            return;

        // 1) Update DrawnUnitData for current run
        if (DrawnUnitData.Instance != null)
        {
            DrawnUnitData.Instance.drawingTexture = tex;
        }

        // 2) Update inventory entry (sprite for cards / selector)
        try
        {
            byte[] png = tex.EncodeToPNG();
            string base64 = Convert.ToBase64String(png);
            selectedPlant.drawingTextureBase64 = base64;
        }
        catch (Exception e)
        {
            Debug.LogError($"WildGrowthSceneManager: Failed to encode drawing texture: {e.Message}");
        }

        // 3) Save inventory again (to store updated drawingTextureBase64)
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.SaveInventory();
            Debug.Log($"WildGrowthSceneManager: Saved upgraded drawing for plant {selectedPlant.plantName}.");
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

        // Clear encounter data since we're returning from a completed battle
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
