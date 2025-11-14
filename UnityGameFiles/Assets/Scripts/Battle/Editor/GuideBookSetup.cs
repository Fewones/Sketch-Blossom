using UnityEngine;
using UnityEngine.UI;

namespace SketchBlossom.Battle.Editor
{
    /// <summary>
    /// Ensures GuideBookManager exists and is properly configured in the scene.
    /// Run this from the Unity menu: Tools > Setup Guide Book Manager
    /// </summary>
    public static class GuideBookSetup
    {
        [UnityEditor.MenuItem("Tools/Setup Guide Book Manager")]
        public static void SetupGuideBookManager()
        {
            Debug.Log("=== SETTING UP GUIDE BOOK MANAGER ===");

            // Find or create GuideBookManager
            GuideBookManager manager = UnityEngine.Object.FindObjectOfType<GuideBookManager>();

            if (manager == null)
            {
                // Create new GameObject with GuideBookManager
                GameObject managerObj = new GameObject("GuideBookManager");
                manager = managerObj.AddComponent<GuideBookManager>();
                Debug.Log("✅ Created GuideBookManager GameObject");
            }
            else
            {
                Debug.Log("✅ Found existing GuideBookManager");
            }

            // Find the guide panel
            GameObject guidePanel = GameObject.Find("GuideBookPanel");
            if (guidePanel == null)
            {
                Debug.LogError("❌ GuidePanel not found! Please create the guide panel first.");
                Debug.LogError("   Use the DrawingBattleSceneBuilder to create the scene.");
                return;
            }

            // Force re-wire the manager
            manager.ForceReWire();

            // Select the manager in hierarchy
            UnityEditor.Selection.activeGameObject = manager.gameObject;

            Debug.Log("✅ GUIDE BOOK MANAGER SETUP COMPLETE!");
            Debug.Log("GuideBookManager is now selected. Check the Inspector for debug buttons.");
        }

        [UnityEditor.MenuItem("Tools/Test Guide Book Navigation")]
        public static void TestGuideBookNavigation()
        {
            Debug.Log("=== TESTING GUIDE BOOK NAVIGATION ===");

            GuideBookManager manager = UnityEngine.Object.FindObjectOfType<GuideBookManager>();

            if (manager == null)
            {
                Debug.LogError("❌ GuideBookManager not found! Run 'Tools > Setup Guide Book Manager' first.");
                return;
            }

            // Open the guide
            manager.OpenGuide();
            Debug.Log("✅ Opened guide book");

            // Debug current state
            manager.DebugCurrentState();
        }

        [UnityEditor.MenuItem("Tools/Debug Guide Panel Structure")]
        public static void DebugGuidePanelStructure()
        {
            Debug.Log("=== GUIDE PANEL STRUCTURE DEBUG ===");

            // Try to find guide panel (try both names for compatibility)
            GameObject guidePanel = GameObject.Find("GuideBookPanel");
            if (guidePanel == null)
            {
                guidePanel = GameObject.Find("GuidePanel");
            }

            if (guidePanel == null)
            {
                Debug.LogError("❌ GuidePanel/GuideBookPanel not found in scene!");
                Debug.LogError("   Please create the guide panel using DrawingBattleSceneBuilder");
                return;
            }

            Debug.Log($"✅ Found {guidePanel.name}");
            Debug.Log($"{guidePanel.name} has {guidePanel.transform.childCount} direct children:");

            // List all direct children
            for (int i = 0; i < guidePanel.transform.childCount; i++)
            {
                Transform child = guidePanel.transform.GetChild(i);
                Debug.Log($"  {i}: {child.name}");

                // If this child has children, list them too
                if (child.childCount > 0)
                {
                    Debug.Log($"    └─ {child.name} has {child.childCount} children:");
                    for (int j = 0; j < child.childCount; j++)
                    {
                        Transform grandchild = child.GetChild(j);
                        Debug.Log($"       {j}: {grandchild.name}");
                    }
                }
            }

            Debug.Log("=== END DEBUG ===");
        }

        [UnityEditor.MenuItem("Tools/Fix Guide Book Pages")]
        public static void FixGuideBookPages()
        {
            Debug.Log("=== FIXING GUIDE BOOK PAGES ===");

            // Find guide panel
            GameObject guidePanel = GameObject.Find("GuideBookPanel");
            if (guidePanel == null)
            {
                Debug.LogError("❌ GuideBookPanel not found!");
                Debug.LogError("   Please use 'Tools > Create Full Drawing Battle Scene' or DrawingBattleSceneBuilder to create the scene first.");
                return;
            }

            // Find content panel (it may be the guide panel itself if created manually)
            Transform contentPanel = guidePanel.transform.Find("ContentPanel");
            Transform panelToSearch = contentPanel != null ? contentPanel : guidePanel.transform;

            if (contentPanel == null)
            {
                Debug.LogWarning("⚠ ContentPanel not found - searching for pages directly in GuideBookPanel");
                Debug.LogWarning("   For best results, recreate the scene using DrawingBattleSceneBuilder");
            }
            else
            {
                Debug.Log("✅ Found ContentPanel");
            }

            // List all pages
            Debug.Log($"{panelToSearch.name} has {panelToSearch.childCount} children:");
            for (int i = 0; i < panelToSearch.childCount; i++)
            {
                Transform child = panelToSearch.GetChild(i);
                Debug.Log($"  {i}: {child.name} (starts with Page: {child.name.StartsWith("Page")})");
            }

            // Find GuideBookManager and force setup
            GuideBookManager manager = UnityEngine.Object.FindObjectOfType<GuideBookManager>();
            if (manager != null)
            {
                manager.ForceReWire();
                manager.ManualSetupPages();
                Debug.Log("✅ Pages have been re-initialized");
            }
            else
            {
                Debug.LogError("❌ GuideBookManager not found!");
                Debug.LogError("   Run 'Tools > Setup Guide Book Manager' to create it.");
            }
        }
    }
}
