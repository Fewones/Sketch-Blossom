using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Detects which move the player drew during battle
/// Analyzes drawing patterns to match type-specific moves.
/// Supports an optional TinyCLIP shape hint to boost recognition accuracy.
/// </summary>
public class MovesetDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Minimum confidence score required to recognize a move")]
    public float confidenceThreshold = 0.5f;

    /// <summary>
    /// Shape label returned by TinyCLIP for the player's drawn gesture.
    /// shapeLabel must match a key in the move_shapes label map (e.g. "circle", "wave").
    /// confidence is the cosine-similarity score from TinyCLIP (0-1).
    /// </summary>
    public struct CLIPMoveHint
    {
        public string shapeLabel;
        public float confidence;
    }

    // Maps each TinyCLIP shape label to the move types it represents.
    // When TinyCLIP recognises one of these shapes the matching moves get
    // a large score bonus, making recognition far more robust than pure
    // geometric heuristics alone.
    private static readonly Dictionary<string, MoveData.MoveType[]> shapeToMoveTypes =
        new Dictionary<string, MoveData.MoveType[]>
        {
            { "fireball",      new[] { MoveData.MoveType.Fireball } },
            { "flame_wave",    new[] { MoveData.MoveType.FlameWave } },
            { "zigzag",        new[] { MoveData.MoveType.Burn } },
            { "curved_line",   new[] { MoveData.MoveType.VineWhip } },
            { "scattered",     new[] { MoveData.MoveType.LeafStorm } },
            { "downward_lines",new[] { MoveData.MoveType.RootAttack } },
            { "water_splash",  new[] { MoveData.MoveType.WaterSplash } },
            { "bubbles",       new[] { MoveData.MoveType.Bubble } },
            { "healing_wave",  new[] { MoveData.MoveType.HealingWave } },
            { "shield",        new[] { MoveData.MoveType.Block } },
            { "slash",         new[] { MoveData.MoveType.Sting, MoveData.MoveType.Cut, MoveData.MoveType.Bump } },
            { "straight_line", new[] { MoveData.MoveType.Sting, MoveData.MoveType.Cut, MoveData.MoveType.Bump } },
        };

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
    /// Detect which move was drawn, using a TinyCLIP shape hint to boost accuracy.
    /// When clipHint.confidence is high the recognised shape strongly biases the
    /// result; when it is low the system falls back to pure geometric heuristics.
    /// </summary>
    public MoveDetectionResult DetectMoveWithCLIP(
        List<LineRenderer> strokes,
        PlantRecognitionSystem.PlantType plantType,
        CLIPMoveHint clipHint)
    {
        if (strokes == null || strokes.Count == 0)
        {
            Debug.LogWarning("No strokes to analyse for move detection!");
            return CreateFailedResult();
        }

        Debug.Log($"=== CLIP-ASSISTED MOVE DETECTION for {plantType} ===");
        Debug.Log($"CLIP hint: shape='{clipHint.shapeLabel}' confidence={clipHint.confidence:F2}");

        DrawingFeatures features = ExtractFeatures(strokes);
        MoveData[] availableMoves = MoveData.GetMovesForPlant(plantType);
        if (availableMoves.Length == 0)
        {
            Debug.LogWarning($"No moves defined for {plantType}!");
            return CreateFailedResult();
        }

        // Determine which move types the CLIP shape hint supports.
        MoveData.MoveType[] clipSupportedTypes = null;
        bool hasCLIPHint = !string.IsNullOrEmpty(clipHint.shapeLabel) &&
                           shapeToMoveTypes.TryGetValue(clipHint.shapeLabel, out clipSupportedTypes);

        MoveDetectionResult result = new MoveDetectionResult();

        foreach (var moveData in availableMoves)
        {
            // Base geometric score (same logic as before).
            float geometricScore = CalculateMoveScore(moveData.moveType, features);

            // CLIP boost: scale with confidence so a low-confidence hint has
            // little influence while a high-confidence hit dominates.
            float clipBoost = 0f;
            if (hasCLIPHint && System.Array.IndexOf(clipSupportedTypes, moveData.moveType) >= 0)
            {
                // Max boost of 0.6 at full CLIP confidence.
                clipBoost = clipHint.confidence * 0.6f;
            }

            float combinedScore = Mathf.Clamp01(geometricScore + clipBoost);
            result.scores[moveData.moveType] = combinedScore;

            Debug.Log($"{moveData.moveType}: geometric={geometricScore:F2} clipBoost={clipBoost:F2} total={combinedScore:F2}");
        }

        var bestMatch = result.scores.OrderByDescending(x => x.Value).First();
        result.detectedMove = bestMatch.Key;
        result.confidence = bestMatch.Value;

        if (result.confidence >= confidenceThreshold)
        {
            result.wasRecognized = true;
            result.quality = CalculateDrawingQuality(strokes, result.detectedMove);
            result.damageMultiplier = Mathf.Lerp(0.5f, 1.5f, result.quality);
            result.qualityRating = GetQualityRating(result.quality);
            Debug.Log($"✅ CLIP-ASSISTED MOVE RECOGNIZED: {result}");
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

            // NORMAL MOVES
            case MoveData.MoveType.Sting:
            case MoveData.MoveType.Cut:
            case MoveData.MoveType.Bump:
                return CalculateSlashScore(features);

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
    /// Block: Very easy - any simple closed shape (circle, square, etc.)
    /// This is the easiest move to recognize!
    /// </summary>
    private float CalculateBlockScore(DrawingFeatures f)
    {
        float score = 0f;

        // Should be 1-3 strokes (simple shape)
        if (f.strokeCount >= 1 && f.strokeCount <= 3) score += 0.3f;
        else if (f.strokeCount <= 5) score += 0.15f;  // Still okay

        // Prefer compact shapes
        float avgSize = (f.width + f.height) / 2f;
        if (avgSize < 4f) score += 0.2f;

        // Bonus for circular strokes (easiest to draw)
        if (f.circularStrokes >= 1) score += 0.3f;

        // Block is still forgiving but won't dominate other moves
        // Reduced from 0.4f to 0.15f to act as a true fallback
        score = Mathf.Max(score, 0.15f);

        return Mathf.Clamp01(score);
    }

    // ===== NORMAL MOVE DETECTION =====

    /// <summary>
    /// Sting/Cut/Bump: A single straight line slash - simple and quick to draw
    /// </summary>
    private float CalculateSlashScore(DrawingFeatures f)
    {
        float score = 0f;

        // Should be exactly 1 stroke (a single line)
        if (f.strokeCount == 1) score += 0.4f;
        else if (f.strokeCount == 2) score += 0.15f;
        else return 0f; // Too many strokes - not a slash

        // Should NOT be circular (straight line, not a loop)
        if (f.circularStrokes == 0) score += 0.2f;
        else score *= 0.2f;

        // Should NOT be spiky (straight, not zigzag)
        if (f.spikyStrokes == 0) score += 0.15f;
        else score *= 0.3f;

        // Should NOT be very curved (straight line)
        if (f.curvedStrokes == 0) score += 0.2f;
        else score *= 0.5f;

        // Bonus for having some length (not just a dot)
        float size = Mathf.Max(f.width, f.height);
        if (size > 0.5f) score += 0.1f;

        return Mathf.Clamp01(score);
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

        // Strong bonus for circular shape
        if (f.circularStrokes >= 1) score += 0.5f;
        else if (f.strokeCount <= 2) score += 0.1f; // Partial credit for simple strokes

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

        // Strong bonus for sharp turns (spiky)
        if (f.spikyStrokes >= 1) score += 0.5f;
        else if (f.strokeCount >= 2) score += 0.15f; // Partial credit for multiple strokes

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

        // Strong bonus for curved strokes
        if (f.curvedStrokes >= 1) score += 0.4f;
        else if (f.strokeCount <= 3) score += 0.1f; // Partial credit for few strokes

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
        else if (f.strokeCount >= 2) score += 0.1f; // Partial credit for some strokes

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
        else if (f.aspectRatio > 0.9f) score += 0.1f; // Partial credit

        // Strong bonus for vertical strokes
        if (f.verticalStrokes >= 1) score += 0.4f;
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

        // Strong bonus for circular strokes
        if (f.circularStrokes >= 1) score += 0.5f;
        else if (f.strokeCount >= 1 && f.strokeCount <= 3) score += 0.15f; // Partial credit for simple strokes

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
        else if (f.aspectRatio < 1.2f) score += 0.1f; // Partial credit

        // Strong bonus for horizontal strokes
        if (f.horizontalStrokes >= 1) score += 0.3f;
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

    private float CalculateStrokeLength(Vector3[] positions)
    {
        float length = 0f;
        for (int i = 1; i < positions.Length; i++)
        {
            length += Vector3.Distance(positions[i - 1], positions[i]);
        }
        return length;
    }

    // ── Quality Scoring ───────────────────────────────────────────────────────
    // Inlined from the former MoveRecognitionSystem component so that all move
    // logic lives in one place.

    private float CalculateDrawingQuality(List<LineRenderer> strokes, MoveData.MoveType moveType)
    {
        if (strokes == null || strokes.Count == 0) return 0f;
        ShapeFeatures sf = AnalyzeShapeFeatures(strokes);
        float quality = CalculateMoveQuality(moveType, sf);
        return Mathf.Clamp01(quality);
    }

    private float CalculateMoveQuality(MoveData.MoveType moveType, ShapeFeatures f)
    {
        switch (moveType)
        {
            case MoveData.MoveType.Block:
                return QualityBlock(f);
            case MoveData.MoveType.Sting:
            case MoveData.MoveType.Cut:
            case MoveData.MoveType.Bump:
                return QualitySlash(f);
            case MoveData.MoveType.Fireball:
            case MoveData.MoveType.Bubble:
                return QualityBall(f);
            case MoveData.MoveType.FlameWave:
            case MoveData.MoveType.VineWhip:
                return QualityWave(f);
            case MoveData.MoveType.Burn:
                return QualityFire(f);
            case MoveData.MoveType.RootAttack:
            case MoveData.MoveType.LeafStorm:
                return QualityPlantGrowth(f);
            case MoveData.MoveType.WaterSplash:
            case MoveData.MoveType.HealingWave:
                return QualityWater(f);
            default:
                return QualityGeneric(f);
        }
    }

    private float QualityBlock(ShapeFeatures f)
    {
        float s = 0f;
        s += Mathf.Clamp01(f.compactness * 2f) * 0.4f;
        s += (1f - Mathf.Clamp01((f.strokeCount - 2) / 5f)) * 0.3f;
        s += (1f - Mathf.Abs(f.curviness - 0.5f) * 2f) * 0.3f;
        return Mathf.Clamp01(s);
    }

    private float QualitySlash(ShapeFeatures f)
    {
        float s = 0f;
        // Reward straightness (low curviness)
        s += (1f - f.curviness) * 0.4f;
        // Reward single stroke
        s += (1f - Mathf.Clamp01((f.strokeCount - 1) / 3f)) * 0.3f;
        // Reward decent length
        s += Mathf.Clamp01(f.totalLength / 3f) * 0.3f;
        return Mathf.Clamp01(s);
    }

    private float QualityBall(ShapeFeatures f)
    {
        float s = 0f;
        s += (1f - Mathf.Abs(1f - f.aspectRatio)) * 0.4f;
        s += f.compactness * 0.4f;
        s += f.curviness * 0.2f;
        return Mathf.Clamp01(s);
    }

    private float QualityWave(ShapeFeatures f)
    {
        float s = 0f;
        s += Mathf.Clamp01(f.aspectRatio - 1f) * 0.4f;
        s += (1f - f.compactness) * 0.3f;
        s += f.curviness * 0.3f;
        return Mathf.Clamp01(s);
    }

    private float QualityFire(ShapeFeatures f)
    {
        float s = 0f;
        s += Mathf.Clamp01((1f / Mathf.Max(f.aspectRatio, 0.01f)) - 0.5f) * 0.3f;
        s += Mathf.Clamp01(f.strokeCount / 5f) * 0.3f;
        s += Mathf.Clamp01(1f - f.curviness) * 0.2f;
        s += f.radialness * 0.2f;
        return Mathf.Clamp01(s);
    }

    private float QualityPlantGrowth(ShapeFeatures f)
    {
        float s = 0f;
        s += f.branchiness * 0.4f;
        s += Mathf.Clamp01(f.strokeCount / 4f) * 0.3f;
        s += f.curviness * 0.3f;
        return Mathf.Clamp01(s);
    }

    private float QualityWater(ShapeFeatures f)
    {
        float s = 0f;
        s += f.curviness * 0.4f;
        s += (1f - Mathf.Abs(1.5f - f.aspectRatio) / 2f) * 0.3f;
        s += Mathf.Clamp01(f.strokeCount / 4f) * 0.3f;
        return Mathf.Clamp01(s);
    }

    private float QualityGeneric(ShapeFeatures f)
    {
        float s = 0f;
        s += f.compactness * 0.3f;
        s += Mathf.Clamp01(f.strokeCount / 4f) * 0.3f;
        s += f.curviness * 0.2f;
        s += (f.branchiness + f.radialness) * 0.2f;
        return Mathf.Clamp01(s);
    }

    private string GetQualityRating(float q)
    {
        if (q >= 0.9f)  return "Perfect!";
        if (q >= 0.75f) return "Excellent";
        if (q >= 0.6f)  return "Good";
        if (q >= 0.4f)  return "Decent";
        if (q >= 0.2f)  return "Poor";
        return "Very Poor";
    }

    // ── ShapeFeatures analysis (different from DrawingFeatures; measures
    //    compactness/curviness/radialness/branchiness rather than stroke counts)

    private class ShapeFeatures
    {
        public int   strokeCount;
        public float aspectRatio;
        public float compactness;
        public float curviness;
        public float radialness;
        public float branchiness;
        public float totalLength;
    }

    private ShapeFeatures AnalyzeShapeFeatures(List<LineRenderer> strokes)
    {
        ShapeFeatures f = new ShapeFeatures { strokeCount = strokes.Count };

        Bounds bounds = SFBounds(strokes);
        float w = bounds.size.x, h = bounds.size.y;
        f.aspectRatio = h > 0f ? w / h : 1f;

        float totalCurv = 0f;
        foreach (var s in strokes)
        {
            f.totalLength += SFStrokeLength(s);
            totalCurv     += SFStrokeCurviness(s);
        }
        f.curviness = strokes.Count > 0 ? totalCurv / strokes.Count : 0f;

        float area = w * h;
        float perim = f.totalLength;
        f.compactness = (area > 0f && perim > 0f)
            ? Mathf.Clamp01((4f * Mathf.PI * area) / (perim * perim))
            : 0f;

        f.radialness  = SFRadialness(strokes, bounds.center);
        f.branchiness = SFBranchiness(strokes);
        return f;
    }

    private Bounds SFBounds(List<LineRenderer> strokes)
    {
        Vector3 min = Vector3.one * float.MaxValue;
        Vector3 max = Vector3.one * float.MinValue;
        foreach (var s in strokes)
        {
            for (int i = 0; i < s.positionCount; i++)
            {
                Vector3 p = s.GetPosition(i);
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
        }
        Bounds b = new Bounds();
        b.SetMinMax(min, max);
        return b;
    }

    private float SFStrokeLength(LineRenderer s)
    {
        float len = 0f;
        for (int i = 1; i < s.positionCount; i++)
            len += Vector3.Distance(s.GetPosition(i - 1), s.GetPosition(i));
        return len;
    }

    private float SFStrokeCurviness(LineRenderer s)
    {
        if (s.positionCount < 3) return 0f;
        float total = 0f; int cnt = 0;
        for (int i = 1; i < s.positionCount - 1; i++)
        {
            Vector3 v1 = s.GetPosition(i)     - s.GetPosition(i - 1);
            Vector3 v2 = s.GetPosition(i + 1) - s.GetPosition(i);
            if (v1.magnitude > 0.01f && v2.magnitude > 0.01f)
            { total += Vector3.Angle(v1, v2); cnt++; }
        }
        return cnt > 0 ? Mathf.Clamp01(total / (cnt * 90f)) : 0f;
    }

    private float SFRadialness(List<LineRenderer> strokes, Vector3 center)
    {
        if (strokes.Count < 2) return 0f;
        float score = 0f; int valid = 0;
        foreach (var s in strokes)
        {
            if (s.positionCount < 2) continue;
            Vector3 start = s.GetPosition(0);
            Vector3 end   = s.GetPosition(s.positionCount - 1);
            if (Vector3.Distance(start, center) < Vector3.Distance(end, center) &&
                Vector3.Distance(start, center) < 2f)
                score += 1f;
            valid++;
        }
        return valid > 0 ? score / valid : 0f;
    }

    private float SFBranchiness(List<LineRenderer> strokes)
    {
        if (strokes.Count < 2) return 0f;
        int conn = 0;
        const float threshold = 1f;
        for (int i = 0; i < strokes.Count; i++)
        for (int j = i + 1; j < strokes.Count; j++)
        {
            var a = strokes[i]; var b = strokes[j];
            if (a.positionCount == 0 || b.positionCount == 0) continue;
            Vector3 aS = a.GetPosition(0), aE = a.GetPosition(a.positionCount - 1);
            Vector3 bS = b.GetPosition(0), bE = b.GetPosition(b.positionCount - 1);
            if (Vector3.Distance(aS, bS) < threshold ||
                Vector3.Distance(aS, bE) < threshold ||
                Vector3.Distance(aE, bS) < threshold ||
                Vector3.Distance(aE, bE) < threshold)
                conn++;
        }
        int maxConn = (strokes.Count * (strokes.Count - 1)) / 2;
        return maxConn > 0 ? (float)conn / maxConn : 0f;
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
