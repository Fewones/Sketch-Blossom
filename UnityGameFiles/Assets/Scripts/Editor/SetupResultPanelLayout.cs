using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to automatically format PlantResultPanel layout
/// Positions all UI elements properly for a clean, readable result display
/// </summary>
public class SetupResultPanelLayout : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Format Result Panel Layout")]
    static void FormatResultPanelLayout()
    {
        Debug.Log("========== FORMATTING RESULT PANEL LAYOUT ==========");

        // Find PlantResultPanel in the scene (including inactive objects)
        PlantResultPanel resultPanel = FindFirstObjectByType<PlantResultPanel>(FindObjectsInactive.Include);
        if (resultPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "PlantResultPanel not found in scene!", "OK");
            Debug.LogError("PlantResultPanel not found in scene!");
            return;
        }

        Debug.Log($"✓ Found PlantResultPanel on: {resultPanel.gameObject.name}");

        // Get all UI element references
        var titleTextField = typeof(PlantResultPanel).GetField("titleText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var plantNameTextField = typeof(PlantResultPanel).GetField("plantNameText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var elementTextField = typeof(PlantResultPanel).GetField("elementText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var statsTextField = typeof(PlantResultPanel).GetField("statsText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var movesTextField = typeof(PlantResultPanel).GetField("movesText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var continueButtonField = typeof(PlantResultPanel).GetField("continueButton", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var redrawButtonField = typeof(PlantResultPanel).GetField("redrawButton", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var panelWindowField = typeof(PlantResultPanel).GetField("panelWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        TextMeshProUGUI titleText = titleTextField?.GetValue(resultPanel) as TextMeshProUGUI;
        TextMeshProUGUI plantNameText = plantNameTextField?.GetValue(resultPanel) as TextMeshProUGUI;
        TextMeshProUGUI elementText = elementTextField?.GetValue(resultPanel) as TextMeshProUGUI;
        TextMeshProUGUI statsText = statsTextField?.GetValue(resultPanel) as TextMeshProUGUI;
        TextMeshProUGUI movesText = movesTextField?.GetValue(resultPanel) as TextMeshProUGUI;
        Button continueButton = continueButtonField?.GetValue(resultPanel) as Button;
        Button redrawButton = redrawButtonField?.GetValue(resultPanel) as Button;
        GameObject panelWindow = panelWindowField?.GetValue(resultPanel) as GameObject;

        if (panelWindow == null)
        {
            EditorUtility.DisplayDialog("Error", "Panel Window is missing!", "OK");
            return;
        }

        Debug.Log($"✓ Found Panel Window: {panelWindow.name}");

        // Format Title (top center)
        if (titleText != null)
        {
            Undo.RecordObject(titleText.GetComponent<RectTransform>(), "Format Title");
            RectTransform rect = titleText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -20f);
            rect.sizeDelta = new Vector2(400f, 40f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 24;
            EditorUtility.SetDirty(titleText.gameObject);
            Debug.Log("✓ Formatted Title: Top Center");
        }

        // Format Plant Name (below title, center)
        if (plantNameText != null)
        {
            Undo.RecordObject(plantNameText.GetComponent<RectTransform>(), "Format Plant Name");
            RectTransform rect = plantNameText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -70f);
            rect.sizeDelta = new Vector2(500f, 60f);
            plantNameText.alignment = TextAlignmentOptions.Center;
            plantNameText.fontSize = 48;
            EditorUtility.SetDirty(plantNameText.gameObject);
            Debug.Log("✓ Formatted Plant Name: Below Title, Centered");
        }

        // Format Element Text (below plant name, center)
        if (elementText != null)
        {
            Undo.RecordObject(elementText.GetComponent<RectTransform>(), "Format Element");
            RectTransform rect = elementText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -140f);
            rect.sizeDelta = new Vector2(300f, 35f);
            elementText.alignment = TextAlignmentOptions.Center;
            elementText.fontSize = 24;
            EditorUtility.SetDirty(elementText.gameObject);
            Debug.Log("✓ Formatted Element: Below Plant Name, Centered");
        }

        // Format Stats (left side of content area)
        if (statsText != null)
        {
            Undo.RecordObject(statsText.GetComponent<RectTransform>(), "Format Stats");
            RectTransform rect = statsText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(40f, 0f);
            rect.sizeDelta = new Vector2(250f, 200f);
            statsText.alignment = TextAlignmentOptions.TopLeft;
            statsText.fontSize = 18;
            EditorUtility.SetDirty(statsText.gameObject);
            Debug.Log("✓ Formatted Stats: Left Side, Centered Vertically");
        }

        // Format Moves (right side of content area)
        if (movesText != null)
        {
            Undo.RecordObject(movesText.GetComponent<RectTransform>(), "Format Moves");
            RectTransform rect = movesText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-40f, 0f);
            rect.sizeDelta = new Vector2(300f, 200f);
            movesText.alignment = TextAlignmentOptions.TopLeft;
            movesText.fontSize = 18;
            EditorUtility.SetDirty(movesText.gameObject);
            Debug.Log("✓ Formatted Moves: Right Side, Centered Vertically");
        }

        // Format Continue Button (bottom-right)
        if (continueButton != null)
        {
            Undo.RecordObject(continueButton.GetComponent<RectTransform>(), "Format Continue Button");
            RectTransform rect = continueButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-20f, 20f);
            rect.sizeDelta = new Vector2(180f, 50f);
            EditorUtility.SetDirty(continueButton.gameObject);
            Debug.Log("✓ Formatted Continue Button: Bottom-Right");
        }

        // Format Redraw Button (bottom-left)
        if (redrawButton != null)
        {
            Undo.RecordObject(redrawButton.GetComponent<RectTransform>(), "Format Redraw Button");
            RectTransform rect = redrawButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(20f, 20f);
            rect.sizeDelta = new Vector2(180f, 50f);
            EditorUtility.SetDirty(redrawButton.gameObject);
            Debug.Log("✓ Formatted Redraw Button: Bottom-Left");
        }

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("========== LAYOUT FORMATTING COMPLETE ==========");
        EditorUtility.DisplayDialog("Success",
            "PlantResultPanel layout formatted successfully!\n\n" +
            "✓ Title: Top Center\n" +
            "✓ Plant Name: Large, Centered\n" +
            "✓ Element: Below Plant Name\n" +
            "✓ Stats: Left Side\n" +
            "✓ Moves: Right Side\n" +
            "✓ Buttons: Bottom Corners",
            "OK");
    }
}
