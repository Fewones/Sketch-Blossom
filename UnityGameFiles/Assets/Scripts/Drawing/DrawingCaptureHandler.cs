using UnityEngine;
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
    [SerializeField] private Color backgroundColor = Color.white; // White background (was transparent)
    [SerializeField] private bool useScreenCapture = false; // Alternative capture method

    /// <summary>
    /// Capture all LineRenderers and convert them to a Texture2D
    /// Uses screen capture method for reliability
    /// </summary>
    public Texture2D CaptureDrawing(List<LineRenderer> strokes, Camera sourceCamera, RectTransform drawingArea = null)
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
        captureCamera.backgroundColor = backgroundColor;
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.cullingMask = -1; // Render everything (all layers)
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

        // Render the scene
        captureCamera.Render();

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
        int nonBackgroundPixels = 0;

        // Sample every 10th pixel for performance
        for (int i = 0; i < pixels.Length; i += 10)
        {
            Color pixel = pixels[i];

            // Check if pixel is not the background color and not fully transparent
            if (pixel.a > 0.1f || !ColorApproximatelyEqual(pixel, backgroundColor))
            {
                nonBackgroundPixels++;
            }
        }

        float percentageNonBackground = (float)nonBackgroundPixels / (pixels.Length / 10) * 100f;
        Debug.Log($"DrawingCaptureHandler: Texture has {percentageNonBackground:F1}% non-background pixels");

        return nonBackgroundPixels > 10; // Need at least 10 non-background pixels
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
}
