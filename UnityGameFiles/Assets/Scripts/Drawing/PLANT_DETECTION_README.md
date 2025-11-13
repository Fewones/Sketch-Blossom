# Plant Detection System

## Overview

The Plant Detection System analyzes player drawings to intuitively identify plant types based on visual characteristics. It detects three plant types, each mapped to an elemental type for combat.

## Detected Plant Types

### 🌻 Sunflower (Fire Type)
**Visual Characteristics:**
- **Round petals** radiating from center
- **Circular overall shape** (aspect ratio ~1.0)
- **Multiple strokes** forming petals (5+ strokes)
- **Radiating pattern** from center outward
- Curved, petal-like strokes

**Detection Logic:**
- Looks for circular strokes
- Measures radiating pattern strength
- Checks for near-square aspect ratio
- Awards points for curved strokes
- Penalizes vertical dominance

### 🌵 Cactus (Grass Type)
**Visual Characteristics:**
- **Tall, vertical shape** (aspect ratio > 1.5)
- **Narrow body** with vertical strokes
- **Spiky edges** (sharp directional changes)
- Simple structure (fewer strokes)
- Not circular

**Detection Logic:**
- Strong bonus for high aspect ratio (tall/narrow)
- Counts vertical strokes
- Detects spiky patterns (sharp angle changes)
- Rewards simplicity (fewer strokes)
- Penalizes circular and radiating patterns

### 🪷 Water Lily (Water Type)
**Visual Characteristics:**
- **Wide, flat shape** (aspect ratio < 0.8)
- **Horizontal orientation** (horizontal strokes)
- **Rounded, smooth leaves**
- Curved, flowing strokes
- May include circular lily pads
- No spikes

**Detection Logic:**
- Strong bonus for low aspect ratio (wide/flat)
- Counts horizontal strokes
- Rewards curved, smooth strokes
- Awards points for circular elements
- Penalizes spiky strokes and vertical dominance

## Technical Architecture

### Core Components

#### 1. **PlantAnalyzer.cs**
Main analysis engine that processes drawing strokes.

**Key Methods:**
- `AnalyzeDrawing(List<LineRenderer> strokes)` - Main entry point
- `ExtractFeatures(strokes)` - Extracts geometric features
- `CalculateSunflowerScore(features)` - Scoring algorithm
- `CalculateCactusScore(features)` - Scoring algorithm
- `CalculateWaterLilyScore(features)` - Scoring algorithm

**Feature Detection:**
- Aspect ratio (height/width)
- Stroke count and patterns
- Circular stroke detection
- Vertical/horizontal orientation
- Spiky vs curved strokes
- Radiating pattern strength

#### 2. **DrawnUnitData.cs** (Updated)
Stores detected plant type between scenes.

**New Fields:**
```csharp
public PlantAnalyzer.PlantType plantType;
public string elementType; // "Fire", "Grass", "Water"
public float detectionConfidence; // 0-1
```

#### 3. **DrawingManager.cs** (Updated)
Integrates plant detection into drawing workflow.

**Integration Points:**
- Automatically creates PlantAnalyzer if not found
- Calls analyzer after drawing completion
- Stores result in DrawnUnitData
- Shows optional UI feedback

#### 4. **PlantDetectionFeedback.cs** (Optional)
Visual UI feedback showing detection results.

**Features:**
- Displays plant name with emoji
- Shows element type with color coding
- Confidence level display
- Auto-hide after 3 seconds

## Usage

### In Unity Editor

1. **Drawing Scene Setup:**
   - DrawingManager will auto-create PlantAnalyzer
   - Optionally add PlantDetectionFeedback UI component
   - Set `showDetectionFeedback = true` in DrawingManager

2. **Battle Scene:**
   - BattleUnit automatically shows plant type in unit name
   - Example: "Sunflower (Fire)"

### Testing Different Plants

**To draw a Sunflower:**
- Draw 5-8 petal strokes radiating from center
- Make strokes curved
- Keep overall shape roughly circular
- Add a center circle (optional)

**To draw a Cactus:**
- Draw 2-3 vertical strokes
- Make the shape tall and narrow
- Add small spiky side strokes (thorns)
- Avoid circular patterns

**To draw a Water Lily:**
- Draw 3-5 wide, horizontal strokes
- Keep shape flat and wide
- Make strokes curved and smooth
- Can add circular lily pad shapes

## Scoring System

Each plant type receives a score from 0.0 to 1.0 based on feature matching. The plant type with the highest score is selected.

**Confidence Levels:**
- **0.8 - 1.0**: Excellent match
- **0.6 - 0.8**: Good match
- **0.4 - 0.6**: Fair match
- **0.0 - 0.4**: Uncertain/Poor match

## Debug Information

The analyzer logs detailed information:
```
=== PLANT ANALYSIS START ===
Analyzing 5 strokes...
Features: Width=3.45, Height=3.12, Aspect=0.90, Strokes=5
Patterns: Circular=1, Vertical=2, Horizontal=1
Patterns: Spiky=0, Curved=4, Radiating=0.80
=== ANALYSIS COMPLETE ===
Sunflower Score: 0.85
Cactus Score: 0.32
Water Lily Score: 0.45
Result: Sunflower (Fire) - Confidence: 85%
```

## Future Enhancements

### Planned Features:
1. **Machine Learning Integration**
   - Train on player drawings
   - Improve detection accuracy
   - Add more plant types

2. **Visual Feedback During Drawing**
   - Real-time preview of detected type
   - Suggestion hints for uncertain drawings

3. **Custom Plant Types**
   - Allow modders to define new plants
   - Scriptable Object-based plant definitions

4. **Stroke Quality Analysis**
   - Reward smooth, intentional strokes
   - Penalize random scribbles

## Tuning Detection

To adjust detection sensitivity, modify the scoring weights in PlantAnalyzer.cs:

**Example - Make Sunflower easier to detect:**
```csharp
// In CalculateSunflowerScore():
score += f.radiatingPattern * 0.5f; // Increased from 0.3f
```

**Example - Make Cactus require higher aspect ratio:**
```csharp
// In CalculateCactusScore():
if (f.aspectRatio > 2.0f) score += 0.35f; // Changed from 1.5f
```

## Troubleshooting

**Issue: All plants detected as same type**
- Check stroke count (need at least 2-3 strokes)
- Verify drawing area size is consistent
- Review debug logs for feature values

**Issue: Low confidence scores**
- Player may need clearer drawing instructions
- Adjust scoring weights
- Add visual reference images in UI

**Issue: Wrong plant type detected**
- Check if drawing matches expected characteristics
- Review feature extraction in debug logs
- Tune scoring algorithms

## Integration Checklist

- [x] PlantAnalyzer.cs created
- [x] DrawnUnitData.cs updated with plant type
- [x] DrawingManager.cs integrated with analyzer
- [x] BattleUnit.cs displays plant type
- [x] PlantDetectionFeedback.cs for UI feedback
- [ ] Add UI panel for detection feedback in DrawingScene
- [ ] Test all three plant types
- [ ] Tune detection sensitivity
- [ ] Add player tutorial/reference images
