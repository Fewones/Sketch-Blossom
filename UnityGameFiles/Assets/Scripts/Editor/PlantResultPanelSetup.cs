using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Automatically creates the Plant Result Panel UI
/// Similar to DrawingPanel but for showing analysis results
/// </summary>
public class PlantResultPanelSetup : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Setup Plant Result Panel (Complete)")]
    public static void SetupCompleteResultPanel()
    {
        Debug.Log("=== PLANT RESULT PANEL SETUP START ===");

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found! Please create a Canvas first.", "OK");
            return;
        }

        // Create ResultOverlay (dark background)
        GameObject overlay = CreateOrGet(canvas.transform, "ResultOverlay");
        ConfigureOverlay(overlay);

        // Create ResultPanel (popup window)
        GameObject panel = CreateOrGet(overlay.transform, "ResultPanel");
        ConfigurePanel(panel);

        // Create UI elements
        GameObject title = CreateText(panel.transform, "TitleText", new Vector2(0, 220), 32, "🌱 Plant Analysis Complete! 🌱");
        GameObject plantName = CreateText(panel.transform, "PlantNameText", new Vector2(0, 160), 48, "Sunflower");
        GameObject element = CreateText(panel.transform, "ElementText", new Vector2(0, 100), 28, "🔥 Fire Type 🔥");
        GameObject confidence = CreateText(panel.transform, "ConfidenceText", new Vector2(0, 50), 22, "Confidence: ★★★★★\n85%");
        GameObject stats = CreateText(panel.transform, "StatsText", new Vector2(-150, -20), 20, "Stats:\nHP: 30\nATK: 15\nDEF: 10", TextAlignmentOptions.Left);
        GameObject moves = CreateText(panel.transform, "MovesText", new Vector2(150, -20), 20, "Moves:\n• Fireball ⚡\n• Flame Wave ⚡⚡\n• Burn ⚡⚡⚡", TextAlignmentOptions.Left);
        GameObject colorInfo = CreateText(panel.transform, "ColorInfoText", new Vector2(0, -120), 18, "Drawing Color: RED");
        GameObject continueBtn = CreateContinueButton(panel.transform);
        GameObject redrawBtn = CreateRedrawButton(panel.transform);

        // Add PlantResultPanel component
        PlantResultPanel panelScript = overlay.GetComponent<PlantResultPanel>();
        if (panelScript == null)
        {
            panelScript = overlay.AddComponent<PlantResultPanel>();
        }

        // Assign references
        panelScript.panelOverlay = overlay;
        panelScript.panelWindow = panel;
        panelScript.titleText = title.GetComponent<TextMeshProUGUI>();
        panelScript.plantNameText = plantName.GetComponent<TextMeshProUGUI>();
        panelScript.elementText = element.GetComponent<TextMeshProUGUI>();
        panelScript.confidenceText = confidence.GetComponent<TextMeshProUGUI>();
        panelScript.statsText = stats.GetComponent<TextMeshProUGUI>();
        panelScript.movesText = moves.GetComponent<TextMeshProUGUI>();
        panelScript.colorInfoText = colorInfo.GetComponent<TextMeshProUGUI>();
        panelScript.continueButton = continueBtn.GetComponent<Button>();
        panelScript.redrawButton = redrawBtn.GetComponent<Button>();

        // Link to DrawingManager
        DrawingManager manager = FindObjectOfType<DrawingManager>();
        if (manager != null)
        {
            manager.plantResultPanel = panelScript;
            EditorUtility.SetDirty(manager);
            Debug.Log("Linked PlantResultPanel to DrawingManager");
        }
        else
        {
            Debug.LogWarning("DrawingManager not found in scene. Link manually if needed.");
        }

        // Hide initially
        overlay.SetActive(false);

        // Mark dirty
        EditorUtility.SetDirty(overlay);
        EditorUtility.SetDirty(panel);

        Debug.Log("=== PLANT RESULT PANEL SETUP COMPLETE ===");
        EditorUtility.DisplayDialog("Success!",
            "Plant Result Panel created successfully!\n\n" +
            "The panel will automatically show analysis results.\n" +
            "It's structured like DrawingPanel with a proper popup.",
            "OK");

        Selection.activeGameObject = overlay;
    }

    private static GameObject CreateOrGet(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            Debug.Log($"Found existing {name}");
            return existing.gameObject;
        }

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Debug.Log($"Created {name}");
        return obj;
    }

    private static void ConfigureOverlay(GameObject overlay)
    {
        RectTransform rect = overlay.GetComponent<RectTransform>();
        if (rect == null) rect = overlay.AddComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Image image = overlay.GetComponent<Image>();
        if (image == null) image = overlay.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.85f);

        // Set as last sibling to render on top
        overlay.transform.SetAsLastSibling();
    }

    private static void ConfigurePanel(GameObject panel)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        if (rect == null) rect = panel.AddComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(700, 600);

        Image image = panel.GetComponent<Image>();
        if (image == null) image = panel.AddComponent<Image>();
        image.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Add border
        Outline outline = panel.GetComponent<Outline>();
        if (outline == null) outline = panel.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(3, -3);
    }

    private static GameObject CreateText(Transform parent, string name, Vector2 pos, float fontSize, string defaultText, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        GameObject textObj = CreateOrGet(parent, name);

        RectTransform rect = textObj.GetComponent<RectTransform>();
        if (rect == null) rect = textObj.AddComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(600, fontSize * 4);

        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        if (text == null) text = textObj.AddComponent<TextMeshProUGUI>();

        text.text = defaultText;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.enableWordWrapping = true;

        // Add shadow
        Shadow shadow = textObj.GetComponent<Shadow>();
        if (shadow == null) shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(2, -2);

        return textObj;
    }

    private static GameObject CreateContinueButton(Transform parent)
    {
        GameObject buttonObj = CreateOrGet(parent, "ContinueButton");

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        if (rect == null) rect = buttonObj.AddComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, -220);
        rect.sizeDelta = new Vector2(250, 60);

        Image image = buttonObj.GetComponent<Image>();
        if (image == null) image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.7f, 0.2f, 1f);

        Button button = buttonObj.GetComponent<Button>();
        if (button == null) button = buttonObj.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.7f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.5f, 0.1f, 1f);
        button.colors = colors;

        // Button text
        GameObject textObj = CreateOrGet(buttonObj.transform, "Text");
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        if (textRect == null) textRect = textObj.AddComponent<RectTransform>();

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI buttonText = textObj.GetComponent<TextMeshProUGUI>();
        if (buttonText == null) buttonText = textObj.AddComponent<TextMeshProUGUI>();

        buttonText.text = "Go to Battle →";
        buttonText.fontSize = 24;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyles.Bold;

        return buttonObj;
    }

    private static GameObject CreateRedrawButton(Transform parent)
    {
        GameObject buttonObj = CreateOrGet(parent, "RedrawButton");

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        if (rect == null) rect = buttonObj.AddComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, -160);
        rect.sizeDelta = new Vector2(200, 50);

        Image image = buttonObj.GetComponent<Image>();
        if (image == null) image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.8f, 0.6f, 0.2f, 1f); // Orange color

        Button button = buttonObj.GetComponent<Button>();
        if (button == null) button = buttonObj.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.8f, 0.6f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.9f, 0.7f, 0.3f, 1f);
        colors.pressedColor = new Color(0.6f, 0.4f, 0.1f, 1f);
        button.colors = colors;

        // Button text
        GameObject textObj = CreateOrGet(buttonObj.transform, "Text");
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        if (textRect == null) textRect = textObj.AddComponent<RectTransform>();

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI buttonText = textObj.GetComponent<TextMeshProUGUI>();
        if (buttonText == null) buttonText = textObj.AddComponent<TextMeshProUGUI>();

        buttonText.text = "← Redraw Plant";
        buttonText.fontSize = 20;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyles.Bold;

        return buttonObj;
    }
}
