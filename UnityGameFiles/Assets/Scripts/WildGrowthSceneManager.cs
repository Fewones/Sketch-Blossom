using System; // for Convert.ToBase64String
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using SketchBlossom.Progression; // for PlayerInventory & PlantInventoryEntry
using SketchBlossom.Model;
using System.Threading.Tasks;

/// <summary>
/// Manages the Wild Growth reward scene:
/// - Shows the plant that just fought (from PlayerInventory)
/// - Player draws exactly one stroke on a canvas to "grow" the plant
/// - Geometric drawing quality (length + coverage) defines a base multiplier (1.3x–1.8x)
/// - Stroke color slightly biases which stat gets more growth (HP / ATK / DEF)
/// - Shows a live preview of the resulting stats
/// - On confirm:
///     * applies the stat changes to the selected PlantInventoryEntry
///     * captures the merged drawing (old art + new stroke)
///     * saves the new drawing both in DrawnUnitData and in the inventory entry
///     * returns to WorldMapScene
/// </summary>
public class WildGrowthSceneManager : MonoBehaviour
{
    [Header("Plant Art")]
    [SerializeField] private Image plantArtImage;
    // This Image shows the *existing* plant art loaded from the inventory entry
    // (selectedPlant.drawingTextureBase64). The new stroke is drawn on top in the canvas.

    [Header("Existing UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button continueButton;   // Fallback "Confirm" button if confirmButton is not wired
    [SerializeField] private GameObject contentPanel; // Not used in logic but kept for layout/visibility toggling if needed

    [Header("Drawing & Controls")]
    [SerializeField] private SimpleDrawingCanvas drawingCanvas; // Handles line drawing & stats
    [SerializeField] private Button clearButton;
    [SerializeField] private Button confirmButton;    // Preferred confirm button; if null, we fall back to continueButton

    [Header("Preview UI")]
    [SerializeField] private TextMeshProUGUI qualityText;       // Shows base quality + color focus
    [SerializeField] private TextMeshProUGUI hpPreviewText;     // Shows HP before/after
    [SerializeField] private TextMeshProUGUI attackPreviewText; // Shows ATK before/after
    [SerializeField] private TextMeshProUGUI defensePreviewText;// Shows DEF before/after

    [Header("Capture Settings")]
    [Tooltip("RectTransform that defines the drawing area we capture (usually the DrawingArea under WG_DrawingPanel).")]
    [SerializeField] private RectTransform drawingCaptureArea;
    // This RectTransform should fully contain: plantArtImage + stroke container.
    // CaptureDrawingToTexture() uses it to produce the final merged card art.

    [Header("Transition Settings")]
    [SerializeField] private bool useAutoTransition = false;
    [SerializeField] private float autoTransitionDelay = 3f;

    [Header("Multiplier Settings")]
    [SerializeField] private float minMultiplier = 1.3f; // Minimum stat growth even for a weak drawing
    [SerializeField] private float maxMultiplier = 1.8f; // Maximum stat growth for a very strong drawing

    [Tooltip("Minimum required strokes before Confirm becomes interactable. For Wild Growth we set this to 1 and enforce maxStrokes = 1 on the canvas.")]
    [SerializeField] private int minRequiredStrokes = 1;

    [Header("Future: CLIP / TinyCLIP Integration")]
    [Tooltip("If true, Confirm will later use TinyCLIP-based analysis instead of local geometric rules.")]
    [SerializeField] private bool useClip;

    private DrawingCaptureHandler captureHandler;
    private ModelManager MM = new ModelManager();

    public class PredictionResponse {
            public string label;
            public float score;
        }

    // Runtime state
    private PlantInventoryEntry selectedPlant; // The plant currently being upgraded (selected in PlayerInventory)
    private float currentMultiplier;           // Base geometric multiplier (before color bias)
    private bool hasAnyDrawing = false;        // True once the player has drawn at least one stroke
    private bool isConfirming = false;         // Guard flag to prevent double-confirm

    private void Start()
    {
        // 1) Determine which plant we are modifying
        LoadSelectedPlantFromInventory();

        Debug.Log($"[WG] DrawnUnitData exists? {DrawnUnitData.Instance != null}");
        if (DrawnUnitData.Instance != null)
        {
            Debug.Log($"[WG] DUD inventoryPlantId = '{DrawnUnitData.Instance.inventoryPlantId}'");
            Debug.Log($"[WG] DUD texture null? {DrawnUnitData.Instance.drawingTexture == null}");
        }

        Debug.Log($"[WG] selectedPlant null? {selectedPlant == null}");
        if (selectedPlant != null)
        {
            Debug.Log($"[WG] selectedPlant id={selectedPlant.plantId}, base64Len={(selectedPlant.drawingTextureBase64?.Length ?? 0)}");
        }

        // 2) Wire UI buttons and initial text
        SetupUI();

        // 3) Load the plant's existing art from inventory into the background image
        LoadPlantArtForWildGrowth();

        // 4) Initialize multiplier and preview (before any drawing)
        currentMultiplier = minMultiplier;
        UpdatePreviewTexts();

        // Optional: automatic transition back to world map (usually unused)
        if (useAutoTransition)
        {
            StartCoroutine(AutoTransitionToWorldMap());
        }

        // Auto-find or create capture handler
        if (captureHandler == null)
            {
                captureHandler = FindFirstObjectByType<DrawingCaptureHandler>();
                if (captureHandler == null)
                {
                    GameObject captureObj = new GameObject("DrawingCaptureHandler");
                    captureHandler = captureObj.AddComponent<DrawingCaptureHandler>();
                    Debug.Log("DrawingSceneManager: Created DrawingCaptureHandler");
                }
            }
    }

    /// <summary>
    /// Loads the selected plant's stored drawing sprite from PlayerInventory
    /// and sets it on plantArtImage. This is the base art that the player will draw on top of.
    /// </summary>
    private void LoadPlantArtForWildGrowth()
    {
        if (plantArtImage == null)
            return;

        if (selectedPlant == null)
        {
            Debug.LogWarning("WildGrowthSceneManager: No selected plant to load art for.");
            return;
        }

        if (string.IsNullOrEmpty(selectedPlant.drawingTextureBase64))
        {
            Debug.Log("WildGrowthSceneManager: Plant has no stored drawingTextureBase64 yet.");
            plantArtImage.sprite = null; // Could assign a fallback sprite here if desired
            return;
        }

        try
        {
            byte[] pngBytes = Convert.FromBase64String(selectedPlant.drawingTextureBase64);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(pngBytes))
            {
                Debug.LogWarning("WildGrowthSceneManager: Failed to LoadImage from plant base64.");
                return;
            }

            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );

            plantArtImage.sprite = sprite;
        }
        catch (Exception e)
        {
            Debug.LogError($"WildGrowthSceneManager: Error decoding plant art base64: {e.Message}");
        }
    }

    private void Update()
    {
        if (drawingCanvas == null)
            return;

        // In non-CLIP mode, we compute geometric quality every frame for live preview.
        if (!useClip)
        {
            UpdateQualityAndPreview();
        }
        else
        {
            // In future CLIP mode, geometric scoring might be replaced.
            // For now we only track "has a stroke?" and show a neutral preview.
            UpdateBasicPreviewWithoutQuality();
        }
    }

    /// <summary>
    /// Sets up button callbacks and initial UI text.
    /// The actual logic lives in OnClearClicked / OnConfirmClicked etc.
    /// </summary> 
    private void SetupUI()
    {
        if (messageText != null)
        {
            messageText.text = "Draw to help your plant grow stronger!";
        }

        // Clear button: remove any previous listeners, then attach our handler
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
            confirmButton = buttonToUse;        // Ensure confirmButton reference is set
            confirmButton.interactable = false; // Locked until at least one stroke exists
        }

        Debug.Log("WildGrowthScene: Setup complete");
    }

    /// <summary>
    /// Reads the currently selected plant from PlayerInventory.
    /// The PostBattle scene should have already selected the correct plant based on DrawnUnitData.inventoryPlantId.
    /// </summary>
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


    

    /// <summary>
    /// Main non-CLIP path:
    /// - Queries the SimpleDrawingCanvas for DrawingStats
    /// - Computes a geometric base multiplier
    /// - Enables/disables Confirm
    /// - Updates the UI preview with stat changes
    /// </summary>
    private void UpdateQualityAndPreview()
    {
        if (selectedPlant == null)
            return;

        DrawingStats stats = drawingCanvas.GetDrawingStats();
        hasAnyDrawing = stats.strokeCount > 0;

        // Confirm is only allowed once at least minRequiredStrokes are drawn.
        if (confirmButton != null)
        {
            bool shouldEnable = stats.strokeCount >= minRequiredStrokes;
            confirmButton.interactable = shouldEnable;
        }
        else
        {
            Debug.LogWarning("[WG] confirmButton is NULL in UpdateQualityAndPreview");
        }

        // Compute geometric multiplier based on stroke length + coverage
        currentMultiplier = ComputeMultiplier(stats);

        // Now update the preview labels (HP / ATK / DEF)
        UpdatePreviewTexts();
    }


    /// <summary>
    /// CLIP mode placeholder:
    /// Used when useClip == true: we still require at least one stroke, but
    /// we do not compute a local geometric quality. Instead we show a neutral preview
    /// and tell the player that their drawing will be analyzed on confirm.
    /// </summary>
    private void UpdateBasicPreviewWithoutQuality()
    {
        if (selectedPlant == null ||
            qualityText == null || hpPreviewText == null ||
            attackPreviewText == null || defensePreviewText == null)
            return;

        DrawingStats stats = drawingCanvas.GetDrawingStats();
        hasAnyDrawing = stats.strokeCount > 0;

        if (confirmButton != null)
        {
            // Even in CLIP mode we still want at least one stroke before confirm.
            confirmButton.interactable = stats.strokeCount >= minRequiredStrokes;
        }

        int baseHP      = selectedPlant.maxHealth;
        int baseAttack  = selectedPlant.attack;
        int baseDefense = selectedPlant.defense;

        qualityText.text = hasAnyDrawing
            ? "Your drawing will be analyzed when you confirm."
            : "Draw to help your plant grow stronger!";

        hpPreviewText.text      = $"HP:  {baseHP}";
        attackPreviewText.text  = $"ATK: {baseAttack}";
        defensePreviewText.text = $"DEF: {baseDefense}";
    }

    /// <summary>
    /// Geometric-only scoring for Wild Growth.
    /// Uses stroke length and coverage of the bounding box inside the drawing area.
    /// Returns a base multiplier between minMultiplier and maxMultiplier.
    ///
    /// This base multiplier is then turned into per-stat multipliers by GetPerStatMultipliers()
    /// to account for color bias (HP/ATK/DEF focus).
    /// </summary>
    private float ComputeMultiplier(DrawingStats stats)
    {
        // No stroke -> fallback to minimum multiplier (should not happen if Confirm is locked correctly)
        if (stats.strokeCount == 0)
            return minMultiplier;

        // --- 1) Length score ----------------------------------------------
        // We define a "useful" range for totalLength:
        //   - below 100: basically a dot -> very low score
        //   - 100–800:   reasonable to very long strokes
        //   - above 800: treated as max (no further reward)
        float clampedLength = Mathf.Clamp(stats.totalLength, 0f, 800f);
        float lengthNorm = Mathf.InverseLerp(100f, 800f, clampedLength);
        lengthNorm = Mathf.Clamp01(lengthNorm);

        // --- 2) Coverage score --------------------------------------------
        // coverage = how large the stroke's bounding box is relative to the canvas
        //   - below 3%: tiny scribble -> low score
        //   - 3–25%:    reasonable to large additions
        //   - above 25%: treated as max
        float coverageNorm = 0f;
        if (stats.canvasArea > 0f)
        {
            float rawCoverage = stats.boundingBoxArea / stats.canvasArea; // 0..1
            float clampedCoverage = Mathf.Clamp(rawCoverage, 0f, 0.25f);
            coverageNorm = Mathf.InverseLerp(0.03f, 0.25f, clampedCoverage);
            coverageNorm = Mathf.Clamp01(coverageNorm);
        }

        // --- 3) Combine into quality --------------------------------------
        // We weight length and coverage equally for now.
        float quality = 0.5f * lengthNorm + 0.5f * coverageNorm;
        quality = Mathf.Clamp01(quality);

        // --- 4) Map to multiplier range -----------------------------------
        float multiplier = Mathf.Lerp(minMultiplier, maxMultiplier, quality);
        return multiplier;
    }

    /// <summary>
    /// Computes per-stat multipliers based on the geometric base multiplier
    /// (currentMultiplier) and the dominant stroke color from the drawing canvas.
    ///
    /// Color meaning (single-stroke Wild Growth):
    /// - Red   -> ATK focus    (+15% ATK)
    /// - Green -> HP focus     (+15% HP)
    /// - Blue  -> DEF focus    (+15% DEF)
    ///
    /// All modifiers are relative to currentMultiplier. If no stroke or no canvas,
    /// all stats stay at currentMultiplier (balanced growth).
    /// </summary>
    private void GetPerStatMultipliers(out float hpMult, out float atkMult, out float defMult)
    {
        // Start with the same multiplier for all stats
        hpMult  = currentMultiplier;
        atkMult = currentMultiplier;
        defMult = currentMultiplier;

        // No canvas or no strokes? Neutral, nothing to adjust.
        if (drawingCanvas == null || drawingCanvas.GetStrokeCount() == 0)
            return;

        // Use the canvas' dominant color helper (already rounds to red/green/blue/white)
        Color dominant = drawingCanvas.GetDominantColor();

        // How strong the color bias is (e.g. +15% to one stat).
        // Adjust this constant for balancing if needed.
        const float colorBonus = 0.15f;

        if (dominant == Color.red)
        {
            atkMult = currentMultiplier * (1f + colorBonus);
        }
        else if (dominant == Color.green)
        {
            hpMult = currentMultiplier * (1f + colorBonus);
        }
        else if (dominant == Color.blue)
        {
            defMult = currentMultiplier * (1f + colorBonus);
        }
        // Any other color (white, mixed) -> neutral, no extra bias.
    }

    /// <summary>
    /// Updates all preview TextMeshPro labels (quality + HP/ATK/DEF values).
    /// Uses currentMultiplier (geometric quality) and color-based per-stat multipliers
    /// from GetPerStatMultipliers().
    /// </summary>
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

        // Compute color-aware multipliers for each stat
        GetPerStatMultipliers(out float hpMult, out float atkMult, out float defMult);

        int newHP      = Mathf.RoundToInt(baseHP      * hpMult);
        int newAttack  = Mathf.RoundToInt(baseAttack  * atkMult);
        int newDefense = Mathf.RoundToInt(baseDefense * defMult);

        float hpPercent  = (hpMult  - 1f) * 100f;
        float atkPercent = (atkMult - 1f) * 100f;
        float defPercent = (defMult - 1f) * 100f;

        if (!hasAnyDrawing)
        {
            qualityText.text = "Draw one stroke to grow your plant!";
        }
        else if (confirmButton != null && !confirmButton.interactable)
        {
            qualityText.text = "Keep drawing… (need at least one stroke).";
        }
        else
        {
            // Show base geometric quality + a simple "color focus" label
            float basePercent = (currentMultiplier - 1f) * 100f;
            string focus =
                hpMult  > atkMult && hpMult  > defMult ? "HP" :
                atkMult > hpMult  && atkMult > defMult ? "ATK" :
                defMult > hpMult  && defMult > atkMult ? "DEF" : "Balanced";

            qualityText.text =
                $"Quality: {basePercent:0}% (x{currentMultiplier:0.00})\n" +
                $"Color focus: {focus}";
        }

        hpPreviewText.text      = $"HP:  {baseHP} → {newHP} (+{hpPercent:0}%)";
        attackPreviewText.text  = $"ATK: {baseAttack} → {newAttack} (+{atkPercent:0}%)";
        defensePreviewText.text = $"DEF: {baseDefense} → {newDefense} (+{defPercent:0}%)";
    }

    /// <summary>
    /// Clears all strokes from the canvas and resets preview state.
    /// Does NOT reset the plant art; only the added stroke is removed.
    /// </summary>
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

    /// <summary>
    /// Handler for the Confirm button.
    /// Starts the coroutine that finalizes the drawing, applies upgrades,
    /// saves the new art and returns to the world map.
    /// </summary>
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
    /// Confirms the upgrade in multiple steps:
    /// 1) Ends any in-progress stroke on the canvas
    /// 2) Waits for end of frame to ensure rendering is up to date
    /// 3) Captures the drawing area (plant art + stroke) into a Texture2D
    /// 4) Applies stat upgrades (geometric + color-based)
    /// 5) Stores the new texture in DrawnUnitData and in the inventory entry
    /// 6) Transitions back to the WorldMapScene
    /// </summary>
    private IEnumerator ConfirmAndCaptureRoutine()
    {
        isConfirming = true;

        // 1) Make sure any in-progress stroke is finished
        if (drawingCanvas != null)
        {
            drawingCanvas.ForceEndStroke();
        }

        // 2) Wait for end of frame so drawing is fully rendered
        yield return new WaitForEndOfFrame();

        // 3) Capture the drawing area to a texture
        Texture2D captured = CaptureDrawingToTexture();

        // 4) Apply stat upgrade
        if (useClip)
        {
            // Placeholder: later, TinyCLIP-based analysis can be implemented here.
            var task = ApplyClipBasedWildGrowth();
            while (!task.IsCompleted)
                yield return null; // warten, bis async Task fertig ist
        }
        else
        {
            ApplyDrawingBasedWildGrowth();
        }

        // 5) Apply new art to DrawnUnitData + inventory
        ApplyNewDrawingTexture(captured);

        // 6) Transition back to world map
        TransitionToWorldMap();

        isConfirming = false;
    }

    /// <summary>
    /// Captures the drawing area (DrawingArea RectTransform) into a new Texture2D.
    /// The texture contains both the original plant art and the newly drawn stroke.
    /// The resulting texture is used as updated card art and inventory sprite.
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
    /// Applies the multiplier-based stat upgrade (geometric + color bias) to the selected plant.
    /// This is the non-CLIP path used in the current implementation.
    /// </summary>
    private void ApplyDrawingBasedWildGrowth()
    {
        if (selectedPlant == null)
            return;

        // Color-aware multipliers for each stat
        GetPerStatMultipliers(out float hpMult, out float atkMult, out float defMult);

        int newMaxHealth = Mathf.RoundToInt(selectedPlant.maxHealth * hpMult);
        int newAttack    = Mathf.RoundToInt(selectedPlant.attack    * atkMult);
        int newDefense   = Mathf.RoundToInt(selectedPlant.defense   * defMult);

        selectedPlant.maxHealth     = newMaxHealth;
        selectedPlant.attack        = newAttack;
        selectedPlant.defense       = newDefense;
        selectedPlant.currentHealth = newMaxHealth; // Heal to full after growth

        selectedPlant.wildGrowthCount++;
        selectedPlant.level++;

        Debug.Log(
            $"Wild Growth (drawing-based) applied to {selectedPlant.plantName}! " +
            $"Now level {selectedPlant.level} with " +
            $"HP:{selectedPlant.maxHealth} (x{hpMult:0.00}), " +
            $"ATK:{selectedPlant.attack} (x{atkMult:0.00}), " +
            $"DEF:{selectedPlant.defense} (x{defMult:0.00})"
        );
    }

    /// <summary>
    /// Placeholder for future TinyCLIP integration.
    /// Currently just falls back to the same multiplier-based upgrade as the non-CLIP path.
    /// </summary>
    private async Task ApplyClipBasedWildGrowth()
    {
        if (selectedPlant == null)
            return;
        
        string plant = selectedPlant.plantName;

        Texture2D drawingTexture = captureHandler.CaptureDrawing(
                drawingCanvas.allStrokes,
                drawingCanvas.mainCamera,
                drawingCanvas.drawingArea
            );

        Debug.Log("Saved new stroke as texture");
        string json = await MM.SendImage(drawingTexture, $"{plant} upgrade_labels");
        PredictionResponse best = JsonUtility.FromJson<PredictionResponse>(json);
        string attribute = best.label; // Which attribute will be enhanced?
        float score = best.score; // How much stronger will the attribute get?
        Debug.Log($"Result: {json}");

        int increase = Mathf.RoundToInt(3 * score * 10);

        switch (attribute)
        {
            case "power": 
                selectedPlant.attack += increase;
                break;
            case "defense": 
                selectedPlant.defense += increase;
                break;
            case "health": 
                selectedPlant.maxHealth += increase;
                break;
            default:
                selectedPlant.maxHealth  += 1;
                selectedPlant.attack     += 1;
                selectedPlant.defense    += 1;
                break;
        }

        selectedPlant.currentHealth = selectedPlant.maxHealth; // Heal to full after growth

        selectedPlant.wildGrowthCount++;
        selectedPlant.level++;

        Debug.Log(
            $"Wild Growth (drawing-based) applied to {selectedPlant.plantName}! " +
            $"Now level {selectedPlant.level} with " +
            $"HP:{selectedPlant.maxHealth}" +
            $"ATK:{selectedPlant.attack}" +
            $"DEF:{selectedPlant.defense}"
        );
    }

    /// <summary>
    /// Writes the captured texture into DrawnUnitData (for the current run)
    /// and into the inventory entry as Base64 (for persistence).
    /// Finally saves the inventory to disk via PlayerInventory.
    /// </summary>
    private void ApplyNewDrawingTexture(Texture2D tex)
    {
        if (tex == null || selectedPlant == null)
        {
            Debug.LogWarning("WildGrowthSceneManager: No texture or plant to apply upgraded drawing.");
            return;
        }

        // 1) Update DrawnUnitData for the current run (used by other scenes)
        if (DrawnUnitData.Instance != null)
        {
            DrawnUnitData.Instance.drawingTexture = tex;
        }

        // 2) Update inventory entry (Base64 sprite for card/selector usage)
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

        // 3) Persist updated stats + updated drawingTextureBase64
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.SaveInventory();
            Debug.Log($"WildGrowthSceneManager: Saved upgraded drawing for plant {selectedPlant.plantName}.");
        }
    }

    /// <summary>
    /// Optional automatic transition back to the world map after a delay.
    /// Usually disabled in Wild Growth (useAutoTransition = false).
    /// </summary>
    private IEnumerator AutoTransitionToWorldMap()
    {
        yield return new WaitForSeconds(autoTransitionDelay);
        TransitionToWorldMap();
    }

    /// <summary>
    /// Transitions back to the WorldMapScene and clears any EnemyEncounterData
    /// from the completed battle.
    /// </summary>
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

    /// <summary>
    /// Clean up button listeners to avoid leaks and double-calls when scenes are reloaded.
    /// </summary>
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
