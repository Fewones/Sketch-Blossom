using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Analyzes move drawings and calculates quality scores for damage scaling
/// The closer the drawing matches the ideal pattern, the more damage it deals
/// </summary>
public class MoveRecognitionSystem : MonoBehaviour
{
    [Header("Quality Thresholds")]
    [Tooltip("Minimum quality multiplier (even for poor drawings)")]
    public float minQualityMultiplier = 0.5f;

    [Tooltip("Maximum quality multiplier (for perfect drawings)")]
    public float maxQualityMultiplier = 1.5f;

    [Header("Balance Settings")]
    [Tooltip("How forgiving the quality system is (higher = more forgiving)")]
    [Range(0.1f, 2f)]
    public float forgivenessFactor = 1.0f;

    /// <summary>
    /// Result of move recognition with quality scoring
    /// </summary>
    public class MoveRecognitionResult
    {
        public MoveData.MoveType detectedMove;
        public float confidence;        // How confident we are this is the right move (0-1)
        public float quality;           // How well drawn it is (0-1)
        public float damageMultiplier;  // Final damage multiplier to apply
        public string qualityRating;    // User-friendly quality description

        public MoveRecognitionResult(MoveData.MoveType move, float confidence, float quality, float damageMultiplier, string rating)
        {
            this.detectedMove = move;
            this.confidence = confidence;
            this.quality = quality;
            this.damageMultiplier = damageMultiplier;
            this.qualityRating = rating;
        }
    }

    /// <summary>
    /// Analyze a drawn move and return recognition result with quality scoring
    /// </summary>
    public MoveRecognitionResult AnalyzeMove(List<LineRenderer> strokes, MoveData.MoveType expectedMove)
    {
        if (strokes == null || strokes.Count == 0)
        {
            return new MoveRecognitionResult(expectedMove, 0f, 0f, minQualityMultiplier, "Failed");
        }

        // Calculate shape features
        ShapeFeatures features = AnalyzeShapeFeatures(strokes);

        // Calculate quality based on move type
        float quality = CalculateMoveQuality(expectedMove, features);

        // Apply forgiveness factor
        quality = Mathf.Pow(quality, 1f / forgivenessFactor);
        quality = Mathf.Clamp01(quality);

        // Calculate damage multiplier
        float damageMultiplier = Mathf.Lerp(minQualityMultiplier, maxQualityMultiplier, quality);

        // Get quality rating
        string qualityRating = GetQualityRating(quality);

        // Confidence is high if we have reasonable stroke data
        float confidence = Mathf.Clamp01(strokes.Count / 5f);

        return new MoveRecognitionResult(expectedMove, confidence, quality, damageMultiplier, qualityRating);
    }

    /// <summary>
    /// Calculate quality score for a specific move type
    /// </summary>
    private float CalculateMoveQuality(MoveData.MoveType moveType, ShapeFeatures features)
    {
        switch (moveType)
        {
            case MoveData.MoveType.Block:
                return CalculateBlockQuality(features);

            case MoveData.MoveType.Fireball:
            case MoveData.MoveType.Bubble:
                return CalculateBallQuality(features);

            case MoveData.MoveType.FlameWave:
            case MoveData.MoveType.VineWhip:
                return CalculateWaveQuality(features);

            case MoveData.MoveType.Burn:
                return CalculateFireQuality(features);

            case MoveData.MoveType.RootAttack:
            case MoveData.MoveType.LeafStorm:
                return CalculatePlantGrowthQuality(features);

            case MoveData.MoveType.WaterSplash:
            case MoveData.MoveType.HealingWave:
                return CalculateWaterAttackQuality(features);

            default:
                // Generic quality calculation
                return CalculateGenericQuality(features);
        }
    }

    #region Move-Specific Quality Calculations

    /// <summary>
    /// Block: Very easy - should be a simple closed shape (circle, square)
    /// </summary>
    private float CalculateBlockQuality(ShapeFeatures features)
    {
        float score = 0f;

        // Prefer compact, closed shapes
        float compactnessScore = Mathf.Clamp01(features.compactness * 2f);
        score += compactnessScore * 0.4f;

        // Prefer few strokes (1-3 strokes is ideal)
        float strokeScore = 1f - Mathf.Clamp01((features.strokeCount - 2) / 5f);
        score += strokeScore * 0.3f;

        // Prefer moderate smoothness (not too jagged)
        float smoothnessScore = 1f - Mathf.Abs(features.curviness - 0.5f) * 2f;
        score += smoothnessScore * 0.3f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Ball attacks: Should be circular/round
    /// </summary>
    private float CalculateBallQuality(ShapeFeatures features)
    {
        float score = 0f;

        // Prefer circular aspect ratio (close to 1:1)
        float aspectScore = 1f - Mathf.Abs(1f - features.aspectRatio);
        score += aspectScore * 0.4f;

        // Prefer high compactness (round shape)
        score += features.compactness * 0.4f;

        // Prefer smooth curves
        score += features.curviness * 0.2f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Wave attacks: Should be horizontal and wavy
    /// </summary>
    private float CalculateWaveQuality(ShapeFeatures features)
    {
        float score = 0f;

        // Prefer wide aspect ratio (horizontal)
        float aspectScore = Mathf.Clamp01(features.aspectRatio - 1f);
        score += aspectScore * 0.4f;

        // Prefer low compactness (spread out)
        float spreadScore = 1f - features.compactness;
        score += spreadScore * 0.3f;

        // Prefer curved lines
        score += features.curviness * 0.3f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Fire attacks: Should have upward, flame-like qualities
    /// </summary>
    private float CalculateFireQuality(ShapeFeatures features)
    {
        float score = 0f;

        // Prefer tall aspect ratio (vertical flames)
        float tallScore = Mathf.Clamp01((1f / features.aspectRatio) - 0.5f);
        score += tallScore * 0.3f;

        // Prefer multiple strokes (flickering flames)
        float strokeScore = Mathf.Clamp01(features.strokeCount / 5f);
        score += strokeScore * 0.3f;

        // Prefer jagged/spiky (not too smooth)
        float jaggednessScore = Mathf.Clamp01(1f - features.curviness);
        score += jaggednessScore * 0.2f;

        // Prefer radial patterns (flames spreading)
        score += features.radialness * 0.2f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Plant growth: Should have branching, organic qualities
    /// </summary>
    private float CalculatePlantGrowthQuality(ShapeFeatures features)
    {
        float score = 0f;

        // Prefer branching patterns
        score += features.branchiness * 0.4f;

        // Prefer multiple strokes (branches/leaves)
        float strokeScore = Mathf.Clamp01(features.strokeCount / 4f);
        score += strokeScore * 0.3f;

        // Prefer smooth, organic curves
        score += features.curviness * 0.3f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Water attacks: Should be fluid and flowing
    /// </summary>
    private float CalculateWaterAttackQuality(ShapeFeatures features)
    {
        float score = 0f;

        // Prefer smooth, flowing curves
        score += features.curviness * 0.4f;

        // Prefer moderate aspect ratio
        float aspectScore = 1f - Mathf.Abs(1.5f - features.aspectRatio) / 2f;
        score += aspectScore * 0.3f;

        // Prefer multiple flowing strokes
        float strokeScore = Mathf.Clamp01(features.strokeCount / 4f);
        score += strokeScore * 0.3f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Generic quality calculation for unspecified moves
    /// </summary>
    private float CalculateGenericQuality(ShapeFeatures features)
    {
        // Balanced scoring - rewards any reasonable drawing
        float score = 0f;

        score += features.compactness * 0.3f;
        score += Mathf.Clamp01(features.strokeCount / 4f) * 0.3f;
        score += features.curviness * 0.2f;
        score += (features.branchiness + features.radialness) * 0.2f;

        return Mathf.Clamp01(score);
    }

    #endregion

    #region Shape Analysis

    private class ShapeFeatures
    {
        public int strokeCount;
        public float aspectRatio;
        public float compactness;
        public float curviness;
        public float radialness;
        public float branchiness;
        public float totalLength;
        public Bounds bounds;
    }

    private ShapeFeatures AnalyzeShapeFeatures(List<LineRenderer> strokes)
    {
        ShapeFeatures features = new ShapeFeatures();

        if (strokes == null || strokes.Count == 0)
            return features;

        features.strokeCount = strokes.Count;

        // Calculate bounds
        features.bounds = CalculateBounds(strokes);

        // Calculate aspect ratio
        float width = features.bounds.size.x;
        float height = features.bounds.size.y;
        features.aspectRatio = height > 0 ? width / height : 1f;

        // Calculate total length and curviness
        features.totalLength = 0f;
        float totalCurviness = 0f;

        foreach (var stroke in strokes)
        {
            features.totalLength += CalculateStrokeLength(stroke);
            totalCurviness += CalculateStrokeCurviness(stroke);
        }

        features.curviness = strokes.Count > 0 ? totalCurviness / strokes.Count : 0f;

        // Calculate compactness (how circle-like the shape is)
        float area = width * height;
        float perimeter = features.totalLength;
        features.compactness = area > 0 ? (4 * Mathf.PI * area) / (perimeter * perimeter) : 0f;
        features.compactness = Mathf.Clamp01(features.compactness);

        // Calculate radialness (how much strokes radiate from center)
        features.radialness = CalculateRadialness(strokes, features.bounds.center);

        // Calculate branchiness (how much strokes branch from each other)
        features.branchiness = CalculateBranchiness(strokes);

        return features;
    }

    private Bounds CalculateBounds(List<LineRenderer> strokes)
    {
        Vector3 min = Vector3.one * float.MaxValue;
        Vector3 max = Vector3.one * float.MinValue;

        foreach (var stroke in strokes)
        {
            for (int i = 0; i < stroke.positionCount; i++)
            {
                Vector3 pos = stroke.GetPosition(i);
                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }
        }

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    private float CalculateStrokeLength(LineRenderer stroke)
    {
        float length = 0f;
        for (int i = 1; i < stroke.positionCount; i++)
        {
            length += Vector3.Distance(stroke.GetPosition(i - 1), stroke.GetPosition(i));
        }
        return length;
    }

    private float CalculateStrokeCurviness(LineRenderer stroke)
    {
        if (stroke.positionCount < 3)
            return 0f;

        float totalAngle = 0f;
        int angleCount = 0;

        for (int i = 1; i < stroke.positionCount - 1; i++)
        {
            Vector3 v1 = stroke.GetPosition(i) - stroke.GetPosition(i - 1);
            Vector3 v2 = stroke.GetPosition(i + 1) - stroke.GetPosition(i);

            if (v1.magnitude > 0.01f && v2.magnitude > 0.01f)
            {
                float angle = Vector3.Angle(v1, v2);
                totalAngle += angle;
                angleCount++;
            }
        }

        return angleCount > 0 ? Mathf.Clamp01(totalAngle / (angleCount * 90f)) : 0f;
    }

    private float CalculateRadialness(List<LineRenderer> strokes, Vector3 center)
    {
        if (strokes.Count < 2)
            return 0f;

        float radialScore = 0f;
        int validStrokes = 0;

        foreach (var stroke in strokes)
        {
            if (stroke.positionCount < 2)
                continue;

            Vector3 start = stroke.GetPosition(0);
            Vector3 end = stroke.GetPosition(stroke.positionCount - 1);

            float startDist = Vector3.Distance(start, center);
            float endDist = Vector3.Distance(end, center);

            // Check if stroke radiates outward from center
            if (startDist < endDist && startDist < 2f)
            {
                radialScore += 1f;
            }

            validStrokes++;
        }

        return validStrokes > 0 ? radialScore / validStrokes : 0f;
    }

    private float CalculateBranchiness(List<LineRenderer> strokes)
    {
        if (strokes.Count < 2)
            return 0f;

        int connectionCount = 0;
        float connectionThreshold = 1f;

        for (int i = 0; i < strokes.Count; i++)
        {
            for (int j = i + 1; j < strokes.Count; j++)
            {
                if (StrokesConnect(strokes[i], strokes[j], connectionThreshold))
                {
                    connectionCount++;
                }
            }
        }

        int maxPossibleConnections = (strokes.Count * (strokes.Count - 1)) / 2;
        return maxPossibleConnections > 0 ? (float)connectionCount / maxPossibleConnections : 0f;
    }

    private bool StrokesConnect(LineRenderer stroke1, LineRenderer stroke2, float threshold)
    {
        if (stroke1.positionCount == 0 || stroke2.positionCount == 0)
            return false;

        Vector3 s1Start = stroke1.GetPosition(0);
        Vector3 s1End = stroke1.GetPosition(stroke1.positionCount - 1);
        Vector3 s2Start = stroke2.GetPosition(0);
        Vector3 s2End = stroke2.GetPosition(stroke2.positionCount - 1);

        return Vector3.Distance(s1Start, s2Start) < threshold ||
               Vector3.Distance(s1Start, s2End) < threshold ||
               Vector3.Distance(s1End, s2Start) < threshold ||
               Vector3.Distance(s1End, s2End) < threshold;
    }

    #endregion

    #region Quality Rating

    private string GetQualityRating(float quality)
    {
        if (quality >= 0.9f) return "Perfect!";
        if (quality >= 0.75f) return "Excellent";
        if (quality >= 0.6f) return "Good";
        if (quality >= 0.4f) return "Decent";
        if (quality >= 0.2f) return "Poor";
        return "Very Poor";
    }

    #endregion
}
