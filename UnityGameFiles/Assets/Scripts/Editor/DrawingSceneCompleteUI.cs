using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Complete Drawing Scene builder with introduction, drawing window, and guide
/// Run from: Tools > Sketch Blossom > Build Complete Drawing Scene UI
/// </summary>
public class DrawingSceneCompleteUI : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Build Complete Drawing Scene UI", priority = 0)]
    public static void BuildCompleteUI()
    {
        if (!EditorUtility.DisplayDialog(
            "Build Complete Drawing Scene UI?",
            "This will DELETE existing UI and build a complete, polished Drawing Scene:\n\n" +
            "- Welcome introduction\n" +
            "- 'Draw my first plant' button\n" +
            "- Drawing window with hint text\n" +
            "- Guide book (press H)\n\n" +
            "Your components are safe. Continue?",
            "Yes, Build It",
            "Cancel"))
        {
            return;
        }

        Debug.Log("╔════════════════════════════════════════════════════╗");
        Debug.Log("║    BUILDING COMPLETE DRAWING SCENE UI              ║");
        Debug.Log("╚════════════════════════════════════════════════════╝");

        // Clean up
        int deleted = CleanupOldUI();

        // Build complete UI
        int created = BuildCompleteScene();

        // Wire everything
        int wired = WireAllComponents();

        Debug.Log("╔════════════════════════════════════════════════════╗");
        Debug.Log($"║  COMPLETE! Deleted: {deleted}, Created: {created}, Wired: {wired}");
        Debug.Log("╚════════════════════════════════════════════════════╝");

        EditorUtility.DisplayDialog(
            "Scene Built!",
            $"Complete Drawing Scene UI is ready!\n\n" +
            $"Deleted: {deleted} old elements\n" +
            $"Created: {created} fresh elements\n" +
            $"Wired: {wired} connections\n\n" +
            "Features:\n" +
            "✅ Welcome introduction\n" +
            "✅ 'Draw my first plant' button\n" +
            "✅ Drawing window\n" +
            "✅ Guide book (press H)\n" +
            "✅ Hint text\n\n" +
            "Press Play to test!",
            "OK"
        );
    }

    private static int CleanupOldUI()
    {
        Debug.Log("\n▶ Cleaning up old UI...");
        int deleted = 0;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return 0;

        List<GameObject> toDelete = new List<GameObject>();

        foreach (Transform child in canvas.transform)
        {
            string name = child.name.ToLower();
            if (name.Contains("panel") || name.Contains("button") ||
                name.Contains("background") || name.Contains("area") ||
                name.Contains("guide") || name.Contains("drawing") ||
                name.Contains("instructions") || name.Contains("hint"))
            {
                toDelete.Add(child.gameObject);
            }
        }

        foreach (var obj in toDelete)
        {
            Debug.Log($"   🗑️ {obj.name}");
            Object.DestroyImmediate(obj);
            deleted++;
        }

        Debug.Log($"   ✅ Deleted {deleted} elements");
        return deleted;
    }

    private static int BuildCompleteScene()
    {
        Debug.Log("\n▶ Building complete scene...");
        int created = 0;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas!");
            return 0;
        }

        // 1. Background
        GameObject bg = CreateBackground(canvas.transform);
        Debug.Log("   ✓ Background");
        created++;

        // 2. Instructions Panel (welcome screen)
        GameObject instructionsPanel = CreateInstructionsPanel(canvas.transform);
        Debug.Log("   ✓ InstructionsPanel with 'Draw my first plant' button");
        created++;

        // 3. Drawing Panel (shown after button click)
        GameObject drawingPanel = CreateDrawingPanel(canvas.transform);
        drawingPanel.SetActive(false); // Hidden initially
        Debug.Log("   ✓ DrawingPanel with drawing area and hint");
        created++;

        // 4. Guide Book Button
        GameObject guideBtn = CreateGuideButton(canvas.transform);
        Debug.Log("   ✓ GuideBookButton");
        created++;

        // 5. Guide Book Panel
        GameObject guidePanel = CreateGuidePanel(canvas.transform);
        Debug.Log("   ✓ GuideBookPanel");
        created++;

        // 6. LineRenderer Prefab
        CreateLineRendererPrefab();
        Debug.Log("   ✓ LineRenderer.prefab");
        created++;

        // 7. EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("   ✓ EventSystem");
            created++;
        }

        Debug.Log($"   ✅ Created {created} elements");
        return created;
    }

    private static GameObject CreateBackground(Transform parent)
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(parent, false);
        bg.transform.SetAsFirstSibling();

        RectTransform rect = bg.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = bg.AddComponent<Image>();
        img.color = new Color(0.82f, 0.94f, 0.82f, 1f); // Sage green

        return bg;
    }

    private static GameObject CreateInstructionsPanel(Transform parent)
    {
        GameObject panel = new GameObject("InstructionsPanel");
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.15f, 0.15f);
        rect.anchorMax = new Vector2(0.85f, 0.85f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.95f); // White panel

        // Title
        GameObject title = new GameObject("TitleText");
        title.transform.SetParent(panel.transform, false);

        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.7f);
        titleRect.anchorMax = new Vector2(0.9f, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "Welcome to Sketch Blossom!";
        titleText.fontSize = 42;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.2f, 0.5f, 0.3f);

        // Instruction text
        GameObject instructions = new GameObject("InstructionText");
        instructions.transform.SetParent(panel.transform, false);

        RectTransform instrRect = instructions.AddComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0.1f, 0.35f);
        instrRect.anchorMax = new Vector2(0.9f, 0.65f);
        instrRect.offsetMin = Vector2.zero;
        instrRect.offsetMax = Vector2.zero;

        TextMeshProUGUI instrText = instructions.AddComponent<TextMeshProUGUI>();
        instrText.text = "In this game, you'll draw your own plant companion that will battle alongside you!\n\n" +
                         "Draw a Sunflower, Cactus, or Water Lily - each has unique abilities.\n\n" +
                         "Ready to create your first plant?";
        instrText.fontSize = 24;
        instrText.alignment = TextAlignmentOptions.Center;
        instrText.color = new Color(0.2f, 0.2f, 0.2f);

        // "Draw my first plant" button
        GameObject startBtn = new GameObject("StartDrawingButton");
        startBtn.transform.SetParent(panel.transform, false);

        RectTransform btnRect = startBtn.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.3f, 0.15f);
        btnRect.anchorMax = new Vector2(0.7f, 0.28f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        Image btnImg = startBtn.AddComponent<Image>();
        btnImg.color = new Color(0.3f, 0.7f, 0.4f, 1f); // Green button

        Button btn = startBtn.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.3f, 0.7f, 0.4f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.8f, 0.5f, 1f);
        colors.pressedColor = new Color(0.2f, 0.6f, 0.3f, 1f);
        btn.colors = colors;

        GameObject btnText = new GameObject("Text");
        btnText.transform.SetParent(startBtn.transform, false);

        RectTransform txtRect = btnText.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(10, 10);
        txtRect.offsetMax = new Vector2(-10, -10);

        TextMeshProUGUI txt = btnText.AddComponent<TextMeshProUGUI>();
        txt.text = "Draw my first plant";
        txt.fontSize = 28;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.fontStyle = FontStyles.Bold;

        return panel;
    }

    private static GameObject CreateDrawingPanel(Transform parent)
    {
        GameObject panel = new GameObject("DrawingPanel");
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Drawing Area (left side)
        GameObject drawArea = new GameObject("DrawingArea");
        drawArea.transform.SetParent(panel.transform, false);

        RectTransform areaRect = drawArea.AddComponent<RectTransform>();
        areaRect.anchorMin = new Vector2(0.05f, 0.1f);
        areaRect.anchorMax = new Vector2(0.5f, 0.9f);
        areaRect.offsetMin = Vector2.zero;
        areaRect.offsetMax = Vector2.zero;

        Image areaImg = drawArea.AddComponent<Image>();
        areaImg.color = new Color(1f, 1f, 1f, 0.3f); // Semi-transparent white

        // Border for drawing area
        GameObject border = new GameObject("Border");
        border.transform.SetParent(drawArea.transform, false);

        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;

        Outline outline = border.AddComponent<Outline>();
        outline.effectColor = new Color(0.3f, 0.6f, 0.4f, 1f);
        outline.effectDistance = new Vector2(3, 3);

        // Hint text - "press H for the guide"
        GameObject hint = new GameObject("HintText");
        hint.transform.SetParent(panel.transform, false);

        RectTransform hintRect = hint.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.05f, 0.02f);
        hintRect.anchorMax = new Vector2(0.5f, 0.08f);
        hintRect.offsetMin = Vector2.zero;
        hintRect.offsetMax = Vector2.zero;

        TextMeshProUGUI hintText = hint.AddComponent<TextMeshProUGUI>();
        hintText.text = "press H for the guide";
        hintText.fontSize = 18;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        hintText.fontStyle = FontStyles.Italic;

        // Stroke counter
        GameObject counter = new GameObject("StrokeCounter");
        counter.transform.SetParent(panel.transform, false);

        RectTransform counterRect = counter.AddComponent<RectTransform>();
        counterRect.anchorMin = new Vector2(0.05f, 0.91f);
        counterRect.anchorMax = new Vector2(0.25f, 0.97f);
        counterRect.offsetMin = Vector2.zero;
        counterRect.offsetMax = Vector2.zero;

        TextMeshProUGUI counterText = counter.AddComponent<TextMeshProUGUI>();
        counterText.text = "Strokes: 0/15";
        counterText.fontSize = 20;
        counterText.alignment = TextAlignmentOptions.Center;
        counterText.color = new Color(0.2f, 0.2f, 0.2f);

        // Finish button
        GameObject finishBtn = new GameObject("FinishButton");
        finishBtn.transform.SetParent(panel.transform, false);

        RectTransform finishRect = finishBtn.AddComponent<RectTransform>();
        finishRect.anchorMin = new Vector2(0.3f, 0.91f);
        finishRect.anchorMax = new Vector2(0.5f, 0.97f);
        finishRect.offsetMin = Vector2.zero;
        finishRect.offsetMax = Vector2.zero;

        Image finishImg = finishBtn.AddComponent<Image>();
        finishImg.color = new Color(0.9f, 0.5f, 0.2f, 1f); // Orange

        Button finishButton = finishBtn.AddComponent<Button>();
        finishButton.interactable = false; // Disabled until first stroke

        GameObject finishText = new GameObject("Text");
        finishText.transform.SetParent(finishBtn.transform, false);

        RectTransform finishTxtRect = finishText.AddComponent<RectTransform>();
        finishTxtRect.anchorMin = Vector2.zero;
        finishTxtRect.anchorMax = Vector2.one;
        finishTxtRect.offsetMin = new Vector2(5, 5);
        finishTxtRect.offsetMax = new Vector2(-5, -5);

        TextMeshProUGUI finishTxt = finishText.AddComponent<TextMeshProUGUI>();
        finishTxt.text = "Finish Drawing";
        finishTxt.fontSize = 18;
        finishTxt.alignment = TextAlignmentOptions.Center;
        finishTxt.color = Color.white;

        return panel;
    }

    private static GameObject CreateGuideButton(Transform parent)
    {
        GameObject btn = new GameObject("GuideBookButton");
        btn.transform.SetParent(parent, false);

        RectTransform rect = btn.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.85f, 0.92f);
        rect.anchorMax = new Vector2(0.98f, 0.99f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = btn.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 0.9f, 1f);
        img.raycastTarget = true;

        Button button = btn.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.9f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        colors.pressedColor = new Color(0.1f, 0.5f, 0.8f, 1f);
        button.colors = colors;

        GameObject text = new GameObject("Text");
        text.transform.SetParent(btn.transform, false);

        RectTransform txtRect = text.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(5, 5);
        txtRect.offsetMax = new Vector2(-5, -5);

        TextMeshProUGUI txt = text.AddComponent<TextMeshProUGUI>();
        txt.text = "GUIDE";
        txt.fontSize = 20;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.fontStyle = FontStyles.Bold;

        return btn;
    }

    private static GameObject CreateGuidePanel(Transform parent)
    {
        GameObject panel = new GameObject("GuideBookPanel");
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.55f, 0.1f);
        rect.anchorMax = new Vector2(0.95f, 0.9f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero; // ON-SCREEN initially

        Image img = panel.AddComponent<Image>();
        img.color = new Color(1f, 0.95f, 0.8f, 0.95f);

        // Title
        GameObject title = new GameObject("PageTitle");
        title.transform.SetParent(panel.transform, false);
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.87f);
        titleRect.anchorMax = new Vector2(0.95f, 0.97f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "Plant Guide";
        titleText.fontSize = 26;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.2f, 0.2f, 0.2f);

        // Description
        GameObject desc = new GameObject("PageDescription");
        desc.transform.SetParent(panel.transform, false);
        RectTransform descRect = desc.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.05f, 0.25f);
        descRect.anchorMax = new Vector2(0.95f, 0.85f);
        descRect.offsetMin = Vector2.zero;
        descRect.offsetMax = Vector2.zero;
        TextMeshProUGUI descText = desc.AddComponent<TextMeshProUGUI>();
        descText.text = "Welcome!";
        descText.fontSize = 18;
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.color = new Color(0.2f, 0.2f, 0.2f);

        // Page number
        GameObject pageNum = new GameObject("PageNumber");
        pageNum.transform.SetParent(panel.transform, false);
        RectTransform pageRect = pageNum.AddComponent<RectTransform>();
        pageRect.anchorMin = new Vector2(0.35f, 0.02f);
        pageRect.anchorMax = new Vector2(0.65f, 0.08f);
        pageRect.offsetMin = Vector2.zero;
        pageRect.offsetMax = Vector2.zero;
        TextMeshProUGUI pageText = pageNum.AddComponent<TextMeshProUGUI>();
        pageText.text = "Page 1 / 5";
        pageText.fontSize = 14;
        pageText.alignment = TextAlignmentOptions.Center;
        pageText.color = new Color(0.4f, 0.4f, 0.4f);

        // Buttons
        CreatePanelButton("CloseButton", panel.transform, new Vector2(0.88f, 0.92f), new Vector2(0.97f, 0.98f), "X", 24);
        CreatePanelButton("PreviousButton", panel.transform, new Vector2(0.05f, 0.12f), new Vector2(0.30f, 0.21f), "< Prev", 16);
        CreatePanelButton("NextButton", panel.transform, new Vector2(0.70f, 0.12f), new Vector2(0.95f, 0.21f), "Next >", 16);

        panel.SetActive(true); // Active so PlantGuideBook.Start() can position it

        return panel;
    }

    private static GameObject CreatePanelButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string text, int fontSize)
    {
        GameObject btn = new GameObject(name);
        btn.transform.SetParent(parent, false);

        RectTransform rect = btn.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = btn.AddComponent<Image>();
        img.color = new Color(0.3f, 0.6f, 0.9f, 1f);

        Button button = btn.AddComponent<Button>();

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btn.transform, false);

        RectTransform txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(5, 5);
        txtRect.offsetMax = new Vector2(-5, -5);

        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = text;
        txt.fontSize = fontSize;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;

        return btn;
    }

    private static void CreateLineRendererPrefab()
    {
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string path = "Assets/Prefabs/LineRenderer.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            GameObject obj = new GameObject("LineRenderer");
            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.black;
            lr.endColor = Color.black;
            lr.positionCount = 0;
            lr.useWorldSpace = true;
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
        }
    }

    private static int WireAllComponents()
    {
        Debug.Log("\n▶ Wiring components...");
        int wired = 0;

        PlantGuideBook guideBook = Object.FindFirstObjectByType<PlantGuideBook>();
        DrawingCanvas drawingCanvas = Object.FindFirstObjectByType<DrawingCanvas>();
        DrawingSceneUI sceneUI = Object.FindFirstObjectByType<DrawingSceneUI>();

        // Wire PlantGuideBook
        if (guideBook != null)
        {
            GameObject panel = GameObject.Find("GuideBookPanel");
            GameObject btn = GameObject.Find("GuideBookButton");

            if (panel != null)
            {
                guideBook.bookPanel = panel;
                guideBook.pageTitle = panel.transform.Find("PageTitle")?.GetComponent<TextMeshProUGUI>();
                guideBook.pageDescription = panel.transform.Find("PageDescription")?.GetComponent<TextMeshProUGUI>();
                guideBook.pageNumberText = panel.transform.Find("PageNumber")?.GetComponent<TextMeshProUGUI>();
                guideBook.closeBookButton = panel.transform.Find("CloseButton")?.GetComponent<Button>();
                guideBook.nextPageButton = panel.transform.Find("NextButton")?.GetComponent<Button>();
                guideBook.previousPageButton = panel.transform.Find("PreviousButton")?.GetComponent<Button>();
                wired++;
            }

            if (btn != null)
            {
                Button button = btn.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => guideBook.OpenBook());
                guideBook.openBookButton = button;
                wired++;
            }

            EditorUtility.SetDirty(guideBook);
            Debug.Log("   ✓ PlantGuideBook");
        }

        // Wire DrawingCanvas
        if (drawingCanvas != null)
        {
            GameObject drawArea = GameObject.Find("DrawingArea");
            if (drawArea != null)
            {
                drawingCanvas.drawingArea = drawArea.GetComponent<RectTransform>();
                wired++;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/LineRenderer.prefab");
            if (prefab != null)
            {
                drawingCanvas.lineRendererPrefab = prefab.GetComponent<LineRenderer>();
                wired++;
            }

            Transform container = drawingCanvas.transform.Find("StrokeContainer");
            if (container == null)
            {
                GameObject cont = new GameObject("StrokeContainer");
                cont.transform.SetParent(drawingCanvas.transform, false);
                container = cont.transform;
            }
            drawingCanvas.strokeContainer = container;

            // Wire stroke counter and finish button
            GameObject counter = GameObject.Find("StrokeCounter");
            if (counter != null)
            {
                drawingCanvas.strokeCountText = counter.GetComponent<TextMeshProUGUI>();
                wired++;
            }

            GameObject finishBtn = GameObject.Find("FinishButton");
            if (finishBtn != null)
            {
                drawingCanvas.finishButton = finishBtn.GetComponent<Button>();
                wired++;
            }

            EditorUtility.SetDirty(drawingCanvas);
            Debug.Log("   ✓ DrawingCanvas");
        }

        // Wire DrawingSceneUI
        if (sceneUI != null)
        {
            GameObject instrPanel = GameObject.Find("InstructionsPanel");
            GameObject drawPanel = GameObject.Find("DrawingPanel");
            GameObject startBtn = GameObject.Find("StartDrawingButton");

            if (instrPanel != null)
            {
                sceneUI.instructionsPanel = instrPanel;
                wired++;
            }

            if (drawPanel != null)
            {
                sceneUI.drawingPanel = drawPanel;
                wired++;
            }

            if (startBtn != null)
            {
                Button button = startBtn.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => sceneUI.OnStartDrawing());
                sceneUI.startDrawingButton = button;
                wired++;
            }

            EditorUtility.SetDirty(sceneUI);
            Debug.Log("   ✓ DrawingSceneUI");
        }

        Debug.Log($"   ✅ Wired {wired} references");
        return wired;
    }
}
