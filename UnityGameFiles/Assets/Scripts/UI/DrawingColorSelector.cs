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
    public SimpleDrawingCanvas simpleDrawingCanvas;

    [Header("Color Picker")]

    public float currentHue, currentSat, currentVal;

    [SerializeField] private RawImage hueImage, satValImage, outputImage;
    [SerializeField] private Slider hueSlider;

    [SerializeField] private TMP_InputField hexInputField;

    [SerializeField] private Texture2D hueTexture, svTexture, outputTexture;

    private void Start()
    {
        // Auto-find SimpleDrawingCanvas
        if (simpleDrawingCanvas == null)
        {
            simpleDrawingCanvas = FindFirstObjectByType<SimpleDrawingCanvas>();
            if (simpleDrawingCanvas != null)
            {
                Debug.Log("DrawingColorSelector: Found SimpleDrawingCanvas");
            }
            else
            {
                Debug.LogError("DrawingColorSelector: SimpleDrawingCanvas not found!");
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

        // Set initial color to green
        if (simpleDrawingCanvas != null)
        {
            simpleDrawingCanvas.SetColor(Color.green);
            Debug.Log("DrawingColorSelector: Set initial color to GREEN");
            // Update button visuals to match
            UpdateButtonVisuals("green");
        }

        // Set up labels if they exist
        if (redLabel != null)
            redLabel.text = "Red\n(Sunflower)";
        if (greenLabel != null)
            greenLabel.text = "Green\n(Cactus)";
        if (blueLabel != null)
            blueLabel.text = "Blue\n(Water Lily)";
        
        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();
        UpdateOutputImage();
    }

    private void OnRedButtonClicked()
    {
        if (simpleDrawingCanvas != null)
        {
            simpleDrawingCanvas.SetColor(Color.red);
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
    }

    private void OnBlueButtonClicked()
    {
        if (simpleDrawingCanvas != null)
        {
            simpleDrawingCanvas.SetColor(Color.blue);
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

    #region "Color Picker"

    private void CreateHueImage()
    {
        hueTexture = new Texture2D(16,1);
        hueTexture.wrapMode = TextureWrapMode.Clamp;

        hueTexture.name = "HueImage";

        for (int i = 0; i < hueTexture.width; i++)
        {
            hueTexture.SetPixel(i, 0, Color.HSVToRGB((float)i / hueTexture.width, 1 ,1));
        }
        hueTexture.Apply();
        currentHue = 0;

        hueImage.texture = hueTexture;
    }

       private void CreateSVImage()
    {
        svTexture = new Texture2D(16,16);
        svTexture.wrapMode = TextureWrapMode.Clamp;

        svTexture.name = "SVImage";

        for (int v = 0; v < svTexture.height; v++)
        {
            for(int s = 0; s < svTexture.width; s++)
            {
                svTexture.SetPixel(s, v, Color.HSVToRGB(currentHue, (float) s / svTexture.width, (float) v / svTexture.height));
            }
            
        }
        svTexture.Apply();
        currentSat = 0;
        currentVal = 0;

        satValImage.texture = svTexture;
    }

        private void CreateOutputImage()
    {
        outputTexture = new Texture2D(16,1);
        outputTexture.wrapMode = TextureWrapMode.Clamp;

        outputTexture.name = "OutputImage";

        Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

        for (int i = 0; i < outputTexture.width; i++)
        {
            outputTexture.SetPixel(i, 0, currentColor);
        }
        outputTexture.Apply();

        outputImage.texture = outputTexture;
    }

    private void UpdateOutputImage()
    {
        Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

        for (int i = 0; i < outputTexture.width; i++)
        {
            outputTexture.SetPixel(i, 0, currentColor);
        }
        outputTexture.Apply();

        hexInputField.text = ColorUtility.ToHtmlStringRGB(currentColor);

        simpleDrawingCanvas.SetColor(currentColor);
    }

    public void SetSV(float sat, float val)
    {
        currentSat = sat;
        currentVal = val;

        UpdateOutputImage();
    }

    public void UpdateSVImage()
    {
        currentHue = hueSlider.value;

        for (int v = 0; v < svTexture.height; v++)
        {
            for(int s = 0; s < svTexture.width; s++)
            {
                svTexture.SetPixel(s, v, Color.HSVToRGB(currentHue, (float) s / svTexture.width, (float) v / svTexture.height));
            }
            
        }
        svTexture.Apply();

        satValImage.texture = svTexture;
        UpdateOutputImage();
    }

    public void OnTextInput()
    {
        if (hexInputField.text.Length < 6){return;}
        
        Color newCol;

        if(ColorUtility.TryParseHtmlString($"#{hexInputField.text}", out newCol))
        {
            Color.RGBToHSV(newCol, out currentHue, out currentSat, out currentVal);
        }
        hueSlider.value = currentHue;

        hexInputField.text = "";

        UpdateOutputImage();
    }
    #endregion
}

