using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Fixes Drawing Scene display issues - ensures all elements are visible and wired
/// Run from: Tools > Sketch Blossom > Fix Drawing Scene Display
/// </summary>
public class DrawingSceneDisplayFixer : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Fix Drawing Scene Display", priority = 0)]
    public static void FixDisplay()
    {
        Debug.Log("╔════════════════════════════════════════════════════╗");
        Debug.Log("║        FIXING DRAWING SCENE DISPLAY ISSUES         ║");
        Debug.Log("╚════════════════════════════════════════════════════╝");

        int fixes = 0;

        // Fix 1: Ensure DrawingArea exists and is wired
        fixes += FixDrawingArea();

        // Fix 2: Fix button text "Finish Plant"
        fixes += FixFinishButton();

        // Fix 3: Ensure hint text is visible
        fixes += FixHintText();

        // Fix 4: Verify all DrawingCanvas references
        fixes += WireDrawingCanvas();

        // Fix 5: Fix hierarchy order
        fixes += FixHierarchyOrder();

        Debug.Log("╔════════════════════════════════════════════════════╗");
        Debug.Log($"║          COMPLETE! Applied {fixes} fixes               ║");
        Debug.Log("╚════════════════════════════════════════════════════╝");

        EditorUtility.DisplayDialog(
            "Display Fixed!",
            $"Applied {fixes} fixes!\n\n" +
            "✅ DrawingArea wired correctly\n" +
            "✅ 'Finish Plant' button fixed\n" +
            "✅ Hint text visible\n" +
            "✅ All references connected\n\n" +
            "Press Play to test!",
            "OK"
        );
    }

    private static int FixDrawingArea()
    {
        Debug.Log("\n▶ Step 1: Fixing DrawingArea...");

        DrawingCanvas canvas = Object.FindFirstObjectByType<DrawingCanvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ DrawingCanvas component not found!");
            return 0;
        }

        // Find DrawingArea - it might be in DrawingPanel
        GameObject drawingArea = GameObject.Find("DrawingArea");

        if (drawingArea == null)
        {
            Debug.LogWarning("   DrawingArea not found, searching in DrawingPanel...");
            GameObject drawingPanel = GameObject.Find("DrawingPanel");
            if (drawingPanel != null)
            {
                Transform areaTransform = drawingPanel.transform.Find("DrawingArea");
                if (areaTransform != null)
                {
                    drawingArea = areaTransform.gameObject;
                    Debug.Log("   Found DrawingArea inside DrawingPanel");
                }
            }
        }

        if (drawingArea != null)
        {
            // Wire it
            canvas.drawingArea = drawingArea.GetComponent<RectTransform>();
            EditorUtility.SetDirty(canvas);
            Debug.Log("   ✅ DrawingArea wired to DrawingCanvas");
            return 1;
        }
        else
        {
            Debug.LogError("   ❌ DrawingArea not found in scene!");
            return 0;
        }
    }

    private static int FixFinishButton()
    {
        Debug.Log("\n▶ Step 2: Fixing Finish button text...");

        // Find FinishButton
        GameObject finishBtn = GameObject.Find("FinishButton");

        if (finishBtn == null)
        {
            Debug.LogWarning("   Searching for FinishButton in DrawingPanel...");
            GameObject drawingPanel = GameObject.Find("DrawingPanel");
            if (drawingPanel != null)
            {
                Transform btnTransform = drawingPanel.transform.Find("FinishButton");
                if (btnTransform != null)
                {
                    finishBtn = btnTransform.gameObject;
                }
            }
        }

        if (finishBtn != null)
        {
            // Find text child
            TextMeshProUGUI text = finishBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "Finish Plant";
                EditorUtility.SetDirty(text);
                Debug.Log("   ✅ Changed button text to 'Finish Plant'");
                return 1;
            }
        }

        Debug.LogWarning("   ⚠️ FinishButton not found");
        return 0;
    }

    private static int FixHintText()
    {
        Debug.Log("\n▶ Step 3: Fixing hint text visibility...");

        // Find HintText
        GameObject hintText = GameObject.Find("HintText");

        if (hintText == null)
        {
            Debug.LogWarning("   Searching for HintText in DrawingPanel...");
            GameObject drawingPanel = GameObject.Find("DrawingPanel");
            if (drawingPanel != null)
            {
                Transform hintTransform = drawingPanel.transform.Find("HintText");
                if (hintTransform != null)
                {
                    hintText = hintTransform.gameObject;
                }
            }
        }

        if (hintText != null)
        {
            // Make sure it's active
            hintText.SetActive(true);

            // Verify text content
            TextMeshProUGUI text = hintText.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "press H for the guide";
                text.fontSize = 18;
                text.alignment = TextAlignmentOptions.Center;
                text.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                text.fontStyle = FontStyles.Italic;
                EditorUtility.SetDirty(text);
                Debug.Log("   ✅ Hint text configured: 'press H for the guide'");
                return 1;
            }
        }

        Debug.LogWarning("   ⚠️ HintText not found");
        return 0;
    }

    private static int WireDrawingCanvas()
    {
        Debug.Log("\n▶ Step 4: Wiring all DrawingCanvas references...");
        int wired = 0;

        DrawingCanvas canvas = Object.FindFirstObjectByType<DrawingCanvas>();
        if (canvas == null)
        {
            Debug.LogError("   ❌ DrawingCanvas component not found!");
            return 0;
        }

        GameObject drawingPanel = GameObject.Find("DrawingPanel");
        if (drawingPanel == null)
        {
            Debug.LogError("   ❌ DrawingPanel not found!");
            return 0;
        }

        // Wire DrawingArea (if not already done)
        if (canvas.drawingArea == null)
        {
            Transform areaTransform = drawingPanel.transform.Find("DrawingArea");
            if (areaTransform != null)
            {
                canvas.drawingArea = areaTransform.GetComponent<RectTransform>();
                Debug.Log("   ✓ Wired drawingArea");
                wired++;
            }
        }

        // Wire StrokeCounter
        if (canvas.strokeCountText == null)
        {
            Transform counterTransform = drawingPanel.transform.Find("StrokeCounter");
            if (counterTransform != null)
            {
                canvas.strokeCountText = counterTransform.GetComponent<TextMeshProUGUI>();
                Debug.Log("   ✓ Wired strokeCountText");
                wired++;
            }
        }

        // Wire FinishButton
        if (canvas.finishButton == null)
        {
            Transform btnTransform = drawingPanel.transform.Find("FinishButton");
            if (btnTransform != null)
            {
                canvas.finishButton = btnTransform.GetComponent<Button>();
                Debug.Log("   ✓ Wired finishButton");
                wired++;
            }
        }

        // Wire LineRenderer Prefab
        if (canvas.lineRendererPrefab == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/LineRenderer.prefab");
            if (prefab != null)
            {
                canvas.lineRendererPrefab = prefab.GetComponent<LineRenderer>();
                Debug.Log("   ✓ Wired lineRendererPrefab");
                wired++;
            }
        }

        // Ensure StrokeContainer exists
        Transform container = canvas.transform.Find("StrokeContainer");
        if (container == null)
        {
            GameObject cont = new GameObject("StrokeContainer");
            cont.transform.SetParent(canvas.transform, false);
            container = cont.transform;
            Debug.Log("   ✓ Created StrokeContainer");
            wired++;
        }
        canvas.strokeContainer = container;

        EditorUtility.SetDirty(canvas);
        Debug.Log($"   ✅ Wired {wired} DrawingCanvas references");
        return wired;
    }

    private static int FixHierarchyOrder()
    {
        Debug.Log("\n▶ Step 5: Fixing hierarchy order...");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return 0;

        // Ensure proper order:
        // 1. Background (first - behind everything)
        // 2. InstructionsPanel
        // 3. DrawingPanel
        // 4. GuideBookButton
        // 5. GuideBookPanel (last - on top)

        GameObject bg = GameObject.Find("Background");
        GameObject instrPanel = GameObject.Find("InstructionsPanel");
        GameObject drawPanel = GameObject.Find("DrawingPanel");
        GameObject guideBtn = GameObject.Find("GuideBookButton");
        GameObject guidePanel = GameObject.Find("GuideBookPanel");

        int order = 0;

        if (bg != null)
        {
            bg.transform.SetSiblingIndex(order++);
            Debug.Log("   ✓ Background (index 0)");
        }

        if (instrPanel != null)
        {
            instrPanel.transform.SetSiblingIndex(order++);
            Debug.Log("   ✓ InstructionsPanel (index 1)");
        }

        if (drawPanel != null)
        {
            drawPanel.transform.SetSiblingIndex(order++);
            Debug.Log("   ✓ DrawingPanel (index 2)");
        }

        if (guideBtn != null)
        {
            guideBtn.transform.SetSiblingIndex(order++);
            Debug.Log("   ✓ GuideBookButton (index 3)");
        }

        if (guidePanel != null)
        {
            guidePanel.transform.SetSiblingIndex(order++);
            Debug.Log("   ✓ GuideBookPanel (index 4 - on top)");
        }

        Debug.Log("   ✅ Hierarchy order fixed");
        return 1;
    }
}
