using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// COMPLETE Drawing Scene cleanup and rebuild tool
/// Removes ALL duplicates and rebuilds the entire UI from scratch
/// Run from: Tools > Sketch Blossom > RESET and Rebuild Drawing Scene
/// </summary>
public class DrawingSceneCompleteRebuild : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/RESET and Rebuild Drawing Scene", priority = 0)]
    public static void ResetAndRebuild()
    {
        if (!EditorUtility.DisplayDialog(
            "Reset Drawing Scene?",
            "This will DELETE all existing UI elements and rebuild from scratch.\n\n" +
            "This includes:\n" +
            "- All GuideBookPanels\n" +
            "- All GuideBookButtons\n" +
            "- All DrawingAreas\n" +
            "- All Backgrounds\n\n" +
            "Your components (PlantGuideBook, DrawingCanvas, etc) will NOT be deleted.\n\n" +
            "Continue?",
            "Yes, Reset Everything",
            "Cancel"))
        {
            return;
        }

        Debug.Log("════════════════════════════════════════════════════");
        Debug.Log("RESETTING AND REBUILDING DRAWING SCENE");
        Debug.Log("════════════════════════════════════════════════════");

        // Step 1: Clean up all duplicates and old UI
        CleanupAllDuplicates();

        // Step 2: Create fresh UI elements
        int created = CreateFreshUI();

        // Step 3: Wire everything together
        int wired = WireAllConnections();

        Debug.Log("════════════════════════════════════════════════════");
        Debug.Log($"REBUILD COMPLETE! Created {created} elements, wired {wired} connections");
        Debug.Log("════════════════════════════════════════════════════");
        Debug.Log("\n✅ Drawing Scene is now clean and ready to use!");
        Debug.Log("Press Play to test!");

        EditorUtility.DisplayDialog(
            "Scene Rebuilt!",
            $"Successfully rebuilt Drawing Scene!\n\n" +
            $"Created: {created} UI elements\n" +
            $"Connected: {wired} references\n\n" +
            "All duplicates removed.\n" +
            "Everything is wired and ready.\n\n" +
            "Press Play to test!",
            "OK"
        );
    }

    private static void CleanupAllDuplicates()
    {
        Debug.Log("\n▶ STEP 1: Cleaning up all duplicates and old UI...");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas found in scene!");
            return;
        }

        int removed = 0;

        // Remove all GuideBookPanels
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        List<GameObject> toDelete = new List<GameObject>();

        foreach (var obj in allObjects)
        {
            string name = obj.name.ToLower();

            // Mark UI elements for deletion (but not components)
            if (name.Contains("guidebookpanel") || name.Contains("bookpanel") ||
                name.Contains("guidebookbutton") ||
                (name.Contains("guide") && name.Contains("button") && obj.GetComponent<Button>() != null) ||
                name == "drawingarea" ||
                (name == "background" && obj.transform.parent == canvas.transform))
            {
                toDelete.Add(obj);
            }
        }

        // Delete marked objects
        foreach (var obj in toDelete)
        {
            Debug.Log($"   Removing: {obj.name}");
            Object.DestroyImmediate(obj);
            removed++;
        }

        Debug.Log($"   ✅ Removed {removed} duplicate/old UI elements");
    }

    private static int CreateFreshUI()
    {
        Debug.Log("\n▶ STEP 2: Creating fresh UI elements...");
        int created = 0;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas found!");
            return 0;
        }

        // 1. Create Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(canvas.transform, false);
        background.transform.SetAsFirstSibling();

        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.82f, 0.94f, 0.82f, 1f);

        Debug.Log("   ✓ Created Background");
        created++;

        // 2. Create DrawingArea
        GameObject drawingArea = new GameObject("DrawingArea");
        drawingArea.transform.SetParent(canvas.transform, false);

        RectTransform daRect = drawingArea.AddComponent<RectTransform>();
        daRect.anchorMin = new Vector2(0.05f, 0.1f);
        daRect.anchorMax = new Vector2(0.5f, 0.9f);
        daRect.offsetMin = Vector2.zero;
        daRect.offsetMax = Vector2.zero;

        Image daImage = drawingArea.AddComponent<Image>();
        daImage.color = new Color(1f, 1f, 1f, 0.1f);

        Debug.Log("   ✓ Created DrawingArea");
        created++;

        // 3. Create GuideBookButton
        GameObject guideButton = new GameObject("GuideBookButton");
        guideButton.transform.SetParent(canvas.transform, false);

        RectTransform btnRect = guideButton.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.85f, 0.92f);
        btnRect.anchorMax = new Vector2(0.98f, 0.99f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        Image btnImage = guideButton.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.6f, 0.9f, 1f);
        btnImage.raycastTarget = true;

        Button btn = guideButton.AddComponent<Button>();

        // Set button colors
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.9f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        colors.pressedColor = new Color(0.1f, 0.5f, 0.8f, 1f);
        btn.colors = colors;

        GameObject btnText = new GameObject("Text");
        btnText.transform.SetParent(guideButton.transform, false);

        RectTransform txtRect = btnText.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(5, 5);
        txtRect.offsetMax = new Vector2(-5, -5);

        TextMeshProUGUI tmpText = btnText.AddComponent<TextMeshProUGUI>();
        tmpText.text = "GUIDE";
        tmpText.fontSize = 20;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;
        tmpText.fontStyle = FontStyles.Bold;

        Debug.Log("   ✓ Created GuideBookButton");
        created++;

        // 4. Create GuideBookPanel
        GameObject panel = new GameObject("GuideBookPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.55f, 0.1f);
        panelRect.anchorMax = new Vector2(0.95f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(1f, 0.95f, 0.8f, 0.95f);

        // Page Title
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

        // Page Description
        GameObject desc = new GameObject("PageDescription");
        desc.transform.SetParent(panel.transform, false);
        RectTransform descRect = desc.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.05f, 0.25f);
        descRect.anchorMax = new Vector2(0.95f, 0.85f);
        descRect.offsetMin = Vector2.zero;
        descRect.offsetMax = Vector2.zero;

        TextMeshProUGUI descText = desc.AddComponent<TextMeshProUGUI>();
        descText.text = "Welcome to the Plant Guide!";
        descText.fontSize = 18;
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.color = new Color(0.2f, 0.2f, 0.2f);

        // Page Number
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

        // Close Button
        CreatePanelButton("CloseButton", panel.transform,
            new Vector2(0.88f, 0.92f), new Vector2(0.97f, 0.98f), "X", 24);

        // Previous Button
        CreatePanelButton("PreviousButton", panel.transform,
            new Vector2(0.05f, 0.12f), new Vector2(0.30f, 0.21f), "Previous", 16);

        // Next Button
        CreatePanelButton("NextButton", panel.transform,
            new Vector2(0.70f, 0.12f), new Vector2(0.95f, 0.21f), "Next", 16);

        // Start panel off-screen and inactive
        panelRect.anchoredPosition = new Vector2(2000f, 0f);
        panel.SetActive(false);

        Debug.Log("   ✓ Created GuideBookPanel with all children");
        created++;

        // 5. Ensure LineRenderer Prefab exists
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string prefabPath = "Assets/Prefabs/LineRenderer.prefab";
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (existingPrefab == null)
        {
            GameObject lineObj = new GameObject("LineRenderer");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.black;
            lr.endColor = Color.black;
            lr.positionCount = 0;
            lr.useWorldSpace = true;

            PrefabUtility.SaveAsPrefabAsset(lineObj, prefabPath);
            Object.DestroyImmediate(lineObj);

            Debug.Log("   ✓ Created LineRenderer prefab");
            created++;
        }

        // 6. Ensure EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("   ✓ Created EventSystem");
            created++;
        }

        Debug.Log($"   ✅ Created {created} fresh UI elements");
        return created;
    }

    private static GameObject CreatePanelButton(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, string text, int fontSize)
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
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);

        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;

        return btnObj;
    }

    private static int WireAllConnections()
    {
        Debug.Log("\n▶ STEP 3: Wiring all connections...");
        int wired = 0;

        // Find components
        PlantGuideBook guideBook = Object.FindFirstObjectByType<PlantGuideBook>();
        DrawingCanvas drawingCanvas = Object.FindFirstObjectByType<DrawingCanvas>();
        DrawingSceneUI sceneUI = Object.FindFirstObjectByType<DrawingSceneUI>();

        // Wire PlantGuideBook
        if (guideBook != null)
        {
            GameObject panel = GameObject.Find("GuideBookPanel");
            GameObject openBtn = GameObject.Find("GuideBookButton");

            if (panel != null)
            {
                guideBook.bookPanel = panel;

                // Find and wire children
                foreach (Transform child in panel.transform)
                {
                    if (child.name == "PageTitle")
                        guideBook.pageTitle = child.GetComponent<TextMeshProUGUI>();
                    else if (child.name == "PageDescription")
                        guideBook.pageDescription = child.GetComponent<TextMeshProUGUI>();
                    else if (child.name == "PageNumber")
                        guideBook.pageNumberText = child.GetComponent<TextMeshProUGUI>();
                    else if (child.name == "CloseButton")
                        guideBook.closeBookButton = child.GetComponent<Button>();
                    else if (child.name == "NextButton")
                        guideBook.nextPageButton = child.GetComponent<Button>();
                    else if (child.name == "PreviousButton")
                        guideBook.previousPageButton = child.GetComponent<Button>();
                }

                Debug.Log("   ✓ Wired GuideBookPanel references");
                wired++;
            }

            if (openBtn != null)
            {
                Button btn = openBtn.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    Debug.Log("Guide Book button clicked!");
                    guideBook.OpenBook();
                });

                guideBook.openBookButton = btn;
                Debug.Log("   ✓ Wired GuideBookButton");
                wired++;
            }

            EditorUtility.SetDirty(guideBook);
        }

        // Wire DrawingCanvas
        if (drawingCanvas != null)
        {
            GameObject drawingArea = GameObject.Find("DrawingArea");
            if (drawingArea != null)
            {
                drawingCanvas.drawingArea = drawingArea.GetComponent<RectTransform>();
                Debug.Log("   ✓ Wired DrawingArea");
                wired++;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/LineRenderer.prefab");
            if (prefab != null)
            {
                drawingCanvas.lineRendererPrefab = prefab.GetComponent<LineRenderer>();
                Debug.Log("   ✓ Wired LineRenderer prefab");
                wired++;
            }

            // Ensure stroke container
            Transform container = drawingCanvas.transform.Find("StrokeContainer");
            if (container == null)
            {
                GameObject containerObj = new GameObject("StrokeContainer");
                containerObj.transform.SetParent(drawingCanvas.transform, false);
                container = containerObj.transform;
            }
            drawingCanvas.strokeContainer = container;

            EditorUtility.SetDirty(drawingCanvas);
        }

        // Wire DrawingSceneUI
        if (sceneUI != null)
        {
            Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var btn in allButtons)
            {
                if (btn.gameObject.name.ToLower().Contains("start"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("Start Drawing clicked!");
                        sceneUI.OnStartDrawing();
                    });

                    sceneUI.startDrawingButton = btn;
                    Debug.Log("   ✓ Wired Start Drawing button");
                    wired++;
                    break;
                }
            }

            EditorUtility.SetDirty(sceneUI);
        }

        Debug.Log($"   ✅ Wired {wired} connections");
        return wired;
    }
}
