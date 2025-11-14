using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Enhanced UI manager for the Drawing Scene
/// Provides better visual feedback, instructions, and user experience
/// </summary>
public class DrawingSceneUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject instructionsPanel;
    public GameObject drawingPanel;
    public GameObject feedbackPanel;

    [Header("Instruction Elements")]
    public TextMeshProUGUI instructionTitle;
    public TextMeshProUGUI instructionText;
    public Button startDrawingButton;

    [Header("Drawing Info")]
    public TextMeshProUGUI strokeCounterText;
    public TextMeshProUGUI hintText;
    public Image progressBar;

    [Header("Feedback Elements")]
    public TextMeshProUGUI detectedPlantText;
    public TextMeshProUGUI elementTypeText;
    public TextMeshProUGUI confidenceText;
    public Image plantIcon;

    [Header("Buttons")]
    public Button clearButton;
    public Button undoButton;
    public Button guideBookButton;

    [Header("Visual Settings")]
    public Color fireColor = new Color(1f, 0.4f, 0f);
    public Color grassColor = new Color(0.3f, 0.8f, 0.3f);
    public Color waterColor = new Color(0.3f, 0.6f, 1f);

    [Header("Animation")]
    public bool useAnimations = true;
    public float fadeSpeed = 2f;

    private bool hasStartedDrawing = false;

    private void Start()
    {
        SetupUI();
        ShowInstructions();
    }

    private void SetupUI()
    {
        // Setup button listeners
        if (startDrawingButton != null)
        {
            startDrawingButton.onClick.AddListener(OnStartDrawing);
        }

        if (clearButton != null)
        {
            clearButton.onClick.AddListener(OnClearCanvas);
        }

        if (undoButton != null)
        {
            undoButton.onClick.AddListener(OnUndo);
        }

        // Initial UI state
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
        if (drawingPanel != null) drawingPanel.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    /// <summary>
    /// Show initial instructions to the player
    /// </summary>
    public void ShowInstructions()
    {
        if (instructionTitle != null)
        {
            instructionTitle.text = "✨ Draw Your Plant Companion! ✨";
        }

        if (instructionText != null)
        {
            instructionText.text =
                "<size=24><b>Welcome to Sketch Blossom!</b></size>\n\n" +
                "Draw a plant that will become your battle companion.\n" +
                "The shape you draw determines its elemental type:\n\n" +
                "🌻 <color=#FF8C42><b>Sunflower</b></color> → Round with petals → <b>Fire Type</b>\n" +
                "🌵 <color=#4ECDC4><b>Cactus</b></color> → Tall and spiky → <b>Grass Type</b>\n" +
                "🌸 <color=#95E1D3><b>Water Lily</b></color> → Wide and flat → <b>Water Type</b>\n\n" +
                "<color=yellow>💡 TIP: Click the book icon anytime for detailed drawing guides!</color>\n\n" +
                "Ready to create your plant warrior?";
        }
    }

    /// <summary>
    /// Called when player clicks "Start Drawing"
    /// </summary>
    public void OnStartDrawing()
    {
        if (instructionsPanel != null)
        {
            if (useAnimations)
            {
                StartCoroutine(FadeOutPanel(instructionsPanel));
            }
            else
            {
                instructionsPanel.SetActive(false);
            }
        }

        // Show overlay (dark background)
        GameObject overlay = GameObject.Find("DrawingOverlay");
        if (overlay != null)
        {
            overlay.SetActive(true);
            Debug.Log("DrawingOverlay activated");
        }

        // Show drawing panel (pop-up window)
        if (drawingPanel != null)
        {
            drawingPanel.SetActive(true);
            if (useAnimations)
            {
                StartCoroutine(FadeInPanel(drawingPanel));
            }
            Debug.Log("DrawingPanel activated");
        }

        // Verify drawing canvas exists
        SimpleDrawingCanvas canvas = FindFirstObjectByType<SimpleDrawingCanvas>();
        if (canvas != null)
        {
            Debug.Log("SimpleDrawingCanvas found - player can now draw");
        }
        else
        {
            Debug.LogError("SimpleDrawingCanvas not found!");
        }

        UpdateHintText("Draw your plant! Use 3-8 strokes for best results.");
        hasStartedDrawing = true;
    }

    /// <summary>
    /// Update stroke counter display
    /// </summary>
    public void UpdateStrokeCounter(int currentStrokes, int maxStrokes)
    {
        if (strokeCounterText != null)
        {
            strokeCounterText.text = $"Strokes: {currentStrokes} / {maxStrokes}";

            // Change color when nearing limit
            if (currentStrokes >= maxStrokes)
            {
                strokeCounterText.color = Color.red;
            }
            else if (currentStrokes >= maxStrokes - 2)
            {
                strokeCounterText.color = Color.yellow;
            }
            else
            {
                strokeCounterText.color = Color.white;
            }
        }

        // Update progress bar
        if (progressBar != null)
        {
            progressBar.fillAmount = (float)currentStrokes / maxStrokes;
        }

        // Update hint based on stroke count
        if (currentStrokes == 1)
        {
            UpdateHintText("Great start! Keep drawing to define the shape.");
        }
        else if (currentStrokes == 3)
        {
            UpdateHintText("Looking good! Add more details or finish when ready.");
        }
        else if (currentStrokes >= maxStrokes - 2)
        {
            UpdateHintText("Almost at the limit! Finish your drawing soon.");
        }
    }

    /// <summary>
    /// Update hint text
    /// </summary>
    public void UpdateHintText(string hint)
    {
        if (hintText != null)
        {
            hintText.text = hint;
        }
    }

    /// <summary>
    /// Show plant detection feedback
    /// </summary>
    public void ShowPlantDetection(PlantRecognitionSystem.RecognitionResult result)
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
        }

        if (detectedPlantText != null)
        {
            string plantName = result.plantData.displayName;
            detectedPlantText.text = $"<size=36><b>{plantName}</b></size>";
        }

        if (elementTypeText != null)
        {
            Color elementColor = GetElementColor(result.element.ToString());
            elementTypeText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(elementColor)}>" +
                                  $"<b>{result.element} Type</b></color>";
        }

        if (confidenceText != null)
        {
            string confidenceBar = GenerateConfidenceBar(result.confidence);
            confidenceText.text = $"Detection: {confidenceBar} {result.confidence:P0}";
        }

        // Update background color based on element
        if (feedbackPanel != null)
        {
            Image panelImage = feedbackPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                Color elementColor = GetElementColor(result.element.ToString());
                elementColor.a = 0.15f; // Subtle background tint
                panelImage.color = elementColor;
            }
        }
    }

    /// <summary>
    /// Get color for element type
    /// </summary>
    private Color GetElementColor(string elementType)
    {
        switch (elementType)
        {
            case "Fire": return fireColor;
            case "Grass": return grassColor;
            case "Water": return waterColor;
            default: return Color.white;
        }
    }

    /// <summary>
    /// Generate visual confidence bar
    /// </summary>
    private string GenerateConfidenceBar(float confidence)
    {
        int filledBars = Mathf.RoundToInt(confidence * 10);
        string bar = "";
        for (int i = 0; i < 10; i++)
        {
            bar += i < filledBars ? "█" : "░";
        }
        return bar;
    }

    /// <summary>
    /// Clear canvas button handler
    /// </summary>
    public void OnClearCanvas()
    {
        SimpleDrawingCanvas canvas = FindFirstObjectByType<SimpleDrawingCanvas>();
        if (canvas != null)
        {
            canvas.ClearAll();
            UpdateHintText("Canvas cleared! Start fresh.");
        }
    }

    /// <summary>
    /// Undo last stroke (if implemented in SimpleDrawingCanvas)
    /// </summary>
    public void OnUndo()
    {
        UpdateHintText("Undo last stroke");
        // Note: You'll need to implement undo in SimpleDrawingCanvas
    }

    // Animation helpers
    private System.Collections.IEnumerator FadeOutPanel(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        panel.SetActive(false);
    }

    private System.Collections.IEnumerator FadeInPanel(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
    }

    /// <summary>
    /// Show a temporary popup message
    /// </summary>
    public void ShowPopupMessage(string message, float duration = 2f)
    {
        StartCoroutine(PopupMessageCoroutine(message, duration));
    }

    private System.Collections.IEnumerator PopupMessageCoroutine(string message, float duration)
    {
        if (hintText != null)
        {
            string originalHint = hintText.text;
            hintText.text = $"<color=yellow>★ {message} ★</color>";
            yield return new WaitForSeconds(duration);
            hintText.text = originalHint;
        }
    }
}
