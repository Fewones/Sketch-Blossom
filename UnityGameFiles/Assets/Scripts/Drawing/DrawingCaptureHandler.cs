using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Handles capturing the plant drawing from LineRenderers and converting it to a Texture2D
/// This texture can then be used as a sprite in the battle scene
/// </summary>
public class DrawingCaptureHandler : MonoBehaviour
{
    [Header("Capture Settings")]
    [SerializeField] private int textureWidth = 512;
    [SerializeField] private int textureHeight = 512;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0); // Transparent background
    [SerializeField] private bool useScreenCapture = false; // Alternative capture method

    /// <summary>
    /// Capture all LineRenderers and convert them to a Texture2D.
    /// When forceTransparent is true the background is fully transparent (for battle sprites).
    /// When false (default) the drawing area's background colour is used (white for CLIP).
    /// If fillTexture is provided it is composited behind the strokes.
    /// </summary>
    public Texture2D CaptureDrawing(List<LineRenderer> strokes, Camera sourceCamera, RectTransform drawingArea = null, bool forceTransparent = false, Texture2D fillTexture = null)
    {
        if (strokes == null || strokes.Count == 0)
        {
            Debug.LogWarning("DrawingCaptureHandler: No strokes to capture!");
            return null;
        }

        if (sourceCamera == null)
        {
            Debug.LogError("DrawingCaptureHandler: Source camera is null!");
            return null;
        }

        Debug.Log($"DrawingCaptureHandler: Capturing {strokes.Count} strokes to texture ({textureWidth}x{textureHeight})");

        // Ensure all strokes are active and visible
        int activeStrokes = 0;
        int inactiveStrokes = 0;
        foreach (var stroke in strokes)
        {
            if (stroke != null)
            {
                // Force activate any inactive strokes
                if (!stroke.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"Stroke '{stroke.name}' was inactive - activating for capture");
                    stroke.gameObject.SetActive(true);
                    inactiveStrokes++;
                }
                else
                {
                    activeStrokes++;
                }
            }
        }
        Debug.Log($"Stroke status: {activeStrokes} active, {inactiveStrokes} were inactive (now activated)");

        // If drawing area is provided and screen capture is enabled, use screen capture method
        if (useScreenCapture && drawingArea != null)
        {
            Debug.Log("Using screen capture method");
            return CaptureFromScreen(drawingArea, sourceCamera);
        }

        // Calculate bounds of all strokes to properly frame the drawing
        Bounds drawingBounds = CalculateDrawingBounds(strokes);

        if (drawingBounds.size == Vector3.zero)
        {
            Debug.LogWarning("DrawingCaptureHandler: Drawing has no size!");
            return null;
        }

        // Create a temporary camera for capturing
        GameObject tempCameraObj = new GameObject("TempCaptureCamera");
        Camera captureCamera = tempCameraObj.AddComponent<Camera>();

        // Configure camera
        captureCamera.orthographic = true;
        if (forceTransparent)
        {
            captureCamera.backgroundColor = new Color(0, 0, 0, 0);
        }
        else if ((drawingArea == null) || (drawingArea.GetComponent<Image> () == null))
        {
            captureCamera.backgroundColor = backgroundColor;
        }
        else
        {
            captureCamera.backgroundColor = drawingArea.GetComponent<Image> ().color;
        }
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.cullingMask = 1 + 2 + 4 + 16 + 32; // Render all layers except background layer (layer 3)
        captureCamera.depth = sourceCamera.depth + 1; // Render after main camera
        captureCamera.nearClipPlane = 0.1f;
        captureCamera.farClipPlane = 100f;

        // Position camera to frame the drawing
        FrameDrawing(captureCamera, drawingBounds);

        Debug.Log($"Capture Camera Setup - Position: {captureCamera.transform.position}, OrthographicSize: {captureCamera.orthographicSize}");

        // Create RenderTexture
        RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
        renderTexture.format = RenderTextureFormat.ARGB32;
        captureCamera.targetTexture = renderTexture;

        // If a fill texture is provided, create a temporary world-space quad behind strokes
        // so the capture camera picks it up automatically.
        GameObject tempFillQuad = null;
        if (fillTexture != null && drawingArea != null)
        {
            tempFillQuad = CreateFillQuad(fillTexture, drawingArea, sourceCamera);
        }

        // Render the scene
        captureCamera.Render();

        // Destroy the temp fill quad immediately
        if (tempFillQuad != null)
        {
            DestroyImmediate(tempFillQuad);
        }

        // Read pixels from RenderTexture into Texture2D
        RenderTexture.active = renderTexture;
        Texture2D capturedTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        capturedTexture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        capturedTexture.Apply();

        // Validate texture has content
        bool hasContent = ValidateTextureContent(capturedTexture);
        if (!hasContent)
        {
            Debug.LogWarning("DrawingCaptureHandler: Captured texture appears to be empty or all transparent!");
        }

        // Cleanup
        RenderTexture.active = null;
        captureCamera.targetTexture = null;
        Destroy(renderTexture);
        Destroy(tempCameraObj);

        Debug.Log("DrawingCaptureHandler: Successfully captured drawing to texture!");
        return capturedTexture;
    }

        // Same method without framing and bounds and another texture for the base plant image
        public Texture2D CaptureWholeDrawingArea(List<LineRenderer> strokes, Camera sourceCamera, RectTransform drawingArea = null, bool forceTransparent = false, Texture2D fillTexture = null)
    {
        if (strokes == null || strokes.Count == 0)
        {
            Debug.LogWarning("DrawingCaptureHandler: No strokes to capture!");
            return null;
        }

        if (sourceCamera == null)
        {
            Debug.LogError("DrawingCaptureHandler: Source camera is null!");
            return null;
        }

        Debug.Log($"DrawingCaptureHandler: Capturing {strokes.Count} strokes to texture ({textureWidth}x{textureHeight})");

        // Ensure all strokes are active and visible
        int activeStrokes = 0;
        int inactiveStrokes = 0;
        foreach (var stroke in strokes)
        {
            if (stroke != null)
            {
                // Force activate any inactive strokes
                if (!stroke.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"Stroke '{stroke.name}' was inactive - activating for capture");
                    stroke.gameObject.SetActive(true);
                    inactiveStrokes++;
                }
                else
                {
                    activeStrokes++;
                }
            }
        }
        Debug.Log($"Stroke status: {activeStrokes} active, {inactiveStrokes} were inactive (now activated)");

        // Create a temporary camera for capturing
        GameObject tempCameraObj = new GameObject("TempCaptureCamera");
        Camera captureCamera = tempCameraObj.AddComponent<Camera>();

        // Configure camera
        captureCamera.orthographic = true;
        if (forceTransparent)
        {
            captureCamera.backgroundColor = new Color(0, 0, 0, 0);
        }
        else if ((drawingArea == null) || (drawingArea.GetComponent<Image> () == null))
        {
            captureCamera.backgroundColor = backgroundColor;
        }
        else
        {
            captureCamera.backgroundColor = drawingArea.GetComponent<Image> ().color;
        }
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.cullingMask = 1 + 2 + 4 + 16 + 32; // Render all layers except background layer (layer 3)
        captureCamera.depth = sourceCamera.depth + 1; // Render after main camera
        captureCamera.nearClipPlane = 0.1f;
        captureCamera.farClipPlane = 100f;

        // We have no drawing bounds but we still need to change the z value of the camera to capture the strokes
        captureCamera.transform.position -= new Vector3(0,0,10);

        // Create RenderTexture
        RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
        renderTexture.format = RenderTextureFormat.ARGB32;
        captureCamera.targetTexture = renderTexture;

        // If a fill texture is provided, create a temporary world-space quad behind strokes
        // so the capture camera picks it up automatically.
        GameObject tempFillQuad = null;
        if (fillTexture != null && drawingArea != null)
        {
            tempFillQuad = CreateFillQuad(fillTexture, drawingArea, sourceCamera);
        }

        // Render the scene
        captureCamera.Render();

        // Destroy the temp fill quad immediately
        if (tempFillQuad != null)
        {
            DestroyImmediate(tempFillQuad);
        }

        // Read pixels from RenderTexture into Texture2D
        RenderTexture.active = renderTexture;
        Texture2D capturedTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        capturedTexture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        capturedTexture.Apply();

        // Validate texture has content
        bool hasContent = ValidateTextureContent(capturedTexture);
        if (!hasContent)
        {
            Debug.LogWarning("DrawingCaptureHandler: Captured texture appears to be empty or all transparent!");
        }

        // Cleanup
        RenderTexture.active = null;
        captureCamera.targetTexture = null;
        Destroy(renderTexture);
        Destroy(tempCameraObj);

        Debug.Log("DrawingCaptureHandler: Successfully captured drawing to texture!");
        return capturedTexture;
    }

    /// <summary>
    /// Calculate the bounds that encompass all strokes
    /// </summary>
    private Bounds CalculateDrawingBounds(List<LineRenderer> strokes)
    {
        Bounds bounds = new Bounds();
        bool firstPoint = true;

        foreach (var stroke in strokes)
        {
            if (stroke == null) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            foreach (var pos in positions)
            {
                if (firstPoint)
                {
                    bounds = new Bounds(pos, Vector3.zero);
                    firstPoint = false;
                }
                else
                {
                    bounds.Encapsulate(pos);
                }
            }
        }

        // Add some padding (10% on each side)
        bounds.Expand(bounds.size * 0.2f);

        Debug.Log($"DrawingCaptureHandler: Drawing bounds = Center:{bounds.center}, Size:{bounds.size}");
        return bounds;
    }

    /// <summary>
    /// Position and size the camera to frame the drawing perfectly
    /// </summary>
    private void FrameDrawing(Camera camera, Bounds bounds)
    {
        // Position camera centered on the drawing, looking at it from front
        camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z - 10f);
        camera.transform.LookAt(bounds.center);

        // Calculate orthographic size to fit the drawing
        // Use the larger dimension to ensure everything fits
        float aspectRatio = (float)textureWidth / textureHeight;
        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        // Calculate required orthographic size
        float horizontalSize = boundsWidth / aspectRatio / 2f;
        float verticalSize = boundsHeight / 2f;

        camera.orthographicSize = Mathf.Max(horizontalSize, verticalSize);

        Debug.Log($"DrawingCaptureHandler: Camera orthographic size = {camera.orthographicSize}");
    }

    /// <summary>
    /// Alternative method: Capture from a specific area on screen
    /// Useful if you want to capture exactly what the player sees
    /// </summary>
    public Texture2D CaptureFromScreen(RectTransform drawingArea, Camera sourceCamera)
    {
        if (drawingArea == null || sourceCamera == null)
        {
            Debug.LogError("DrawingCaptureHandler: DrawingArea or Camera is null!");
            return null;
        }

        // Get screen space corners of the drawing area
        Vector3[] corners = new Vector3[4];
        drawingArea.GetWorldCorners(corners);

        // Convert to screen space
        Vector2 min = sourceCamera.WorldToScreenPoint(corners[0]);
        Vector2 max = sourceCamera.WorldToScreenPoint(corners[2]);

        int width = Mathf.RoundToInt(max.x - min.x);
        int height = Mathf.RoundToInt(max.y - min.y);

        // Read pixels from screen
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(min.x, min.y, width, height), 0, 0);
        screenshot.Apply();

        // Scale to desired size
        Texture2D scaledTexture = ScaleTexture(screenshot, textureWidth, textureHeight);
        Destroy(screenshot);

        return scaledTexture;
    }

    /// <summary>
    /// Scale a texture to a new size
    /// </summary>
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        Graphics.Blit(source, rt);

        RenderTexture.active = rt;
        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    /// <summary>
    /// Validate that the texture has visible content (not all transparent/background)
    /// </summary>
    private bool ValidateTextureContent(Texture2D texture)
    {
        if (texture == null) return false;

        Color[] pixels = texture.GetPixels();
        int visiblePixels = 0;

        // Sample every 10th pixel for performance
        for (int i = 0; i < pixels.Length; i += 10)
        {
            Color pixel = pixels[i];

            // Check if pixel is visible (has alpha > 0.1 and not matching background)
            bool isVisible = pixel.a > 0.1f && !ColorApproximatelyEqual(pixel, backgroundColor);

            if (isVisible)
            {
                visiblePixels++;
            }
        }

        float percentageVisible = (float)visiblePixels / (pixels.Length / 10) * 100f;
        Debug.Log($"DrawingCaptureHandler: Texture has {percentageVisible:F1}% visible pixels (alpha > 0.1)");

        return visiblePixels > 10; // Need at least 10 visible pixels
    }

    /// <summary>
    /// Check if two colors are approximately equal
    /// </summary>
    private bool ColorApproximatelyEqual(Color a, Color b, float threshold = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold &&
               Mathf.Abs(a.a - b.a) < threshold;
    }

    /// <summary>
    /// Creates a temporary world-space quad textured with the fill layer.
    /// Positioned behind all strokes so the capture camera renders it in the background.
    /// </summary>
    private GameObject CreateFillQuad(Texture2D fillTex, RectTransform drawArea, Camera srcCamera)
    {
        // Get the drawing area bounds in screen space, then project into world space
        // at a depth behind all strokes (strokes start at distance 10 from camera).
        Vector3[] corners = new Vector3[4];
        drawArea.GetWorldCorners(corners);
        Vector2 minScreen = srcCamera.WorldToScreenPoint(corners[0]);
        Vector2 maxScreen = srcCamera.WorldToScreenPoint(corners[2]);

        float behindZ = 10.5f; // strokes are at 10 → 9.8; this sits behind them
        Vector3 bl = srcCamera.ScreenToWorldPoint(new Vector3(minScreen.x, minScreen.y, behindZ));
        Vector3 br = srcCamera.ScreenToWorldPoint(new Vector3(maxScreen.x, minScreen.y, behindZ));
        Vector3 tl = srcCamera.ScreenToWorldPoint(new Vector3(minScreen.x, maxScreen.y, behindZ));
        Vector3 tr = srcCamera.ScreenToWorldPoint(new Vector3(maxScreen.x, maxScreen.y, behindZ));

        GameObject obj = new GameObject("_TempFillQuad");
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { bl, br, tl, tr };
        mesh.uv = new Vector2[] {
            new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0, 1), new Vector2(1, 1)
        };
        mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        mf.mesh = mesh;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = fillTex;
        mr.material = mat;

        return obj;
    }
}
