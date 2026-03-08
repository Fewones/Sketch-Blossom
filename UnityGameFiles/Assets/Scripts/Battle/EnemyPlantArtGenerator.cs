using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates procedural art for enemy plants in battle.
/// Each of the 9 plant types has 3 distinct art variations, all drawn programmatically.
/// Art is generated as Texture2D sprites at runtime.
/// </summary>
public static class EnemyPlantArtGenerator
{
    private const int TEX_SIZE = 256;

    /// <summary>
    /// Get the generated art for a specific plant type and variant (0-2)
    /// </summary>
    public static Texture2D GeneratePlantArt(PlantRecognitionSystem.PlantType plantType, int variant)
    {
        variant = Mathf.Clamp(variant, 0, 2);

        switch (plantType)
        {
            // Fire plants
            case PlantRecognitionSystem.PlantType.Sunflower:
                return GenerateSunflower(variant);
            case PlantRecognitionSystem.PlantType.FireRose:
                return GenerateFireRose(variant);
            case PlantRecognitionSystem.PlantType.FlameTulip:
                return GenerateFlameTulip(variant);

            // Grass plants
            case PlantRecognitionSystem.PlantType.Cactus:
                return GenerateCactus(variant);
            case PlantRecognitionSystem.PlantType.VineFlower:
                return GenerateVineFlower(variant);
            case PlantRecognitionSystem.PlantType.GrassSprout:
                return GenerateGrassSprout(variant);

            // Water plants
            case PlantRecognitionSystem.PlantType.WaterLily:
                return GenerateWaterLily(variant);
            case PlantRecognitionSystem.PlantType.CoralBloom:
                return GenerateCoralBloom(variant);
            case PlantRecognitionSystem.PlantType.BubbleFlower:
                return GenerateBubbleFlower(variant);

            default:
                return GenerateSunflower(0);
        }
    }

    // ========== FIRE PLANTS ==========

    private static Texture2D GenerateSunflower(int variant)
    {
        Texture2D tex = CreateBlankTexture();
        Color petalColor, centerColor, stemColor;

        switch (variant)
        {
            case 0: // Classic bright sunflower
                petalColor = new Color(1f, 0.8f, 0.1f);
                centerColor = new Color(0.6f, 0.2f, 0f);
                stemColor = new Color(0.2f, 0.5f, 0.1f);
                break;
            case 1: // Deep orange sunflower
                petalColor = new Color(1f, 0.5f, 0.1f);
                centerColor = new Color(0.4f, 0.1f, 0f);
                stemColor = new Color(0.3f, 0.4f, 0.1f);
                break;
            default: // Crimson sunflower
                petalColor = new Color(0.9f, 0.2f, 0.1f);
                centerColor = new Color(0.3f, 0.05f, 0f);
                stemColor = new Color(0.15f, 0.35f, 0.1f);
                break;
        }

        int cx = TEX_SIZE / 2, cy = TEX_SIZE / 2 + 20;

        // Stem
        DrawRect(tex, cx - 4, 10, 8, cy - 40, stemColor);

        // Petals (different count per variant)
        int petalCount = 8 + variant * 2;
        float petalLen = 40 + variant * 5;
        for (int i = 0; i < petalCount; i++)
        {
            float angle = (360f / petalCount) * i * Mathf.Deg2Rad;
            DrawPetal(tex, cx, cy, angle, petalLen, 12, petalColor);
        }

        // Center
        DrawCircle(tex, cx, cy, 20 + variant * 3, centerColor);

        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateFireRose(int variant)
    {
        Texture2D tex = CreateBlankTexture();
        Color outerColor, innerColor, stemColor;

        switch (variant)
        {
            case 0: // Classic red rose
                outerColor = new Color(0.8f, 0.1f, 0.1f);
                innerColor = new Color(1f, 0.3f, 0.2f);
                stemColor = new Color(0.2f, 0.4f, 0.1f);
                break;
            case 1: // Magenta rose
                outerColor = new Color(0.9f, 0.1f, 0.4f);
                innerColor = new Color(1f, 0.4f, 0.5f);
                stemColor = new Color(0.15f, 0.35f, 0.15f);
                break;
            default: // Dark crimson rose with orange glow
                outerColor = new Color(0.5f, 0.05f, 0.05f);
                innerColor = new Color(1f, 0.5f, 0.1f);
                stemColor = new Color(0.2f, 0.3f, 0.05f);
                break;
        }

        int cx = TEX_SIZE / 2, cy = TEX_SIZE / 2 + 15;

        // Stem with thorns
        DrawRect(tex, cx - 3, 10, 6, cy - 30, stemColor);
        // Thorns
        DrawRect(tex, cx - 10, cy - 60, 8, 4, stemColor);
        DrawRect(tex, cx + 5, cy - 80, 8, 4, stemColor);

        // Rose layers (concentric circles with petals)
        int layers = 3 + variant;
        for (int l = layers; l >= 0; l--)
        {
            float radius = 15 + l * 10;
            Color layerColor = Color.Lerp(innerColor, outerColor, (float)l / layers);
            int petalCount = 5 + l;
            for (int p = 0; p < petalCount; p++)
            {
                float angle = ((360f / petalCount) * p + l * 15) * Mathf.Deg2Rad;
                DrawPetal(tex, cx, cy, angle, radius, 10 + l * 2, layerColor);
            }
        }

        // Center bud
        DrawCircle(tex, cx, cy, 10, innerColor);

        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateFlameTulip(int variant)
    {
        Texture2D tex = CreateBlankTexture();
        Color tipColor, baseColor, stemColor;

        switch (variant)
        {
            case 0: // Orange-yellow flame
                tipColor = new Color(1f, 0.9f, 0.1f);
                baseColor = new Color(1f, 0.4f, 0f);
                stemColor = new Color(0.2f, 0.5f, 0.15f);
                break;
            case 1: // Blue-white flame
                tipColor = new Color(0.8f, 0.9f, 1f);
                baseColor = new Color(0.3f, 0.4f, 1f);
                stemColor = new Color(0.1f, 0.3f, 0.2f);
                break;
            default: // Purple-red flame
                tipColor = new Color(1f, 0.3f, 0.1f);
                baseColor = new Color(0.5f, 0.1f, 0.5f);
                stemColor = new Color(0.2f, 0.35f, 0.1f);
                break;
        }

        int cx = TEX_SIZE / 2, baseCy = TEX_SIZE / 2 - 10;

        // Tall stem
        DrawRect(tex, cx - 3, 10, 6, baseCy - 10, stemColor);

        // Tulip cup shape - 3 main petals pointing up
        int petalWidth = 18 + variant * 3;
        for (int p = -1; p <= 1; p++)
        {
            float offsetX = p * petalWidth * 0.6f;
            Color c = Color.Lerp(baseColor, tipColor, 0.5f + p * 0.2f);
            // Draw tapered petal going upward
            for (int h = 0; h < 60 + variant * 10; h++)
            {
                float t = (float)h / (60 + variant * 10);
                int w = Mathf.RoundToInt(Mathf.Lerp(petalWidth, 3, t * t));
                Color pixelColor = Color.Lerp(baseColor, tipColor, t);
                int px = Mathf.RoundToInt(cx + offsetX * (1f - t * 0.5f));
                int py = baseCy + h;
                DrawRect(tex, px - w / 2, py, w, 2, pixelColor);
            }
        }

        tex.Apply();
        return tex;
    }

    // ========== GRASS PLANTS ==========

    private static Texture2D GenerateCactus(int variant)
    {
        Texture2D tex = CreateBlankTexture();
        Color bodyColor, spikeColor, flowerColor;

        switch (variant)
        {
            case 0: // Classic green cactus
                bodyColor = new Color(0.2f, 0.7f, 0.2f);
                spikeColor = new Color(0.9f, 0.9f, 0.6f);
                flowerColor = new Color(1f, 0.4f, 0.6f);
                break;
            case 1: // Blue-green cactus
                bodyColor = new Color(0.15f, 0.55f, 0.45f);
                spikeColor = new Color(0.8f, 0.8f, 0.7f);
                flowerColor = new Color(0.9f, 0.9f, 0.2f);
                break;
            default: // Dark sage cactus
                bodyColor = new Color(0.3f, 0.5f, 0.2f);
                spikeColor = new Color(0.7f, 0.65f, 0.5f);
                flowerColor = new Color(1f, 0.2f, 0.2f);
                break;
        }

        int cx = TEX_SIZE / 2, baseY = 30;

        // Main body
        int bodyW = 30 + variant * 5;
        int bodyH = 120 + variant * 10;
        DrawRoundedRect(tex, cx - bodyW / 2, baseY, bodyW, bodyH, bodyColor);

        // Arms (different positions per variant)
        if (variant == 0)
        {
            DrawRect(tex, cx - bodyW / 2 - 20, baseY + 50, 20, 12, bodyColor);
            DrawRect(tex, cx - bodyW / 2 - 20, baseY + 50, 12, 40, bodyColor);
            DrawRect(tex, cx + bodyW / 2, baseY + 70, 20, 12, bodyColor);
            DrawRect(tex, cx + bodyW / 2 + 8, baseY + 70, 12, 35, bodyColor);
        }
        else if (variant == 1)
        {
            DrawRect(tex, cx - bodyW / 2 - 25, baseY + 40, 25, 12, bodyColor);
            DrawRect(tex, cx - bodyW / 2 - 25, baseY + 40, 12, 50, bodyColor);
        }
        else
        {
            DrawRect(tex, cx + bodyW / 2, baseY + 45, 22, 12, bodyColor);
            DrawRect(tex, cx + bodyW / 2 + 10, baseY + 45, 12, 45, bodyColor);
            DrawRect(tex, cx - bodyW / 2 - 18, baseY + 80, 18, 12, bodyColor);
            DrawRect(tex, cx - bodyW / 2 - 18, baseY + 80, 12, 30, bodyColor);
        }

        // Spikes
        for (int i = 0; i < 8 + variant * 2; i++)
        {
            int sx = cx + Random.Range(-bodyW / 2 + 2, bodyW / 2 - 2);
            int sy = baseY + Random.Range(5, bodyH - 5);
            DrawRect(tex, sx, sy, 2, 6, spikeColor);
        }

        // Flower on top
        DrawCircle(tex, cx, baseY + bodyH + 5, 8, flowerColor);

        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateVineFlower(int variant)
    {
        Texture2D tex = CreateBlankTexture();
        Color vineColor, flowerColor, leafColor;

        switch (variant)
        {
            case 0: // Green vine with purple flowers
                vineColor = new Color(0.15f, 0.5f, 0.1f);
                flowerColor = new Color(0.7f, 0.3f, 0.9f);
                leafColor = new Color(0.2f, 0.7f, 0.15f);
                break;
            case 1: // Dark vine with white flowers
                vineColor = new Color(0.1f, 0.35f, 0.1f);
                flowerColor = new Color(0.95f, 0.95f, 0.9f);
                leafColor = new Color(0.15f, 0.55f, 0.1f);
                break;
            default: // Brown vine with blue flowers
                vineColor = new Color(0.35f, 0.25f, 0.1f);
                flowerColor = new Color(0.3f, 0.5f, 1f);
                leafColor = new Color(0.25f, 0.6f, 0.2f);
                break;
        }

        int cx = TEX_SIZE / 2;

        // Curving vine (sine wave)
        int segments = 80 + variant * 20;
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            int x = cx + Mathf.RoundToInt(Mathf.Sin(t * Mathf.PI * (2 + variant)) * (30 + variant * 10));
            int y = 20 + Mathf.RoundToInt(t * 200);
            DrawCircle(tex, x, y, 3, vineColor);

            // Leaves along vine
            if (i % (15 - variant * 3) == 0)
            {
                int leafDir = (i % 2 == 0) ? 1 : -1;
                DrawPetal(tex, x, y, leafDir * 0.5f, 15, 8, leafColor);
            }
        }

        // Flowers at intervals
        int flowerCount = 2 + variant;
        for (int f = 0; f < flowerCount; f++)
        {
            float t = 0.3f + (0.5f / flowerCount) * f;
            int fx = cx + Mathf.RoundToInt(Mathf.Sin(t * Mathf.PI * (2 + variant)) * (30 + variant * 10));
            int fy = 20 + Mathf.RoundToInt(t * 200);
            // Flower petals
            for (int p = 0; p < 5; p++)
            {
                float angle = (360f / 5) * p * Mathf.Deg2Rad;
                DrawPetal(tex, fx, fy, angle, 12, 6, flowerColor);
            }
            DrawCircle(tex, fx, fy, 4, Color.Lerp(flowerColor, Color.yellow, 0.5f));
        }

        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateGrassSprout(int variant)
    {
        Texture2D tex = CreateBlankTexture();
        Color mainColor, tipColor, soilColor;

        switch (variant)
        {
            case 0: // Bright green sprouts
                mainColor = new Color(0.2f, 0.8f, 0.2f);
                tipColor = new Color(0.5f, 1f, 0.3f);
                soilColor = new Color(0.4f, 0.25f, 0.1f);
                break;
            case 1: // Yellow-green sprouts
                mainColor = new Color(0.4f, 0.7f, 0.1f);
                tipColor = new Color(0.7f, 0.9f, 0.2f);
                soilColor = new Color(0.35f, 0.2f, 0.1f);
                break;
            default: // Teal sprouts
                mainColor = new Color(0.1f, 0.6f, 0.5f);
                tipColor = new Color(0.3f, 0.9f, 0.7f);
                soilColor = new Color(0.3f, 0.2f, 0.1f);
                break;
        }

        int cx = TEX_SIZE / 2;

        // Soil mound
        DrawCircle(tex, cx, 30, 50, soilColor);
        DrawRect(tex, cx - 60, 10, 120, 25, soilColor);

        // Grass blades (different counts per variant)
        int bladeCount = 5 + variant * 3;
        for (int i = 0; i < bladeCount; i++)
        {
            float spread = ((float)i / bladeCount - 0.5f) * 2f;
            int baseX = cx + Mathf.RoundToInt(spread * 40);
            int height = 60 + Random.Range(0, 40 + variant * 20);
            float curve = spread * (0.3f + variant * 0.1f);

            // Draw each blade as a series of points curving outward
            for (int h = 0; h < height; h++)
            {
                float t = (float)h / height;
                int x = baseX + Mathf.RoundToInt(curve * h * 0.5f);
                int y = 35 + h;
                int width = Mathf.RoundToInt(Mathf.Lerp(4, 1, t));
                Color c = Color.Lerp(mainColor, tipColor, t);
                DrawRect(tex, x - width / 2, y, width, 2, c);
            }
        }

        tex.Apply();
        return tex;
    }

    // ========== WATER PLANTS ==========

    private static Texture2D GenerateWaterLily(int variant)
    {
        Texture2D tex = CreateBlankTexture();
        Color petalColor, padColor, waterColor;

        switch (variant)
        {
            case 0: // White lily on green pad
                petalColor = new Color(1f, 0.95f, 0.95f);
                padColor = new Color(0.15f, 0.6f, 0.2f);
                waterColor = new Color(0.2f, 0.4f, 0.8f);
                break;
            case 1: // Pink lily on dark pad
                petalColor = new Color(1f, 0.6f, 0.7f);
                padColor = new Color(0.1f, 0.4f, 0.15f);
                waterColor = new Color(0.15f, 0.3f, 0.7f);
                break;
            default: // Lavender lily on teal pad
                petalColor = new Color(0.7f, 0.6f, 1f);
                padColor = new Color(0.1f, 0.5f, 0.4f);
                waterColor = new Color(0.1f, 0.35f, 0.6f);
                break;
        }

        int cx = TEX_SIZE / 2, cy = TEX_SIZE / 2 - 10;

        // Water surface lines
        for (int w = 0; w < 3; w++)
        {
            int wy = cy - 25 + w * 20;
            for (int x = 20; x < TEX_SIZE - 20; x++)
            {
                int yOff = Mathf.RoundToInt(Mathf.Sin(x * 0.05f) * 3);
                SetPixelSafe(tex, x, wy + yOff, waterColor * 0.7f);
                SetPixelSafe(tex, x, wy + yOff + 1, waterColor * 0.7f);
            }
        }

        // Lily pad (flat oval)
        DrawEllipse(tex, cx, cy, 55 + variant * 5, 25, padColor);

        // Flower petals
        int petalCount = 6 + variant * 2;
        for (int i = 0; i < petalCount; i++)
        {
            float angle = (360f / petalCount) * i * Mathf.Deg2Rad;
            float petalLen = 22 + variant * 3;
            DrawPetal(tex, cx, cy + 15, angle, petalLen, 8 + variant, petalColor);
        }

        // Center
        DrawCircle(tex, cx, cy + 15, 6, new Color(1f, 0.9f, 0.3f));

        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateCoralBloom(int variant)
    {
        Texture2D tex = CreateBlankTexture();
        Color branchColor, tipColor, baseColor;

        switch (variant)
        {
            case 0: // Red coral
                branchColor = new Color(0.9f, 0.3f, 0.3f);
                tipColor = new Color(1f, 0.6f, 0.5f);
                baseColor = new Color(0.5f, 0.3f, 0.25f);
                break;
            case 1: // Blue coral
                branchColor = new Color(0.3f, 0.4f, 0.9f);
                tipColor = new Color(0.5f, 0.7f, 1f);
                baseColor = new Color(0.2f, 0.25f, 0.5f);
                break;
            default: // Purple coral
                branchColor = new Color(0.6f, 0.2f, 0.7f);
                tipColor = new Color(0.8f, 0.5f, 1f);
                baseColor = new Color(0.3f, 0.15f, 0.4f);
                break;
        }

        int cx = TEX_SIZE / 2;

        // Base rock
        DrawEllipse(tex, cx, 25, 40, 15, baseColor);

        // Coral branches (tree-like structure)
        DrawCoralBranch(tex, cx, 35, 90 + variant * 15, 0f, 3 + variant, branchColor, tipColor);

        tex.Apply();
        return tex;
    }

    private static void DrawCoralBranch(Texture2D tex, int x, int y, int length, float angle, int depth, Color branchColor, Color tipColor)
    {
        if (depth <= 0 || length < 5) return;

        int endX = x + Mathf.RoundToInt(Mathf.Sin(angle) * length * 0.3f);
        int endY = y + length;

        // Draw this branch segment
        float t = 1f - (float)depth / 5f;
        Color c = Color.Lerp(branchColor, tipColor, t);
        int width = Mathf.Max(2, depth * 2);

        for (int i = 0; i <= length; i++)
        {
            float frac = (float)i / length;
            int px = Mathf.RoundToInt(Mathf.Lerp(x, endX, frac));
            int py = Mathf.RoundToInt(Mathf.Lerp(y, endY, frac));
            int w = Mathf.RoundToInt(Mathf.Lerp(width, width / 2f, frac));
            DrawRect(tex, px - w / 2, py, w, 2, Color.Lerp(branchColor, tipColor, frac * t));
        }

        // Tip blob
        if (depth <= 1)
        {
            DrawCircle(tex, endX, endY, 5, tipColor);
        }

        // Branching
        DrawCoralBranch(tex, endX, endY, length / 2, angle - 0.4f, depth - 1, branchColor, tipColor);
        DrawCoralBranch(tex, endX, endY, length / 2, angle + 0.4f, depth - 1, branchColor, tipColor);
    }

    private static Texture2D GenerateBubbleFlower(int variant)
    {
        Texture2D tex = CreateBlankTexture();
        Color bubbleColor, stemColor, highlightColor;

        switch (variant)
        {
            case 0: // Blue bubbles
                bubbleColor = new Color(0.3f, 0.6f, 1f);
                stemColor = new Color(0.15f, 0.4f, 0.3f);
                highlightColor = new Color(0.7f, 0.9f, 1f);
                break;
            case 1: // Teal bubbles
                bubbleColor = new Color(0.2f, 0.8f, 0.7f);
                stemColor = new Color(0.1f, 0.35f, 0.25f);
                highlightColor = new Color(0.6f, 1f, 0.9f);
                break;
            default: // Purple bubbles
                bubbleColor = new Color(0.5f, 0.3f, 0.9f);
                stemColor = new Color(0.2f, 0.3f, 0.4f);
                highlightColor = new Color(0.7f, 0.6f, 1f);
                break;
        }

        int cx = TEX_SIZE / 2;

        // Stem
        DrawRect(tex, cx - 3, 15, 6, 100, stemColor);

        // Bubble clusters
        int bubbleCount = 5 + variant * 3;
        for (int i = 0; i < bubbleCount; i++)
        {
            int bx = cx + Random.Range(-40 - variant * 10, 40 + variant * 10);
            int by = 100 + Random.Range(-30, 80 + variant * 15);
            int radius = Random.Range(8, 18 + variant * 3);

            // Bubble outline
            DrawCircleOutline(tex, bx, by, radius, bubbleColor);
            // Inner fill (semi-transparent look)
            DrawCircle(tex, bx, by, radius - 2, Color.Lerp(bubbleColor, Color.clear, 0.3f));
            // Highlight
            DrawCircle(tex, bx - radius / 3, by + radius / 3, radius / 4, highlightColor);
        }

        tex.Apply();
        return tex;
    }

    // ========== DRAWING HELPERS ==========

    private static Texture2D CreateBlankTexture()
    {
        Texture2D tex = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Fill with transparent
        Color[] clear = new Color[TEX_SIZE * TEX_SIZE];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.clear;
        tex.SetPixels(clear);

        return tex;
    }

    private static void SetPixelSafe(Texture2D tex, int x, int y, Color color)
    {
        if (x >= 0 && x < TEX_SIZE && y >= 0 && y < TEX_SIZE)
        {
            // Alpha blend
            Color existing = tex.GetPixel(x, y);
            if (color.a > 0.01f)
            {
                Color blended = Color.Lerp(existing, color, color.a);
                blended.a = Mathf.Max(existing.a, color.a);
                tex.SetPixel(x, y, blended);
            }
        }
    }

    private static void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color color)
    {
        for (int x = cx - radius; x <= cx + radius; x++)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
                {
                    SetPixelSafe(tex, x, y, color);
                }
            }
        }
    }

    private static void DrawCircleOutline(Texture2D tex, int cx, int cy, int radius, Color color)
    {
        for (int x = cx - radius; x <= cx + radius; x++)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                int dist = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                if (dist <= radius * radius && dist >= (radius - 2) * (radius - 2))
                {
                    SetPixelSafe(tex, x, y, color);
                }
            }
        }
    }

    private static void DrawEllipse(Texture2D tex, int cx, int cy, int rx, int ry, Color color)
    {
        for (int x = cx - rx; x <= cx + rx; x++)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                float dx = (float)(x - cx) / rx;
                float dy = (float)(y - cy) / ry;
                if (dx * dx + dy * dy <= 1f)
                {
                    SetPixelSafe(tex, x, y, color);
                }
            }
        }
    }

    private static void DrawRect(Texture2D tex, int x, int y, int w, int h, Color color)
    {
        for (int px = x; px < x + w; px++)
        {
            for (int py = y; py < y + h; py++)
            {
                SetPixelSafe(tex, px, py, color);
            }
        }
    }

    private static void DrawRoundedRect(Texture2D tex, int x, int y, int w, int h, Color color)
    {
        int r = Mathf.Min(w, h) / 4;
        DrawRect(tex, x + r, y, w - 2 * r, h, color);
        DrawRect(tex, x, y + r, w, h - 2 * r, color);
        DrawCircle(tex, x + r, y + r, r, color);
        DrawCircle(tex, x + w - r, y + r, r, color);
        DrawCircle(tex, x + r, y + h - r, r, color);
        DrawCircle(tex, x + w - r, y + h - r, r, color);
    }

    private static void DrawPetal(Texture2D tex, int cx, int cy, float angle, float length, int width, Color color)
    {
        for (int i = 0; i < (int)length; i++)
        {
            float t = (float)i / length;
            int x = cx + Mathf.RoundToInt(Mathf.Cos(angle) * i);
            int y = cy + Mathf.RoundToInt(Mathf.Sin(angle) * i);
            int w = Mathf.RoundToInt(width * Mathf.Sin(t * Mathf.PI)); // Bulge in middle
            w = Mathf.Max(1, w);

            for (int dx = -w / 2; dx <= w / 2; dx++)
            {
                for (int dy = -w / 2; dy <= w / 2; dy++)
                {
                    if (dx * dx + dy * dy <= (w / 2) * (w / 2))
                    {
                        SetPixelSafe(tex, x + dx, y + dy, color);
                    }
                }
            }
        }
    }
}
