using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages color selection UI for the drawing system
/// Provides Red, Green, and Blue color buttons for plant type influence
/// </summary>
public class DrawingColorSelector : MonoBehaviour
{

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

        CreateHueImage(); // Creates the image for the Hue Slider
        CreateSVImage();  // Creates the image for the Color Picker
        CreateOutputImage(); // Creates the image for the current color
        UpdateOutputImage(); // Sets the output image to the current color
    }

    #region Color Picker

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

