using UnityEngine;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Generates small reference-drawing textures for battle moves.
    /// Each texture shows the gesture the player needs to draw.
    /// Uses DrawingShape (not MoveType) so the same move type can show
    /// different shapes depending on which plant uses it.
    /// </summary>
    public static class MoveShapePreview
    {
        /// <summary>
        /// Generate a reference-drawing preview for the given DrawingShape.
        /// </summary>
        public static Texture2D GeneratePreview(MoveData.DrawingShape shape,
                                                int width, int height,
                                                Color drawColor)
        {
            Color[] pixels = new Color[width * height];

            switch (shape)
            {
                case MoveData.DrawingShape.Circle:          DrawCircle(pixels, width, height, drawColor);          break;
                case MoveData.DrawingShape.StraightLine:    DrawStraightLine(pixels, width, height, drawColor);    break;
                case MoveData.DrawingShape.Zigzag:          DrawZigzag(pixels, width, height, drawColor);          break;
                case MoveData.DrawingShape.WavyLine:        DrawWavyLine(pixels, width, height, drawColor);        break;
                case MoveData.DrawingShape.Plus:             DrawPlus(pixels, width, height, drawColor);            break;
                case MoveData.DrawingShape.XCross:           DrawXCross(pixels, width, height, drawColor);          break;
                case MoveData.DrawingShape.Arrow:            DrawArrow(pixels, width, height, drawColor);           break;
                case MoveData.DrawingShape.MultipleCircles:  DrawMultipleCircles(pixels, width, height, drawColor); break;
                case MoveData.DrawingShape.Star:             DrawStar(pixels, width, height, drawColor);            break;
                case MoveData.DrawingShape.Square:           DrawSquare(pixels, width, height, drawColor);          break;
                case MoveData.DrawingShape.Triangle:         DrawTriangle(pixels, width, height, drawColor);        break;
                case MoveData.DrawingShape.Checkmark:        DrawCheckmark(pixels, width, height, drawColor);       break;
                case MoveData.DrawingShape.Spiral:           DrawSpiral(pixels, width, height, drawColor);          break;
            }

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Legacy overload: accepts MoveType + plant type, looks up the DrawingShape.
        /// </summary>
        public static Texture2D GeneratePreview(MoveData.MoveType moveType,
                                                int width, int height,
                                                Color drawColor,
                                                PlantRecognitionSystem.PlantType plantType)
        {
            MoveData[] moves = MoveData.GetMovesForPlant(plantType);
            foreach (var move in moves)
            {
                if (move.moveType == moveType)
                    return GeneratePreview(move.drawingShape, width, height, drawColor);
            }
            // Fallback: return a circle
            return GeneratePreview(MoveData.DrawingShape.Circle, width, height, drawColor);
        }

        // ─────────────────────────────────────────────────────
        //  Shape drawing routines — one per DrawingShape
        // ─────────────────────────────────────────────────────

        private static void DrawCircle(Color[] pixels, int w, int h, Color color)
        {
            float cx = w * 0.5f, cy = h * 0.5f;
            float radius = Mathf.Min(w, h) * 0.38f;
            int steps = 120;
            Vector2 prev = new Vector2(cx + radius, cy);
            for (int i = 1; i <= steps; i++)
            {
                float angle = (float)i / steps * Mathf.PI * 2f;
                Vector2 next = new Vector2(cx + Mathf.Cos(angle) * radius,
                                           cy + Mathf.Sin(angle) * radius);
                PaintLine(pixels, w, h, prev, next, color, 3f);
                prev = next;
            }
        }

        private static void DrawStraightLine(Color[] pixels, int w, int h, Color color)
        {
            PaintLine(pixels, w, h,
                new Vector2(w * 0.15f, h * 0.5f),
                new Vector2(w * 0.85f, h * 0.5f),
                color, 3f);
        }

        private static void DrawZigzag(Color[] pixels, int w, int h, Color color)
        {
            Vector2[] pts =
            {
                new Vector2(w * 0.05f, h * 0.72f),
                new Vector2(w * 0.25f, h * 0.18f),
                new Vector2(w * 0.45f, h * 0.78f),
                new Vector2(w * 0.65f, h * 0.18f),
                new Vector2(w * 0.85f, h * 0.72f),
                new Vector2(w * 0.95f, h * 0.28f),
            };
            for (int i = 0; i < pts.Length - 1; i++)
                PaintLine(pixels, w, h, pts[i], pts[i + 1], color, 3f);
        }

        private static void DrawWavyLine(Color[] pixels, int w, int h, Color color)
        {
            float amplitude = h * 0.2f;
            float frequency = 2f * Mathf.PI / w * 1.8f;
            Vector2 prev = new Vector2(0, h * 0.5f);
            for (int x = 1; x <= w; x++)
            {
                float y = h * 0.5f + amplitude * Mathf.Sin(x * frequency);
                PaintLine(pixels, w, h, prev, new Vector2(x, y), color, 3f);
                prev = new Vector2(x, y);
            }
        }

        private static void DrawPlus(Color[] pixels, int w, int h, Color color)
        {
            // Horizontal bar
            PaintLine(pixels, w, h,
                new Vector2(w * 0.15f, h * 0.5f),
                new Vector2(w * 0.85f, h * 0.5f),
                color, 3f);
            // Vertical bar
            PaintLine(pixels, w, h,
                new Vector2(w * 0.5f, h * 0.15f),
                new Vector2(w * 0.5f, h * 0.85f),
                color, 3f);
        }

        private static void DrawXCross(Color[] pixels, int w, int h, Color color)
        {
            PaintLine(pixels, w, h,
                new Vector2(w * 0.15f, h * 0.15f),
                new Vector2(w * 0.85f, h * 0.85f),
                color, 3f);
            PaintLine(pixels, w, h,
                new Vector2(w * 0.85f, h * 0.15f),
                new Vector2(w * 0.15f, h * 0.85f),
                color, 3f);
        }

        private static void DrawArrow(Color[] pixels, int w, int h, Color color)
        {
            // Shaft
            PaintLine(pixels, w, h,
                new Vector2(w * 0.15f, h * 0.5f),
                new Vector2(w * 0.85f, h * 0.5f),
                color, 3f);
            // Arrowhead
            PaintLine(pixels, w, h,
                new Vector2(w * 0.85f, h * 0.5f),
                new Vector2(w * 0.65f, h * 0.25f),
                color, 3f);
            PaintLine(pixels, w, h,
                new Vector2(w * 0.85f, h * 0.5f),
                new Vector2(w * 0.65f, h * 0.75f),
                color, 3f);
        }

        private static void DrawMultipleCircles(Color[] pixels, int w, int h, Color color)
        {
            (Vector2 center, float radius)[] circles =
            {
                (new Vector2(w * 0.35f, h * 0.55f), Mathf.Min(w, h) * 0.20f),
                (new Vector2(w * 0.68f, h * 0.42f), Mathf.Min(w, h) * 0.15f),
                (new Vector2(w * 0.55f, h * 0.75f), Mathf.Min(w, h) * 0.12f),
            };
            foreach (var (center, radius) in circles)
            {
                int steps = 60;
                Vector2 prev = new Vector2(center.x + radius, center.y);
                for (int i = 1; i <= steps; i++)
                {
                    float angle = (float)i / steps * Mathf.PI * 2f;
                    Vector2 next = new Vector2(center.x + Mathf.Cos(angle) * radius,
                                               center.y + Mathf.Sin(angle) * radius);
                    PaintLine(pixels, w, h, prev, next, color, 2.5f);
                    prev = next;
                }
            }
        }

        private static void DrawStar(Color[] pixels, int w, int h, Color color)
        {
            float cx = w * 0.5f, cy = h * 0.5f;
            float armLen = Mathf.Min(w, h) * 0.38f;
            int arms = 5;
            for (int i = 0; i < arms; i++)
            {
                float angle = (float)i / arms * Mathf.PI * 2f - Mathf.PI * 0.5f;
                Vector2 tip = new Vector2(cx + Mathf.Cos(angle) * armLen,
                                          cy + Mathf.Sin(angle) * armLen);
                PaintLine(pixels, w, h, new Vector2(cx, cy), tip, color, 3f);
            }
        }

        private static void DrawSquare(Color[] pixels, int w, int h, Color color)
        {
            float m = 0.2f; // margin
            Vector2 tl = new Vector2(w * m, h * (1 - m));
            Vector2 tr = new Vector2(w * (1 - m), h * (1 - m));
            Vector2 br = new Vector2(w * (1 - m), h * m);
            Vector2 bl = new Vector2(w * m, h * m);
            PaintLine(pixels, w, h, tl, tr, color, 3f);
            PaintLine(pixels, w, h, tr, br, color, 3f);
            PaintLine(pixels, w, h, br, bl, color, 3f);
            PaintLine(pixels, w, h, bl, tl, color, 3f);
        }

        private static void DrawTriangle(Color[] pixels, int w, int h, Color color)
        {
            Vector2 top = new Vector2(w * 0.5f, h * 0.85f);
            Vector2 bl  = new Vector2(w * 0.15f, h * 0.15f);
            Vector2 br  = new Vector2(w * 0.85f, h * 0.15f);
            PaintLine(pixels, w, h, top, bl, color, 3f);
            PaintLine(pixels, w, h, bl, br, color, 3f);
            PaintLine(pixels, w, h, br, top, color, 3f);
        }

        private static void DrawCheckmark(Color[] pixels, int w, int h, Color color)
        {
            Vector2 start = new Vector2(w * 0.15f, h * 0.55f);
            Vector2 bottom = new Vector2(w * 0.4f, h * 0.2f);
            Vector2 end = new Vector2(w * 0.85f, h * 0.8f);
            PaintLine(pixels, w, h, start, bottom, color, 3f);
            PaintLine(pixels, w, h, bottom, end, color, 3f);
        }

        private static void DrawSpiral(Color[] pixels, int w, int h, Color color)
        {
            float cx = w * 0.5f, cy = h * 0.5f;
            float maxRadius = Mathf.Min(w, h) * 0.4f;
            int steps = 180;
            float totalRevolutions = 2.5f;
            Vector2 prev = new Vector2(cx, cy);
            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / steps;
                float angle = t * totalRevolutions * Mathf.PI * 2f;
                float radius = t * maxRadius;
                Vector2 next = new Vector2(cx + Mathf.Cos(angle) * radius,
                                           cy + Mathf.Sin(angle) * radius);
                PaintLine(pixels, w, h, prev, next, color, 2.5f);
                prev = next;
            }
        }

        // ─────────────────────────────────────────────────────
        //  Low-level pixel painters
        // ─────────────────────────────────────────────────────

        private static void PaintLine(Color[] pixels, int w, int h,
                                      Vector2 from, Vector2 to,
                                      Color color, float thickness)
        {
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, to)));
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                PaintDot(pixels, w, h, Vector2.Lerp(from, to, t), color, thickness);
            }
        }

        private static void PaintDot(Color[] pixels, int w, int h,
                                     Vector2 center, Color color, float radius)
        {
            int r  = Mathf.CeilToInt(radius);
            int cx = Mathf.RoundToInt(center.x);
            int cy = Mathf.RoundToInt(center.y);

            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > radius) continue;

                    int px = cx + dx;
                    int py = cy + dy;
                    if (px < 0 || px >= w || py < 0 || py >= h) continue;

                    int idx = py * w + px;
                    float alpha = Mathf.Clamp01(1f - Mathf.Max(0f, dist - (radius - 1f)));
                    Color src = new Color(color.r, color.g, color.b, color.a * alpha);
                    pixels[idx] = AlphaBlend(pixels[idx], src);
                }
            }
        }

        private static Color AlphaBlend(Color dst, Color src)
        {
            float a = src.a + dst.a * (1f - src.a);
            if (a <= 0f) return Color.clear;
            float r = (src.r * src.a + dst.r * dst.a * (1f - src.a)) / a;
            float g = (src.g * src.a + dst.g * dst.a * (1f - src.a)) / a;
            float b = (src.b * src.a + dst.b * dst.a * (1f - src.a)) / a;
            return new Color(r, g, b, a);
        }
    }
}
