using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SketchBlossom.Model;

/// <summary>
/// Complete plant recognition system
/// Analyzes color (element) and shape (plant type) from drawings
/// </summary>
public class PlantRecognitionSystem : MonoBehaviour
{
    public enum ElementType
    {
        Fire,    // Red
        Grass,   // Green
        Water    // Blue
    }

    public enum PlantType
    {
        // Fire Plants (Red)
        Sunflower,
        FireRose,
        FlameTulip,

        // Grass Plants (Green)
        Cactus,
        VineFlower,
        GrassSprout,

        // Water Plants (Blue)
        WaterLily,
        CoralBloom,
        BubbleFlower
    }

    [System.Serializable]
    public class PlantData
    {
        public PlantType type;
        public ElementType element;
        public string displayName;
        public int baseHP;
        public int baseAttack;
        public int baseDefense;
        public string description;

        public PlantData(PlantType type, ElementType element, string name, int hp, int atk, int def, string desc)
        {
            this.type = type;
            this.element = element;
            this.displayName = name;
            this.baseHP = hp;
            this.baseAttack = atk;
            this.baseDefense = def;
            this.description = desc;
        }
    }

    [System.Serializable]
    public class RecognitionResult
    {
        public PlantType plantType;
        public ElementType element;
        public PlantData plantData;
        public float confidence;
        public Color dominantColor;
        public bool isValidPlant;  // True if drawing meets threshold criteria

        public RecognitionResult(PlantType type, ElementType elem, PlantData data, float conf, Color color, bool isValid = true)
        {
            plantType = type;
            element = elem;
            plantData = data;
            confidence = conf;
            dominantColor = color;
            isValidPlant = isValid;
        }
    }

    // Plant database with fixed stats
    private static Dictionary<PlantType, PlantData> plantDatabase;
    private static Dictionary<string, PlantType> stringToPlant;

    private void Awake()
    {
        InitializePlantDatabase();
        InitializeStringToPlant();
    }

    private void InitializePlantDatabase()
    {
        plantDatabase = new Dictionary<PlantType, PlantData>
        {
            // FIRE PLANTS (Red) - High attack, medium HP, low defense
            { PlantType.Sunflower, new PlantData(
                PlantType.Sunflower, ElementType.Fire, "Sunflower",
                hp: 30, atk: 18, def: 8,
                "Radiant fire flower with circular petals"
            )},
            { PlantType.FireRose, new PlantData(
                PlantType.FireRose, ElementType.Fire, "Fire Rose",
                hp: 35, atk: 16, def: 10,
                "Compact blazing rose with many layers"
            )},
            { PlantType.FlameTulip, new PlantData(
                PlantType.FlameTulip, ElementType.Fire, "Flame Tulip",
                hp: 28, atk: 20, def: 6,
                "Tall elegant flame flower with simple shape"
            )},

            // GRASS PLANTS (Green) - Balanced stats
            { PlantType.Cactus, new PlantData(
                PlantType.Cactus, ElementType.Grass, "Cactus",
                hp: 32, atk: 12, def: 14,
                "Spiky vertical desert guardian"
            )},
            { PlantType.VineFlower, new PlantData(
                PlantType.VineFlower, ElementType.Grass, "Vine Flower",
                hp: 35, atk: 14, def: 12,
                "Flowing curved vine with blooms"
            )},
            { PlantType.GrassSprout, new PlantData(
                PlantType.GrassSprout, ElementType.Grass, "Grass Sprout",
                hp: 30, atk: 10, def: 16,
                "Bushy low-growing grass cluster"
            )},

            // WATER PLANTS (Blue) - High HP, low attack, high defense
            { PlantType.WaterLily, new PlantData(
                PlantType.WaterLily, ElementType.Water, "Water Lily",
                hp: 40, atk: 10, def: 14,
                "Peaceful floating flower spreading wide"
            )},
            { PlantType.CoralBloom, new PlantData(
                PlantType.CoralBloom, ElementType.Water, "Coral Bloom",
                hp: 38, atk: 12, def: 12,
                "Branching underwater coral flower"
            )},
            { PlantType.BubbleFlower, new PlantData(
                PlantType.BubbleFlower, ElementType.Water, "Bubble Flower",
                hp: 36, atk: 8, def: 16,
                "Clustered bubble-like blooms"
            )}
        };
    }

    private void InitializeStringToPlant()
    {
        stringToPlant = new Dictionary<string, PlantType>
            {
                // FIRE PLANTS (Red) - High attack, medium HP, low defense
                { "sunflower", PlantType.Sunflower},
                { "fire rose", PlantType.FireRose},
                { "flame tulip", PlantType.FlameTulip},

                // GRASS PLANTS (Green) - Balanced stats
                { "cactus", PlantType.Cactus},
                { "vine flower", PlantType.VineFlower},
                { "grass sprout", PlantType.GrassSprout},

                // WATER PLANTS (Blue) - High HP, low attack, high defense
                { "water lily", PlantType.WaterLily},
                { "coral bloom", PlantType.CoralBloom},
                { "bubble flower", PlantType.BubbleFlower}
        };
    }
   

    /// <summary>
    /// Analyze drawn strokes and recognize the plant
    /// </summary>
    public RecognitionResult AnalyzeDrawing(string label, float score, List<LineRenderer> strokes, Color dominantColor)
    {
        if (strokes == null || strokes.Count == 0)
        {
            Debug.LogWarning("No strokes to analyze!");
            return CreateDefaultResult(dominantColor);
        }

        Debug.Log("=== PLANT RECOGNITION START ===");

        // Determine if plant is valid and plantType
        bool isValidPlant = score > 0.2;
        PlantType plantType = stringToPlant[label];
        Debug.Log($"Step 1: Plant type detected = {label}");
        
        // Calculate confidence
        float confidence = CalculateConfidence(strokes.Count, score);
        Debug.Log($"Step 2: Confidence = {confidence:P0}");
        
        // Get plant data and create result
        PlantData data = plantDatabase[plantType];
        RecognitionResult result = new RecognitionResult(plantType, data.element, data, confidence, dominantColor, isValidPlant);
        

        Debug.Log("=== PLANT RECOGNITION COMPLETE ===");
        if (!isValidPlant)
        {
            Debug.LogWarning("⚠️  DRAWING DOES NOT MEET REQUIREMENTS - NOT A VALID PLANT!");
            Debug.LogWarning("   Please try again with the correct pattern.");
        }
        LogResult(result);

        return result;
    }

    private ElementType DetermineElementFromColor(Color color)
    {
        if (color.r > color.g && color.r > color.b)
            return ElementType.Fire;
        else if (color.g > color.r && color.g > color.b)
            return ElementType.Grass;
        else if (color.b > color.r && color.b > color.g)
            return ElementType.Water;
        else
            return ElementType.Grass; // Default
    }

    private float CalculateConfidence(int strokes, float accuracy)
    {
        // Base confidence on stroke count and features
        float confidence = 0.5f;

        if (strokes >= 3)
            confidence += 0.2f;
        if (strokes >= 5)
            confidence += 0.15f;
        if (accuracy > 0.24f)
            confidence += Mathf.RoundToInt(200*(accuracy - 0.24f))/10;

        return Mathf.Clamp01(confidence);
    }

    private RecognitionResult CreateDefaultResult(Color color)
    {
        ElementType element = DetermineElementFromColor(color);
        PlantType defaultType = element == ElementType.Fire ? PlantType.Sunflower :
                                element == ElementType.Grass ? PlantType.Cactus :
                                PlantType.WaterLily;

        PlantData data = plantDatabase[defaultType];
        return new RecognitionResult(defaultType, element, data, 0.3f, color);
    }

    private void LogResult(RecognitionResult result)
    {
        Debug.Log($"📋 FINAL RESULT:");
        Debug.Log($"   ✅ Valid Plant: {result.isValidPlant}");
        Debug.Log($"   🌱 Plant: {result.plantData.displayName} ({result.plantType})");
        Debug.Log($"   🔥 Element: {result.element}");
        Debug.Log($"   ⭐ Confidence: {result.confidence:P0}");
        Debug.Log($"   💚 Stats: HP={result.plantData.baseHP}, ATK={result.plantData.baseAttack}, DEF={result.plantData.baseDefense}");
    }

    public static PlantData GetPlantData(PlantType type)
    {
        if (plantDatabase == null)
        {
            // Initialize if called statically before Awake
            var temp = new PlantRecognitionSystem();
            temp.InitializePlantDatabase();
        }
        return plantDatabase[type];
    }
}
