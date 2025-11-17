# Plant Detection System - Technical Documentation

Complete guide to the drawing-based plant recognition system in Sketch Blossom.

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Detection Pipeline](#detection-pipeline)
3. [All 9 Plant Types Reference](#all-9-plant-types-reference)
4. [Drawing Requirements Guide](#drawing-requirements-guide)
5. [Validation System](#validation-system)
6. [Color Analysis](#color-analysis)
7. [Technical Implementation](#technical-implementation)
8. [Drawing Tips & Troubleshooting](#drawing-tips--troubleshooting)

---

## System Overview

### Core Concept

At game start, players draw their first plant using limited strokes. The system analyzes **color, shape features, and patterns** to determine the plant type. Unlike battle move detection, plant detection uses **strict validation** - invalid drawings are rejected and must be redrawn.

### Key Features

- **9 Unique Plant Types** across 3 elements (3 plants per element)
- **Color-Based Element Detection** - Red = Fire, Green = Grass, Blue = Water
- **Strict Pattern Matching** - Each plant has specific requirements
- **Validation System** - Drawings must meet thresholds or be rejected
- **Unique Stats per Plant** - Each plant has distinct HP/Attack/Defense values

### Plant Categories

| Element | Count | Plants | Base Stats Range |
|---------|-------|--------|------------------|
| **🔥 Fire** | 3 | Sunflower, Fire Rose, Flame Tulip | High Attack (16-20) |
| **🌿 Grass** | 3 | Cactus, Vine Flower, Grass Sprout | Balanced (10-14 ATK) |
| **💧 Water** | 3 | Water Lily, Coral Bloom, Bubble Flower | High HP (34-40) |

---

## Detection Pipeline

### Step-by-Step Process

```
Player Drawing
    ↓
[1] Color Analysis (Dominant color determines element)
    ↓
[2] Shape Feature Extraction
    ↓
[3] Pattern Matching (for plants of detected element)
    ↓
[4] Strict Validation (must meet thresholds)
    ↓
[5] Plant Created with Stats OR Rejection
```

### 1. Color Analysis

**Primary Step:** Determines element type before shape analysis.

| Dominant Color | Element | Next Step |
|----------------|---------|-----------|
| Red (R > G && R > B) | 🔥 Fire | Check Fire plant patterns |
| Green (G > R && G > B) | 🌿 Grass | Check Grass plant patterns |
| Blue (B > R && B > G) | 💧 Water | Check Water plant patterns |

**Critical:** If no dominant color is detected, the drawing is rejected.

### 2. Shape Feature Extraction

The system analyzes your drawing and extracts these features:

| Feature | Description | Used For |
|---------|-------------|----------|
| `strokeCount` | Total number of strokes drawn | Complexity detection |
| `width` | Horizontal bounding box size | Aspect ratio calculation |
| `height` | Vertical bounding box size | Aspect ratio calculation |
| `aspectRatio` | height / width | Shape orientation |
| `compactness` | Density of drawing | Pattern tightness |
| `curviness` | Smoothness of strokes | Petal vs spike detection |
| `circleCount` | Number of circular strokes | Sunflower petals, bubbles |
| `verticalLineCount` | Tall vertical strokes | Cactus, stems |
| `horizontalLineCount` | Wide horizontal strokes | Water lily leaves |
| `overlappingStrokes` | Strokes that cross | Rose petals, coral |
| `avgCurvature` | Average curve angle | Distinguishes smooth vs sharp |

### 3. Plant-Specific Pattern Matching

Each plant type has **strict requirements**. The system checks:
- Minimum/maximum stroke counts
- Required shape features (circles, lines, curves)
- Pattern-specific logic (overlaps, orientations)

### 4. Validation System

**Unlike battle moves, plant detection is STRICT:**

```
✓ VALID Drawing:
  → Meets all required thresholds
  → isValid = true
  → Plant created with stats

✗ INVALID Drawing:
  → Fails one or more requirements
  → isValid = false
  → Player must redraw
  → Feedback message explains why
```

**Confidence is binary:** Either the drawing is valid for a specific plant type, or it's not.

---

## All 9 Plant Types Reference

### 🔥 Fire Plants

#### Sunflower
| Property | Value |
|----------|-------|
| **Element** | Fire 🔥 |
| **Color Required** | Red (4+ red circles) + Green (1 green line for stem) |
| **Pattern** | 4+ circular strokes + 1 vertical green stem |
| **Key Features** | Multiple red circles (petals), green center stem |
| **Base Stats** | HP: 30, ATK: 18, DEF: 8 |
| **Moves** | Block, Fireball, Solar Flare |

**Drawing Requirements:**
```csharp
✓ Red circles >= 4
✓ Green lines >= 1 (vertical stem)
✓ isValid = true if both conditions met
```

---

#### Fire Rose
| Property | Value |
|----------|-------|
| **Element** | Fire 🔥 |
| **Color Required** | Red (5+ overlapping strokes) + Green (1 line) |
| **Pattern** | 5+ overlapping red strokes + green stem |
| **Key Features** | Overlapping petals creating layered effect |
| **Base Stats** | HP: 28, ATK: 20, DEF: 6 |
| **Moves** | Block, Ember Petals, Passion Burst |

**Drawing Requirements:**
```csharp
✓ Overlapping red strokes >= 5
✓ Green line >= 1
✓ isValid = true if both conditions met
```

---

#### Flame Tulip
| Property | Value |
|----------|-------|
| **Element** | Fire 🔥 |
| **Color Required** | Red (3+ long vertical strokes) |
| **Pattern** | 3+ tall vertical red strokes |
| **Key Features** | Elongated upward petals |
| **Base Stats** | HP: 32, ATK: 16, DEF: 10 |
| **Moves** | Block, Flame Strike, Inferno Wave |

**Drawing Requirements:**
```csharp
✓ Long vertical red strokes >= 3
✓ Aspect ratio > 1.2 (tall)
✓ isValid = true if both conditions met
```

---

### 🌿 Grass Plants

#### Cactus
| Property | Value |
|----------|-------|
| **Element** | Grass 🌿 |
| **Color Required** | Green (2+ long vertical lines) |
| **Pattern** | 2+ tall vertical green strokes |
| **Key Features** | Simple vertical structure |
| **Base Stats** | HP: 34, ATK: 12, DEF: 12 |
| **Moves** | Block, Needle Shot, Spine Storm |

**Drawing Requirements:**
```csharp
✓ Long vertical green strokes >= 2
✓ Aspect ratio > 1.5 (very tall)
✓ isValid = true if both conditions met
```

---

#### Vine Flower
| Property | Value |
|----------|-------|
| **Element** | Grass 🌿 |
| **Color Required** | Green (3+ curved strokes) |
| **Pattern** | 3+ curved flowing green strokes |
| **Key Features** | Smooth curved vines |
| **Base Stats** | HP: 32, ATK: 14, DEF: 10 |
| **Moves** | Block, Vine Lash, Strangling Roots |

**Drawing Requirements:**
```csharp
✓ Curved green strokes >= 3
✓ Average curvature > threshold
✓ isValid = true if both conditions met
```

---

#### Grass Sprout
| Property | Value |
|----------|-------|
| **Element** | Grass 🌿 |
| **Color Required** | Green (5+ short strokes) |
| **Pattern** | 5+ small scattered green strokes |
| **Key Features** | Many small grass blades |
| **Base Stats** | HP: 36, ATK: 10, DEF: 14 |
| **Moves** | Block, Razor Leaf, Growth Surge |

**Drawing Requirements:**
```csharp
✓ Green stroke count >= 5
✓ Strokes relatively short
✓ isValid = true if both conditions met
```

---

### 💧 Water Plants

#### Water Lily
| Property | Value |
|----------|-------|
| **Element** | Water 💧 |
| **Color Required** | Blue (3+ horizontal strokes) |
| **Pattern** | 3+ wide horizontal blue strokes |
| **Key Features** | Flat floating leaves |
| **Base Stats** | HP: 38, ATK: 10, DEF: 12 |
| **Moves** | Block, Lily Splash, Tranquil Petals (HEAL) |

**Drawing Requirements:**
```csharp
✓ Horizontal blue strokes >= 3
✓ Aspect ratio < 0.8 (wide)
✓ isValid = true if both conditions met
```

---

#### Coral Bloom
| Property | Value |
|----------|-------|
| **Element** | Water 💧 |
| **Color Required** | Blue (4+ overlapping strokes) |
| **Pattern** | 4+ overlapping blue strokes |
| **Key Features** | Layered coral structure |
| **Base Stats** | HP: 40, ATK: 8, DEF: 14 |
| **Moves** | Block, Coral Spike, Tidal Burst |

**Drawing Requirements:**
```csharp
✓ Overlapping blue strokes >= 4
✓ Overlap detected by system
✓ isValid = true if both conditions met
```

---

#### Bubble Flower
| Property | Value |
|----------|-------|
| **Element** | Water 💧 |
| **Color Required** | Blue (3+ circles) |
| **Pattern** | 3+ circular blue strokes |
| **Key Features** | Multiple bubble shapes |
| **Base Stats** | HP: 34, ATK: 12, DEF: 10 |
| **Moves** | Block, Bubble Barrage, Bubble Remedy (HEAL) |

**Drawing Requirements:**
```csharp
✓ Blue circles >= 3
✓ Circular detection threshold met
✓ isValid = true if both conditions met
```

---

## Drawing Requirements Guide

### How to Draw Each Plant

#### 🔥 Fire Plants

**Sunflower:**
```
1. Switch to RED color
2. Draw 4+ circles around a central point (petals)
3. Switch to GREEN color
4. Draw 1 vertical line down from center (stem)
```
**Tips:**
- Circles don't need to be perfect
- Spread circles around center
- Stem should be relatively vertical

---

**Fire Rose:**
```
1. Switch to RED color
2. Draw 5+ overlapping petal strokes
3. Layer them on top of each other
4. Switch to GREEN color
5. Add 1 stem line
```
**Tips:**
- Overlap is key - strokes must cross
- Think of layered rose petals
- More overlaps = more rose-like

---

**Flame Tulip:**
```
1. Switch to RED color
2. Draw 3+ long vertical strokes upward
3. Make them tall and elongated
```
**Tips:**
- Vertical orientation critical
- Strokes should be relatively long
- Think upward flame shape

---

#### 🌿 Grass Plants

**Cactus:**
```
1. Switch to GREEN color
2. Draw 2-3 long vertical lines
3. Make it tall and narrow
```
**Tips:**
- Very vertical (aspect ratio > 1.5)
- Simple is better
- Can add small side spikes (optional)

---

**Vine Flower:**
```
1. Switch to GREEN color
2. Draw 3+ curved, flowing strokes
3. Make them smooth and vine-like
```
**Tips:**
- Curves should be smooth, not jagged
- Think of hanging vines
- Can interweave them

---

**Grass Sprout:**
```
1. Switch to GREEN color
2. Draw 5+ short, scattered strokes
3. Spread them around
```
**Tips:**
- Many small strokes
- Scatter them (not all in one line)
- Think of grass blades

---

#### 💧 Water Plants

**Water Lily:**
```
1. Switch to BLUE color
2. Draw 3+ horizontal strokes
3. Make it wide and flat
```
**Tips:**
- Horizontal orientation critical (aspect ratio < 0.8)
- Wide, not tall
- Think floating leaves

---

**Coral Bloom:**
```
1. Switch to BLUE color
2. Draw 4+ overlapping strokes
3. Create a layered structure
```
**Tips:**
- Overlaps required (like Fire Rose)
- Can be more irregular than rose
- Think underwater coral branches

---

**Bubble Flower:**
```
1. Switch to BLUE color
2. Draw 3+ circles
3. Vary sizes slightly
```
**Tips:**
- Circles should be distinct (not overlapping like sunflower)
- Can be different sizes
- Think of soap bubbles

---

## Validation System

### How Validation Works

**Two-Stage Process:**

**Stage 1: Color Validation**
```
IF no dominant color detected:
  → REJECT: "Please use a clear color (Red, Green, or Blue)"
ELSE:
  → Proceed to Stage 2
```

**Stage 2: Pattern Validation**
```
FOR each plant of the detected element:
  Check if drawing meets pattern requirements

IF any plant pattern matches:
  → VALID: Create that plant
ELSE:
  → REJECT: "Drawing doesn't match any [Element] plant patterns"
```

### Validation Feedback

When a drawing is rejected, the system provides specific feedback:

| Rejection Reason | Message | Solution |
|------------------|---------|----------|
| No dominant color | "Please use Red, Green, or Blue" | Use stronger colors |
| Too few strokes | "Add more strokes to define the plant" | Draw more strokes |
| Wrong pattern for element | "Doesn't match Fire plant patterns" | Check requirements for Fire plants |
| Ambiguous shape | "Drawing unclear - try a simpler shape" | Follow specific plant guide |

### Debug Logging

The system logs detailed analysis (visible in Unity Console):

```
=== PLANT RECOGNITION START ===
Color Analysis: Red=0.85, Green=0.42, Blue=0.21
→ Element: FIRE

Shape Analysis:
- Strokes: 5
- Circles: 4 red, 1 green line detected
- Aspect Ratio: 1.1 (balanced)

Pattern Matching:
Sunflower:
  ✓ Red circles (4) >= 4 required
  ✓ Green line (1) >= 1 required
  ✓ isValid = TRUE
Fire Rose:
  ✗ Overlapping strokes (2) < 5 required
  ✗ isValid = FALSE
Flame Tulip:
  ✗ Long vertical strokes (1) < 3 required
  ✗ isValid = FALSE

=== RESULT: Sunflower (Fire) ===
```

---

## Color Analysis

### How Color Detection Works

**Process:**
1. Iterate through all drawn strokes
2. Sample color from each stroke's LineRenderer
3. Calculate dominant color across all strokes

**Dominance Calculation:**
```csharp
Color dominant = CalculateDominantColor(allStrokes);

if (dominant.r > dominant.g && dominant.r > dominant.b)
    → Element = FIRE
else if (dominant.g > dominant.r && dominant.g > dominant.b)
    → Element = GRASS
else if (dominant.b > dominant.r && dominant.b > dominant.g)
    → Element = WATER
else
    → REJECT (no clear dominant color)
```

### Color Threshold Requirements

**Minimum Color Difference:**
- Dominant channel must be **clearly** higher than other channels
- Close values (e.g., R=0.6, G=0.58) may be rejected
- Use **saturated colors** for best results

**Best Practices:**
- Use bright, saturated reds/greens/blues
- Avoid mixed colors (purple, orange, etc.)
- If using color picker, stick to primary colors

---

## Technical Implementation

### Key Classes

#### PlantRecognitionSystem.cs
**Purpose:** Main plant detection engine

**Location:** `UnityGameFiles/Assets/Scripts/Recognition/PlantRecognitionSystem.cs`

**Main Method:**
```csharp
PlantRecognitionResult RecognizePlant(List<LineRenderer> strokes)
```

**Process:**
1. `CalculateDominantColor()` - Determines element (lines 190-200)
2. `ExtractShapeFeatures()` - Analyzes drawing geometry (lines 233-488)
3. Pattern matching for element:
   - `DetectFirePlants()` - Lines 506-554
   - `DetectGrassPlants()` - Lines 556-604
   - `DetectWaterPlants()` - Lines 606-654
4. Validation check - `isValid` flag set per plant
5. Return result with plant type and stats

---

#### PlantRecognitionResult (Struct)
```csharp
public struct PlantRecognitionResult {
    public PlantType plantType;      // Enum: Sunflower, Cactus, etc.
    public ElementType elementType;  // Fire, Grass, Water
    public bool isValid;             // Passed validation?
    public string plantName;         // Display name
    public int baseHP;               // Starting HP
    public int baseAttack;           // Starting ATK
    public int baseDefense;          // Starting DEF
}
```

---

#### Feature Detection Methods

**Circle Detection** (lines 685-732):
```csharp
bool IsCircularStroke(LineRenderer stroke)
- Checks if stroke forms closed loop
- Calculates point-to-centroid variance
- Returns true if low variance (circular)
```

**Vertical/Horizontal Detection** (lines 359-366, 473-515):
```csharp
CountVerticalStrokes() / CountHorizontalStrokes()
- Measures aspect ratio of each stroke
- Classifies as vertical if height > width × 1.1
- Classifies as horizontal if width > height × 1.1
```

**Overlap Detection** (lines 369-379):
```csharp
CountOverlappingStrokes()
- Checks bounding box intersections
- Counts strokes that cross each other
- Used for roses and coral
```

---

### Plant Database

**Defined in PlantRecognitionSystem.cs (lines 90-143):**

| Plant | HP | ATK | DEF | Notes |
|-------|----|----|-----|-------|
| Sunflower | 30 | 18 | 8 | Balanced fire plant |
| Fire Rose | 28 | 20 | 6 | Glass cannon |
| Flame Tulip | 32 | 16 | 10 | Defensive fire |
| Cactus | 34 | 12 | 12 | Balanced grass |
| Vine Flower | 32 | 14 | 10 | Offensive grass |
| Grass Sprout | 36 | 10 | 14 | Defensive grass |
| Water Lily | 38 | 10 | 12 | Healer (Tranquil Petals) |
| Coral Bloom | 40 | 8 | 14 | Tank |
| Bubble Flower | 34 | 12 | 10 | Healer (Bubble Remedy) |

**Stat Patterns:**
- Fire: High ATK, lower HP/DEF
- Grass: Balanced across all stats
- Water: High HP, lower ATK, includes healers

---

### Integration Points

#### DrawingSceneManager.cs
**Calls Recognition System:**
```csharp
// After player finishes drawing
PlantRecognitionResult result = plantRecognizer.RecognizePlant(allStrokes);

if (result.isValid) {
    // Create plant, proceed to world map
    SavePlantToInventory(result);
    LoadScene("WorldMapScene");
} else {
    // Show error, allow redraw
    ShowFeedback("Drawing invalid. Please try again.");
    ClearCanvas();
}
```

---

#### PlayerInventory.cs
**Stores Plant Data:**
```csharp
public class DrawnPlantData {
    public PlantType plantType;
    public ElementType elementType;
    public int currentHP;
    public int maxHP;
    public int attack;
    public int defense;
    public Texture2D capturedDrawing; // Visual sprite
}
```

---

## Drawing Tips & Troubleshooting

### General Tips

**For All Plants:**
1. **Use strong colors** - Bright red, green, or blue
2. **Follow the pattern** - Each plant has specific requirements
3. **Check the guidebook** - In-game guide shows examples
4. **Practice the stroke count** - Too few or too many will fail
5. **Watch the feedback** - System tells you what's wrong

---

### Element-Specific Tips

**🔥 Fire Plants:**
- **Color switching is key** - Most need red AND green
- Sunflower: Focus on circular petals + vertical stem
- Fire Rose: Overlaps are critical - layer strokes
- Flame Tulip: Make it TALL - vertical orientation matters

**🌿 Grass Plants:**
- **All green, varied patterns**
- Cactus: Simple and vertical wins
- Vine Flower: Smooth curves, not jagged
- Grass Sprout: More strokes = better (need 5+)

**💧 Water Plants:**
- **Orientation matters most**
- Water Lily: WIDE not tall - horizontal strokes
- Coral Bloom: Overlaps like Fire Rose
- Bubble Flower: Distinct circles, not touching

---

### Common Issues & Solutions

#### "No plant detected from drawing"

**Possible Causes:**
1. **Color not dominant enough**
   - Solution: Use brighter, more saturated colors
   - Check: Are you using pure red/green/blue?

2. **Stroke count wrong**
   - Solution: Count strokes needed for each plant
   - Sunflower needs 5 (4 red circles + 1 green line)
   - Grass Sprout needs 5+ green strokes

3. **Pattern doesn't match any plant**
   - Solution: Open in-game guidebook
   - Follow exact pattern for desired plant

---

#### "Drawing keeps failing validation"

**Debugging Steps:**

1. **Check Unity Console** for detailed logs
   ```
   Look for:
   - "Red circles: X" (should be 4+ for Sunflower)
   - "Overlapping strokes: X" (should be 5+ for Fire Rose)
   - "isValid = FALSE" (tells you which check failed)
   ```

2. **Verify color dominance**
   ```
   Console should show:
   "Color Analysis: Red=0.XX, Green=0.XX, Blue=0.XX"
   → One value should be clearly highest
   ```

3. **Count your strokes**
   ```
   Console shows: "Analyzing X strokes..."
   → Match against plant requirements
   ```

---

#### "Wrong plant type detected"

**Why This Happens:**
- Your drawing matched a different plant's pattern
- Example: Drew Fire Rose but system saw Sunflower

**Solution:**
- Be more specific with required features
- Fire Rose: **Must have 5+ overlaps** (4 won't work)
- Sunflower: **Must have circles** (not just any strokes)

---

#### "I keep getting Cactus when I want other grass plants"

**Reason:**
- Cactus has simple requirements (2 vertical lines)
- Easy to accidentally match

**Solution:**
- Vine Flower: Add more **curves** (not straight lines)
- Grass Sprout: Draw **5+ short** strokes (not 2 long ones)

---

### Testing & Tuning

#### For Developers: Adjust Detection Sensitivity

**Make a plant easier to detect:**
```csharp
// In PlantRecognitionSystem.cs
// Example: Make Sunflower accept 3 circles instead of 4

if (redCircles >= 3 && greenLines >= 1) { // Changed from 4
    isValid = true;
    return new PlantRecognitionResult(...);
}
```

**Make a plant stricter:**
```csharp
// Example: Require 6 overlaps for Fire Rose

if (overlappingStrokes >= 6 && greenLines >= 1) { // Changed from 5
    isValid = true;
    return new PlantRecognitionResult(...);
}
```

**Add new plant type:**
1. Add to `PlantType` enum
2. Define stats in plant database (lines 90-143)
3. Add detection function (follow existing pattern)
4. Add to appropriate element's detection block

---

## Strategy Implications

### Plant Choice Matters

**High-Risk, High-Reward:**
- **Fire Rose** (20 ATK, 28 HP) - Hardest to draw, highest damage
- Best for: Skilled players, aggressive strategy

**Safe & Reliable:**
- **Grass Sprout** (10 ATK, 36 HP, 14 DEF) - Easy to draw, tanky
- Best for: Beginners, defensive strategy

**Specialized Roles:**
- **Water Lily** - Healing capability (Tranquil Petals)
- **Coral Bloom** - Ultimate tank (40 HP, 14 DEF)

### Drawing Skill vs Combat Skill

**Drawing Difficulty Ranking:**

**Easy to Draw:**
1. Cactus (2 vertical lines)
2. Grass Sprout (5 short strokes)
3. Water Lily (3 horizontal strokes)

**Medium Difficulty:**
4. Sunflower (circles + stem with color switch)
5. Flame Tulip (3 vertical strokes, tall)
6. Bubble Flower (3 circles)

**Hard to Draw:**
7. Vine Flower (curved strokes, requires smoothness)
8. Coral Bloom (overlaps, proper structure)
9. Fire Rose (5+ overlaps + color switch)

**Strategic Consideration:**
- Easier plants = more consistent in creating your team
- Harder plants = better stats/abilities if you can draw them

---

## Future Enhancements

### Planned Features

1. **Machine Learning Integration**
   - Train model on player drawings
   - Adaptive recognition (learns your style)
   - Reduce false rejections

2. **More Plant Varieties**
   - 3-6 additional plants per element
   - Hybrid/dual-type plants
   - Legendary ultra-rare plants

3. **Drawing Hints**
   - Ghost outlines showing correct pattern
   - Real-time feedback during drawing
   - Color indicator showing element being drawn

4. **Accessibility Features**
   - Colorblind mode (patterns + labels)
   - Simplified validation for accessibility
   - Template tracing option

### Potential Improvements

- **Stroke pressure analysis** (for tablets/styluses)
- **Drawing speed/smoothness** bonus
- **Style variants** (loose vs precise) for same plant
- **Player-submitted templates** for community recognition

---

## Appendix: Quick Reference

### Plant Pattern Cheat Sheet

| Plant | Color(s) | Strokes | Key Pattern | Difficulty |
|-------|----------|---------|-------------|------------|
| **Sunflower** | Red + Green | 5 | 4 red circles + 1 green stem | ⭐⭐ |
| **Fire Rose** | Red + Green | 6+ | 5+ overlapping red + green | ⭐⭐⭐ |
| **Flame Tulip** | Red | 3+ | 3+ tall vertical red | ⭐⭐ |
| **Cactus** | Green | 2+ | 2+ vertical green lines | ⭐ |
| **Vine Flower** | Green | 3+ | 3+ curved green vines | ⭐⭐⭐ |
| **Grass Sprout** | Green | 5+ | 5+ short scattered green | ⭐ |
| **Water Lily** | Blue | 3+ | 3+ horizontal blue | ⭐ |
| **Coral Bloom** | Blue | 4+ | 4+ overlapping blue | ⭐⭐⭐ |
| **Bubble Flower** | Blue | 3+ | 3+ blue circles | ⭐⭐ |

### Color Requirements Summary

| Element | Primary Color | Secondary Color | When Used |
|---------|---------------|-----------------|-----------|
| Fire | Red | Green (optional) | Most fire plants need both |
| Grass | Green | None | Pure green drawings |
| Water | Blue | None | Pure blue drawings |

### Stat Comparison

**Highest HP:** Coral Bloom (40)
**Highest ATK:** Fire Rose (20)
**Highest DEF:** Grass Sprout, Coral Bloom (14)
**Most Balanced:** Cactus (34/12/12)
**Best Healers:** Water Lily, Bubble Flower (healing moves)

---

**Last Updated:** 17/11/2025
**Version:** 2.0 - Complete 9-Plant System
**Previous Version:** 1.0 - Original 3-Plant System (Deprecated)
