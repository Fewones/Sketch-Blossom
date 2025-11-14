using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Restores the drawing screen to its original working layout
/// </summary>
public class RestoreDrawingScreenLayout : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Restore Drawing Screen Layout")]
    static void RestoreLayout()
    {
        Debug.Log("========== RESTORING DRAWING SCREEN LAYOUT ==========");

        // Find main canvas
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Canvas not found!", "OK");
            return;
        }

        // Find DrawingPanel
        Transform drawingPanelTransform = mainCanvas.transform.Find("DrawingPanel");
        if (drawingPanelTransform == null)
        {
            EditorUtility.DisplayDialog("Error", "DrawingPanel not found in Canvas!", "OK");
            return;
        }

        GameObject drawingPanel = drawingPanelTransform.gameObject;
        RectTransform panelRect = drawingPanel.GetComponent<RectTransform>();

        // Set DrawingPanel to fill screen
        Undo.RecordObject(panelRect, "Restore DrawingPanel Size");
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        EditorUtility.SetDirty(drawingPanel);
        Debug.Log("✓ DrawingPanel set to fill screen");

        // Find and fix DrawingArea (the white rectangle where you draw)
        SimpleDrawingCanvas drawingCanvas = FindFirstObjectByType<SimpleDrawingCanvas>(FindObjectsInactive.Include);

        // Verify drawingArea is assigned to the correct white rectangle GameObject
        if (drawingCanvas != null && drawingPanelTransform != null)
        {
            Transform drawingAreaTransform = drawingPanelTransform.Find("DrawingArea");
            if (drawingAreaTransform != null)
            {
                RectTransform drawingAreaRect = drawingAreaTransform.GetComponent<RectTransform>();
                if (drawingCanvas.drawingArea != drawingAreaRect)
                {
                    Undo.RecordObject(drawingCanvas, "Assign DrawingArea Reference");
                    drawingCanvas.drawingArea = drawingAreaRect;
                    EditorUtility.SetDirty(drawingCanvas);
                    Debug.Log("✓ Assigned white DrawingArea rectangle to SimpleDrawingCanvas");
                }
            }
            else
            {
                Debug.LogError("❌ Could not find 'DrawingArea' GameObject in DrawingPanel! Please ensure the white rectangle is named 'DrawingArea'");
            }
        }

        if (drawingCanvas != null && drawingCanvas.drawingArea != null)
        {
            RectTransform drawingAreaRect = drawingCanvas.drawingArea;
            Undo.RecordObject(drawingAreaRect, "Restore DrawingArea Size");

            // Make it larger to fill most of the screen
            drawingAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
            drawingAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
            drawingAreaRect.pivot = new Vector2(0.5f, 0.5f);
            drawingAreaRect.anchoredPosition = new Vector2(0f, -30f); // Slightly down to leave room for UI
            drawingAreaRect.sizeDelta = new Vector2(1700f, 950f); // Wider for better screen fit

            // Add RectMask2D to clip drawing to this area
            UnityEngine.UI.RectMask2D mask = drawingAreaRect.GetComponent<UnityEngine.UI.RectMask2D>();
            if (mask == null)
            {
                mask = Undo.AddComponent<UnityEngine.UI.RectMask2D>(drawingAreaRect.gameObject);
                Debug.Log("✓ Added RectMask2D to clip drawing to bounds");
            }

            EditorUtility.SetDirty(drawingAreaRect.gameObject);
            Debug.Log("✓ DrawingArea restored to 1700x950 centered with clipping mask");
        }

        // Ensure stroke container is a child of drawing area for proper clipping
        if (drawingCanvas != null && drawingCanvas.strokeContainer != null && drawingCanvas.drawingArea != null)
        {
            if (drawingCanvas.strokeContainer.parent != drawingCanvas.drawingArea)
            {
                Undo.SetTransformParent(drawingCanvas.strokeContainer, drawingCanvas.drawingArea, "Move stroke container to drawing area");
                Debug.Log("✓ Moved stroke container under drawing area for proper clipping");
            }
        }

        // Calculate position above the white DrawingArea rectangle
        float uiYPosition = 0f;
        if (drawingCanvas != null && drawingCanvas.drawingArea != null)
        {
            RectTransform drawingAreaRect = drawingCanvas.drawingArea;
            // Calculate the top edge of the drawing area: center Y + half height
            float drawingAreaTop = drawingAreaRect.anchoredPosition.y + (drawingAreaRect.sizeDelta.y / 2f);
            uiYPosition = drawingAreaTop + 40f; // 40 pixels above the white rectangle
            Debug.Log($"DrawingArea top edge at Y={drawingAreaTop}, positioning UI at Y={uiYPosition}");
        }

        // Find and position stroke counter centered above the WHITE drawing area rectangle
        if (drawingPanelTransform != null)
        {
            // Find StrokeCounter text
            Transform strokeCounterTransform = drawingPanelTransform.Find("StrokeCounter");
            if (strokeCounterTransform != null)
            {
                RectTransform strokeRect = strokeCounterTransform.GetComponent<RectTransform>();
                Undo.RecordObject(strokeRect, "Position Stroke Counter");

                // Position above the white DrawingArea rectangle
                strokeRect.anchorMin = new Vector2(0.5f, 0.5f);
                strokeRect.anchorMax = new Vector2(0.5f, 0.5f);
                strokeRect.pivot = new Vector2(1f, 0.5f); // Right-aligned pivot for stroke counter
                strokeRect.anchoredPosition = new Vector2(-180f, uiYPosition);
                strokeRect.sizeDelta = new Vector2(150f, 40f);

                // Increase font size if it has TextMeshProUGUI
                TextMeshProUGUI strokeText = strokeCounterTransform.GetComponent<TextMeshProUGUI>();
                if (strokeText != null)
                {
                    Undo.RecordObject(strokeText, "Increase Stroke Counter Text");
                    strokeText.fontSize = 20f;
                    strokeText.alignment = TextAlignmentOptions.Right;
                    EditorUtility.SetDirty(strokeText.gameObject);
                }

                EditorUtility.SetDirty(strokeCounterTransform.gameObject);
                Debug.Log("✓ Stroke counter positioned above white DrawingArea rectangle");
            }
        }

        // Find and reposition color buttons horizontally above the WHITE drawing area rectangle
        DrawingColorSelector colorSelector = FindFirstObjectByType<DrawingColorSelector>(FindObjectsInactive.Include);
        if (colorSelector != null)
        {
            // Position buttons horizontally in a row, centered above the white DrawingArea
            // Red button - first in row
            if (colorSelector.redButton != null)
            {
                Undo.RecordObject(colorSelector.redButton.GetComponent<RectTransform>(), "Restore Red Button");
                RectTransform rect = colorSelector.redButton.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(90f, 50f); // Wider buttons
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(-145f, uiYPosition); // Above white DrawingArea, left of center

                // Increase text size
                TextMeshProUGUI buttonText = colorSelector.redButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    Undo.RecordObject(buttonText, "Increase Red Button Text");
                    buttonText.fontSize = 16f;
                    EditorUtility.SetDirty(buttonText.gameObject);
                }

                EditorUtility.SetDirty(colorSelector.redButton.gameObject);
                Debug.Log("✓ Red button repositioned to 90x50");
            }

            // Green button - second in row (center)
            if (colorSelector.greenButton != null)
            {
                Undo.RecordObject(colorSelector.greenButton.GetComponent<RectTransform>(), "Restore Green Button");
                RectTransform rect = colorSelector.greenButton.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(90f, 50f);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(-45f, uiYPosition); // Above white DrawingArea, center

                // Increase text size
                TextMeshProUGUI buttonText = colorSelector.greenButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    Undo.RecordObject(buttonText, "Increase Green Button Text");
                    buttonText.fontSize = 16f;
                    EditorUtility.SetDirty(buttonText.gameObject);
                }

                EditorUtility.SetDirty(colorSelector.greenButton.gameObject);
                Debug.Log("✓ Green button repositioned to 90x50");
            }

            // Blue button - third in row (right)
            if (colorSelector.blueButton != null)
            {
                Undo.RecordObject(colorSelector.blueButton.GetComponent<RectTransform>(), "Restore Blue Button");
                RectTransform rect = colorSelector.blueButton.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(90f, 50f);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(55f, uiYPosition); // Above white DrawingArea, right of center

                // Increase text size
                TextMeshProUGUI buttonText = colorSelector.blueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    Undo.RecordObject(buttonText, "Increase Blue Button Text");
                    buttonText.fontSize = 16f;
                    EditorUtility.SetDirty(buttonText.gameObject);
                }

                EditorUtility.SetDirty(colorSelector.blueButton.gameObject);
                Debug.Log("✓ Blue button repositioned to 90x50");
            }

            Debug.Log("✓ Color buttons repositioned horizontally above white DrawingArea rectangle");
        }


        // Find and restore result panel to simple centered layout
        PlantResultPanel resultPanel = FindFirstObjectByType<PlantResultPanel>(FindObjectsInactive.Include);
        if (resultPanel != null)
        {
            var panelWindowField = typeof(PlantResultPanel).GetField("panelWindow",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            GameObject panelWindow = panelWindowField?.GetValue(resultPanel) as GameObject;

            if (panelWindow != null)
            {
                RectTransform windowRect = panelWindow.GetComponent<RectTransform>();
                Undo.RecordObject(windowRect, "Restore Result Panel");

                // Center it with reasonable size
                windowRect.anchorMin = new Vector2(0.5f, 0.5f);
                windowRect.anchorMax = new Vector2(0.5f, 0.5f);
                windowRect.pivot = new Vector2(0.5f, 0.5f);
                windowRect.anchoredPosition = Vector2.zero;
                windowRect.sizeDelta = new Vector2(600f, 500f);

                EditorUtility.SetDirty(panelWindow);
                Debug.Log("✓ Result panel restored to centered 600x500");

                // Remove any extra background objects we created
                Transform headerBg = panelWindow.transform.Find("HeaderBackground");
                if (headerBg != null)
                {
                    Undo.DestroyObjectImmediate(headerBg.gameObject);
                    Debug.Log("✓ Removed HeaderBackground");
                }

                Transform statsBg = panelWindow.transform.Find("StatsBackground");
                if (statsBg != null)
                {
                    Undo.DestroyObjectImmediate(statsBg.gameObject);
                    Debug.Log("✓ Removed StatsBackground");
                }

                Transform movesBg = panelWindow.transform.Find("MovesBackground");
                if (movesBg != null)
                {
                    Undo.DestroyObjectImmediate(movesBg.gameObject);
                    Debug.Log("✓ Removed MovesBackground");
                }

                Transform drawingFrame = panelWindow.transform.Find("DrawingFrame");
                if (drawingFrame != null)
                {
                    Undo.DestroyObjectImmediate(drawingFrame.gameObject);
                    Debug.Log("✓ Removed DrawingFrame");
                }

                // Set panel window background to semi-transparent gray
                Image panelImage = panelWindow.GetComponent<Image>();
                if (panelImage != null)
                {
                    Undo.RecordObject(panelImage, "Set Panel Background");
                    panelImage.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
                    EditorUtility.SetDirty(panelImage);
                }
            }

            // Reset text elements to simple centered layout
            var titleTextField = typeof(PlantResultPanel).GetField("titleText",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var plantNameTextField = typeof(PlantResultPanel).GetField("plantNameText",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var elementTextField = typeof(PlantResultPanel).GetField("elementText",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var statsTextField = typeof(PlantResultPanel).GetField("statsText",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var movesTextField = typeof(PlantResultPanel).GetField("movesText",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var continueButtonField = typeof(PlantResultPanel).GetField("continueButton",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var redrawButtonField = typeof(PlantResultPanel).GetField("redrawButton",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            TextMeshProUGUI titleText = titleTextField?.GetValue(resultPanel) as TextMeshProUGUI;
            TextMeshProUGUI plantNameText = plantNameTextField?.GetValue(resultPanel) as TextMeshProUGUI;
            TextMeshProUGUI elementText = elementTextField?.GetValue(resultPanel) as TextMeshProUGUI;
            TextMeshProUGUI statsText = statsTextField?.GetValue(resultPanel) as TextMeshProUGUI;
            TextMeshProUGUI movesText = movesTextField?.GetValue(resultPanel) as TextMeshProUGUI;
            Button continueButton = continueButtonField?.GetValue(resultPanel) as Button;
            Button redrawButton = redrawButtonField?.GetValue(resultPanel) as Button;

            // Simple vertical layout
            float yPos = 200f;

            if (titleText != null)
            {
                Undo.RecordObject(titleText.GetComponent<RectTransform>(), "Restore Title");
                RectTransform rect = titleText.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -20f);
                rect.sizeDelta = new Vector2(500f, 40f);
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.fontSize = 24;
                titleText.color = Color.black;
                EditorUtility.SetDirty(titleText.gameObject);
            }

            if (plantNameText != null)
            {
                Undo.RecordObject(plantNameText.GetComponent<RectTransform>(), "Restore Plant Name");
                RectTransform rect = plantNameText.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -70f);
                rect.sizeDelta = new Vector2(500f, 50f);
                plantNameText.alignment = TextAlignmentOptions.Center;
                plantNameText.fontSize = 36;
                EditorUtility.SetDirty(plantNameText.gameObject);
            }

            if (elementText != null)
            {
                Undo.RecordObject(elementText.GetComponent<RectTransform>(), "Restore Element");
                RectTransform rect = elementText.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -130f);
                rect.sizeDelta = new Vector2(500f, 30f);
                elementText.alignment = TextAlignmentOptions.Center;
                elementText.fontSize = 20;
                EditorUtility.SetDirty(elementText.gameObject);
            }

            if (statsText != null)
            {
                Undo.RecordObject(statsText.GetComponent<RectTransform>(), "Restore Stats");
                RectTransform rect = statsText.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(-120f, 0f);
                rect.sizeDelta = new Vector2(200f, 150f);
                statsText.alignment = TextAlignmentOptions.TopLeft;
                statsText.fontSize = 18;
                statsText.color = Color.black;
                EditorUtility.SetDirty(statsText.gameObject);
            }

            if (movesText != null)
            {
                Undo.RecordObject(movesText.GetComponent<RectTransform>(), "Restore Moves");
                RectTransform rect = movesText.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(120f, 0f);
                rect.sizeDelta = new Vector2(200f, 150f);
                movesText.alignment = TextAlignmentOptions.TopLeft;
                movesText.fontSize = 18;
                movesText.color = Color.black;
                EditorUtility.SetDirty(movesText.gameObject);
            }

            if (continueButton != null)
            {
                Undo.RecordObject(continueButton.GetComponent<RectTransform>(), "Restore Continue Button");
                RectTransform rect = continueButton.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
                rect.anchoredPosition = new Vector2(-20f, 20f);
                rect.sizeDelta = new Vector2(150f, 40f);
                EditorUtility.SetDirty(continueButton.gameObject);
            }

            if (redrawButton != null)
            {
                Undo.RecordObject(redrawButton.GetComponent<RectTransform>(), "Restore Redraw Button");
                RectTransform rect = redrawButton.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(20f, 20f);
                rect.sizeDelta = new Vector2(150f, 40f);
                EditorUtility.SetDirty(redrawButton.gameObject);
            }

            Debug.Log("✓ Result panel text and buttons restored to original layout");
        }

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        // Clear selection to prevent accessing destroyed objects
        UnityEditor.Selection.activeObject = null;

        Debug.Log("========== DRAWING SCREEN RESTORED ==========");
        EditorUtility.DisplayDialog("Success",
            "Drawing screen layout has been updated!\n\n" +
            "✓ DrawingPanel: Full screen\n" +
            "✓ DrawingArea: 1700x950 with RectMask2D clipping\n" +
            "✓ Stroke Container: Parented to drawing area for clipping\n" +
            "✓ Stroke Counter: Centered above drawing rectangle\n" +
            "✓ Color Buttons: Horizontal toolbar above drawing rectangle\n" +
            "✓ Button Text: Increased to 16pt\n" +
            "✓ Drawing restricted to white rectangle bounds\n" +
            "✓ Result Panel: Simple centered layout\n" +
            "✓ All extra backgrounds removed",
            "OK");
    }
}
