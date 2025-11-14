using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;

/// <summary>
/// Editor script to automatically setup PlantResultPanel button connections
/// </summary>
public class PlantResultPanelSetup : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Setup Result Panel Buttons")]
    public static void SetupResultPanelButtons()
    {
        // Find PlantResultPanel in the scene
        PlantResultPanel resultPanel = FindObjectOfType<PlantResultPanel>(true);

        if (resultPanel == null)
        {
            EditorUtility.DisplayDialog("Error",
                "PlantResultPanel not found in the scene!\n\n" +
                "Make sure DrawingScene is open and PlantResultPanel exists.",
                "OK");
            return;
        }

        Debug.Log("========== SETTING UP RESULT PANEL BUTTONS ==========");
        Debug.Log($"Found PlantResultPanel: {resultPanel.gameObject.name}");

        int buttonsSetup = 0;

        // Get the continue button field
        SerializedObject so = new SerializedObject(resultPanel);
        SerializedProperty continueButtonProp = so.FindProperty("continueButton");
        SerializedProperty redrawButtonProp = so.FindProperty("redrawButton");

        // Setup Continue Button
        if (continueButtonProp.objectReferenceValue != null)
        {
            Button continueButton = continueButtonProp.objectReferenceValue as Button;
            if (SetupButton(continueButton, resultPanel, "OnContinueButton"))
            {
                buttonsSetup++;
                Debug.Log($"✓ Continue button setup: {continueButton.gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Continue button reference is NULL in PlantResultPanel");
        }

        // Setup Redraw Button
        if (redrawButtonProp.objectReferenceValue != null)
        {
            Button redrawButton = redrawButtonProp.objectReferenceValue as Button;
            if (SetupButton(redrawButton, resultPanel, "OnRedrawButton"))
            {
                buttonsSetup++;
                Debug.Log($"✓ Redraw button setup: {redrawButton.gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Redraw button reference is NULL in PlantResultPanel");
        }

        // Mark the scene as dirty so changes are saved
        EditorUtility.SetDirty(resultPanel.gameObject);
        if (continueButtonProp.objectReferenceValue != null)
            EditorUtility.SetDirty(continueButtonProp.objectReferenceValue as Object);
        if (redrawButtonProp.objectReferenceValue != null)
            EditorUtility.SetDirty(redrawButtonProp.objectReferenceValue as Object);

        Debug.Log($"========== SETUP COMPLETE: {buttonsSetup} buttons configured ==========");

        EditorUtility.DisplayDialog("Success!",
            $"Result Panel buttons setup complete!\n\n" +
            $"Buttons configured: {buttonsSetup}\n\n" +
            "Make sure to save your scene!",
            "OK");
    }

    private static bool SetupButton(Button button, PlantResultPanel panel, string methodName)
    {
        if (button == null || panel == null)
            return false;

        // Remove all existing persistent listeners
        int oldCount = button.onClick.GetPersistentEventCount();
        for (int i = oldCount - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(button.onClick, i);
        }

        Debug.Log($"  Removed {oldCount} old listeners from {button.gameObject.name}");

        // Add the new persistent listener
        UnityAction action = null;

        if (methodName == "OnContinueButton")
        {
            action = new UnityAction(panel.OnContinueButton);
        }
        else if (methodName == "OnRedrawButton")
        {
            action = new UnityAction(panel.OnRedrawButton);
        }

        if (action != null)
        {
            UnityEventTools.AddVoidPersistentListener(button.onClick, action);
            EditorUtility.SetDirty(button);
            Debug.Log($"  ✓ Added listener for {methodName}");
            return true;
        }
        else
        {
            Debug.LogError($"Method {methodName} not found or could not create action!");
            return false;
        }
    }

    [MenuItem("Tools/Sketch Blossom/Auto-Find and Assign Result Panel Buttons")]
    public static void AutoFindAndAssignButtons()
    {
        // Find PlantResultPanel in the scene
        PlantResultPanel resultPanel = FindObjectOfType<PlantResultPanel>(true);

        if (resultPanel == null)
        {
            EditorUtility.DisplayDialog("Error",
                "PlantResultPanel not found in the scene!",
                "OK");
            return;
        }

        Debug.Log("========== AUTO-FINDING BUTTONS ==========");

        SerializedObject so = new SerializedObject(resultPanel);
        bool foundSomething = false;

        // Try to find Continue button
        SerializedProperty continueButtonProp = so.FindProperty("continueButton");
        if (continueButtonProp.objectReferenceValue == null)
        {
            // Search for button named "ContinueButton" or contains "Continue"
            Button[] allButtons = resultPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in allButtons)
            {
                if (btn.gameObject.name.Contains("Continue") || btn.gameObject.name.Contains("Battle"))
                {
                    continueButtonProp.objectReferenceValue = btn;
                    Debug.Log($"✓ Found Continue button: {btn.gameObject.name}");
                    foundSomething = true;
                    break;
                }
            }
        }

        // Try to find Redraw button
        SerializedProperty redrawButtonProp = so.FindProperty("redrawButton");
        if (redrawButtonProp.objectReferenceValue == null)
        {
            Button[] allButtons = resultPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in allButtons)
            {
                if (btn.gameObject.name.Contains("Redraw") || btn.gameObject.name.Contains("Again"))
                {
                    redrawButtonProp.objectReferenceValue = btn;
                    Debug.Log($"✓ Found Redraw button: {btn.gameObject.name}");
                    foundSomething = true;
                    break;
                }
            }
        }

        if (foundSomething)
        {
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(resultPanel);

            Debug.Log("========== AUTO-FIND COMPLETE ==========");

            // Now setup the buttons
            SetupResultPanelButtons();
        }
        else
        {
            EditorUtility.DisplayDialog("No Changes",
                "Buttons are already assigned or could not be found automatically.\n\n" +
                "Please assign them manually in the Inspector.",
                "OK");
        }
    }

    [MenuItem("Tools/Sketch Blossom/Complete Result Panel Setup")]
    public static void CompleteSetup()
    {
        Debug.Log("========== COMPLETE RESULT PANEL SETUP ==========");

        // Step 1: Auto-find buttons
        PlantResultPanel resultPanel = FindObjectOfType<PlantResultPanel>(true);

        if (resultPanel == null)
        {
            EditorUtility.DisplayDialog("Error",
                "PlantResultPanel not found in the scene!\n\n" +
                "Make sure DrawingScene is open.",
                "OK");
            return;
        }

        // Auto-assign buttons if not assigned
        SerializedObject so = new SerializedObject(resultPanel);
        SerializedProperty continueButtonProp = so.FindProperty("continueButton");
        SerializedProperty redrawButtonProp = so.FindProperty("redrawButton");

        int foundCount = 0;

        if (continueButtonProp.objectReferenceValue == null)
        {
            Button[] allButtons = resultPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in allButtons)
            {
                if (btn.gameObject.name.Contains("Continue") || btn.gameObject.name.Contains("Battle"))
                {
                    continueButtonProp.objectReferenceValue = btn;
                    foundCount++;
                    break;
                }
            }
        }
        else
        {
            foundCount++;
        }

        if (redrawButtonProp.objectReferenceValue == null)
        {
            Button[] allButtons = resultPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in allButtons)
            {
                if (btn.gameObject.name.Contains("Redraw") || btn.gameObject.name.Contains("Again"))
                {
                    redrawButtonProp.objectReferenceValue = btn;
                    foundCount++;
                    break;
                }
            }
        }
        else
        {
            foundCount++;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(resultPanel);

        Debug.Log($"✓ Found/assigned {foundCount} button references");

        // Step 2: Setup button listeners
        SetupResultPanelButtons();
    }
}
