using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.Reflection;

namespace SketchBlossom.Battle.Editor
{
    /// <summary>
    /// Editor utility to fix/update existing DrawingBattleScene without rebuilding.
    /// Finds all UI components and wires them to BattleManager automatically.
    /// </summary>
    public class DrawingBattleSceneFixer : EditorWindow
    {
        [MenuItem("Tools/Sketch Blossom/Fix Drawing Battle Scene")]
        public static void ShowWindow()
        {
            GetWindow<DrawingBattleSceneFixer>("Battle Scene Fixer");
        }

        private Vector2 scrollPosition;
        private string logOutput = "Click 'Fix Scene' to auto-wire all references.\n\nThis will:\n- Find BattleManager\n- Locate all UI components\n- Wire up all references\n- No scene rebuild needed!";

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Drawing Battle Scene Fixer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This tool automatically fixes your DrawingBattleScene by finding all UI components and wiring them to the BattleManager.\n\n" +
                "No need to rebuild the entire scene!",
                MessageType.Info
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("Fix Scene", GUILayout.Height(40)))
            {
                FixScene();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Output Log:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            EditorGUILayout.TextArea(logOutput, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        [MenuItem("Tools/Sketch Blossom/Fix Drawing Battle Scene (Quick)", false, 1)]
        public static void FixSceneQuick()
        {
            var fixer = new DrawingBattleSceneFixer();
            fixer.FixScene();
        }

        private void FixScene()
        {
            logOutput = "=== FIXING DRAWING BATTLE SCENE ===\n\n";
            Log("Starting scene fix...");

            // Find BattleManager
            DrawingBattleSceneManager battleManager = FindObjectOfType<DrawingBattleSceneManager>();
            if (battleManager == null)
            {
                LogError("❌ BattleManager not found in scene!");
                LogError("Make sure you have a GameObject with DrawingBattleSceneManager component.");
                return;
            }
            Log($"✅ Found BattleManager: {battleManager.gameObject.name}");

            // Find all components
            BattleDrawingCanvas drawingCanvas = FindComponent<BattleDrawingCanvas>("DrawingArea");
            BattleHPBar playerHPBar = FindComponentInParent<BattleHPBar>("PlayerHPBar", "PlayerArea");
            BattleHPBar enemyHPBar = FindComponentInParent<BattleHPBar>("EnemyHPBar", "EnemyArea");
            Button finishButton = FindComponentInParent<Button>("FinishDrawingButton");
            Button clearButton = FindComponentInParent<Button>("ClearDrawingButton");
            TextMeshProUGUI turnIndicator = FindComponentInParent<TextMeshProUGUI>("TurnIndicator");
            TextMeshProUGUI actionText = FindComponentInParent<TextMeshProUGUI>("ActionText");
            TextMeshProUGUI availableMovesText = FindComponentInParent<TextMeshProUGUI>("AvailableMovesText");

            // Find move detection systems (should be on BattleManager)
            MovesetDetector moveDetector = battleManager.GetComponent<MovesetDetector>();
            MoveRecognitionSystem moveRecognition = battleManager.GetComponent<MoveRecognitionSystem>();

            if (moveDetector == null)
            {
                LogWarning("⚠ MovesetDetector not found, adding...");
                moveDetector = battleManager.gameObject.AddComponent<MovesetDetector>();
            }

            if (moveRecognition == null)
            {
                LogWarning("⚠ MoveRecognitionSystem not found, adding...");
                moveRecognition = battleManager.gameObject.AddComponent<MoveRecognitionSystem>();
            }

            if (moveDetector != null && moveRecognition != null)
            {
                moveDetector.recognitionSystem = moveRecognition;
                Log("✅ Move detection systems linked");
            }

            // Wire up all references using reflection
            Log("\n--- Wiring References ---");
            var managerType = typeof(DrawingBattleSceneManager);

            SetPrivateField(managerType, battleManager, "drawingCanvas", drawingCanvas);
            SetPrivateField(managerType, battleManager, "finishDrawingButton", finishButton);
            SetPrivateField(managerType, battleManager, "clearDrawingButton", clearButton);
            SetPrivateField(managerType, battleManager, "playerHPBar", playerHPBar);
            SetPrivateField(managerType, battleManager, "enemyHPBar", enemyHPBar);
            SetPrivateField(managerType, battleManager, "turnIndicatorText", turnIndicator);
            SetPrivateField(managerType, battleManager, "actionText", actionText);
            SetPrivateField(managerType, battleManager, "availableMovesText", availableMovesText);
            SetPrivateField(managerType, battleManager, "movesetDetector", moveDetector);
            SetPrivateField(managerType, battleManager, "moveRecognitionSystem", moveRecognition);

            // Mark scene as dirty so it saves
            EditorUtility.SetDirty(battleManager);
            if (moveDetector != null) EditorUtility.SetDirty(moveDetector);
            if (moveRecognition != null) EditorUtility.SetDirty(moveRecognition);

            Log("\n=== SUMMARY ===");
            Log($"Drawing Canvas: {(drawingCanvas != null ? "✅" : "❌")}");
            Log($"Action Text: {(actionText != null ? "✅" : "❌")}");
            Log($"Turn Indicator: {(turnIndicator != null ? "✅" : "❌")}");
            Log($"Available Moves: {(availableMovesText != null ? "✅" : "❌")}");
            Log($"Player HP Bar: {(playerHPBar != null ? "✅" : "❌")}");
            Log($"Enemy HP Bar: {(enemyHPBar != null ? "✅" : "❌")}");
            Log($"Finish Button: {(finishButton != null ? "✅" : "❌")}");
            Log($"Clear Button: {(clearButton != null ? "✅" : "❌")}");
            Log($"Move Detector: {(moveDetector != null ? "✅" : "❌")}");
            Log($"Move Recognition: {(moveRecognition != null ? "✅" : "❌")}");

            Log("\n✅ SCENE FIXED! All references wired up.");
            Log("Save your scene (Ctrl+S) to keep changes.");

            Debug.Log("[DrawingBattleSceneFixer] Scene fixed successfully!");
        }

        private T FindComponent<T>(string objectName) where T : Component
        {
            GameObject obj = GameObject.Find(objectName);
            if (obj != null)
            {
                T component = obj.GetComponent<T>();
                if (component != null)
                {
                    Log($"✅ Found {typeof(T).Name} in '{objectName}'");
                    return component;
                }
                else
                {
                    LogWarning($"⚠ Found '{objectName}' but no {typeof(T).Name} component");
                }
            }
            else
            {
                LogWarning($"⚠ GameObject '{objectName}' not found");
            }
            return null;
        }

        private T FindComponentInParent<T>(string objectName, string parentHint = null) where T : Component
        {
            // Try direct find first
            GameObject obj = GameObject.Find(objectName);

            // If not found and we have a parent hint, try finding under parent
            if (obj == null && !string.IsNullOrEmpty(parentHint))
            {
                GameObject parent = GameObject.Find(parentHint);
                if (parent != null)
                {
                    Transform found = parent.transform.Find(objectName);
                    if (found != null)
                    {
                        obj = found.gameObject;
                    }
                }
            }

            // If still not found, search in all objects (slower but thorough)
            if (obj == null)
            {
                T[] allComponents = FindObjectsOfType<T>();
                foreach (var component in allComponents)
                {
                    if (component.gameObject.name == objectName)
                    {
                        obj = component.gameObject;
                        break;
                    }
                }
            }

            if (obj != null)
            {
                T component = obj.GetComponent<T>();
                if (component != null)
                {
                    Log($"✅ Found {typeof(T).Name} in '{objectName}'");
                    return component;
                }
                else
                {
                    LogWarning($"⚠ Found '{objectName}' but no {typeof(T).Name} component");
                }
            }
            else
            {
                LogWarning($"⚠ GameObject '{objectName}' not found");
            }
            return null;
        }

        private void SetPrivateField(System.Type type, object instance, string fieldName, object value)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(instance, value);
                string status = value != null ? "✅" : "❌";
                Log($"{status} Set {fieldName}: {(value != null ? "Connected" : "NULL")}");
            }
            else
            {
                LogWarning($"⚠ Field '{fieldName}' not found in {type.Name}");
            }
        }

        private void Log(string message)
        {
            logOutput += message + "\n";
            Repaint();
        }

        private void LogWarning(string message)
        {
            logOutput += message + "\n";
            Debug.LogWarning(message);
            Repaint();
        }

        private void LogError(string message)
        {
            logOutput += message + "\n";
            Debug.LogError(message);
            Repaint();
        }
    }
}
