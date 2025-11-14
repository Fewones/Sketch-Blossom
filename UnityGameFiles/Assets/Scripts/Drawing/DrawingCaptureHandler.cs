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
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0); // Transparent background

    /// <summary>
    /// Capture all LineRenderers and convert them to a Texture2D
    /// </summary>
    public Texture2D CaptureDrawing(List<LineRenderer> strokes, Camera sourceCamera)
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
        captureCamera.cullingMask = sourceCamera.cullingMask; // Use same layers as main camera

        // Position camera to frame the drawing
        FrameDrawing(captureCamera, drawingBounds);

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
}
