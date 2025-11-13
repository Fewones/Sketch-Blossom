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

    private void Start()
    {
        Debug.Log("PlantResultPanel.Start() called");

        // Hide panel initially
        if (panelOverlay != null)
        {
            panelOverlay.SetActive(false);
            Debug.Log("PlantResultPanel: Panel hidden on start");
        }
        else
        {
            Debug.LogError("PlantResultPanel: panelOverlay is NULL!");
        }

        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinue);
            Debug.Log("PlantResultPanel: Continue button listener added");
        }
        else
        {
            Debug.LogError("PlantResultPanel: continueButton is NULL!");
        }

        // Setup redraw button
        if (redrawButton != null)
        {
            redrawButton.onClick.AddListener(OnRedraw);
            Debug.Log("PlantResultPanel: Redraw button listener added");
        }
        else
        {
            Debug.LogWarning("PlantResultPanel: redrawButton is NULL - button may not exist yet");
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

        // Show the panel
        panelOverlay.SetActive(true);
        Debug.Log("PlantResultPanel: Panel overlay activated!");

        // Log detailed results to console
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

        // Update title
        if (titleText != null)
        {
            titleText.text = "🌱 Plant Recognition Complete! 🌱";
        }

        // Update plant name with emoji
        if (plantNameText != null)
        {
            string emoji = GetPlantEmoji(result.element);
            plantNameText.text = $"{emoji} {result.plantData.displayName} {emoji}";
            plantNameText.color = GetElementColor(result.element);
        }

        // Update element
        if (elementText != null)
        {
            string elementEmoji = GetElementEmoji(result.element);
            elementText.text = $"{elementEmoji} {result.element} Type {elementEmoji}";
            elementText.color = GetElementColor(result.element);
        }

        // Update confidence
        if (confidenceText != null)
        {
            string stars = GetStars(result.confidence);
            confidenceText.text = $"Confidence: {stars}\n{result.confidence:P0}";
        }

        // Update stats
        if (statsText != null && unitData != null)
        {
            statsText.text = $"<b>Plant Stats:</b>\n" +
                            $"❤️ HP: {unitData.health}\n" +
                            $"⚔️ Attack: {unitData.attack}\n" +
                            $"🛡️ Defense: {unitData.defense}";
        }

        // Update color info
        if (colorInfoText != null)
        {
            string colorName = GetColorName(result.dominantColor);
            colorInfoText.text = $"<b>Drawing Color:</b>\n{colorName}";
        }

        // Update moves (reuse moves array declared above)
        if (movesText != null)
        {
            string movesStr = "<b>Available Moves:</b>\n";
            foreach (var move in moves)
            {
                string powerBars = GetPowerBars(move.basePower);
                movesStr += $"• {move.moveName} {powerBars}\n";
            }
            movesText.text = movesStr;
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
            Debug.Log("PlantResultPanel: Panel hidden");
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
