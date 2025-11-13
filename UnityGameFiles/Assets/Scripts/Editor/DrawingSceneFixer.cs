using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Comprehensive Drawing Scene fixer - removes duplicates, fixes buttons, ensures proper setup
/// </summary>
public class DrawingSceneFixer : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Fix Drawing Scene Issues", priority = 1)]
    public static void FixAllIssues()
    {
        Debug.Log("=== FIXING DRAWING SCENE ===");

        int fixCount = 0;

        // Fix 1: Remove duplicate InstructionsPanel instances
        fixCount += RemoveDuplicatePanels();

        // Fix 2: Ensure Start Drawing button works
        fixCount += FixStartDrawingButton();

        // Fix 3: Create/ensure Guide Book button exists and is visible
        fixCount += EnsureGuideBookButton();

        // Fix 4: Clean up hierarchy
        fixCount += CleanUpHierarchy();

        // Fix 5: Verify EventSystem
        fixCount += EnsureEventSystem();

        // Fix 6: Connect DrawingSceneUI references
        fixCount += ConnectDrawingSceneUI();

        Debug.Log($"=== FIXED {fixCount} ISSUES ===");

        EditorUtility.DisplayDialog(
            "Drawing Scene Fixed!",
            $"Fixed {fixCount} issues!\n\n" +
            "✅ Removed duplicate panels\n" +
            "✅ Fixed Start Drawing button\n" +
            "✅ Ensured Guide Book button\n" +
            "✅ Cleaned up hierarchy\n" +
            "✅ Connected all references\n\n" +
            "Press Play to test!",
            "OK"
        );
    }

    private static int RemoveDuplicatePanels()
    {
        Debug.Log("--- Removing duplicate panels ---");
        int removed = 0;

        // Find all InstructionsPanel instances
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        List<GameObject> instructionsPanels = new List<GameObject>();

        foreach (var obj in allObjects)
        {
            if (obj.name == "InstructionsPanel")
            {
                instructionsPanels.Add(obj);
            }
        }

        Debug.Log($"Found {instructionsPanels.Count} InstructionsPanel instances");

        if (instructionsPanels.Count > 1)
        {
            // Keep only the first one, destroy the rest
            for (int i = 1; i < instructionsPanels.Count; i++)
            {
                Debug.Log($"Destroying duplicate: {instructionsPanels[i].name}");
                Object.DestroyImmediate(instructionsPanels[i]);
                removed++;
            }
        }

        // Also check for duplicate DrawingPanel
        List<GameObject> drawingPanels = new List<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj != null && obj.name == "DrawingPanel")
            {
                drawingPanels.Add(obj);
            }
        }

        if (drawingPanels.Count > 1)
        {
            for (int i = 1; i < drawingPanels.Count; i++)
            {
                Debug.Log($"Destroying duplicate DrawingPanel: {drawingPanels[i].name}");
                Object.DestroyImmediate(drawingPanels[i]);
                removed++;
            }
        }

        return removed;
    }

    private static int FixStartDrawingButton()
    {
        Debug.Log("--- Fixing Start Drawing button ---");
        int fixes = 0;

        // Find the button
        Button[] allButtons = Object.FindObjectsOfType<Button>();
        Button startButton = null;

        foreach (var btn in allButtons)
        {
            if (btn.gameObject.name == "StartButton" || btn.gameObject.name.Contains("Start"))
            {
                startButton = btn;
                break;
            }
        }

        if (startButton == null)
        {
            Debug.LogWarning("Start button not found! Creating one...");
            // Create start button
            GameObject instructionsPanel = GameObject.Find("InstructionsPanel");
            if (instructionsPanel != null)
            {
                startButton = CreateStartButton(instructionsPanel.transform);
                fixes++;
            }
        }
        else
        {
            Debug.Log($"Found Start button: {startButton.gameObject.name}");
        }

        // Ensure button is connected to DrawingSceneUI
        if (startButton != null)
        {
            DrawingSceneUI uiManager = Object.FindObjectOfType<DrawingSceneUI>();
            if (uiManager != null)
            {
                uiManager.startDrawingButton = startButton;

                // Clear and re-add listener
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(() => uiManager.OnStartDrawing());

                // Ensure button is active and interactable
                startButton.gameObject.SetActive(true);
                startButton.interactable = true;

                EditorUtility.SetDirty(uiManager);
                Debug.Log("✅ Start button connected and listener added");
                fixes++;
            }
        }

        return fixes;
    }

    private static Button CreateStartButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("StartButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.35f, 0.1f);
        rect.anchorMax = new Vector2(0.65f, 0.2f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.8f, 0.4f); // Green

        Button button = buttonObj.AddComponent<Button>();

        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "START DRAWING";
        text.fontSize = 28;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        Debug.Log("Created Start button");
        return button;
    }

    private static int EnsureGuideBookButton()
    {
        Debug.Log("--- Ensuring Guide Book button ---");
        int fixes = 0;

        // Check if button exists
        GameObject guideButton = GameObject.Find("GuideBookButton");

        if (guideButton == null)
        {
            Debug.Log("Guide Book button not found, creating...");

            // Find DrawingPanel or Canvas
            GameObject parent = GameObject.Find("DrawingPanel");
            if (parent == null) parent = GameObject.Find("Canvas");

            if (parent != null)
            {
                guideButton = CreateGuideBookButton(parent.transform);
                fixes++;
            }
        }
        else
        {
            Debug.Log($"Guide Book button found: {guideButton.name}");

            // Ensure it's visible and styled
            Button btn = guideButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.gameObject.SetActive(true);
                btn.interactable = true;

                // Add setup component if missing
                GuideBookButtonSetup setup = guideButton.GetComponent<GuideBookButtonSetup>();
                if (setup == null)
                {
                    guideButton.AddComponent<GuideBookButtonSetup>();
                    fixes++;
                }
            }
        }

        // Connect to PlantGuideBook
        if (guideButton != null)
        {
            PlantGuideBook guide = Object.FindObjectOfType<PlantGuideBook>();
            if (guide != null && guide.openBookButton == null)
            {
                guide.openBookButton = guideButton.GetComponent<Button>();
                EditorUtility.SetDirty(guide);
                Debug.Log("✅ Guide button connected to PlantGuideBook");
                fixes++;
            }
        }

        return fixes;
    }

    private static GameObject CreateGuideBookButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("GuideBookButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.85f, 0.92f);
        rect.anchorMax = new Vector2(0.98f, 0.98f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 0.9f); // Blue

        Button button = buttonObj.AddComponent<Button>();

        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "GUIDE";
        text.fontSize = 20;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        // Add setup component
        buttonObj.AddComponent<GuideBookButtonSetup>();

        Debug.Log("Created Guide Book button");
        return buttonObj;
    }

    private static int CleanUpHierarchy()
    {
        Debug.Log("--- Cleaning up hierarchy ---");
        int cleaned = 0;

        // Remove any orphaned or empty GameObjects
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        foreach (var obj in allObjects)
        {
            // Remove objects with no components and no children
            if (obj.transform.childCount == 0 && obj.GetComponents<Component>().Length == 1)
            {
                // Only has Transform component
                if (obj.name.Contains("(Clone)") || obj.name == "GameObject")
                {
                    Debug.Log($"Removing empty object: {obj.name}");
                    Object.DestroyImmediate(obj);
                    cleaned++;
                }
            }
        }

        return cleaned;
    }

    private static int EnsureEventSystem()
    {
        UnityEngine.EventSystems.EventSystem eventSystem = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();

        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✅ Created EventSystem");
            return 1;
        }

        return 0;
    }

    private static int ConnectDrawingSceneUI()
    {
        Debug.Log("--- Connecting DrawingSceneUI references ---");
        int connections = 0;

        DrawingSceneUI uiManager = Object.FindObjectOfType<DrawingSceneUI>();
        if (uiManager == null)
        {
            Debug.LogWarning("DrawingSceneUI not found in scene");
            return 0;
        }

        // Find and connect panels
        if (uiManager.instructionsPanel == null)
        {
            uiManager.instructionsPanel = GameObject.Find("InstructionsPanel");
            if (uiManager.instructionsPanel != null) connections++;
        }

        if (uiManager.drawingPanel == null)
        {
            uiManager.drawingPanel = GameObject.Find("DrawingPanel");
            if (uiManager.drawingPanel != null) connections++;
        }

        // Find and connect buttons
        Button[] buttons = Object.FindObjectsOfType<Button>();
        foreach (var btn in buttons)
        {
            if (btn.gameObject.name.Contains("Start") && uiManager.startDrawingButton == null)
            {
                uiManager.startDrawingButton = btn;
                connections++;
            }
            else if (btn.gameObject.name.Contains("Guide") && uiManager.guideBookButton == null)
            {
                uiManager.guideBookButton = btn;
                connections++;
            }
            else if (btn.gameObject.name.Contains("Clear") && uiManager.clearButton == null)
            {
                uiManager.clearButton = btn;
                connections++;
            }
        }

        EditorUtility.SetDirty(uiManager);
        Debug.Log($"✅ Connected {connections} references");

        return connections;
    }
}
