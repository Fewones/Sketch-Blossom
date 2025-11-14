using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

namespace SketchBlossom.Battle
{
    [CustomEditor(typeof(GuideBookManager))]
    public class GuideBookDebugger : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GuideBookManager manager = (GuideBookManager)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Force Re-wire All References"))
            {
                manager.ForceReWire();
                EditorUtility.SetDirty(manager);
            }

            if (GUILayout.Button("Debug Guide Book Setup"))
            {
                DebugGuideBookSetup(manager);
            }

            if (GUILayout.Button("Test Open Guide"))
            {
                manager.OpenGuide();
            }

            if (GUILayout.Button("Test Next Page"))
            {
                manager.NextPage();
            }

            if (GUILayout.Button("Test Previous Page"))
            {
                manager.PreviousPage();
            }

            if (GUILayout.Button("Manually Fix Page Navigation"))
            {
                ManuallyFixPageNavigation(manager);
            }
        }

        private void DebugGuideBookSetup(GuideBookManager manager)
        {
            Debug.Log("=== GUIDE BOOK DEBUG INFO ===");

            // Check for guide panel
            GameObject guidePanel = GameObject.Find("GuidePanel");
            Debug.Log($"GuidePanel found: {guidePanel != null}");

            if (guidePanel != null)
            {
                // Check for buttons
                Button[] buttons = guidePanel.GetComponentsInChildren<Button>(true);
                Debug.Log($"Total buttons found: {buttons.Length}");
                foreach (Button btn in buttons)
                {
                    Debug.Log($"  - Button: {btn.name}");
                }

                // Check for pages
                Transform contentPanel = guidePanel.transform.Find("ContentPanel");
                if (contentPanel != null)
                {
                    Debug.Log($"ContentPanel found with {contentPanel.childCount} children");
                    List<GameObject> pages = new List<GameObject>();
                    for (int i = 0; i < contentPanel.childCount; i++)
                    {
                        Transform child = contentPanel.GetChild(i);
                        Debug.Log($"  - Child {i}: {child.name} (starts with Page: {child.name.StartsWith("Page")})");
                        if (child.name.StartsWith("Page"))
                        {
                            pages.Add(child.gameObject);
                        }
                    }
                    Debug.Log($"Found {pages.Count} page objects");
                }
                else
                {
                    Debug.LogError("ContentPanel not found!");
                }

                // Check for page indicator
                TextMeshProUGUI[] texts = guidePanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                Debug.Log($"Total TextMeshProUGUI components: {texts.Length}");
                foreach (TextMeshProUGUI text in texts)
                {
                    if (text.name == "PageIndicator")
                    {
                        Debug.Log($"  - PageIndicator found: {text.text}");
                    }
                }
            }

            Debug.Log("=== END DEBUG INFO ===");
        }

        private void ManuallyFixPageNavigation(GuideBookManager manager)
        {
            Debug.Log("=== MANUALLY FIXING PAGE NAVIGATION ===");

            GameObject guidePanel = GameObject.Find("GuidePanel");
            if (guidePanel == null)
            {
                Debug.LogError("GuidePanel not found!");
                return;
            }

            // Find and assign buttons using reflection
            var managerType = typeof(GuideBookManager);

            // Find buttons
            Button[] buttons = guidePanel.GetComponentsInChildren<Button>(true);
            Button prevButton = null;
            Button nextButton = null;
            Button closeButton = null;

            foreach (Button btn in buttons)
            {
                if (btn.name == "PreviousPageButton")
                {
                    prevButton = btn;
                    Debug.Log("✅ Found PreviousPageButton");
                }
                else if (btn.name == "NextPageButton")
                {
                    nextButton = btn;
                    Debug.Log("✅ Found NextPageButton");
                }
                else if (btn.name == "CloseButton")
                {
                    closeButton = btn;
                    Debug.Log("✅ Found CloseButton");
                }
            }

            // Find pages
            Transform contentPanel = guidePanel.transform.Find("ContentPanel");
            List<GameObject> pages = new List<GameObject>();
            if (contentPanel != null)
            {
                for (int i = 0; i < contentPanel.childCount; i++)
                {
                    Transform child = contentPanel.GetChild(i);
                    if (child.name.StartsWith("Page"))
                    {
                        pages.Add(child.gameObject);
                    }
                }
                Debug.Log($"✅ Found {pages.Count} pages");
            }

            // Find page indicator
            TextMeshProUGUI pageIndicator = null;
            TextMeshProUGUI[] texts = guidePanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name == "PageIndicator")
                {
                    pageIndicator = text;
                    Debug.Log("✅ Found PageIndicator");
                    break;
                }
            }

            // Use reflection to set private fields
            SetPrivateField(manager, "previousPageButton", prevButton);
            SetPrivateField(manager, "nextPageButton", nextButton);
            SetPrivateField(manager, "closeGuideButton", closeButton);
            SetPrivateField(manager, "pageIndicatorText", pageIndicator);
            SetPrivateField(manager, "guidePanel", guidePanel);
            SetPrivateField(manager, "pages", pages.ToArray());
            SetPrivateField(manager, "currentPageIndex", 0);

            // Setup listeners manually
            if (prevButton != null)
            {
                prevButton.onClick.RemoveAllListeners();
                prevButton.onClick.AddListener(() => manager.PreviousPage());
                Debug.Log("✅ Setup PreviousPage listener");
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(() => manager.NextPage());
                Debug.Log("✅ Setup NextPage listener");
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => manager.CloseGuide());
                Debug.Log("✅ Setup Close listener");
            }

            // Initialize page display
            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].SetActive(i == 0);
            }

            if (pageIndicator != null && pages.Count > 0)
            {
                pageIndicator.text = $"Page 1 / {pages.Count}";
            }

            EditorUtility.SetDirty(manager);
            Debug.Log("✅ PAGE NAVIGATION MANUALLY FIXED!");
            Debug.Log("Pages should now work correctly. Test with 'Test Next Page' and 'Test Previous Page' buttons.");
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
                Debug.Log($"  ✅ Set {fieldName} = {value != null} (type: {value?.GetType().Name})");
            }
            else
            {
                Debug.LogWarning($"  ⚠ Field '{fieldName}' not found");
            }
        }
    }
}
