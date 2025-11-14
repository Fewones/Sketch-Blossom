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
    /// Show analysis result as simple text (NEW SYSTEM)
    /// </summary>
    public void ShowResult(PlantRecognitionSystem.RecognitionResult result)
    {
        if (result == null || resultText == null)
        {
            Debug.LogWarning("Cannot show result - missing references");
            return;
        }

        // Build result string
        string colorName = GetColorName(result.dominantColor);
        string emoji = GetElementEmoji(result.element);

        string resultString = $"<size=36><b>{emoji} {result.plantData.displayName} {emoji}</b></size>\n\n";
        resultString += $"<color=yellow>Element:</color> <b>{result.element}</b>\n";
        resultString += $"<color=yellow>Confidence:</color> {GetStars(result.confidence)} <b>{result.confidence:P0}</b>\n";
        resultString += $"<color=yellow>Dominant Color:</color> {colorName}\n\n";
        resultString += $"<color=cyan><b>Available Moves:</b></color>\n";

        // Get moves
        MoveData[] moves = MoveData.GetMovesForPlant(result.plantType);
        foreach (var move in moves)
        {
            resultString += $"• {move.moveName} (Power: {move.basePower})\n";
        }

        resultText.text = resultString;
        resultText.gameObject.SetActive(true);

        Debug.Log($"[RESULT DISPLAY] Showing: {result.plantData.displayName} ({result.element})");
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
