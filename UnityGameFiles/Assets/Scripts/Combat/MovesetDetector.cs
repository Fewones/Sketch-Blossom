using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Detects which move the player drew during battle
/// Analyzes drawing patterns to match type-specific moves
/// </summary>
public class MovesetDetector : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Move recognition system for quality scoring")]
    public MoveRecognitionSystem recognitionSystem;

    [Header("Detection Settings")]
    [Tooltip("Minimum confidence score required to recognize a move")]
    public float confidenceThreshold = 0.5f;

    public class MoveDetectionResult
    {
        public MoveData.MoveType detectedMove;
        public float confidence;
        public bool wasRecognized;
        public Dictionary<MoveData.MoveType, float> scores;

        // Quality scoring
        public float quality;           // How well the move was drawn (0-1)
        public float damageMultiplier;  // Damage multiplier based on quality
        public string qualityRating;    // User-friendly quality description

        public MoveDetectionResult()
        {
            scores = new Dictionary<MoveData.MoveType, float>();
            wasRecognized = false;
            quality = 0f;
            damageMultiplier = 1f;
            qualityRating = "Poor";
        }

        public override string ToString()
        {
            if (wasRecognized)
                return $"{detectedMove} - Confidence: {confidence:P0} | Quality: {qualityRating} ({damageMultiplier:F2}x damage)";
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

            // Calculate quality using MoveRecognitionSystem
            if (recognitionSystem != null)
            {
                var qualityResult = recognitionSystem.AnalyzeMove(strokes, result.detectedMove);
                result.quality = qualityResult.quality;
                result.damageMultiplier = qualityResult.damageMultiplier;
                result.qualityRating = qualityResult.qualityRating;
            }
            else
            {
                // Fallback if no recognition system
                result.quality = result.confidence;
                result.damageMultiplier = 1f;
                result.qualityRating = "Unknown";
                Debug.LogWarning("MoveRecognitionSystem not assigned! Quality scoring disabled.");
            }

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
            // UNIVERSAL MOVES
            case MoveData.MoveType.Block:
                return CalculateBlockScore(features);
            case MoveData.MoveType.Tackle:
                return CalculateTackleScore(features);

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

    // ===== UNIVERSAL MOVE DETECTION =====

    /// <summary>
    /// Block: Draw a SQUARE shape
    /// Key differentiator from circle: squares have sharp CORNERS (spikyStrokes),
    /// circles have smooth curves (circularStrokes)
    /// </summary>
    private float CalculateBlockScore(DrawingFeatures f)
    {
        float score = 0f;

        // Must be a closed shape
        if (f.isClosedShape) score += 0.3f;
        else return 0.1f; // Not closed = not a square

        // CRITICAL: Must have sharp corners (this is what makes a square, not a circle!)
        if (f.spikyStrokes >= 1) score += 0.35f;
        else return 0.15f; // No sharp corners = smooth circle, not a square

        // Penalty for circular strokes (smooth curves = circle, not square)
        if (f.circularStrokes >= 1) score *= 0.4f;

        // Aspect ratio should be roughly square (0.5 to 1.8)
        if (f.aspectRatio >= 0.5f && f.aspectRatio <= 1.8f) score += 0.1f;

        // No/few self-intersections (squares don't cross themselves)
        if (f.selfIntersections <= 2) score += 0.1f;
        else score *= 0.5f; // Many crossings = probably a star

        // Should be 1-4 strokes
        if (f.strokeCount >= 1 && f.strokeCount <= 4) score += 0.1f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Tackle: Draw a STAR shape (pentagram)
    /// Key differentiator: closed + sharp corners + self-intersecting lines
    /// </summary>
    private float CalculateTackleScore(DrawingFeatures f)
    {
        float score = 0f;

        // Must be a closed shape
        if (f.isClosedShape) score += 0.2f;
        else return 0.1f; // Not closed = not a star

        // Must have sharp corners (spiky strokes)
        if (f.spikyStrokes >= 1) score += 0.25f;
        else return 0.15f; // No sharp corners = not a star

        // CRITICAL: Stars self-intersect (the lines cross over each other)
        // This is the key differentiator from a square
        if (f.selfIntersections >= 3) score += 0.35f;
        else if (f.selfIntersections >= 1) score += 0.15f;
        else return Mathf.Clamp(score, 0.1f, 0.35f); // No crossings = probably a square

        // Path length should be significantly longer than perimeter
        if (f.pathLengthRatio >= 1.5f) score += 0.1f;

        // Should be drawn in 1-5 strokes
        if (f.strokeCount <= 2) score += 0.1f;
        else if (f.strokeCount <= 5) score += 0.05f;

        // Aspect ratio should be roughly equal (star is roughly symmetrical)
        if (f.aspectRatio >= 0.5f && f.aspectRatio <= 1.8f) score += 0.05f;

        return Mathf.Clamp01(score);
    }

    // ===== FIRE MOVE DETECTION =====

    /// <summary>
    /// Fireball: Single circular/oval shape (smooth circle)
    /// </summary>
    private float CalculateFireballScore(DrawingFeatures f)
    {
        float score = 0f;

        // Should be 1-2 strokes (circle with optional tail)
        if (f.strokeCount >= 1 && f.strokeCount <= 2) score += 0.3f;

        // Strong bonus for circular shape (smooth closed curve)
        if (f.circularStrokes >= 1) score += 0.6f;
        else if (f.strokeCount <= 2) score += 0.1f; // Partial credit for simple strokes

        // Should be compact (not too spread out)
        float size = Mathf.Max(f.width, f.height);
        if (size < 3f) score += 0.2f;

        // Penalty for sharp corners (that's a square, not a circle)
        if (f.spikyStrokes >= 1) score *= 0.5f;

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
        else if (f.aspectRatio < 1.0f) score += 0.15f;

        // Horizontal strokes
        if (f.horizontalStrokes >= 1) score += 0.35f;

        // Wavy/curved pattern
        if (f.curvedStrokes >= 1) score += 0.3f;

        // Even a single horizontal stroke without curve should score decently
        if (f.horizontalStrokes >= 1 && f.strokeCount <= 2) score += 0.1f;

        // Penalty for circular shapes
        if (f.circularStrokes > 0) score *= 0.5f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Burn: Zigzag or angular pattern (open-ended spiky lines)
    /// </summary>
    private float CalculateBurnScore(DrawingFeatures f)
    {
        float score = 0f;

        // Strong bonus for sharp turns (spiky)
        if (f.spikyStrokes >= 1) score += 0.55f;
        else if (f.strokeCount >= 2) score += 0.15f; // Partial credit for multiple strokes

        // Can be vertical or diagonal
        if (f.verticalStrokes >= 1 || f.spikyStrokes >= 2) score += 0.3f;

        // Penalty for circular shapes (zigzags are open, not circular)
        if (f.circularStrokes > 0) score *= 0.3f;

        // Bonus for multiple sharp strokes
        if (f.spikyStrokes >= 2) score += 0.2f;

        // Zigzags should be open (not closed like star/square)
        if (!f.isClosedShape) score += 0.1f;

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

        // Strong bonus for curved strokes
        if (f.curvedStrokes >= 1) score += 0.5f;
        else if (f.strokeCount <= 3) score += 0.15f; // Partial credit for few strokes

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
        if (f.strokeCount >= 5) score += 0.5f;
        else if (f.strokeCount >= 3) score += 0.35f;
        else if (f.strokeCount >= 2) score += 0.15f; // Partial credit for some strokes

        // Strokes should be relatively short/scattered
        if (f.strokeCount >= 4) score += 0.3f;

        // Mix of directions (not all same direction)
        if (f.horizontalStrokes >= 1 && f.verticalStrokes >= 1) score += 0.2f;
        else if (f.horizontalStrokes >= 1 || f.verticalStrokes >= 1) score += 0.1f;

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
        else if (f.aspectRatio > 0.8f) score += 0.15f; // Partial credit

        // Strong bonus for vertical strokes
        if (f.verticalStrokes >= 1) score += 0.5f;
        else if (f.strokeCount >= 1) score += 0.1f; // Partial credit for any strokes

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
        if (f.curvedStrokes >= 1) score += 0.5f;

        // Can be vertical or mixed direction
        if (f.verticalStrokes >= 1 || (f.horizontalStrokes >= 1 && f.curvedStrokes >= 1)) score += 0.3f;

        // Multiple strokes for splash effect
        if (f.strokeCount >= 2 && f.strokeCount <= 5) score += 0.2f;

        // Penalty for being too horizontal
        if (f.aspectRatio < 0.5f) score *= 0.6f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Bubble: Small circular shapes (smooth circles)
    /// </summary>
    private float CalculateBubbleScore(DrawingFeatures f)
    {
        float score = 0f;

        // Strong bonus for circular strokes
        if (f.circularStrokes >= 1) score += 0.6f;
        else if (f.strokeCount >= 1 && f.strokeCount <= 3) score += 0.15f; // Partial credit for simple strokes

        // Bonus for multiple circles
        if (f.circularStrokes >= 2) score += 0.3f;

        // Should be relatively small/compact
        float avgSize = (f.width + f.height) / 2f;
        if (avgSize < 4f) score += 0.2f;

        // Penalty for sharp corners (that's a square, not bubbles)
        if (f.spikyStrokes >= 1) score *= 0.5f;

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
        else if (f.aspectRatio < 1.2f) score += 0.15f; // Partial credit

        // Strong bonus for horizontal strokes
        if (f.horizontalStrokes >= 1) score += 0.35f;
        else if (f.curvedStrokes >= 1) score += 0.15f; // Partial credit for curved

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

        // Shape-specific features for star/square detection
        features.totalSharpTurns = CountTotalSharpTurns(strokes);
        features.isClosedShape = CheckClosedShape(strokes);
        features.selfIntersections = CountSelfIntersections(strokes);

        // Path length ratio: total stroke length / bounding box perimeter
        float totalLength = 0f;
        foreach (var stroke in strokes)
        {
            if (stroke == null) continue;
            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);
            totalLength += CalculateStrokeLength(positions);
        }
        float perimeter = 2f * (features.width + features.height);
        features.pathLengthRatio = perimeter > 0.01f ? totalLength / perimeter : 0f;

        Debug.Log($"Features: W={features.width:F2}, H={features.height:F2}, Aspect={features.aspectRatio:F2}, Strokes={features.strokeCount}");
        Debug.Log($"Patterns: Circ={features.circularStrokes}, Vert={features.verticalStrokes}, Horiz={features.horizontalStrokes}, Spiky={features.spikyStrokes}, Curved={features.curvedStrokes}");
        Debug.Log($"Shapes: SharpTurns={features.totalSharpTurns}, Closed={features.isClosedShape}, SelfX={features.selfIntersections}, PathRatio={features.pathLengthRatio:F2}");

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

            // Relaxed from 1.3x to 1.1x to be more forgiving
            if (verticalExtent > horizontalExtent * 1.1f)
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

            // Relaxed from 1.3x to 1.1x to be more forgiving
            if (horizontalExtent > verticalExtent * 1.1f)
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

                // Relaxed from 80° to 60° to be more forgiving
                if (angle > 60f) sharpTurns++;
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

    /// <summary>
    /// Count total number of sharp direction changes (>60 degrees) across all strokes
    /// Star: 8-12+ turns, Square: 3-5 turns
    /// </summary>
    private int CountTotalSharpTurns(List<LineRenderer> strokes)
    {
        int totalTurns = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 4) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            for (int i = 1; i < positions.Length - 1; i++)
            {
                Vector3 dir1 = (positions[i] - positions[i - 1]).normalized;
                Vector3 dir2 = (positions[i + 1] - positions[i]).normalized;

                // Skip near-zero length segments
                if (dir1.sqrMagnitude < 0.001f || dir2.sqrMagnitude < 0.001f) continue;

                float angle = Vector3.Angle(dir1, dir2);
                if (angle > 60f)
                {
                    totalTurns++;
                }
            }
        }
        return totalTurns;
    }

    /// <summary>
    /// Check if the main stroke forms a closed shape (start point near end point)
    /// </summary>
    private bool CheckClosedShape(List<LineRenderer> strokes)
    {
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 6) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            float startEndDist = Vector3.Distance(positions[0], positions[positions.Length - 1]);
            float totalLength = CalculateStrokeLength(positions);

            // Closed if start and end are within 30% of total length
            if (totalLength > 0.5f && startEndDist < totalLength * 0.3f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Count the number of self-intersections in strokes
    /// Stars have many (~5), squares have none
    /// </summary>
    private int CountSelfIntersections(List<LineRenderer> strokes)
    {
        int intersections = 0;
        foreach (var stroke in strokes)
        {
            if (stroke == null || stroke.positionCount < 6) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            // Check pairs of non-adjacent line segments for intersection
            // Skip segments that are close together (adjacent/near-adjacent)
            int segmentCount = positions.Length - 1;
            for (int i = 0; i < segmentCount; i++)
            {
                for (int j = i + 3; j < segmentCount; j++) // Skip adjacent (+3 to avoid noise from close segments)
                {
                    if (SegmentsIntersect2D(
                        positions[i], positions[i + 1],
                        positions[j], positions[j + 1]))
                    {
                        intersections++;
                    }
                }
            }
        }
        return intersections;
    }

    /// <summary>
    /// Check if two 2D line segments intersect (ignoring Z)
    /// </summary>
    private bool SegmentsIntersect2D(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
    {
        float d1 = Cross2D(b2 - b1, a1 - b1);
        float d2 = Cross2D(b2 - b1, a2 - b1);
        float d3 = Cross2D(a2 - a1, b1 - a1);
        float d4 = Cross2D(a2 - a1, b2 - a1);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
        {
            return true;
        }
        return false;
    }

    private float Cross2D(Vector3 a, Vector3 b)
    {
        return a.x * b.y - a.y * b.x;
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

        // Shape-specific features for Block (square) and Tackle (star) detection
        public int totalSharpTurns;       // Total sharp direction changes across all strokes
        public bool isClosedShape;        // Whether the main stroke starts and ends near the same point
        public float pathLengthRatio;     // Total path length / bounding box perimeter (star >> square)
        public int selfIntersections;     // Number of times strokes cross themselves (star has many)
    }
}
