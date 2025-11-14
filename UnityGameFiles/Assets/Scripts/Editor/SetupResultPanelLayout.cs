using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to automatically format PlantResultPanel layout and drawing UI
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

        // Make panel window background transparent
        Image panelWindowImage = panelWindow.GetComponent<Image>();
        if (panelWindowImage == null)
        {
            panelWindowImage = panelWindow.AddComponent<Image>();
            Debug.Log("✓ Added Image component to Panel Window");
        }
        Undo.RecordObject(panelWindowImage, "Set Panel Background Color");
        panelWindowImage.color = new Color(0f, 0f, 0f, 0f); // Fully transparent
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

        // Make it transparent (no visual frame, just a spacer)
        Undo.RecordObject(frameImage, "Set Frame Color");
        frameImage.color = new Color(0f, 0f, 0f, 0f); // Fully transparent
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
            titleText.color = Color.white; // White text on dark background
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
            // Color will be set by PlantResultPanel based on element type
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
            // Color will be set by PlantResultPanel based on element type
            EditorUtility.SetDirty(elementText.gameObject);
            Debug.Log($"✓ Formatted Element: Above frame at y={frameTop + 10f}");
        }

        // Create/Format background panel for header (title, name, element)
        Transform existingHeaderBg = panelWindow.transform.Find("HeaderBackground");
        GameObject headerBgObj;
        if (existingHeaderBg != null)
        {
            headerBgObj = existingHeaderBg.gameObject;
        }
        else
        {
            headerBgObj = new GameObject("HeaderBackground");
            headerBgObj.transform.SetParent(panelWindow.transform, false);
            headerBgObj.transform.SetAsFirstSibling(); // Behind text
        }

        RectTransform headerBgRect = headerBgObj.GetComponent<RectTransform>();
        if (headerBgRect == null) headerBgRect = headerBgObj.AddComponent<RectTransform>();

        Image headerBgImage = headerBgObj.GetComponent<Image>();
        if (headerBgImage == null) headerBgImage = headerBgObj.AddComponent<Image>();

        Undo.RecordObject(headerBgRect, "Format Header Background");
        headerBgRect.anchorMin = new Vector2(0.5f, 0.5f);
        headerBgRect.anchorMax = new Vector2(0.5f, 0.5f);
        headerBgRect.pivot = new Vector2(0.5f, 0f);
        headerBgRect.anchoredPosition = new Vector2(0f, frameTop);
        headerBgRect.sizeDelta = new Vector2(drawingWidth + 40f, 150f);

        Undo.RecordObject(headerBgImage, "Set Header Background Color");
        headerBgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Dark semi-transparent
        EditorUtility.SetDirty(headerBgObj);
        Debug.Log("✓ Created header background panel");

        // Format Stats (left of frame) with background
        Transform existingStatsBg = panelWindow.transform.Find("StatsBackground");
        GameObject statsBgObj;
        if (existingStatsBg != null)
        {
            statsBgObj = existingStatsBg.gameObject;
        }
        else
        {
            statsBgObj = new GameObject("StatsBackground");
            statsBgObj.transform.SetParent(panelWindow.transform, false);
            if (statsText != null) statsBgObj.transform.SetSiblingIndex(statsText.transform.GetSiblingIndex());
        }

        RectTransform statsBgRect = statsBgObj.GetComponent<RectTransform>();
        if (statsBgRect == null) statsBgRect = statsBgObj.AddComponent<RectTransform>();

        Image statsBgImage = statsBgObj.GetComponent<Image>();
        if (statsBgImage == null) statsBgImage = statsBgObj.AddComponent<Image>();

        Undo.RecordObject(statsBgRect, "Format Stats Background");
        statsBgRect.anchorMin = new Vector2(0.5f, 0.5f);
        statsBgRect.anchorMax = new Vector2(0.5f, 0.5f);
        statsBgRect.pivot = new Vector2(1f, 0.5f);
        statsBgRect.anchoredPosition = new Vector2(frameLeft - 20f, -50f);
        statsBgRect.sizeDelta = new Vector2(220f, drawingHeight + 20f);

        Undo.RecordObject(statsBgImage, "Set Stats Background Color");
        statsBgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Dark semi-transparent
        EditorUtility.SetDirty(statsBgObj);
        Debug.Log("✓ Created stats background panel");

        if (statsText != null)
        {
            Undo.RecordObject(statsText.GetComponent<RectTransform>(), "Format Stats");
            RectTransform rect = statsText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f); // Pivot on right edge
            rect.anchoredPosition = new Vector2(frameLeft - 30f, -50f); // 30px left of frame (10px padding from bg)
            rect.sizeDelta = new Vector2(200f, drawingHeight);
            statsText.alignment = TextAlignmentOptions.TopLeft;
            statsText.fontSize = 20;
            statsText.color = Color.white; // White text on dark background
            EditorUtility.SetDirty(statsText.gameObject);
            Debug.Log($"✓ Formatted Stats: Left of frame at x={frameLeft - 30f}");
        }

        // Format Moves (right of frame) with background
        Transform existingMovesBg = panelWindow.transform.Find("MovesBackground");
        GameObject movesBgObj;
        if (existingMovesBg != null)
        {
            movesBgObj = existingMovesBg.gameObject;
        }
        else
        {
            movesBgObj = new GameObject("MovesBackground");
            movesBgObj.transform.SetParent(panelWindow.transform, false);
            if (movesText != null) movesBgObj.transform.SetSiblingIndex(movesText.transform.GetSiblingIndex());
        }

        RectTransform movesBgRect = movesBgObj.GetComponent<RectTransform>();
        if (movesBgRect == null) movesBgRect = movesBgObj.AddComponent<RectTransform>();

        Image movesBgImage = movesBgObj.GetComponent<Image>();
        if (movesBgImage == null) movesBgImage = movesBgObj.AddComponent<Image>();

        Undo.RecordObject(movesBgRect, "Format Moves Background");
        movesBgRect.anchorMin = new Vector2(0.5f, 0.5f);
        movesBgRect.anchorMax = new Vector2(0.5f, 0.5f);
        movesBgRect.pivot = new Vector2(0f, 0.5f);
        movesBgRect.anchoredPosition = new Vector2(frameRight + 20f, -50f);
        movesBgRect.sizeDelta = new Vector2(220f, drawingHeight + 20f);

        Undo.RecordObject(movesBgImage, "Set Moves Background Color");
        movesBgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Dark semi-transparent
        EditorUtility.SetDirty(movesBgObj);
        Debug.Log("✓ Created moves background panel");

        if (movesText != null)
        {
            Undo.RecordObject(movesText.GetComponent<RectTransform>(), "Format Moves");
            RectTransform rect = movesText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f); // Pivot on left edge
            rect.anchoredPosition = new Vector2(frameRight + 30f, -50f); // 30px right of frame (10px padding from bg)
            rect.sizeDelta = new Vector2(200f, drawingHeight);
            movesText.alignment = TextAlignmentOptions.TopLeft;
            movesText.fontSize = 20;
            movesText.color = Color.white; // White text on dark background
            EditorUtility.SetDirty(movesText.gameObject);
            Debug.Log($"✓ Formatted Moves: Right of frame at x={frameRight + 30f}");
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

        // ===== FORMAT DRAWING UI ELEMENTS =====
        Debug.Log("========== FORMATTING DRAWING UI ==========");

        // DON'T resize drawing area - leave it as is
        // The result panel frame already matches the drawing area size
        Debug.Log($"✓ Drawing area left unchanged at: {drawingWidth}x{drawingHeight}");

        // Find and format color selector buttons
        DrawingColorSelector colorSelector = FindFirstObjectByType<DrawingColorSelector>(FindObjectsInactive.Include);
        if (colorSelector != null)
        {
            Debug.Log("✓ Found DrawingColorSelector");

            // Find stroke counter text to position buttons next to it
            Canvas mainCanvas = FindFirstObjectByType<Canvas>();
            TextMeshProUGUI strokeCounter = null;
            if (mainCanvas != null)
            {
                // Look for stroke counter in DrawingPanel
                Transform drawingPanel = mainCanvas.transform.Find("DrawingPanel");
                if (drawingPanel != null)
                {
                    // Search for StrokeCounter text
                    foreach (Transform child in drawingPanel)
                    {
                        TextMeshProUGUI[] texts = child.GetComponentsInChildren<TextMeshProUGUI>(true);
                        foreach (var text in texts)
                        {
                            if (text.gameObject.name.Contains("Stroke") || text.gameObject.name.Contains("Counter"))
                            {
                                strokeCounter = text;
                                Debug.Log($"✓ Found stroke counter: {strokeCounter.gameObject.name}");
                                break;
                            }
                        }
                        if (strokeCounter != null) break;
                    }
                }
            }

            // Resize color buttons
            if (colorSelector.redButton != null)
            {
                Undo.RecordObject(colorSelector.redButton.GetComponent<RectTransform>(), "Resize Red Button");
                RectTransform rect = colorSelector.redButton.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(80f, 80f); // Larger button size
                EditorUtility.SetDirty(colorSelector.redButton.gameObject);
                Debug.Log("✓ Resized Red button to 80x80");
            }

            if (colorSelector.greenButton != null)
            {
                Undo.RecordObject(colorSelector.greenButton.GetComponent<RectTransform>(), "Resize Green Button");
                RectTransform rect = colorSelector.greenButton.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(80f, 80f);
                EditorUtility.SetDirty(colorSelector.greenButton.gameObject);
                Debug.Log("✓ Resized Green button to 80x80");
            }

            if (colorSelector.blueButton != null)
            {
                Undo.RecordObject(colorSelector.blueButton.GetComponent<RectTransform>(), "Resize Blue Button");
                RectTransform rect = colorSelector.blueButton.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(80f, 80f);
                EditorUtility.SetDirty(colorSelector.blueButton.gameObject);
                Debug.Log("✓ Resized Blue button to 80x80");
            }

            // If stroke counter found, reposition buttons next to it
            if (strokeCounter != null)
            {
                RectTransform counterRect = strokeCounter.GetComponent<RectTransform>();

                // Position color buttons horizontally next to stroke counter
                if (colorSelector.redButton != null)
                {
                    Undo.RecordObject(colorSelector.redButton.GetComponent<RectTransform>(), "Reposition Red Button");
                    RectTransform rect = colorSelector.redButton.GetComponent<RectTransform>();
                    rect.anchorMin = counterRect.anchorMin;
                    rect.anchorMax = counterRect.anchorMax;
                    rect.pivot = new Vector2(0f, 0.5f);
                    rect.anchoredPosition = new Vector2(counterRect.anchoredPosition.x + 150f, counterRect.anchoredPosition.y);
                    EditorUtility.SetDirty(colorSelector.redButton.gameObject);
                    Debug.Log("✓ Repositioned Red button next to stroke counter");
                }

                if (colorSelector.greenButton != null)
                {
                    Undo.RecordObject(colorSelector.greenButton.GetComponent<RectTransform>(), "Reposition Green Button");
                    RectTransform rect = colorSelector.greenButton.GetComponent<RectTransform>();
                    rect.anchorMin = counterRect.anchorMin;
                    rect.anchorMax = counterRect.anchorMax;
                    rect.pivot = new Vector2(0f, 0.5f);
                    rect.anchoredPosition = new Vector2(counterRect.anchoredPosition.x + 240f, counterRect.anchoredPosition.y);
                    EditorUtility.SetDirty(colorSelector.greenButton.gameObject);
                    Debug.Log("✓ Repositioned Green button next to stroke counter");
                }

                if (colorSelector.blueButton != null)
                {
                    Undo.RecordObject(colorSelector.blueButton.GetComponent<RectTransform>(), "Reposition Blue Button");
                    RectTransform rect = colorSelector.blueButton.GetComponent<RectTransform>();
                    rect.anchorMin = counterRect.anchorMin;
                    rect.anchorMax = counterRect.anchorMax;
                    rect.pivot = new Vector2(0f, 0.5f);
                    rect.anchoredPosition = new Vector2(counterRect.anchoredPosition.x + 330f, counterRect.anchoredPosition.y);
                    EditorUtility.SetDirty(colorSelector.blueButton.gameObject);
                    Debug.Log("✓ Repositioned Blue button next to stroke counter");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ DrawingColorSelector not found - color buttons not formatted");
        }

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("========== LAYOUT FORMATTING COMPLETE ==========");
        EditorUtility.DisplayDialog("Success",
            "PlantResultPanel and Drawing UI formatted successfully!\n\n" +
            "✓ Result Frame: Matches drawing area (" + drawingWidth + "x" + drawingHeight + ")\n" +
            "✓ Color Buttons: Resized to 80x80 and repositioned\n" +
            "✓ Result Panel: Dark backgrounds for readability\n" +
            "✓ Title/Type: Above frame (white text)\n" +
            "✓ Stats: Left of frame (white text on dark bg)\n" +
            "✓ Moves: Right of frame (white text on dark bg)\n" +
            "✓ Buttons: Below frame",
            "OK");
    }
}
