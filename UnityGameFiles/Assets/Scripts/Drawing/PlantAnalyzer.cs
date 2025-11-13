using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Analyzes drawn plants and intuitively detects their type
/// Detects: Sunflower (Fire), Cactus (Grass), Water Lily (Water)
/// </summary>
public class PlantAnalyzer : MonoBehaviour
{
    public enum PlantType
    {
        Unknown,
        Sunflower,  // Fire type
        Cactus,     // Grass type
        WaterLily   // Water type
    }

    [System.Serializable]
    public class PlantAnalysisResult
    {
        public PlantType detectedType;
        public float confidence;
        public string elementType; // "Fire", "Grass", "Water"
        public Dictionary<PlantType, float> scores;

        public PlantAnalysisResult()
        {
            scores = new Dictionary<PlantType, float>();
        }

        public override string ToString()
        {
            return $"{detectedType} ({elementType}) - Confidence: {confidence:P0}";
        }
    }

    /// <summary>
    /// Analyze all strokes from DrawingCanvas and detect plant type
    /// </summary>
    public PlantAnalysisResult AnalyzeDrawing(List<LineRenderer> strokes)
    {
        if (strokes == null || strokes.Count == 0)
        {
            Debug.LogWarning("No strokes to analyze!");
            return CreateUnknownResult();
        }

        Debug.Log("=== PLANT ANALYSIS START ===");
        Debug.Log($"Analyzing {strokes.Count} strokes...");

        // Extract geometric features from all strokes
        DrawingFeatures features = ExtractFeatures(strokes);

        // Calculate scores for each plant type
        PlantAnalysisResult result = new PlantAnalysisResult();
        result.scores[PlantType.Sunflower] = CalculateSunflowerScore(features);
        result.scores[PlantType.Cactus] = CalculateCactusScore(features);
        result.scores[PlantType.WaterLily] = CalculateWaterLilyScore(features);

        // Determine best match
        var bestMatch = result.scores.OrderByDescending(x => x.Value).First();
        result.detectedType = bestMatch.Key;
        result.confidence = bestMatch.Value;
        result.elementType = GetElementType(result.detectedType);

        Debug.Log($"=== ANALYSIS COMPLETE ===");
        Debug.Log($"Sunflower Score: {result.scores[PlantType.Sunflower]:F2}");
        Debug.Log($"Cactus Score: {result.scores[PlantType.Cactus]:F2}");
        Debug.Log($"Water Lily Score: {result.scores[PlantType.WaterLily]:F2}");
        Debug.Log($"Result: {result}");

        return result;
    }

    /// <summary>
    /// Extract geometric features from drawing strokes
    /// </summary>
    private DrawingFeatures ExtractFeatures(List<LineRenderer> strokes)
    {
        DrawingFeatures features = new DrawingFeatures();

        List<Vector3> allPoints = new List<Vector3>();

        // Collect all points from all strokes
        foreach (var stroke in strokes)
        {
            if (stroke == null) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);
            allPoints.AddRange(positions);
        }

        if (allPoints.Count == 0) return features;

        // Calculate bounding box
        float minX = allPoints.Min(p => p.x);
        float maxX = allPoints.Max(p => p.x);
        float minY = allPoints.Min(p => p.y);
        float maxY = allPoints.Max(p => p.y);

        features.width = maxX - minX;
        features.height = maxY - minY;
        features.aspectRatio = features.height / Mathf.Max(features.width, 0.001f);
        features.center = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
        features.strokeCount = strokes.Count;

        Debug.Log($"Features: Width={features.width:F2}, Height={features.height:F2}, Aspect={features.aspectRatio:F2}, Strokes={features.strokeCount}");

        // Analyze stroke patterns
        features.circularStrokes = CountCircularStrokes(strokes);
        features.verticalStrokes = CountVerticalStrokes(strokes, features);
        features.horizontalStrokes = CountHorizontalStrokes(strokes, features);
        features.spikyStrokes = CountSpikyStrokes(strokes);
        features.curvedStrokes = CountCurvedStrokes(strokes);

        // Analyze stroke distribution
        features.radiatingPattern = DetectRadiatingPattern(strokes, features.center);
        features.centerMass = CalculateCenterMass(allPoints);

        Debug.Log($"Patterns: Circular={features.circularStrokes}, Vertical={features.verticalStrokes}, Horizontal={features.horizontalStrokes}");
        Debug.Log($"Patterns: Spiky={features.spikyStrokes}, Curved={features.curvedStrokes}, Radiating={features.radiatingPattern:F2}");

        return features;
    }

    /// <summary>
    /// Calculate how likely this is a Sunflower (Fire type)
    /// Characteristics: Round petals, circular center, radiating pattern
    /// </summary>
    private float CalculateSunflowerScore(DrawingFeatures f)
    {
        float score = 0f;

        // Sunflowers have near-square aspect ratio (circular overall shape)
        float aspectPenalty = Mathf.Abs(f.aspectRatio - 1f);
        score += Mathf.Max(0, 1f - aspectPenalty) * 0.25f;

        // Multiple strokes (petals)
        if (f.strokeCount >= 5) score += 0.2f;
        else if (f.strokeCount >= 3) score += 0.1f;

        // Circular or radiating pattern
        if (f.circularStrokes >= 1) score += 0.25f;
        score += f.radiatingPattern * 0.3f;

        // Curved strokes (petals are curved)
        score += Mathf.Min(f.curvedStrokes / (float)f.strokeCount, 1f) * 0.2f;

        // Penalty for vertical dominance (sunflowers are round, not tall)
        if (f.aspectRatio > 1.5f) score *= 0.5f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Calculate how likely this is a Cactus (Grass type)
    /// Characteristics: Tall vertical shape, spiky edges, narrow
    /// </summary>
    private float CalculateCactusScore(DrawingFeatures f)
    {
        float score = 0f;

        // Cactus is tall and narrow (high aspect ratio)
        if (f.aspectRatio > 1.5f) score += 0.35f;
        else if (f.aspectRatio > 1.2f) score += 0.2f;
        else if (f.aspectRatio > 1.0f) score += 0.1f;

        // Vertical strokes
        score += Mathf.Min(f.verticalStrokes / (float)f.strokeCount, 1f) * 0.25f;

        // Spiky strokes (thorns)
        score += Mathf.Min(f.spikyStrokes / (float)f.strokeCount, 1f) * 0.3f;

        // Fewer strokes than sunflower (cactus is simpler)
        if (f.strokeCount <= 5) score += 0.1f;

        // Penalty for circular patterns (cactus is not circular)
        if (f.circularStrokes > 0) score *= 0.6f;

        // Penalty for radiating pattern
        score -= f.radiatingPattern * 0.2f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Calculate how likely this is a Water Lily (Water type)
    /// Characteristics: Horizontal/wide, rounded leaves, floating pattern
    /// </summary>
    private float CalculateWaterLilyScore(DrawingFeatures f)
    {
        float score = 0f;

        // Water lily is wide and flat (low aspect ratio)
        if (f.aspectRatio < 0.8f) score += 0.35f;
        else if (f.aspectRatio < 1.0f) score += 0.2f;

        // Horizontal strokes
        score += Mathf.Min(f.horizontalStrokes / (float)f.strokeCount, 1f) * 0.25f;

        // Curved, smooth strokes (rounded leaves)
        score += Mathf.Min(f.curvedStrokes / (float)f.strokeCount, 1f) * 0.25f;

        // Circular elements (lily pads are round)
        if (f.circularStrokes >= 1) score += 0.15f;

        // Moderate stroke count (3-7 strokes typical)
        if (f.strokeCount >= 3 && f.strokeCount <= 7) score += 0.1f;

        // Penalty for spiky strokes (water lily is smooth)
        score -= Mathf.Min(f.spikyStrokes / (float)f.strokeCount, 1f) * 0.2f;

        // Penalty for vertical dominance
        if (f.aspectRatio > 1.3f) score *= 0.5f;

        return Mathf.Clamp01(score);
    }

    // ===== FEATURE DETECTION HELPERS =====

    private int CountCircularStrokes(List<LineRenderer> strokes)
    {
        int count = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 10) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            // Check if stroke forms a closed loop
            float startEndDist = Vector3.Distance(positions[0], positions[positions.Length - 1]);
            float totalLength = CalculateStrokeLength(positions);

            // If start and end are close relative to total length, it's circular
            if (startEndDist < totalLength * 0.3f && totalLength > 1f)
            {
                count++;
            }
        }
        return count;
    }

    private int CountVerticalStrokes(List<LineRenderer> strokes, DrawingFeatures features)
    {
        int count = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 3) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            // Calculate vertical vs horizontal extent
            float verticalExtent = Mathf.Abs(positions[positions.Length - 1].y - positions[0].y);
            float horizontalExtent = Mathf.Abs(positions[positions.Length - 1].x - positions[0].x);

            if (verticalExtent > horizontalExtent * 1.5f)
            {
                count++;
            }
        }
        return count;
    }

    private int CountHorizontalStrokes(List<LineRenderer> strokes, DrawingFeatures features)
    {
        int count = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 3) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            float verticalExtent = Mathf.Abs(positions[positions.Length - 1].y - positions[0].y);
            float horizontalExtent = Mathf.Abs(positions[positions.Length - 1].x - positions[0].x);

            if (horizontalExtent > verticalExtent * 1.5f)
            {
                count++;
            }
        }
        return count;
    }

    private int CountSpikyStrokes(List<LineRenderer> strokes)
    {
        int count = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 4) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            // Detect sharp direction changes (spikes)
            int sharpTurns = 0;
            for (int i = 1; i < positions.Length - 1; i++)
            {
                Vector3 dir1 = (positions[i] - positions[i - 1]).normalized;
                Vector3 dir2 = (positions[i + 1] - positions[i]).normalized;
                float angle = Vector3.Angle(dir1, dir2);

                if (angle > 90f) // Sharp turn
                {
                    sharpTurns++;
                }
            }

            // If stroke has multiple sharp turns, it's spiky
            if (sharpTurns >= 2)
            {
                count++;
            }
        }
        return count;
    }

    private int CountCurvedStrokes(List<LineRenderer> strokes)
    {
        int count = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 5) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            // Measure curvature - smooth curves have consistent angle changes
            float totalAngleChange = 0f;
            int angleCount = 0;

            for (int i = 1; i < positions.Length - 1; i++)
            {
                Vector3 dir1 = (positions[i] - positions[i - 1]).normalized;
                Vector3 dir2 = (positions[i + 1] - positions[i]).normalized;
                float angle = Vector3.Angle(dir1, dir2);

                totalAngleChange += angle;
                angleCount++;
            }

            float avgAngle = angleCount > 0 ? totalAngleChange / angleCount : 0f;

            // Curved strokes have moderate, consistent angle changes
            if (avgAngle > 5f && avgAngle < 45f)
            {
                count++;
            }
        }
        return count;
    }

    private float DetectRadiatingPattern(List<LineRenderer> strokes, Vector2 center)
    {
        if (strokes.Count < 3) return 0f;

        int radiatingStrokes = 0;

        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 2) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            // Check if stroke starts near center and radiates outward
            float startDist = Vector2.Distance(center, new Vector2(positions[0].x, positions[0].y));
            float endDist = Vector2.Distance(center, new Vector2(positions[positions.Length - 1].x, positions[positions.Length - 1].y));

            // Or vice versa (drawn from outside to center)
            if (Mathf.Abs(startDist - endDist) > 0.5f)
            {
                radiatingStrokes++;
            }
        }

        return radiatingStrokes / (float)strokes.Count;
    }

    private Vector2 CalculateCenterMass(List<Vector3> points)
    {
        if (points.Count == 0) return Vector2.zero;

        Vector2 sum = Vector2.zero;
        foreach (var p in points)
        {
            sum += new Vector2(p.x, p.y);
        }
        return sum / points.Count;
    }

    private float CalculateStrokeLength(Vector3[] positions)
    {
        float length = 0f;
        for (int i = 1; i < positions.Length; i++)
        {
            length += Vector3.Distance(positions[i - 1], positions[i]);
        }
        return length;
    }

    private string GetElementType(PlantType type)
    {
        switch (type)
        {
            case PlantType.Sunflower: return "Fire";
            case PlantType.Cactus: return "Grass";
            case PlantType.WaterLily: return "Water";
            default: return "Unknown";
        }
    }

    private PlantAnalysisResult CreateUnknownResult()
    {
        return new PlantAnalysisResult
        {
            detectedType = PlantType.Unknown,
            confidence = 0f,
            elementType = "Unknown"
        };
    }

    // ===== FEATURE DATA STRUCTURE =====

    private class DrawingFeatures
    {
        public float width;
        public float height;
        public float aspectRatio; // height/width
        public Vector2 center;
        public int strokeCount;

        public int circularStrokes;
        public int verticalStrokes;
        public int horizontalStrokes;
        public int spikyStrokes;
        public int curvedStrokes;

        public float radiatingPattern; // 0-1 score
        public Vector2 centerMass;
    }
}
