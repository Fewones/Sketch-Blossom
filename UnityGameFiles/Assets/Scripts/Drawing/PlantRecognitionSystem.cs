using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Complete plant recognition system
/// Analyzes color (element) and shape (plant type) from drawings
/// </summary>
public class PlantRecognitionSystem : MonoBehaviour
{
    public enum ElementType
    {
        Fire,    // Red
        Grass,   // Green
        Water    // Blue
    }

    public enum PlantType
    {
        // Fire Plants (Red)
        Sunflower,
        FireRose,
        FlameTulip,

        // Grass Plants (Green)
        Cactus,
        VineFlower,
        GrassSprout,

        // Water Plants (Blue)
        WaterLily,
        CoralBloom,
        BubbleFlower
    }

    [System.Serializable]
    public class PlantData
    {
        public PlantType type;
        public ElementType element;
        public string displayName;
        public int baseHP;
        public int baseAttack;
        public int baseDefense;
        public string description;

        public PlantData(PlantType type, ElementType element, string name, int hp, int atk, int def, string desc)
        {
            this.type = type;
            this.element = element;
            this.displayName = name;
            this.baseHP = hp;
            this.baseAttack = atk;
            this.baseDefense = def;
            this.description = desc;
        }
    }

    [System.Serializable]
    public class RecognitionResult
    {
        public PlantType plantType;
        public ElementType element;
        public PlantData plantData;
        public float confidence;
        public Color dominantColor;

        public RecognitionResult(PlantType type, ElementType elem, PlantData data, float conf, Color color)
        {
            plantType = type;
            element = elem;
            plantData = data;
            confidence = conf;
            dominantColor = color;
        }
    }

    // Plant database with fixed stats
    private static Dictionary<PlantType, PlantData> plantDatabase;

    private void Awake()
    {
        InitializePlantDatabase();
    }

    private void InitializePlantDatabase()
    {
        plantDatabase = new Dictionary<PlantType, PlantData>
        {
            // FIRE PLANTS (Red) - High attack, medium HP, low defense
            { PlantType.Sunflower, new PlantData(
                PlantType.Sunflower, ElementType.Fire, "Sunflower",
                hp: 30, atk: 18, def: 8,
                "Radiant fire flower with circular petals"
            )},
            { PlantType.FireRose, new PlantData(
                PlantType.FireRose, ElementType.Fire, "Fire Rose",
                hp: 35, atk: 16, def: 10,
                "Compact blazing rose with many layers"
            )},
            { PlantType.FlameTulip, new PlantData(
                PlantType.FlameTulip, ElementType.Fire, "Flame Tulip",
                hp: 28, atk: 20, def: 6,
                "Tall elegant flame flower with simple shape"
            )},

            // GRASS PLANTS (Green) - Balanced stats
            { PlantType.Cactus, new PlantData(
                PlantType.Cactus, ElementType.Grass, "Cactus",
                hp: 32, atk: 12, def: 14,
                "Spiky vertical desert guardian"
            )},
            { PlantType.VineFlower, new PlantData(
                PlantType.VineFlower, ElementType.Grass, "Vine Flower",
                hp: 35, atk: 14, def: 12,
                "Flowing curved vine with blooms"
            )},
            { PlantType.GrassSprout, new PlantData(
                PlantType.GrassSprout, ElementType.Grass, "Grass Sprout",
                hp: 30, atk: 10, def: 16,
                "Bushy low-growing grass cluster"
            )},

            // WATER PLANTS (Blue) - High HP, low attack, high defense
            { PlantType.WaterLily, new PlantData(
                PlantType.WaterLily, ElementType.Water, "Water Lily",
                hp: 40, atk: 10, def: 14,
                "Peaceful floating flower spreading wide"
            )},
            { PlantType.CoralBloom, new PlantData(
                PlantType.CoralBloom, ElementType.Water, "Coral Bloom",
                hp: 38, atk: 12, def: 12,
                "Branching underwater coral flower"
            )},
            { PlantType.BubbleFlower, new PlantData(
                PlantType.BubbleFlower, ElementType.Water, "Bubble Flower",
                hp: 36, atk: 8, def: 16,
                "Clustered bubble-like blooms"
            )}
        };
    }

    /// <summary>
    /// Analyze drawn strokes and recognize the plant
    /// </summary>
    public RecognitionResult AnalyzeDrawing(List<LineRenderer> strokes, Color dominantColor)
    {
        if (strokes == null || strokes.Count == 0)
        {
            Debug.LogWarning("No strokes to analyze!");
            return CreateDefaultResult(dominantColor);
        }

        Debug.Log("=== PLANT RECOGNITION START ===");

        // Step 1: Determine element from color
        ElementType element = DetermineElementFromColor(dominantColor);
        Debug.Log($"Step 1: Element detected = {element} (from color: {dominantColor})");

        // Step 2: Analyze shape features
        ShapeFeatures features = AnalyzeShapeFeatures(strokes);
        LogShapeFeatures(features);

        // Step 3: Determine plant type based on element and shape
        PlantType plantType = DeterminePlantType(element, features);
        Debug.Log($"Step 3: Plant type detected = {plantType}");

        // Step 4: Calculate confidence
        float confidence = CalculateConfidence(features);
        Debug.Log($"Step 4: Confidence = {confidence:P0}");

        // Step 5: Get plant data and create result
        PlantData data = plantDatabase[plantType];
        RecognitionResult result = new RecognitionResult(plantType, element, data, confidence, dominantColor);

        Debug.Log("=== PLANT RECOGNITION COMPLETE ===");
        LogResult(result);

        return result;
    }

    private ElementType DetermineElementFromColor(Color color)
    {
        if (color.r > color.g && color.r > color.b)
            return ElementType.Fire;
        else if (color.g > color.r && color.g > color.b)
            return ElementType.Grass;
        else if (color.b > color.r && color.b > color.g)
            return ElementType.Water;
        else
            return ElementType.Grass; // Default
    }

    private class ShapeFeatures
    {
        public float aspectRatio;        // Height / Width
        public float compactness;        // How circular/clustered
        public float curviness;          // How curved vs straight
        public int strokeCount;
        public float avgStrokeLength;
        public bool isVertical;          // Taller than wide
        public bool isHorizontal;        // Wider than tall
        public bool isRadial;            // Strokes from center
        public bool isBranching;         // Multiple branches
    }

    private ShapeFeatures AnalyzeShapeFeatures(List<LineRenderer> strokes)
    {
        ShapeFeatures features = new ShapeFeatures();

        // Calculate bounding box
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        float totalLength = 0f;

        foreach (var stroke in strokes)
        {
            if (stroke == null) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            // Update bounding box
            foreach (var pos in positions)
            {
                min.x = Mathf.Min(min.x, pos.x);
                min.y = Mathf.Min(min.y, pos.y);
                max.x = Mathf.Max(max.x, pos.x);
                max.y = Mathf.Max(max.y, pos.y);
            }

            // Calculate stroke length
            for (int i = 1; i < positions.Length; i++)
            {
                totalLength += Vector3.Distance(positions[i - 1], positions[i]);
            }
        }

        float width = max.x - min.x;
        float height = max.y - min.y;

        features.strokeCount = strokes.Count;
        features.avgStrokeLength = totalLength / Mathf.Max(1, strokes.Count);
        features.aspectRatio = height / Mathf.Max(0.1f, width);
        features.isVertical = features.aspectRatio > 1.3f;
        features.isHorizontal = features.aspectRatio < 0.7f;

        // Calculate compactness (how close strokes are to center)
        Vector2 center = (min + max) / 2f;
        float avgDistanceFromCenter = 0f;
        int pointCount = 0;

        foreach (var stroke in strokes)
        {
            if (stroke == null) continue;
            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            foreach (var pos in positions)
            {
                avgDistanceFromCenter += Vector2.Distance(new Vector2(pos.x, pos.y), center);
                pointCount++;
            }
        }

        avgDistanceFromCenter /= Mathf.Max(1, pointCount);
        float maxDimension = Mathf.Max(width, height);
        features.compactness = 1f - (avgDistanceFromCenter / Mathf.Max(0.1f, maxDimension));

        // Calculate curviness (direction changes)
        float totalAngleChange = 0f;
        int segmentCount = 0;

        foreach (var stroke in strokes)
        {
            if (stroke == null) continue;
            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            for (int i = 2; i < positions.Length; i++)
            {
                Vector2 dir1 = new Vector2(positions[i - 1].x - positions[i - 2].x, positions[i - 1].y - positions[i - 2].y);
                Vector2 dir2 = new Vector2(positions[i].x - positions[i - 1].x, positions[i].y - positions[i - 1].y);

                if (dir1.magnitude > 0.01f && dir2.magnitude > 0.01f)
                {
                    float angle = Vector2.Angle(dir1, dir2);
                    totalAngleChange += angle;
                    segmentCount++;
                }
            }
        }

        features.curviness = totalAngleChange / Mathf.Max(1, segmentCount);

        // Detect radial pattern (many strokes from center)
        features.isRadial = features.strokeCount >= 5 && features.compactness > 0.3f;

        // Detect branching (multiple short strokes)
        features.isBranching = features.strokeCount >= 4 && features.avgStrokeLength < 3f;

        return features;
    }

    private PlantType DeterminePlantType(ElementType element, ShapeFeatures features)
    {
        switch (element)
        {
            case ElementType.Fire:
                return DetermineFirePlant(features);
            case ElementType.Grass:
                return DetermineGrassPlant(features);
            case ElementType.Water:
                return DetermineWaterPlant(features);
            default:
                return PlantType.GrassSprout;
        }
    }

    private PlantType DetermineFirePlant(ShapeFeatures features)
    {
        // Sunflower: Radial pattern, circular
        if (features.isRadial && features.compactness > 0.4f)
            return PlantType.Sunflower;

        // Fire Rose: Compact, many strokes, circular
        if (features.strokeCount >= 6 && features.compactness > 0.5f)
            return PlantType.FireRose;

        // Flame Tulip: Vertical, simple shape (few strokes)
        if (features.isVertical || features.strokeCount <= 3)
            return PlantType.FlameTulip;

        // Default
        return PlantType.Sunflower;
    }

    private PlantType DetermineGrassPlant(ShapeFeatures features)
    {
        // Cactus: Very vertical, straight lines
        if (features.isVertical && features.curviness < 30f)
            return PlantType.Cactus;

        // Vine Flower: Curved, flowing
        if (features.curviness > 40f && !features.isHorizontal)
            return PlantType.VineFlower;

        // Grass Sprout: Short, bushy, many small strokes
        if (features.strokeCount >= 5 || features.aspectRatio < 1.2f)
            return PlantType.GrassSprout;

        // Default
        return PlantType.Cactus;
    }

    private PlantType DetermineWaterPlant(ShapeFeatures features)
    {
        // Water Lily: Horizontal, spreading
        if (features.isHorizontal)
            return PlantType.WaterLily;

        // Coral Bloom: Branching pattern
        if (features.isBranching || features.strokeCount >= 6)
            return PlantType.CoralBloom;

        // Bubble Flower: Compact, clustered circles
        if (features.compactness > 0.5f || features.strokeCount >= 4)
            return PlantType.BubbleFlower;

        // Default
        return PlantType.WaterLily;
    }

    private float CalculateConfidence(ShapeFeatures features)
    {
        // Base confidence on stroke count and features
        float confidence = 0.5f;

        if (features.strokeCount >= 3)
            confidence += 0.2f;
        if (features.strokeCount >= 5)
            confidence += 0.15f;
        if (features.avgStrokeLength > 2f)
            confidence += 0.15f;

        return Mathf.Clamp01(confidence);
    }

    private RecognitionResult CreateDefaultResult(Color color)
    {
        ElementType element = DetermineElementFromColor(color);
        PlantType defaultType = element == ElementType.Fire ? PlantType.Sunflower :
                                element == ElementType.Grass ? PlantType.Cactus :
                                PlantType.WaterLily;

        PlantData data = plantDatabase[defaultType];
        return new RecognitionResult(defaultType, element, data, 0.3f, color);
    }

    private void LogShapeFeatures(ShapeFeatures features)
    {
        Debug.Log($"Shape Features:");
        Debug.Log($"  - Aspect Ratio: {features.aspectRatio:F2} (V:{features.isVertical}, H:{features.isHorizontal})");
        Debug.Log($"  - Compactness: {features.compactness:F2}");
        Debug.Log($"  - Curviness: {features.curviness:F1}°");
        Debug.Log($"  - Strokes: {features.strokeCount}, Avg Length: {features.avgStrokeLength:F2}");
        Debug.Log($"  - Radial: {features.isRadial}, Branching: {features.isBranching}");
    }

    private void LogResult(RecognitionResult result)
    {
        Debug.Log($"📋 FINAL RESULT:");
        Debug.Log($"   🌱 Plant: {result.plantData.displayName} ({result.plantType})");
        Debug.Log($"   🔥 Element: {result.element}");
        Debug.Log($"   ⭐ Confidence: {result.confidence:P0}");
        Debug.Log($"   💚 Stats: HP={result.plantData.baseHP}, ATK={result.plantData.baseAttack}, DEF={result.plantData.baseDefense}");
    }

    public static PlantData GetPlantData(PlantType type)
    {
        if (plantDatabase == null)
        {
            // Initialize if called statically before Awake
            var temp = new PlantRecognitionSystem();
            temp.InitializePlantDatabase();
        }
        return plantDatabase[type];
    }
}
