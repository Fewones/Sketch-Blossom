using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// Fixes ALL NULL reference errors in Drawing Scene
/// Automatically creates missing UI elements, prefabs, and connects references
/// Run from: Tools > Sketch Blossom > Fix ALL Drawing Scene References
/// </summary>
public class DrawingSceneReferencesFixer : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Fix ALL Drawing Scene References", priority = 1)]
    public static void FixAllReferences()
    {
        Debug.Log("════════════════════════════════════════════════════");
        Debug.Log("FIXING ALL DRAWING SCENE REFERENCES");
        Debug.Log("════════════════════════════════════════════════════");

        int totalFixes = 0;

        // Fix PlantGuideBook references
        totalFixes += FixPlantGuideBookReferences();

        // Fix DrawingCanvas references
        totalFixes += FixDrawingCanvasReferences();

        // Fix DrawingSceneUI references
        totalFixes += FixDrawingSceneUIReferences();

        // Ensure EventSystem
        totalFixes += EnsureEventSystem();

        // Apply background
        totalFixes += ApplyBackground();

        Debug.Log("════════════════════════════════════════════════════");
        Debug.Log($"COMPLETE! Fixed {totalFixes} references");
        Debug.Log("════════════════════════════════════════════════════");

        EditorUtility.DisplayDialog(
            "All References Fixed!",
            $"Successfully fixed {totalFixes} references!\n\n" +
            "✅ PlantGuideBook references fixed\n" +
            "✅ DrawingCanvas references fixed\n" +
            "✅ DrawingSceneUI references fixed\n" +
            "✅ All UI elements created\n\n" +
            "Press Play to test the scene!",
            "OK"
        );
    }

    private static int FixPlantGuideBookReferences()
    {
        Debug.Log("\n▶ Fixing PlantGuideBook References...");
        int fixes = 0;

        PlantGuideBook guideBook = Object.FindFirstObjectByType<PlantGuideBook>();
        if (guideBook == null)
        {
            Debug.LogError("❌ PlantGuideBook component not found in scene!");
            return 0;
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas not found!");
            return 0;
        }

        // Fix 1: Create/Find Guide Book Panel
        if (guideBook.bookPanel == null)
        {
            GameObject panel = GameObject.Find("GuideBookPanel");
            if (panel == null) panel = GameObject.Find("BookPanel");

            if (panel == null)
            {
                Debug.Log("   Creating GuideBookPanel...");
                panel = CreateGuideBookPanel(canvas.transform);
                fixes++;
            }

            guideBook.bookPanel = panel;

            // Connect all child references
            TextMeshProUGUI[] texts = panel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name.Contains("Title") && guideBook.pageTitle == null)
                {
                    guideBook.pageTitle = text;
                    Debug.Log("   ✓ Connected pageTitle");
                }
                else if (text.gameObject.name.Contains("Description") && guideBook.pageDescription == null)
                {
                    guideBook.pageDescription = text;
                    Debug.Log("   ✓ Connected pageDescription");
                }
                else if (text.gameObject.name.Contains("Number") && guideBook.pageNumberText == null)
                {
                    guideBook.pageNumberText = text;
                    Debug.Log("   ✓ Connected pageNumberText");
                }
            }

            Button[] buttons = panel.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn.gameObject.name.Contains("Close") && guideBook.closeBookButton == null)
                {
                    guideBook.closeBookButton = btn;
                    Debug.Log("   ✓ Connected closeBookButton");
                }
                else if (btn.gameObject.name.Contains("Next") && guideBook.nextPageButton == null)
                {
                    guideBook.nextPageButton = btn;
                    Debug.Log("   ✓ Connected nextPageButton");
                }
                else if ((btn.gameObject.name.Contains("Previous") || btn.gameObject.name.Contains("Prev"))
                         && guideBook.previousPageButton == null)
                {
                    guideBook.previousPageButton = btn;
                    Debug.Log("   ✓ Connected previousPageButton");
                }
            }

            Debug.Log("   ✅ GuideBookPanel fixed");
            fixes++;
        }

        // Fix 2: Create/Find Guide Book Button
        if (guideBook.openBookButton == null)
        {
            Button openButton = null;

            // Search for existing button
            Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var btn in allButtons)
            {
                if (btn.gameObject.name.ToLower().Contains("guide") || btn.gameObject.name.ToLower().Contains("book"))
                {
                    // Check it's not a child of the guide panel
                    if (guideBook.bookPanel == null || !btn.transform.IsChildOf(guideBook.bookPanel.transform))
                    {
                        openButton = btn;
                        Debug.Log($"   Found existing Guide button: {btn.gameObject.name}");
                        break;
                    }
                }
            }

            if (openButton == null)
            {
                Debug.Log("   Creating GuideBookButton...");
                openButton = CreateGuideBookButton(canvas.transform);
                fixes++;
            }

            // Connect button
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(() => {
                Debug.Log("Guide Book button clicked!");
                guideBook.OpenBook();
            });

            guideBook.openBookButton = openButton;
            openButton.transform.SetAsLastSibling();

            Debug.Log("   ✅ GuideBookButton fixed");
            fixes++;
        }

        EditorUtility.SetDirty(guideBook);
        return fixes;
    }

    private static int FixDrawingCanvasReferences()
    {
        Debug.Log("\n▶ Fixing DrawingCanvas References...");
        int fixes = 0;

        DrawingCanvas canvas = Object.FindFirstObjectByType<DrawingCanvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ DrawingCanvas component not found in scene!");
            return 0;
        }

        // Fix 1: Drawing Area
        if (canvas.drawingArea == null)
        {
            // Try to find existing drawing area
            GameObject drawingAreaObj = GameObject.Find("DrawingArea");
            if (drawingAreaObj == null)
            {
                // Search for any RectTransform with "draw" in the name
                RectTransform[] allRects = Object.FindObjectsOfType<RectTransform>();
                foreach (var rect in allRects)
                {
                    if (rect.gameObject.name.ToLower().Contains("draw") &&
                        rect.gameObject.name.ToLower().Contains("area"))
                    {
                        drawingAreaObj = rect.gameObject;
                        break;
                    }
                }
            }

            if (drawingAreaObj == null)
            {
                Debug.Log("   Creating DrawingArea...");
                drawingAreaObj = CreateDrawingArea();
                fixes++;
            }

            canvas.drawingArea = drawingAreaObj.GetComponent<RectTransform>();
            Debug.Log("   ✅ DrawingArea fixed");
            fixes++;
        }

        // Fix 2: LineRenderer Prefab
        if (canvas.lineRendererPrefab == null)
        {
            // Try to find existing prefab
            string[] guids = AssetDatabase.FindAssets("LineRenderer t:Prefab");
            LineRenderer prefab = null;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (obj != null && obj.GetComponent<LineRenderer>() != null)
                {
                    prefab = obj.GetComponent<LineRenderer>();
                    Debug.Log($"   Found existing LineRenderer prefab: {path}");
                    break;
                }
            }

            if (prefab == null)
            {
                Debug.Log("   Creating LineRenderer prefab...");
                prefab = CreateLineRendererPrefab();
                fixes++;
            }

            canvas.lineRendererPrefab = prefab;
            Debug.Log("   ✅ LineRenderer prefab fixed");
            fixes++;
        }

        // Fix 3: Stroke Container
        if (canvas.strokeContainer == null)
        {
            Transform container = canvas.transform.Find("StrokeContainer");
            if (container == null)
            {
                GameObject containerObj = new GameObject("StrokeContainer");
                containerObj.transform.SetParent(canvas.transform, false);
                container = containerObj.transform;
                Debug.Log("   ✓ Created StrokeContainer");
            }
            canvas.strokeContainer = container;
            fixes++;
        }

        EditorUtility.SetDirty(canvas);
        return fixes;
    }

    private static int FixDrawingSceneUIReferences()
    {
        Debug.Log("\n▶ Fixing DrawingSceneUI References...");
        int fixes = 0;

        DrawingSceneUI uiManager = Object.FindFirstObjectByType<DrawingSceneUI>();
        if (uiManager == null)
        {
            Debug.LogWarning("   DrawingSceneUI not found - skipping");
            return 0;
        }

        // Find Start button
        if (uiManager.startDrawingButton == null)
        {
            Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var btn in allButtons)
            {
                if (btn.gameObject.name.ToLower().Contains("start"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => uiManager.OnStartDrawing());
                    uiManager.startDrawingButton = btn;
                    Debug.Log("   ✓ Connected Start Drawing button");
                    fixes++;
                    break;
                }
            }
        }

        EditorUtility.SetDirty(uiManager);
        return fixes;
    }

    private static GameObject CreateGuideBookPanel(Transform parent)
    {
        GameObject panel = new GameObject("GuideBookPanel");
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.55f, 0.1f);
        panelRect.anchorMax = new Vector2(0.95f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(1f, 0.95f, 0.8f, 0.95f);

        // Page Title
        GameObject titleObj = new GameObject("PageTitle");
        titleObj.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.85f);
        titleRect.anchorMax = new Vector2(0.95f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Plant Guide";
        titleText.fontSize = 28;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.2f, 0.2f, 0.2f);

        // Page Description
        GameObject descObj = new GameObject("PageDescription");
        descObj.transform.SetParent(panel.transform, false);
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.05f, 0.25f);
        descRect.anchorMax = new Vector2(0.95f, 0.8f);
        descRect.offsetMin = Vector2.zero;
        descRect.offsetMax = Vector2.zero;
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = "Learn how to draw plants!";
        descText.fontSize = 18;
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.color = new Color(0.2f, 0.2f, 0.2f);

        // Page Number
        GameObject pageNumObj = new GameObject("PageNumber");
        pageNumObj.transform.SetParent(panel.transform, false);
        RectTransform pageNumRect = pageNumObj.AddComponent<RectTransform>();
        pageNumRect.anchorMin = new Vector2(0.4f, 0.02f);
        pageNumRect.anchorMax = new Vector2(0.6f, 0.08f);
        pageNumRect.offsetMin = Vector2.zero;
        pageNumRect.offsetMax = Vector2.zero;
        TextMeshProUGUI pageNumText = pageNumObj.AddComponent<TextMeshProUGUI>();
        pageNumText.text = "Page 1 / 5";
        pageNumText.fontSize = 16;
        pageNumText.alignment = TextAlignmentOptions.Center;
        pageNumText.color = new Color(0.4f, 0.4f, 0.4f);

        // Buttons
        CreateSimpleButton("CloseButton", panel.transform,
            new Vector2(0.85f, 0.9f), new Vector2(0.95f, 0.98f), "X");
        CreateSimpleButton("PreviousButton", panel.transform,
            new Vector2(0.05f, 0.1f), new Vector2(0.25f, 0.2f), "< Prev");
        CreateSimpleButton("NextButton", panel.transform,
            new Vector2(0.75f, 0.1f), new Vector2(0.95f, 0.2f), "Next >");

        // Start with panel closed (off-screen)
        panelRect.anchoredPosition = new Vector2(2000f, panelRect.anchoredPosition.y);
        panel.SetActive(false);

        return panel;
    }

    private static Button CreateGuideBookButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("GuideBookButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.85f, 0.9f);
        rect.anchorMax = new Vector2(0.98f, 0.98f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 0.9f, 1f);
        img.raycastTarget = true;

        Button button = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "GUIDE";
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;

        return button;
    }

    private static GameObject CreateDrawingArea()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return null;

        GameObject drawingAreaObj = new GameObject("DrawingArea");
        drawingAreaObj.transform.SetParent(canvas.transform, false);

        RectTransform rect = drawingAreaObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.1f);
        rect.anchorMax = new Vector2(0.6f, 0.9f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = drawingAreaObj.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.1f); // Very transparent white

        return drawingAreaObj;
    }

    private static LineRenderer CreateLineRendererPrefab()
    {
        // Create prefab directory if it doesn't exist
        string prefabDir = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabDir))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Create GameObject with LineRenderer
        GameObject lineObj = new GameObject("LineRendererPrefab");
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

        // Configure LineRenderer
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;

        // Save as prefab
        string prefabPath = $"{prefabDir}/LineRenderer.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(lineObj, prefabPath);

        // Destroy the temporary GameObject
        Object.DestroyImmediate(lineObj);

        Debug.Log($"   Created LineRenderer prefab at: {prefabPath}");
        return prefab.GetComponent<LineRenderer>();
    }

    private static GameObject CreateSimpleButton(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, string text)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.6f, 0.9f, 1f);

        Button btn = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = 18;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;

        return btnObj;
    }

    private static int ApplyBackground()
    {
        Debug.Log("\n▶ Applying Background...");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return 0;

        Transform existingBg = canvas.transform.Find("Background");
        if (existingBg != null)
        {
            Debug.Log("   Background already exists");
            return 0;
        }

        GameObject background = new GameObject("Background");
        background.transform.SetParent(canvas.transform, false);
        background.transform.SetAsFirstSibling();

        RectTransform rect = background.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = background.AddComponent<Image>();
        image.color = new Color(0.82f, 0.94f, 0.82f, 1f); // Soft sage green

        Debug.Log("   ✅ Applied background");
        return 1;
    }

    private static int EnsureEventSystem()
    {
        Debug.Log("\n▶ Ensuring EventSystem...");

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("   ✅ Created EventSystem");
            return 1;
        }

        Debug.Log("   EventSystem already exists");
        return 0;
    }
}
