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

        [UnityEditor.MenuItem("Tools/Create Guide Book Pages")]
        public static void CreateGuideBookPages()
        {
            Debug.Log("=== CREATING GUIDE BOOK PAGES ===");

            // Find guide panel
            GameObject guidePanel = GameObject.Find("GuideBookPanel");
            if (guidePanel == null)
            {
                Debug.LogError("❌ GuideBookPanel not found!");
                Debug.LogError("   Please create GuideBookPanel first.");
                return;
            }

            // Find or create ContentPanel
            Transform contentPanel = guidePanel.transform.Find("ContentPanel");
            if (contentPanel == null)
            {
                Debug.LogWarning("⚠ ContentPanel not found - creating pages directly in GuideBookPanel");
                contentPanel = guidePanel.transform;
            }
            else
            {
                Debug.Log("✅ Found ContentPanel");
            }

            // Delete existing pages
            for (int i = contentPanel.childCount - 1; i >= 0; i--)
            {
                Transform child = contentPanel.GetChild(i);
                if (child.name.StartsWith("Page"))
                {
                    Debug.Log($"Deleting existing page: {child.name}");
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }

            // Create all 4 pages
            CreateGuidePage0_Introduction(contentPanel);
            CreateGuidePage1_FireMoves(contentPanel);
            CreateGuidePage2_GrassMoves(contentPanel);
            CreateGuidePage3_WaterMoves(contentPanel);

            Debug.Log("✅ GUIDE BOOK PAGES CREATED!");
            Debug.Log("Run 'Tools > Fix Guide Book Pages' to wire them up to the GuideBookManager");
        }

        private static GameObject CreateTextElement(string name, Transform parent, string text, int fontSize)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TMPro.TextMeshProUGUI tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;

            return textObj;
        }

        private static void CreateGuidePage0_Introduction(Transform parent)
        {
            GameObject page = new GameObject("Page0_Introduction");
            page.transform.SetParent(parent);
            RectTransform pageRT = page.AddComponent<RectTransform>();
            pageRT.anchorMin = new Vector2(0, 0.15f);
            pageRT.anchorMax = new Vector2(1, 0.85f);
            pageRT.offsetMin = Vector2.zero;
            pageRT.offsetMax = Vector2.zero;

            // Intro text
            string introText = "Welcome to the Drawing Guide!\n\n" +
                "Draw patterns to execute battle moves.\n\n" +
                "The quality of your drawing affects damage:\n" +
                "• Perfect drawing = 1.5x damage\n" +
                "• Good drawing = 1.0x damage\n" +
                "• Poor drawing = 0.5x damage\n\n" +
                "Use Next to see move guides organized by element type.";

            GameObject textObj = CreateTextElement("IntroText", page.transform, introText, 18);
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(50, 0);
            textRT.offsetMax = new Vector2(-50, 0);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Center;
            textObj.GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.2f, 0.1f, 0.05f);

            Debug.Log("✅ Created Page 0: Introduction");
        }

        private static void CreateGuidePage1_FireMoves(Transform parent)
        {
            GameObject page = new GameObject("Page1_FireMoves");
            page.transform.SetParent(parent);
            RectTransform pageRT = page.AddComponent<RectTransform>();
            pageRT.anchorMin = new Vector2(0, 0.15f);
            pageRT.anchorMax = new Vector2(1, 0.85f);
            pageRT.offsetMin = Vector2.zero;
            pageRT.offsetMax = Vector2.zero;

            // Page title
            GameObject titleObj = CreateTextElement("PageTitle", page.transform, "🔥 Fire Moves", 24);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.9f);
            titleRT.anchorMax = new Vector2(0.5f, 0.9f);
            titleRT.sizeDelta = new Vector2(400, 40);
            titleObj.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Center;
            titleObj.GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;
            titleObj.GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.8f, 0.2f, 0.0f);

            // Fire moves
            CreateMoveGuideEntry(page.transform, "Block", "Draw any simple shape\n(Square, Circle, etc.)", 0.5f, 0.7f);
            CreateMoveGuideEntry(page.transform, "Fireball", "Draw a circle or spiral\nClosed loop shape", 0.5f, 0.5f);
            CreateMoveGuideEntry(page.transform, "Burn", "Draw zigzag or spiky lines\nSharp angles", 0.5f, 0.3f);
            CreateMoveGuideEntry(page.transform, "Flame Wave", "Draw wavy horizontal lines\nSmooth curves", 0.5f, 0.1f);

            Debug.Log("✅ Created Page 1: Fire Moves");
        }

        private static void CreateGuidePage2_GrassMoves(Transform parent)
        {
            GameObject page = new GameObject("Page2_GrassMoves");
            page.transform.SetParent(parent);
            RectTransform pageRT = page.AddComponent<RectTransform>();
            pageRT.anchorMin = new Vector2(0, 0.15f);
            pageRT.anchorMax = new Vector2(1, 0.85f);
            pageRT.offsetMin = Vector2.zero;
            pageRT.offsetMax = Vector2.zero;

            // Page title
            GameObject titleObj = CreateTextElement("PageTitle", page.transform, "🌿 Grass Moves", 24);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.9f);
            titleRT.anchorMax = new Vector2(0.5f, 0.9f);
            titleRT.sizeDelta = new Vector2(400, 40);
            titleObj.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Center;
            titleObj.GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;
            titleObj.GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.2f, 0.7f, 0.1f);

            // Grass moves
            CreateMoveGuideEntry(page.transform, "Block", "Draw any simple shape\n(Square, Circle, etc.)", 0.5f, 0.7f);
            CreateMoveGuideEntry(page.transform, "Vine Whip", "Draw a curved S-shape\nSmooth flowing line", 0.5f, 0.5f);
            CreateMoveGuideEntry(page.transform, "Leaf Storm", "Draw many small scattered marks\n5+ short strokes", 0.5f, 0.3f);
            CreateMoveGuideEntry(page.transform, "Root Attack", "Draw vertical lines downward\nTall aspect ratio", 0.5f, 0.1f);

            Debug.Log("✅ Created Page 2: Grass Moves");
        }

        private static void CreateGuidePage3_WaterMoves(Transform parent)
        {
            GameObject page = new GameObject("Page3_WaterMoves");
            page.transform.SetParent(parent);
            RectTransform pageRT = page.AddComponent<RectTransform>();
            pageRT.anchorMin = new Vector2(0, 0.15f);
            pageRT.anchorMax = new Vector2(1, 0.85f);
            pageRT.offsetMin = Vector2.zero;
            pageRT.offsetMax = Vector2.zero;

            // Page title
            GameObject titleObj = CreateTextElement("PageTitle", page.transform, "💧 Water Moves", 24);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.9f);
            titleRT.anchorMax = new Vector2(0.5f, 0.9f);
            titleRT.sizeDelta = new Vector2(400, 40);
            titleObj.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Center;
            titleObj.GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;
            titleObj.GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.1f, 0.5f, 0.9f);

            // Water moves
            CreateMoveGuideEntry(page.transform, "Block", "Draw any simple shape\n(Square, Circle, etc.)", 0.5f, 0.7f);
            CreateMoveGuideEntry(page.transform, "Water Splash", "Draw wavy horizontal lines\n2-5 curved strokes", 0.5f, 0.5f);
            CreateMoveGuideEntry(page.transform, "Bubble", "Draw small circles (2-3)\nCompact circular shapes", 0.5f, 0.3f);
            CreateMoveGuideEntry(page.transform, "Healing Wave", "Draw smooth horizontal wave\nGentle flowing curve", 0.5f, 0.1f);

            Debug.Log("✅ Created Page 3: Water Moves");
        }

        private static void CreateMoveGuideEntry(Transform parent, string moveName, string description, float xPos, float yPos)
        {
            GameObject entry = new GameObject($"Guide_{moveName}");
            entry.transform.SetParent(parent);
            RectTransform entryRT = entry.AddComponent<RectTransform>();
            entryRT.anchorMin = new Vector2(xPos, yPos);
            entryRT.anchorMax = new Vector2(xPos, yPos);
            entryRT.sizeDelta = new Vector2(600, 80);

            // Background
            UnityEngine.UI.Image entryBg = entry.AddComponent<UnityEngine.UI.Image>();
            entryBg.color = new Color(0.85f, 0.8f, 0.65f, 0.3f);

            // Move name (bold)
            GameObject nameObj = CreateTextElement("Name", entry.transform, moveName, 18);
            RectTransform nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(0.4f, 1f);
            nameRT.offsetMin = new Vector2(10, 0);
            nameRT.offsetMax = new Vector2(0, -5);
            nameObj.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            nameObj.GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;
            nameObj.GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.2f, 0.1f, 0.05f);

            // Description
            GameObject descObj = CreateTextElement("Description", entry.transform, description, 14);
            RectTransform descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0.4f, 0f);
            descRT.anchorMax = new Vector2(1, 1f);
            descRT.offsetMin = new Vector2(10, 5);
            descRT.offsetMax = new Vector2(-10, -5);
            descObj.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            descObj.GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.3f, 0.2f, 0.1f);
        }
    }
}
