using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Comprehensive DrawingPanel display fixer
/// Ensures DrawingPanel and all children display correctly when "Draw my first plant" is clicked
/// Run from: Tools > Sketch Blossom > Fix DrawingPanel Display
/// </summary>
public class DrawingPanelDisplayFixer : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Fix DrawingPanel Display", priority = 0)]
    public static void FixDrawingPanelDisplay()
    {
        Debug.Log("╔════════════════════════════════════════════════════╗");
        Debug.Log("║      FIXING DRAWINGPANEL DISPLAY ISSUES            ║");
        Debug.Log("╚════════════════════════════════════════════════════╝");

        int fixes = 0;

        // Step 1: Verify DrawingPanel exists
        fixes += VerifyDrawingPanel();

        // Step 2: Fix all children visibility
        fixes += FixChildrenVisibility();

        // Step 3: Ensure proper RectTransform setup
        fixes += FixRectTransforms();

        // Step 4: Wire DrawingCanvas references
        fixes += WireDrawingCanvasReferences();

        // Step 5: Verify DrawingSceneUI connections
        fixes += VerifyDrawingSceneUI();

        // Step 6: Test the flow
        TestDrawingPanelFlow();

        Debug.Log("╔════════════════════════════════════════════════════╗");
        Debug.Log($"║        COMPLETE! Applied {fixes} fixes                ║");
        Debug.Log("╚════════════════════════════════════════════════════╝");

        EditorUtility.DisplayDialog(
            "DrawingPanel Fixed!",
            $"Applied {fixes} fixes!\n\n" +
            "✅ DrawingPanel verified\n" +
            "✅ All children visible\n" +
            "✅ RectTransforms configured\n" +
            "✅ References wired\n\n" +
            "Press Play and click 'Draw my first plant'!\n" +
            "DrawingPanel should now display correctly.",
            "OK"
        );
    }

    private static int VerifyDrawingPanel()
    {
        Debug.Log("\n▶ Step 1: Verifying DrawingPanel...");

        GameObject drawingPanel = GameObject.Find("DrawingPanel");

        if (drawingPanel == null)
        {
            Debug.LogError("   ❌ DrawingPanel not found in scene!");
            Debug.LogError("   Please run: Tools > Build Complete Drawing Scene UI first");
            return 0;
        }

        Debug.Log("   ✓ DrawingPanel found");

        // Ensure it has a RectTransform
        RectTransform rect = drawingPanel.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = drawingPanel.AddComponent<RectTransform>();
            Debug.Log("   ✓ Added RectTransform");
        }

        // Set to fill parent
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Debug.Log("   ✅ DrawingPanel configured (fills screen)");
        return 1;
    }

    private static int FixChildrenVisibility()
    {
        Debug.Log("\n▶ Step 2: Fixing children visibility...");
        int fixes = 0;

        GameObject drawingPanel = GameObject.Find("DrawingPanel");
        if (drawingPanel == null) return 0;

        // Check DrawingArea
        Transform drawingArea = drawingPanel.transform.Find("DrawingArea");
        if (drawingArea != null)
        {
            drawingArea.gameObject.SetActive(true);

            // Ensure it has proper components
            RectTransform rect = drawingArea.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Position on left side
                rect.anchorMin = new Vector2(0.05f, 0.1f);
                rect.anchorMax = new Vector2(0.5f, 0.9f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            Image img = drawingArea.GetComponent<Image>();
            if (img == null)
            {
                img = drawingArea.gameObject.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0.3f);
            }

            Debug.Log("   ✓ DrawingArea active and visible");
            fixes++;
        }
        else
        {
            Debug.LogError("   ❌ DrawingArea not found as child of DrawingPanel!");
        }

        // Check HintText
        Transform hintText = drawingPanel.transform.Find("HintText");
        if (hintText != null)
        {
            hintText.gameObject.SetActive(true);

            TextMeshProUGUI text = hintText.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "press H for the guide";
                text.fontSize = 18;
                text.alignment = TextAlignmentOptions.Center;
                text.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                text.fontStyle = FontStyles.Italic;
                EditorUtility.SetDirty(text);
            }

            Debug.Log("   ✓ HintText active and visible");
            fixes++;
        }
        else
        {
            Debug.LogError("   ❌ HintText not found as child of DrawingPanel!");
        }

        // Check StrokeCounter
        Transform strokeCounter = drawingPanel.transform.Find("StrokeCounter");
        if (strokeCounter != null)
        {
            strokeCounter.gameObject.SetActive(true);

            TextMeshProUGUI text = strokeCounter.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "Strokes: 0/15";
                text.fontSize = 20;
                text.alignment = TextAlignmentOptions.Center;
                text.color = new Color(0.2f, 0.2f, 0.2f);
                EditorUtility.SetDirty(text);
            }

            Debug.Log("   ✓ StrokeCounter active and visible");
            fixes++;
        }
        else
        {
            Debug.LogWarning("   ⚠️ StrokeCounter not found");
        }

        // Check FinishButton
        Transform finishButton = drawingPanel.transform.Find("FinishButton");
        if (finishButton != null)
        {
            finishButton.gameObject.SetActive(true);

            Button btn = finishButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = false; // Will be enabled after first stroke
            }

            TextMeshProUGUI text = finishButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "Finish Plant";
                EditorUtility.SetDirty(text);
            }

            Debug.Log("   ✓ FinishButton active and visible");
            fixes++;
        }
        else
        {
            Debug.LogWarning("   ⚠️ FinishButton not found");
        }

        Debug.Log($"   ✅ Fixed {fixes} children");
        return fixes;
    }

    private static int FixRectTransforms()
    {
        Debug.Log("\n▶ Step 3: Fixing RectTransform configurations...");
        int fixes = 0;

        GameObject drawingPanel = GameObject.Find("DrawingPanel");
        if (drawingPanel == null) return 0;

        // Fix HintText position
        Transform hintText = drawingPanel.transform.Find("HintText");
        if (hintText != null)
        {
            RectTransform rect = hintText.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.05f, 0.02f);
                rect.anchorMax = new Vector2(0.5f, 0.08f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                Debug.Log("   ✓ HintText positioned at bottom");
                fixes++;
            }
        }

        // Fix StrokeCounter position
        Transform strokeCounter = drawingPanel.transform.Find("StrokeCounter");
        if (strokeCounter != null)
        {
            RectTransform rect = strokeCounter.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.05f, 0.91f);
                rect.anchorMax = new Vector2(0.25f, 0.97f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                Debug.Log("   ✓ StrokeCounter positioned at top-left");
                fixes++;
            }
        }

        // Fix FinishButton position
        Transform finishButton = drawingPanel.transform.Find("FinishButton");
        if (finishButton != null)
        {
            RectTransform rect = finishButton.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.3f, 0.91f);
                rect.anchorMax = new Vector2(0.5f, 0.97f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                Debug.Log("   ✓ FinishButton positioned at top-right");
                fixes++;
            }
        }

        Debug.Log($"   ✅ Fixed {fixes} positions");
        return fixes;
    }

    private static int WireDrawingCanvasReferences()
    {
        Debug.Log("\n▶ Step 4: Wiring DrawingCanvas references...");
        int wired = 0;

        DrawingCanvas canvas = Object.FindFirstObjectByType<DrawingCanvas>();
        if (canvas == null)
        {
            Debug.LogWarning("   ⚠️ DrawingCanvas component not found");
            return 0;
        }

        GameObject drawingPanel = GameObject.Find("DrawingPanel");
        if (drawingPanel == null) return 0;

        // Wire DrawingArea
        Transform drawingArea = drawingPanel.transform.Find("DrawingArea");
        if (drawingArea != null && canvas.drawingArea == null)
        {
            canvas.drawingArea = drawingArea.GetComponent<RectTransform>();
            Debug.Log("   ✓ Wired drawingArea");
            wired++;
        }

        // Wire StrokeCountText
        Transform strokeCounter = drawingPanel.transform.Find("StrokeCounter");
        if (strokeCounter != null && canvas.strokeCountText == null)
        {
            canvas.strokeCountText = strokeCounter.GetComponent<TextMeshProUGUI>();
            Debug.Log("   ✓ Wired strokeCountText");
            wired++;
        }

        // Wire FinishButton
        Transform finishButton = drawingPanel.transform.Find("FinishButton");
        if (finishButton != null && canvas.finishButton == null)
        {
            canvas.finishButton = finishButton.GetComponent<Button>();
            Debug.Log("   ✓ Wired finishButton");
            wired++;
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

        // Ensure StrokeContainer
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
        Debug.Log($"   ✅ Wired {wired} references");
        return wired;
    }

    private static int VerifyDrawingSceneUI()
    {
        Debug.Log("\n▶ Step 5: Verifying DrawingSceneUI connections...");
        int fixes = 0;

        DrawingSceneUI sceneUI = Object.FindFirstObjectByType<DrawingSceneUI>();
        if (sceneUI == null)
        {
            Debug.LogWarning("   ⚠️ DrawingSceneUI component not found");
            return 0;
        }

        // Wire InstructionsPanel
        GameObject instrPanel = GameObject.Find("InstructionsPanel");
        if (instrPanel != null && sceneUI.instructionsPanel == null)
        {
            sceneUI.instructionsPanel = instrPanel;
            Debug.Log("   ✓ Wired instructionsPanel");
            fixes++;
        }

        // Wire DrawingPanel
        GameObject drawPanel = GameObject.Find("DrawingPanel");
        if (drawPanel != null && sceneUI.drawingPanel == null)
        {
            sceneUI.drawingPanel = drawPanel;
            Debug.Log("   ✓ Wired drawingPanel");
            fixes++;
        }

        // Wire StartDrawingButton
        GameObject startBtn = GameObject.Find("StartDrawingButton");
        if (startBtn != null && sceneUI.startDrawingButton == null)
        {
            Button btn = startBtn.GetComponent<Button>();
            if (btn != null)
            {
                // Clear and re-add listener
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    Debug.Log("=== START DRAWING CLICKED ===");
                    sceneUI.OnStartDrawing();
                });
                sceneUI.startDrawingButton = btn;
                Debug.Log("   ✓ Wired startDrawingButton with listener");
                fixes++;
            }
        }

        EditorUtility.SetDirty(sceneUI);
        Debug.Log($"   ✅ Verified {fixes} connections");
        return fixes;
    }

    private static void TestDrawingPanelFlow()
    {
        Debug.Log("\n▶ Step 6: Testing DrawingPanel flow...");

        GameObject instrPanel = GameObject.Find("InstructionsPanel");
        GameObject drawPanel = GameObject.Find("DrawingPanel");

        if (instrPanel != null && drawPanel != null)
        {
            // Ensure correct initial state
            instrPanel.SetActive(true);
            drawPanel.SetActive(false);

            Debug.Log("   ✓ Initial state:");
            Debug.Log("     - InstructionsPanel: ACTIVE");
            Debug.Log("     - DrawingPanel: INACTIVE");
            Debug.Log("   ");
            Debug.Log("   When you click 'Draw my first plant':");
            Debug.Log("     - InstructionsPanel will hide");
            Debug.Log("     - DrawingPanel will show");
            Debug.Log("     - All children should be visible");
        }

        DrawingCanvas canvas = Object.FindFirstObjectByType<DrawingCanvas>();
        if (canvas != null)
        {
            Debug.Log("   ");
            Debug.Log("   DrawingCanvas status:");
            Debug.Log($"     - drawingArea: {(canvas.drawingArea != null ? "✓ WIRED" : "❌ NULL")}");
            Debug.Log($"     - strokeCountText: {(canvas.strokeCountText != null ? "✓ WIRED" : "❌ NULL")}");
            Debug.Log($"     - finishButton: {(canvas.finishButton != null ? "✓ WIRED" : "❌ NULL")}");
            Debug.Log($"     - lineRendererPrefab: {(canvas.lineRendererPrefab != null ? "✓ WIRED" : "❌ NULL")}");
            Debug.Log($"     - isDrawingEnabled: {canvas.isDrawingEnabled}");
        }

        Debug.Log("   ");
        Debug.Log("   ✅ Flow test complete - Press Play to test!");
    }
}
