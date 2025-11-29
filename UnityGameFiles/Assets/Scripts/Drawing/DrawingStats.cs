using UnityEngine;

/// <summary>
/// Summary information about a drawing on the canvas
/// </summary>
[System.Serializable]
public struct DrawingStats
{
    // How many separate strokes / lines the player drew
    public int strokeCount;

    // Sum of all stroke segment lengths (in canvas space)
    public float totalLength;

    // Area of the bounding box that contains all strokes
    public float boundingBoxArea;

    // Area of the whole canvas (so we can estimate coverage)
    public float canvasArea;
}
