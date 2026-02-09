using UnityEngine;
using TMPro; // Required for TextMeshPro
using UnityEngine.UI; // Required for Slider components

public class SliderValueDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI valueText; // Drag your text object here
    
    [Header("Settings")]
    [SerializeField] private string suffix = "%"; // Symbol to show after number

    // This function will be called by the Slider
    public void UpdateText(float value)
    {
        if (valueText != null)
        {
            // Converts 0.0-1.0 to 0-100
            int percentage = Mathf.RoundToInt(value * 100);
            
            // Updates the text (e.g., "75%")
            valueText.text = percentage.ToString() + suffix;
        }
    }
}