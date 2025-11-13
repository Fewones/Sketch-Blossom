using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor script to automatically format PlantResultPanel layout
/// Positions buttons properly and cleans up the panel structure
/// </summary>
public class SetupResultPanelLayout : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Format Result Panel Layout")]
    static void FormatResultPanelLayout()
    {
        Debug.Log("========== FORMATTING RESULT PANEL LAYOUT ==========");

        // Find PlantResultPanel in the scene
        PlantResultPanel resultPanel = FindFirstObjectByType<PlantResultPanel>();
        if (resultPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "PlantResultPanel not found in scene!", "OK");
            Debug.LogError("PlantResultPanel not found in scene!");
            return;
        }

        Debug.Log($"✓ Found PlantResultPanel on: {resultPanel.gameObject.name}");

        // Get references using reflection
        var continueButtonField = typeof(PlantResultPanel).GetField("continueButton",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var redrawButtonField = typeof(PlantResultPanel).GetField("redrawButton",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var panelWindowField = typeof(PlantResultPanel).GetField("panelWindow",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        Button continueButton = continueButtonField?.GetValue(resultPanel) as Button;
        Button redrawButton = redrawButtonField?.GetValue(resultPanel) as Button;
        GameObject panelWindow = panelWindowField?.GetValue(resultPanel) as GameObject;

        if (continueButton == null || redrawButton == null || panelWindow == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Missing button or panel window references!\n\n" +
                $"Continue Button: {(continueButton != null ? "OK" : "MISSING")}\n" +
                $"Redraw Button: {(redrawButton != null ? "OK" : "MISSING")}\n" +
                $"Panel Window: {(panelWindow != null ? "OK" : "MISSING")}",
                "OK");
            return;
        }

        Debug.Log($"✓ Found Continue Button on: {continueButton.gameObject.name}");
        Debug.Log($"✓ Found Redraw Button on: {redrawButton.gameObject.name}");
        Debug.Log($"✓ Found Panel Window: {panelWindow.name}");

        // Record undo
        Undo.RecordObject(continueButton.GetComponent<RectTransform>(), "Format Continue Button");
        Undo.RecordObject(redrawButton.GetComponent<RectTransform>(), "Format Redraw Button");

        // Format Continue Button (bottom-right)
        RectTransform continueRect = continueButton.GetComponent<RectTransform>();
        continueRect.anchorMin = new Vector2(1f, 0f); // Bottom-right anchor
        continueRect.anchorMax = new Vector2(1f, 0f);
        continueRect.pivot = new Vector2(1f, 0f);
        continueRect.anchoredPosition = new Vector2(-20f, 20f); // 20px padding from edges
        continueRect.sizeDelta = new Vector2(180f, 50f); // Button size

        Debug.Log("✓ Formatted Continue Button: Bottom-Right (180x50, 20px padding)");

        // Format Redraw Button (bottom-left)
        RectTransform redrawRect = redrawButton.GetComponent<RectTransform>();
        redrawRect.anchorMin = new Vector2(0f, 0f); // Bottom-left anchor
        redrawRect.anchorMax = new Vector2(0f, 0f);
        redrawRect.pivot = new Vector2(0f, 0f);
        redrawRect.anchoredPosition = new Vector2(20f, 20f); // 20px padding from edges
        redrawRect.sizeDelta = new Vector2(180f, 50f); // Button size

        Debug.Log("✓ Formatted Redraw Button: Bottom-Left (180x50, 20px padding)");

        // Mark scene as dirty
        EditorUtility.SetDirty(continueButton.gameObject);
        EditorUtility.SetDirty(redrawButton.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("========== LAYOUT FORMATTING COMPLETE ==========");
        EditorUtility.DisplayDialog("Success",
            "PlantResultPanel layout formatted successfully!\n\n" +
            "✓ Continue Button: Bottom-Right\n" +
            "✓ Redraw Button: Bottom-Left\n" +
            "✓ Both buttons: 180x50, 20px padding",
            "OK");
    }
}
