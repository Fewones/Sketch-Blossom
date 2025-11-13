using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to automatically set up the drawing recognition system
/// Run via: Tools > Sketch Blossom > Setup Drawing System
/// </summary>
public class SetupDrawingSystem : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Setup Drawing System (Auto)")]
    public static void AutoSetup()
    {
        Debug.Log("========== SETTING UP DRAWING SYSTEM ==========");

        // Find or create DrawingManager
        DrawingManager manager = FindObjectOfType<DrawingManager>();
        if (manager == null)
        {
            GameObject managerObj = new GameObject("DrawingManager");
            manager = managerObj.AddComponent<DrawingManager>();
            Debug.Log("✓ Created DrawingManager");
        }
        else
        {
            Debug.Log("✓ Found existing DrawingManager");
        }

        // Find or create SimpleDrawingCanvas
        SimpleDrawingCanvas canvas = FindObjectOfType<SimpleDrawingCanvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("SimpleDrawingCanvas");
            canvas = canvasObj.AddComponent<SimpleDrawingCanvas>();
            Debug.Log("✓ Created SimpleDrawingCanvas");
        }
        else
        {
            Debug.Log("✓ Found existing SimpleDrawingCanvas");
        }

        // Setup SimpleDrawingCanvas references
        SetupSimpleCanvas(canvas);

        // Find or create PlantRecognitionSystem
        PlantRecognitionSystem recognitionSystem = FindObjectOfType<PlantRecognitionSystem>();
        if (recognitionSystem == null)
        {
            GameObject recObj = new GameObject("PlantRecognitionSystem");
            recognitionSystem = recObj.AddComponent<PlantRecognitionSystem>();
            Debug.Log("✓ Created PlantRecognitionSystem");
        }
        else
        {
            Debug.Log("✓ Found existing PlantRecognitionSystem");
        }

        // Setup DrawingManager references
        SetupDrawingManager(manager, canvas, recognitionSystem);

        // Find UI elements
        SetupUIReferences(manager);

        // Link color selector to SimpleDrawingCanvas
        SetupColorSelector(canvas);

        // Mark objects as dirty
        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(recognitionSystem);

        Debug.Log("========== SETUP COMPLETE ==========");

        string message = "Drawing system has been set up!\n\n";
        if (canvas.lineRendererPrefab != null)
        {
            message += "✓ All references configured!\n\n";
            message += "Ready to test:\n";
            message += "1. Enter Play mode\n";
            message += "2. Draw in the DrawingPanel\n";
            message += "3. Use color buttons to change colors\n";
            message += "4. Click Finish to analyze drawing";
        }
        else
        {
            message += "⚠️ Action required:\n";
            message += "Please assign LineRenderer prefab manually\n";
            message += "to SimpleDrawingCanvas in the Inspector.";
        }

        EditorUtility.DisplayDialog("Setup Complete", message, "OK");
    }

    static void SetupSimpleCanvas(SimpleDrawingCanvas canvas)
    {
        // Find camera
        if (canvas.mainCamera == null)
        {
            canvas.mainCamera = Camera.main;
            if (canvas.mainCamera != null)
                Debug.Log("  ✓ Assigned main camera");
            else
                Debug.LogWarning("  ⚠️ Main camera not found! Please assign manually.");
        }

        // Find or create stroke container
        if (canvas.strokeContainer == null)
        {
            Transform container = canvas.transform.Find("StrokeContainer");
            if (container == null)
            {
                GameObject containerObj = new GameObject("StrokeContainer");
                containerObj.transform.SetParent(canvas.transform);
                containerObj.transform.localPosition = Vector3.zero;
                canvas.strokeContainer = containerObj.transform;
                Debug.Log("  ✓ Created StrokeContainer");
            }
            else
            {
                canvas.strokeContainer = container;
                Debug.Log("  ✓ Found StrokeContainer");
            }
        }

        // Find LineRenderer prefab
        if (canvas.lineRendererPrefab == null)
        {
            // Try to find existing prefabs
            string[] prefabPaths = {
                "Assets/Prefabs/LineRenderer.prefab",
                "Assets/StrokeLine.prefab"
            };

            foreach (string path in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    LineRenderer lr = prefab.GetComponent<LineRenderer>();
                    if (lr != null)
                    {
                        canvas.lineRendererPrefab = lr;
                        Debug.Log($"  ✓ Assigned LineRenderer prefab from {path}");
                        break;
                    }
                }
            }

            if (canvas.lineRendererPrefab == null)
            {
                Debug.LogWarning("  ⚠️ LineRenderer prefab not found! Please assign manually from Assets/Prefabs/LineRenderer.prefab");
            }
        }

        // Find drawing area
        if (canvas.drawingArea == null)
        {
            Canvas uiCanvas = FindObjectOfType<Canvas>();
            if (uiCanvas != null)
            {
                Transform drawingPanel = uiCanvas.transform.Find("DrawingPanel");
                if (drawingPanel != null)
                {
                    canvas.drawingArea = drawingPanel.GetComponent<RectTransform>();
                    Debug.Log("  ✓ Found DrawingPanel as drawing area");
                }
                else
                {
                    Debug.LogWarning("  ⚠️ DrawingPanel not found in Canvas! Drawing area not set.");
                }
            }
            else
            {
                Debug.LogWarning("  ⚠️ No Canvas found in scene!");
            }
        }

        EditorUtility.SetDirty(canvas);
    }

    static void SetupDrawingManager(DrawingManager manager, SimpleDrawingCanvas canvas, PlantRecognitionSystem recognition)
    {
        // Use reflection to set the simple canvas reference
        var field = typeof(DrawingManager).GetField("simpleCanvas",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(manager, canvas);
            Debug.Log("  ✓ Linked SimpleDrawingCanvas to DrawingManager");
        }

        // Set recognition system
        var recField = typeof(DrawingManager).GetField("recognitionSystem",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (recField != null)
        {
            recField.SetValue(manager, recognition);
            Debug.Log("  ✓ Linked PlantRecognitionSystem to DrawingManager");
        }

        EditorUtility.SetDirty(manager);
    }

    static void SetupUIReferences(DrawingManager manager)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("  ⚠️ No Canvas found in scene");
            return;
        }

        // Find PlantResultPanel
        PlantResultPanel resultPanel = FindObjectOfType<PlantResultPanel>(true);
        if (resultPanel != null)
        {
            var field = typeof(DrawingManager).GetField("plantResultPanel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(manager, resultPanel);
                Debug.Log("  ✓ Linked PlantResultPanel");
            }
        }

        EditorUtility.SetDirty(manager);
    }

    static void SetupColorSelector(SimpleDrawingCanvas canvas)
    {
        // Find existing DrawingColorSelector
        DrawingColorSelector colorSelector = FindObjectOfType<DrawingColorSelector>();
        if (colorSelector != null)
        {
            // Link SimpleDrawingCanvas to color selector
            var field = typeof(DrawingColorSelector).GetField("simpleDrawingCanvas",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(colorSelector, canvas);
                Debug.Log("  ✓ Linked SimpleDrawingCanvas to DrawingColorSelector");
            }

            EditorUtility.SetDirty(colorSelector);
        }
        else
        {
            Debug.LogWarning("  ⚠️ DrawingColorSelector not found in scene - color buttons won't work");
        }
    }

    [MenuItem("Tools/Sketch Blossom/Create Color Buttons")]
    public static void CreateColorButtons()
    {
        SimpleDrawingCanvas canvas = FindObjectOfType<SimpleDrawingCanvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "SimpleDrawingCanvas not found!\nRun 'Setup Drawing System' first.", "OK");
            return;
        }

        Canvas uiCanvas = FindObjectOfType<Canvas>();
        if (uiCanvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene!", "OK");
            return;
        }

        // Check if color buttons already exist
        Transform existingPanel = uiCanvas.transform.Find("ColorButtonPanel");
        if (existingPanel != null)
        {
            bool delete = EditorUtility.DisplayDialog("Color Buttons Exist",
                "Color buttons already exist.\nDelete and recreate them?", "Yes", "Cancel");
            if (delete)
            {
                GameObject.DestroyImmediate(existingPanel.gameObject);
                Debug.Log("Deleted existing color buttons");
            }
            else
            {
                return;
            }
        }

        // Create color button panel
        GameObject buttonPanel = new GameObject("ColorButtonPanel");
        buttonPanel.transform.SetParent(uiCanvas.transform, false); // Set worldPositionStays to false

        RectTransform panelRect = buttonPanel.AddComponent<RectTransform>();

        // Anchor to top-left corner
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);

        // Position from top-left
        panelRect.anchoredPosition = new Vector2(10, -10);

        // Small panel size
        panelRect.sizeDelta = new Vector2(150, 40);

        // Reset scale to ensure proper sizing
        panelRect.localScale = Vector3.one;

        HorizontalLayoutGroup layout = buttonPanel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 5;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(5, 5, 5, 5);

        // Create Red button
        CreateColorButton(buttonPanel.transform, "RedButton", Color.red, canvas);

        // Create Green button
        CreateColorButton(buttonPanel.transform, "GreenButton", Color.green, canvas);

        // Create Blue button
        CreateColorButton(buttonPanel.transform, "BlueButton", Color.blue, canvas);

        Debug.Log("✓ Created color buttons (40x40 pixels)");
        EditorUtility.DisplayDialog("Success",
            "Color buttons created!\n\n" +
            "They are small buttons in the top-left corner.\n" +
            "They are linked to SimpleDrawingCanvas for color selection.",
            "OK");
    }

    static void CreateColorButton(Transform parent, string name, Color color, SimpleDrawingCanvas canvas)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false); // Set worldPositionStays to false

        RectTransform rect = buttonObj.AddComponent<RectTransform>();

        // Smaller button size (40x40 pixels)
        rect.sizeDelta = new Vector2(40, 40);

        // Reset local position and scale
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;

        Image image = buttonObj.AddComponent<Image>();
        image.color = color;

        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => canvas.SetColor(color));

        Debug.Log($"  Created {name} (40x40 pixels)");
    }

    [MenuItem("Tools/Sketch Blossom/Delete Color Buttons")]
    public static void DeleteColorButtons()
    {
        Canvas uiCanvas = FindObjectOfType<Canvas>();
        if (uiCanvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene!", "OK");
            return;
        }

        Transform buttonPanel = uiCanvas.transform.Find("ColorButtonPanel");
        if (buttonPanel != null)
        {
            GameObject.DestroyImmediate(buttonPanel.gameObject);
            Debug.Log("✓ Deleted color buttons");
            EditorUtility.DisplayDialog("Success", "Color buttons deleted!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Not Found", "ColorButtonPanel not found in scene.", "OK");
        }
    }
}
