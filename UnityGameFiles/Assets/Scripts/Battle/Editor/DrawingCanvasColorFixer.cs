using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace SketchBlossom.Battle.Editor
{
    /// <summary>
    /// Editor tool to fix the drawing canvas color issue.
    /// Lines were white on white background - this makes them black and visible!
    /// </summary>
    public class DrawingCanvasColorFixer : EditorWindow
    {
        [MenuItem("Tools/Sketch Blossom/Fix Drawing Canvas Colors")]
        public static void ShowWindow()
        {
            GetWindow<DrawingCanvasColorFixer>("Drawing Color Fix");
        }

        private Vector2 scrollPosition;
        private string logOutput = "Click 'Fix Drawing Colors' to make lines visible!\n\nThis will:\n- Set drawing color to BLACK\n- Fix line material color\n- Ensure proper line width\n- Make lines visible on white background";

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Drawing Canvas Color Fixer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This fixes the white-on-white drawing problem!\n\n" +
                "Lines will be changed to BLACK so you can see them on the white drawing area.",
                MessageType.Info
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("Fix Drawing Colors", GUILayout.Height(40)))
            {
                FixDrawingColors();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Output Log:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            EditorGUILayout.TextArea(logOutput, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        [MenuItem("Tools/Sketch Blossom/Fix Drawing Canvas Colors (Quick)", false, 2)]
        public static void FixDrawingColorsQuick()
        {
            var fixer = new DrawingCanvasColorFixer();
            fixer.FixDrawingColors();
        }

        private void FixDrawingColors()
        {
            logOutput = "=== FIXING DRAWING CANVAS COLORS ===\n\n";
            Log("Starting color fix...");

            // Find all BattleDrawingCanvas components in scene
            BattleDrawingCanvas[] canvases = FindObjectsOfType<BattleDrawingCanvas>();

            if (canvases.Length == 0)
            {
                LogError("❌ No BattleDrawingCanvas found in scene!");
                LogError("Make sure DrawingArea has BattleDrawingCanvas component.");
                return;
            }

            Log($"✅ Found {canvases.Length} BattleDrawingCanvas component(s)");

            foreach (var canvas in canvases)
            {
                Log($"\n--- Fixing {canvas.gameObject.name} ---");

                // Fix using reflection since fields are private
                var canvasType = typeof(BattleDrawingCanvas);

                // 1. Set drawing color to BLACK
                SetPrivateField(canvasType, canvas, "drawingColor", Color.black);
                Log("✅ Set drawingColor to BLACK");

                // 2. Set line width to visible amount
                SetPrivateField(canvasType, canvas, "lineWidth", 0.15f);
                Log("✅ Set lineWidth to 0.15 (good visibility)");

                // 3. Fix line material color
                Material lineMaterial = GetPrivateField<Material>(canvasType, canvas, "lineMaterial");
                if (lineMaterial != null)
                {
                    lineMaterial.color = Color.black;
                    Log("✅ Set lineMaterial color to BLACK");
                }
                else
                {
                    // Create new black material
                    Material newMaterial = new Material(Shader.Find("Sprites/Default"));
                    newMaterial.color = Color.black;
                    SetPrivateField(canvasType, canvas, "lineMaterial", newMaterial);
                    Log("✅ Created new BLACK line material");
                }

                // 4. Verify drawing area background is visible
                UnityEngine.UI.Image bg = canvas.GetComponent<UnityEngine.UI.Image>();
                if (bg != null)
                {
                    if (bg.color.a < 0.9f)
                    {
                        bg.color = new Color(0.95f, 0.95f, 0.95f, 1f);
                        Log("✅ Fixed drawing area background (light gray)");
                    }
                    if (!bg.raycastTarget)
                    {
                        bg.raycastTarget = true;
                        Log("✅ Enabled raycast target for drawing");
                    }
                }

                // Mark as dirty
                EditorUtility.SetDirty(canvas);
                if (lineMaterial != null)
                {
                    EditorUtility.SetDirty(lineMaterial);
                }
            }

            Log("\n=== SUMMARY ===");
            Log($"Fixed {canvases.Length} drawing canvas(es)");
            Log("Drawing color: BLACK ✅");
            Log("Line width: 0.15 (visible) ✅");
            Log("Material color: BLACK ✅");
            Log("\n✅ DRAWING COLORS FIXED!");
            Log("Lines should now be VISIBLE (black on white background)");
            Log("Save your scene (Ctrl+S) to keep changes.");

            Debug.Log("[DrawingCanvasColorFixer] Drawing colors fixed successfully! Lines are now BLACK.");
        }

        private void SetPrivateField(System.Type type, object instance, string fieldName, object value)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(instance, value);
            }
            else
            {
                LogWarning($"⚠ Field '{fieldName}' not found");
            }
        }

        private T GetPrivateField<T>(System.Type type, object instance, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                return (T)field.GetValue(instance);
            }
            return default(T);
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

    /// <summary>
    /// Add context menu to BattleDrawingCanvas component
    /// </summary>
    [CustomEditor(typeof(BattleDrawingCanvas))]
    public class BattleDrawingCanvasEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("If lines are invisible, click 'Fix Colors' below!", MessageType.Info);

            if (GUILayout.Button("Fix Colors (Make Lines BLACK)", GUILayout.Height(30)))
            {
                BattleDrawingCanvas canvas = (BattleDrawingCanvas)target;

                // Fix using reflection
                var canvasType = typeof(BattleDrawingCanvas);

                // Set drawing color to BLACK
                var colorField = canvasType.GetField("drawingColor", BindingFlags.NonPublic | BindingFlags.Instance);
                if (colorField != null)
                {
                    colorField.SetValue(canvas, Color.black);
                    Debug.Log("✅ Set drawingColor to BLACK");
                }

                // Set line width
                var widthField = canvasType.GetField("lineWidth", BindingFlags.NonPublic | BindingFlags.Instance);
                if (widthField != null)
                {
                    widthField.SetValue(canvas, 0.15f);
                    Debug.Log("✅ Set lineWidth to 0.15");
                }

                // Fix material
                var matField = canvasType.GetField("lineMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
                if (matField != null)
                {
                    Material mat = (Material)matField.GetValue(canvas);
                    if (mat == null)
                    {
                        mat = new Material(Shader.Find("Sprites/Default"));
                    }
                    mat.color = Color.black;
                    matField.SetValue(canvas, mat);
                    Debug.Log("✅ Set lineMaterial to BLACK");
                }

                EditorUtility.SetDirty(canvas);
                Debug.Log("✅ Drawing colors fixed! Lines are now BLACK and visible.");
            }
        }
    }
}
