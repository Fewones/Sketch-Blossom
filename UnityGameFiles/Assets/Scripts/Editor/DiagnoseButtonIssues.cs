using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;

/// <summary>
/// Diagnoses and fixes UI button click issues
/// </summary>
public class DiagnoseButtonIssues : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Diagnose Continue Button Issues")]
    public static void DiagnoseButton()
    {
        Debug.Log("========== DIAGNOSING CONTINUE BUTTON ==========");

        // Find PlantResultPanel
        PlantResultPanel resultPanel = FindObjectOfType<PlantResultPanel>(true);
        if (resultPanel == null)
        {
            EditorUtility.DisplayDialog("Error",
                "PlantResultPanel not found in the scene!\n\nMake sure DrawingScene is open.",
                "OK");
            return;
        }

        SerializedObject so = new SerializedObject(resultPanel);
        SerializedProperty continueButtonProp = so.FindProperty("continueButton");

        if (continueButtonProp.objectReferenceValue == null)
        {
            Debug.LogError("❌ Continue button is NOT assigned in PlantResultPanel!");
            EditorUtility.DisplayDialog("Error",
                "Continue button is not assigned!\n\n" +
                "Run: Tools > Sketch Blossom > Complete Result Panel Setup",
                "OK");
            return;
        }

        Button continueButton = continueButtonProp.objectReferenceValue as Button;
        Debug.Log($"✓ Found Continue button: {continueButton.gameObject.name}");

        bool hasIssues = false;
        string issuesFound = "";

        // Check 1: Button is active
        if (!continueButton.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"⚠️ Button is INACTIVE! GameObject or parent is disabled.");
            issuesFound += "• Button or its parent is inactive\n";
            hasIssues = true;
        }
        else
        {
            Debug.Log("✓ Button is active in hierarchy");
        }

        // Check 2: Button is interactable
        if (!continueButton.interactable)
        {
            Debug.LogWarning($"⚠️ Button is NOT INTERACTABLE!");
            issuesFound += "• Button.interactable is false\n";
            hasIssues = true;
        }
        else
        {
            Debug.Log("✓ Button is interactable");
        }

        // Check 3: Button has Image component with Raycast Target enabled
        Image buttonImage = continueButton.GetComponent<Image>();
        if (buttonImage == null)
        {
            Debug.LogError("❌ Button has NO IMAGE component!");
            issuesFound += "• Button missing Image component\n";
            hasIssues = true;
        }
        else if (!buttonImage.raycastTarget)
        {
            Debug.LogWarning("⚠️ Button Image raycastTarget is DISABLED!");
            issuesFound += "• Button Image raycastTarget is false\n";
            hasIssues = true;
        }
        else
        {
            Debug.Log("✓ Button has Image with raycastTarget enabled");
        }

        // Check 4: EventSystem exists
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("❌ NO EVENTSYSTEM in scene!");
            issuesFound += "• No EventSystem found in scene\n";
            hasIssues = true;
        }
        else
        {
            Debug.Log($"✓ EventSystem exists: {eventSystem.gameObject.name}");
        }

        // Check 5: Canvas has GraphicRaycaster
        Canvas canvas = continueButton.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ Button is not under a Canvas!");
            issuesFound += "• Button is not under a Canvas\n";
            hasIssues = true;
        }
        else
        {
            Debug.Log($"✓ Button is under Canvas: {canvas.gameObject.name}");

            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogError("❌ Canvas has NO GraphicRaycaster!");
                issuesFound += "• Canvas missing GraphicRaycaster component\n";
                hasIssues = true;
            }
            else
            {
                Debug.Log("✓ Canvas has GraphicRaycaster");
            }
        }

        // Check 6: RectTransform size
        RectTransform rectTransform = continueButton.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 size = rectTransform.rect.size;
            Debug.Log($"✓ Button size: {size.x} x {size.y}");
            if (size.x < 10 || size.y < 10)
            {
                Debug.LogWarning("⚠️ Button size is very small!");
                issuesFound += $"• Button size is very small ({size.x}x{size.y})\n";
                hasIssues = true;
            }
        }

        // Check 7: Button has onClick listeners
        int listenerCount = continueButton.onClick.GetPersistentEventCount();
        Debug.Log($"Button has {listenerCount} persistent onClick listeners");
        if (listenerCount == 0)
        {
            Debug.LogWarning("⚠️ Button has NO onClick listeners!");
            issuesFound += "• Button has no onClick listeners configured\n";
            hasIssues = true;
        }
        else
        {
            for (int i = 0; i < listenerCount; i++)
            {
                var target = continueButton.onClick.GetPersistentTarget(i);
                var methodName = continueButton.onClick.GetPersistentMethodName(i);
                Debug.Log($"  Listener {i}: {target?.GetType().Name}.{methodName}");
            }
        }

        // Check 8: Check for blocking UI elements
        Canvas parentCanvas = continueButton.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            Image[] allImages = parentCanvas.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                if (img.raycastTarget && img != buttonImage)
                {
                    RectTransform imgRect = img.GetComponent<RectTransform>();
                    // Check if this image might be covering the button
                    if (imgRect != null && imgRect.IsParentOf(rectTransform))
                    {
                        Debug.LogWarning($"⚠️ Potential blocker: {img.gameObject.name} (parent with raycastTarget)");
                    }
                }
            }
        }

        Debug.Log("========== DIAGNOSIS COMPLETE ==========");

        if (hasIssues)
        {
            EditorUtility.DisplayDialog("Issues Found!",
                $"The Continue button has the following issues:\n\n{issuesFound}\n" +
                "Click 'Fix Button Issues' to attempt automatic repair.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("No Issues Found",
                "The Continue button appears to be set up correctly!\n\n" +
                "If it's still not clickable, check:\n" +
                "• Is the ResultPanel visible when you click?\n" +
                "• Is there another UI element blocking it?\n" +
                "• Check the Scene view to see button position",
                "OK");
        }
    }

    [MenuItem("Tools/Sketch Blossom/Fix Continue Button Issues")]
    public static void FixButtonIssues()
    {
        Debug.Log("========== FIXING CONTINUE BUTTON ISSUES ==========");

        // Find PlantResultPanel
        PlantResultPanel resultPanel = FindObjectOfType<PlantResultPanel>(true);
        if (resultPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "PlantResultPanel not found!", "OK");
            return;
        }

        SerializedObject so = new SerializedObject(resultPanel);
        SerializedProperty continueButtonProp = so.FindProperty("continueButton");

        if (continueButtonProp.objectReferenceValue == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Continue button is not assigned!\n\n" +
                "Run: Tools > Sketch Blossom > Complete Result Panel Setup",
                "OK");
            return;
        }

        Button continueButton = continueButtonProp.objectReferenceValue as Button;
        int fixesApplied = 0;

        // Fix 1: Make button interactable
        if (!continueButton.interactable)
        {
            continueButton.interactable = true;
            EditorUtility.SetDirty(continueButton);
            Debug.Log("✓ Set button.interactable = true");
            fixesApplied++;
        }

        // Fix 2: Ensure Image component with raycast target
        Image buttonImage = continueButton.GetComponent<Image>();
        if (buttonImage == null)
        {
            buttonImage = continueButton.gameObject.AddComponent<Image>();
            buttonImage.color = new Color(1, 1, 1, 0.01f); // Nearly transparent
            Debug.Log("✓ Added Image component to button");
            fixesApplied++;
        }

        if (!buttonImage.raycastTarget)
        {
            buttonImage.raycastTarget = true;
            EditorUtility.SetDirty(buttonImage);
            Debug.Log("✓ Enabled raycastTarget on button Image");
            fixesApplied++;
        }

        // Fix 3: Ensure EventSystem exists
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("✓ Created EventSystem");
            fixesApplied++;
        }

        // Fix 4: Ensure Canvas has GraphicRaycaster
        Canvas canvas = continueButton.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log($"✓ Added GraphicRaycaster to Canvas: {canvas.gameObject.name}");
                fixesApplied++;
            }
        }

        // Fix 5: Ensure button has listeners
        int listenerCount = continueButton.onClick.GetPersistentEventCount();
        if (listenerCount == 0)
        {
            Debug.Log("Button has no listeners - running setup script...");
            PlantResultPanelSetup.SetupResultPanelButtons();
            fixesApplied++;
        }

        Debug.Log($"========== APPLIED {fixesApplied} FIXES ==========");

        EditorUtility.DisplayDialog("Fixes Applied!",
            $"Applied {fixesApplied} fixes to the Continue button.\n\n" +
            "Make sure to save your scene!\n\n" +
            "Test the button now.",
            "OK");
    }

    [MenuItem("Tools/Sketch Blossom/Add Click Debugger to Continue Button")]
    public static void AddClickDebugger()
    {
        // Find PlantResultPanel
        PlantResultPanel resultPanel = FindObjectOfType<PlantResultPanel>(true);
        if (resultPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "PlantResultPanel not found!", "OK");
            return;
        }

        SerializedObject so = new SerializedObject(resultPanel);
        SerializedProperty continueButtonProp = so.FindProperty("continueButton");

        if (continueButtonProp.objectReferenceValue == null)
        {
            EditorUtility.DisplayDialog("Error", "Continue button is not assigned!", "OK");
            return;
        }

        Button continueButton = continueButtonProp.objectReferenceValue as Button;

        // Check if debugger already exists
        ButtonClickDebugger existingDebugger = continueButton.GetComponent<ButtonClickDebugger>();
        if (existingDebugger != null)
        {
            Debug.Log("ButtonClickDebugger already exists on Continue button");
            EditorUtility.DisplayDialog("Already Added",
                "ButtonClickDebugger is already on the Continue button.\n\n" +
                "Check the Console when you hover/click the button.",
                "OK");
            return;
        }

        // Add debugger
        continueButton.gameObject.AddComponent<ButtonClickDebugger>();
        EditorUtility.SetDirty(continueButton.gameObject);

        Debug.Log("✓ Added ButtonClickDebugger to Continue button");
        EditorUtility.DisplayDialog("Debugger Added!",
            "ButtonClickDebugger has been added to the Continue button.\n\n" +
            "Now when you run the game:\n" +
            "• Hover over the button → see 'MOUSE ENTERED' in Console\n" +
            "• Click the button → see 'BUTTON CLICKED' in Console\n\n" +
            "If you don't see these logs, the button isn't receiving mouse events.",
            "OK");
    }
}
