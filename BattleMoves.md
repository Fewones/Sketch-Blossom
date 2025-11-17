# Battle Moves System - Technical Documentation

Complete guide to the drawing-based battle move detection system in Sketch Blossom.

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Move Detection Pipeline](#move-detection-pipeline)
3. [All 27 Moves Reference](#all-27-moves-reference)
4. [Drawing Patterns Guide](#drawing-patterns-guide)
5. [Quality Scoring System](#quality-scoring-system)
6. [Type Advantage System](#type-advantage-system)
7. [Technical Implementation](#technical-implementation)

---

## System Overview

### Core Concept

Players draw moves during battle using the same drawing system as plant creation. The system analyzes the drawing's **shape, stroke count, and patterns** to determine which move was intended.

### Key Features

- **27 Unique Moves** across 9 plant types (3 moves per plant)
- **Quality-based Damage** - Better drawings deal more damage (0.5x - 1.5x multiplier)
- **Pattern Recognition** - Each move has distinct shape requirements
- **Type Advantages** - Water > Fire > Grass > Water (1.5x / 0.5x)
- **Forgiving Detection** - Allows for artistic interpretation while maintaining clear distinctions

### Move Categories

| Category | Count | Examples |
|----------|-------|----------|
| **Universal** | 1 | Block (all plants have it) |
| **Fire Moves** | 3 | Fireball, FlameWave, Burn |
| **Grass Moves** | 3 | VineWhip, LeafStorm, RootAttack |
| **Water Moves** | 3 | WaterSplash, Bubble, HealingWave |

---

## Move Detection Pipeline

### Step-by-Step Process

```
Player Drawing
    ↓
[1] Extract Features
    ↓
[2] Calculate Move Scores (for each available move)
    ↓
[3] Find Best Match
    ↓
[4] Confidence Check (threshold: 0.5)
    ↓
[5] Quality Analysis (if recognized)
    ↓
[6] Execute Move with Multiplier
```

### 1. Feature Extraction

The system analyzes your drawing and extracts these features:

| Feature | Description | Example |
|---------|-------------|---------|
| `strokeCount` | Total number of strokes drawn | 1 stroke = simple, 5+ = complex |
| `width` | Horizontal size of bounding box | Wide = horizontal moves |
| `height` | Vertical size of bounding box | Tall = vertical moves |
| `aspectRatio` | height / width | >1.2 = tall, <0.7 = wide |
| `circularStrokes` | Number of circular/closed shapes | Circles, ovals |
| `verticalStrokes` | Strokes that go up/down | Tall lines |
| `horizontalStrokes` | Strokes that go left/right | Wide lines |
| `spikyStrokes` | Strokes with sharp turns | Zigzags, lightning |
| `curvedStrokes` | Smooth flowing strokes | Waves, curves |

### 2. Move Scoring

Each available move receives a score (0.0 - 1.0) based on how well your drawing matches that move's pattern.

**Example: Fireball Detection**
```csharp
Score Calculation:
+ 0.5 if 1+ circular strokes detected
+ 0.3 if 1-2 total strokes
+ 0.2 if compact size (< 3 units)
────────────────────────
= 1.0 (perfect match!)
```

### 3. Confidence Threshold

- **Threshold: 0.5** (50% confidence)
- If no move scores above 0.5, the drawing is **not recognized**
- The highest-scoring move is selected

### 4. Quality Scoring

Once a move is recognized, its **drawing quality** is analyzed:

| Quality Rating | Multiplier | Description |
|----------------|------------|-------------|
| Perfect! | 1.5x | Flawless execution |
| Excellent! | 1.3x | Great drawing |
| Good | 1.1x | Solid attempt |
| Decent | 1.0x | Acceptable |
| Poor | 0.8x | Weak drawing |
| Very Poor | 0.5x | Barely recognized |

**Quality affects damage/healing:** A perfectly drawn Fireball deals 50% more damage!

---

## All 27 Moves Reference

### 🔥 Fire Plants

#### Sunflower Moves
| Move | Type | Power | Pattern | Colors | Effects |
|------|------|-------|---------|--------|---------|
| **Block** | Defense | 0 | 1-3 circles | Gold / Orange | Crystals |
| **Fireball** | Attack | 20 | Perfect circle | Bright Orange / Yellow | Flames |
| **Solar Flare** | Attack | 28 | Sharp zigzags | Deep Orange-Red / Bright Yellow | Lightning ⚡ |

#### Fire Rose Moves
| Move | Type | Power | Pattern | Colors | Effects |
|------|------|-------|---------|--------|---------|
| **Block** | Defense | 0 | 1-3 circles | Deep Red / Orange-Red | Petals |
| **Ember Petals** | Attack | 22 | Scattered jagged lines | Crimson / Orange | Petals 🌸 |
| **Passion Burst** | Attack | 26 | Large circle with flair | Hot Pink-Red / Red-Orange | Flames |

#### Flame Tulip Moves
| Move | Type | Power | Pattern | Colors | Effects |
|------|------|-------|---------|--------|---------|
| **Block** | Defense | 0 | 1-3 circles | Rose / Coral | Petals |
| **Flame Strike** | Attack | 24 | Clean circle | Pure Flame Orange / Light Orange | Flames |
| **Inferno Wave** | Attack | 30 | Aggressive zigzags | Deep Flame / Bright Fire | Smoke 💨 |

---

### 🌿 Grass Plants

#### Cactus Moves
| Move | Type | Power | Pattern | Colors | Effects |
|------|------|-------|---------|--------|---------|
| **Block** | Defense | 0 | 1-3 circles | Desert Green / Sandy Brown | Crystals |
| **Needle Shot** | Attack | 20 | Single curved line | Bright Green / Tan | Crystals ✨ |
| **Spine Storm** | Attack | 26 | 5+ scattered strokes | Dark Green / Pale Yellow | Crystals |

#### Vine Flower Moves
| Move | Type | Power | Pattern | Colors | Effects |
|------|------|-------|---------|--------|---------|
| **Block** | Defense | 0 | 1-3 circles | Vibrant Green / Olive | Vines |
| **Vine Lash** | Attack | 22 | Long curved line | Fresh Green / Dark Green | Vines 🌿 |
| **Strangling Roots** | Attack | 26 | Vertical downward strokes | Forest Green / Brown | Roots |

#### Grass Sprout Moves
| Move | Type | Power | Pattern | Colors | Effects |
|------|------|-------|---------|--------|---------|
| **Block** | Defense | 0 | 1-3 circles | Light Green / Yellow-Green | Leaves |
| **Razor Leaf** | Attack | 20 | 5+ quick strokes | Bright Grass / Medium Green | Leaves 🍃 |
| **Growth Surge** | Attack | 24 | Tall vertical lines | Grass Green / Earth Brown | Roots |

---

### 💧 Water Plants

#### Water Lily Moves
| Move | Type | Power | Pattern | Colors | Effects |
|------|------|-------|---------|--------|---------|
| **Block** | Defense | 0 | 1-3 circles | Sky Blue / Aqua | Water |
| **Lily Splash** | Attack | 20 | Smooth wavy curves | Clear Blue / Light Cyan | Water 💧 |
| **Tranquil Petals** | Healing | 25 | Gentle horizontal waves | Pale Cyan / Mint | Petals (heals!) |

#### Coral Bloom Moves
| Move | Type | Power | Pattern | Colors | Effects |
|------|------|-------|---------|--------|---------|
| **Block** | Defense | 0 | 1-3 circles | Coral Pink / Ocean Blue | Crystals |
| **Coral Spike** | Attack | 22 | Curved flowing lines | Pink Coral / Deep Blue | Crystals ✨ |
| **Tidal Burst** | Attack | 26 | Multiple circles | Vivid Blue / White Foam | Bubbles |

#### Bubble Flower Moves
| Move | Type | Power | Pattern | Colors | Effects |
|------|------|-------|---------|--------|---------|
| **Block** | Defense | 0 | 1-3 circles | Light Blue / Almost White | Bubbles |
| **Bubble Barrage** | Attack | 24 | Many small circles | Medium Blue / Pale Blue | Bubbles 💙 |
| **Bubble Remedy** | Healing | 22 | Smooth flowing waves | Turquoise / Mint Green | Bubbles (heals!) |

---

## Drawing Patterns Guide

### 🛡️ Block (Universal)

**How to Draw:** 1-3 simple circular strokes

**Detection Logic:**
- ✅ Prefers 1-3 strokes
- ✅ Bonus for circular shapes
- ✅ Compact size is good
- ⚠️ Fallback move if nothing else matches

**Tips:**
- Simplest move to execute
- Quick defense in emergencies
- Quality doesn't matter as much (defensive reduction based on quality)

---

### 🔥 Fire Move Patterns

#### Fireball
**Pattern:** Single perfect circle

**Detection Logic:**
```
Score Calculation:
+ 0.5 if 1+ circular strokes
+ 0.3 if 1-2 total strokes
+ 0.2 if compact (< 3 units)
───────────────────────────
Best for: Round, compact shapes
```

**Tips:**
- Draw a clean circle
- Don't add extra strokes
- Size doesn't matter much

#### Burn / Solar Flare / Inferno Wave
**Pattern:** Zigzag or spiky lines

**Detection Logic:**
```
Score Calculation:
+ 0.5 if spiky/sharp turns
+ 0.3 if vertical orientation
+ 0.2 if multiple spikes
- 0.7 penalty if circular
───────────────────────────
Best for: Lightning-like zigzags
```

**Tips:**
- Draw sharp angles (like lightning ⚡)
- Vertical or diagonal works
- More spikes = better

---

### 🌿 Grass Move Patterns

#### VineWhip / Vine Lash
**Pattern:** Single curved line

**Detection Logic:**
```
Score Calculation:
+ 0.4 if curved strokes
+ 0.3 if 1-2 strokes
+ 0.2 if elongated (not compact)
- 0.5 penalty if circular
───────────────────────────
Best for: Long whip-like curves
```

**Tips:**
- One smooth curved stroke
- Don't close the loop
- Longer is better

#### LeafStorm / Razor Leaf
**Pattern:** 5+ scattered short strokes

**Detection Logic:**
```
Score Calculation:
+ 0.4 if 5+ strokes
+ 0.3 if many scattered strokes
+ 0.2 if mixed directions
- 0.4 penalty if circular
───────────────────────────
Best for: Multiple quick strokes everywhere
```

**Tips:**
- Draw many small strokes
- Scatter them around
- Mix up directions

#### RootAttack / Growth Surge
**Pattern:** Vertical downward lines

**Detection Logic:**
```
Score Calculation:
+ 0.4 if vertical strokes
+ 0.3 if tall aspect ratio (>1.2)
+ 0.2 if multiple vertical strokes
- 0.5 penalty if horizontal dominant
───────────────────────────
Best for: Tall, vertical lines going down
```

**Tips:**
- Draw top-to-bottom
- Multiple vertical strokes work
- Avoid horizontal dominance

---

### 💧 Water Move Patterns

#### WaterSplash / Lily Splash
**Pattern:** Upward wavy lines

**Detection Logic:**
```
Score Calculation:
+ 0.4 if curved/wavy strokes
+ 0.3 if vertical or mixed
+ 0.2 if 2-5 strokes
───────────────────────────
Best for: Wavy upward splashes
```

**Tips:**
- Draw flowing curves
- Can be vertical or mixed
- Multiple waves good

#### Bubble / Bubble Barrage
**Pattern:** Multiple small circles

**Detection Logic:**
```
Score Calculation:
+ 0.5 if circular strokes
+ 0.3 if multiple circles (2+)
+ 0.2 if compact/small
───────────────────────────
Best for: Several small circles
```

**Tips:**
- Draw multiple circles
- Keep them relatively small
- More circles = better recognition

#### HealingWave / Tranquil Petals / Bubble Remedy
**Pattern:** Smooth horizontal wave

**Detection Logic:**
```
Score Calculation:
+ 0.3 if horizontal (width > height)
+ 0.3 if horizontal strokes
+ 0.3 if curved/smooth
- 0.5 penalty if spiky
───────────────────────────
Best for: Gentle horizontal waves
```

**Tips:**
- Draw side-to-side
- Keep it smooth (not jagged)
- One or two waves work

---

## Quality Scoring System

### How Quality is Calculated

Each move type has a **specific quality function** that analyzes shape characteristics.

#### Shape Analysis Features

| Feature | Description | Good For |
|---------|-------------|----------|
| **Compactness** | How tight/dense the shape is | Fireballs, Bubbles |
| **Curviness** | Smoothness of strokes | Waves, Vines |
| **Radialness** | How much it spreads from center | Sunflower attacks |
| **Branchiness** | Multiple directions from points | Leaf Storm |

### Quality Examples

#### Perfect Fireball (1.5x damage)
- ✅ Perfectly round circle
- ✅ Single stroke
- ✅ Smooth curve
- ✅ Compact size

#### Poor Fireball (0.5x damage)
- ❌ Wobbly shape
- ❌ Multiple overlapping strokes
- ❌ Not circular
- ❌ Too large/scattered

### Quality Tips for Each Move Type

**Circular Moves (Fireball, Bubble):**
- Draw one smooth circle
- Close the shape completely
- Don't add extra strokes
- Medium size is best

**Wavy Moves (WaterSplash, HealingWave):**
- Smooth, flowing curves
- Consistent wave amplitude
- Avoid sharp corners
- 1-2 wave cycles ideal

**Spiky Moves (Burn, Lightning attacks):**
- Sharp, defined angles
- Consistent spike spacing
- Clear zigzag pattern
- Don't round the corners

**Multi-Stroke Moves (LeafStorm):**
- Many quick, decisive strokes
- Relatively uniform size
- Good spacing
- Mixed directions

---

## Type Advantage System

### Type Triangle

```
     💧 WATER
      /    \
    1.5x   0.5x
    /        \
🔥 FIRE ─── 🌿 GRASS
     0.5x  1.5x
```

### Damage Multipliers

| Attacker → Defender | Multiplier | Text |
|---------------------|------------|------|
| Water → Fire | **1.5x** | "It's super effective!" 💚 |
| Fire → Grass | **1.5x** | "It's super effective!" 💚 |
| Grass → Water | **1.5x** | "It's super effective!" 💚 |
| Fire → Water | **0.5x** | "It's not very effective..." ❤️ |
| Grass → Fire | **0.5x** | "It's not very effective..." ❤️ |
| Water → Grass | **0.5x** | "It's not very effective..." ❤️ |
| Same Type | 1.0x | *(no message)* |

### Damage Formula

```
Final Damage = (Move Power + Attacker ATK) × Type Advantage × Quality Multiplier
```

**Example:**
```
Fireball (20 power) from Sunflower (18 ATK)
Against Water Lily (Water type)
Drawn with "Excellent" quality (1.3x)

= (20 + 18) × 0.5 × 1.3
= 38 × 0.5 × 1.3
= 24.7 damage (rounded to 25)
```

---

## Technical Implementation

### Key Classes

#### MoveData.cs
**Purpose:** Define all move properties

**Key Properties:**
- `moveType` - Enum identifying the move
- `moveName` - Display name
- `basePower` - Base damage/healing
- `primaryColor` / `secondaryColor` - Visual colors
- `visualEffect` - Effect type (Flames, Bubbles, etc.)
- `animationIntensity` - Speed/scale of animation (0.5-2.0)
- `screenShakeAmount` - Camera shake intensity (0-1.0)
- `drawingHint` - Text hint for player

**Static Method:**
```csharp
MoveData[] GetMovesForPlant(PlantType plantType)
// Returns all 3 moves available to a specific plant
```

---

#### MovesetDetector.cs
**Purpose:** Analyze drawings and detect moves

**Main Method:**
```csharp
MoveDetectionResult DetectMove(List<LineRenderer> strokes, PlantType plantType)
```

**Process:**
1. Extract features from drawing
2. Score each available move
3. Find best match
4. Apply confidence threshold
5. Calculate quality if recognized

**Detection Functions:**
- `CalculateBlockScore()`
- `CalculateFireballScore()`
- `CalculateBurnScore()`
- `CalculateVineWhipScore()`
- `CalculateLeafStormScore()`
- *(and more...)*

---

#### MoveRecognitionSystem.cs
**Purpose:** Quality scoring for recognized moves

**Main Method:**
```csharp
QualityResult AnalyzeMove(List<LineRenderer> strokes, MoveType moveType)
```

**Returns:**
- `quality` - Raw quality score (0-1)
- `damageMultiplier` - Final multiplier (0.5-1.5)
- `qualityRating` - Text rating ("Perfect!", "Good", etc.)

**Quality Functions:**
- `CalculateFireballQuality()`
- `CalculateBurnQuality()`
- `CalculateWaveQuality()`
- *(move-specific quality analyzers)*

---

#### MoveExecutor.cs
**Purpose:** Execute moves with animations and effects

**Main Method:**
```csharp
IEnumerator ExecuteMove(MoveData move, BattleUnit attacker, BattleUnit target, ...)
```

**Features:**
- Unique move colors (not generic element colors)
- Gradient flash effects (primary → secondary)
- Screen shake based on move power
- Animation speed scaling
- Type advantage calculation
- Healing/defensive move handling

---

### Configuration Constants

**In MovesetDetector:**
- `confidenceThreshold = 0.5` - Minimum score to recognize a move

**In MoveRecognitionSystem:**
- `forgivenessFactor = 1.1` - Makes quality scoring more lenient (was 1.3, adjusted for balance)

**In MoveExecutor:**
- `screenShakeMultiplier = 1.0` - Global screen shake intensity

---

## Strategy Tips

### For Players

**Maximize Damage:**
1. Learn the exact pattern for your plant's moves
2. Practice drawing them smoothly
3. Use type advantage (1.5x is huge!)
4. Combine quality (1.5x) + type advantage (1.5x) = 2.25x total!

**Defense Strategy:**
- Block is always available (easy to draw)
- Quality affects damage reduction (30%-70%)
- Perfect block = 70% damage reduction!

**Healing Moves:**
- Water Lily and Bubble Flower have healing
- Quality affects healing amount
- Heal when HP is below 50%

### For Developers

**Tuning Detection:**
- Adjust scores in `CalculateMoveScore()` functions
- Lower `confidenceThreshold` to make detection easier
- Raise threshold to require more precision

**Balancing Quality:**
- Modify `forgivenessFactor` in MoveRecognitionSystem
- Higher = more forgiving (easier to get high quality)
- Lower = more strict (harder to get Perfect ratings)

**Visual Feedback:**
- Edit colors in `MoveData.GetMovesForPlant()`
- Adjust `animationIntensity` for move speed
- Tune `screenShakeAmount` for impact feel

---

## Common Issues & Solutions

### "My move isn't being recognized!"

**Possible Causes:**
1. **Drawing doesn't match pattern** - Check the specific move's pattern requirements
2. **Too few/many strokes** - Some moves require specific stroke counts
3. **Shape not distinct enough** - Make your circles rounder, your zigzags sharper

**Solutions:**
- Open the Move Guide (press M in battle)
- Read the drawing hint for your move
- Practice the specific pattern
- Simplify your drawing

### "I always get 'Poor' quality"

**Possible Causes:**
1. **Sloppy drawing** - Wobbly lines, multiple overlapping strokes
2. **Wrong shape** - E.g., drawing oval for Fireball instead of circle
3. **Too large/small** - Extreme sizes can hurt quality

**Solutions:**
- Draw more deliberately and smoothly
- Match the exact shape required
- Use medium-sized drawings
- Practice in non-combat situations

### "Block keeps triggering instead of my attack"

**Possible Causes:**
1. Block is the fallback move (minimum score: 0.15)
2. Your attack drawing doesn't meet the pattern requirements

**Solutions:**
- Make your attack pattern more distinct
- For Fireball, draw a PERFECT circle
- For Burn, make SHARP zigzags
- Add more strokes for LeafStorm (need 5+)

---

## Future Enhancements

Potential improvements to the system:

### Combo Moves
- Detect sequences of moves
- Bonus damage for specific combinations
- "Finisher" moves after combos

### Particle Systems
- Use `visualEffect` enum to spawn particles
- Flames for fire moves
- Bubbles for water moves
- Leaves for grass moves

### Sound Effects
- Unique sounds per `VisualEffect`
- Quality-based sound variations
- Type advantage audio cues

### Move Unlocking
- Start with limited moves
- Unlock more through battles/progression
- Ultimate moves require mastery

---

## Appendix: Quick Reference

### Move Pattern Cheat Sheet

| Move Type | Draw This | Strokes | Key Feature |
|-----------|-----------|---------|-------------|
| Block | ⭕ Circle | 1-3 | Simple & round |
| Fireball | ⭕ Perfect circle | 1-2 | Very round |
| Burn | ⚡ Zigzag | 1-3 | Sharp angles |
| VineWhip | 〰️ Curve | 1-2 | Long & curved |
| LeafStorm | ✨ Scattered | 5+ | Many strokes |
| RootAttack | ⬇️ Vertical lines | 1-3 | Tall & downward |
| WaterSplash | 🌊 Wavy | 2-5 | Curved waves |
| Bubble | 🫧 Multiple circles | 2+ | Small circles |
| HealingWave | 〰️ Horizontal wave | 1-2 | Wide & smooth |

### Power Rankings

**Highest Damage Moves:**
1. Inferno Wave (30) - Flame Tulip
2. Solar Flare (28) - Sunflower
3. Spine Storm, Passion Burst, Strangling Roots, Tidal Burst (26)

**Healing Moves:**
1. Tranquil Petals (25) - Water Lily
2. Bubble Remedy (22) - Bubble Flower

---

**Last Updated:** 2025
**Version:** 1.0 - Initial Enhanced Move System

For integration instructions, see [INTEGRATION_TODO.md](INTEGRATION_TODO.md)
