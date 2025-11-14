using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Rebuilds the BattleScene drawing system from scratch
/// Run this AFTER cleaning up old systems
/// </summary>
public class RebuildBattleDrawingSystem : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Battle Scene/2. Rebuild Drawing System")]
    public static void RebuildDrawingSystem()
    {
        Debug.Log("========== REBUILDING BATTLE DRAWING SYSTEM ==========");

        // Verify we're in a battle scene
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Battle"))
        {
            EditorUtility.DisplayDialog("Wrong Scene",
                $"This tool is for Battle scenes only.\n\nCurrent scene: {sceneName}",
                "OK");
            return;
        }

        // Find or verify required components
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error",
                "No Canvas found in scene!\n\nPlease create a Canvas first.",
                "OK");
            return;
        }

        CombatManager combatManager = FindObjectOfType<CombatManager>();
        if (combatManager == null)
        {
            EditorUtility.DisplayDialog("Error",
                "No CombatManager found in scene!\n\nPlease add CombatManager first.",
                "OK");
            return;
        }

        Debug.Log($"✓ Using Canvas: {canvas.gameObject.name}");
        Debug.Log($"✓ Using CombatManager: {combatManager.gameObject.name}");

        // Ensure Canvas has proper settings
        SetupCanvas(canvas);

        // Step 1: Create BattleDrawingManager
        BattleDrawingManager battleManager = CreateBattleDrawingManager();

        // Step 2: Create SimpleDrawingCanvas
        SimpleDrawingCanvas drawingCanvas = CreateSimpleDrawingCanvas(canvas);

        // Step 3: Create LineRenderer prefab for strokes
        LineRenderer lineRendererPrefab = CreateLineRendererPrefab();

        // Step 4: Create Drawing Panel UI
        GameObject drawingPanel = CreateDrawingPanelUI(canvas);

        // Step 5: Wire everything together
        WireComponents(battleManager, drawingCanvas, lineRendererPrefab, drawingPanel, combatManager);

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("========== REBUILD COMPLETE ==========");

        EditorUtility.DisplayDialog("Success!",
            "Battle Drawing System has been rebuilt!\n\n" +
            "Components created:\n" +
            "• BattleDrawingManager\n" +
            "• SimpleDrawingCanvas\n" +
            "• Drawing Panel UI (bottom center)\n" +
            "• LineRenderer prefab\n\n" +
            "Make sure to:\n" +
            "1. Save your scene (Ctrl+S)\n" +
            "2. Test the drawing system in Play mode",
            "OK");
    }

    private static void SetupCanvas(Canvas canvas)
    {
        Debug.Log("Setting up Canvas...");

        // Ensure Canvas has required components
        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            Debug.Log("✓ Added GraphicRaycaster to Canvas");
        }

        // Ensure EventSystem exists
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✓ Created EventSystem");
        }

        // Set Canvas to Screen Space - Overlay for proper layering
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        Debug.Log("✓ Canvas configured");
    }

    private static BattleDrawingManager CreateBattleDrawingManager()
    {
        Debug.Log("Creating BattleDrawingManager...");

        GameObject managerObj = new GameObject("BattleDrawingManager");
        BattleDrawingManager manager = managerObj.AddComponent<BattleDrawingManager>();

        Debug.Log("✓ Created BattleDrawingManager");
        return manager;
    }

    private static SimpleDrawingCanvas CreateSimpleDrawingCanvas(Canvas canvas)
    {
        Debug.Log("Creating SimpleDrawingCanvas...");

        // Check if one already exists
        SimpleDrawingCanvas existing = FindObjectOfType<SimpleDrawingCanvas>();
        if (existing != null)
        {
            Debug.Log("✓ Using existing SimpleDrawingCanvas");
            return existing;
        }

        // Create new one
        GameObject canvasObj = new GameObject("BattleDrawingCanvas");
        canvasObj.transform.SetParent(canvas.transform, false);
        SimpleDrawingCanvas drawingCanvas = canvasObj.AddComponent<SimpleDrawingCanvas>();

        // Create stroke container
        GameObject strokeContainer = new GameObject("StrokeContainer");
        strokeContainer.transform.SetParent(canvasObj.transform, false);

        // Configure SimpleDrawingCanvas
        SerializedObject so = new SerializedObject(drawingCanvas);

        SerializedProperty mainCameraProp = so.FindProperty("mainCamera");
        mainCameraProp.objectReferenceValue = Camera.main;

        SerializedProperty strokeContainerProp = so.FindProperty("strokeContainer");
        strokeContainerProp.objectReferenceValue = strokeContainer.transform;

        SerializedProperty lineWidthProp = so.FindProperty("lineWidth");
        lineWidthProp.floatValue = 0.05f;

        SerializedProperty maxStrokesProp = so.FindProperty("maxStrokes");
        maxStrokesProp.intValue = 30;

        SerializedProperty minPointDistanceProp = so.FindProperty("minPointDistance");
        minPointDistanceProp.floatValue = 0.03f;

        SerializedProperty currentColorProp = so.FindProperty("currentColor");
        currentColorProp.colorValue = new Color(0.2f, 1f, 0.2f); // Bright green for visibility

        so.ApplyModifiedProperties();

        // Initially disable the canvas (BattleDrawingManager will enable it)
        drawingCanvas.enabled = false;

        Debug.Log("✓ Created SimpleDrawingCanvas (initially disabled)");
        return drawingCanvas;
    }

    private static LineRenderer CreateLineRendererPrefab()
    {
        Debug.Log("Creating LineRenderer prefab...");

        // Check if prefab already exists
        string prefabPath = "Assets/Prefabs/LineRendererPrefab.prefab";
        LineRenderer existingPrefab = AssetDatabase.LoadAssetAtPath<LineRenderer>(prefabPath);
        if (existingPrefab != null)
        {
            Debug.Log("✓ Using existing LineRenderer prefab");
            return existingPrefab;
        }

        // Create prefab directory if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Create temporary LineRenderer
        GameObject tempObj = new GameObject("LineRendererPrefab");
        LineRenderer lineRenderer = tempObj.AddComponent<LineRenderer>();

        // Configure LineRenderer
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.sortingOrder = 100; // High sorting order to draw on top
        lineRenderer.numCapVertices = 5;
        lineRenderer.numCornerVertices = 5;

        // Save as prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tempObj, prefabPath);
        LineRenderer prefabLineRenderer = prefab.GetComponent<LineRenderer>();

        // Cleanup temp object
        Object.DestroyImmediate(tempObj);

        Debug.Log($"✓ Created LineRenderer prefab at {prefabPath}");
        return prefabLineRenderer;
    }

    private static GameObject CreateDrawingPanelUI(Canvas canvas)
    {
        Debug.Log("Creating Drawing Panel UI...");

        // Create main panel
        GameObject drawingPanel = new GameObject("BattleDrawingPanel");
        drawingPanel.transform.SetParent(canvas.transform, false);
        drawingPanel.layer = LayerMask.NameToLayer("UI");

        RectTransform panelRect = drawingPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0, 80); // 80 pixels from bottom
        panelRect.sizeDelta = new Vector2(500, 400); // 500 wide, 400 tall

        Image panelImage = drawingPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Very dark gray, almost opaque
        panelImage.raycastTarget = false; // Don't block drawing

        // Add subtle border
        Outline panelOutline = drawingPanel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        panelOutline.effectDistance = new Vector2(2, -2);

        Debug.Log("✓ Created main panel");

        // Create Drawing Area
        GameObject drawingArea = new GameObject("DrawingArea");
        drawingArea.transform.SetParent(drawingPanel.transform, false);
        drawingArea.layer = LayerMask.NameToLayer("UI");

        RectTransform areaRect = drawingArea.AddComponent<RectTransform>();
        areaRect.anchorMin = new Vector2(0.5f, 0.5f);
        areaRect.anchorMax = new Vector2(0.5f, 0.5f);
        areaRect.pivot = new Vector2(0.5f, 0.5f);
        areaRect.anchoredPosition = new Vector2(0, 30); // Offset up slightly
        areaRect.sizeDelta = new Vector2(450, 280); // Drawing area size

        RawImage areaImage = drawingArea.AddComponent<RawImage>();
        areaImage.color = new Color(0.05f, 0.05f, 0.05f, 1f); // Very dark, for contrast
        areaImage.raycastTarget = true; // This is where mouse events are detected

        Debug.Log("✓ Created drawing area");

        // Create Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(drawingPanel.transform, false);
        titleObj.layer = LayerMask.NameToLayer("UI");

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -5);
        titleRect.sizeDelta = new Vector2(450, 40);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Draw Your Move";
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 1f, 1f, 1f);
        titleText.raycastTarget = false;

        Debug.Log("✓ Created title text");

        // Create Instruction Text
        GameObject instructionObj = new GameObject("InstructionText");
        instructionObj.transform.SetParent(drawingPanel.transform, false);
        instructionObj.layer = LayerMask.NameToLayer("UI");

        RectTransform instructionRect = instructionObj.AddComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0.5f, 1f);
        instructionRect.anchorMax = new Vector2(0.5f, 1f);
        instructionRect.pivot = new Vector2(0.5f, 1f);
        instructionRect.anchoredPosition = new Vector2(0, -45);
        instructionRect.sizeDelta = new Vector2(450, 80);

        TextMeshProUGUI instructionText = instructionObj.AddComponent<TextMeshProUGUI>();
        instructionText.text = "Available Moves:\n• Fireball • Wave • Block";
        instructionText.fontSize = 16;
        instructionText.alignment = TextAlignmentOptions.Center;
        instructionText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        instructionText.raycastTarget = false;

        Debug.Log("✓ Created instruction text");

        // Create Submit Button
        GameObject submitButton = CreateButton("SubmitButton", drawingPanel.transform,
            new Vector2(0.7f, 0f), new Vector2(0, 15), new Vector2(140, 50),
            "SUBMIT", new Color(0.2f, 0.8f, 0.2f, 1f), 22);

        Debug.Log("✓ Created submit button");

        // Create Clear Button
        GameObject clearButton = CreateButton("ClearButton", drawingPanel.transform,
            new Vector2(0.3f, 0f), new Vector2(0, 15), new Vector2(140, 50),
            "CLEAR", new Color(0.8f, 0.3f, 0.2f, 1f), 22);

        Debug.Log("✓ Created clear button");

        // Hide panel initially
        drawingPanel.SetActive(false);

        Debug.Log("✓ Drawing Panel UI complete");
        return drawingPanel;
    }

    private static GameObject CreateButton(string name, Transform parent, Vector2 anchorPos, Vector2 position, Vector2 size, string text, Color color, int fontSize)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent, false);
        button.layer = LayerMask.NameToLayer("UI");

        RectTransform btnRect = button.AddComponent<RectTransform>();
        btnRect.anchorMin = anchorPos;
        btnRect.anchorMax = anchorPos;
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = size;

        Image btnImage = button.AddComponent<Image>();
        btnImage.color = color;

        Button btn = button.AddComponent<Button>();
        btn.targetGraphic = btnImage;

        // Add hover effect
        ColorBlock colors = btn.colors;
        colors.highlightedColor = color * 1.2f;
        colors.pressedColor = color * 0.8f;
        btn.colors = colors;

        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        textObj.layer = LayerMask.NameToLayer("UI");

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = text;
        btnText.fontSize = fontSize;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        btnText.raycastTarget = false;

        return button;
    }

    private static void WireComponents(BattleDrawingManager battleManager, SimpleDrawingCanvas drawingCanvas,
        LineRenderer lineRendererPrefab, GameObject drawingPanel, CombatManager combatManager)
    {
        Debug.Log("Wiring components together...");

        // Wire BattleDrawingManager
        SerializedObject managerSO = new SerializedObject(battleManager);

        SerializedProperty drawingCanvasProp = managerSO.FindProperty("drawingCanvas");
        drawingCanvasProp.objectReferenceValue = drawingCanvas;

        SerializedProperty drawingPanelProp = managerSO.FindProperty("drawingPanel");
        drawingPanelProp.objectReferenceValue = drawingPanel;

        Transform drawingAreaTransform = drawingPanel.transform.Find("DrawingArea");
        if (drawingAreaTransform != null)
        {
            SerializedProperty drawingAreaImageProp = managerSO.FindProperty("drawingAreaImage");
            drawingAreaImageProp.objectReferenceValue = drawingAreaTransform.GetComponent<RawImage>();
        }

        Transform submitTransform = drawingPanel.transform.Find("SubmitButton");
        if (submitTransform != null)
        {
            SerializedProperty submitButtonProp = managerSO.FindProperty("submitButton");
            submitButtonProp.objectReferenceValue = submitTransform.GetComponent<Button>();
        }

        Transform clearTransform = drawingPanel.transform.Find("ClearButton");
        if (clearTransform != null)
        {
            SerializedProperty clearButtonProp = managerSO.FindProperty("clearButton");
            clearButtonProp.objectReferenceValue = clearTransform.GetComponent<Button>();
        }

        Transform instructionTransform = drawingPanel.transform.Find("InstructionText");
        if (instructionTransform != null)
        {
            SerializedProperty instructionTextProp = managerSO.FindProperty("instructionText");
            instructionTextProp.objectReferenceValue = instructionTransform.GetComponent<TextMeshProUGUI>();
        }

        SerializedProperty combatManagerProp = managerSO.FindProperty("combatManager");
        combatManagerProp.objectReferenceValue = combatManager;

        managerSO.ApplyModifiedProperties();
        Debug.Log("✓ BattleDrawingManager wired");

        // Wire SimpleDrawingCanvas
        SerializedObject canvasSO = new SerializedObject(drawingCanvas);

        SerializedProperty lineRendererPrefabProp = canvasSO.FindProperty("lineRendererPrefab");
        lineRendererPrefabProp.objectReferenceValue = lineRendererPrefab;

        SerializedProperty drawingAreaProp = canvasSO.FindProperty("drawingArea");
        if (drawingAreaTransform != null)
        {
            drawingAreaProp.objectReferenceValue = drawingAreaTransform.GetComponent<RectTransform>();
        }

        canvasSO.ApplyModifiedProperties();
        Debug.Log("✓ SimpleDrawingCanvas wired");

        // Wire CombatManager
        SerializedObject combatSO = new SerializedObject(combatManager);

        SerializedProperty battleDrawingManagerProp = combatSO.FindProperty("battleDrawingManager");
        battleDrawingManagerProp.objectReferenceValue = battleManager;

        combatSO.ApplyModifiedProperties();
        Debug.Log("✓ CombatManager wired");

        Debug.Log("✓ All components wired together");
    }
}
