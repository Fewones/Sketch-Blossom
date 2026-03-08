using UnityEngine;
using System.Collections.Generic;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Generates small reference-drawing textures for each battle move.
    /// Each texture shows the gesture the player needs to draw to trigger that move.
    /// All drawing is procedural (no external assets required).
    /// </summary>
    public static class MoveShapePreview
    {
        // Map every move type to its canonical gesture shape name.
        private static readonly Dictionary<MoveData.MoveType, string> moveToShape =
            new Dictionary<MoveData.MoveType, string>
            {
                { MoveData.MoveType.Block,         "shield"         },
                { MoveData.MoveType.Fireball,      "circle"         },
                { MoveData.MoveType.FlameWave,     "wave"           },
                { MoveData.MoveType.Burn,          "zigzag"         },
                { MoveData.MoveType.VineWhip,      "curved_line"    },
                { MoveData.MoveType.LeafStorm,     "scattered"      },
                { MoveData.MoveType.RootAttack,    "downward_lines" },
                { MoveData.MoveType.WaterSplash,   "wave"           },
                { MoveData.MoveType.Bubble,        "circle"         },
                { MoveData.MoveType.HealingWave,   "wave"           },
            };

        /// <summary>
        /// Generate a reference-drawing preview texture for the given move.
        /// The shape is drawn in <paramref name="drawColor"/> on a transparent background.
        /// Returns null for unrecognised move types.
        /// </summary>
        public static Texture2D GeneratePreview(MoveData.MoveType moveType,
                                                int width, int height,
                                                Color drawColor)
        {
            Color[] pixels = new Color[width * height]; // all transparent (default)

            if (moveToShape.TryGetValue(moveType, out string shape))
            {
                switch (shape)
                {
                    case "circle":         DrawCircle(pixels, width, height, drawColor);        break;
                    case "wave":           DrawWave(pixels, width, height, drawColor);          break;
                    case "zigzag":         DrawZigzag(pixels, width, height, drawColor);        break;
                    case "downward_lines": DrawDownwardLines(pixels, width, height, drawColor); break;
                    case "scattered":      DrawScattered(pixels, width, height, drawColor);     break;
                    case "curved_line":    DrawCurvedLine(pixels, width, height, drawColor);    break;
                    case "shield":         DrawShield(pixels, width, height, drawColor);        break;
                }
            }

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        // ─────────────────────────────────────────────────────
        //  Shape drawing routines
        // ─────────────────────────────────────────────────────

        private static void DrawCircle(Color[] pixels, int w, int h, Color color)
        {
            float cx = w * 0.5f, cy = h * 0.5f;
            float radius = Mathf.Min(w, h) * 0.36f;
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

        private static void DrawWave(Color[] pixels, int w, int h, Color color)
        {
            float amplitude = h * 0.28f;
            float frequency = 2f * Mathf.PI / w * 1.8f; // ~1.8 full cycles across width
            Vector2 prev = new Vector2(0, h * 0.5f + amplitude * Mathf.Sin(0));

            for (int x = 1; x <= w; x++)
            {
                float y = h * 0.5f + amplitude * Mathf.Sin(x * frequency);
                Vector2 next = new Vector2(x, y);
                PaintLine(pixels, w, h, prev, next, color, 3f);
                prev = next;
            }
        }

        private static void DrawZigzag(Color[] pixels, int w, int h, Color color)
        {
            Vector2[] pts = new Vector2[]
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

        private static void DrawDownwardLines(Color[] pixels, int w, int h, Color color)
        {
            float[] xs = { w * 0.25f, w * 0.5f, w * 0.75f };
            foreach (float x in xs)
                PaintLine(pixels, w, h,
                    new Vector2(x, h * 0.1f),
                    new Vector2(x, h * 0.9f),
                    color, 3f);
        }

        private static void DrawScattered(Color[] pixels, int w, int h, Color color)
        {
            // Fixed positions so the preview is always deterministic.
            (Vector2 center, float angleDeg, float halfLen)[] strokes =
            {
                (new Vector2(w * 0.20f, h * 0.28f),  40f,  w * 0.13f),
                (new Vector2(w * 0.55f, h * 0.18f), -25f,  w * 0.13f),
                (new Vector2(w * 0.80f, h * 0.38f),  75f,  w * 0.13f),
                (new Vector2(w * 0.30f, h * 0.68f), -55f,  w * 0.13f),
                (new Vector2(w * 0.68f, h * 0.72f),  15f,  w * 0.13f),
                (new Vector2(w * 0.10f, h * 0.55f), -80f,  w * 0.10f),
            };

            foreach (var (center, angleDeg, halfLen) in strokes)
            {
                float rad = angleDeg * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * halfLen;
                PaintLine(pixels, w, h, center - dir, center + dir, color, 2.5f);
            }
        }

        private static void DrawCurvedLine(Color[] pixels, int w, int h, Color color)
        {
            // Quadratic Bézier: bottom-left → control point (top-left) → top-right
            Vector2 p0 = new Vector2(w * 0.10f, h * 0.82f);
            Vector2 p1 = new Vector2(w * 0.15f, h * 0.15f);
            Vector2 p2 = new Vector2(w * 0.90f, h * 0.20f);

            int steps = 80;
            Vector2 prev = p0;
            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector2 next = (1 - t) * (1 - t) * p0
                             + 2f * (1 - t) * t * p1
                             + t * t * p2;
                PaintLine(pixels, w, h, prev, next, color, 3f);
                prev = next;
            }
        }

        private static void DrawShield(Color[] pixels, int w, int h, Color color)
        {
            // Flat top + two angled sides meeting at a bottom point.
            Vector2 topL  = new Vector2(w * 0.20f, h * 0.22f);
            Vector2 topR  = new Vector2(w * 0.80f, h * 0.22f);
            Vector2 bot   = new Vector2(w * 0.50f, h * 0.85f);

            PaintLine(pixels, w, h, topL, topR, color, 3f);  // top bar
            PaintLine(pixels, w, h, topL, bot,  color, 3f);  // left side
            PaintLine(pixels, w, h, topR, bot,  color, 3f);  // right side
        }

        // ─────────────────────────────────────────────────────
        //  Low-level pixel painters
        // ─────────────────────────────────────────────────────

        /// <summary>Paint a thick line between two points into the pixel array.</summary>
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

        /// <summary>Paint an anti-aliased filled circle at a point.</summary>
        private static void PaintDot(Color[] pixels, int w, int h,
                                     Vector2 center, Color color, float radius)
        {
            int r = Mathf.CeilToInt(radius);
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
                    // Anti-alias: fade at the outer edge of the brush.
                    float alpha = Mathf.Clamp01(1f - Mathf.Max(0f, dist - (radius - 1f)));
                    Color src = new Color(color.r, color.g, color.b, color.a * alpha);
                    // Alpha-blend over any existing pixel.
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
