using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// One-click automatic fixer for Guide Book - runs on scene load
/// </summary>
[InitializeOnLoad]
public class AutoGuideBookFixer
{
    static AutoGuideBookFixer()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        // Run fix when entering edit mode (after stopping play)
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            AutoFixGuideBook();
        }
    }

    [MenuItem("Tools/Sketch Blossom/Auto-Fix Guide Book NOW", priority = 1)]
    public static void AutoFixGuideBook()
    {
        Debug.Log("=== AUTO-FIXING GUIDE BOOK ===");

        // Find the manager
        GameObject manager = GameObject.Find("GuideBookManager");

        if (manager == null)
        {
            Debug.LogWarning("GuideBookManager not found. Searching all objects...");

            // Search for it by component instead
            PlantGuideBook[] guides = Object.FindObjectsOfType<PlantGuideBook>();
            if (guides.Length > 0)
            {
                manager = guides[0].gameObject;
                Debug.Log($"Found PlantGuideBook component on: {manager.name}");
            }
            else
            {
                Debug.LogError("❌ No PlantGuideBook component found in scene!");
                Debug.LogError("Run: Tools > Sketch Blossom > Setup Drawing Scene UI first!");
                EditorUtility.DisplayDialog("Error", "GuideBookManager not found!\n\nRun the setup script first:\nTools > Sketch Blossom > Setup Drawing Scene UI", "OK");
                return;
            }
        }

        PlantGuideBook guide = manager.GetComponent<PlantGuideBook>();
        if (guide == null)
        {
            Debug.LogError("PlantGuideBook component not found!");
            return;
        }

        int fixCount = 0;

        // Fix 1: Find and assign bookPanel
        if (guide.bookPanel == null)
        {
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name == "GuideBookPanel")
                {
                    guide.bookPanel = obj;
                    Debug.Log($"✅ Fixed: Assigned bookPanel = {obj.name}");
                    fixCount++;
                    break;
                }
            }
        }

        // Fix 2: Find and assign openBookButton
        if (guide.openBookButton == null)
        {
            Button[] allButtons = Object.FindObjectsOfType<Button>();
            foreach (var btn in allButtons)
            {
                if (btn.gameObject.name == "GuideBookButton")
                {
                    guide.openBookButton = btn;
                    Debug.Log($"✅ Fixed: Assigned openBookButton = {btn.gameObject.name}");
                    fixCount++;
                    break;
                }
            }
        }

        // Fix 3: Find and assign closeButton
        if (guide.closeBookButton == null)
        {
            Button[] allButtons = Object.FindObjectsOfType<Button>();
            foreach (var btn in allButtons)
            {
                if (btn.gameObject.name == "CloseButton" && btn.transform.parent != null && btn.transform.parent.name.Contains("GuideBook"))
                {
                    guide.closeBookButton = btn;
                    Debug.Log($"✅ Fixed: Assigned closeBookButton");
                    fixCount++;
                    break;
                }
            }
        }

        // Fix 4: Navigation buttons
        Button[] buttons = Object.FindObjectsOfType<Button>();
        foreach (var btn in buttons)
        {
            if (btn.gameObject.name == "NextButton" && guide.nextPageButton == null)
            {
                guide.nextPageButton = btn;
                Debug.Log($"✅ Fixed: Assigned nextPageButton");
                fixCount++;
            }
            else if (btn.gameObject.name == "PreviousButton" && guide.previousPageButton == null)
            {
                guide.previousPageButton = btn;
                Debug.Log($"✅ Fixed: Assigned previousPageButton");
                fixCount++;
            }
        }

        // Fix 5: Text elements
        TextMeshProUGUI[] allText = Object.FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allText)
        {
            if (text.gameObject.name == "PageTitle" && guide.pageTitle == null)
            {
                guide.pageTitle = text;
                fixCount++;
            }
            else if (text.gameObject.name == "PageDescription" && guide.pageDescription == null)
            {
                guide.pageDescription = text;
                fixCount++;
            }
            else if (text.gameObject.name == "PageNumberText" && guide.pageNumberText == null)
            {
                guide.pageNumberText = text;
                fixCount++;
            }
        }

        // Fix 6: Ensure button is visible and active
        if (guide.openBookButton != null)
        {
            guide.openBookButton.gameObject.SetActive(true);
            guide.openBookButton.interactable = true;
            Debug.Log("✅ Fixed: Button is now active and interactable");
            fixCount++;
        }

        // Fix 7: Position bookPanel correctly if it exists
        if (guide.bookPanel != null)
        {
            RectTransform rect = guide.bookPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.4f, 0.1f);
                rect.anchorMax = new Vector2(0.95f, 0.9f);
                rect.anchoredPosition = Vector2.zero;
                Debug.Log("✅ Fixed: Panel positioned correctly");
                fixCount++;
            }

            // Start with it hidden
            guide.bookPanel.SetActive(false);
        }

        // Fix 8: Ensure EventSystem exists
        UnityEngine.EventSystems.EventSystem eventSystem = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✅ Fixed: Created EventSystem");
            fixCount++;
        }

        // Mark the object as dirty so Unity saves the changes
        EditorUtility.SetDirty(guide);
        EditorUtility.SetDirty(manager);

        Debug.Log($"=== FIXED {fixCount} ISSUES ===");

        // Show results
        string message = $"Fixed {fixCount} issues!\n\n";
        message += $"bookPanel: {(guide.bookPanel != null ? "✅" : "❌")}\n";
        message += $"openBookButton: {(guide.openBookButton != null ? "✅" : "❌")}\n";
        message += $"closeBookButton: {(guide.closeBookButton != null ? "✅" : "❌")}\n";
        message += $"nextPageButton: {(guide.nextPageButton != null ? "✅" : "❌")}\n";
        message += $"previousPageButton: {(guide.previousPageButton != null ? "✅" : "❌")}\n\n";

        if (guide.bookPanel != null && guide.openBookButton != null)
        {
            message += "✅ Ready to test!\n\nPress Play and press H key to open guide.";
        }
        else
        {
            message += "⚠️ Some references still missing.\nRun setup script first:\nTools > Setup Drawing Scene UI";
        }

        EditorUtility.DisplayDialog("Auto-Fix Complete", message, "OK");
    }

    [MenuItem("Tools/Sketch Blossom/Force Assign References", priority = 2)]
    public static void ForceAssignReferences()
    {
        // More aggressive version - searches deeper
        Debug.Log("=== FORCE ASSIGNING REFERENCES ===");

        PlantGuideBook guide = Object.FindObjectOfType<PlantGuideBook>();
        if (guide == null)
        {
            EditorUtility.DisplayDialog("Error", "No PlantGuideBook found in scene!", "OK");
            return;
        }

        // Search ALL GameObjects in scene
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>(true); // Include inactive

        foreach (var obj in allObjects)
        {
            // Panel
            if (obj.name.Contains("GuideBook") && obj.name.Contains("Panel") && guide.bookPanel == null)
            {
                guide.bookPanel = obj;
                Debug.Log($"Assigned bookPanel: {obj.name}");
            }

            // Look for buttons
            Button btn = obj.GetComponent<Button>();
            if (btn != null)
            {
                if (obj.name.Contains("Guide") && obj.name.Contains("Button") && guide.openBookButton == null)
                {
                    guide.openBookButton = btn;
                    Debug.Log($"Assigned openBookButton: {obj.name}");
                }
                else if (obj.name == "CloseButton" && guide.closeBookButton == null)
                {
                    guide.closeBookButton = btn;
                    Debug.Log($"Assigned closeBookButton: {obj.name}");
                }
                else if (obj.name == "NextButton" && guide.nextPageButton == null)
                {
                    guide.nextPageButton = btn;
                    Debug.Log($"Assigned nextPageButton");
                }
                else if (obj.name == "PreviousButton" && guide.previousPageButton == null)
                {
                    guide.previousPageButton = btn;
                    Debug.Log($"Assigned previousPageButton");
                }
            }

            // Look for text
            TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                if (obj.name == "PageTitle" && guide.pageTitle == null)
                {
                    guide.pageTitle = text;
                    Debug.Log($"Assigned pageTitle");
                }
                else if (obj.name == "PageDescription" && guide.pageDescription == null)
                {
                    guide.pageDescription = text;
                    Debug.Log($"Assigned pageDescription");
                }
                else if (obj.name.Contains("PageNumber") && guide.pageNumberText == null)
                {
                    guide.pageNumberText = text;
                    Debug.Log($"Assigned pageNumberText");
                }
            }
        }

        EditorUtility.SetDirty(guide);

        string result = "Force assign complete!\n\n";
        result += $"Panel: {(guide.bookPanel != null ? "✅" : "❌")}\n";
        result += $"Open Button: {(guide.openBookButton != null ? "✅" : "❌")}\n";
        result += $"Close Button: {(guide.closeBookButton != null ? "✅" : "❌")}\n";

        EditorUtility.DisplayDialog("Force Assign Complete", result, "OK");

        Debug.Log("=== FORCE ASSIGN COMPLETE ===");
    }
}
