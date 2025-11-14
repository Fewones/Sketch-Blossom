using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Diagnoses and fixes the battle drawing panel structure and references
/// </summary>
public class DiagnoseBattleDrawing : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Battle Scene/Diagnose Drawing System")]
    public static void DiagnoseDrawingSystem()
    {
        Debug.Log("========== DIAGNOSING BATTLE DRAWING SYSTEM ==========");

        // Verify we're in a battle scene
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Battle"))
        {
            EditorUtility.DisplayDialog("Wrong Scene",
                $"This tool is for Battle scenes only.\n\nCurrent scene: {sceneName}",
                "OK");
            return;
        }

        bool hasErrors = false;
        string errorReport = "";

        // Find components
        BattleDrawingManager battleManager = FindObjectOfType<BattleDrawingManager>();
        SimpleDrawingCanvas drawingCanvas = FindObjectOfType<SimpleDrawingCanvas>();
        CombatManager combatManager = FindObjectOfType<CombatManager>();
        Canvas canvas = FindObjectOfType<Canvas>();

        Debug.Log("=== CHECKING COMPONENTS ===");

        // Check BattleDrawingManager
        if (battleManager == null)
        {
            Debug.LogError("❌ BattleDrawingManager NOT FOUND");
            errorReport += "• BattleDrawingManager missing\n";
            hasErrors = true;
        }
        else
        {
            Debug.Log($"✓ BattleDrawingManager found: {battleManager.gameObject.name}");

            // Check all references
            SerializedObject so = new SerializedObject(battleManager);

            if (so.FindProperty("drawingCanvas").objectReferenceValue == null)
            {
                Debug.LogError("❌ BattleDrawingManager.drawingCanvas is NULL");
                errorReport += "• BattleDrawingManager.drawingCanvas not assigned\n";
                hasErrors = true;
            }
            else
            {
                Debug.Log("✓ drawingCanvas assigned");
            }

            if (so.FindProperty("drawingPanel").objectReferenceValue == null)
            {
                Debug.LogError("❌ BattleDrawingManager.drawingPanel is NULL");
                errorReport += "• BattleDrawingManager.drawingPanel not assigned\n";
                hasErrors = true;
            }
            else
            {
                Debug.Log("✓ drawingPanel assigned");
            }

            if (so.FindProperty("drawingAreaImage").objectReferenceValue == null)
            {
                Debug.LogError("❌ BattleDrawingManager.drawingAreaImage is NULL");
                errorReport += "• BattleDrawingManager.drawingAreaImage not assigned\n";
                hasErrors = true;
            }
            else
            {
                Debug.Log("✓ drawingAreaImage assigned");
            }

            if (so.FindProperty("submitButton").objectReferenceValue == null)
            {
                Debug.LogError("❌ BattleDrawingManager.submitButton is NULL");
                errorReport += "• BattleDrawingManager.submitButton not assigned\n";
                hasErrors = true;
            }
            else
            {
                Debug.Log("✓ submitButton assigned");
            }
        }

        // Check SimpleDrawingCanvas
        if (drawingCanvas == null)
        {
            Debug.LogError("❌ SimpleDrawingCanvas NOT FOUND");
            errorReport += "• SimpleDrawingCanvas missing\n";
            hasErrors = true;
        }
        else
        {
            Debug.Log($"✓ SimpleDrawingCanvas found: {drawingCanvas.gameObject.name}");

            SerializedObject canvasSO = new SerializedObject(drawingCanvas);

            if (canvasSO.FindProperty("mainCamera").objectReferenceValue == null)
            {
                Debug.LogError("❌ SimpleDrawingCanvas.mainCamera is NULL");
                errorReport += "• SimpleDrawingCanvas.mainCamera not assigned\n";
                hasErrors = true;
            }
            else
            {
                Debug.Log("✓ mainCamera assigned");
            }

            if (canvasSO.FindProperty("drawingArea").objectReferenceValue == null)
            {
                Debug.LogError("❌ SimpleDrawingCanvas.drawingArea is NULL");
                errorReport += "• SimpleDrawingCanvas.drawingArea not assigned\n";
                hasErrors = true;
            }
            else
            {
                Debug.Log("✓ drawingArea assigned");

                // Check the drawing area RectTransform
                RectTransform drawingAreaRect = canvasSO.FindProperty("drawingArea").objectReferenceValue as RectTransform;
                if (drawingAreaRect != null)
                {
                    Debug.Log($"  Drawing area size: {drawingAreaRect.rect.size}");
                    Debug.Log($"  Drawing area position: {drawingAreaRect.anchoredPosition}");

                    // Check if it has a RawImage for raycasting
                    RawImage rawImage = drawingAreaRect.GetComponent<RawImage>();
                    if (rawImage == null)
                    {
                        Debug.LogWarning("⚠️ DrawingArea has no RawImage component!");
                        errorReport += "• DrawingArea missing RawImage (needed for mouse detection)\n";
                        hasErrors = true;
                    }
                    else if (!rawImage.raycastTarget)
                    {
                        Debug.LogWarning("⚠️ DrawingArea RawImage raycastTarget is DISABLED!");
                        errorReport += "• DrawingArea RawImage raycastTarget disabled\n";
                        hasErrors = true;
                    }
                    else
                    {
                        Debug.Log("✓ DrawingArea has RawImage with raycastTarget enabled");
                    }
                }
            }

            if (canvasSO.FindProperty("strokeContainer").objectReferenceValue == null)
            {
                Debug.LogError("❌ SimpleDrawingCanvas.strokeContainer is NULL");
                errorReport += "• SimpleDrawingCanvas.strokeContainer not assigned\n";
                hasErrors = true;
            }
            else
            {
                Debug.Log("✓ strokeContainer assigned");
            }

            if (canvasSO.FindProperty("lineRendererPrefab").objectReferenceValue == null)
            {
                Debug.LogError("❌ SimpleDrawingCanvas.lineRendererPrefab is NULL");
                errorReport += "• SimpleDrawingCanvas.lineRendererPrefab not assigned\n";
                hasErrors = true;
            }
            else
            {
                Debug.Log("✓ lineRendererPrefab assigned");
            }
        }

        // Check hierarchy structure
        Debug.Log("\n=== CHECKING HIERARCHY STRUCTURE ===");
        if (canvas != null)
        {
            Transform battlePanel = canvas.transform.Find("BattleDrawingPanel");
            if (battlePanel == null)
            {
                Debug.LogError("❌ BattleDrawingPanel NOT FOUND in Canvas");
                errorReport += "• BattleDrawingPanel missing from Canvas\n";
                hasErrors = true;
            }
            else
            {
                Debug.Log($"✓ BattleDrawingPanel found");

                // Check children
                Transform drawingArea = battlePanel.Find("DrawingArea");
                if (drawingArea == null)
                {
                    Debug.LogError("❌ DrawingArea NOT FOUND in BattleDrawingPanel");
                    errorReport += "• DrawingArea missing from BattleDrawingPanel\n";
                    hasErrors = true;
                }
                else
                {
                    Debug.Log("✓ DrawingArea found");
                }

                Transform submitBtn = battlePanel.Find("SubmitButton");
                if (submitBtn == null)
                {
                    Debug.LogError("❌ SubmitButton NOT FOUND in BattleDrawingPanel");
                    errorReport += "• SubmitButton missing\n";
                    hasErrors = true;
                }
                else
                {
                    Debug.Log("✓ SubmitButton found");

                    // Check button listener
                    Button btn = submitBtn.GetComponent<Button>();
                    if (btn != null && btn.onClick.GetPersistentEventCount() == 0)
                    {
                        Debug.LogWarning("⚠️ SubmitButton has no onClick listeners!");
                        errorReport += "• SubmitButton has no onClick listeners\n";
                        hasErrors = true;
                    }
                }

                Transform clearBtn = battlePanel.Find("ClearButton");
                if (clearBtn == null)
                {
                    Debug.LogError("❌ ClearButton NOT FOUND in BattleDrawingPanel");
                    errorReport += "• ClearButton missing\n";
                    hasErrors = true;
                }
                else
                {
                    Debug.Log("✓ ClearButton found");
                }
            }
        }

        // Check layering
        Debug.Log("\n=== CHECKING LAYERING ===");
        if (canvas != null)
        {
            Debug.Log($"Canvas renderMode: {canvas.renderMode}");
            Debug.Log($"Canvas sortingOrder: {canvas.sortingOrder}");

            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogError("❌ Canvas missing GraphicRaycaster!");
                errorReport += "• Canvas missing GraphicRaycaster\n";
                hasErrors = true;
            }
            else
            {
                Debug.Log("✓ Canvas has GraphicRaycaster");
            }
        }

        // Check EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("❌ NO EventSystem in scene!");
            errorReport += "• EventSystem missing\n";
            hasErrors = true;
        }
        else
        {
            Debug.Log($"✓ EventSystem found: {eventSystem.gameObject.name}");
        }

        Debug.Log("\n========== DIAGNOSIS COMPLETE ==========");

        if (hasErrors)
        {
            bool fix = EditorUtility.DisplayDialog("Issues Found!",
                $"Found the following issues:\n\n{errorReport}\n" +
                "Would you like to attempt automatic fixes?",
                "Yes, Fix Issues", "No, Cancel");

            if (fix)
            {
                FixDrawingSystem();
            }
        }
        else
        {
            EditorUtility.DisplayDialog("No Issues Found",
                "The battle drawing system appears to be set up correctly!\n\n" +
                "If drawing still doesn't work, check the Console logs when:\n" +
                "• The drawing panel appears\n" +
                "• You try to draw\n" +
                "• You click Submit\n\n" +
                "The logs will show exactly what's happening.",
                "OK");
        }
    }

    [MenuItem("Tools/Sketch Blossom/Battle Scene/Fix Drawing System")]
    public static void FixDrawingSystem()
    {
        Debug.Log("========== FIXING BATTLE DRAWING SYSTEM ==========");

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene!", "OK");
            return;
        }

        // Find or create BattleDrawingPanel
        Transform battlePanelTransform = canvas.transform.Find("BattleDrawingPanel");
        GameObject battlePanel;

        if (battlePanelTransform == null)
        {
            Debug.LogError("❌ BattleDrawingPanel not found. Please run: Tools > Sketch Blossom > Battle Scene > 2. Rebuild Drawing System");
            EditorUtility.DisplayDialog("Error",
                "BattleDrawingPanel not found!\n\n" +
                "Please run: Tools > Sketch Blossom > Battle Scene > 2. Rebuild Drawing System",
                "OK");
            return;
        }

        battlePanel = battlePanelTransform.gameObject;

        // Find DrawingArea
        Transform drawingAreaTransform = battlePanel.transform.Find("DrawingArea");
        if (drawingAreaTransform == null)
        {
            Debug.LogError("❌ DrawingArea not found in BattleDrawingPanel!");
            return;
        }

        RectTransform drawingAreaRect = drawingAreaTransform.GetComponent<RectTransform>();

        // Fix DrawingArea - ensure it has RawImage with raycastTarget
        RawImage drawingAreaImage = drawingAreaRect.GetComponent<RawImage>();
        if (drawingAreaImage == null)
        {
            drawingAreaImage = drawingAreaRect.gameObject.AddComponent<RawImage>();
            drawingAreaImage.color = new Color(0.05f, 0.05f, 0.05f, 1f);
            Debug.Log("✓ Added RawImage to DrawingArea");
        }

        if (!drawingAreaImage.raycastTarget)
        {
            drawingAreaImage.raycastTarget = true;
            EditorUtility.SetDirty(drawingAreaImage);
            Debug.Log("✓ Enabled raycastTarget on DrawingArea");
        }

        // Ensure panel background doesn't block raycasts
        Image panelImage = battlePanel.GetComponent<Image>();
        if (panelImage != null && panelImage.raycastTarget)
        {
            panelImage.raycastTarget = false;
            EditorUtility.SetDirty(panelImage);
            Debug.Log("✓ Disabled raycastTarget on panel background");
        }

        // Find components
        BattleDrawingManager battleManager = FindObjectOfType<BattleDrawingManager>();
        SimpleDrawingCanvas drawingCanvas = FindObjectOfType<SimpleDrawingCanvas>();

        // Wire up BattleDrawingManager
        if (battleManager != null && drawingCanvas != null)
        {
            SerializedObject managerSO = new SerializedObject(battleManager);

            SerializedProperty drawingCanvasProp = managerSO.FindProperty("drawingCanvas");
            if (drawingCanvasProp.objectReferenceValue == null)
            {
                drawingCanvasProp.objectReferenceValue = drawingCanvas;
                Debug.Log("✓ Assigned drawingCanvas to BattleDrawingManager");
            }

            SerializedProperty drawingPanelProp = managerSO.FindProperty("drawingPanel");
            if (drawingPanelProp.objectReferenceValue == null)
            {
                drawingPanelProp.objectReferenceValue = battlePanel;
                Debug.Log("✓ Assigned drawingPanel to BattleDrawingManager");
            }

            SerializedProperty drawingAreaImageProp = managerSO.FindProperty("drawingAreaImage");
            if (drawingAreaImageProp.objectReferenceValue == null)
            {
                drawingAreaImageProp.objectReferenceValue = drawingAreaImage;
                Debug.Log("✓ Assigned drawingAreaImage to BattleDrawingManager");
            }

            Transform submitBtn = battlePanel.transform.Find("SubmitButton");
            if (submitBtn != null)
            {
                SerializedProperty submitButtonProp = managerSO.FindProperty("submitButton");
                submitButtonProp.objectReferenceValue = submitBtn.GetComponent<Button>();
                Debug.Log("✓ Assigned submitButton to BattleDrawingManager");
            }

            Transform clearBtn = battlePanel.transform.Find("ClearButton");
            if (clearBtn != null)
            {
                SerializedProperty clearButtonProp = managerSO.FindProperty("clearButton");
                clearButtonProp.objectReferenceValue = clearBtn.GetComponent<Button>();
                Debug.Log("✓ Assigned clearButton to BattleDrawingManager");
            }

            Transform instructionText = battlePanel.transform.Find("InstructionText");
            if (instructionText != null)
            {
                SerializedProperty instructionTextProp = managerSO.FindProperty("instructionText");
                instructionTextProp.objectReferenceValue = instructionText.GetComponent<TextMeshProUGUI>();
                Debug.Log("✓ Assigned instructionText to BattleDrawingManager");
            }

            CombatManager combatManager = FindObjectOfType<CombatManager>();
            if (combatManager != null)
            {
                SerializedProperty combatManagerProp = managerSO.FindProperty("combatManager");
                combatManagerProp.objectReferenceValue = combatManager;
                Debug.Log("✓ Assigned combatManager to BattleDrawingManager");
            }

            managerSO.ApplyModifiedProperties();
        }

        // Wire up SimpleDrawingCanvas
        if (drawingCanvas != null)
        {
            SerializedObject canvasSO = new SerializedObject(drawingCanvas);

            SerializedProperty mainCameraProp = canvasSO.FindProperty("mainCamera");
            if (mainCameraProp.objectReferenceValue == null)
            {
                mainCameraProp.objectReferenceValue = Camera.main;
                Debug.Log("✓ Assigned mainCamera to SimpleDrawingCanvas");
            }

            SerializedProperty drawingAreaProp = canvasSO.FindProperty("drawingArea");
            if (drawingAreaProp.objectReferenceValue == null)
            {
                drawingAreaProp.objectReferenceValue = drawingAreaRect;
                Debug.Log("✓ Assigned drawingArea to SimpleDrawingCanvas");
            }

            canvasSO.ApplyModifiedProperties();
        }

        // Ensure Canvas has GraphicRaycaster
        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            Debug.Log("✓ Added GraphicRaycaster to Canvas");
        }

        // Ensure EventSystem exists
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✓ Created EventSystem");
        }

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("========== FIX COMPLETE ==========");

        EditorUtility.DisplayDialog("Fixes Applied!",
            "The drawing system has been fixed.\n\n" +
            "Key fixes:\n" +
            "• DrawingArea has RawImage with raycastTarget\n" +
            "• Panel background doesn't block clicks\n" +
            "• All references wired up\n" +
            "• EventSystem ensured\n\n" +
            "Save your scene and test!",
            "OK");
    }
}
