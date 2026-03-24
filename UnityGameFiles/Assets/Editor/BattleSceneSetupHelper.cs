using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using SketchBlossom.Battle;
using TMPro;

/// <summary>
/// Editor utility to automatically setup the battle scene with MoveExecutor
/// and check for common configuration issues
/// </summary>
public class BattleSceneSetupHelper : EditorWindow
{
    private Vector2 scrollPosition;
    private bool autoFixIssues = true;

    [MenuItem("Sketch Blossom/Battle Scene Setup Helper")]
    public static void ShowWindow()
    {
        var window = GetWindow<BattleSceneSetupHelper>("Battle Setup");
        window.minSize = new Vector2(500, 600);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Battle Scene Setup Helper", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool helps you set up the enhanced battle move system.\n" +
            "It will check for missing components and help wire everything together.",
            MessageType.Info
        );

        EditorGUILayout.Space();
        autoFixIssues = EditorGUILayout.Toggle("Auto-fix issues when possible", autoFixIssues);
        EditorGUILayout.Space();

        if (GUILayout.Button("Check Battle Scene Setup", GUILayout.Height(40)))
        {
            CheckBattleScene();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add MoveExecutor to BattleManager", GUILayout.Height(30)))
        {
            AddMoveExecutorToBattleManager();
        }

        if (GUILayout.Button("Auto-Wire MoveExecutor References", GUILayout.Height(30)))
        {
            AutoWireMoveExecutor();
        }

        if (GUILayout.Button("Check Guide Book Setup", GUILayout.Height(30)))
        {
            CheckGuideBookSetup();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Fixes:", EditorStyles.boldLabel);

        if (GUILayout.Button("Assign Main Camera to MoveExecutor"))
        {
            AssignCameraToMoveExecutor();
        }

        if (GUILayout.Button("Create MoveGuideBook GameObject"))
        {
            CreateMoveGuideBook();
        }

        if (GUILayout.Button("Create Guide Book Move Entry Rows"))
        {
            CreateGuideBookMoveEntries();
        }
    }

    private void CheckBattleScene()
    {
        Debug.Log("=== BATTLE SCENE SETUP CHECK ===");

        // Find BattleManager
        var battleManager = FindFirstObjectByType<DrawingBattleSceneManager>();
        if (battleManager == null)
        {
            Debug.LogError("DrawingBattleSceneManager not found in scene! Are you in the DrawingBattleScene?");
            return;
        }

        Debug.Log("Found DrawingBattleSceneManager");

        // Check for MoveExecutor component
        var moveExecutor = FindFirstObjectByType<MoveExecutor>();
        if (moveExecutor == null)
        {
            Debug.LogWarning("MoveExecutor component not found in scene!");
            if (autoFixIssues)
            {
                Debug.Log("Adding MoveExecutor to BattleManager...");
                AddMoveExecutorToBattleManager();
            }
        }
        else
        {
            Debug.Log("Found MoveExecutor component");

            // Check camera assignment
            if (moveExecutor.mainCamera == null)
            {
                Debug.LogWarning("MoveExecutor.mainCamera is not assigned!");
                if (autoFixIssues)
                {
                    Debug.Log("Auto-assigning main camera...");
                    AssignCameraToMoveExecutor();
                }
            }
            else
            {
                Debug.Log($"MoveExecutor camera assigned: {moveExecutor.mainCamera.name}");
            }
        }

        // Check guide book setup
        CheckGuideBookSetup();

        Debug.Log("=== SETUP CHECK COMPLETE ===");
    }

    private void AddMoveExecutorToBattleManager()
    {
        var battleManager = FindFirstObjectByType<DrawingBattleSceneManager>();
        if (battleManager == null)
        {
            Debug.LogError("BattleManager not found! Make sure you're in the DrawingBattleScene.");
            return;
        }

        // Check if MoveExecutor already exists
        var existing = battleManager.GetComponent<MoveExecutor>();
        if (existing != null)
        {
            Debug.Log("MoveExecutor already exists on BattleManager");
            return;
        }

        // Add MoveExecutor component
        var moveExecutor = battleManager.gameObject.AddComponent<MoveExecutor>();
        Debug.Log("Added MoveExecutor component to BattleManager");

        // Try to auto-wire references
        AutoWireMoveExecutor();

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private void AutoWireMoveExecutor()
    {
        var moveExecutor = FindFirstObjectByType<MoveExecutor>();
        if (moveExecutor == null)
        {
            Debug.LogError("MoveExecutor not found! Add it first.");
            return;
        }

        Debug.Log("=== AUTO-WIRING MOVEEXECUTOR ===");

        // Assign camera
        if (moveExecutor.mainCamera == null)
        {
            AssignCameraToMoveExecutor();
        }

        // Look for UI text elements
        var allTexts = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);

        foreach (var text in allTexts)
        {
            // Look for move name text
            if (text.name.ToLower().Contains("move") && text.name.ToLower().Contains("name"))
            {
                moveExecutor.moveNameText = text;
                Debug.Log($"Assigned moveNameText: {text.name}");
            }

            // Look for effectiveness text
            if (text.name.ToLower().Contains("effectiveness") || text.name.ToLower().Contains("action"))
            {
                if (moveExecutor.effectivenessText == null)
                {
                    moveExecutor.effectivenessText = text;
                    Debug.Log($"Assigned effectivenessText: {text.name}");
                }
            }
        }

        Debug.Log("=== AUTO-WIRE COMPLETE ===");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private void AssignCameraToMoveExecutor()
    {
        var moveExecutor = FindFirstObjectByType<MoveExecutor>();
        if (moveExecutor == null)
        {
            Debug.LogError("MoveExecutor not found!");
            return;
        }

        // Find main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }

        if (mainCamera != null)
        {
            moveExecutor.mainCamera = mainCamera;
            Debug.Log($"Assigned camera to MoveExecutor: {mainCamera.name}");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        else
        {
            Debug.LogError("No camera found in scene!");
        }
    }

    private void CheckGuideBookSetup()
    {
        Debug.Log("=== CHECKING GUIDE BOOK SETUP ===");

        // Check for old GuideBookManager
        var oldGuideBook = FindFirstObjectByType<GuideBookManager>();
        if (oldGuideBook != null)
        {
            Debug.LogWarning($"Found old GuideBookManager on '{oldGuideBook.gameObject.name}'");
            Debug.LogWarning("   This shows PLANT guides, not MOVE guides!");
            Debug.LogWarning("   Consider replacing with MoveGuideBook component.");
        }

        // Check for new MoveGuideBook
        var newGuideBook = FindFirstObjectByType<MoveGuideBook>();
        if (newGuideBook != null)
        {
            Debug.Log($"Found MoveGuideBook on '{newGuideBook.gameObject.name}'");

            bool hasIssues = false;
            if (newGuideBook.bookPanel == null)
            {
                Debug.LogWarning("   bookPanel not assigned!");
                hasIssues = true;
            }
            if (newGuideBook.openBookButton == null)
            {
                Debug.LogWarning("   openBookButton not assigned!");
                hasIssues = true;
            }

            // Check move entry rows
            if (newGuideBook.moveEntryTexts == null || newGuideBook.moveEntryTexts.Length < 4)
            {
                Debug.LogWarning("   moveEntryTexts needs 4 slots! Click 'Create Guide Book Move Entry Rows' to fix.");
                hasIssues = true;
            }

            if (newGuideBook.moveShapePreviews == null || newGuideBook.moveShapePreviews.Length < 4)
            {
                Debug.LogWarning("   moveShapePreviews needs 4 RawImage slots! Click 'Create Guide Book Move Entry Rows' to fix.");
                hasIssues = true;
            }

            if (newGuideBook.moveEntriesContainer == null)
            {
                Debug.LogWarning("   moveEntriesContainer not assigned! Click 'Create Guide Book Move Entry Rows' to fix.");
                hasIssues = true;
            }

            if (!hasIssues)
            {
                Debug.Log("   MoveGuideBook appears to be properly configured");
            }
        }
        else
        {
            Debug.LogWarning("MoveGuideBook not found in scene!");
        }

        Debug.Log("=== GUIDE BOOK CHECK COMPLETE ===");
    }

    private void CreateMoveGuideBook()
    {
        // Check if one already exists
        var existing = FindFirstObjectByType<MoveGuideBook>();
        if (existing != null)
        {
            Debug.LogWarning($"MoveGuideBook already exists on '{existing.gameObject.name}'");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Create new GameObject
        GameObject guideBookObj = new GameObject("MoveGuideBookManager");
        var guideBook = guideBookObj.AddComponent<MoveGuideBook>();

        Debug.Log("Created MoveGuideBookManager GameObject");
        Debug.Log("You still need to:");
        Debug.Log("   1. Create the UI panel for the guide book");
        Debug.Log("   2. Assign all UI references in the Inspector");

        Selection.activeGameObject = guideBookObj;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    /// <summary>
    /// Creates 4 horizontal move-entry rows inside the GuideBookPanel.
    /// Each row contains a TextMeshProUGUI (move info) on the left and a
    /// RawImage (shape preview) on the right. This ensures previews are
    /// visually connected to their corresponding move text.
    /// Replaces the old ShapePreviewContainer approach.
    /// </summary>
    private void CreateGuideBookMoveEntries()
    {
        var guideBook = FindFirstObjectByType<MoveGuideBook>();
        if (guideBook == null)
        {
            Debug.LogError("MoveGuideBook not found in scene! Create it first.");
            return;
        }

        if (guideBook.bookPanel == null)
        {
            Debug.LogError("MoveGuideBook.bookPanel is not assigned! Wire it up first.");
            return;
        }

        // Remove old ShapePreviewContainer if it exists (from the previous layout).
        Transform oldContainer = guideBook.bookPanel.transform.Find("ShapePreviewContainer");
        if (oldContainer != null)
        {
            DestroyImmediate(oldContainer.gameObject);
            Debug.Log("Removed old ShapePreviewContainer.");
        }

        // Create or reuse the MoveEntriesContainer.
        Transform existingContainer = guideBook.bookPanel.transform.Find("MoveEntriesContainer");
        GameObject container;
        if (existingContainer != null)
        {
            container = existingContainer.gameObject;
            Debug.Log("Found existing MoveEntriesContainer – recreating children.");
            for (int i = container.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(container.transform.GetChild(i).gameObject);
        }
        else
        {
            container = new GameObject("MoveEntriesContainer");
            container.transform.SetParent(guideBook.bookPanel.transform, false);

            var rect = container.AddComponent<RectTransform>();
            // Positioned below the header flavor text and above the nav buttons.
            // pageDescription is shrunk to y 0.78-0.85 on plant pages, so the
            // container fills from y 0.13 (above buttons) to y 0.77 (below header).
            rect.anchorMin = new Vector2(0.03f, 0.13f);
            rect.anchorMax = new Vector2(0.97f, 0.77f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // Ensure the container has a VerticalLayoutGroup.
        var vLayout = container.GetComponent<VerticalLayoutGroup>();
        if (vLayout == null)
            vLayout = container.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 6f;
        vLayout.childAlignment = TextAnchor.UpperLeft;
        vLayout.childForceExpandWidth = true;
        vLayout.childForceExpandHeight = false;
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;
        vLayout.padding = new RectOffset(8, 8, 4, 4);

        RawImage[] previews = new RawImage[4];
        TextMeshProUGUI[] entryTexts = new TextMeshProUGUI[4];

        for (int i = 0; i < 4; i++)
        {
            // --- Row: horizontal layout [Text | Preview Image] ---
            var row = new GameObject($"MoveRow_{i}");
            row.transform.SetParent(container.transform, false);

            var rowRect = row.AddComponent<RectTransform>();
            // Each row has a fixed height – 4 rows must fit in the container.
            rowRect.sizeDelta = new Vector2(0, 0);

            var hLayout = row.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 8f;
            hLayout.childAlignment = TextAnchor.MiddleLeft;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.padding = new RectOffset(4, 4, 2, 2);

            // Make the row expand to fill available vertical space equally.
            var rowLayoutElem = row.AddComponent<LayoutElement>();
            rowLayoutElem.flexibleHeight = 1f;
            rowLayoutElem.minHeight = 60f;

            // Optional: subtle background per row for visual separation.
            var rowImage = row.AddComponent<Image>();
            rowImage.color = new Color(1f, 1f, 1f, 0.08f);

            // --- Text element (flexible width) ---
            var textObj = new GameObject($"MoveText_{i}");
            textObj.transform.SetParent(row.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(0, 0);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 13;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.richText = true;
            entryTexts[i] = tmp;

            var textLayoutElem = textObj.AddComponent<LayoutElement>();
            textLayoutElem.flexibleWidth = 1f;  // Takes remaining space
            textLayoutElem.minWidth = 100f;

            // --- Shape preview image (fixed width) ---
            var imgObj = new GameObject($"ShapePreview_{i}");
            imgObj.transform.SetParent(row.transform, false);

            var imgRect = imgObj.AddComponent<RectTransform>();
            imgRect.sizeDelta = new Vector2(64, 64);

            var rawImage = imgObj.AddComponent<RawImage>();
            rawImage.color = Color.white;
            previews[i] = rawImage;

            var imgLayoutElem = imgObj.AddComponent<LayoutElement>();
            imgLayoutElem.preferredWidth = 64;
            imgLayoutElem.preferredHeight = 64;
            imgLayoutElem.flexibleWidth = 0f;  // Fixed size, no stretching
        }

        // Wire into MoveGuideBook.
        guideBook.moveEntriesContainer = container.transform;
        guideBook.moveEntryTexts = entryTexts;
        guideBook.moveShapePreviews = previews;

        Debug.Log("Created 4 move entry rows (text + preview), wired into MoveGuideBook.");
        Debug.Log("   Each row: [move text (left, flexible)] + [shape preview (right, 64x64)]");

        EditorUtility.SetDirty(guideBook);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = container;
    }
}
