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

        if (GUILayout.Button("Create Guide Book Shape Previews & Labels"))
        {
            CreateGuideBookShapePreviews();
        }
    }

    private void CheckBattleScene()
    {
        Debug.Log("=== BATTLE SCENE SETUP CHECK ===");

        // Find BattleManager
        var battleManager = FindFirstObjectByType<DrawingBattleSceneManager>();
        if (battleManager == null)
        {
            Debug.LogError("❌ DrawingBattleSceneManager not found in scene! Are you in the DrawingBattleScene?");
            return;
        }

        Debug.Log("✅ Found DrawingBattleSceneManager");

        // Check for MoveExecutor component
        var moveExecutor = FindFirstObjectByType<MoveExecutor>();
        if (moveExecutor == null)
        {
            Debug.LogWarning("⚠️ MoveExecutor component not found in scene!");
            if (autoFixIssues)
            {
                Debug.Log("→ Adding MoveExecutor to BattleManager...");
                AddMoveExecutorToBattleManager();
            }
        }
        else
        {
            Debug.Log("✅ Found MoveExecutor component");

            // Check camera assignment
            if (moveExecutor.mainCamera == null)
            {
                Debug.LogWarning("⚠️ MoveExecutor.mainCamera is not assigned!");
                if (autoFixIssues)
                {
                    Debug.Log("→ Auto-assigning main camera...");
                    AssignCameraToMoveExecutor();
                }
            }
            else
            {
                Debug.Log($"✅ MoveExecutor camera assigned: {moveExecutor.mainCamera.name}");
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
            Debug.LogError("❌ BattleManager not found! Make sure you're in the DrawingBattleScene.");
            return;
        }

        // Check if MoveExecutor already exists
        var existing = battleManager.GetComponent<MoveExecutor>();
        if (existing != null)
        {
            Debug.Log("✅ MoveExecutor already exists on BattleManager");
            return;
        }

        // Add MoveExecutor component
        var moveExecutor = battleManager.gameObject.AddComponent<MoveExecutor>();
        Debug.Log("✅ Added MoveExecutor component to BattleManager");

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
            Debug.LogError("❌ MoveExecutor not found! Add it first.");
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
                Debug.Log($"✅ Assigned moveNameText: {text.name}");
            }

            // Look for effectiveness text
            if (text.name.ToLower().Contains("effectiveness") || text.name.ToLower().Contains("action"))
            {
                if (moveExecutor.effectivenessText == null)
                {
                    moveExecutor.effectivenessText = text;
                    Debug.Log($"✅ Assigned effectivenessText: {text.name}");
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
            Debug.LogError("❌ MoveExecutor not found!");
            return;
        }

        // Find main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            // Try to find any camera
            mainCamera = FindFirstObjectByType<Camera>();
        }

        if (mainCamera != null)
        {
            moveExecutor.mainCamera = mainCamera;
            Debug.Log($"✅ Assigned camera to MoveExecutor: {mainCamera.name}");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        else
        {
            Debug.LogError("❌ No camera found in scene!");
        }
    }

    private void CheckGuideBookSetup()
    {
        Debug.Log("=== CHECKING GUIDE BOOK SETUP ===");

        // Check for old GuideBookManager
        var oldGuideBook = FindFirstObjectByType<GuideBookManager>();
        if (oldGuideBook != null)
        {
            Debug.LogWarning($"⚠️ Found old GuideBookManager on '{oldGuideBook.gameObject.name}'");
            Debug.LogWarning("   This shows PLANT guides, not MOVE guides!");
            Debug.LogWarning("   Consider replacing with MoveGuideBook component.");
        }

        // Check for new MoveGuideBook
        var newGuideBook = FindFirstObjectByType<MoveGuideBook>();
        if (newGuideBook != null)
        {
            Debug.Log($"✅ Found MoveGuideBook on '{newGuideBook.gameObject.name}'");

            // Check if references are assigned
            bool hasIssues = false;
            if (newGuideBook.bookPanel == null)
            {
                Debug.LogWarning("   ⚠️ bookPanel not assigned!");
                hasIssues = true;
            }
            if (newGuideBook.openBookButton == null)
            {
                Debug.LogWarning("   ⚠️ openBookButton not assigned!");
                hasIssues = true;
            }

            // Check shape preview slots
            if (newGuideBook.moveShapePreviews == null || newGuideBook.moveShapePreviews.Length < 4)
            {
                Debug.LogWarning("   ⚠️ moveShapePreviews needs 4 RawImage slots! Click 'Create Guide Book Shape Previews & Labels' to fix.");
                hasIssues = true;
            }
            else
            {
                bool anyNull = false;
                foreach (var img in newGuideBook.moveShapePreviews)
                    if (img == null) { anyNull = true; break; }
                if (anyNull)
                {
                    Debug.LogWarning("   ⚠️ Some moveShapePreviews slots are unassigned! Click 'Create Guide Book Shape Previews & Labels' to fix.");
                    hasIssues = true;
                }
                else
                {
                    Debug.Log("   ✅ Shape preview slots configured (4/4)");
                }
            }

            if (newGuideBook.moveShapeLabels == null || newGuideBook.moveShapeLabels.Length < 4)
            {
                Debug.LogWarning("   ⚠️ moveShapeLabels needs 4 TextMeshProUGUI slots! Click 'Create Guide Book Shape Previews & Labels' to fix.");
                hasIssues = true;
            }

            if (!hasIssues)
            {
                Debug.Log("   ✅ MoveGuideBook appears to be properly configured");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ MoveGuideBook not found in scene!");
            Debug.LogWarning("   You won't have an in-game move guide.");
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

        Debug.Log("✅ Created MoveGuideBookManager GameObject");
        Debug.Log("⚠️ You still need to:");
        Debug.Log("   1. Create the UI panel for the guide book");
        Debug.Log("   2. Assign all UI references in the Inspector");
        Debug.Log("   See INTEGRATION_TODO.md for detailed instructions");

        Selection.activeGameObject = guideBookObj;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    /// <summary>
    /// Creates 4 RawImage preview slots and 4 TextMeshProUGUI labels inside the
    /// GuideBookPanel, then wires them into the MoveGuideBook component.
    /// </summary>
    private void CreateGuideBookShapePreviews()
    {
        var guideBook = FindFirstObjectByType<MoveGuideBook>();
        if (guideBook == null)
        {
            Debug.LogError("❌ MoveGuideBook not found in scene! Create it first.");
            return;
        }

        if (guideBook.bookPanel == null)
        {
            Debug.LogError("❌ MoveGuideBook.bookPanel is not assigned! Wire it up first.");
            return;
        }

        // Create a container for the shape previews inside the book panel.
        Transform existingContainer = guideBook.bookPanel.transform.Find("ShapePreviewContainer");
        GameObject container;
        if (existingContainer != null)
        {
            container = existingContainer.gameObject;
            Debug.Log("Found existing ShapePreviewContainer – recreating children.");
            // Clear existing children
            for (int i = container.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(container.transform.GetChild(i).gameObject);
        }
        else
        {
            container = new GameObject("ShapePreviewContainer");
            container.transform.SetParent(guideBook.bookPanel.transform, false);

            var rect = container.AddComponent<RectTransform>();
            // Position along the right side of the book panel
            rect.anchorMin = new Vector2(0.65f, 0.1f);
            rect.anchorMax = new Vector2(0.98f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layout = container.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(4, 4, 4, 4);
        }

        RawImage[] previews = new RawImage[4];
        TextMeshProUGUI[] labels = new TextMeshProUGUI[4];

        for (int i = 0; i < 4; i++)
        {
            // Wrapper for each move slot (image + label)
            var slot = new GameObject($"MoveSlot_{i}");
            slot.transform.SetParent(container.transform, false);

            var slotRect = slot.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(0, 100);

            var slotLayout = slot.AddComponent<VerticalLayoutGroup>();
            slotLayout.spacing = 2f;
            slotLayout.childAlignment = TextAnchor.UpperCenter;
            slotLayout.childForceExpandWidth = false;
            slotLayout.childForceExpandHeight = false;

            // RawImage for the shape preview
            var imgObj = new GameObject($"ShapePreview_{i}");
            imgObj.transform.SetParent(slot.transform, false);

            var imgRect = imgObj.AddComponent<RectTransform>();
            imgRect.sizeDelta = new Vector2(64, 64);

            var rawImage = imgObj.AddComponent<RawImage>();
            rawImage.color = Color.white;
            previews[i] = rawImage;

            // Add a LayoutElement so the image keeps its preferred size
            var imgLayoutElem = imgObj.AddComponent<LayoutElement>();
            imgLayoutElem.preferredWidth = 64;
            imgLayoutElem.preferredHeight = 64;

            // Label beneath the preview
            var lblObj = new GameObject($"ShapeLabel_{i}");
            lblObj.transform.SetParent(slot.transform, false);

            var lblRect = lblObj.AddComponent<RectTransform>();
            lblRect.sizeDelta = new Vector2(120, 28);

            var label = lblObj.AddComponent<TextMeshProUGUI>();
            label.text = "";
            label.fontSize = 11;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.2f, 0.2f, 0.2f);
            labels[i] = label;

            var lblLayoutElem = lblObj.AddComponent<LayoutElement>();
            lblLayoutElem.preferredWidth = 120;
            lblLayoutElem.preferredHeight = 28;
        }

        // Wire into MoveGuideBook
        guideBook.moveShapePreviews = previews;
        guideBook.moveShapeLabels = labels;

        Debug.Log("✅ Created 4 shape preview slots and 4 labels, wired into MoveGuideBook.");
        Debug.Log("   Preview container placed on the right side of the book panel.");

        EditorUtility.SetDirty(guideBook);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = container;
    }
}
