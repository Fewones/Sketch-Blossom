using UnityEngine;
using UnityEditor;

/// <summary>
/// Diagnoses LineRenderer visibility issues in battle scene
/// </summary>
public class DiagnoseBattleDrawing : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Battle Scene/Diagnose LineRenderer Visibility")]
    public static void DiagnoseVisibility()
    {
        Debug.Log("========== DIAGNOSING LINE RENDERER VISIBILITY ==========\n");

        // Check camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("❌ No Main Camera found!");
            EditorUtility.DisplayDialog("Error", "No Main Camera found in scene!", "OK");
            return;
        }

        Debug.Log("CAMERA SETUP:");
        Debug.Log($"  Name: {mainCam.name}");
        Debug.Log($"  Position: {mainCam.transform.position}");
        Debug.Log($"  Rotation: {mainCam.transform.eulerAngles}");
        Debug.Log($"  Projection: {(mainCam.orthographic ? "Orthographic" : "Perspective")}");
        if (mainCam.orthographic)
        {
            Debug.Log($"  Orthographic Size: {mainCam.orthographicSize}");
        }
        Debug.Log($"  Near Clip: {mainCam.nearClipPlane}, Far Clip: {mainCam.farClipPlane}");

        // Check if z=10 is within clip planes
        float testZ = 10f;
        if (testZ < mainCam.nearClipPlane || testZ > mainCam.farClipPlane)
        {
            Debug.LogError($"❌ LineRenderers at z={testZ} are OUTSIDE camera clip planes!");
            Debug.LogError($"   Near: {mainCam.nearClipPlane}, Far: {mainCam.farClipPlane}");
            Debug.LogError("   SOLUTION: Adjust camera far clip plane to at least 15");
        }
        else
        {
            Debug.Log($"✓ z={testZ} is within camera clip planes");
        }

        // Check BattleDrawingCanvas
        BattleDrawingCanvas canvas = FindObjectOfType<BattleDrawingCanvas>();
        if (canvas == null)
        {
            Debug.LogError("\n❌ BattleDrawingCanvas NOT FOUND!");
            Debug.LogError("   Please run: Tools > Sketch Blossom > Battle Scene > 2. Rebuild Drawing System");
            return;
        }

        Debug.Log($"\n✓ BATTLEDRAWINGCANVAS FOUND");
        Debug.Log($"  GameObject: {canvas.gameObject.name}");
        Debug.Log($"  Enabled: {canvas.enabled}");
        Debug.Log($"  Stroke Count: {canvas.GetStrokeCount()}");

        // Check for wrong canvas types
        SimpleDrawingCanvas[] simpleCanvases = FindObjectsOfType<SimpleDrawingCanvas>();
        if (simpleCanvases.Length > 0)
        {
            Debug.LogWarning($"\n⚠️ FOUND {simpleCanvases.Length} SimpleDrawingCanvas (should only be in DrawingScene!)");
            foreach (var sc in simpleCanvases)
            {
                Debug.LogWarning($"   - {sc.gameObject.name}");
            }
            Debug.LogWarning("   SOLUTION: Run cleanup script first!");
        }

        // Check LineRenderers
        if (canvas.allStrokes.Count == 0)
        {
            Debug.LogWarning("\n⚠️ No strokes exist yet. Try drawing something in Play mode first!");
        }
        else
        {
            Debug.Log($"\nLINERENDERER ANALYSIS ({canvas.allStrokes.Count} strokes):");
            for (int i = 0; i < canvas.allStrokes.Count; i++)
            {
                LineRenderer lr = canvas.allStrokes[i];
                if (lr == null)
                {
                    Debug.LogWarning($"  Stroke #{i + 1}: NULL");
                    continue;
                }

                Debug.Log($"\n  Stroke #{i + 1}:");
                Debug.Log($"    GameObject: {lr.gameObject.name}");
                Debug.Log($"    Active: {lr.gameObject.activeSelf}");
                Debug.Log($"    Enabled: {lr.enabled}");
                Debug.Log($"    Width: {lr.startWidth}");
                Debug.Log($"    Color: {lr.startColor}");
                Debug.Log($"    Material: {(lr.material ? lr.material.name : "NULL")}");
                if (lr.material)
                {
                    Debug.Log($"    Shader: {lr.material.shader.name}");
                    Debug.Log($"    Material Color: {lr.material.color}");
                }
                Debug.Log($"    Points: {lr.positionCount}");
                Debug.Log($"    Sorting Order: {lr.sortingOrder}");
                Debug.Log($"    Use World Space: {lr.useWorldSpace}");

                // Check if material is valid
                if (lr.material == null)
                {
                    Debug.LogError("    ❌ Material is NULL!");
                }
                else if (lr.material.shader == null)
                {
                    Debug.LogError("    ❌ Shader is NULL!");
                }

                // Log positions
                if (lr.positionCount > 0)
                {
                    Vector3[] positions = new Vector3[lr.positionCount];
                    lr.GetPositions(positions);
                    Debug.Log($"    First Point: {positions[0]}");
                    if (lr.positionCount > 1)
                    {
                        Debug.Log($"    Last Point: {positions[positions.Length - 1]}");
                    }

                    // Check if positions are reasonable
                    bool allAtOrigin = true;
                    foreach (var pos in positions)
                    {
                        if (pos.magnitude > 0.01f)
                        {
                            allAtOrigin = false;
                            break;
                        }
                    }

                    if (allAtOrigin)
                    {
                        Debug.LogWarning("    ⚠️ All points are at origin (0,0,0)!");
                    }
                }
            }
        }

        Debug.Log("\n========== DIAGNOSIS COMPLETE ==========");
        Debug.Log("\nCOMMON SOLUTIONS:");
        Debug.Log("1. If you see 2 canvases: Run cleanup script");
        Debug.Log("2. If camera far clip is too small: Increase it to 15+");
        Debug.Log("3. If material/shader is NULL: Rebuild drawing system");
        Debug.Log("4. If all points are at origin: Camera reference is wrong");
    }
}
