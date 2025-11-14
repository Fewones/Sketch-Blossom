using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using SketchBlossom.Drawing;
using SketchBlossom.Battle;

namespace SketchBlossom.Editor
{
    /// <summary>
    /// Complete automated setup for the Drawing Scene
    /// Creates and wires up all necessary components
    /// </summary>
    public class SetupDrawingSceneComplete : EditorWindow
    {
        [MenuItem("Tools/Sketch Blossom/Setup Drawing Scene (Complete)", false, 1)]
        public static void ShowWindow()
        {
            SetupDrawingSceneComplete window = GetWindow<SetupDrawingSceneComplete>("Drawing Scene Setup");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private Vector2 scrollPosition;
        private bool showDetails = false;

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Drawing Scene Complete Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This will set up the entire Drawing Scene with clean architecture:\n\n" +
                "• DrawingSceneManager (game flow)\n" +
                "• DrawingSceneUIController (all UI)\n" +
                "• SimpleDrawingCanvas (drawing)\n" +
                "• PlantRecognitionSystem (analysis)\n" +
                "• GuideBookManager (help)\n" +
                "• PlantResultPanel (results)",
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("Setup Complete Drawing Scene", GUILayout.Height(40)))
            {
                SetupCompleteScene();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Remove Old Manager Scripts", GUILayout.Height(30)))
            {
                RemoveOldScripts();
            }

            GUILayout.Space(10);

            showDetails = EditorGUILayout.Foldout(showDetails, "Show Setup Details");
            if (showDetails)
            {
                EditorGUILayout.HelpBox(
                    "Setup Process:\n\n" +
                    "1. Create DrawingSceneManager GameObject\n" +
                    "2. Create DrawingSceneUIController GameObject\n" +
                    "3. Find or create SimpleDrawingCanvas\n" +
                    "4. Find or create PlantRecognitionSystem\n" +
                    "5. Wire up all references automatically\n" +
                    "6. Setup UI hierarchy if needed\n" +
                    "7. Validate all connections",
                    MessageType.None
                );
            }

            EditorGUILayout.EndScrollView();
        }

        private static void SetupCompleteScene()
        {
            Debug.Log("========== DRAWING SCENE SETUP START ==========");

            // Step 1: Create or find main manager
            DrawingSceneManager sceneManager = SetupSceneManager();

            // Step 2: Create or find UI controller
            DrawingSceneUIController uiController = SetupUIController();

            // Step 3: Find or create drawing canvas
            SimpleDrawingCanvas drawingCanvas = SetupDrawingCanvas();

            // Step 4: Find or create recognition system
            PlantRecognitionSystem recognitionSystem = SetupRecognitionSystem();

            // Step 5: Find other components
            PlantResultPanel resultPanel = FindFirstObjectByType<PlantResultPanel>(FindObjectsInactive.Include);
            GuideBookManager guideBook = FindFirstObjectByType<GuideBookManager>();

            // Step 6: Wire up references
            WireUpReferences(sceneManager, uiController, drawingCanvas, recognitionSystem, resultPanel, guideBook);

            // Step 7: Validate
            ValidateSetup(sceneManager, uiController, drawingCanvas, recognitionSystem);

            Debug.Log("========== DRAWING SCENE SETUP COMPLETE ==========");
            EditorUtility.DisplayDialog("Setup Complete",
                "Drawing Scene has been set up successfully!\n\n" +
                "Check the Console for details.", "OK");
        }

        private static DrawingSceneManager SetupSceneManager()
        {
            DrawingSceneManager manager = FindFirstObjectByType<DrawingSceneManager>();

            if (manager == null)
            {
                GameObject managerObj = new GameObject("DrawingSceneManager");
                manager = managerObj.AddComponent<DrawingSceneManager>();
                Debug.Log("✓ Created DrawingSceneManager");
            }
            else
            {
                Debug.Log("✓ Found existing DrawingSceneManager");
            }

            return manager;
        }

        private static DrawingSceneUIController SetupUIController()
        {
            DrawingSceneUIController uiController = FindFirstObjectByType<DrawingSceneUIController>();

            if (uiController == null)
            {
                GameObject uiObj = new GameObject("DrawingSceneUIController");
                uiController = uiObj.AddComponent<DrawingSceneUIController>();
                Debug.Log("✓ Created DrawingSceneUIController");
            }
            else
            {
                Debug.Log("✓ Found existing DrawingSceneUIController");
            }

            return uiController;
        }

        private static SimpleDrawingCanvas SetupDrawingCanvas()
        {
            SimpleDrawingCanvas canvas = FindFirstObjectByType<SimpleDrawingCanvas>();

            if (canvas == null)
            {
                Debug.LogWarning("⚠ SimpleDrawingCanvas not found - please add it manually");
            }
            else
            {
                Debug.Log("✓ Found SimpleDrawingCanvas");
            }

            return canvas;
        }

        private static PlantRecognitionSystem SetupRecognitionSystem()
        {
            PlantRecognitionSystem recognition = FindFirstObjectByType<PlantRecognitionSystem>();

            if (recognition == null)
            {
                GameObject recognitionObj = new GameObject("PlantRecognitionSystem");
                recognition = recognitionObj.AddComponent<PlantRecognitionSystem>();
                Debug.Log("✓ Created PlantRecognitionSystem");
            }
            else
            {
                Debug.Log("✓ Found existing PlantRecognitionSystem");
            }

            return recognition;
        }

        private static void WireUpReferences(
            DrawingSceneManager sceneManager,
            DrawingSceneUIController uiController,
            SimpleDrawingCanvas drawingCanvas,
            PlantRecognitionSystem recognitionSystem,
            PlantResultPanel resultPanel,
            GuideBookManager guideBook)
        {
            // Wire up scene manager
            SerializedObject managerSO = new SerializedObject(sceneManager);
            managerSO.FindProperty("drawingCanvas").objectReferenceValue = drawingCanvas;
            managerSO.FindProperty("recognitionSystem").objectReferenceValue = recognitionSystem;
            managerSO.FindProperty("uiController").objectReferenceValue = uiController;
            managerSO.FindProperty("resultPanel").objectReferenceValue = resultPanel;
            managerSO.FindProperty("guideBook").objectReferenceValue = guideBook;
            managerSO.ApplyModifiedProperties();

            Debug.Log("✓ Wired up DrawingSceneManager references");

            // Wire up UI controller
            SerializedObject uiSO = new SerializedObject(uiController);
            uiSO.FindProperty("drawingCanvas").objectReferenceValue = drawingCanvas;

            // Find UI panels
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform instructionsPanel = canvas.transform.Find("InstructionsPanel");
                Transform drawingOverlay = canvas.transform.Find("DrawingOverlay");
                Transform drawingPanel = canvas.transform.Find("DrawingPanel");

                if (instructionsPanel != null)
                    uiSO.FindProperty("instructionsPanel").objectReferenceValue = instructionsPanel.gameObject;
                if (drawingOverlay != null)
                    uiSO.FindProperty("drawingOverlay").objectReferenceValue = drawingOverlay.gameObject;
                if (drawingPanel != null)
                    uiSO.FindProperty("drawingPanel").objectReferenceValue = drawingPanel.gameObject;

                // Find buttons and text elements
                if (drawingPanel != null)
                {
                    Button finishButton = drawingPanel.Find("FinishButton")?.GetComponent<Button>();
                    Button clearButton = drawingPanel.Find("ClearButton")?.GetComponent<Button>();
                    Button guideBookButton = drawingPanel.Find("GuideBookButton")?.GetComponent<Button>();
                    TextMeshProUGUI strokeCountText = drawingPanel.Find("StrokeCountText")?.GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI hintText = drawingPanel.Find("HintText")?.GetComponent<TextMeshProUGUI>();

                    if (finishButton != null)
                        uiSO.FindProperty("finishButton").objectReferenceValue = finishButton;
                    if (clearButton != null)
                        uiSO.FindProperty("clearButton").objectReferenceValue = clearButton;
                    if (guideBookButton != null)
                        uiSO.FindProperty("guideBookButton").objectReferenceValue = guideBookButton;
                    if (strokeCountText != null)
                        uiSO.FindProperty("strokeCountText").objectReferenceValue = strokeCountText;
                    if (hintText != null)
                        uiSO.FindProperty("hintText").objectReferenceValue = hintText;
                }

                if (instructionsPanel != null)
                {
                    TextMeshProUGUI instructionTitle = instructionsPanel.Find("Title")?.GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI instructionText = instructionsPanel.Find("Text")?.GetComponent<TextMeshProUGUI>();
                    Button startButton = instructionsPanel.Find("StartButton")?.GetComponent<Button>();

                    if (instructionTitle != null)
                        uiSO.FindProperty("instructionTitle").objectReferenceValue = instructionTitle;
                    if (instructionText != null)
                        uiSO.FindProperty("instructionText").objectReferenceValue = instructionText;
                    if (startButton != null)
                        uiSO.FindProperty("startDrawingButton").objectReferenceValue = startButton;
                }
            }

            uiSO.ApplyModifiedProperties();
            Debug.Log("✓ Wired up DrawingSceneUIController references");
        }

        private static void ValidateSetup(
            DrawingSceneManager sceneManager,
            DrawingSceneUIController uiController,
            SimpleDrawingCanvas drawingCanvas,
            PlantRecognitionSystem recognitionSystem)
        {
            Debug.Log("========== VALIDATION ==========");

            bool isValid = true;

            if (sceneManager == null)
            {
                Debug.LogError("✗ DrawingSceneManager missing!");
                isValid = false;
            }

            if (uiController == null)
            {
                Debug.LogError("✗ DrawingSceneUIController missing!");
                isValid = false;
            }

            if (drawingCanvas == null)
            {
                Debug.LogError("✗ SimpleDrawingCanvas missing!");
                isValid = false;
            }

            if (recognitionSystem == null)
            {
                Debug.LogError("✗ PlantRecognitionSystem missing!");
                isValid = false;
            }

            if (isValid)
            {
                Debug.Log("✓ All core components present!");
            }

            Debug.Log("================================");
        }

        private static void RemoveOldScripts()
        {
            bool removed = false;

            // Find and destroy old DrawingManager
            var oldDrawingManager = FindFirstObjectByType<DrawingManager>();
            if (oldDrawingManager != null)
            {
                DestroyImmediate(oldDrawingManager.gameObject);
                Debug.Log("✓ Removed old DrawingManager");
                removed = true;
            }

            // Find and destroy old DrawingSceneUI
            var oldDrawingSceneUI = FindFirstObjectByType<DrawingSceneUI>();
            if (oldDrawingSceneUI != null)
            {
                DestroyImmediate(oldDrawingSceneUI);
                Debug.Log("✓ Removed old DrawingSceneUI component");
                removed = true;
            }

            if (removed)
            {
                EditorUtility.DisplayDialog("Cleanup Complete",
                    "Old manager scripts have been removed.\n\n" +
                    "Check the Console for details.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No Old Scripts Found",
                    "No old manager scripts were found in the scene.", "OK");
            }
        }
    }
}
