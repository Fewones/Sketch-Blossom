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

    [Header("Colors")]
    public Color sunflowerColor = new Color(1f, 0.6f, 0f);
    public Color cactusColor = new Color(0.2f, 0.8f, 0.2f);
    public Color waterLilyColor = new Color(0.3f, 0.6f, 1f);

    private System.Action onContinueCallback;

    private void Start()
    {
        // Hide panel initially
        if (panelOverlay != null)
        {
            panelOverlay.SetActive(false);
        }

        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinue);
        }
    }

    public void ShowResults(PlantAnalyzer.PlantAnalysisResult result, Color dominantColor, DrawnUnitData unitData, System.Action onContinue = null)
    {
        if (result == null)
        {
            Debug.LogError("PlantResultPanel: Cannot show null result");
            return;
        }

        onContinueCallback = onContinue;

        // Show the panel
        if (panelOverlay != null)
        {
            panelOverlay.SetActive(true);
        }

        Debug.Log("PlantResultPanel: Displaying results on screen");

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

        // Update moves
        if (movesText != null)
        {
            MoveData[] moves = MoveData.GetMovesForPlant(result.detectedType);
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
        // Hide panel
        if (panelOverlay != null)
        {
            panelOverlay.SetActive(false);
        }

        // Invoke callback
        onContinueCallback?.Invoke();
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
