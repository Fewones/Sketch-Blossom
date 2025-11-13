using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ULTIMATE Drawing Scene fixer - deletes EVERYTHING and rebuilds from scratch
/// This is the nuclear option that WILL fix your scene
/// Run from: Tools > Sketch Blossom > NUCLEAR OPTION - Fix Drawing Scene
/// </summary>
public class DrawingSceneNuclearFix : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/NUCLEAR OPTION - Fix Drawing Scene", priority = 0)]
    public static void NuclearFix()
    {
        if (!EditorUtility.DisplayDialog(
            "NUCLEAR OPTION - Complete Reset?",
            "This will DELETE and REBUILD the ENTIRE Drawing Scene UI.\n\n" +
            "DELETES:\n" +
            "- ALL panels (Guide, Drawing, Instructions, etc.)\n" +
            "- ALL buttons\n" +
            "- ALL backgrounds\n" +
            "- ALL drawing areas\n\n" +
            "KEEPS SAFE:\n" +
            "- Your components (PlantGuideBook, DrawingCanvas, etc.)\n" +
            "- Your scripts\n\n" +
            "This WILL fix your scene. Continue?",
            "YES - Nuclear Fix",
            "Cancel"))
        {
            return;
        }

        Debug.Log("╔════════════════════════════════════════════════════╗");
        Debug.Log("║     NUCLEAR OPTION - COMPLETE SCENE REBUILD        ║");
        Debug.Log("╚════════════════════════════════════════════════════╝");

        // Nuclear cleanup
        int deleted = NuclearCleanup();

        // Rebuild from scratch
        int created = RebuildFromScratch();

        // Wire everything
        int wired = WireEverything();

        Debug.Log("╔════════════════════════════════════════════════════╗");
        Debug.Log($"║  COMPLETE! Deleted: {deleted}, Created: {created}, Wired: {wired}");
        Debug.Log("╚════════════════════════════════════════════════════╝");

        EditorUtility.DisplayDialog(
            "Scene Fixed!",
            $"Nuclear fix complete!\n\n" +
            $"Deleted: {deleted} broken elements\n" +
            $"Created: {created} fresh elements\n" +
            $"Wired: {wired} connections\n\n" +
            "Your Drawing Scene is now CLEAN and WORKING!\n\n" +
            "Press Play to test!",
            "OK"
        );
    }

    private static int NuclearCleanup()
    {
        Debug.Log("\n▶ NUCLEAR CLEANUP - Deleting ALL UI elements...");
        int deleted = 0;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas found!");
            return 0;
        }

        // Get all children of Canvas
        List<GameObject> toDelete = new List<GameObject>();

        foreach (Transform child in canvas.transform)
        {
            string name = child.name.ToLower();

            // Delete ANY panel, button, background, or drawing area
            // But NOT the Canvas itself or EventSystem
            if (name.Contains("panel") ||
                name.Contains("button") ||
                name.Contains("background") ||
                name.Contains("area") ||
                name.Contains("guidebook") ||
                name.Contains("guide") ||
                name.Contains("book") ||
                name.Contains("drawing") ||
                name.Contains("instructions"))
            {
                toDelete.Add(child.gameObject);
            }
        }

        // Delete
        foreach (var obj in toDelete)
        {
            Debug.Log($"   🗑️ Deleting: {obj.name}");
            Object.DestroyImmediate(obj);
            deleted++;
        }

        Debug.Log($"   ✅ Deleted {deleted} UI elements");
        return deleted;
    }

    private static int RebuildFromScratch()
    {
        Debug.Log("\n▶ REBUILDING - Creating fresh UI from scratch...");
        int created = 0;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas!");
            return 0;
        }

        // 1. Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvas.transform, false);
        bg.transform.SetAsFirstSibling();
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.82f, 0.94f, 0.82f, 1f);
        Debug.Log("   ✓ Created Background");
        created++;

        // 2. DrawingArea
        GameObject drawArea = new GameObject("DrawingArea");
        drawArea.transform.SetParent(canvas.transform, false);
        RectTransform daRect = drawArea.AddComponent<RectTransform>();
        daRect.anchorMin = new Vector2(0.05f, 0.1f);
        daRect.anchorMax = new Vector2(0.5f, 0.9f);
        daRect.offsetMin = Vector2.zero;
        daRect.offsetMax = Vector2.zero;
        Image daImg = drawArea.AddComponent<Image>();
        daImg.color = new Color(1f, 1f, 1f, 0.1f);
        Debug.Log("   ✓ Created DrawingArea");
        created++;

        // 3. GuideBookButton
        GameObject guideBtn = new GameObject("GuideBookButton");
        guideBtn.transform.SetParent(canvas.transform, false);
        RectTransform btnRect = guideBtn.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.85f, 0.92f);
        btnRect.anchorMax = new Vector2(0.98f, 0.99f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        Image btnImg = guideBtn.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.9f, 1f);
        btnImg.raycastTarget = true;

        Button btn = guideBtn.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.9f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        colors.pressedColor = new Color(0.1f, 0.5f, 0.8f, 1f);
        btn.colors = colors;

        GameObject btnTxt = new GameObject("Text");
        btnTxt.transform.SetParent(guideBtn.transform, false);
        RectTransform txtRect = btnTxt.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(5, 5);
        txtRect.offsetMax = new Vector2(-5, -5);
        TextMeshProUGUI txt = btnTxt.AddComponent<TextMeshProUGUI>();
        txt.text = "GUIDE";
        txt.fontSize = 20;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.fontStyle = FontStyles.Bold;
        Debug.Log("   ✓ Created GuideBookButton");
        created++;

        // 4. GuideBookPanel - IMPORTANT: Position it ON-SCREEN initially!
        // PlantGuideBook.Start() will move it off-screen
        GameObject panel = new GameObject("GuideBookPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        // Position it on the right side of the screen (on-screen, not off-screen!)
        panelRect.anchorMin = new Vector2(0.55f, 0.1f);
        panelRect.anchorMax = new Vector2(0.95f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        // Set the anchored position to (0, 0) so PlantGuideBook captures the correct open position
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(1f, 0.95f, 0.8f, 0.95f);

        // Title
        GameObject title = new GameObject("PageTitle");
        title.transform.SetParent(panel.transform, false);
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.87f);
        titleRect.anchorMax = new Vector2(0.95f, 0.97f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        TextMeshProUGUI titleTxt = title.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "Plant Guide";
        titleTxt.fontSize = 26;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.color = new Color(0.2f, 0.2f, 0.2f);

        // Description
        GameObject desc = new GameObject("PageDescription");
        desc.transform.SetParent(panel.transform, false);
        RectTransform descRect = desc.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.05f, 0.25f);
        descRect.anchorMax = new Vector2(0.95f, 0.85f);
        descRect.offsetMin = Vector2.zero;
        descRect.offsetMax = Vector2.zero;
        TextMeshProUGUI descTxt = desc.AddComponent<TextMeshProUGUI>();
        descTxt.text = "Welcome!";
        descTxt.fontSize = 18;
        descTxt.alignment = TextAlignmentOptions.TopLeft;
        descTxt.color = new Color(0.2f, 0.2f, 0.2f);

        // Page Number
        GameObject pageNum = new GameObject("PageNumber");
        pageNum.transform.SetParent(panel.transform, false);
        RectTransform pageRect = pageNum.AddComponent<RectTransform>();
        pageRect.anchorMin = new Vector2(0.35f, 0.02f);
        pageRect.anchorMax = new Vector2(0.65f, 0.08f);
        pageRect.offsetMin = Vector2.zero;
        pageRect.offsetMax = Vector2.zero;
        TextMeshProUGUI pageTxt = pageNum.AddComponent<TextMeshProUGUI>();
        pageTxt.text = "Page 1 / 5";
        pageTxt.fontSize = 14;
        pageTxt.alignment = TextAlignmentOptions.Center;
        pageTxt.color = new Color(0.4f, 0.4f, 0.4f);

        // Buttons
        CreateButton("CloseButton", panel.transform, new Vector2(0.88f, 0.92f), new Vector2(0.97f, 0.98f), "X", 24);
        CreateButton("PreviousButton", panel.transform, new Vector2(0.05f, 0.12f), new Vector2(0.30f, 0.21f), "< Prev", 16);
        CreateButton("NextButton", panel.transform, new Vector2(0.70f, 0.12f), new Vector2(0.95f, 0.21f), "Next >", 16);

        // Panel stays ACTIVE - PlantGuideBook.Start() will handle hiding it
        panel.SetActive(true);

        Debug.Log("   ✓ Created GuideBookPanel (positioned ON-SCREEN for PlantGuideBook.Start())");
        created++;

        // 5. LineRenderer Prefab
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string prefabPath = "Assets/Prefabs/LineRenderer.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
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
            Debug.Log("   ✓ Created LineRenderer.prefab");
            created++;
        }

        // 6. EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("   ✓ Created EventSystem");
            created++;
        }

        Debug.Log($"   ✅ Created {created} fresh elements");
        return created;
    }

    private static GameObject CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string text, int fontSize)
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

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);

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

        return btnObj;
    }

    private static int WireEverything()
    {
        Debug.Log("\n▶ WIRING - Connecting all references...");
        int wired = 0;

        // Find components
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

                // Wire children
                guideBook.pageTitle = panel.transform.Find("PageTitle")?.GetComponent<TextMeshProUGUI>();
                guideBook.pageDescription = panel.transform.Find("PageDescription")?.GetComponent<TextMeshProUGUI>();
                guideBook.pageNumberText = panel.transform.Find("PageNumber")?.GetComponent<TextMeshProUGUI>();
                guideBook.closeBookButton = panel.transform.Find("CloseButton")?.GetComponent<Button>();
                guideBook.nextPageButton = panel.transform.Find("NextButton")?.GetComponent<Button>();
                guideBook.previousPageButton = panel.transform.Find("PreviousButton")?.GetComponent<Button>();

                Debug.Log("   ✓ Wired GuideBookPanel and all children");
                wired++;
            }

            if (btn != null)
            {
                Button button = btn.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    Debug.Log("=== GUIDE BUTTON CLICKED ===");
                    guideBook.OpenBook();
                });
                guideBook.openBookButton = button;
                Debug.Log("   ✓ Wired GuideBookButton with click handler");
                wired++;
            }

            EditorUtility.SetDirty(guideBook);
        }
        else
        {
            Debug.LogWarning("   ⚠️ PlantGuideBook component not found - skipping");
        }

        // Wire DrawingCanvas
        if (drawingCanvas != null)
        {
            GameObject drawArea = GameObject.Find("DrawingArea");
            if (drawArea != null)
            {
                drawingCanvas.drawingArea = drawArea.GetComponent<RectTransform>();
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

            // Stroke container
            Transform container = drawingCanvas.transform.Find("StrokeContainer");
            if (container == null)
            {
                GameObject containerObj = new GameObject("StrokeContainer");
                containerObj.transform.SetParent(drawingCanvas.transform, false);
                container = containerObj.transform;
                Debug.Log("   ✓ Created StrokeContainer");
            }
            drawingCanvas.strokeContainer = container;

            EditorUtility.SetDirty(drawingCanvas);
        }
        else
        {
            Debug.LogWarning("   ⚠️ DrawingCanvas component not found - skipping");
        }

        // Wire DrawingSceneUI
        if (sceneUI != null)
        {
            Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var button in allButtons)
            {
                if (button.gameObject.name.ToLower().Contains("start"))
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        Debug.Log("=== START DRAWING CLICKED ===");
                        sceneUI.OnStartDrawing();
                    });
                    sceneUI.startDrawingButton = button;
                    Debug.Log("   ✓ Wired Start Drawing button");
                    wired++;
                    break;
                }
            }

            EditorUtility.SetDirty(sceneUI);
        }
        else
        {
            Debug.LogWarning("   ⚠️ DrawingSceneUI component not found - skipping");
        }

        Debug.Log($"   ✅ Wired {wired} connections");
        return wired;
    }
}
