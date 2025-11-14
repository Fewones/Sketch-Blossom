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
            GameObject guidePanel = GameObject.Find("GuidePanel");
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

        [UnityEditor.MenuItem("Tools/Fix Guide Book Pages")]
        public static void FixGuideBookPages()
        {
            Debug.Log("=== FIXING GUIDE BOOK PAGES ===");

            // Find guide panel
            GameObject guidePanel = GameObject.Find("GuidePanel");
            if (guidePanel == null)
            {
                Debug.LogError("❌ GuidePanel not found!");
                return;
            }

            // Find content panel
            Transform contentPanel = guidePanel.transform.Find("ContentPanel");
            if (contentPanel == null)
            {
                Debug.LogError("❌ ContentPanel not found!");
                return;
            }

            // List all pages
            Debug.Log($"ContentPanel has {contentPanel.childCount} children:");
            for (int i = 0; i < contentPanel.childCount; i++)
            {
                Transform child = contentPanel.GetChild(i);
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
            }
        }
    }
}
