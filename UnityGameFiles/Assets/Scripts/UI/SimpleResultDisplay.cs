using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple on-screen text display for plant analysis results
/// Shows in the DrawingPanel itself so it's always visible
/// </summary>
public class SimpleResultDisplay : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public float displayDuration = 3f;

    private void Start()
    {
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Show analysis result as simple text
    /// </summary>
    public void ShowResult(PlantAnalyzer.PlantAnalysisResult result, Color dominantColor)
    {
        if (result == null || resultText == null)
        {
            Debug.LogWarning("Cannot show result - missing references");
            return;
        }

        // Build result string
        string colorName = GetColorName(dominantColor);
        string emoji = GetEmoji(result.detectedType);

        string resultString = $"<size=36><b>{emoji} {result.detectedType} {emoji}</b></size>\n\n";
        resultString += $"<color=yellow>Element:</color> <b>{result.elementType}</b>\n";
        resultString += $"<color=yellow>Confidence:</color> {GetStars(result.confidence)} <b>{result.confidence:P0}</b>\n";
        resultString += $"<color=yellow>Dominant Color:</color> {colorName}\n\n";
        resultString += $"<color=cyan><b>Available Moves:</b></color>\n";

        // Get moves
        MoveData[] moves = MoveData.GetMovesForPlant(result.detectedType);
        foreach (var move in moves)
        {
            resultString += $"• {move.moveName} (Power: {move.basePower})\n";
        }

        resultText.text = resultString;
        resultText.gameObject.SetActive(true);

        Debug.Log($"[RESULT DISPLAY] Showing: {result.detectedType} ({result.elementType})");
    }

    /// <summary>
    /// Hide the result text
    /// </summary>
    public void HideResult()
    {
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
    }

    private string GetEmoji(PlantAnalyzer.PlantType type)
    {
        switch (type)
        {
            case PlantAnalyzer.PlantType.Sunflower: return "🌻";
            case PlantAnalyzer.PlantType.Cactus: return "🌵";
            case PlantAnalyzer.PlantType.WaterLily: return "🪷";
            default: return "❓";
        }
    }

    private string GetColorName(Color color)
    {
        if (color.r > color.g && color.r > color.b)
            return "<color=red><b>RED</b></color>";
        else if (color.g > color.r && color.g > color.b)
            return "<color=green><b>GREEN</b></color>";
        else if (color.b > color.r && color.b > color.g)
            return "<color=blue><b>BLUE</b></color>";
        else
            return "Mixed";
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
}
