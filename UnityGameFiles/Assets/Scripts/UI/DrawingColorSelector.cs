using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages color selection UI for the drawing system
/// Provides Red, Green, and Blue color buttons for plant type influence
/// </summary>
public class DrawingColorSelector : MonoBehaviour
{
    [Header("UI References")]
    public Button redButton;
    public Button greenButton;
    public Button blueButton;

    [Header("Visual Feedback")]
    public Image redButtonImage;
    public Image greenButtonImage;
    public Image blueButtonImage;

    [Header("Selected State Colors")]
    public Color selectedTint = new Color(1f, 1f, 1f, 1f); // Bright
    public Color unselectedTint = new Color(0.6f, 0.6f, 0.6f, 0.8f); // Dimmed

    [Header("Optional Labels")]
    public TextMeshProUGUI redLabel;
    public TextMeshProUGUI greenLabel;
    public TextMeshProUGUI blueLabel;

    [Header("References")]
    public DrawingCanvas drawingCanvas;

    private void Start()
    {
        // Auto-find DrawingCanvas if not assigned
        if (drawingCanvas == null)
        {
            drawingCanvas = FindFirstObjectByType<DrawingCanvas>();
            if (drawingCanvas == null)
            {
                Debug.LogError("DrawingColorSelector: Could not find DrawingCanvas!");
            }
        }

        // Auto-find button images if not assigned
        if (redButtonImage == null && redButton != null)
            redButtonImage = redButton.GetComponent<Image>();
        if (greenButtonImage == null && greenButton != null)
            greenButtonImage = greenButton.GetComponent<Image>();
        if (blueButtonImage == null && blueButton != null)
            blueButtonImage = blueButton.GetComponent<Image>();

        // Setup button listeners
        if (redButton != null)
            redButton.onClick.AddListener(OnRedButtonClicked);
        if (greenButton != null)
            greenButton.onClick.AddListener(OnGreenButtonClicked);
        if (blueButton != null)
            blueButton.onClick.AddListener(OnBlueButtonClicked);

        // Set initial state (green is default)
        UpdateButtonVisuals("green");

        // Set up labels if they exist
        if (redLabel != null)
            redLabel.text = "Red\n(Sunflower)";
        if (greenLabel != null)
            greenLabel.text = "Green\n(Cactus)";
        if (blueLabel != null)
            blueLabel.text = "Blue\n(Water Lily)";
    }

    private void OnRedButtonClicked()
    {
        if (drawingCanvas != null)
        {
            drawingCanvas.SelectRedColor();
            UpdateButtonVisuals("red");
            Debug.Log("Color selector: Red selected (Sunflower)");
        }
    }

    private void OnGreenButtonClicked()
    {
        if (drawingCanvas != null)
        {
            drawingCanvas.SelectGreenColor();
            UpdateButtonVisuals("green");
            Debug.Log("Color selector: Green selected (Cactus)");
        }
    }

    private void OnBlueButtonClicked()
    {
        if (drawingCanvas != null)
        {
            drawingCanvas.SelectBlueColor();
            UpdateButtonVisuals("blue");
            Debug.Log("Color selector: Blue selected (Water Lily)");
        }
    }

    /// <summary>
    /// Update button visuals to show which is selected
    /// </summary>
    private void UpdateButtonVisuals(string selectedColor)
    {
        // Update red button
        if (redButtonImage != null)
        {
            redButtonImage.color = selectedColor == "red" ? selectedTint : unselectedTint;
        }

        // Update green button
        if (greenButtonImage != null)
        {
            greenButtonImage.color = selectedColor == "green" ? selectedTint : unselectedTint;
        }

        // Update blue button
        if (blueButtonImage != null)
        {
            blueButtonImage.color = selectedColor == "blue" ? selectedTint : unselectedTint;
        }
    }

    /// <summary>
    /// Public method to reset to default color (green)
    /// </summary>
    public void ResetToDefault()
    {
        OnGreenButtonClicked();
    }
}
