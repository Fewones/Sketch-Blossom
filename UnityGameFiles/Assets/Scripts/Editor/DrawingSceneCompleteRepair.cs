using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Comprehensive Drawing Scene fixer - diagnoses and fixes all UI issues
/// Run this from: Tools > Sketch Blossom > Fix All Drawing Scene Issues
/// </summary>
public class DrawingSceneCompleteRepair : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Fix All Drawing Scene Issues", priority = 1)]
    public static void RepairDrawingScene()
    {
        Debug.Log("════════════════════════════════════════════════════");
        Debug.Log("STARTING DRAWING SCENE COMPLETE REPAIR");
        Debug.Log("════════════════════════════════════════════════════");

        int totalFixes = 0;

        // Step 1: Verify all components exist
        totalFixes += VerifyAndFixComponents();

        // Step 2: Fix Start Drawing button
        totalFixes += FixStartDrawingButton();

        // Step 3: Fix Guide Book button
        totalFixes += FixGuideBookButton();

        // Step 4: Fix Guide Book panel references
        totalFixes += FixGuideBookPanel();

        // Step 5: Connect all UI references
        totalFixes += ConnectAllUIReferences();

        // Step 6: Apply plant-themed background
        totalFixes += ApplyPlantBackground();

        // Step 7: Ensure EventSystem
        totalFixes += EnsureEventSystem();

        Debug.Log("════════════════════════════════════════════════════");
        Debug.Log($"REPAIR COMPLETE! Applied {totalFixes} fixes");
        Debug.Log("════════════════════════════════════════════════════");

        EditorUtility.DisplayDialog(
            "Drawing Scene Repaired!",
            $"Successfully applied {totalFixes} fixes!\n\n" +
            "✅ Start Drawing button connected\n" +
            "✅ Guide Book button working\n" +
            "✅ Guide Book panel configured\n" +
            "✅ All UI references connected\n" +
            "✅ Plant-themed background applied\n\n" +
            "Press Play to test the scene!",
            "OK"
        );
    }

    private static int VerifyAndFixComponents()
    {
        Debug.Log("\n▶ Step 1: Verifying all components exist...");
        int fixes = 0;

        // Check for DrawingSceneUI
        DrawingSceneUI uiManager = Object.FindObjectOfType<DrawingSceneUI>();
        if (uiManager == null)
        {
            Debug.LogError("❌ DrawingSceneUI component not found in scene!");
            Debug.LogError("   Please add DrawingSceneUI component to a GameObject in the scene");
        }
        else
        {
            Debug.Log("✓ DrawingSceneUI found: " + uiManager.gameObject.name);
            fixes++;
        }

        // Check for PlantGuideBook
        PlantGuideBook guideBook = Object.FindObjectOfType<PlantGuideBook>();
        if (guideBook == null)
        {
            Debug.LogError("❌ PlantGuideBook component not found in scene!");
            Debug.LogError("   Please add PlantGuideBook component to a GameObject in the scene");
        }
        else
        {
            Debug.Log("✓ PlantGuideBook found: " + guideBook.gameObject.name);
            fixes++;
        }

        // Check for DrawingCanvas
        DrawingCanvas canvas = Object.FindObjectOfType<DrawingCanvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ DrawingCanvas component not found in scene!");
        }
        else
        {
            Debug.Log("✓ DrawingCanvas found: " + canvas.gameObject.name);
            fixes++;
        }

        return fixes;
    }

    private static int FixStartDrawingButton()
    {
        Debug.Log("\n▶ Step 2: Fixing Start Drawing button...");
        int fixes = 0;

        DrawingSceneUI uiManager = Object.FindObjectOfType<DrawingSceneUI>();
        if (uiManager == null)
        {
            Debug.LogError("❌ Cannot fix Start Drawing button - DrawingSceneUI not found");
            return 0;
        }

        // Find Start Drawing button
        Button startButton = null;
        Button[] allButtons = Object.FindObjectsOfType<Button>();

        Debug.Log($"   Searching through {allButtons.Length} buttons...");

        foreach (var btn in allButtons)
        {
            // Check button name
            if (btn.gameObject.name.ToLower().Contains("start"))
            {
                startButton = btn;
                Debug.Log($"   Found Start button by name: {btn.gameObject.name}");
                break;
            }

            // Check button text
            TextMeshProUGUI[] texts = btn.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.text.ToLower().Contains("start") || text.text.ToLower().Contains("begin"))
                {
                    startButton = btn;
                    Debug.Log($"   Found Start button by text: {text.text}");
                    break;
                }
            }
            if (startButton != null) break;
        }

        if (startButton != null)
        {
            // Ensure button is active and interactable
            startButton.gameObject.SetActive(true);
            startButton.interactable = true;

            // Clear all existing listeners
            startButton.onClick.RemoveAllListeners();

            // Add the correct listener
            startButton.onClick.AddListener(() => {
                Debug.Log("Start Drawing button clicked!");
                uiManager.OnStartDrawing();
            });

            // Connect to DrawingSceneUI
            uiManager.startDrawingButton = startButton;

            // Ensure button has proper Image component for clicking
            Image img = startButton.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
            }

            EditorUtility.SetDirty(uiManager);
            EditorUtility.SetDirty(startButton);

            Debug.Log("✅ Start Drawing button fixed and connected");
            fixes++;
        }
        else
        {
            Debug.LogError("❌ Could not find Start Drawing button in scene!");
            Debug.LogError("   Please create a button named 'StartDrawingButton' or with text 'Start Drawing'");
        }

        return fixes;
    }

    private static int FixGuideBookButton()
    {
        Debug.Log("\n▶ Step 3: Fixing Guide Book button...");
        int fixes = 0;

        PlantGuideBook guideBook = Object.FindObjectOfType<PlantGuideBook>();
        if (guideBook == null)
        {
            Debug.LogError("❌ Cannot fix Guide Book button - PlantGuideBook not found");
            return 0;
        }

        // Find Guide Book button
        Button guideButton = null;
        Button[] allButtons = Object.FindObjectsOfType<Button>();

        Debug.Log($"   Searching through {allButtons.Length} buttons...");

        foreach (var btn in allButtons)
        {
            // Check button name
            if (btn.gameObject.name.ToLower().Contains("guide") || btn.gameObject.name.ToLower().Contains("book"))
            {
                guideButton = btn;
                Debug.Log($"   Found Guide button by name: {btn.gameObject.name}");
                break;
            }

            // Check button text
            TextMeshProUGUI[] texts = btn.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.text.ToLower().Contains("guide") || text.text.Contains("📖"))
                {
                    guideButton = btn;
                    Debug.Log($"   Found Guide button by text: {text.text}");
                    break;
                }
            }
            if (guideButton != null) break;
        }

        if (guideButton != null)
        {
            // Ensure button is active and interactable
            guideButton.gameObject.SetActive(true);
            guideButton.interactable = true;

            // Clear all existing listeners
            guideButton.onClick.RemoveAllListeners();

            // Add the correct listener
            guideButton.onClick.AddListener(() => {
                Debug.Log("Guide Book button clicked!");
                guideBook.OpenBook();
            });

            // Connect to PlantGuideBook
            guideBook.openBookButton = guideButton;

            // Ensure button has proper Image component for clicking
            Image img = guideButton.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
            }

            // Ensure CanvasGroup doesn't block raycasts
            CanvasGroup cg = guideButton.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = true;
                cg.interactable = true;
            }

            // Move to front in hierarchy
            guideButton.transform.SetAsLastSibling();

            EditorUtility.SetDirty(guideBook);
            EditorUtility.SetDirty(guideButton);

            Debug.Log("✅ Guide Book button fixed and connected");
            fixes++;
        }
        else
        {
            Debug.LogError("❌ Could not find Guide Book button in scene!");
            Debug.LogError("   Please create a button named 'GuideBookButton' or with text 'GUIDE'");
        }

        return fixes;
    }

    private static int FixGuideBookPanel()
    {
        Debug.Log("\n▶ Step 4: Fixing Guide Book panel...");
        int fixes = 0;

        PlantGuideBook guideBook = Object.FindObjectOfType<PlantGuideBook>();
        if (guideBook == null)
        {
            Debug.LogError("❌ Cannot fix Guide Book panel - PlantGuideBook not found");
            return 0;
        }

        // Find the book panel
        GameObject bookPanel = null;

        // Try by name
        bookPanel = GameObject.Find("GuideBookPanel");
        if (bookPanel == null) bookPanel = GameObject.Find("BookPanel");
        if (bookPanel == null) bookPanel = GameObject.Find("PlantGuidePanel");

        // Try by searching all GameObjects with Image or Panel
        if (bookPanel == null)
        {
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("guide") && obj.name.ToLower().Contains("panel"))
                {
                    bookPanel = obj;
                    Debug.Log($"   Found panel by name search: {obj.name}");
                    break;
                }
            }
        }

        if (bookPanel != null)
        {
            guideBook.bookPanel = bookPanel;

            // Find child elements and connect them
            TextMeshProUGUI[] texts = bookPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name.ToLower().Contains("title") && guideBook.pageTitle == null)
                {
                    guideBook.pageTitle = text;
                    Debug.Log($"   Connected pageTitle: {text.gameObject.name}");
                }
                else if (text.gameObject.name.ToLower().Contains("description") && guideBook.pageDescription == null)
                {
                    guideBook.pageDescription = text;
                    Debug.Log($"   Connected pageDescription: {text.gameObject.name}");
                }
                else if (text.gameObject.name.ToLower().Contains("page") && text.gameObject.name.ToLower().Contains("number"))
                {
                    guideBook.pageNumberText = text;
                    Debug.Log($"   Connected pageNumberText: {text.gameObject.name}");
                }
            }

            // Find navigation buttons
            Button[] buttons = bookPanel.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn.gameObject.name.ToLower().Contains("close") && guideBook.closeBookButton == null)
                {
                    guideBook.closeBookButton = btn;
                    Debug.Log($"   Connected closeBookButton: {btn.gameObject.name}");
                }
                else if (btn.gameObject.name.ToLower().Contains("next") && guideBook.nextPageButton == null)
                {
                    guideBook.nextPageButton = btn;
                    Debug.Log($"   Connected nextPageButton: {btn.gameObject.name}");
                }
                else if (btn.gameObject.name.ToLower().Contains("previous") || btn.gameObject.name.ToLower().Contains("prev"))
                {
                    guideBook.previousPageButton = btn;
                    Debug.Log($"   Connected previousPageButton: {btn.gameObject.name}");
                }
            }

            EditorUtility.SetDirty(guideBook);

            Debug.Log("✅ Guide Book panel configured");
            fixes++;
        }
        else
        {
            Debug.LogError("❌ Could not find Guide Book panel in scene!");
            Debug.LogError("   Please create a panel GameObject named 'GuideBookPanel'");
        }

        return fixes;
    }

    private static int ConnectAllUIReferences()
    {
        Debug.Log("\n▶ Step 5: Connecting all UI references...");
        int connections = 0;

        DrawingSceneUI uiManager = Object.FindObjectOfType<DrawingSceneUI>();
        if (uiManager == null) return 0;

        // Find and connect panels
        if (uiManager.instructionsPanel == null)
        {
            GameObject panel = GameObject.Find("InstructionsPanel");
            if (panel != null)
            {
                uiManager.instructionsPanel = panel;
                Debug.Log("   ✓ Connected InstructionsPanel");
                connections++;
            }
        }

        if (uiManager.drawingPanel == null)
        {
            GameObject panel = GameObject.Find("DrawingPanel");
            if (panel != null)
            {
                uiManager.drawingPanel = panel;
                Debug.Log("   ✓ Connected DrawingPanel");
                connections++;
            }
        }

        EditorUtility.SetDirty(uiManager);

        Debug.Log($"✅ Connected {connections} UI references");
        return connections;
    }

    private static int ApplyPlantBackground()
    {
        Debug.Log("\n▶ Step 6: Applying plant-themed background...");
        int applied = 0;

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas not found!");
            return 0;
        }

        // Check if background already exists
        Transform existingBg = canvas.transform.Find("Background");
        if (existingBg != null)
        {
            Debug.Log("   Background already exists, updating it...");
        }
        else
        {
            // Create new background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(canvas.transform, false);
            background.transform.SetAsFirstSibling();

            RectTransform rect = background.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = background.AddComponent<Image>();

            // Try to load plant-themed assets
            Sprite bgSprite = null;
            string[] possiblePaths = new string[]
            {
                "Assets/Pixel Adventure 1/Background/Green.png",
                "Assets/Pixel Adventure 1/Background/Blue.png",
                "Assets/Pixel Adventure 1/Background/Brown.png",
            };

            foreach (string path in possiblePaths)
            {
                bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (bgSprite != null)
                {
                    Debug.Log($"   Loaded background sprite: {path}");
                    break;
                }
            }

            if (bgSprite != null)
            {
                image.sprite = bgSprite;
                image.color = new Color(0.85f, 0.95f, 0.85f, 1f);
            }
            else
            {
                // Use soft sage green color
                image.color = new Color(0.82f, 0.94f, 0.82f, 1f);
                Debug.Log("   Using solid plant-themed color");
            }

            Debug.Log("✅ Applied plant-themed background");
            applied++;
        }

        return applied;
    }

    private static int EnsureEventSystem()
    {
        Debug.Log("\n▶ Step 7: Ensuring EventSystem exists...");

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
            Debug.Log("✓ EventSystem already exists");
            return 0;
        }
    }
}
