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
    /// Show the plant analysis result with full details (NEW SYSTEM)
    /// </summary>
    public void ShowResult(PlantRecognitionSystem.RecognitionResult result, System.Action onContinue = null)
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
            string emoji = GetElementEmoji(result.element);
            plantTypeText.text = $"{emoji} {result.plantData.displayName} {emoji}";
            plantTypeText.color = GetElementColor(result.element);
            plantTypeText.fontSize = 48;
        }

        // Set element type
        if (elementTypeText != null)
        {
            string elementEmoji = GetElementEmoji(result.element);
            elementTypeText.text = $"{elementEmoji} {result.element} Type {elementEmoji}";
            elementTypeText.color = GetElementColor(result.element);
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
            string colorName = GetColorName(result.dominantColor);
            colorInfoText.text = $"Dominant Color: {colorName}\n" +
                                 $"Color Match: {GetColorMatchDescription(result.element, result.dominantColor)}";
        }

        // Set available moves
        if (movesText != null)
        {
            string movesList = GetMovesDescription(result.plantType);
            movesText.text = $"<b>Available Moves:</b>\n{movesList}";
        }

        Debug.Log($"=== SHOWING PLANT ANALYSIS RESULT ===");
        Debug.Log($"Plant Type: {result.plantType}");
        Debug.Log($"Element: {result.element}");
        Debug.Log($"Confidence: {result.confidence:P0}");
        Debug.Log($"Dominant Color: {result.dominantColor}");

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

    private string GetElementEmoji(PlantRecognitionSystem.ElementType element)
    {
        switch (element)
        {
            case PlantRecognitionSystem.ElementType.Fire: return "🔥";
            case PlantRecognitionSystem.ElementType.Grass: return "🌿";
            case PlantRecognitionSystem.ElementType.Water: return "💧";
            default: return "❓";
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

    private string GetColorMatchDescription(PlantRecognitionSystem.ElementType element, Color color)
    {
        bool isMatch = false;

        if (element == PlantRecognitionSystem.ElementType.Fire && color.r > color.g && color.r > color.b)
            isMatch = true;
        else if (element == PlantRecognitionSystem.ElementType.Grass && color.g > color.r && color.g > color.b)
            isMatch = true;
        else if (element == PlantRecognitionSystem.ElementType.Water && color.b > color.r && color.b > color.g)
            isMatch = true;

        return isMatch ? "✓ Perfect!" : "○ Partial";
    }

    private string GetMovesDescription(PlantRecognitionSystem.PlantType type)
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
    /// Static helper to show result from anywhere (NEW SYSTEM)
    /// </summary>
    public static void ShowAnalysisResult(PlantRecognitionSystem.RecognitionResult result, System.Action onContinue = null)
    {
        PlantAnalysisResultPanel panel = FindObjectOfType<PlantAnalysisResultPanel>();
        if (panel != null)
        {
            panel.ShowResult(result, onContinue);
        }
        else
        {
            Debug.LogWarning("PlantAnalysisResultPanel not found in scene!");
            onContinue?.Invoke(); // Continue anyway
        }
    }
}
