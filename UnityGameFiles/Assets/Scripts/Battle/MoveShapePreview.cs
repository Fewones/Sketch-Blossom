using UnityEngine;
using System.Collections.Generic;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Generates small reference-drawing textures for each battle move.
    /// Each texture shows the gesture the player needs to draw to trigger that move.
    /// All drawing is procedural (no external assets required).
    ///
    /// Shape labels (one per move type, matching labelMaps.json "move_shapes"):
    ///   fireball      – Fireball          single large circle
    ///   flame_wave    – FlameWave         sharp wave with tall peaks
    ///   zigzag        – Burn              jagged lightning bolt
    ///   curved_line   – VineWhip          single long sweeping arc
    ///   scattered     – LeafStorm         scattered short strokes
    ///   downward_lines– RootAttack        three parallel vertical lines
    ///   water_splash  – WaterSplash       upward-angled wave
    ///   bubbles       – Bubble            three small circles clustered
    ///   healing_wave  – HealingWave       gentle low-amplitude wave
    ///   shield        – Block             triangular shield outline
    /// </summary>
    public static class MoveShapePreview
    {
        // Maps every move type to its canonical gesture shape label.
        // Labels must match the keys used in MovesetDetector.shapeToMoveTypes
        // and in labelMaps.json "move_shapes".
        private static readonly Dictionary<MoveData.MoveType, string> moveToShape =
            new Dictionary<MoveData.MoveType, string>
            {
                { MoveData.MoveType.Block,         "shield"         },
                { MoveData.MoveType.Fireball,      "fireball"       },
                { MoveData.MoveType.FlameWave,     "flame_wave"     },
                { MoveData.MoveType.Burn,          "zigzag"         },
                { MoveData.MoveType.VineWhip,      "curved_line"    },
                { MoveData.MoveType.LeafStorm,     "scattered"      },
                { MoveData.MoveType.RootAttack,    "downward_lines" },
                { MoveData.MoveType.WaterSplash,   "water_splash"   },
                { MoveData.MoveType.Bubble,        "bubbles"        },
                { MoveData.MoveType.HealingWave,   "healing_wave"   },
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
            Color[] pixels = new Color[width * height]; // all transparent by default

            if (moveToShape.TryGetValue(moveType, out string shape))
            {
                switch (shape)
                {
                    case "fireball":       DrawFireball(pixels, width, height, drawColor);      break;
                    case "flame_wave":     DrawFlameWave(pixels, width, height, drawColor);     break;
                    case "zigzag":         DrawZigzag(pixels, width, height, drawColor);        break;
                    case "curved_line":    DrawCurvedLine(pixels, width, height, drawColor);    break;
                    case "scattered":      DrawScattered(pixels, width, height, drawColor);     break;
                    case "downward_lines": DrawDownwardLines(pixels, width, height, drawColor); break;
                    case "water_splash":   DrawWaterSplash(pixels, width, height, drawColor);   break;
                    case "bubbles":        DrawBubbles(pixels, width, height, drawColor);       break;
                    case "healing_wave":   DrawHealingWave(pixels, width, height, drawColor);   break;
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
        //  Shape drawing routines — one per move label
        // ─────────────────────────────────────────────────────

        /// <summary>Fireball — single large circle.</summary>
        private static void DrawFireball(Color[] pixels, int w, int h, Color color)
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

        /// <summary>FlameWave — sharp horizontal wave with tall aggressive peaks.</summary>
        private static void DrawFlameWave(Color[] pixels, int w, int h, Color color)
        {
            float amplitude = h * 0.38f;          // tall peaks
            float frequency = 2f * Mathf.PI / w * 2.2f; // slightly more than 2 cycles
            Vector2 prev = new Vector2(0, h * 0.5f + amplitude * Mathf.Sin(0));

            for (int x = 1; x <= w; x++)
            {
                float y = h * 0.5f + amplitude * Mathf.Sin(x * frequency);
                PaintLine(pixels, w, h, prev, new Vector2(x, y), color, 3f);
                prev = new Vector2(x, y);
            }
        }

        /// <summary>Burn — jagged lightning-bolt zigzag.</summary>
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

        /// <summary>VineWhip — single long curved Bézier arc.</summary>
        private static void DrawCurvedLine(Color[] pixels, int w, int h, Color color)
        {
            Vector2 p0 = new Vector2(w * 0.10f, h * 0.82f);
            Vector2 p1 = new Vector2(w * 0.15f, h * 0.15f); // control point
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

        /// <summary>LeafStorm — six scattered short strokes at varied angles.</summary>
        private static void DrawScattered(Color[] pixels, int w, int h, Color color)
        {
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

        /// <summary>RootAttack — three parallel vertical lines going downward.</summary>
        private static void DrawDownwardLines(Color[] pixels, int w, int h, Color color)
        {
            float[] xs = { w * 0.25f, w * 0.50f, w * 0.75f };
            foreach (float x in xs)
                PaintLine(pixels, w, h,
                    new Vector2(x, h * 0.10f),
                    new Vector2(x, h * 0.90f),
                    color, 3f);
        }

        /// <summary>WaterSplash — smooth wave oriented upward (more vertical sweep).</summary>
        private static void DrawWaterSplash(Color[] pixels, int w, int h, Color color)
        {
            // Draw a wave that rises from bottom-left, crests near the top, and
            // falls back down — giving an upward-splash silhouette.
            float cx = w * 0.5f;
            float amplitude = w * 0.30f;
            float frequency = Mathf.PI / h; // one half-cycle vertically

            Vector2 prev = new Vector2(cx + amplitude * Mathf.Sin(0), 0);
            for (int y = 1; y <= h; y++)
            {
                float x = cx + amplitude * Mathf.Sin(y * frequency * 2f);
                PaintLine(pixels, w, h, prev, new Vector2(x, y), color, 3f);
                prev = new Vector2(x, y);
            }
        }

        /// <summary>Bubble — three small circles of different sizes clustered together.</summary>
        private static void DrawBubbles(Color[] pixels, int w, int h, Color color)
        {
            (Vector2 center, float radius)[] bubbles =
            {
                (new Vector2(w * 0.35f, h * 0.55f), Mathf.Min(w, h) * 0.22f), // large, left
                (new Vector2(w * 0.68f, h * 0.42f), Mathf.Min(w, h) * 0.15f), // medium, right
                (new Vector2(w * 0.55f, h * 0.72f), Mathf.Min(w, h) * 0.10f), // small, bottom
            };

            foreach (var (center, radius) in bubbles)
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

        /// <summary>HealingWave — gentle smooth horizontal wave with low amplitude.</summary>
        private static void DrawHealingWave(Color[] pixels, int w, int h, Color color)
        {
            float amplitude = h * 0.16f;           // low, gentle
            float frequency = 2f * Mathf.PI / w * 1.5f; // ~1.5 cycles
            Vector2 prev = new Vector2(0, h * 0.5f + amplitude * Mathf.Sin(0));

            for (int x = 1; x <= w; x++)
            {
                float y = h * 0.5f + amplitude * Mathf.Sin(x * frequency);
                PaintLine(pixels, w, h, prev, new Vector2(x, y), color, 3f);
                prev = new Vector2(x, y);
            }
        }

        /// <summary>Block — triangular shield outline (flat top + two angled sides to a bottom point).</summary>
        private static void DrawShield(Color[] pixels, int w, int h, Color color)
        {
            Vector2 topL = new Vector2(w * 0.20f, h * 0.22f);
            Vector2 topR = new Vector2(w * 0.80f, h * 0.22f);
            Vector2 bot  = new Vector2(w * 0.50f, h * 0.85f);

            PaintLine(pixels, w, h, topL, topR, color, 3f); // top bar
            PaintLine(pixels, w, h, topL, bot,  color, 3f); // left side
            PaintLine(pixels, w, h, topR, bot,  color, 3f); // right side
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
