using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Editor script to automatically set up the color selection UI for the drawing system
/// Usage: Tools > Sketch Blossom > Setup Color Selection UI
/// </summary>
public class ColorSelectionSetup : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Setup Color Selection UI")]
    public static void SetupColorSelectionUI()
    {
        Debug.Log("=== COLOR SELECTION UI SETUP START ===");

        // Find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene! Please create a Canvas first.", "OK");
            Debug.LogError("No Canvas found!");
            return;
        }

        // Find DrawingPanel
        Transform drawingPanelTransform = canvas.transform.Find("DrawingPanel");
        if (drawingPanelTransform == null)
        {
            EditorUtility.DisplayDialog("Error", "DrawingPanel not found in Canvas! Please create DrawingPanel first.", "OK");
            Debug.LogError("DrawingPanel not found!");
            return;
        }

        GameObject drawingPanel = drawingPanelTransform.gameObject;
        Debug.Log("Found DrawingPanel: " + drawingPanel.name);

        // Create or find ColorSelector container
        Transform colorSelectorTransform = drawingPanelTransform.Find("ColorSelector");
        GameObject colorSelector;

        if (colorSelectorTransform == null)
        {
            colorSelector = new GameObject("ColorSelector");
            colorSelector.transform.SetParent(drawingPanelTransform, false);
            Debug.Log("Created ColorSelector GameObject");
        }
        else
        {
            colorSelector = colorSelectorTransform.gameObject;
            Debug.Log("Found existing ColorSelector GameObject");
        }

        // Add RectTransform if needed
        RectTransform colorSelectorRect = colorSelector.GetComponent<RectTransform>();
        if (colorSelectorRect == null)
        {
            colorSelectorRect = colorSelector.AddComponent<RectTransform>();
        }

        // Position ColorSelector (top-left area of DrawingPanel)
        colorSelectorRect.anchorMin = new Vector2(0f, 1f);
        colorSelectorRect.anchorMax = new Vector2(0f, 1f);
        colorSelectorRect.pivot = new Vector2(0f, 1f);
        colorSelectorRect.anchoredPosition = new Vector2(20f, -20f);
        colorSelectorRect.sizeDelta = new Vector2(200f, 80f);

        Debug.Log("Configured ColorSelector position");

        // Create color buttons
        GameObject redButton = CreateColorButton(colorSelector.transform, "RedButton", Color.red, new Vector2(0f, 0f), "Red\n(Sunflower)");
        GameObject greenButton = CreateColorButton(colorSelector.transform, "GreenButton", Color.green, new Vector2(70f, 0f), "Green\n(Cactus)");
        GameObject blueButton = CreateColorButton(colorSelector.transform, "BlueButton", Color.blue, new Vector2(140f, 0f), "Blue\n(Water Lily)");

        Debug.Log("Created color buttons");

        // Add or get DrawingColorSelector component
        DrawingColorSelector colorSelectorComponent = colorSelector.GetComponent<DrawingColorSelector>();
        if (colorSelectorComponent == null)
        {
            colorSelectorComponent = colorSelector.AddComponent<DrawingColorSelector>();
            Debug.Log("Added DrawingColorSelector component");
        }
        else
        {
            Debug.Log("Found existing DrawingColorSelector component");
        }

        // Find DrawingCanvas in scene
        DrawingCanvas drawingCanvas = FindObjectOfType<DrawingCanvas>();
        if (drawingCanvas == null)
        {
            Debug.LogWarning("DrawingCanvas not found in scene. You'll need to assign it manually.");
        }

        // Assign references to DrawingColorSelector
        colorSelectorComponent.redButton = redButton.GetComponent<Button>();
        colorSelectorComponent.greenButton = greenButton.GetComponent<Button>();
        colorSelectorComponent.blueButton = blueButton.GetComponent<Button>();

        colorSelectorComponent.redButtonImage = redButton.GetComponent<Image>();
        colorSelectorComponent.greenButtonImage = greenButton.GetComponent<Image>();
        colorSelectorComponent.blueButtonImage = blueButton.GetComponent<Image>();

        colorSelectorComponent.redLabel = redButton.GetComponentInChildren<TextMeshProUGUI>();
        colorSelectorComponent.greenLabel = greenButton.GetComponentInChildren<TextMeshProUGUI>();
        colorSelectorComponent.blueLabel = blueButton.GetComponentInChildren<TextMeshProUGUI>();

        colorSelectorComponent.drawingCanvas = drawingCanvas;

        Debug.Log("Assigned all references to DrawingColorSelector");

        // Mark scene as dirty so changes are saved
        EditorUtility.SetDirty(colorSelector);
        EditorUtility.SetDirty(drawingPanel);
        if (drawingCanvas != null)
        {
            EditorUtility.SetDirty(drawingCanvas.gameObject);
        }

        Debug.Log("=== COLOR SELECTION UI SETUP COMPLETE ===");
        EditorUtility.DisplayDialog("Success",
            "Color Selection UI setup complete!\n\n" +
            "Created:\n" +
            "- ColorSelector container\n" +
            "- Red Button (Sunflower)\n" +
            "- Green Button (Cactus)\n" +
            "- Blue Button (Water Lily)\n" +
            "- DrawingColorSelector component\n\n" +
            "All references have been assigned automatically.",
            "OK");

        // Select the ColorSelector in hierarchy for easy inspection
        Selection.activeGameObject = colorSelector;
    }

    /// <summary>
    /// Create a color button with proper configuration
    /// </summary>
    private static GameObject CreateColorButton(Transform parent, string name, Color color, Vector2 position, string labelText)
    {
        // Check if button already exists
        Transform existingButton = parent.Find(name);
        if (existingButton != null)
        {
            Debug.Log($"Button {name} already exists, reconfiguring it");
            GameObject existing = existingButton.gameObject;
            ConfigureButton(existing, color, position, labelText);
            return existing;
        }

        // Create button GameObject
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent, false);

        // Configure button
        ConfigureButton(button, color, position, labelText);

        Debug.Log($"Created button: {name}");
        return button;
    }

    /// <summary>
    /// Configure button appearance and layout
    /// </summary>
    private static void ConfigureButton(GameObject button, Color color, Vector2 position, string labelText)
    {
        // Add RectTransform
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = button.AddComponent<RectTransform>();
        }

        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(60f, 60f);

        // Add Image component
        Image image = button.GetComponent<Image>();
        if (image == null)
        {
            image = button.AddComponent<Image>();
        }
        image.color = color;

        // Add Button component
        Button buttonComponent = button.GetComponent<Button>();
        if (buttonComponent == null)
        {
            buttonComponent = button.AddComponent<Button>();
        }

        // Configure button colors
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = color;
        colors.highlightedColor = new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f, 1f);
        colors.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, 1f);
        colors.selectedColor = color;
        colors.disabledColor = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 0.5f);
        buttonComponent.colors = colors;

        // Create or update label
        Transform labelTransform = button.transform.Find("Label");
        GameObject label;

        if (labelTransform == null)
        {
            label = new GameObject("Label");
            label.transform.SetParent(button.transform, false);
        }
        else
        {
            label = labelTransform.gameObject;
        }

        // Configure label RectTransform
        RectTransform labelRect = label.GetComponent<RectTransform>();
        if (labelRect == null)
        {
            labelRect = label.AddComponent<RectTransform>();
        }

        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        labelRect.anchoredPosition = Vector2.zero;

        // Add TextMeshProUGUI component
        TextMeshProUGUI text = label.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = label.AddComponent<TextMeshProUGUI>();
        }

        text.text = labelText;
        text.fontSize = 10;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;

        // Add outline for better visibility
        if (text.GetComponent<Outline>() == null)
        {
            Outline outline = label.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);
        }
    }

    [MenuItem("Tools/Sketch Blossom/Setup Simple Result Display")]
    public static void SetupSimpleResultDisplay()
    {
        Debug.Log("=== SIMPLE RESULT DISPLAY SETUP START ===");

        // Find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene!", "OK");
            return;
        }

        // Find DrawingPanel
        Transform drawingPanelTransform = canvas.transform.Find("DrawingPanel");
        if (drawingPanelTransform == null)
        {
            EditorUtility.DisplayDialog("Error", "DrawingPanel not found in Canvas!", "OK");
            return;
        }

        // Create or find ResultText
        Transform existingText = drawingPanelTransform.Find("ResultText");
        GameObject resultTextObj;

        if (existingText == null)
        {
            resultTextObj = new GameObject("ResultText");
            resultTextObj.transform.SetParent(drawingPanelTransform, false);
            Debug.Log("Created ResultText GameObject");
        }
        else
        {
            resultTextObj = existingText.gameObject;
            Debug.Log("Found existing ResultText");
        }

        // Configure RectTransform
        RectTransform rect = resultTextObj.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = resultTextObj.AddComponent<RectTransform>();
        }

        // Position in center of drawing panel
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(500f, 400f);

        // Add TextMeshProUGUI
        TextMeshProUGUI text = resultTextObj.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = resultTextObj.AddComponent<TextMeshProUGUI>();
        }

        text.text = "Plant Analysis Results";
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.enableWordWrapping = true;

        // Add background
        Image bg = resultTextObj.GetComponent<Image>();
        if (bg == null)
        {
            bg = resultTextObj.AddComponent<Image>();
        }
        bg.color = new Color(0f, 0f, 0f, 0.8f);

        // Add outline for readability
        Outline outline = resultTextObj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = resultTextObj.AddComponent<Outline>();
        }
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        // Create or get SimpleResultDisplay component on DrawingPanel
        SimpleResultDisplay displayComponent = drawingPanelTransform.GetComponent<SimpleResultDisplay>();
        if (displayComponent == null)
        {
            displayComponent = drawingPanelTransform.gameObject.AddComponent<SimpleResultDisplay>();
            Debug.Log("Added SimpleResultDisplay component to DrawingPanel");
        }

        displayComponent.resultText = text;

        // Note: This display method is deprecated - use PlantResultPanel instead
        // The setup is kept for backwards compatibility but won't be linked to DrawingManager
        Debug.Log("SimpleResultDisplay created (deprecated - use PlantResultPanel instead)");

        // Mark objects as dirty
        EditorUtility.SetDirty(resultTextObj);
        EditorUtility.SetDirty(drawingPanelTransform.gameObject);

        Debug.Log("=== SIMPLE RESULT DISPLAY SETUP COMPLETE ===");
        EditorUtility.DisplayDialog("Success",
            "Simple Result Display setup complete!\n\n" +
            "A text display has been added to show plant analysis results.",
            "OK");

        Selection.activeGameObject = resultTextObj;
    }

    [MenuItem("Tools/Sketch Blossom/Setup Analysis Result Panel")]
    public static void SetupAnalysisResultPanel()
    {
        Debug.Log("=== ANALYSIS RESULT PANEL SETUP START ===");

        // Find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene! Please create a Canvas first.", "OK");
            Debug.LogError("No Canvas found!");
            return;
        }

        // Create or find AnalysisResultPanel
        Transform existingPanel = canvas.transform.Find("AnalysisResultPanel");
        GameObject resultPanel;

        if (existingPanel == null)
        {
            resultPanel = new GameObject("AnalysisResultPanel");
            resultPanel.transform.SetParent(canvas.transform, false);
            Debug.Log("Created AnalysisResultPanel GameObject");
        }
        else
        {
            resultPanel = existingPanel.gameObject;
            Debug.Log("Found existing AnalysisResultPanel");
        }

        // Add PlantAnalysisResultPanel component
        PlantAnalysisResultPanel panelComponent = resultPanel.GetComponent<PlantAnalysisResultPanel>();
        if (panelComponent == null)
        {
            panelComponent = resultPanel.AddComponent<PlantAnalysisResultPanel>();
            Debug.Log("Added PlantAnalysisResultPanel component");
        }

        // Create panel UI structure
        GameObject panelBackground = CreateAnalysisPanelUI(resultPanel.transform);
        panelComponent.resultPanel = panelBackground;

        // Assign UI references
        panelComponent.titleText = panelBackground.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        panelComponent.plantTypeText = panelBackground.transform.Find("PlantTypeText")?.GetComponent<TextMeshProUGUI>();
        panelComponent.elementTypeText = panelBackground.transform.Find("ElementTypeText")?.GetComponent<TextMeshProUGUI>();
        panelComponent.confidenceText = panelBackground.transform.Find("ConfidenceText")?.GetComponent<TextMeshProUGUI>();
        panelComponent.colorInfoText = panelBackground.transform.Find("ColorInfoText")?.GetComponent<TextMeshProUGUI>();
        panelComponent.movesText = panelBackground.transform.Find("MovesText")?.GetComponent<TextMeshProUGUI>();
        panelComponent.continueButton = panelBackground.transform.Find("ContinueButton")?.GetComponent<Button>();

        // Note: This panel method is deprecated - use PlantResultPanel instead
        // The setup is kept for backwards compatibility but won't be linked to DrawingManager
        Debug.Log("AnalysisResultPanel created (deprecated - use PlantResultPanel instead)");

        // Mark objects as dirty
        EditorUtility.SetDirty(resultPanel);
        EditorUtility.SetDirty(panelComponent);

        Debug.Log("=== ANALYSIS RESULT PANEL SETUP COMPLETE ===");
        EditorUtility.DisplayDialog("Success",
            "Analysis Result Panel setup complete!\n\n" +
            "The panel will display plant detection results before transitioning to battle.",
            "OK");

        Selection.activeGameObject = resultPanel;
    }

    private static GameObject CreateAnalysisPanelUI(Transform parent)
    {
        // Check if Background already exists
        Transform existingBg = parent.Find("Background");
        if (existingBg != null)
        {
            Debug.Log("Panel Background already exists, reconfiguring...");
            return existingBg.gameObject;
        }

        // Create background panel
        GameObject background = new GameObject("Background");
        background.transform.SetParent(parent, false);

        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.85f); // Dark semi-transparent background

        // Create content panel (centered box)
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(background.transform, false);

        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(600f, 500f);

        Image contentImage = contentPanel.AddComponent<Image>();
        contentImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Create text elements
        CreateResultText(background.transform, "TitleText", new Vector2(0, 200), 32, "Plant Analysis Complete!");
        CreateResultText(background.transform, "PlantTypeText", new Vector2(0, 130), 48, "Sunflower");
        CreateResultText(background.transform, "ElementTypeText", new Vector2(0, 70), 24, "Fire Type");
        CreateResultText(background.transform, "ConfidenceText", new Vector2(0, 20), 20, "Confidence: ★★★★★");
        CreateResultText(background.transform, "ColorInfoText", new Vector2(0, -40), 18, "Color: Red");
        CreateResultText(background.transform, "MovesText", new Vector2(0, -120), 18, "Available Moves:\n• Fireball\n• Flame Wave\n• Burn");

        // Create continue button
        GameObject continueButton = new GameObject("ContinueButton");
        continueButton.transform.SetParent(background.transform, false);

        RectTransform buttonRect = continueButton.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0, -200);
        buttonRect.sizeDelta = new Vector2(200, 50);

        Image buttonImage = continueButton.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);

        Button button = continueButton.AddComponent<Button>();

        // Button text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(continueButton.transform, false);

        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Continue to Battle";
        buttonText.fontSize = 18;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        // Initially hide the panel
        background.SetActive(false);

        return background;
    }

    private static void CreateResultText(Transform parent, string name, Vector2 position, float fontSize, string defaultText)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(550, fontSize * 3);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = defaultText;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        // Add outline for readability
        if (text.GetComponent<Outline>() == null)
        {
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);
        }
    }

    [MenuItem("Tools/Sketch Blossom/Remove Color Selection UI")]
    public static void RemoveColorSelectionUI()
    {
        // Find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene!", "OK");
            return;
        }

        // Find DrawingPanel
        Transform drawingPanelTransform = canvas.transform.Find("DrawingPanel");
        if (drawingPanelTransform == null)
        {
            EditorUtility.DisplayDialog("Error", "DrawingPanel not found in Canvas!", "OK");
            return;
        }

        // Find ColorSelector
        Transform colorSelectorTransform = drawingPanelTransform.Find("ColorSelector");
        if (colorSelectorTransform == null)
        {
            EditorUtility.DisplayDialog("Info", "ColorSelector not found. Nothing to remove.", "OK");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog("Confirm Removal",
            "Are you sure you want to remove the Color Selection UI?\n\nThis will delete the ColorSelector GameObject and all its children.",
            "Yes, Remove",
            "Cancel");

        if (confirm)
        {
            DestroyImmediate(colorSelectorTransform.gameObject);
            Debug.Log("Color Selection UI removed");
            EditorUtility.DisplayDialog("Success", "Color Selection UI has been removed.", "OK");
        }
    }

    [MenuItem("Tools/Sketch Blossom/Validate Color Selection Setup")]
    public static void ValidateSetup()
    {
        Debug.Log("=== VALIDATING COLOR SELECTION SETUP ===");

        string report = "Color Selection Setup Validation:\n\n";
        bool allValid = true;

        // Check Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            report += "❌ Canvas not found\n";
            allValid = false;
        }
        else
        {
            report += "✓ Canvas found\n";

            // Check DrawingPanel
            Transform drawingPanel = canvas.transform.Find("DrawingPanel");
            if (drawingPanel == null)
            {
                report += "❌ DrawingPanel not found\n";
                allValid = false;
            }
            else
            {
                report += "✓ DrawingPanel found\n";

                // Check ColorSelector
                Transform colorSelector = drawingPanel.Find("ColorSelector");
                if (colorSelector == null)
                {
                    report += "❌ ColorSelector not found\n";
                    allValid = false;
                }
                else
                {
                    report += "✓ ColorSelector found\n";

                    // Check DrawingColorSelector component
                    DrawingColorSelector component = colorSelector.GetComponent<DrawingColorSelector>();
                    if (component == null)
                    {
                        report += "❌ DrawingColorSelector component missing\n";
                        allValid = false;
                    }
                    else
                    {
                        report += "✓ DrawingColorSelector component found\n";

                        // Check button references
                        report += component.redButton != null ? "✓ Red button assigned\n" : "❌ Red button not assigned\n";
                        report += component.greenButton != null ? "✓ Green button assigned\n" : "❌ Green button not assigned\n";
                        report += component.blueButton != null ? "✓ Blue button assigned\n" : "❌ Blue button not assigned\n";

                        // Check DrawingCanvas reference
                        if (component.drawingCanvas == null)
                        {
                            report += "⚠️ DrawingCanvas not assigned (will auto-find at runtime)\n";
                        }
                        else
                        {
                            report += "✓ DrawingCanvas assigned\n";
                        }

                        allValid = allValid && component.redButton != null && component.greenButton != null && component.blueButton != null;
                    }
                }
            }
        }

        // Check DrawingCanvas in scene
        DrawingCanvas drawingCanvas = FindObjectOfType<DrawingCanvas>();
        if (drawingCanvas == null)
        {
            report += "⚠️ DrawingCanvas not found in scene\n";
        }
        else
        {
            report += "✓ DrawingCanvas found in scene\n";
        }

        Debug.Log(report);

        if (allValid)
        {
            EditorUtility.DisplayDialog("Validation Success", report + "\n✓ All components are properly set up!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Validation Issues", report + "\n⚠️ Some issues were found. Run Setup again to fix.", "OK");
        }
    }
}
