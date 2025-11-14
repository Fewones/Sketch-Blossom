using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates and displays a proper popup panel for plant analysis results
/// Similar to DrawingPanel but for showing results
/// </summary>
public class PlantResultPanel : MonoBehaviour
{
    [Header("Panel Objects")]
    public GameObject panelOverlay;
    public GameObject panelWindow;

    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI plantNameText;
    public TextMeshProUGUI elementText;
    public TextMeshProUGUI confidenceText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI colorInfoText;
    public Button continueButton;
    public Button redrawButton;

    [Header("Colors")]
    public Color sunflowerColor = new Color(1f, 0.6f, 0f);
    public Color cactusColor = new Color(0.2f, 0.8f, 0.2f);
    public Color waterLilyColor = new Color(0.3f, 0.6f, 1f);

    private System.Action onContinueCallback;
    private System.Action onRedrawCallback;
    private bool hasBeenInitialized = false;

    private void Awake()
    {
        // Use Awake instead of Start - runs before any SetActive calls
        Debug.Log("PlantResultPanel.Awake() called");

        // Setup button listeners in Awake (won't interfere with ShowResults)
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinue);
            Debug.Log("PlantResultPanel: Continue button listener added");
        }

        if (redrawButton != null)
        {
            redrawButton.onClick.AddListener(OnRedraw);
            Debug.Log("PlantResultPanel: Redraw button listener added");
        }

        hasBeenInitialized = true;
    }

    private void OnEnable()
    {
        // OnEnable runs every time the GameObject is activated
        // Only hide panels if we're being enabled but NOT during ShowResults
        if (hasBeenInitialized && !IsShowingResults())
        {
            Debug.Log("PlantResultPanel.OnEnable() - Hiding panels (not showing results)");
            HidePanelInternal();
        }
    }

    private bool IsShowingResults()
    {
        // If either panel is active, we're showing results
        return (panelOverlay != null && panelOverlay.activeSelf) ||
               (panelWindow != null && panelWindow.activeSelf);
    }

    private void HidePanelInternal()
    {
        if (panelOverlay != null)
        {
            panelOverlay.SetActive(false);
        }

        if (panelWindow != null)
        {
            panelWindow.SetActive(false);
        }
    }

    public void ShowResults(PlantRecognitionSystem.RecognitionResult result, DrawnUnitData unitData, System.Action onContinue = null, System.Action onRedraw = null)
    {
        Debug.Log("========== PLANT RESULT PANEL: SHOW RESULTS ==========");

        if (result == null)
        {
            Debug.LogError("PlantResultPanel: Cannot show null result");
            return;
        }

        if (panelOverlay == null)
        {
            Debug.LogError("PlantResultPanel: panelOverlay is NULL! Cannot show results.");
            return;
        }

        onContinueCallback = onContinue;
        onRedrawCallback = onRedraw;

        // Show the panel overlay (background)
        panelOverlay.SetActive(true);
        Debug.Log("PlantResultPanel: Panel overlay activated!");

        // Show the panel window (content)
        if (panelWindow != null)
        {
            panelWindow.SetActive(true);
            Debug.Log("PlantResultPanel: Panel window activated!");
        }
        else
        {
            Debug.LogWarning("PlantResultPanel: panelWindow is NULL - content may not be visible!");
        }

        // Check if plant is valid
        if (!result.isValidPlant)
        {
            // Show invalid plant error message
            Debug.LogWarning("=== INVALID PLANT - DOES NOT MEET REQUIREMENTS ===");
            Debug.LogWarning("Drawing does not meet the required pattern criteria!");
            Debug.LogWarning("User must redraw to continue.");
            Debug.LogWarning("==============================");

            // Update title
            if (titleText != null)
            {
                titleText.text = "Invalid Drawing!";
            }

            // Show error message
            if (plantNameText != null)
            {
                plantNameText.text = "<size=48><b><color=red>❌ Not a Valid Plant!</color></b></size>";
                plantNameText.color = Color.red;
            }

            // Show requirements message
            if (elementText != null)
            {
                elementText.text = "<size=20>Drawing doesn't meet requirements</size>";
                elementText.color = Color.red;
            }

            // Show redraw instruction
            if (confidenceText != null)
            {
                confidenceText.text = "<size=18>Please try again!</size>";
            }

            // Show requirements for all plant types
            if (statsText != null)
            {
                statsText.text = "<b><size=20>Requirements</size></b>\n\n" +
                    "<size=14><b>🔥 Fire Plants (Red):</b>\n" +
                    "• Sunflower: 4+ red circles + 1 green line\n" +
                    "• Fire Rose: 5+ overlapping red strokes + 1 green line\n" +
                    "• Flame Tulip: 3+ long vertical red strokes\n\n" +
                    "<b>🌿 Grass Plants (Green):</b>\n" +
                    "• Cactus: 2+ long vertical green strokes\n" +
                    "• Vine Flower: 3+ curved green strokes\n" +
                    "• Grass Sprout: 5+ short green strokes\n\n" +
                    "<b>💧 Water Plants (Blue):</b>\n" +
                    "• Water Lily: 3+ horizontal blue strokes\n" +
                    "• Coral Bloom: 4+ overlapping blue strokes\n" +
                    "• Bubble Flower: 3+ blue circles</size>";
            }

            // Hide moves
            if (movesText != null)
            {
                movesText.text = "";
            }

            // Hide color info
            if (colorInfoText != null)
            {
                colorInfoText.text = "";
            }

            // DISABLE Continue button (can't continue with invalid plant)
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
                Debug.Log("Continue button DISABLED - plant is invalid");
            }

            // ENABLE Redraw button
            if (redrawButton != null)
            {
                redrawButton.gameObject.SetActive(true);
            }

            Debug.Log("PlantResultPanel: Showing INVALID PLANT error");
            return;
        }

        // VALID PLANT - Show normal results
        Debug.Log("=== PLANT RECOGNITION RESULTS ===");
        Debug.Log($"🌱 Plant: {result.plantData.displayName} ({result.plantType})");
        Debug.Log($"🔥 Element: {result.element}");
        Debug.Log($"⭐ Confidence: {result.confidence:P0}");
        if (unitData != null)
        {
            Debug.Log($"❤️ HP: {unitData.health}");
            Debug.Log($"⚔️ Attack: {unitData.attack}");
            Debug.Log($"🛡️ Defense: {unitData.defense}");
        }
        Debug.Log($"🎨 Dominant Color: {GetColorName(result.dominantColor)}");

        // Get moves once and use for both console and UI
        MoveData[] moves = MoveData.GetMovesForPlant(result.plantType);
        Debug.Log("⚔️ Available Moves:");
        foreach (var move in moves)
        {
            Debug.Log($"  • {move.moveName} (Power: {move.basePower})");
        }
        Debug.Log("==============================");

        // Update title - Clean and simple
        if (titleText != null)
        {
            titleText.text = "Plant Recognized!";
        }

        // Update plant name - Large and prominent
        if (plantNameText != null)
        {
            string emoji = GetPlantEmoji(result.element);
            plantNameText.text = $"<size=48><b>{result.plantData.displayName}</b></size>";
            plantNameText.color = GetElementColor(result.element);
        }

        // Update element - Clean type display
        if (elementText != null)
        {
            string elementEmoji = GetElementEmoji(result.element);
            elementText.text = $"<size=24>{elementEmoji} {result.element} Type</size>";
            elementText.color = GetElementColor(result.element);
        }

        // Update confidence - Simplified
        if (confidenceText != null)
        {
            string stars = GetStars(result.confidence);
            confidenceText.text = $"<size=18>{stars}</size>";
        }

        // Update stats - Clean table format
        if (statsText != null && unitData != null)
        {
            statsText.text =
                $"<b><size=20>Stats</size></b>\n\n" +
                $"<size=18>❤️  HP:  {unitData.health,3}</size>\n" +
                $"<size=18>⚔️  ATK: {unitData.attack,3}</size>\n" +
                $"<size=18>🛡️  DEF: {unitData.defense,3}</size>";
        }

        // Update color info - Minimized (element already shows this)
        if (colorInfoText != null)
        {
            colorInfoText.text = ""; // Hide - redundant with element type
        }

        // Update moves - Clean list format
        if (movesText != null)
        {
            string movesStr = "<b><size=20>Moves</size></b>\n\n";
            foreach (var move in moves)
            {
                string powerBars = GetPowerBars(move.basePower);
                string powerText = move.basePower > 0 ? $" {powerBars}" : " 🛡️";
                movesStr += $"<size=18>• {move.moveName}{powerText}</size>\n";
            }
            movesText.text = movesStr;
        }

        // ENABLE Continue button (plant is valid)
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }

        // ENABLE Redraw button
        if (redrawButton != null)
        {
            redrawButton.gameObject.SetActive(true);
        }

        Debug.Log($"PlantResultPanel: Showing {result.plantData.displayName} ({result.element})");
    }

    private void OnContinue()
    {
        Debug.Log("PlantResultPanel: Continue to Battle button clicked");

        // Hide panel
        if (panelOverlay != null)
        {
            panelOverlay.SetActive(false);
        }

        // Invoke callback
        onContinueCallback?.Invoke();
    }

    private void OnRedraw()
    {
        Debug.Log("PlantResultPanel: Redraw button clicked");

        // Hide panel
        HidePanel();

        // Invoke redraw callback
        onRedrawCallback?.Invoke();
    }

    /// <summary>
    /// Hide the result panel
    /// </summary>
    public void HidePanel()
    {
        if (panelOverlay != null)
        {
            panelOverlay.SetActive(false);
            Debug.Log("PlantResultPanel: Panel overlay hidden");
        }

        if (panelWindow != null)
        {
            panelWindow.SetActive(false);
            Debug.Log("PlantResultPanel: Panel window hidden");
        }
    }

    private string GetPlantEmoji(PlantRecognitionSystem.ElementType element)
    {
        switch (element)
        {
            case PlantRecognitionSystem.ElementType.Fire: return "🔥";
            case PlantRecognitionSystem.ElementType.Grass: return "🌿";
            case PlantRecognitionSystem.ElementType.Water: return "💧";
            default: return "❓";
        }
    }

    private string GetElementEmoji(PlantRecognitionSystem.ElementType element)
    {
        switch (element)
        {
            case PlantRecognitionSystem.ElementType.Fire: return "🔥";
            case PlantRecognitionSystem.ElementType.Grass: return "🌿";
            case PlantRecognitionSystem.ElementType.Water: return "💧";
            default: return "";
        }
    }

    private Color GetElementColor(PlantRecognitionSystem.ElementType element)
    {
        switch (element)
        {
            case PlantRecognitionSystem.ElementType.Fire: return sunflowerColor;
            case PlantRecognitionSystem.ElementType.Grass: return cactusColor;
            case PlantRecognitionSystem.ElementType.Water: return waterLilyColor;
            default: return Color.white;
        }
    }

    private string GetStars(float confidence)
    {
        int stars = Mathf.RoundToInt(confidence * 5);
        string result = "";
        for (int i = 0; i < 5; i++)
        {
            result += i < stars ? "★" : "☆";
        }
        return result;
    }

    private string GetColorName(Color color)
    {
        if (color.r > color.g && color.r > color.b)
            return "<color=red>🔥 RED (Fire Plant)</color>";
        else if (color.g > color.r && color.g > color.b)
            return "<color=green>🌿 GREEN (Grass Plant)</color>";
        else if (color.b > color.r && color.b > color.g)
            return "<color=blue>💧 BLUE (Water Plant)</color>";
        else
            return "Mixed Colors";
    }

    private string GetPowerBars(int power)
    {
        if (power >= 25) return "⚡⚡⚡";
        if (power >= 20) return "⚡⚡";
        return "⚡";
    }
}
