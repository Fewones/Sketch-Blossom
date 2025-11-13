using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Displays the plant analysis result in a prominent popup panel
/// Shows plant type, element, confidence, and color analysis
/// </summary>
public class PlantAnalysisResultPanel : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject resultPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI plantTypeText;
    public TextMeshProUGUI elementTypeText;
    public TextMeshProUGUI confidenceText;
    public TextMeshProUGUI colorInfoText;
    public TextMeshProUGUI movesText;
    public Button continueButton;

    [Header("Visual Settings")]
    public Color sunflowerColor = new Color(1f, 0.6f, 0f); // Orange
    public Color cactusColor = new Color(0.2f, 0.8f, 0.2f); // Green
    public Color waterLilyColor = new Color(0.2f, 0.6f, 1f); // Blue

    [Header("Display Settings")]
    public float displayDuration = 3f; // How long to show before auto-continuing
    public bool requireButtonPress = true; // If true, user must click Continue

    private System.Action onContinueCallback;

    private void Start()
    {
        // Hide panel on start
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    /// <summary>
    /// Show the plant analysis result with full details
    /// </summary>
    public void ShowResult(PlantAnalyzer.PlantAnalysisResult result, Color dominantColor, System.Action onContinue = null)
    {
        if (result == null)
        {
            Debug.LogWarning("Cannot show result - result is null");
            return;
        }

        onContinueCallback = onContinue;

        // Show panel
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        // Set title
        if (titleText != null)
        {
            titleText.text = "🌱 Plant Analysis Complete! 🌱";
        }

        // Set plant type with emoji
        if (plantTypeText != null)
        {
            string emoji = GetPlantEmoji(result.detectedType);
            plantTypeText.text = $"{emoji} {result.detectedType} {emoji}";
            plantTypeText.color = GetPlantColor(result.detectedType);
            plantTypeText.fontSize = 48;
        }

        // Set element type
        if (elementTypeText != null)
        {
            string elementEmoji = GetElementEmoji(result.elementType);
            elementTypeText.text = $"{elementEmoji} {result.elementType} Type {elementEmoji}";
            elementTypeText.color = GetPlantColor(result.detectedType);
        }

        // Set confidence
        if (confidenceText != null)
        {
            string confidenceLevel = GetConfidenceDescription(result.confidence);
            string stars = GetConfidenceStars(result.confidence);
            confidenceText.text = $"Detection Confidence: {stars}\n{confidenceLevel} ({result.confidence:P0})";
        }

        // Set color info
        if (colorInfoText != null)
        {
            string colorName = GetColorName(dominantColor);
            colorInfoText.text = $"Dominant Color: {colorName}\n" +
                                 $"Color Match: {GetColorMatchDescription(result.detectedType, dominantColor)}";
        }

        // Set available moves
        if (movesText != null)
        {
            string movesList = GetMovesDescription(result.detectedType);
            movesText.text = $"<b>Available Moves:</b>\n{movesList}";
        }

        Debug.Log($"=== SHOWING PLANT ANALYSIS RESULT ===");
        Debug.Log($"Plant Type: {result.detectedType}");
        Debug.Log($"Element: {result.elementType}");
        Debug.Log($"Confidence: {result.confidence:P0}");
        Debug.Log($"Dominant Color: {dominantColor}");

        // Handle auto-continue or button press
        if (!requireButtonPress)
        {
            StartCoroutine(AutoContinueAfterDelay());
        }
        else if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }
    }

    private void OnContinueClicked()
    {
        HidePanel();
        onContinueCallback?.Invoke();
    }

    private IEnumerator AutoContinueAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        OnContinueClicked();
    }

    private void HidePanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    // Helper methods for display

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

    private string GetConfidenceDescription(float confidence)
    {
        if (confidence >= 0.8f) return "Excellent Match!";
        if (confidence >= 0.6f) return "Good Match";
        if (confidence >= 0.4f) return "Fair Match";
        return "Uncertain";
    }

    private string GetConfidenceStars(float confidence)
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
            return "<color=red>Red</color>";
        else if (color.g > color.r && color.g > color.b)
            return "<color=green>Green</color>";
        else if (color.b > color.r && color.b > color.g)
            return "<color=blue>Blue</color>";
        else
            return "Mixed";
    }

    private string GetColorMatchDescription(PlantAnalyzer.PlantType type, Color color)
    {
        bool isMatch = false;

        if (type == PlantAnalyzer.PlantType.Sunflower && color.r > color.g && color.r > color.b)
            isMatch = true;
        else if (type == PlantAnalyzer.PlantType.Cactus && color.g > color.r && color.g > color.b)
            isMatch = true;
        else if (type == PlantAnalyzer.PlantType.WaterLily && color.b > color.r && color.b > color.g)
            isMatch = true;

        return isMatch ? "✓ Perfect!" : "○ Partial";
    }

    private string GetMovesDescription(PlantAnalyzer.PlantType type)
    {
        MoveData[] moves = MoveData.GetMovesForPlant(type);
        string result = "";

        foreach (var move in moves)
        {
            string powerIcon = move.basePower >= 25 ? "⚡⚡⚡" : move.basePower >= 20 ? "⚡⚡" : "⚡";
            result += $"• {move.moveName} {powerIcon}\n";
        }

        return result;
    }

    /// <summary>
    /// Static helper to show result from anywhere
    /// </summary>
    public static void ShowAnalysisResult(PlantAnalyzer.PlantAnalysisResult result, Color dominantColor, System.Action onContinue = null)
    {
        PlantAnalysisResultPanel panel = FindObjectOfType<PlantAnalysisResultPanel>();
        if (panel != null)
        {
            panel.ShowResult(result, dominantColor, onContinue);
        }
        else
        {
            Debug.LogWarning("PlantAnalysisResultPanel not found in scene!");
            onContinue?.Invoke(); // Continue anyway
        }
    }
}
