using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SketchBlossom.Battle;

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
}
