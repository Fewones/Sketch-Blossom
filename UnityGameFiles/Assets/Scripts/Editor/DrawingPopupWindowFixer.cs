using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Converts DrawingPanel to a proper centered pop-up window with all elements visible
/// Run from: Tools > Sketch Blossom > Create Drawing Popup Window
/// </summary>
public class DrawingPopupWindowFixer : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Create Drawing Popup Window", priority = 0)]
    public static void CreateDrawingPopup()
    {
        if (!EditorUtility.DisplayDialog(
            "Create Drawing Popup Window",
            "This will recreate the DrawingPanel as a centered pop-up window with:\n\n" +
            "• Semi-transparent dark overlay\n" +
            "• Centered pop-up panel (80% screen)\n" +
            "• DrawingArea in center\n" +
            "• Finish Plant button\n" +
            "• Stroke counter\n" +
            "• Hint text\n\n" +
            "Continue?",
            "Yes", "Cancel"))
        {
            return;
        }

        int steps = 0;

        // Step 1: Find Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Canvas not found in scene!", "OK");
            return;
        }

        // Step 2: Delete old DrawingPanel and overlay if they exist
        Transform oldPanel = canvas.transform.Find("DrawingPanel");
        if (oldPanel != null)
        {
            DestroyImmediate(oldPanel.gameObject);
            Debug.Log("Deleted old DrawingPanel");
            steps++;
        }

        Transform oldOverlay = canvas.transform.Find("DrawingOverlay");
        if (oldOverlay != null)
        {
            DestroyImmediate(oldOverlay.gameObject);
            Debug.Log("Deleted old DrawingOverlay");
            steps++;
        }

        // Step 3: Create dark overlay background
        GameObject overlay = CreateOverlay(canvas.transform);
        Debug.Log("Created dark overlay background");
        steps++;

        // Step 4: Create pop-up panel
        GameObject panel = CreatePopupPanel(canvas.transform);
        Debug.Log("Created pop-up panel");
        steps++;

        // Step 5: Create DrawingArea inside panel
        GameObject drawingArea = CreateDrawingArea(panel.transform);
        Debug.Log("Created DrawingArea");
        steps++;

        // Step 6: Create title
        CreateTitle(panel.transform);
        Debug.Log("Created title");
        steps++;

        // Step 7: Create hint text
        CreateHintText(panel.transform);
        Debug.Log("Created hint text");
        steps++;

        // Step 8: Create stroke counter
        CreateStrokeCounter(panel.transform);
        Debug.Log("Created stroke counter");
        steps++;

        // Step 9: Create Finish Plant button
        CreateFinishButton(panel.transform);
        Debug.Log("Created Finish Plant button");
        steps++;

        // Step 10: Update DrawingSceneUI references
        DrawingSceneUI sceneUI = Object.FindFirstObjectByType<DrawingSceneUI>();
        if (sceneUI != null)
        {
            sceneUI.drawingPanel = panel;
            EditorUtility.SetDirty(sceneUI);
            Debug.Log("Updated DrawingSceneUI.drawingPanel reference");
            steps++;
        }

        // Step 11: Verify DrawingCanvas can find everything
        DrawingCanvas drawingCanvas = Object.FindFirstObjectByType<DrawingCanvas>();
        if (drawingCanvas != null)
        {
            Debug.Log("DrawingCanvas will auto-find all references on scene start");
            steps++;
        }

        EditorUtility.DisplayDialog(
            "Success!",
            $"Drawing popup window created successfully!\n\n" +
            $"Steps completed: {steps}\n\n" +
            "The DrawingPanel is now a centered pop-up window with:\n" +
            "✓ Dark overlay background\n" +
            "✓ Centered panel (80% screen size)\n" +
            "✓ DrawingArea for drawing\n" +
            "✓ Title, hint text, stroke counter\n" +
            "✓ Finish Plant button\n\n" +
            "Press Play and click 'Draw my first plant' to see it!",
            "OK");
    }

    private static GameObject CreateOverlay(Transform parent)
    {
        GameObject overlay = new GameObject("DrawingOverlay");
        overlay.transform.SetParent(parent, false);

        // Add Image component for dark background
        Image img = overlay.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black

        // Fill entire screen
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Start inactive (will be shown when drawing starts)
        overlay.SetActive(false);

        return overlay;
    }

    private static GameObject CreatePopupPanel(Transform parent)
    {
        GameObject panel = new GameObject("DrawingPanel");
        panel.transform.SetParent(parent, false);

        // Add Image for background
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.95f, 0.98f, 0.95f, 1f); // Light greenish white

        // Center panel, 80% of screen size
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.1f);
        rect.anchorMax = new Vector2(0.9f, 0.9f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Add outline
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.5f, 0.2f, 1f); // Dark green
        outline.effectDistance = new Vector2(3, -3);

        // Start inactive (will be shown when drawing starts)
        panel.SetActive(false);

        return panel;
    }

    private static GameObject CreateDrawingArea(Transform parent)
    {
        GameObject area = new GameObject("DrawingArea");
        area.transform.SetParent(parent, false);

        // White background for drawing
        Image img = area.AddComponent<Image>();
        img.color = Color.white;

        // Center area, leave margins for UI elements
        RectTransform rect = area.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.2f); // Leave space at top and bottom
        rect.anchorMax = new Vector2(0.9f, 0.75f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Green border
        Outline outline = area.AddComponent<Outline>();
        outline.effectColor = new Color(0.3f, 0.7f, 0.3f, 1f); // Green
        outline.effectDistance = new Vector2(2, -2);

        area.SetActive(true); // Keep active so it can be found

        return area;
    }

    private static void CreateTitle(Transform parent)
    {
        GameObject title = new GameObject("Title");
        title.transform.SetParent(parent, false);

        TextMeshProUGUI text = title.AddComponent<TextMeshProUGUI>();
        text.text = "Draw Your Plant";
        text.fontSize = 36;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(0.2f, 0.5f, 0.2f, 1f); // Dark green
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = title.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.85f);
        rect.anchorMax = new Vector2(0.9f, 0.95f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        title.SetActive(true);
    }

    private static void CreateHintText(Transform parent)
    {
        GameObject hint = new GameObject("HintText");
        hint.transform.SetParent(parent, false);

        TextMeshProUGUI text = hint.AddComponent<TextMeshProUGUI>();
        text.text = "press H for the guide";
        text.fontSize = 18;
        text.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Italic;

        RectTransform rect = hint.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.12f);
        rect.anchorMax = new Vector2(0.9f, 0.18f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        hint.SetActive(true);
    }

    private static void CreateStrokeCounter(Transform parent)
    {
        GameObject counter = new GameObject("StrokeCounter");
        counter.transform.SetParent(parent, false);

        TextMeshProUGUI text = counter.AddComponent<TextMeshProUGUI>();
        text.text = "Strokes: 0/15";
        text.fontSize = 24;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(0.2f, 0.5f, 0.2f, 1f); // Dark green
        text.alignment = TextAlignmentOptions.Left;

        RectTransform rect = counter.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.77f);
        rect.anchorMax = new Vector2(0.5f, 0.83f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        counter.SetActive(true);
    }

    private static void CreateFinishButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("FinishButton");
        buttonObj.transform.SetParent(parent, false);

        // Add Image for button background
        Image bg = buttonObj.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.7f, 0.3f, 1f); // Green

        // Add Button component
        Button button = buttonObj.AddComponent<Button>();

        // Button colors
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.8f, 0.4f, 1f);
        colors.pressedColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        button.colors = colors;

        // Position button at bottom center
        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.35f, 0.05f);
        rect.anchorMax = new Vector2(0.65f, 0.12f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Add button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Finish Plant";
        text.fontSize = 24;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        buttonObj.SetActive(true);
    }
}
