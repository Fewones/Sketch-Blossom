using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates and assigns a proper material for LineRenderer colors
/// This ensures colors display correctly
/// </summary>
public class LineRendererMaterialFix : EditorWindow
{
    [MenuItem("Tools/Sketch Blossom/Fix LineRenderer Colors")]
    public static void FixLineRendererColors()
    {
        Debug.Log("=== FIXING LINERENDERER COLORS ===");

        // Find the LineRenderer prefab
        GameObject prefab = FindLineRendererPrefab();
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Could not find LineRenderer prefab!\n\n" +
                "Please assign the LineRenderer prefab to DrawingCanvas manually.",
                "OK");
            return;
        }

        LineRenderer lineRenderer = prefab.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Prefab doesn't have a LineRenderer component!",
                "OK");
            return;
        }

        // Create a new unlit color material
        Material colorMaterial = CreateUnlitColorMaterial();

        // Assign material to LineRenderer
        lineRenderer.material = colorMaterial;
        lineRenderer.startColor = Color.white; // Default to white, will be overridden
        lineRenderer.endColor = Color.white;

        // Save the prefab
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        if (!string.IsNullOrEmpty(prefabPath))
        {
            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log($"Updated LineRenderer prefab at: {prefabPath}");
        }

        Debug.Log("=== LINERENDERER COLOR FIX COMPLETE ===");
        EditorUtility.DisplayDialog("Success!",
            "LineRenderer material has been fixed!\n\n" +
            "A new 'LineColorMaterial' has been created and assigned.\n" +
            "Colors should now display correctly when drawing.",
            "OK");
    }

    private static GameObject FindLineRendererPrefab()
    {
        // Try to find LineRenderer prefab in Assets
        string[] guids = AssetDatabase.FindAssets("LineRenderer t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.GetComponent<LineRenderer>() != null)
            {
                Debug.Log($"Found LineRenderer prefab: {path}");
                return prefab;
            }
        }

        // Also check for StrokeLine
        guids = AssetDatabase.FindAssets("StrokeLine t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.GetComponent<LineRenderer>() != null)
            {
                Debug.Log($"Found StrokeLine prefab: {path}");
                return prefab;
            }
        }

        Debug.LogWarning("Could not find LineRenderer prefab automatically");
        return null;
    }

    private static Material CreateUnlitColorMaterial()
    {
        // Check if material already exists
        string materialPath = "Assets/Materials/LineColorMaterial.mat";
        Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (existingMat != null)
        {
            Debug.Log("Using existing LineColorMaterial");
            return existingMat;
        }

        // Create Materials folder if needed
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        // Create new material with Sprites/Default shader (supports vertex colors)
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            // Fallback to Unlit/Color
            shader = Shader.Find("Unlit/Color");
        }
        if (shader == null)
        {
            // Last resort - create a simple unlit shader
            shader = Shader.Find("Unlit/Transparent");
        }

        Material material = new Material(shader);
        material.name = "LineColorMaterial";
        material.color = Color.white; // Will be multiplied by vertex colors

        // Save the material
        AssetDatabase.CreateAsset(material, materialPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created new material: {materialPath} with shader: {shader.name}");
        return material;
    }

    [MenuItem("Tools/Sketch Blossom/Test Color Display")]
    public static void TestColorDisplay()
    {
        DrawingCanvas canvas = FindObjectOfType<DrawingCanvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No DrawingCanvas found in scene!", "OK");
            return;
        }

        Debug.Log("=== COLOR DISPLAY TEST ===");
        Debug.Log($"Current color: {canvas.currentDrawingColor}");
        Debug.Log($"Red color setting: {canvas.redColor}");
        Debug.Log($"Green color setting: {canvas.greenColor}");
        Debug.Log($"Blue color setting: {canvas.blueColor}");

        if (canvas.lineRendererPrefab != null)
        {
            LineRenderer lr = canvas.lineRendererPrefab.GetComponent<LineRenderer>();
            if (lr != null)
            {
                Debug.Log($"LineRenderer material: {(lr.material != null ? lr.material.name : "NULL")}");
                Debug.Log($"LineRenderer shader: {(lr.material != null ? lr.material.shader.name : "NULL")}");
                Debug.Log($"LineRenderer startColor: {lr.startColor}");
                Debug.Log($"LineRenderer endColor: {lr.endColor}");
            }
        }
        else
        {
            Debug.LogError("LineRenderer prefab is not assigned!");
        }
    }
}
