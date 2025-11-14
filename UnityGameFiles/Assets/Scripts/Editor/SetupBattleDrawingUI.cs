using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor script to automatically setup the battle drawing UI
/// Creates a drawing panel between the stat displays
/// </summary>
public class SetupBattleDrawingUI : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Setup Battle Drawing UI")]
    public static void SetupBattleDrawing()
    {
        Debug.Log("========== SETTING UP BATTLE DRAWING UI ==========");

        // Find or create Battle Drawing Manager
        BattleDrawingManager manager = FindObjectOfType<BattleDrawingManager>();
        if (manager == null)
        {
            GameObject managerObj = new GameObject("BattleDrawingManager");
            manager = managerObj.AddComponent<BattleDrawingManager>();
            Debug.Log("✓ Created BattleDrawingManager");
        }
        else
        {
            Debug.Log("✓ Found existing BattleDrawingManager");
        }

        // Find the Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error",
                "No Canvas found in scene!\n\n" +
                "Please create a Canvas first.",
                "OK");
            return;
        }

        Debug.Log($"✓ Using Canvas: {canvas.gameObject.name}");

        // Create Drawing Panel if it doesn't exist
        GameObject drawingPanel = GameObject.Find("BattleDrawingPanel");
        if (drawingPanel == null)
        {
            drawingPanel = new GameObject("BattleDrawingPanel");
            drawingPanel.transform.SetParent(canvas.transform, false);

            // Add RectTransform
            RectTransform panelRect = drawingPanel.AddComponent<RectTransform>();

            // Position at bottom center
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0, 50); // 50 pixels from bottom
            panelRect.sizeDelta = new Vector2(450, 350); // 450 wide, 350 tall

            // Add background image
            Image panelImage = drawingPanel.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Dark gray, semi-transparent

            Debug.Log("✓ Created BattleDrawingPanel");
        }
        else
        {
            Debug.Log("✓ Found existing BattleDrawingPanel");
        }

        RectTransform drawingPanelRect = drawingPanel.GetComponent<RectTransform>();

        // Create Drawing Area (where player draws)
        Transform drawingAreaTransform = drawingPanel.transform.Find("DrawingArea");
        GameObject drawingArea;
        if (drawingAreaTransform == null)
        {
            drawingArea = new GameObject("DrawingArea");
            drawingArea.transform.SetParent(drawingPanel.transform, false);

            RectTransform areaRect = drawingArea.AddComponent<RectTransform>();
            areaRect.anchorMin = new Vector2(0.5f, 0.5f);
            areaRect.anchorMax = new Vector2(0.5f, 0.5f);
            areaRect.pivot = new Vector2(0.5f, 0.5f);
            areaRect.anchoredPosition = new Vector2(0, 20); // Slight offset up
            areaRect.sizeDelta = new Vector2(400, 250);

            // Add visual background
            RawImage areaImage = drawingArea.AddComponent<RawImage>();
            areaImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Darker gray

            Debug.Log("✓ Created DrawingArea");
        }
        else
        {
            drawingArea = drawingAreaTransform.gameObject;
            Debug.Log("✓ Found existing DrawingArea");
        }

        // Create Instruction Text
        Transform instructionTransform = drawingPanel.transform.Find("InstructionText");
        GameObject instructionTextObj;
        if (instructionTransform == null)
        {
            instructionTextObj = new GameObject("InstructionText");
            instructionTextObj.transform.SetParent(drawingPanel.transform, false);

            RectTransform textRect = instructionTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 1f);
            textRect.anchorMax = new Vector2(0.5f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = new Vector2(0, -10);
            textRect.sizeDelta = new Vector2(400, 60);

            TextMeshProUGUI instructionText = instructionTextObj.AddComponent<TextMeshProUGUI>();
            instructionText.text = "Draw your move!";
            instructionText.fontSize = 18;
            instructionText.alignment = TextAlignmentOptions.Center;
            instructionText.color = Color.white;

            Debug.Log("✓ Created InstructionText");
        }
        else
        {
            instructionTextObj = instructionTransform.gameObject;
            Debug.Log("✓ Found existing InstructionText");
        }

        // Create Submit Button
        Transform submitTransform = drawingPanel.transform.Find("SubmitButton");
        GameObject submitButton;
        if (submitTransform == null)
        {
            submitButton = new GameObject("SubmitButton");
            submitButton.transform.SetParent(drawingPanel.transform, false);

            RectTransform btnRect = submitButton.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.7f, 0f);
            btnRect.anchorMax = new Vector2(0.7f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0, 10);
            btnRect.sizeDelta = new Vector2(120, 40);

            // Add Button component
            Button btn = submitButton.AddComponent<Button>();
            Image btnImage = submitButton.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green

            // Add text
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(submitButton.transform, false);
            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "Submit";
            btnText.fontSize = 20;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;

            Debug.Log("✓ Created SubmitButton");
        }
        else
        {
            submitButton = submitTransform.gameObject;
            Debug.Log("✓ Found existing SubmitButton");
        }

        // Create Clear Button
        Transform clearTransform = drawingPanel.transform.Find("ClearButton");
        GameObject clearButton;
        if (clearTransform == null)
        {
            clearButton = new GameObject("ClearButton");
            clearButton.transform.SetParent(drawingPanel.transform, false);

            RectTransform btnRect = clearButton.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.3f, 0f);
            btnRect.anchorMax = new Vector2(0.3f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0, 10);
            btnRect.sizeDelta = new Vector2(120, 40);

            // Add Button component
            Button btn = clearButton.AddComponent<Button>();
            Image btnImage = clearButton.AddComponent<Image>();
            btnImage.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Red

            // Add text
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(clearButton.transform, false);
            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "Clear";
            btnText.fontSize = 20;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;

            Debug.Log("✓ Created ClearButton");
        }
        else
        {
            clearButton = clearTransform.gameObject;
            Debug.Log("✓ Found existing ClearButton");
        }

        // Create or find SimpleDrawingCanvas
        SimpleDrawingCanvas drawingCanvas = FindObjectOfType<SimpleDrawingCanvas>();
        if (drawingCanvas == null)
        {
            GameObject canvasObj = new GameObject("BattleDrawingCanvas");
            canvasObj.transform.SetParent(canvas.transform, false);
            drawingCanvas = canvasObj.AddComponent<SimpleDrawingCanvas>();

            // Create stroke container
            GameObject strokeContainer = new GameObject("StrokeContainer");
            strokeContainer.transform.SetParent(canvasObj.transform, false);

            Debug.Log("✓ Created SimpleDrawingCanvas");
        }
        else
        {
            Debug.Log("✓ Found existing SimpleDrawingCanvas");
        }

        // Wire up BattleDrawingManager
        SerializedObject managerSO = new SerializedObject(manager);

        SerializedProperty drawingCanvasProp = managerSO.FindProperty("drawingCanvas");
        drawingCanvasProp.objectReferenceValue = drawingCanvas;

        SerializedProperty drawingPanelProp = managerSO.FindProperty("drawingPanel");
        drawingPanelProp.objectReferenceValue = drawingPanel;

        SerializedProperty drawingAreaImageProp = managerSO.FindProperty("drawingAreaImage");
        drawingAreaImageProp.objectReferenceValue = drawingArea.GetComponent<RawImage>();

        SerializedProperty submitButtonProp = managerSO.FindProperty("submitButton");
        submitButtonProp.objectReferenceValue = submitButton.GetComponent<Button>();

        SerializedProperty clearButtonProp = managerSO.FindProperty("clearButton");
        clearButtonProp.objectReferenceValue = clearButton.GetComponent<Button>();

        SerializedProperty instructionTextProp = managerSO.FindProperty("instructionText");
        instructionTextProp.objectReferenceValue = instructionTextObj.GetComponent<TextMeshProUGUI>();

        managerSO.ApplyModifiedProperties();

        // Wire up SimpleDrawingCanvas
        SerializedObject canvasSO = new SerializedObject(drawingCanvas);

        SerializedProperty mainCameraProp = canvasSO.FindProperty("mainCamera");
        if (mainCameraProp.objectReferenceValue == null)
        {
            mainCameraProp.objectReferenceValue = Camera.main;
        }

        SerializedProperty drawingAreaProp = canvasSO.FindProperty("drawingArea");
        drawingAreaProp.objectReferenceValue = drawingArea.GetComponent<RectTransform>();

        SerializedProperty strokeContainerProp = canvasSO.FindProperty("strokeContainer");
        if (strokeContainerProp.objectReferenceValue == null)
        {
            Transform container = drawingCanvas.transform.Find("StrokeContainer");
            if (container != null)
            {
                strokeContainerProp.objectReferenceValue = container;
            }
        }

        canvasSO.ApplyModifiedProperties();

        // Wire up CombatManager
        CombatManager combatManager = FindObjectOfType<CombatManager>();
        if (combatManager != null)
        {
            SerializedObject combatSO = new SerializedObject(combatManager);
            SerializedProperty battleDrawingManagerProp = combatSO.FindProperty("battleDrawingManager");
            battleDrawingManagerProp.objectReferenceValue = manager;
            combatSO.ApplyModifiedProperties();
            Debug.Log("✓ Wired up CombatManager");
        }

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("========== SETUP COMPLETE ==========");

        EditorUtility.DisplayDialog("Success!",
            "Battle Drawing UI has been set up!\n\n" +
            "Components created:\n" +
            "• BattleDrawingPanel (bottom center)\n" +
            "• DrawingArea (400x250)\n" +
            "• Submit and Clear buttons\n" +
            "• SimpleDrawingCanvas\n" +
            "• BattleDrawingManager\n\n" +
            "Make sure to save your scene!",
            "OK");
    }
}
