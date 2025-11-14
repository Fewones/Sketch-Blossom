using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Displays visual feedback when a plant is detected
/// Shows the plant type, element, and confidence
/// </summary>
public class PlantDetectionFeedback : MonoBehaviour
{
    [Header("UI References")]
    public GameObject feedbackPanel;
    public TextMeshProUGUI plantNameText;
    public TextMeshProUGUI elementText;
    public TextMeshProUGUI confidenceText;

    [Header("Display Settings")]
    public float displayDuration = 3f;
    public Color fireColor = new Color(1f, 0.5f, 0f); // Orange
    public Color grassColor = new Color(0.2f, 0.8f, 0.2f); // Green
    public Color waterColor = new Color(0.2f, 0.5f, 1f); // Blue

    private void Start()
    {
        // Hide panel on start
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Display the plant detection result
    /// </summary>
    public void ShowDetectionResult(PlantRecognitionSystem.RecognitionResult result)
    {
        if (result == null || feedbackPanel == null)
        {
            Debug.LogWarning("Cannot show detection result - missing references");
            return;
        }

        // Check if the drawing is valid
        if (!result.isValidPlant)
        {
            // Show invalid plant message
            if (plantNameText != null)
            {
                plantNameText.text = "❌ Not a Valid Plant!";
            }

            if (elementText != null)
            {
                elementText.text = "Drawing doesn't meet requirements";
                elementText.color = Color.red;
            }

            if (confidenceText != null)
            {
                confidenceText.text = "Please try again!";
            }
        }
        else
        {
            // Show valid plant detection
            if (plantNameText != null)
            {
                plantNameText.text = GetPlantDisplayName(result.plantType);
            }

            if (elementText != null)
            {
                elementText.text = $"{result.element} Type";
                elementText.color = GetElementColor(result.element.ToString());
            }

            if (confidenceText != null)
            {
                string confidenceLevel = GetConfidenceLevel(result.confidence);
                confidenceText.text = $"Detection: {confidenceLevel}";
            }
        }

        // Show panel
        feedbackPanel.SetActive(true);

        // Auto-hide after duration
        StartCoroutine(HideAfterDelay());
    }

    /// <summary>
    /// Quick method to show result (can be called from DrawingManager)
    /// </summary>
    public static void ShowResult(PlantRecognitionSystem.RecognitionResult result)
    {
        PlantDetectionFeedback feedback = FindObjectOfType<PlantDetectionFeedback>();
        if (feedback != null)
        {
            feedback.ShowDetectionResult(result);
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
    }

    private string GetPlantDisplayName(PlantRecognitionSystem.PlantType type)
    {
        switch (type)
        {
            // Fire plants
            case PlantRecognitionSystem.PlantType.Sunflower:
                return "🔥 Sunflower";
            case PlantRecognitionSystem.PlantType.FireRose:
                return "🔥 Fire Rose";
            case PlantRecognitionSystem.PlantType.FlameTulip:
                return "🔥 Flame Tulip";

            // Grass plants
            case PlantRecognitionSystem.PlantType.Cactus:
                return "🌿 Cactus";
            case PlantRecognitionSystem.PlantType.VineFlower:
                return "🌿 Vine Flower";
            case PlantRecognitionSystem.PlantType.GrassSprout:
                return "🌿 Grass Sprout";

            // Water plants
            case PlantRecognitionSystem.PlantType.WaterLily:
                return "💧 Water Lily";
            case PlantRecognitionSystem.PlantType.CoralBloom:
                return "💧 Coral Bloom";
            case PlantRecognitionSystem.PlantType.BubbleFlower:
                return "💧 Bubble Flower";

            default:
                return "❓ Unknown Plant";
        }
    }

    private Color GetElementColor(string element)
    {
        switch (element)
        {
            case "Fire": return fireColor;
            case "Grass": return grassColor;
            case "Water": return waterColor;
            default: return Color.gray;
        }
    }

    private string GetConfidenceLevel(float confidence)
    {
        if (confidence >= 0.8f) return "Excellent!";
        if (confidence >= 0.6f) return "Good";
        if (confidence >= 0.4f) return "Fair";
        return "Uncertain";
    }

    /// <summary>
    /// Manual hide (can be called from button)
    /// </summary>
    public void HideFeedback()
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
    }
}
