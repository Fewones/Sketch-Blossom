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

        // Find the drawing area to get its dimensions
        SimpleDrawingCanvas drawingCanvas = FindFirstObjectByType<SimpleDrawingCanvas>(FindObjectsInactive.Include);
        RectTransform drawingAreaRect = null;
        float drawingWidth = 800f;  // Default fallback
        float drawingHeight = 600f; // Default fallback

        if (drawingCanvas != null && drawingCanvas.drawingArea != null)
        {
            drawingAreaRect = drawingCanvas.drawingArea;
            drawingWidth = drawingAreaRect.rect.width;
            drawingHeight = drawingAreaRect.rect.height;
            Debug.Log($"✓ Found DrawingArea: {drawingWidth}x{drawingHeight}");
        }
        else
        {
            Debug.LogWarning("⚠️ DrawingArea not found, using default size");
        }

        // Add/Update background color for panel window (transparent so drawing shows through)
        Image panelWindowImage = panelWindow.GetComponent<Image>();
        if (panelWindowImage == null)
        {
            panelWindowImage = panelWindow.AddComponent<Image>();
            Debug.Log("✓ Added Image component to Panel Window");
        }
        Undo.RecordObject(panelWindowImage, "Set Panel Background Color");
        panelWindowImage.color = new Color(0.95f, 0.95f, 0.95f, 0f); // Transparent - let drawing show through
        EditorUtility.SetDirty(panelWindowImage);
        Debug.Log("✓ Set Panel Window background to transparent");

        // Create or find a visual frame rectangle in the center
        Transform existingFrame = panelWindow.transform.Find("DrawingFrame");
        GameObject frameObj;
        if (existingFrame != null)
        {
            frameObj = existingFrame.gameObject;
            Debug.Log("✓ Found existing DrawingFrame");
        }
        else
        {
            frameObj = new GameObject("DrawingFrame");
            frameObj.transform.SetParent(panelWindow.transform, false);
            Debug.Log("✓ Created new DrawingFrame");
        }

        RectTransform frameRect = frameObj.GetComponent<RectTransform>();
        if (frameRect == null)
        {
            frameRect = frameObj.AddComponent<RectTransform>();
        }

        Image frameImage = frameObj.GetComponent<Image>();
        if (frameImage == null)
        {
            frameImage = frameObj.AddComponent<Image>();
        }

        // Position the frame in the center (same size as drawing area)
        Undo.RecordObject(frameRect, "Format Drawing Frame");
        frameRect.anchorMin = new Vector2(0.5f, 0.5f);
        frameRect.anchorMax = new Vector2(0.5f, 0.5f);
        frameRect.pivot = new Vector2(0.5f, 0.5f);
        frameRect.anchoredPosition = new Vector2(0f, -50f); // Slight offset down from center
        frameRect.sizeDelta = new Vector2(drawingWidth, drawingHeight);

        // Make it a subtle outline frame
        Undo.RecordObject(frameImage, "Set Frame Color");
        frameImage.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Semi-transparent gray
        EditorUtility.SetDirty(frameObj);
        Debug.Log($"✓ Created frame rectangle: {drawingWidth}x{drawingHeight}");

        // Calculate positions based on frame
        float frameTop = -50f + drawingHeight / 2f;
        float frameBottom = -50f - drawingHeight / 2f;
        float frameLeft = -drawingWidth / 2f;
        float frameRight = drawingWidth / 2f;

        // Format Title (above frame, centered)
        if (titleText != null)
        {
            Undo.RecordObject(titleText.GetComponent<RectTransform>(), "Format Title");
            RectTransform rect = titleText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, frameTop + 100f); // 100px above frame
            rect.sizeDelta = new Vector2(drawingWidth, 40f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 28;
            EditorUtility.SetDirty(titleText.gameObject);
            Debug.Log($"✓ Formatted Title: Above frame at y={frameTop + 100f}");
        }

        // Format Plant Name (above frame, below title)
        if (plantNameText != null)
        {
            Undo.RecordObject(plantNameText.GetComponent<RectTransform>(), "Format Plant Name");
            RectTransform rect = plantNameText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, frameTop + 50f); // 50px above frame
            rect.sizeDelta = new Vector2(drawingWidth, 40f);
            plantNameText.alignment = TextAlignmentOptions.Center;
            plantNameText.fontSize = 36;
            EditorUtility.SetDirty(plantNameText.gameObject);
            Debug.Log($"✓ Formatted Plant Name: Above frame at y={frameTop + 50f}");
        }

        // Format Element Text (directly above frame)
        if (elementText != null)
        {
            Undo.RecordObject(elementText.GetComponent<RectTransform>(), "Format Element");
            RectTransform rect = elementText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, frameTop + 10f); // 10px above frame
            rect.sizeDelta = new Vector2(drawingWidth, 30f);
            elementText.alignment = TextAlignmentOptions.Center;
            elementText.fontSize = 22;
            EditorUtility.SetDirty(elementText.gameObject);
            Debug.Log($"✓ Formatted Element: Above frame at y={frameTop + 10f}");
        }

        // Format Stats (left of frame)
        if (statsText != null)
        {
            Undo.RecordObject(statsText.GetComponent<RectTransform>(), "Format Stats");
            RectTransform rect = statsText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f); // Pivot on right edge
            rect.anchoredPosition = new Vector2(frameLeft - 20f, -50f); // 20px left of frame
            rect.sizeDelta = new Vector2(200f, drawingHeight);
            statsText.alignment = TextAlignmentOptions.TopLeft;
            statsText.fontSize = 20;
            EditorUtility.SetDirty(statsText.gameObject);
            Debug.Log($"✓ Formatted Stats: Left of frame at x={frameLeft - 20f}");
        }

        // Format Moves (right of frame)
        if (movesText != null)
        {
            Undo.RecordObject(movesText.GetComponent<RectTransform>(), "Format Moves");
            RectTransform rect = movesText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f); // Pivot on left edge
            rect.anchoredPosition = new Vector2(frameRight + 20f, -50f); // 20px right of frame
            rect.sizeDelta = new Vector2(200f, drawingHeight);
            movesText.alignment = TextAlignmentOptions.TopLeft;
            movesText.fontSize = 20;
            EditorUtility.SetDirty(movesText.gameObject);
            Debug.Log($"✓ Formatted Moves: Right of frame at x={frameRight + 20f}");
        }

        // Format Continue Button (below frame, right side)
        if (continueButton != null)
        {
            Undo.RecordObject(continueButton.GetComponent<RectTransform>(), "Format Continue Button");
            RectTransform rect = continueButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(frameRight, frameBottom - 35f); // Below frame, aligned right
            rect.sizeDelta = new Vector2(180f, 50f);
            EditorUtility.SetDirty(continueButton.gameObject);
            Debug.Log($"✓ Formatted Continue Button: Below frame at y={frameBottom - 35f}");
        }

        // Format Redraw Button (below frame, left side)
        if (redrawButton != null)
        {
            Undo.RecordObject(redrawButton.GetComponent<RectTransform>(), "Format Redraw Button");
            RectTransform rect = redrawButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(frameLeft, frameBottom - 35f); // Below frame, aligned left
            rect.sizeDelta = new Vector2(180f, 50f);
            EditorUtility.SetDirty(redrawButton.gameObject);
            Debug.Log($"✓ Formatted Redraw Button: Below frame at y={frameBottom - 35f}");
        }

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("========== LAYOUT FORMATTING COMPLETE ==========");
        EditorUtility.DisplayDialog("Success",
            "PlantResultPanel layout formatted successfully!\n\n" +
            "✓ Drawing Frame: Center (" + drawingWidth + "x" + drawingHeight + ")\n" +
            "✓ Title/Type: Above frame\n" +
            "✓ Stats: Left of frame\n" +
            "✓ Moves: Right of frame\n" +
            "✓ Buttons: Below frame\n" +
            "✓ Background: Transparent (drawing visible)",
            "OK");
    }
}
