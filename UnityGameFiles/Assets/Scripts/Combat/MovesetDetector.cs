using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Detects which move the player drew during battle
/// Analyzes drawing patterns to match type-specific moves
/// </summary>
public class MovesetDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Minimum confidence score required to recognize a move")]
    public float confidenceThreshold = 0.5f;

    public class MoveDetectionResult
    {
        public MoveData.MoveType detectedMove;
        public float confidence;
        public bool wasRecognized;
        public Dictionary<MoveData.MoveType, float> scores;

        public MoveDetectionResult()
        {
            scores = new Dictionary<MoveData.MoveType, float>();
            wasRecognized = false;
        }

        public override string ToString()
        {
            if (wasRecognized)
                return $"{detectedMove} - Confidence: {confidence:P0}";
            else
                return "Move not recognized!";
        }
    }

    /// <summary>
    /// Analyze drawing and detect which move was drawn
    /// Only checks moves available to the given plant type
    /// </summary>
    public MoveDetectionResult DetectMove(List<LineRenderer> strokes, PlantRecognitionSystem.PlantType plantType)
    {
        if (strokes == null || strokes.Count == 0)
        {
            Debug.LogWarning("No strokes to analyze for move detection!");
            return CreateFailedResult();
        }

        Debug.Log($"=== MOVE DETECTION START for {plantType} ===");
        Debug.Log($"Analyzing {strokes.Count} strokes...");

        // Extract features from the drawing
        DrawingFeatures features = ExtractFeatures(strokes);

        // Get available moves for this plant type
        MoveData[] availableMoves = MoveData.GetMovesForPlant(plantType);
        if (availableMoves.Length == 0)
        {
            Debug.LogWarning($"No moves defined for {plantType}!");
            return CreateFailedResult();
        }

        // Calculate scores for each available move
        MoveDetectionResult result = new MoveDetectionResult();

        foreach (var moveData in availableMoves)
        {
            float score = CalculateMoveScore(moveData.moveType, features);
            result.scores[moveData.moveType] = score;
            Debug.Log($"{moveData.moveType} Score: {score:F2}");
        }

        // Find best match
        var bestMatch = result.scores.OrderByDescending(x => x.Value).First();
        result.detectedMove = bestMatch.Key;
        result.confidence = bestMatch.Value;

        // Check if confidence meets threshold
        if (result.confidence >= confidenceThreshold)
        {
            result.wasRecognized = true;
            Debug.Log($"✅ MOVE RECOGNIZED: {result}");
        }
        else
        {
            result.wasRecognized = false;
            Debug.Log($"❌ MOVE NOT RECOGNIZED (best: {result.detectedMove} at {result.confidence:P0})");
        }

        return result;
    }

    /// <summary>
    /// Calculate how well the drawing matches a specific move
    /// </summary>
    private float CalculateMoveScore(MoveData.MoveType moveType, DrawingFeatures features)
    {
        switch (moveType)
        {
            // FIRE MOVES
            case MoveData.MoveType.Fireball:
                return CalculateFireballScore(features);
            case MoveData.MoveType.FlameWave:
                return CalculateFlameWaveScore(features);
            case MoveData.MoveType.Burn:
                return CalculateBurnScore(features);

            // GRASS MOVES
            case MoveData.MoveType.VineWhip:
                return CalculateVineWhipScore(features);
            case MoveData.MoveType.LeafStorm:
                return CalculateLeafStormScore(features);
            case MoveData.MoveType.RootAttack:
                return CalculateRootAttackScore(features);

            // WATER MOVES
            case MoveData.MoveType.WaterSplash:
                return CalculateWaterSplashScore(features);
            case MoveData.MoveType.Bubble:
                return CalculateBubbleScore(features);
            case MoveData.MoveType.HealingWave:
                return CalculateHealingWaveScore(features);

            default:
                return 0f;
        }
    }

    // ===== FIRE MOVE DETECTION =====

    /// <summary>
    /// Fireball: Single circular/oval shape
    /// </summary>
    private float CalculateFireballScore(DrawingFeatures f)
    {
        float score = 0f;

        // Should be 1-2 strokes (circle with optional tail)
        if (f.strokeCount >= 1 && f.strokeCount <= 2) score += 0.3f;

        // Must have circular shape
        if (f.circularStrokes >= 1) score += 0.5f;
        else return 0f; // No circle = not a fireball

        // Should be compact (not too spread out)
        float size = Mathf.Max(f.width, f.height);
        if (size < 3f) score += 0.2f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Flame Wave: Horizontal wavy pattern
    /// </summary>
    private float CalculateFlameWaveScore(DrawingFeatures f)
    {
        float score = 0f;

        // Should be horizontal (wide, not tall)
        if (f.aspectRatio < 0.7f) score += 0.3f;

        // Horizontal strokes
        if (f.horizontalStrokes >= 1) score += 0.3f;

        // Wavy/curved pattern
        if (f.curvedStrokes >= 1) score += 0.3f;

        // Penalty for circular shapes
        if (f.circularStrokes > 0) score *= 0.5f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Burn: Zigzag or angular pattern
    /// </summary>
    private float CalculateBurnScore(DrawingFeatures f)
    {
        float score = 0f;

        // Should have sharp turns (spiky)
        if (f.spikyStrokes >= 1) score += 0.5f;
        else return 0f; // No spikes = not burn

        // Can be vertical or diagonal
        if (f.verticalStrokes >= 1 || f.spikyStrokes >= 2) score += 0.3f;

        // Penalty for circular shapes
        if (f.circularStrokes > 0) score *= 0.3f;

        // Bonus for multiple sharp strokes
        if (f.spikyStrokes >= 2) score += 0.2f;

        return Mathf.Clamp01(score);
    }

    // ===== GRASS MOVE DETECTION =====

    /// <summary>
    /// Vine Whip: Curved/spiral single line
    /// </summary>
    private float CalculateVineWhipScore(DrawingFeatures f)
    {
        float score = 0f;

        // Should be 1-2 strokes
        if (f.strokeCount >= 1 && f.strokeCount <= 2) score += 0.3f;

        // Must be curved
        if (f.curvedStrokes >= 1) score += 0.4f;
        else return 0f; // Not curved = not vine

        // Should be elongated (not compact)
        if (f.aspectRatio > 0.8f && f.aspectRatio < 2.0f) score += 0.2f;

        // Penalty for circular (vine whips don't close)
        if (f.circularStrokes > 0) score *= 0.5f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Leaf Storm: Multiple short strokes scattered
    /// </summary>
    private float CalculateLeafStormScore(DrawingFeatures f)
    {
        float score = 0f;

        // Should have many strokes (3+)
        if (f.strokeCount >= 5) score += 0.4f;
        else if (f.strokeCount >= 3) score += 0.25f;
        else return 0f; // Too few strokes

        // Strokes should be relatively short/scattered
        if (f.strokeCount >= 4) score += 0.3f;

        // Mix of directions (not all same direction)
        if (f.horizontalStrokes >= 1 && f.verticalStrokes >= 1) score += 0.2f;

        // Penalty for circular shapes
        if (f.circularStrokes > 0) score *= 0.6f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Root Attack: Vertical downward lines
    /// </summary>
    private float CalculateRootAttackScore(DrawingFeatures f)
    {
        float score = 0f;

        // Should be tall (high aspect ratio)
        if (f.aspectRatio > 1.2f) score += 0.3f;

        // Must have vertical strokes
        if (f.verticalStrokes >= 1) score += 0.4f;
        else return 0f; // No vertical = not roots

        // Bonus for multiple vertical strokes
        if (f.verticalStrokes >= 2) score += 0.2f;

        // Penalty for horizontal dominance
        if (f.horizontalStrokes > f.verticalStrokes) score *= 0.5f;

        // Penalty for circular
        if (f.circularStrokes > 0) score *= 0.5f;

        return Mathf.Clamp01(score);
    }

    // ===== WATER MOVE DETECTION =====

    /// <summary>
    /// Water Splash: Upward wavy lines
    /// </summary>
    private float CalculateWaterSplashScore(DrawingFeatures f)
    {
        float score = 0f;

        // Should have curved/wavy strokes
        if (f.curvedStrokes >= 1) score += 0.4f;

        // Can be vertical or mixed direction
        if (f.verticalStrokes >= 1 || (f.horizontalStrokes >= 1 && f.curvedStrokes >= 1)) score += 0.3f;

        // Multiple strokes for splash effect
        if (f.strokeCount >= 2 && f.strokeCount <= 5) score += 0.2f;

        // Penalty for being too horizontal
        if (f.aspectRatio < 0.5f) score *= 0.6f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Bubble: Small circular shapes
    /// </summary>
    private float CalculateBubbleScore(DrawingFeatures f)
    {
        float score = 0f;

        // Must have circular strokes
        if (f.circularStrokes >= 1) score += 0.5f;
        else return 0f; // No circles = not bubbles

        // Bonus for multiple circles
        if (f.circularStrokes >= 2) score += 0.3f;

        // Should be relatively small/compact
        float avgSize = (f.width + f.height) / 2f;
        if (avgSize < 4f) score += 0.2f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Healing Wave: Smooth horizontal wave
    /// </summary>
    private float CalculateHealingWaveScore(DrawingFeatures f)
    {
        float score = 0f;

        // Should be horizontal and wide
        if (f.aspectRatio < 0.8f) score += 0.3f;

        // Must have horizontal strokes
        if (f.horizontalStrokes >= 1) score += 0.3f;
        else return 0f; // Not horizontal = not healing wave

        // Should be smooth/curved
        if (f.curvedStrokes >= 1) score += 0.3f;

        // Penalty for spiky (healing is smooth)
        if (f.spikyStrokes > 0) score *= 0.5f;

        // Penalty for circular
        if (f.circularStrokes > 0) score *= 0.5f;

        return Mathf.Clamp01(score);
    }

    // ===== FEATURE EXTRACTION =====

    private DrawingFeatures ExtractFeatures(List<LineRenderer> strokes)
    {
        DrawingFeatures features = new DrawingFeatures();
        List<Vector3> allPoints = new List<Vector3>();

        // Collect all points
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
        features.strokeCount = strokes.Count;

        // Analyze stroke patterns
        features.circularStrokes = CountCircularStrokes(strokes);
        features.verticalStrokes = CountVerticalStrokes(strokes);
        features.horizontalStrokes = CountHorizontalStrokes(strokes);
        features.spikyStrokes = CountSpikyStrokes(strokes);
        features.curvedStrokes = CountCurvedStrokes(strokes);

        Debug.Log($"Features: W={features.width:F2}, H={features.height:F2}, Aspect={features.aspectRatio:F2}, Strokes={features.strokeCount}");
        Debug.Log($"Patterns: Circ={features.circularStrokes}, Vert={features.verticalStrokes}, Horiz={features.horizontalStrokes}, Spiky={features.spikyStrokes}, Curved={features.curvedStrokes}");

        return features;
    }

    // ===== FEATURE DETECTION HELPERS =====

    private int CountCircularStrokes(List<LineRenderer> strokes)
    {
        int count = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 8) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            float startEndDist = Vector3.Distance(positions[0], positions[positions.Length - 1]);
            float totalLength = CalculateStrokeLength(positions);

            if (startEndDist < totalLength * 0.35f && totalLength > 0.5f)
            {
                count++;
            }
        }
        return count;
    }

    private int CountVerticalStrokes(List<LineRenderer> strokes)
    {
        int count = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 3) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            float verticalExtent = Mathf.Abs(positions[positions.Length - 1].y - positions[0].y);
            float horizontalExtent = Mathf.Abs(positions[positions.Length - 1].x - positions[0].x);

            if (verticalExtent > horizontalExtent * 1.3f)
            {
                count++;
            }
        }
        return count;
    }

    private int CountHorizontalStrokes(List<LineRenderer> strokes)
    {
        int count = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 3) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            float verticalExtent = Mathf.Abs(positions[positions.Length - 1].y - positions[0].y);
            float horizontalExtent = Mathf.Abs(positions[positions.Length - 1].x - positions[0].x);

            if (horizontalExtent > verticalExtent * 1.3f)
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

            int sharpTurns = 0;
            for (int i = 1; i < positions.Length - 1; i++)
            {
                Vector3 dir1 = (positions[i] - positions[i - 1]).normalized;
                Vector3 dir2 = (positions[i + 1] - positions[i]).normalized;
                float angle = Vector3.Angle(dir1, dir2);

                if (angle > 80f) sharpTurns++;
            }

            if (sharpTurns >= 2) count++;
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

            if (avgAngle > 5f && avgAngle < 45f)
            {
                count++;
            }
        }
        return count;
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

    private MoveDetectionResult CreateFailedResult()
    {
        return new MoveDetectionResult
        {
            detectedMove = MoveData.MoveType.Unknown,
            confidence = 0f,
            wasRecognized = false
        };
    }

    // ===== FEATURE DATA STRUCTURE =====

    private class DrawingFeatures
    {
        public float width;
        public float height;
        public float aspectRatio;
        public int strokeCount;
        public int circularStrokes;
        public int verticalStrokes;
        public int horizontalStrokes;
        public int spikyStrokes;
        public int curvedStrokes;
    }
}
