using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Cleans up the BattleScene by removing outdated and conflicting components
/// Run this FIRST before rebuilding the drawing system
/// </summary>
public class CleanupBattleScene : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Battle Scene/1. Cleanup Old Systems")]
    public static void CleanupOldSystems()
    {
        if (!EditorUtility.DisplayDialog("Cleanup Battle Scene",
            "This will remove all old drawing-related components from the BattleScene.\n\n" +
            "This includes:\n" +
            "• Old DrawingCanvas components\n" +
            "• Legacy drawing panels and UI\n" +
            "• Conflicting managers\n" +
            "• Duplicate components\n\n" +
            "This operation can be undone with Ctrl+Z.\n\n" +
            "Continue?",
            "Yes, Clean Up", "Cancel"))
        {
            return;
        }

        Debug.Log("========== CLEANING UP BATTLE SCENE ==========");
        int itemsRemoved = 0;

        // Find the Canvas once
        Canvas canvas = FindObjectOfType<Canvas>();

        // Find and remove old DrawingCanvas components
        DrawingCanvas[] oldCanvases = FindObjectsOfType<DrawingCanvas>();
        foreach (var oldCanvas in oldCanvases)
        {
            Debug.Log($"Removing old DrawingCanvas: {oldCanvas.gameObject.name}");
            Undo.DestroyObjectImmediate(oldCanvas);
            itemsRemoved++;
        }

        // Find and remove DrawingManager (should only be in DrawingScene)
        DrawingManager[] drawingManagers = FindObjectsOfType<DrawingManager>();
        foreach (var manager in drawingManagers)
        {
            Debug.Log($"Removing DrawingManager: {manager.gameObject.name} (should only be in DrawingScene)");
            Undo.DestroyObjectImmediate(manager.gameObject);
            itemsRemoved++;
        }

        // Find and remove old drawing panels (not BattleDrawingPanel)
        if (canvas != null)
        {
            List<GameObject> toRemove = new List<GameObject>();

            // Look for old panel names
            string[] oldPanelNames = new string[]
            {
                "DrawingPanel",
                "DrawingOverlay",
                "DrawingUI",
                "ResultPanel",
                "ResultOverlay",
                "PlantResultPanel"
            };

            foreach (string panelName in oldPanelNames)
            {
                Transform panelTransform = canvas.transform.Find(panelName);
                if (panelTransform != null)
                {
                    // Don't remove BattleDrawingPanel
                    if (!panelTransform.gameObject.name.Contains("Battle"))
                    {
                        toRemove.Add(panelTransform.gameObject);
                    }
                }
            }

            // Remove old panels
            foreach (var obj in toRemove)
            {
                Debug.Log($"Removing old UI panel: {obj.name}");
                Undo.DestroyObjectImmediate(obj);
                itemsRemoved++;
            }
        }

        // Find and remove PlantResultPanel component (should only be in DrawingScene)
        PlantResultPanel[] resultPanels = FindObjectsOfType<PlantResultPanel>(true);
        foreach (var panel in resultPanels)
        {
            Debug.Log($"Removing PlantResultPanel: {panel.gameObject.name} (should only be in DrawingScene)");
            Undo.DestroyObjectImmediate(panel.gameObject);
            itemsRemoved++;
        }

        // Clean up old SimpleDrawingCanvas if it has conflicting references
        SimpleDrawingCanvas[] simpleCanvases = FindObjectsOfType<SimpleDrawingCanvas>();
        if (simpleCanvases.Length > 1)
        {
            Debug.LogWarning($"Found {simpleCanvases.Length} SimpleDrawingCanvas components! Removing extras...");
            for (int i = 1; i < simpleCanvases.Length; i++)
            {
                Debug.Log($"Removing duplicate SimpleDrawingCanvas: {simpleCanvases[i].gameObject.name}");
                Undo.DestroyObjectImmediate(simpleCanvases[i].gameObject);
                itemsRemoved++;
            }
        }

        // Clean up old BattleDrawingManager if it exists (will be recreated)
        BattleDrawingManager[] battleManagers = FindObjectsOfType<BattleDrawingManager>();
        if (battleManagers.Length > 0)
        {
            Debug.Log("Removing existing BattleDrawingManager (will be recreated)");
            foreach (var manager in battleManagers)
            {
                Undo.DestroyObjectImmediate(manager.gameObject);
                itemsRemoved++;
            }
        }

        // Remove old BattleDrawingPanel if it exists (will be recreated properly)
        if (canvas != null)
        {
            Transform oldBattlePanel = canvas.transform.Find("BattleDrawingPanel");
            if (oldBattlePanel != null)
            {
                Debug.Log("Removing old BattleDrawingPanel (will be recreated)");
                Undo.DestroyObjectImmediate(oldBattlePanel.gameObject);
                itemsRemoved++;
            }
        }

        // Clean up CombatManager references to old systems
        CombatManager combatManager = FindObjectOfType<CombatManager>();
        if (combatManager != null)
        {
            SerializedObject so = new SerializedObject(combatManager);

            SerializedProperty drawingPanelProp = so.FindProperty("drawingPanel");
            SerializedProperty drawingCanvasProp = so.FindProperty("drawingCanvas");
            SerializedProperty attackButtonProp = so.FindProperty("attackButton");
            SerializedProperty battleDrawingManagerProp = so.FindProperty("battleDrawingManager");

            // Clear legacy references
            if (drawingPanelProp.objectReferenceValue != null)
            {
                Debug.Log("Clearing legacy drawingPanel reference from CombatManager");
                drawingPanelProp.objectReferenceValue = null;
            }
            if (drawingCanvasProp.objectReferenceValue != null)
            {
                Debug.Log("Clearing legacy drawingCanvas reference from CombatManager");
                drawingCanvasProp.objectReferenceValue = null;
            }
            if (attackButtonProp.objectReferenceValue != null)
            {
                Debug.Log("Clearing legacy attackButton reference from CombatManager");
                attackButtonProp.objectReferenceValue = null;
            }
            if (battleDrawingManagerProp.objectReferenceValue != null)
            {
                Debug.Log("Clearing battleDrawingManager reference (will be reassigned)");
                battleDrawingManagerProp.objectReferenceValue = null;
            }

            so.ApplyModifiedProperties();
        }

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"========== CLEANUP COMPLETE: Removed {itemsRemoved} items ==========");

        EditorUtility.DisplayDialog("Cleanup Complete!",
            $"Removed {itemsRemoved} outdated components from BattleScene.\n\n" +
            "Next steps:\n" +
            "1. Save your scene (Ctrl+S)\n" +
            "2. Run: Tools > Sketch Blossom > Battle Scene > 2. Rebuild Drawing System",
            "OK");
    }

    [MenuItem("Tools/Sketch Blossom/Battle Scene/Reset Everything (Cleanup + Rebuild)")]
    public static void ResetEverything()
    {
        if (!EditorUtility.DisplayDialog("Reset Battle Scene Drawing System",
            "This will:\n" +
            "1. Remove all old drawing components\n" +
            "2. Rebuild the drawing system from scratch\n\n" +
            "This operation can be undone with Ctrl+Z.\n\n" +
            "Continue?",
            "Yes, Reset", "Cancel"))
        {
            return;
        }

        // Step 1: Cleanup
        CleanupOldSystems();

        // Step 2: Rebuild (call the other script)
        RebuildBattleDrawingSystem.RebuildDrawingSystem();
    }
}
