using UnityEngine;

/// <summary>
/// Stores the stats and properties of a unit created from drawing
/// This persists between scenes using DontDestroyOnLoad
/// </summary>
public class DrawnUnitData : MonoBehaviour
{
    public static DrawnUnitData Instance { get; private set; }

    [Header("Unit Stats (Generated from Drawing)")]
    public int attack = 10;
    public int defense = 10;
    public int health = 30;

    [Header("Plant Type")]
    public PlantAnalyzer.PlantType plantType = PlantAnalyzer.PlantType.Unknown;
    public string elementType = "Unknown"; // "Fire", "Grass", "Water"
    public float detectionConfidence = 0f;

    [Header("Drawing Properties")]
    public int strokeCount = 0;
    public float totalDrawingLength = 0f;
    public int totalPoints = 0;

    [Header("Visual Data")]
    public Texture2D drawingTexture; // Store the drawing as a texture
    public Color unitColor = Color.green;

    private void Awake()
    {
        // Singleton pattern - only one instance persists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Set unit stats from drawing analysis
    /// </summary>
    public void SetStatsFromDrawing(int strokes, float length, int points)
    {
        strokeCount = strokes;
        totalDrawingLength = length;
        totalPoints = points;

        // Calculate stats based on drawing properties
        // You can tune these formulas
        attack = Mathf.Clamp(strokes * 5, 5, 30);
        defense = Mathf.Clamp((int)(length * 10), 5, 25);
        health = Mathf.Clamp(points / 2, 20, 60);

        Debug.Log($"Unit Stats Generated - ATK: {attack}, DEF: {defense}, HP: {health}");
    }

    /// <summary>
    /// Alternative: Set stats directly (for testing or custom units)
    /// </summary>
    public void SetStats(int atk, int def, int hp)
    {
        attack = atk;
        defense = def;
        health = hp;
    }

    /// <summary>
    /// Set plant type from analysis
    /// </summary>
    public void SetPlantType(PlantAnalyzer.PlantAnalysisResult result)
    {
        if (result != null)
        {
            plantType = result.detectedType;
            elementType = result.elementType;
            detectionConfidence = result.confidence;

            Debug.Log($"Plant Type Set: {plantType} ({elementType}) - {detectionConfidence:P0} confidence");
        }
    }

    /// <summary>
    /// Clear data when starting a new run
    /// </summary>
    public void ClearData()
    {
        attack = 10;
        defense = 10;
        health = 30;
        strokeCount = 0;
        totalDrawingLength = 0f;
        totalPoints = 0;
        drawingTexture = null;
        plantType = PlantAnalyzer.PlantType.Unknown;
        elementType = "Unknown";
        detectionConfidence = 0f;
    }
}
