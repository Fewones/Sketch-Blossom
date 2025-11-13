using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Diagnostic tool to check why PlantResultPanel isn't showing
/// </summary>
public class DiagnoseResultPanel : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Diagnose Result Panel Issue")]
    public static void DiagnoseSetup()
    {
        Debug.Log("========== DIAGNOSIS START ==========");

        // Check for DrawingManager
        DrawingManager manager = FindObjectOfType<DrawingManager>();
        if (manager == null)
        {
            Debug.LogError("❌ DrawingManager NOT FOUND in scene!");
            EditorUtility.DisplayDialog("Error", "DrawingManager not found!\n\nPlease add a DrawingManager component to your scene.", "OK");
            return;
        }
        else
        {
            Debug.Log("✓ DrawingManager found: " + manager.gameObject.name);
        }

        // Check DrawingCanvas reference
        if (manager.drawingCanvas == null)
        {
            Debug.LogError("❌ DrawingManager.drawingCanvas is NULL!");
        }
        else
        {
            Debug.Log("✓ DrawingManager.drawingCanvas assigned: " + manager.drawingCanvas.gameObject.name);

            // Check finish button
            if (manager.drawingCanvas.finishButton == null)
            {
                Debug.LogError("❌ DrawingCanvas.finishButton is NULL!");

                // Try to find it
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    Debug.Log("Searching for FinishButton in Canvas hierarchy...");
                    Transform drawingPanel = canvas.transform.Find("DrawingPanel");
                    if (drawingPanel != null)
                    {
                        Debug.Log("✓ Found DrawingPanel");
                        Transform finishBtn = drawingPanel.Find("FinishButton");
                        if (finishBtn != null)
                        {
                            Debug.Log("✓ Found FinishButton at: Canvas/DrawingPanel/FinishButton");
                            Button btn = finishBtn.GetComponent<Button>();
                            if (btn != null)
                            {
                                Debug.Log("✓ FinishButton has Button component, assigning to DrawingCanvas...");
                                manager.drawingCanvas.finishButton = btn;
                                EditorUtility.SetDirty(manager.drawingCanvas);
                                Debug.Log("✓ FinishButton assigned to DrawingCanvas!");
                            }
                            else
                            {
                                Debug.LogError("❌ FinishButton doesn't have a Button component!");
                            }
                        }
                        else
                        {
                            Debug.LogError("❌ FinishButton not found in DrawingPanel!");
                            Debug.LogError("Check hierarchy: Canvas > DrawingPanel > FinishButton");
                        }
                    }
                    else
                    {
                        Debug.LogError("❌ DrawingPanel not found in Canvas!");
                    }
                }
            }
            else
            {
                Debug.Log("✓ Finish button found: " + manager.drawingCanvas.finishButton.gameObject.name);

                // Check button listeners
                int listenerCount = manager.drawingCanvas.finishButton.onClick.GetPersistentEventCount();
                Debug.Log($"Finish button has {listenerCount} persistent listeners");
            }
        }

        // Check PlantAnalyzer reference
        if (manager.plantAnalyzer == null)
        {
            Debug.LogWarning("⚠️ DrawingManager.plantAnalyzer is NULL!");
        }
        else
        {
            Debug.Log("✓ PlantAnalyzer assigned: " + manager.plantAnalyzer.gameObject.name);
        }

        // Check PlantResultPanel reference
        if (manager.plantResultPanel == null)
        {
            Debug.LogError("❌ DrawingManager.plantResultPanel is NULL!");
            Debug.Log("Attempting to find PlantResultPanel in scene...");

            PlantResultPanel panel = FindObjectOfType<PlantResultPanel>(true); // Include inactive
            if (panel != null)
            {
                Debug.Log("✓ Found PlantResultPanel on: " + panel.gameObject.name);
                Debug.Log("  Linking it to DrawingManager...");
                manager.plantResultPanel = panel;
                EditorUtility.SetDirty(manager);
                Debug.Log("✓ PlantResultPanel linked to DrawingManager!");
            }
            else
            {
                Debug.LogError("❌ PlantResultPanel component not found anywhere in scene!");
                EditorUtility.DisplayDialog("Error",
                    "PlantResultPanel not found!\n\n" +
                    "Please run: Tools > Sketch Blossom > Setup Plant Result Panel (Complete)",
                    "OK");
                return;
            }
        }
        else
        {
            Debug.Log("✓ PlantResultPanel assigned: " + manager.plantResultPanel.gameObject.name);
        }

        // Check PlantResultPanel structure
        PlantResultPanel resultPanel = manager.plantResultPanel;
        if (resultPanel != null)
        {
            Debug.Log("\n--- PlantResultPanel Structure Check ---");

            if (resultPanel.panelOverlay == null)
                Debug.LogError("❌ panelOverlay is NULL!");
            else
            {
                Debug.Log("✓ panelOverlay: " + resultPanel.panelOverlay.name);
                Debug.Log("  Active: " + resultPanel.panelOverlay.activeSelf);
                Debug.Log("  ActiveInHierarchy: " + resultPanel.panelOverlay.activeInHierarchy);
            }

            if (resultPanel.panelWindow == null)
                Debug.LogError("❌ panelWindow is NULL!");
            else
                Debug.Log("✓ panelWindow: " + resultPanel.panelWindow.name);

            if (resultPanel.titleText == null)
                Debug.LogError("❌ titleText is NULL!");
            else
                Debug.Log("✓ titleText assigned");

            if (resultPanel.plantNameText == null)
                Debug.LogError("❌ plantNameText is NULL!");
            else
                Debug.Log("✓ plantNameText assigned");

            if (resultPanel.continueButton == null)
                Debug.LogError("❌ continueButton is NULL!");
            else
                Debug.Log("✓ continueButton assigned");

            if (resultPanel.redrawButton == null)
                Debug.LogError("❌ redrawButton is NULL!");
            else
                Debug.Log("✓ redrawButton assigned");
        }

        Debug.Log("\n========== DIAGNOSIS COMPLETE ==========");

        // Summary dialog
        if (manager.plantResultPanel != null && manager.plantResultPanel.panelOverlay != null)
        {
            EditorUtility.DisplayDialog("Diagnosis Complete",
                "All components look good!\n\n" +
                "If the panel still doesn't show:\n" +
                "1. Enter Play Mode\n" +
                "2. Draw something\n" +
                "3. Click Finish Drawing\n" +
                "4. Check Console for detailed logs",
                "OK");
        }
    }
}
