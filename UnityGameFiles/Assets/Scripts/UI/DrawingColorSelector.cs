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
    public SimpleDrawingCanvas simpleDrawingCanvas;

    private void Start()
    {
        // Auto-find SimpleDrawingCanvas first (preferred)
        if (simpleDrawingCanvas == null)
        {
            simpleDrawingCanvas = FindFirstObjectByType<SimpleDrawingCanvas>();
            if (simpleDrawingCanvas != null)
            {
                Debug.Log("DrawingColorSelector: Using SimpleDrawingCanvas");
            }
        }

        // Fall back to DrawingCanvas if SimpleDrawingCanvas not found
        if (simpleDrawingCanvas == null && drawingCanvas == null)
        {
            drawingCanvas = FindFirstObjectByType<DrawingCanvas>();
            if (drawingCanvas != null)
            {
                Debug.Log("DrawingColorSelector: Using legacy DrawingCanvas");
            }
            else
            {
                Debug.LogError("DrawingColorSelector: Could not find any drawing canvas!");
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

        // Set initial color to green (not just visuals - actually set the color!)
        if (simpleDrawingCanvas != null)
        {
            simpleDrawingCanvas.SetColor(Color.green);
            Debug.Log("DrawingColorSelector: Set initial color to GREEN");
        }
        else if (drawingCanvas != null)
        {
            drawingCanvas.SelectGreenColor();
            Debug.Log("DrawingColorSelector: Set initial color to GREEN (legacy)");
        }

        // Update button visuals to match
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
        if (simpleDrawingCanvas != null)
        {
            simpleDrawingCanvas.SetColor(Color.red);
            UpdateButtonVisuals("red");
            Debug.Log("Color selector: Red selected (Sunflower)");
        }
        else if (drawingCanvas != null)
        {
            drawingCanvas.SelectRedColor();
            UpdateButtonVisuals("red");
            Debug.Log("Color selector: Red selected (Sunflower)");
        }
    }

    private void OnGreenButtonClicked()
    {
        if (simpleDrawingCanvas != null)
        {
            simpleDrawingCanvas.SetColor(Color.green);
            UpdateButtonVisuals("green");
            Debug.Log("Color selector: Green selected (Cactus)");
        }
        else if (drawingCanvas != null)
        {
            drawingCanvas.SelectGreenColor();
            UpdateButtonVisuals("green");
            Debug.Log("Color selector: Green selected (Cactus)");
        }
    }

    private void OnBlueButtonClicked()
    {
        if (simpleDrawingCanvas != null)
        {
            simpleDrawingCanvas.SetColor(Color.blue);
            UpdateButtonVisuals("blue");
            Debug.Log("Color selector: Blue selected (Water Lily)");
        }
        else if (drawingCanvas != null)
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
