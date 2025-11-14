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

        // New robust detection features
        public int redCircleCount;       // Number of red circular strokes
        public int greenLineCount;       // Number of green line strokes
        public int overlappingRedStrokes; // Number of overlapping red strokes
        public int verticalRedStrokes;   // Number of vertical red strokes
        public int longVerticalRedStrokes; // Number of long vertical red strokes
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

        // NEW ROBUST DETECTION: Analyze strokes by color and shape
        List<LineRenderer> redStrokes = new List<LineRenderer>();
        List<LineRenderer> greenStrokes = new List<LineRenderer>();

        // Separate strokes by color
        foreach (var stroke in strokes)
        {
            if (stroke == null) continue;

            Color strokeColor = GetStrokeColor(stroke);
            if (IsColorRed(strokeColor))
            {
                redStrokes.Add(stroke);
            }
            else if (IsColorGreen(strokeColor))
            {
                greenStrokes.Add(stroke);
            }
        }

        // Count red circles
        features.redCircleCount = 0;
        foreach (var stroke in redStrokes)
        {
            if (IsStrokeCircular(stroke))
            {
                features.redCircleCount++;
            }
        }

        // Count green lines (non-circular green strokes)
        features.greenLineCount = 0;
        foreach (var stroke in greenStrokes)
        {
            if (!IsStrokeCircular(stroke))
            {
                features.greenLineCount++;
            }
        }

        // Count overlapping red strokes
        features.overlappingRedStrokes = 0;
        for (int i = 0; i < redStrokes.Count; i++)
        {
            for (int j = i + 1; j < redStrokes.Count; j++)
            {
                if (DoStrokesOverlap(redStrokes[i], redStrokes[j]))
                {
                    features.overlappingRedStrokes++;
                }
            }
        }

        // Count vertical red strokes
        features.verticalRedStrokes = 0;
        features.longVerticalRedStrokes = 0;
        const float LONG_STROKE_THRESHOLD = 2.0f; // Minimum length to be considered "long"

        foreach (var stroke in redStrokes)
        {
            float strokeLength;
            if (IsStrokeVertical(stroke, out strokeLength))
            {
                features.verticalRedStrokes++;
                if (strokeLength >= LONG_STROKE_THRESHOLD)
                {
                    features.longVerticalRedStrokes++;
                }
            }
        }

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
        Debug.Log("=== ROBUST FIRE PLANT DETECTION ===");

        // ROBUST DETECTION LOGIC:
        // Sunflower: Roughly 4 or more red circles + one green line
        bool isSunflower = features.redCircleCount >= 4 && features.greenLineCount >= 1;
        Debug.Log($"Sunflower check: {features.redCircleCount} red circles, {features.greenLineCount} green lines -> {isSunflower}");

        if (isSunflower)
        {
            Debug.Log("✓ Detected: SUNFLOWER (4+ red circles + green line)");
            return PlantType.Sunflower;
        }

        // Fire Rose: At least 5 overlapping red strokes + one green line
        bool isFireRose = features.overlappingRedStrokes >= 5 && features.greenLineCount >= 1;
        Debug.Log($"Fire Rose check: {features.overlappingRedStrokes} overlapping red strokes, {features.greenLineCount} green lines -> {isFireRose}");

        if (isFireRose)
        {
            Debug.Log("✓ Detected: FIRE ROSE (5+ overlapping red strokes + green line)");
            return PlantType.FireRose;
        }

        // Flame Tulip: 3 long enough mostly vertical red strokes
        bool isFlameTulip = features.longVerticalRedStrokes >= 3;
        Debug.Log($"Flame Tulip check: {features.longVerticalRedStrokes} long vertical red strokes -> {isFlameTulip}");

        if (isFlameTulip)
        {
            Debug.Log("✓ Detected: FLAME TULIP (3+ long vertical red strokes)");
            return PlantType.FlameTulip;
        }

        // FALLBACK: Use legacy detection for edge cases
        Debug.Log("No robust match found, using fallback detection...");

        // Fallback Sunflower: Radial pattern, circular
        if (features.isRadial && features.compactness > 0.4f)
        {
            Debug.Log("✓ Fallback: SUNFLOWER (radial pattern)");
            return PlantType.Sunflower;
        }

        // Fallback Fire Rose: Compact, many strokes, circular
        if (features.strokeCount >= 6 && features.compactness > 0.5f)
        {
            Debug.Log("✓ Fallback: FIRE ROSE (many compact strokes)");
            return PlantType.FireRose;
        }

        // Fallback Flame Tulip: Vertical, simple shape (few strokes)
        if (features.isVertical || features.strokeCount <= 3)
        {
            Debug.Log("✓ Fallback: FLAME TULIP (vertical or simple)");
            return PlantType.FlameTulip;
        }

        // Default to Sunflower
        Debug.Log("✓ Default: SUNFLOWER");
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

    /// <summary>
    /// Checks if a stroke is circular (closed loop with round shape)
    /// </summary>
    private bool IsStrokeCircular(LineRenderer stroke)
    {
        if (stroke == null || stroke.positionCount < 10) return false;

        Vector3[] positions = new Vector3[stroke.positionCount];
        stroke.GetPositions(positions);

        // Check if start and end points are close (closed loop)
        float closedDistance = Vector3.Distance(positions[0], positions[positions.Length - 1]);

        // Calculate the perimeter
        float perimeter = 0f;
        for (int i = 1; i < positions.Length; i++)
        {
            perimeter += Vector3.Distance(positions[i - 1], positions[i]);
        }

        // A closed loop should have start/end close together
        if (closedDistance > perimeter * 0.2f) return false;

        // Calculate center and average radius
        Vector2 center = Vector2.zero;
        foreach (var pos in positions)
        {
            center += new Vector2(pos.x, pos.y);
        }
        center /= positions.Length;

        // Calculate radius variance (how consistent the radius is)
        float avgRadius = 0f;
        foreach (var pos in positions)
        {
            avgRadius += Vector2.Distance(new Vector2(pos.x, pos.y), center);
        }
        avgRadius /= positions.Length;

        float radiusVariance = 0f;
        foreach (var pos in positions)
        {
            float radius = Vector2.Distance(new Vector2(pos.x, pos.y), center);
            radiusVariance += Mathf.Abs(radius - avgRadius);
        }
        radiusVariance /= positions.Length;

        // A circle should have low radius variance
        float varianceRatio = radiusVariance / Mathf.Max(0.01f, avgRadius);
        return varianceRatio < 0.3f; // 30% variance threshold
    }

    /// <summary>
    /// Checks if a stroke is mostly vertical
    /// </summary>
    private bool IsStrokeVertical(LineRenderer stroke, out float strokeLength)
    {
        strokeLength = 0f;
        if (stroke == null || stroke.positionCount < 2) return false;

        Vector3[] positions = new Vector3[stroke.positionCount];
        stroke.GetPositions(positions);

        // Calculate bounding box
        float minY = float.MaxValue, maxY = float.MinValue;
        float minX = float.MaxValue, maxX = float.MinValue;

        foreach (var pos in positions)
        {
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
        }

        float height = maxY - minY;
        float width = maxX - minX;
        strokeLength = height;

        // Stroke is vertical if height > width * 1.5
        return height > width * 1.5f;
    }

    /// <summary>
    /// Checks if two strokes overlap (have points close to each other)
    /// </summary>
    private bool DoStrokesOverlap(LineRenderer stroke1, LineRenderer stroke2, float threshold = 0.5f)
    {
        if (stroke1 == null || stroke2 == null) return false;

        Vector3[] positions1 = new Vector3[stroke1.positionCount];
        Vector3[] positions2 = new Vector3[stroke2.positionCount];
        stroke1.GetPositions(positions1);
        stroke2.GetPositions(positions2);

        // Sample points to check for overlaps
        int sampleRate = Mathf.Max(1, positions1.Length / 5); // Sample ~5 points
        int overlapCount = 0;
        int sampledPoints = 0;

        for (int i = 0; i < positions1.Length; i += sampleRate)
        {
            sampledPoints++;
            Vector2 point1 = new Vector2(positions1[i].x, positions1[i].y);

            // Check if any point in stroke2 is close to this point
            foreach (var pos2 in positions2)
            {
                Vector2 point2 = new Vector2(pos2.x, pos2.y);
                if (Vector2.Distance(point1, point2) < threshold)
                {
                    overlapCount++;
                    break;
                }
            }
        }

        // Consider overlapping if at least 40% of sampled points are close
        return (float)overlapCount / sampledPoints >= 0.4f;
    }

    /// <summary>
    /// Gets the color of a stroke
    /// </summary>
    private Color GetStrokeColor(LineRenderer stroke)
    {
        if (stroke == null) return Color.white;

        // Get color from the material or gradient
        if (stroke.material != null && stroke.material.HasProperty("_Color"))
        {
            return stroke.material.GetColor("_Color");
        }

        // Fallback to gradient start color
        return stroke.startColor;
    }

    /// <summary>
    /// Determines if a color is primarily red
    /// </summary>
    private bool IsColorRed(Color color)
    {
        return color.r > 0.5f && color.r > color.g && color.r > color.b;
    }

    /// <summary>
    /// Determines if a color is primarily green
    /// </summary>
    private bool IsColorGreen(Color color)
    {
        return color.g > 0.5f && color.g > color.r && color.g > color.b;
    }

    private void LogShapeFeatures(ShapeFeatures features)
    {
        Debug.Log($"Shape Features:");
        Debug.Log($"  - Aspect Ratio: {features.aspectRatio:F2} (V:{features.isVertical}, H:{features.isHorizontal})");
        Debug.Log($"  - Compactness: {features.compactness:F2}");
        Debug.Log($"  - Curviness: {features.curviness:F1}°");
        Debug.Log($"  - Strokes: {features.strokeCount}, Avg Length: {features.avgStrokeLength:F2}");
        Debug.Log($"  - Radial: {features.isRadial}, Branching: {features.isBranching}");
        Debug.Log($"  - Red Circles: {features.redCircleCount}, Green Lines: {features.greenLineCount}");
        Debug.Log($"  - Overlapping Red Strokes: {features.overlappingRedStrokes}");
        Debug.Log($"  - Vertical Red Strokes: {features.verticalRedStrokes}, Long Vertical: {features.longVerticalRedStrokes}");
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
