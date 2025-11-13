using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Debug and fix Guide Book issues
/// </summary>
public class GuideBookDebugger : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Debug Guide Book")]
    public static void ShowWindow()
    {
        GuideBookDebugger window = GetWindow<GuideBookDebugger>("Guide Book Debugger");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Guide Book Debugger", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool helps diagnose and fix Guide Book visibility issues.",
            MessageType.Info);

        GUILayout.Space(20);

        if (GUILayout.Button("Run Full Diagnostic", GUILayout.Height(40)))
        {
            RunDiagnostic();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Fix Common Issues", GUILayout.Height(40)))
        {
            FixCommonIssues();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Force Show Guide Book", GUILayout.Height(40)))
        {
            ForceShowGuideBook();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Find All Guide Book Components", GUILayout.Height(40)))
        {
            FindAllComponents();
        }

        GUILayout.Space(20);

        EditorGUILayout.HelpBox(
            "COMMON ISSUES:\n\n" +
            "1. Button not visible → Check button is active\n" +
            "2. Button doesn't respond → Check event system\n" +
            "3. Panel doesn't appear → Check panel position\n" +
            "4. References null → Run 'Fix Common Issues'",
            MessageType.Warning);
    }

    private static void RunDiagnostic()
    {
        Debug.Log("=== GUIDE BOOK DIAGNOSTIC START ===");

        // Check for GuideBookManager
        GameObject manager = GameObject.Find("GuideBookManager");
        if (manager == null)
        {
            Debug.LogError("❌ GuideBookManager GameObject not found in scene!");
            EditorUtility.DisplayDialog("Diagnostic Failed", "GuideBookManager not found!\nRun the setup script first:\nTools > Sketch Blossom > Setup Drawing Scene UI", "OK");
            return;
        }
        Debug.Log("✅ GuideBookManager found");

        // Check for PlantGuideBook component
        PlantGuideBook guide = manager.GetComponent<PlantGuideBook>();
        if (guide == null)
        {
            Debug.LogError("❌ PlantGuideBook component not found!");
            EditorUtility.DisplayDialog("Diagnostic Failed", "PlantGuideBook component missing!\nAdd the component manually to GuideBookManager.", "OK");
            return;
        }
        Debug.Log("✅ PlantGuideBook component found");

        // Check book panel
        if (guide.bookPanel == null)
        {
            Debug.LogError("❌ bookPanel reference is NULL!");
        }
        else
        {
            Debug.Log($"✅ bookPanel: {guide.bookPanel.name}");
            Debug.Log($"   - Active: {guide.bookPanel.activeSelf}");
            RectTransform rect = guide.bookPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                Debug.Log($"   - Position: {rect.anchoredPosition}");
            }
        }

        // Check open button
        if (guide.openBookButton == null)
        {
            Debug.LogError("❌ openBookButton reference is NULL!");
        }
        else
        {
            Debug.Log($"✅ openBookButton: {guide.openBookButton.gameObject.name}");
            Debug.Log($"   - Active: {guide.openBookButton.gameObject.activeSelf}");
            Debug.Log($"   - Interactable: {guide.openBookButton.interactable}");
            Debug.Log($"   - Listeners: {guide.openBookButton.onClick.GetPersistentEventCount()}");
        }

        // Check close button
        if (guide.closeBookButton == null)
        {
            Debug.LogError("❌ closeBookButton reference is NULL!");
        }
        else
        {
            Debug.Log($"✅ closeBookButton found");
        }

        // Check navigation buttons
        Debug.Log($"nextPageButton: {(guide.nextPageButton != null ? "✅" : "❌")}");
        Debug.Log($"previousPageButton: {(guide.previousPageButton != null ? "✅" : "❌")}");

        // Check text elements
        Debug.Log($"pageTitle: {(guide.pageTitle != null ? "✅" : "❌")}");
        Debug.Log($"pageDescription: {(guide.pageDescription != null ? "✅" : "❌")}");
        Debug.Log($"pageNumberText: {(guide.pageNumberText != null ? "✅" : "❌")}");

        // Check for EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("❌ EventSystem not found! Buttons won't work!");
        }
        else
        {
            Debug.Log("✅ EventSystem found");
        }

        Debug.Log("=== DIAGNOSTIC COMPLETE ===");
        Debug.Log("Check the Console for detailed results!");

        EditorUtility.DisplayDialog(
            "Diagnostic Complete",
            "Check the Console window for detailed results.\n\n" +
            "Look for ❌ symbols to see what's wrong.\n\n" +
            "Then click 'Fix Common Issues' button.",
            "OK");
    }

    private static void FixCommonIssues()
    {
        Debug.Log("=== FIXING COMMON ISSUES ===");

        int fixCount = 0;

        // Find manager
        GameObject manager = GameObject.Find("GuideBookManager");
        if (manager == null)
        {
            Debug.LogError("GuideBookManager not found! Run setup script first.");
            return;
        }

        PlantGuideBook guide = manager.GetComponent<PlantGuideBook>();
        if (guide == null)
        {
            Debug.LogError("PlantGuideBook component not found!");
            return;
        }

        // Fix 1: Ensure bookPanel is assigned and positioned correctly
        if (guide.bookPanel == null)
        {
            guide.bookPanel = GameObject.Find("GuideBookPanel");
            if (guide.bookPanel != null)
            {
                Debug.Log("✅ Fixed: Assigned bookPanel");
                fixCount++;
            }
        }

        if (guide.bookPanel != null)
        {
            // Make sure it has RectTransform
            RectTransform rect = guide.bookPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Position it correctly (right side of screen)
                rect.anchorMin = new Vector2(0.4f, 0.1f);
                rect.anchorMax = new Vector2(0.95f, 0.9f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.anchoredPosition = new Vector2(0, 0);

                Debug.Log("✅ Fixed: Panel position");
                fixCount++;
            }

            // Make sure it starts hidden
            guide.bookPanel.SetActive(false);
        }

        // Fix 2: Find and assign openBookButton
        if (guide.openBookButton == null)
        {
            Button[] buttons = GameObject.FindObjectsOfType<Button>();
            foreach (var btn in buttons)
            {
                if (btn.gameObject.name == "GuideBookButton")
                {
                    guide.openBookButton = btn;
                    Debug.Log("✅ Fixed: Assigned openBookButton");
                    fixCount++;
                    break;
                }
            }
        }

        // Make sure open button is visible and active
        if (guide.openBookButton != null)
        {
            guide.openBookButton.gameObject.SetActive(true);
            guide.openBookButton.interactable = true;

            // Add text if missing
            TextMeshProUGUI btnText = guide.openBookButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null && string.IsNullOrEmpty(btnText.text))
            {
                btnText.text = "📖 Guide";
                Debug.Log("✅ Fixed: Button text");
                fixCount++;
            }
        }

        // Fix 3: Find navigation buttons
        if (guide.nextPageButton == null || guide.previousPageButton == null)
        {
            Button[] buttons = GameObject.FindObjectsOfType<Button>();
            foreach (var btn in buttons)
            {
                if (btn.gameObject.name == "NextButton")
                {
                    guide.nextPageButton = btn;
                    Debug.Log("✅ Fixed: Assigned nextPageButton");
                    fixCount++;
                }
                else if (btn.gameObject.name == "PreviousButton")
                {
                    guide.previousPageButton = btn;
                    Debug.Log("✅ Fixed: Assigned previousPageButton");
                    fixCount++;
                }
            }
        }

        // Fix 4: Find text components
        TextMeshProUGUI[] allText = GameObject.FindObjectsOfType<TextMeshProUGUI>();
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

        // Fix 5: Ensure EventSystem exists
        UnityEngine.EventSystems.EventSystem eventSystem = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✅ Fixed: Created EventSystem");
            fixCount++;
        }

        EditorUtility.SetDirty(guide);

        Debug.Log($"=== FIXED {fixCount} ISSUES ===");

        EditorUtility.DisplayDialog(
            "Fixes Applied",
            $"Fixed {fixCount} issues!\n\n" +
            "Press Play and try opening the guide book.\n\n" +
            "If still not working, check Console for errors.",
            "OK");
    }

    private static void ForceShowGuideBook()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Enter Play Mode",
                "This function only works in Play Mode.\n\nPress Play first, then run this again.",
                "OK");
            return;
        }

        PlantGuideBook guide = GameObject.FindObjectOfType<PlantGuideBook>();
        if (guide == null)
        {
            Debug.LogError("PlantGuideBook not found in scene!");
            return;
        }

        if (guide.bookPanel != null)
        {
            guide.bookPanel.SetActive(true);
            RectTransform rect = guide.bookPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
            }
            Debug.Log("✅ Forced guide book panel to show");
        }

        EditorUtility.DisplayDialog(
            "Guide Book Forced Visible",
            "The guide book panel should now be visible.\n\n" +
            "If you still don't see it, check the hierarchy and make sure GuideBookPanel exists.",
            "OK");
    }

    private static void FindAllComponents()
    {
        Debug.Log("=== SEARCHING FOR GUIDE BOOK COMPONENTS ===");

        // Find all GameObjects with relevant names
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        Debug.Log("\n📦 Panels:");
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("GuideBook") || obj.name.Contains("Panel"))
            {
                Debug.Log($"  - {GetGameObjectPath(obj)}");
            }
        }

        Debug.Log("\n🔘 Buttons:");
        Button[] buttons = GameObject.FindObjectsOfType<Button>();
        foreach (var btn in buttons)
        {
            Debug.Log($"  - {GetGameObjectPath(btn.gameObject)} (Interactable: {btn.interactable})");
        }

        Debug.Log("\n📝 TextMeshPro Elements:");
        TextMeshProUGUI[] texts = GameObject.FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            Debug.Log($"  - {GetGameObjectPath(text.gameObject)}");
        }

        Debug.Log("\n=== SEARCH COMPLETE ===");

        EditorUtility.DisplayDialog(
            "Component Search Complete",
            "Check the Console for a list of all found components.\n\n" +
            "This helps you see what's actually in the scene.",
            "OK");
    }

    private static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
