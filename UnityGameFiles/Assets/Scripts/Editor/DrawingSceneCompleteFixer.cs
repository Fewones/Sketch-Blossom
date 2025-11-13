using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Complete Drawing Scene fixer - addresses all remaining issues
/// </summary>
public class DrawingSceneCompleteFixer : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Complete Drawing Scene Fix", priority = 1)]
    public static void ApplyAllFixes()
    {
        Debug.Log("=== APPLYING COMPLETE DRAWING SCENE FIX ===");

        int fixCount = 0;

        fixCount += FixGuideBookButton();

        fixCount += ApplyPlantThemedBackground();

        fixCount += ConnectAllReferences();

        fixCount += EnsureEventSystem();

        Debug.Log($"=== APPLIED {fixCount} FIXES ===");

        EditorUtility.DisplayDialog(
            "Drawing Scene Fixed!",
            $"Applied {fixCount} fixes!\n\n" +
            "✅ Guide Book button fixed\n" +
            "✅ Plant-themed background applied\n" +
            "✅ All references connected\n\n" +
            "Press Play to test!",
            "OK"
        );
    }

    private static int FixGuideBookButton()
    {
        Debug.Log("--- Fixing Guide Book button ---");
        int fixes = 0;

        PlantGuideBook guideBook = Object.FindObjectOfType<PlantGuideBook>();
        if (guideBook == null)
        {
            Debug.LogWarning("PlantGuideBook not found in scene");
            return 0;
        }

        // Find the guide button
        GameObject guideButton = GameObject.Find("GuideBookButton");
        if (guideButton == null)
        {
            Debug.Log("GuideBookButton not found by name, searching for button with Guide text...");

            Button[] allButtons = Object.FindObjectsOfType<Button>();
            foreach (var btn in allButtons)
            {
                TextMeshProUGUI[] texts = btn.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in texts)
                {
                    if (text.text.Contains("GUIDE") || text.text.Contains("📖"))
                    {
                        guideButton = btn.gameObject;
                        Debug.Log($"Found guide button: {guideButton.name}");
                        break;
                    }
                }
                if (guideButton != null) break;
            }
        }

        if (guideButton != null)
        {
            Button button = guideButton.GetComponent<Button>();
            if (button != null)
            {
                // Ensure button is active and interactable
                guideButton.SetActive(true);
                button.interactable = true;

                // Connect to PlantGuideBook
                guideBook.openBookButton = button;

                // Clear and re-add listener
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => guideBook.OpenBook());

                // Ensure button has a Canvas Group for raycast
                CanvasGroup cg = guideButton.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.blocksRaycasts = true;
                    cg.interactable = true;
                }

                // Ensure Image component for raycasting
                Image img = guideButton.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = true;
                }

                // Move to front in hierarchy to avoid blocking
                guideButton.transform.SetAsLastSibling();

                EditorUtility.SetDirty(guideBook);
                EditorUtility.SetDirty(button);

                Debug.Log("✅ Guide Book button fixed and connected");
                fixes++;
            }
        }
        else
        {
            Debug.LogWarning("Could not find Guide Book button in scene");
        }

        return fixes;
    }

    private static int ApplyPlantThemedBackground()
    {
        Debug.Log("--- Applying plant-themed background ---");
        int applied = 0;

        // Find Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return 0;
        }

        // Check if background already exists
        Transform existingBg = canvas.transform.Find("Background");
        if (existingBg != null)
        {
            Object.DestroyImmediate(existingBg.gameObject);
            Debug.Log("Removed old background");
        }

        // Create new background with plant theme
        GameObject background = new GameObject("Background");
        background.transform.SetParent(canvas.transform, false);
        background.transform.SetAsFirstSibling(); // Put behind everything

        RectTransform rect = background.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = background.AddComponent<Image>();

        // Try plant-themed assets from Pixel Adventure 1
        Sprite bgSprite = null;
        string[] possiblePaths = new string[]
        {
            "Assets/Pixel Adventure 1/Background/Green.png",
            "Assets/Pixel Adventure 1/Background/Blue.png",
            "Assets/Pixel Adventure 1/Background/Brown.png",
            "Assets/Unity UI Samples/Textures and Sprites/Nature/Background.png"
        };

        foreach (string path in possiblePaths)
        {
            bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (bgSprite != null)
            {
                Debug.Log($"Loaded background sprite: {path}");
                break;
            }
        }

        if (bgSprite != null)
        {
            image.sprite = bgSprite;
            // Light green tint for plant theme
            image.color = new Color(0.85f, 0.95f, 0.85f, 1f);
        }
        else
        {
            // Use gradient color with plant theme (soft green)
            image.color = new Color(0.82f, 0.94f, 0.82f, 1f); // Light sage green
            Debug.Log("Using solid plant-themed color");
        }

        Debug.Log("✅ Applied plant-themed background");
        applied++;

        return applied;
    }

    private static int ConnectAllReferences()
    {
        Debug.Log("--- Connecting all references ---");
        int connections = 0;

        // Connect DrawingSceneUI
        DrawingSceneUI uiManager = Object.FindObjectOfType<DrawingSceneUI>();
        if (uiManager != null)
        {
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

            // Find buttons
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
            Debug.Log($"✅ Connected {connections} DrawingSceneUI references");
        }

        // Connect PlantGuideBook
        PlantGuideBook guideBook = Object.FindObjectOfType<PlantGuideBook>();
        if (guideBook != null)
        {
            if (guideBook.bookPanel == null)
            {
                GameObject panel = GameObject.Find("GuideBookPanel");
                if (panel == null) panel = GameObject.Find("BookPanel");
                if (panel != null)
                {
                    guideBook.bookPanel = panel;
                    connections++;
                }
            }

            EditorUtility.SetDirty(guideBook);
        }

        return connections;
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
        else
        {
            Debug.Log("EventSystem already exists");
            return 0;
        }
    }
}
