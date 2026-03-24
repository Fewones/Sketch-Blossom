using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Detects which move the player drew during battle.
/// Uses DrawingShape from MoveData to match geometric patterns, ensuring
/// each plant's moves are drawn with distinct, non-confusable shapes.
/// Supports an optional TinyCLIP shape hint to boost recognition accuracy.
/// </summary>
public class MovesetDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Minimum confidence score required to recognize a move")]
    public float confidenceThreshold = 0.5f;

    /// <summary>
    /// Shape label returned by TinyCLIP for the player's drawn gesture.
    /// </summary>
    public struct CLIPMoveHint
    {
        public string shapeLabel;
        public float confidence;
    }

    // Maps TinyCLIP shape labels (from labelMaps.json) to DrawingShapes for CLIP boost.
    // Keys must exactly match the values in labelMaps.json "move_shapes".
    private static readonly Dictionary<string, MoveData.DrawingShape[]> shapeLabelToDrawingShapes =
        new Dictionary<string, MoveData.DrawingShape[]>
        {
            { "circle",           new[] { MoveData.DrawingShape.Circle } },
            { "straight_line",    new[] { MoveData.DrawingShape.StraightLine } },
            { "zigzag",           new[] { MoveData.DrawingShape.Zigzag } },
            { "wavy_line",        new[] { MoveData.DrawingShape.WavyLine } },
            { "plus",             new[] { MoveData.DrawingShape.Plus } },
            { "x_cross",          new[] { MoveData.DrawingShape.XCross } },
            { "arrow",            new[] { MoveData.DrawingShape.Arrow } },
            { "multiple_circles", new[] { MoveData.DrawingShape.MultipleCircles } },
            { "star",             new[] { MoveData.DrawingShape.Star } },
            { "square",           new[] { MoveData.DrawingShape.Square } },
            { "triangle",         new[] { MoveData.DrawingShape.Triangle } },
            { "checkmark",        new[] { MoveData.DrawingShape.Checkmark } },
            { "spiral",           new[] { MoveData.DrawingShape.Spiral } },
        };

    /// <summary>
    /// Shapes that CLIP commonly confuses with each other due to visual similarity.
    /// When CLIP returns a primary shape, secondary shapes receive a reduced boost
    /// (secondaryBoostFactor) to handle misclassification gracefully.
    /// </summary>
    private static readonly Dictionary<MoveData.DrawingShape, MoveData.DrawingShape[]> shapeConfusionMap =
        new Dictionary<MoveData.DrawingShape, MoveData.DrawingShape[]>
        {
            { MoveData.DrawingShape.Circle,          new[] { MoveData.DrawingShape.Spiral, MoveData.DrawingShape.Square } },
            { MoveData.DrawingShape.Spiral,          new[] { MoveData.DrawingShape.Circle } },
            { MoveData.DrawingShape.Triangle,        new[] { MoveData.DrawingShape.Square, MoveData.DrawingShape.Circle } },
            { MoveData.DrawingShape.Square,          new[] { MoveData.DrawingShape.Triangle, MoveData.DrawingShape.Circle } },
            { MoveData.DrawingShape.Checkmark,       new[] { MoveData.DrawingShape.Arrow } },
            { MoveData.DrawingShape.Arrow,           new[] { MoveData.DrawingShape.Checkmark } },
            { MoveData.DrawingShape.MultipleCircles, new[] { MoveData.DrawingShape.Circle } },
            { MoveData.DrawingShape.Zigzag,          new[] { MoveData.DrawingShape.WavyLine } },
            { MoveData.DrawingShape.WavyLine,        new[] { MoveData.DrawingShape.Zigzag } },
            { MoveData.DrawingShape.XCross,          new[] { MoveData.DrawingShape.Plus, MoveData.DrawingShape.Star } },
            { MoveData.DrawingShape.Plus,             new[] { MoveData.DrawingShape.XCross } },
            { MoveData.DrawingShape.Star,             new[] { MoveData.DrawingShape.XCross, MoveData.DrawingShape.Plus } },
        };

    /// <summary>
    /// How much of the primary CLIP score to give to confused/secondary shapes.
    /// 0.65 means a secondary match gets 65% of the normalized CLIP score.
    /// </summary>
    private const float secondaryBoostFactor = 0.65f;

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
    /// Detect which move was drawn, using DrawingShape for per-move shape detection
    /// and an optional TinyCLIP hint to boost accuracy.
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

        Debug.Log($"=== CLIP-PRIMARY MOVE DETECTION for {plantType} ===");
        Debug.Log($"CLIP hint: shape='{clipHint.shapeLabel}' confidence={clipHint.confidence:F2}");

        MoveData[] availableMoves = MoveData.GetMovesForPlant(plantType);
        if (availableMoves.Length == 0)
        {
            Debug.LogWarning($"No moves defined for {plantType}!");
            return CreateFailedResult();
        }

        // Determine which DrawingShapes the CLIP hint supports.
        MoveData.DrawingShape[] clipSupportedShapes = null;
        bool hasCLIPHint = !string.IsNullOrEmpty(clipHint.shapeLabel) &&
                           shapeLabelToDrawingShapes.TryGetValue(clipHint.shapeLabel, out clipSupportedShapes);

        MoveDetectionResult result = new MoveDetectionResult();

        if (hasCLIPHint)
        {
            // ── CLIP IS PRIMARY ──
            // TinyCLIP compares the drawing against all 13 shape descriptions and
            // returns the best match.  Because probability is spread across 13
            // labels, even a correct match may only reach 25-40% raw confidence.
            // We normalize by multiplying by 2.5 so that a strong CLIP signal
            // (e.g. 0.33 → 0.83) comfortably exceeds the confidence threshold,
            // while weak/random matches (e.g. 0.10 → 0.25) still fail.
            float normalizedClip = Mathf.Clamp01(clipHint.confidence * 2.5f);

            // Collect secondary shapes: shapes commonly confused with the primary CLIP match.
            // These get a reduced boost so near-misses still have a chance.
            HashSet<MoveData.DrawingShape> secondaryShapes = new HashSet<MoveData.DrawingShape>();
            foreach (var primaryShape in clipSupportedShapes)
            {
                if (shapeConfusionMap.TryGetValue(primaryShape, out var confused))
                {
                    foreach (var cs in confused)
                        secondaryShapes.Add(cs);
                }
            }

            foreach (var moveData in availableMoves)
            {
                float score;
                if (System.Array.IndexOf(clipSupportedShapes, moveData.drawingShape) >= 0)
                {
                    // Primary match: full normalized score
                    score = normalizedClip;
                }
                else if (secondaryShapes.Contains(moveData.drawingShape))
                {
                    // Secondary match: reduced score for commonly confused shapes
                    score = normalizedClip * secondaryBoostFactor;
                }
                else
                {
                    score = 0.1f;  // Non-matching moves get a low baseline
                }
                result.scores[moveData.moveType] = score;
                Debug.Log($"{moveData.moveName} ({moveData.drawingShape}): clip_raw={clipHint.confidence:F2} normalized={score:F2}");
            }
        }
        else
        {
            // ── GEOMETRIC FALLBACK ──
            // Only used when TinyCLIP server is unavailable or returned no result.
            Debug.Log("CLIP unavailable — falling back to geometric detection");
            DrawingFeatures features = ExtractFeatures(strokes);

            foreach (var moveData in availableMoves)
            {
                float geometricScore = CalculateShapeScore(moveData.drawingShape, features);
                result.scores[moveData.moveType] = geometricScore;
                Debug.Log($"{moveData.moveName} ({moveData.drawingShape}): geometric={geometricScore:F2}");
            }
        }

        var bestMatch = result.scores.OrderByDescending(x => x.Value).First();
        result.detectedMove = bestMatch.Key;
        result.confidence = bestMatch.Value;

        if (result.confidence >= confidenceThreshold)
        {
            result.wasRecognized = true;
            // Use detection confidence as quality: better recognized = better attack
            // Map confidence from [threshold..1] to [0..1] so barely-recognized moves start low
            result.quality = Mathf.InverseLerp(confidenceThreshold, 1f, result.confidence);
            result.damageMultiplier = Mathf.Lerp(0.5f, 1.5f, result.quality);
            result.qualityRating = GetQualityRating(result.quality);
            Debug.Log($"✅ MOVE RECOGNIZED: {result}");
        }
        else
        {
            result.wasRecognized = false;
            Debug.Log($"❌ MOVE NOT RECOGNIZED (best: {result.detectedMove} at {result.confidence:P0})");
        }

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════
    // SHAPE DETECTION - One function per DrawingShape
    // ═══════════════════════════════════════════════════════════════════

    private float CalculateShapeScore(MoveData.DrawingShape shape, DrawingFeatures f)
    {
        switch (shape)
        {
            case MoveData.DrawingShape.Circle:          return ScoreCircle(f);
            case MoveData.DrawingShape.StraightLine:    return ScoreStraightLine(f);
            case MoveData.DrawingShape.Zigzag:          return ScoreZigzag(f);
            case MoveData.DrawingShape.WavyLine:        return ScoreWavyLine(f);
            case MoveData.DrawingShape.Plus:             return ScorePlus(f);
            case MoveData.DrawingShape.XCross:           return ScoreXCross(f);
            case MoveData.DrawingShape.Arrow:            return ScoreArrow(f);
            case MoveData.DrawingShape.MultipleCircles:  return ScoreMultipleCircles(f);
            case MoveData.DrawingShape.Star:             return ScoreStar(f);
            case MoveData.DrawingShape.Square:           return ScoreSquare(f);
            case MoveData.DrawingShape.Triangle:         return ScoreTriangle(f);
            case MoveData.DrawingShape.Checkmark:        return ScoreCheckmark(f);
            case MoveData.DrawingShape.Spiral:           return ScoreSpiral(f);
            default: return 0f;
        }
    }

    /// <summary> Circle: single closed round stroke </summary>
    private float ScoreCircle(DrawingFeatures f)
    {
        float score = 0f;
        if (f.strokeCount == 1) score += 0.25f;
        else if (f.strokeCount == 2) score += 0.1f;
        else return 0.05f;

        if (f.circularStrokes >= 1) score += 0.4f;
        if (f.curvedStrokes >= 1) score += 0.2f;   // Circles are curved — strong signal

        // Hand-drawn circles often have minor sharp turns — only penalize heavily if very spiky
        if (f.spikyStrokes == 0) score += 0.1f;
        else score *= 0.7f;    // Mild penalty (hand-drawn imperfection is expected)

        float ar = f.aspectRatio;
        if (ar > 0.6f && ar < 1.6f) score += 0.1f; // Roughly round

        return Mathf.Clamp01(score);
    }

    /// <summary> StraightLine: single straight open stroke </summary>
    private float ScoreStraightLine(DrawingFeatures f)
    {
        float score = 0f;
        if (f.strokeCount == 1) score += 0.4f;
        else if (f.strokeCount == 2) score += 0.1f;
        else return 0f;

        if (f.circularStrokes == 0) score += 0.2f;
        else score *= 0.2f;
        if (f.spikyStrokes == 0) score += 0.15f;
        else score *= 0.3f;
        if (f.curvedStrokes == 0) score += 0.2f;
        else score *= 0.5f;

        float size = Mathf.Max(f.width, f.height);
        if (size > 0.5f) score += 0.1f;

        return Mathf.Clamp01(score);
    }

    /// <summary> Zigzag: single stroke with many sharp turns, OPEN (not closed) </summary>
    private float ScoreZigzag(DrawingFeatures f)
    {
        float score = 0f;
        if (f.spikyStrokes >= 1) score += 0.45f;
        if (f.spikyStrokes >= 2) score += 0.15f;

        // Zigzags are OPEN (not closed) — strongly penalize closed shapes (that's a square/triangle)
        if (f.circularStrokes > 0) score *= 0.2f;

        // 1-2 strokes preferred
        if (f.strokeCount >= 1 && f.strokeCount <= 2) score += 0.15f;
        else if (f.strokeCount <= 4) score += 0.05f;

        // Slightly prefer elongated shapes, but zigzags can also be square-ish (W/M shape)
        if (f.aspectRatio < 0.5f || f.aspectRatio > 2.0f) score += 0.15f;
        else if (f.aspectRatio < 0.7f || f.aspectRatio > 1.4f) score += 0.1f;
        else score += 0.05f;  // Small baseline — don't zero out square-ish zigzags

        return Mathf.Clamp01(score);
    }

    /// <summary> WavyLine: single curved horizontal stroke </summary>
    private float ScoreWavyLine(DrawingFeatures f)
    {
        float score = 0f;

        // Should be curved
        if (f.curvedStrokes >= 1) score += 0.4f;

        // Should be horizontal (wider than tall)
        if (f.aspectRatio < 0.7f) score += 0.3f;
        else if (f.aspectRatio < 1.0f) score += 0.15f;

        if (f.horizontalStrokes >= 1) score += 0.15f;

        // Should NOT be spiky or circular
        if (f.spikyStrokes > 0) score *= 0.4f;
        if (f.circularStrokes > 0) score *= 0.4f;

        // 1-2 strokes
        if (f.strokeCount <= 2) score += 0.1f;

        return Mathf.Clamp01(score);
    }

    /// <summary> Plus: two crossing strokes, one horizontal + one vertical </summary>
    private float ScorePlus(DrawingFeatures f)
    {
        float score = 0f;

        // Must be exactly 2 strokes
        if (f.strokeCount == 2) score += 0.35f;
        else if (f.strokeCount == 3) score += 0.1f;
        else return 0.05f;

        // One horizontal + one vertical
        if (f.horizontalStrokes >= 1 && f.verticalStrokes >= 1) score += 0.4f;
        else if (f.horizontalStrokes >= 1 || f.verticalStrokes >= 1) score += 0.15f;

        // Roughly square proportions
        float ar = f.aspectRatio;
        if (ar > 0.5f && ar < 2.0f) score += 0.15f;

        // Should NOT be circular
        if (f.circularStrokes > 0) score *= 0.3f;

        return Mathf.Clamp01(score);
    }

    /// <summary> XCross: two crossing diagonal strokes </summary>
    private float ScoreXCross(DrawingFeatures f)
    {
        float score = 0f;

        // Must be exactly 2 strokes — this is the strongest signal for X
        if (f.strokeCount == 2) score += 0.4f;
        else if (f.strokeCount == 3) score += 0.1f;
        else return 0.05f;

        // X strokes are diagonal — neither purely horizontal nor purely vertical.
        // Hand-drawn X may have one stroke slightly classified as H or V, so be lenient.
        bool hasDiagonal = (f.horizontalStrokes == 0 && f.verticalStrokes == 0);
        if (hasDiagonal) score += 0.35f;
        else if (f.horizontalStrokes + f.verticalStrokes <= 1) score += 0.25f;
        else score += 0.1f;  // Even with H+V classification, still give some score for 2 strokes

        // Roughly square proportions (X is symmetric)
        float ar = f.aspectRatio;
        if (ar > 0.5f && ar < 2.0f) score += 0.15f;

        // X strokes are OPEN — should not be circular (closed)
        if (f.circularStrokes > 0) score *= 0.3f;

        return Mathf.Clamp01(score);
    }

    /// <summary> Arrow: 2-3 strokes with a line + V tip (sharp pointed structure) </summary>
    private float ScoreArrow(DrawingFeatures f)
    {
        float score = 0f;

        // 2-3 strokes (shaft + head)
        if (f.strokeCount >= 2 && f.strokeCount <= 3) score += 0.35f;
        else if (f.strokeCount == 1 && f.spikyStrokes >= 1) score += 0.15f;
        else return 0.05f;

        // Arrow tip creates sharp angles
        if (f.spikyStrokes >= 1) score += 0.3f;

        // Should be directional (wider or taller than round)
        if (f.aspectRatio < 0.7f || f.aspectRatio > 1.4f) score += 0.15f;

        // Should NOT be circular
        if (f.circularStrokes > 0) score *= 0.3f;

        // Some size
        float size = Mathf.Max(f.width, f.height);
        if (size > 0.5f) score += 0.1f;

        return Mathf.Clamp01(score);
    }

    /// <summary> MultipleCircles: 3+ small circular strokes </summary>
    private float ScoreMultipleCircles(DrawingFeatures f)
    {
        float score = 0f;

        if (f.circularStrokes >= 3) score += 0.5f;
        else if (f.circularStrokes >= 2) score += 0.3f;
        else if (f.circularStrokes >= 1) score += 0.1f;

        if (f.strokeCount >= 3) score += 0.3f;
        else if (f.strokeCount >= 2) score += 0.1f;

        // Spread out area suggests multiple shapes
        float avgSize = (f.width + f.height) / 2f;
        if (avgSize > 1f) score += 0.15f;

        // Penalty for spiky (circles shouldn't be angular)
        if (f.spikyStrokes > 0) score *= 0.5f;

        return Mathf.Clamp01(score);
    }

    /// <summary> Star: 3+ strokes radiating outward from center </summary>
    private float ScoreStar(DrawingFeatures f)
    {
        float score = 0f;

        // Needs multiple strokes
        if (f.strokeCount >= 4) score += 0.35f;
        else if (f.strokeCount >= 3) score += 0.25f;
        else return 0.05f;

        // Mix of directions (radiating pattern)
        if (f.horizontalStrokes >= 1 && f.verticalStrokes >= 1) score += 0.25f;
        else if (f.horizontalStrokes >= 1 || f.verticalStrokes >= 1) score += 0.1f;

        // Roughly equal width/height (radiating pattern)
        float ar = f.aspectRatio;
        if (ar > 0.5f && ar < 2.0f) score += 0.15f;

        // Should NOT be circular (star arms are open)
        if (f.circularStrokes > 0) score *= 0.5f;

        // Bonus for varied stroke types
        if (f.spikyStrokes >= 1 || f.curvedStrokes >= 1) score += 0.1f;

        return Mathf.Clamp01(score);
    }

    /// <summary> Square: closed shape with sharp corners, roughly equal aspect </summary>
    private float ScoreSquare(DrawingFeatures f)
    {
        float score = 0f;

        // 1 stroke (continuous) or up to 4 (sides) — 2 strokes is unusual for a square
        if (f.strokeCount == 1 || f.strokeCount == 4) score += 0.2f;
        else if (f.strokeCount == 3) score += 0.15f;
        else if (f.strokeCount == 2) score += 0.05f;  // 2 strokes is more likely X or plus
        else return 0.05f;

        // A square must be CLOSED — open strokes are not a square
        if (f.circularStrokes >= 1 && f.spikyStrokes >= 1) score += 0.4f;   // Perfect: closed + corners
        else if (f.circularStrokes >= 1) score += 0.1f;  // Closed but no corners = maybe sloppy square
        else if (f.spikyStrokes >= 1) score += 0.15f;    // Sharp corners but not closed — weak signal
        else return 0.05f;

        // Squares have a near-equal aspect ratio (key differentiator from triangle)
        float ar = f.aspectRatio;
        if (ar > 0.7f && ar < 1.4f) score += 0.2f;    // Near-square ratio: strong bonus
        else if (ar > 0.5f && ar < 2.0f) score += 0.05f;

        // Squares have straight edges — curved strokes strongly suggest circle, not square
        if (f.curvedStrokes == 0) score += 0.15f;
        else score *= 0.5f;

        return Mathf.Clamp01(score);
    }

    /// <summary> Triangle: closed shape with 3 sharp corners </summary>
    private float ScoreTriangle(DrawingFeatures f)
    {
        float score = 0f;

        // 1 stroke (continuous) or 3 (sides) — 2 strokes is unusual for a triangle
        if (f.strokeCount == 1 || f.strokeCount == 3) score += 0.2f;
        else if (f.strokeCount == 2) score += 0.05f;  // 2 strokes is more likely X or plus
        else if (f.strokeCount == 4) score += 0.05f;   // 4 strokes more likely square
        else return 0.05f;

        // A triangle must be CLOSED — open strokes are not a triangle
        if (f.circularStrokes >= 1 && f.spikyStrokes >= 1) score += 0.4f;   // Perfect: closed + corners
        else if (f.circularStrokes >= 1) score += 0.1f;  // Closed but no corners = maybe a sloppy triangle
        else if (f.spikyStrokes >= 1) score += 0.15f;    // Sharp corners but not closed — weak signal
        else return 0.05f;

        // Triangles tend to be taller (pointed top) — key differentiator from square
        float ar = f.aspectRatio;
        if (ar > 1.0f && ar < 2.5f) score += 0.2f;      // Taller than wide: strong triangle signal
        else if (ar > 0.7f && ar < 1.0f) score += 0.1f;  // Roughly square: mild bonus (equilateral triangle)
        else if (ar <= 0.7f) score += 0.05f;              // Very wide: less likely triangle

        // Triangles have straight edges — curved strokes suggest circle
        if (f.curvedStrokes == 0) score += 0.15f;
        else score *= 0.5f;

        return Mathf.Clamp01(score);
    }

    /// <summary> Checkmark: V-shaped single stroke with one sharp turn </summary>
    private float ScoreCheckmark(DrawingFeatures f)
    {
        float score = 0f;

        // 1 stroke strongly preferred (checkmark is one continuous stroke)
        // Arrow typically needs 2-3 strokes (shaft + head), so 1 stroke is a key differentiator
        if (f.strokeCount == 1) score += 0.4f;
        else if (f.strokeCount == 2) score += 0.1f;
        else return 0.05f;

        // Has at least one sharp turn (the V point)
        if (f.spikyStrokes >= 1) score += 0.3f;

        // Should NOT be closed
        if (f.circularStrokes == 0) score += 0.15f;
        else score *= 0.3f;

        // Checkmarks are typically wider than tall or roughly square
        if (f.aspectRatio < 1.3f) score += 0.1f;

        return Mathf.Clamp01(score);
    }

    /// <summary> Spiral: single curved stroke (open or closed) </summary>
    private float ScoreSpiral(DrawingFeatures f)
    {
        float score = 0f;

        // Should be 1 stroke
        if (f.strokeCount == 1) score += 0.3f;
        else if (f.strokeCount == 2) score += 0.1f;
        else return 0.05f;

        // Must be curved — this is the defining feature
        if (f.curvedStrokes >= 1) score += 0.45f;

        // Spirals can be open or closed — accept both, slight bonus for open
        if (f.circularStrokes == 0 && f.curvedStrokes >= 1) score += 0.15f;
        else if (f.circularStrokes >= 1 && f.curvedStrokes >= 1) score += 0.1f;

        // Should NOT be spiky
        if (f.spikyStrokes == 0) score += 0.1f;
        else score *= 0.5f;

        return Mathf.Clamp01(score);
    }

    // ═══════════════════════════════════════════════════════════════════
    // FEATURE EXTRACTION
    // ═══════════════════════════════════════════════════════════════════

    private DrawingFeatures ExtractFeatures(List<LineRenderer> strokes)
    {
        DrawingFeatures features = new DrawingFeatures();
        List<Vector3> allPoints = new List<Vector3>();

        foreach (var stroke in strokes)
        {
            if (stroke == null) continue;
            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);
            allPoints.AddRange(positions);
        }

        if (allPoints.Count == 0) return features;

        float minX = allPoints.Min(p => p.x);
        float maxX = allPoints.Max(p => p.x);
        float minY = allPoints.Min(p => p.y);
        float maxY = allPoints.Max(p => p.y);

        features.width = maxX - minX;
        features.height = maxY - minY;
        features.aspectRatio = features.height / Mathf.Max(features.width, 0.001f);
        features.strokeCount = strokes.Count;

        features.circularStrokes = CountCircularStrokes(strokes);
        features.verticalStrokes = CountVerticalStrokes(strokes);
        features.horizontalStrokes = CountHorizontalStrokes(strokes);
        features.spikyStrokes = CountSpikyStrokes(strokes);
        features.curvedStrokes = CountCurvedStrokes(strokes);

        Debug.Log($"Features: W={features.width:F2}, H={features.height:F2}, Aspect={features.aspectRatio:F2}, Strokes={features.strokeCount}");
        Debug.Log($"Patterns: Circ={features.circularStrokes}, Vert={features.verticalStrokes}, Horiz={features.horizontalStrokes}, Spiky={features.spikyStrokes}, Curved={features.curvedStrokes}");

        return features;
    }

    // ═══════════════════════════════════════════════════════════════════
    // FEATURE DETECTION HELPERS
    // ═══════════════════════════════════════════════════════════════════

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
                count++;
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

            if (verticalExtent > horizontalExtent * 1.1f)
                count++;
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

            if (horizontalExtent > verticalExtent * 1.1f)
                count++;
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

            // Downsample to ~20-30 evenly spaced points so that direction
            // changes at zigzag corners aren't diluted across many tiny segments.
            Vector3[] sampled = DownsamplePoints(positions, 25);

            int sharpTurns = 0;
            for (int i = 1; i < sampled.Length - 1; i++)
            {
                Vector3 dir1 = (sampled[i] - sampled[i - 1]).normalized;
                Vector3 dir2 = (sampled[i + 1] - sampled[i]).normalized;
                float angle = Vector3.Angle(dir1, dir2);
                if (angle > 45f) sharpTurns++;
            }

            if (sharpTurns >= 2) count++;
        }
        return count;
    }

    /// <summary>
    /// Downsample a point array to at most maxPoints evenly spaced samples.
    /// If the array already has fewer points, return it unchanged.
    /// </summary>
    private Vector3[] DownsamplePoints(Vector3[] positions, int maxPoints)
    {
        if (positions.Length <= maxPoints) return positions;

        Vector3[] sampled = new Vector3[maxPoints];
        for (int i = 0; i < maxPoints; i++)
        {
            float t = (float)i / (maxPoints - 1);
            int idx = Mathf.RoundToInt(t * (positions.Length - 1));
            sampled[i] = positions[idx];
        }
        return sampled;
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
            // Accept smooth curves and tighter curves like spirals (up to 60)
            if (avgAngle > 5f && avgAngle < 60f)
                count++;
        }
        return count;
    }

    private float CalculateStrokeLength(Vector3[] positions)
    {
        float length = 0f;
        for (int i = 1; i < positions.Length; i++)
            length += Vector3.Distance(positions[i - 1], positions[i]);
        return length;
    }

    // ═══════════════════════════════════════════════════════════════════
    // QUALITY SCORING
    // ═══════════════════════════════════════════════════════════════════

    private float CalculateDrawingQuality(List<LineRenderer> strokes, MoveData.DrawingShape shape)
    {
        if (strokes == null || strokes.Count == 0) return 0f;
        ShapeFeatures sf = AnalyzeShapeFeatures(strokes);
        float quality = CalculateShapeQuality(shape, sf);
        return Mathf.Clamp01(quality);
    }

    private float CalculateShapeQuality(MoveData.DrawingShape shape, ShapeFeatures f)
    {
        switch (shape)
        {
            case MoveData.DrawingShape.Circle:
                return QualityClosedRound(f);
            case MoveData.DrawingShape.Square:
            case MoveData.DrawingShape.Triangle:
                return QualityClosedAngular(f);
            case MoveData.DrawingShape.StraightLine:
                return QualityStraightLine(f);
            case MoveData.DrawingShape.Zigzag:
                return QualityZigzag(f);
            case MoveData.DrawingShape.WavyLine:
            case MoveData.DrawingShape.Spiral:
                return QualityCurvedLine(f);
            case MoveData.DrawingShape.Plus:
            case MoveData.DrawingShape.XCross:
                return QualityCrossing(f);
            case MoveData.DrawingShape.Arrow:
            case MoveData.DrawingShape.Checkmark:
                return QualityPointed(f);
            case MoveData.DrawingShape.MultipleCircles:
                return QualityMultiCircle(f);
            case MoveData.DrawingShape.Star:
                return QualityRadial(f);
            default:
                return QualityGeneric(f);
        }
    }

    // Closed round shapes (circle) - reward closedness + roundness
    private float QualityClosedRound(ShapeFeatures f)
    {
        float s = 0f;
        s += (1f - Mathf.Abs(1f - f.aspectRatio)) * 0.4f;
        s += f.compactness * 0.4f;
        s += f.curviness * 0.2f;
        return Mathf.Clamp01(s);
    }

    // Closed angular shapes (square, triangle) - reward closedness + sharpness
    private float QualityClosedAngular(ShapeFeatures f)
    {
        float s = 0f;
        s += f.compactness * 0.35f;
        s += (1f - f.curviness) * 0.3f;   // Reward angular lines
        s += (1f - Mathf.Clamp01((f.strokeCount - 1) / 4f)) * 0.2f;
        s += (1f - Mathf.Abs(1f - f.aspectRatio)) * 0.15f;
        return Mathf.Clamp01(s);
    }

    // Straight line - reward straightness + length
    private float QualityStraightLine(ShapeFeatures f)
    {
        float s = 0f;
        s += (1f - f.curviness) * 0.4f;
        s += (1f - Mathf.Clamp01((f.strokeCount - 1) / 3f)) * 0.3f;
        s += Mathf.Clamp01(f.totalLength / 3f) * 0.3f;
        return Mathf.Clamp01(s);
    }

    // Zigzag - reward sharp turns + length
    private float QualityZigzag(ShapeFeatures f)
    {
        float s = 0f;
        s += (1f - f.curviness) * 0.3f;   // Angular
        s += Mathf.Clamp01(f.strokeCount / 3f) * 0.2f;
        s += Mathf.Clamp01(f.totalLength / 3f) * 0.3f;
        s += f.branchiness * 0.2f;
        return Mathf.Clamp01(s);
    }

    // Curved lines (wavy, spiral) - reward smooth curves
    private float QualityCurvedLine(ShapeFeatures f)
    {
        float s = 0f;
        s += f.curviness * 0.4f;
        s += Mathf.Clamp01(f.totalLength / 3f) * 0.3f;
        s += (1f - Mathf.Clamp01((f.strokeCount - 1) / 3f)) * 0.3f;
        return Mathf.Clamp01(s);
    }

    // Crossing patterns (plus, X) - reward 2 strokes + intersection
    private float QualityCrossing(ShapeFeatures f)
    {
        float s = 0f;
        // Ideal is 2 strokes
        s += (f.strokeCount == 2) ? 0.4f : Mathf.Max(0f, 0.3f - Mathf.Abs(f.strokeCount - 2) * 0.1f);
        s += Mathf.Clamp01(f.totalLength / 3f) * 0.3f;
        s += (1f - Mathf.Abs(1f - f.aspectRatio)) * 0.3f; // Roughly square proportions
        return Mathf.Clamp01(s);
    }

    // Pointed shapes (arrow, checkmark) - reward clean sharp angle
    private float QualityPointed(ShapeFeatures f)
    {
        float s = 0f;
        s += (1f - f.curviness) * 0.3f;
        s += Mathf.Clamp01(f.totalLength / 3f) * 0.3f;
        s += (1f - Mathf.Clamp01((f.strokeCount - 2) / 3f)) * 0.2f;
        s += f.branchiness * 0.2f;
        return Mathf.Clamp01(s);
    }

    // Multiple circles - reward circle count + spread
    private float QualityMultiCircle(ShapeFeatures f)
    {
        float s = 0f;
        s += Mathf.Clamp01(f.strokeCount / 3f) * 0.35f;
        s += f.curviness * 0.3f;
        s += f.compactness * 0.2f;
        s += (1f - Mathf.Abs(1f - f.aspectRatio)) * 0.15f;
        return Mathf.Clamp01(s);
    }

    // Radial patterns (star) - reward many strokes from center
    private float QualityRadial(ShapeFeatures f)
    {
        float s = 0f;
        s += Mathf.Clamp01(f.strokeCount / 4f) * 0.3f;
        s += f.radialness * 0.35f;
        s += f.branchiness * 0.2f;
        s += Mathf.Clamp01(f.totalLength / 4f) * 0.15f;
        return Mathf.Clamp01(s);
    }

    // Generic fallback
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

    // ═══════════════════════════════════════════════════════════════════
    // SHAPE FEATURES ANALYSIS (for quality scoring)
    // ═══════════════════════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════════════════════
    // FEATURE DATA STRUCTURE
    // ═══════════════════════════════════════════════════════════════════

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
