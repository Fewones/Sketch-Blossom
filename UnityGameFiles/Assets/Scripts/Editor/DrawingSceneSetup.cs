using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Automated setup script for Drawing Scene UI
/// Run this ONCE from Unity Editor: Tools > Sketch Blossom > Setup Drawing Scene UI
/// </summary>
public class DrawingSceneSetup : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Setup Drawing Scene UI")]
    public static void SetupUI()
    {
        if (!EditorUtility.DisplayDialog(
            "Setup Drawing Scene UI",
            "This will create the complete UI hierarchy for the Drawing Scene.\n\n" +
            "Make sure you have DrawingScene open and have a Canvas in the scene.\n\n" +
            "Continue?",
            "Yes, Setup UI",
            "Cancel"))
        {
            return;
        }

        // Find or create Canvas
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // Create UI Manager GameObjects
        CreateUIManagers();

        // Create main UI hierarchy
        CreateInstructionsPanel(canvas.transform);
        CreateDrawingPanel(canvas.transform);
        CreateGuideBookPanel(canvas.transform);

        // Setup components
        SetupDrawingSceneUI();
        SetupPlantGuideBook();

        Debug.Log("✅ Drawing Scene UI setup complete!");
        EditorUtility.DisplayDialog(
            "Setup Complete!",
            "Drawing Scene UI has been created successfully!\n\n" +
            "Next steps:\n" +
            "1. Assign references in Inspector (UIManager, GuideBook)\n" +
            "2. Optional: Download UI assets and apply sprites\n" +
            "3. Test the scene!",
            "OK");
    }

    private static void CreateUIManagers()
    {
        // Create UIManager if doesn't exist
        GameObject uiManager = GameObject.Find("UIManager");
        if (uiManager == null)
        {
            uiManager = new GameObject("UIManager");
            uiManager.AddComponent<DrawingSceneUI>();
        }

        // Create GuideBook if doesn't exist
        GameObject guideBook = GameObject.Find("GuideBookManager");
        if (guideBook == null)
        {
            guideBook = new GameObject("GuideBookManager");
            guideBook.AddComponent<PlantGuideBook>();
        }
    }

    private static GameObject CreateInstructionsPanel(Transform parent)
    {
        // Main Panel
        GameObject panel = CreateUIObject("InstructionsPanel", parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        SetFullScreen(rect);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(1f, 1f, 1f, 0.95f);

        // Title Text
        GameObject titleObj = CreateUIObject("TitleText", panel.transform);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "✨ Draw Your Plant Companion! ✨";
        title.fontSize = 48;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(0.18f, 0.2f, 0.21f);

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.7f);
        titleRect.anchorMax = new Vector2(0.9f, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // Instruction Text
        GameObject instructionObj = CreateUIObject("InstructionText", panel.transform);
        TextMeshProUGUI instruction = instructionObj.AddComponent<TextMeshProUGUI>();
        instruction.text = "Welcome to Sketch Blossom!\n\nDraw a plant to create your battle companion.\n" +
                          "Each shape determines its type:\n\n" +
                          "🌻 Sunflower → Fire Type\n" +
                          "🌵 Cactus → Grass Type\n" +
                          "🌸 Water Lily → Water Type\n\n" +
                          "Click the book icon anytime for guides!";
        instruction.fontSize = 24;
        instruction.alignment = TextAlignmentOptions.Center;
        instruction.color = new Color(0.39f, 0.43f, 0.45f);

        RectTransform instructionRect = instructionObj.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0.15f, 0.3f);
        instructionRect.anchorMax = new Vector2(0.85f, 0.65f);
        instructionRect.offsetMin = Vector2.zero;
        instructionRect.offsetMax = Vector2.zero;

        // Start Button
        GameObject buttonObj = CreateButton("StartButton", "START DRAWING", panel.transform);
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.35f, 0.1f);
        buttonRect.anchorMax = new Vector2(0.65f, 0.2f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        return panel;
    }

    private static GameObject CreateDrawingPanel(Transform parent)
    {
        // Main Drawing Panel
        GameObject panel = CreateUIObject("DrawingPanel", parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        SetFullScreen(rect);
        panel.SetActive(false); // Hidden by default

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.9f, 0.95f, 0.9f, 1f);

        // Top Bar
        GameObject topBar = CreateUIObject("TopBar", panel.transform);
        RectTransform topBarRect = topBar.GetComponent<RectTransform>();
        topBarRect.anchorMin = new Vector2(0f, 0.9f);
        topBarRect.anchorMax = new Vector2(1f, 1f);
        topBarRect.offsetMin = Vector2.zero;
        topBarRect.offsetMax = Vector2.zero;

        Image topBarImage = topBar.AddComponent<Image>();
        topBarImage.color = new Color(0.78f, 0.89f, 0.82f);

        // Top Bar - Title
        GameObject titleObj = CreateUIObject("TitleText", topBar.transform);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "Draw Your Plant";
        title.fontSize = 32;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.3f, 0f);
        titleRect.anchorMax = new Vector2(0.7f, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // Top Bar - Guide Book Button
        GameObject guideButtonObj = CreateButton("GuideBookButton", "📖 Guide", topBar.transform);
        RectTransform guideButtonRect = guideButtonObj.GetComponent<RectTransform>();
        guideButtonRect.anchorMin = new Vector2(0.85f, 0.2f);
        guideButtonRect.anchorMax = new Vector2(0.98f, 0.8f);
        guideButtonRect.offsetMin = Vector2.zero;
        guideButtonRect.offsetMax = Vector2.zero;

        // Top Bar - Stroke Counter
        GameObject strokeCounterObj = CreateUIObject("StrokeCounter", topBar.transform);
        TextMeshProUGUI strokeCounter = strokeCounterObj.AddComponent<TextMeshProUGUI>();
        strokeCounter.text = "Strokes: 0/15";
        strokeCounter.fontSize = 24;
        strokeCounter.alignment = TextAlignmentOptions.Left;

        RectTransform strokeCounterRect = strokeCounterObj.GetComponent<RectTransform>();
        strokeCounterRect.anchorMin = new Vector2(0.02f, 0.2f);
        strokeCounterRect.anchorMax = new Vector2(0.2f, 0.8f);
        strokeCounterRect.offsetMin = Vector2.zero;
        strokeCounterRect.offsetMax = Vector2.zero;

        // Drawing Area
        GameObject drawingArea = CreateUIObject("DrawingArea", panel.transform);
        RectTransform drawingRect = drawingArea.GetComponent<RectTransform>();
        drawingRect.anchorMin = new Vector2(0.15f, 0.2f);
        drawingRect.anchorMax = new Vector2(0.85f, 0.85f);
        drawingRect.offsetMin = Vector2.zero;
        drawingRect.offsetMax = Vector2.zero;

        Image drawingAreaImage = drawingArea.AddComponent<Image>();
        drawingAreaImage.color = Color.white;

        // Drawing Area - Border
        GameObject border = CreateUIObject("Border", drawingArea.transform);
        RectTransform borderRect = border.GetComponent<RectTransform>();
        SetFullScreen(borderRect);
        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        borderImage.type = Image.Type.Sliced;

        // Stroke Container
        GameObject strokeContainer = CreateUIObject("StrokeContainer", drawingArea.transform);
        RectTransform strokeRect = strokeContainer.GetComponent<RectTransform>();
        SetFullScreen(strokeRect);

        // Bottom Bar
        GameObject bottomBar = CreateUIObject("BottomBar", panel.transform);
        RectTransform bottomBarRect = bottomBar.GetComponent<RectTransform>();
        bottomBarRect.anchorMin = new Vector2(0f, 0f);
        bottomBarRect.anchorMax = new Vector2(1f, 0.15f);
        bottomBarRect.offsetMin = Vector2.zero;
        bottomBarRect.offsetMax = Vector2.zero;

        Image bottomBarImage = bottomBar.AddComponent<Image>();
        bottomBarImage.color = new Color(0.9f, 0.9f, 0.9f);

        // Bottom Bar - Hint Text
        GameObject hintObj = CreateUIObject("HintText", bottomBar.transform);
        TextMeshProUGUI hint = hintObj.AddComponent<TextMeshProUGUI>();
        hint.text = "Draw your plant!";
        hint.fontSize = 20;
        hint.fontStyle = FontStyles.Italic;
        hint.alignment = TextAlignmentOptions.Center;
        hint.color = new Color(0.39f, 0.43f, 0.45f);

        RectTransform hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.1f, 0.5f);
        hintRect.anchorMax = new Vector2(0.9f, 0.9f);
        hintRect.offsetMin = Vector2.zero;
        hintRect.offsetMax = Vector2.zero;

        // Bottom Bar - Clear Button
        GameObject clearButton = CreateButton("ClearButton", "Clear", bottomBar.transform);
        RectTransform clearRect = clearButton.GetComponent<RectTransform>();
        clearRect.anchorMin = new Vector2(0.1f, 0.1f);
        clearRect.anchorMax = new Vector2(0.25f, 0.45f);
        clearRect.offsetMin = Vector2.zero;
        clearRect.offsetMax = Vector2.zero;

        // Bottom Bar - Finish Button
        GameObject finishButton = CreateButton("FinishButton", "FINISH", bottomBar.transform);
        RectTransform finishRect = finishButton.GetComponent<RectTransform>();
        finishRect.anchorMin = new Vector2(0.75f, 0.1f);
        finishRect.anchorMax = new Vector2(0.9f, 0.45f);
        finishRect.offsetMin = Vector2.zero;
        finishRect.offsetMax = Vector2.zero;

        // Feedback Panel (hidden by default)
        GameObject feedbackPanel = CreateUIObject("FeedbackPanel", panel.transform);
        RectTransform feedbackRect = feedbackPanel.GetComponent<RectTransform>();
        feedbackRect.anchorMin = new Vector2(0.3f, 0.3f);
        feedbackRect.anchorMax = new Vector2(0.7f, 0.6f);
        feedbackRect.offsetMin = Vector2.zero;
        feedbackRect.offsetMax = Vector2.zero;

        Image feedbackImage = feedbackPanel.AddComponent<Image>();
        feedbackImage.color = new Color(1f, 1f, 1f, 0.95f);
        feedbackPanel.SetActive(false);

        // Feedback Panel Content
        GameObject detectedPlantObj = CreateUIObject("DetectedPlantText", feedbackPanel.transform);
        TextMeshProUGUI detectedPlant = detectedPlantObj.AddComponent<TextMeshProUGUI>();
        detectedPlant.text = "Sunflower";
        detectedPlant.fontSize = 36;
        detectedPlant.fontStyle = FontStyles.Bold;
        detectedPlant.alignment = TextAlignmentOptions.Center;

        RectTransform detectedRect = detectedPlantObj.GetComponent<RectTransform>();
        detectedRect.anchorMin = new Vector2(0.1f, 0.6f);
        detectedRect.anchorMax = new Vector2(0.9f, 0.9f);
        detectedRect.offsetMin = Vector2.zero;
        detectedRect.offsetMax = Vector2.zero;

        GameObject elementTypeObj = CreateUIObject("ElementTypeText", feedbackPanel.transform);
        TextMeshProUGUI elementType = elementTypeObj.AddComponent<TextMeshProUGUI>();
        elementType.text = "Fire Type";
        elementType.fontSize = 28;
        elementType.alignment = TextAlignmentOptions.Center;

        RectTransform elementRect = elementTypeObj.GetComponent<RectTransform>();
        elementRect.anchorMin = new Vector2(0.1f, 0.4f);
        elementRect.anchorMax = new Vector2(0.9f, 0.6f);
        elementRect.offsetMin = Vector2.zero;
        elementRect.offsetMax = Vector2.zero;

        GameObject confidenceObj = CreateUIObject("ConfidenceText", feedbackPanel.transform);
        TextMeshProUGUI confidence = confidenceObj.AddComponent<TextMeshProUGUI>();
        confidence.text = "Confidence: 85%";
        confidence.fontSize = 20;
        confidence.alignment = TextAlignmentOptions.Center;

        RectTransform confidenceRect = confidenceObj.GetComponent<RectTransform>();
        confidenceRect.anchorMin = new Vector2(0.1f, 0.1f);
        confidenceRect.anchorMax = new Vector2(0.9f, 0.4f);
        confidenceRect.offsetMin = Vector2.zero;
        confidenceRect.offsetMax = Vector2.zero;

        return panel;
    }

    private static GameObject CreateGuideBookPanel(Transform parent)
    {
        // Main Guide Book Panel
        GameObject panel = CreateUIObject("GuideBookPanel", parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.4f, 0.1f);
        rect.anchorMax = new Vector2(0.95f, 0.9f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.98f, 0.93f, 0.84f); // Parchment color
        panel.SetActive(false); // Hidden by default

        // Close Button
        GameObject closeButton = CreateButton("CloseButton", "✕", panel.transform);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.92f, 0.92f);
        closeRect.anchorMax = new Vector2(0.98f, 0.98f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;

        // Page Title
        GameObject titleObj = CreateUIObject("PageTitle", panel.transform);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "Plant Guide";
        title.fontSize = 40;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(0.29f, 0.25f, 0.21f);

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.85f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // Page Description
        GameObject descObj = CreateUIObject("PageDescription", panel.transform);
        TextMeshProUGUI desc = descObj.AddComponent<TextMeshProUGUI>();
        desc.text = "Guide content will appear here...";
        desc.fontSize = 20;
        desc.alignment = TextAlignmentOptions.TopLeft;
        desc.color = new Color(0.29f, 0.25f, 0.21f);
        desc.enableWordWrapping = true;

        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.1f, 0.2f);
        descRect.anchorMax = new Vector2(0.9f, 0.8f);
        descRect.offsetMin = Vector2.zero;
        descRect.offsetMax = Vector2.zero;

        // Guide Image (optional)
        GameObject imageObj = CreateUIObject("GuideImage", panel.transform);
        Image guideImage = imageObj.AddComponent<Image>();
        guideImage.color = new Color(1f, 1f, 1f, 0f); // Transparent by default

        RectTransform imageRect = imageObj.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.3f, 0.45f);
        imageRect.anchorMax = new Vector2(0.7f, 0.75f);
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        // Navigation Panel
        GameObject navPanel = CreateUIObject("NavigationPanel", panel.transform);
        RectTransform navRect = navPanel.GetComponent<RectTransform>();
        navRect.anchorMin = new Vector2(0.2f, 0.05f);
        navRect.anchorMax = new Vector2(0.8f, 0.15f);
        navRect.offsetMin = Vector2.zero;
        navRect.offsetMax = Vector2.zero;

        // Previous Button
        GameObject prevButton = CreateButton("PreviousButton", "◀ Previous", navPanel.transform);
        RectTransform prevRect = prevButton.GetComponent<RectTransform>();
        prevRect.anchorMin = new Vector2(0f, 0.2f);
        prevRect.anchorMax = new Vector2(0.4f, 0.8f);
        prevRect.offsetMin = Vector2.zero;
        prevRect.offsetMax = Vector2.zero;

        // Page Number
        GameObject pageNumObj = CreateUIObject("PageNumberText", navPanel.transform);
        TextMeshProUGUI pageNum = pageNumObj.AddComponent<TextMeshProUGUI>();
        pageNum.text = "Page 1 / 5";
        pageNum.fontSize = 18;
        pageNum.alignment = TextAlignmentOptions.Center;

        RectTransform pageNumRect = pageNumObj.GetComponent<RectTransform>();
        pageNumRect.anchorMin = new Vector2(0.4f, 0.2f);
        pageNumRect.anchorMax = new Vector2(0.6f, 0.8f);
        pageNumRect.offsetMin = Vector2.zero;
        pageNumRect.offsetMax = Vector2.zero;

        // Next Button
        GameObject nextButton = CreateButton("NextButton", "Next ▶", navPanel.transform);
        RectTransform nextRect = nextButton.GetComponent<RectTransform>();
        nextRect.anchorMin = new Vector2(0.6f, 0.2f);
        nextRect.anchorMax = new Vector2(1f, 0.8f);
        nextRect.offsetMin = Vector2.zero;
        nextRect.offsetMax = Vector2.zero;

        return panel;
    }

    private static void SetupDrawingSceneUI()
    {
        GameObject uiManager = GameObject.Find("UIManager");
        if (uiManager == null) return;

        DrawingSceneUI ui = uiManager.GetComponent<DrawingSceneUI>();
        if (ui == null) return;

        // Find and assign references
        ui.instructionsPanel = GameObject.Find("InstructionsPanel");
        ui.drawingPanel = GameObject.Find("DrawingPanel");
        ui.feedbackPanel = GameObject.Find("FeedbackPanel");

        ui.instructionTitle = GameObject.Find("InstructionsPanel/TitleText")?.GetComponent<TextMeshProUGUI>();
        ui.instructionText = GameObject.Find("InstructionsPanel/InstructionText")?.GetComponent<TextMeshProUGUI>();
        ui.startDrawingButton = GameObject.Find("InstructionsPanel/StartButton")?.GetComponent<Button>();

        ui.strokeCounterText = GameObject.Find("StrokeCounter")?.GetComponent<TextMeshProUGUI>();
        ui.hintText = GameObject.Find("HintText")?.GetComponent<TextMeshProUGUI>();

        ui.detectedPlantText = GameObject.Find("DetectedPlantText")?.GetComponent<TextMeshProUGUI>();
        ui.elementTypeText = GameObject.Find("ElementTypeText")?.GetComponent<TextMeshProUGUI>();
        ui.confidenceText = GameObject.Find("ConfidenceText")?.GetComponent<TextMeshProUGUI>();

        ui.clearButton = GameObject.Find("ClearButton")?.GetComponent<Button>();
        ui.guideBookButton = GameObject.Find("GuideBookButton")?.GetComponent<Button>();

        EditorUtility.SetDirty(uiManager);
    }

    private static void SetupPlantGuideBook()
    {
        GameObject guideManager = GameObject.Find("GuideBookManager");
        if (guideManager == null)
        {
            Debug.LogError("GuideBookManager not found!");
            return;
        }

        PlantGuideBook guide = guideManager.GetComponent<PlantGuideBook>();
        if (guide == null)
        {
            Debug.LogError("PlantGuideBook component not found!");
            return;
        }

        // Find and assign references
        guide.bookPanel = GameObject.Find("GuideBookPanel");
        guide.openBookButton = GameObject.Find("GuideBookButton")?.GetComponent<Button>();
        guide.closeBookButton = GameObject.Find("GuideBookPanel/CloseButton")?.GetComponent<Button>();

        // Look for navigation buttons in hierarchy
        Button[] allButtons = GameObject.FindObjectsOfType<Button>();
        foreach (var btn in allButtons)
        {
            if (btn.gameObject.name == "NextButton")
                guide.nextPageButton = btn;
            else if (btn.gameObject.name == "PreviousButton")
                guide.previousPageButton = btn;
        }

        // Find text elements
        TextMeshProUGUI[] allText = GameObject.FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allText)
        {
            if (text.gameObject.name == "PageTitle")
                guide.pageTitle = text;
            else if (text.gameObject.name == "PageDescription")
                guide.pageDescription = text;
            else if (text.gameObject.name == "PageNumberText")
                guide.pageNumberText = text;
        }

        guide.guideImage = GameObject.Find("GuideBookPanel/GuideImage")?.GetComponent<Image>();

        // Debug output
        Debug.Log($"Guide Book Setup - Panel: {guide.bookPanel != null}, OpenBtn: {guide.openBookButton != null}, CloseBtn: {guide.closeBookButton != null}");
        Debug.Log($"Next: {guide.nextPageButton != null}, Prev: {guide.previousPageButton != null}");

        EditorUtility.SetDirty(guideManager);
    }

    // Helper methods
    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        return obj;
    }

    private static GameObject CreateButton(string name, string text, Transform parent)
    {
        GameObject buttonObj = CreateUIObject(name, parent);
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0f, 0.72f, 0.58f); // Green

        Button button = buttonObj.AddComponent<Button>();

        GameObject textObj = CreateUIObject("Text", buttonObj.transform);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 24;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        SetFullScreen(textRect);

        return buttonObj;
    }

    private static void SetFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
