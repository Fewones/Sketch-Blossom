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

    public void ShowResults(PlantAnalyzer.PlantAnalysisResult result, Color dominantColor, DrawnUnitData unitData, System.Action onContinue = null, System.Action onRedraw = null)
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
        Debug.Log("=== PLANT ANALYSIS RESULTS ===");
        Debug.Log($"🌱 Plant Type: {result.detectedType}");
        Debug.Log($"🔥 Element: {result.elementType}");
        Debug.Log($"⭐ Confidence: {result.confidence:P0}");
        if (unitData != null)
        {
            Debug.Log($"❤️ HP: {unitData.health}");
            Debug.Log($"⚔️ Attack: {unitData.attack}");
            Debug.Log($"🛡️ Defense: {unitData.defense}");
        }
        Debug.Log($"🎨 Dominant Color: {GetColorName(dominantColor)}");

        // Get moves once and use for both console and UI
        MoveData[] moves = MoveData.GetMovesForPlant(result.detectedType);
        Debug.Log("⚔️ Available Moves:");
        foreach (var move in moves)
        {
            Debug.Log($"  • {move.moveName} (Power: {move.basePower})");
        }
        Debug.Log("==============================");

        // Update title
        if (titleText != null)
        {
            titleText.text = "🌱 Plant Analysis Complete! 🌱";
        }

        // Update plant name with emoji
        if (plantNameText != null)
        {
            string emoji = GetPlantEmoji(result.detectedType);
            plantNameText.text = $"{emoji} {result.detectedType} {emoji}";
            plantNameText.color = GetPlantColor(result.detectedType);
        }

        // Update element
        if (elementText != null)
        {
            string elementEmoji = GetElementEmoji(result.elementType);
            elementText.text = $"{elementEmoji} {result.elementType} Type {elementEmoji}";
            elementText.color = GetPlantColor(result.detectedType);
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
            statsText.text = $"<b>Stats Generated:</b>\n" +
                            $"❤️ HP: {unitData.health}\n" +
                            $"⚔️ Attack: {unitData.attack}\n" +
                            $"🛡️ Defense: {unitData.defense}";
        }

        // Update color info
        if (colorInfoText != null)
        {
            string colorName = GetColorName(dominantColor);
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

        Debug.Log($"PlantResultPanel: Showing {result.detectedType} ({result.elementType})");
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
        if (panelOverlay != null)
        {
            panelOverlay.SetActive(false);
        }

        // Invoke redraw callback
        onRedrawCallback?.Invoke();
    }

    private string GetPlantEmoji(PlantAnalyzer.PlantType type)
    {
        switch (type)
        {
            case PlantAnalyzer.PlantType.Sunflower: return "🌻";
            case PlantAnalyzer.PlantType.Cactus: return "🌵";
            case PlantAnalyzer.PlantType.WaterLily: return "🪷";
            default: return "❓";
        }
    }

    private string GetElementEmoji(string element)
    {
        switch (element)
        {
            case "Fire": return "🔥";
            case "Grass": return "🌿";
            case "Water": return "💧";
            default: return "";
        }
    }

    private Color GetPlantColor(PlantAnalyzer.PlantType type)
    {
        switch (type)
        {
            case PlantAnalyzer.PlantType.Sunflower: return sunflowerColor;
            case PlantAnalyzer.PlantType.Cactus: return cactusColor;
            case PlantAnalyzer.PlantType.WaterLily: return waterLilyColor;
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
            return "<color=red>RED (Sunflower Boost)</color>";
        else if (color.g > color.r && color.g > color.b)
            return "<color=green>GREEN (Cactus Boost)</color>";
        else if (color.b > color.r && color.b > color.g)
            return "<color=blue>BLUE (Water Lily Boost)</color>";
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
