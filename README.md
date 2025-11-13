# FINAL GAME IDEA: Sketch Blossom

Engine: Unity
Platforms: PC/Mac (Steam), Tablet, Mobile
Input Methods: Mouse (PC), Touch/Stylus (Tablet/Mobile)
Genre: Drawing-Based Battle Game
Theme: Draw to Fight

### Team Members
- Michael Dieterle - Project Lead
- Sanja Nikolic - ..
- Stefan - ..
- Marwa - ..

## Core Gameplay Loop (PRIORITY)

The game emphasizes **real-time drawing combat** where what you draw directly determines what happens in battle.

### 1. Draw Your Plant

**Game Start:**
- Player draws a plant using limited strokes
- **Intuitive Analysis System**:
  - Sunflower → Fire Plant (with fire abilities)
  - Cactus → Grass Plant (with grass abilities)
  - Water Lily → Water Plant (with water abilities)
- System automatically detects plant type based on visual characteristics
- Plant enters battle with type-specific moveset

### 2. Battle System - Draw to Attack

**Combat Mechanics:**
- Player must **physically draw** attacks and moves during battle
- **Detection System**:
  - Draw a fireball → System recognizes → Fire attack executes
  - Draw a water splash → System recognizes → Water attack executes
  - Draw unrecognizable shape → **NO ATTACK HAPPENS**
- Each plant type has specific movesets that can be drawn:
  - **Fire Plants**: Fireballs, flame waves, burn effects
  - **Water Plants**: Water splashes, bubbles, healing waves
  - **Grass Plants**: Vine whips, leaf storms, root attacks

**Combat Flow:**
1. Player draws their move/attack
2. System analyzes the drawing in real-time
3. If detected → Move executes with appropriate effects
4. If not detected → Attack fails, turn wasted
5. Repeat until battle ends

### 3. Victory & Progression

- Defeat enemies using detected moves
- Progress through encounters
- (Future: Unlock new movesets, face stronger enemies)

## Key Design Pillars

**Drawing Recognition is Core:**
- Success depends on drawing recognizable moves
- Intuitive plant-to-type mapping (visual characteristics matter)
- Real-time feedback on detection
- Skill-based combat through drawing accuracy

**Type System:**
- Water > Fire > Grass > Water (type advantage multipliers)
- Each type has unique moveset to draw from

## Development Priority

**PHASE 1 - CORE LOOP (Current Focus):**
1. Plant drawing & analysis system
2. Intuitive type detection (sunflower = fire, etc.)
3. Battle scene with drawing input
4. Moveset detection system (fireball, water splash, etc.)
5. Attack execution based on detected drawings
6. Failure state when drawing not recognized

## Detailed Gameplay Diagram
```
┌─────────────────────────────────────────────────┐
│              GAME START                         │
│           Drawing Canvas Appears                │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  DRAW YOUR PLANT (Limited Strokes)             │
│  → Player draws a plant freehand                │
│  → Examples:                                    │
│     • Sunflower (round petals, stem)           │
│     • Cactus (spiky, thick body)               │
│     • Water Lily (floating leaves, water)      │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  INTUITIVE PLANT ANALYSIS                      │
│  → System analyzes drawing characteristics:     │
│     • Shape recognition (round, spiky, wavy)    │
│     • Visual patterns (petals, thorns, leaves)  │
│     • Color/shading (future enhancement)        │
│  → Automatically assigns type:                  │
│     • Sunflower → FIRE PLANT 🔥                │
│     • Cactus → GRASS PLANT 🌱                  │
│     • Water Lily → WATER PLANT 💧              │
│  → Plant gets type-specific moveset             │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  ENTER BATTLE SCENE                            │
│  → Player's plant appears on battlefield        │
│  → Enemy plant appears (AI/preset)              │
│  → Drawing canvas ready for combat              │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  PLAYER'S TURN: Draw Attack/Move               │
│  → Drawing canvas activates                     │
│  → Player draws their move (e.g., fireball)     │
│  → Limited time/strokes per turn                │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  REAL-TIME MOVESET DETECTION                   │
│  → System analyzes drawn shape:                 │
│                                                  │
│  ✓ RECOGNIZED:                                  │
│     • Fireball (circle with flames) → Attack!   │
│     • Water Splash (wavy lines) → Attack!       │
│     • Vine Whip (curved line) → Attack!         │
│                                                  │
│  ✗ NOT RECOGNIZED:                              │
│     • Random scribble → NO ATTACK               │
│     • Incomplete shape → NO ATTACK              │
│     • Wrong type move → NO ATTACK               │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  ATTACK EXECUTION                               │
│  → If detected: Move executes                   │
│     • Animation plays                           │
│     • Damage calculated (with type advantage)   │
│     • Enemy HP reduced                          │
│  → If not detected: Turn wasted                 │
│     • Feedback: "Move not recognized!"          │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  ENEMY TURN                                     │
│  → AI/Preset enemy attacks                      │
│  → Damage to player's plant                     │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
         Repeat: Draw → Detect → Execute
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  BATTLE END                                     │
│  → Player HP = 0 → DEFEAT                       │
│  → Enemy HP = 0 → VICTORY                       │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  PROGRESSION (Future Phase)                     │
│  → Unlock new movesets                          │
│  → Face stronger enemies                        │
│  → Draw new plants with new abilities           │
└─────────────────────────────────────────────────┘

```

## 🛠️ **UNITY TECHNICAL ARCHITECTURE**

### **Core Systems to Build (Priority Order)**

```
SketchBlossom_Unity/
├── Assets/
│   ├── Scripts/
│   │   ├── Drawing/
│   │   │   ├── DrawingCanvas.cs          ← ✅ Cross-platform input handling
│   │   │   ├── DrawingManager.cs         ← ✅ Manage drawing flow
│   │   │   ├── StrokeRecorder.cs         ← Track drawing strokes
│   │   │   ├── PlantAnalyzer.cs          ← 🔥 PRIORITY: Intuitive plant type detection
│   │   │   └── MovesetDetector.cs        ← 🔥 PRIORITY: Attack/move recognition
│   │   │
│   │   ├── Combat/
│   │   │   ├── CombatManager.cs          ← 🔥 PRIORITY: Turn-based drawing combat
│   │   │   ├── MoveExecutor.cs           ← 🔥 PRIORITY: Execute detected moves
│   │   │   ├── TypeAdvantage.cs          ← Water>Fire>Grass calculations
│   │   │   ├── DamageCalculator.cs       ← Damage with type multipliers
│   │   │   └── TurnManager.cs            ← Player/Enemy turn handling
│   │   │
│   │   ├── Units/
│   │   │   ├── BattleUnit.cs             ← ✅ Plant unit in battle (HP, Type, Stats)
│   │   │   ├── DrawnUnitData.cs          ← ✅ Store drawn plant data
│   │   │   ├── Moveset.cs                ← Available moves per plant type
│   │   │   └── MoveData.cs               ← Individual move properties
│   │   │
│   │   ├── Recognition/
│   │   │   ├── ShapeRecognizer.cs        ← 🔥 PRIORITY: Basic shape detection
│   │   │   ├── PatternMatcher.cs         ← Match drawing to known moves
│   │   │   ├── FeatureExtractor.cs       ← Extract drawing characteristics
│   │   │   └── TrainingDataManager.cs    ← (Future: ML training data)
│   │   │
│   │   └── UI/
│   │       ├── BattleUI.cs               ← Battle HUD (HP bars, turn indicator)
│   │       ├── FeedbackDisplay.cs        ← Show "Move recognized!" or "Failed!"
│   │       └── DrawingPrompt.cs          ← Show available moves to draw
│   │
│   ├── Scenes/
│   │   ├── MainMenu.scene                ← ✅ Game start
│   │   ├── DrawingScene.scene            ← ✅ Draw initial plant
│   │   └── BattleScene.scene             ← 🔥 PRIORITY: Combat with drawing input
│   │
│   └── Prefabs/
│       ├── StrokeLine.prefab             ← ✅ Visual line for drawing
│       ├── PlantUnit.prefab              ← Player/Enemy plant in battle
│       └── AttackEffect.prefab           ← VFX for moves (fireball, splash, etc.)
```

### **Phase 1 Implementation Checklist**

**1. Drawing System** ✅ **COMPLETE**
- [x] DrawingCanvas.cs - Input handling
- [x] DrawingManager.cs - Flow management
- [x] Basic stroke rendering
- [x] PlantGuideBook.cs - Interactive hint book system
- [x] DrawingSceneUI.cs - Enhanced UX with feedback
- [ ] Enhanced stroke data (velocity, pressure, pattern recognition data)

**2. Plant Recognition System** ✅ **COMPLETE**
- [x] PlantAnalyzer.cs - Detect plant type from drawing
  - Sunflower detection (round petals, center circle)
  - Cactus detection (vertical shape, spiky edges)
  - Water Lily detection (floating, wavy/rounded leaves)
- [x] Intuitive characteristic mapping system
- [x] Visual feedback: Show detected type to player
- [x] PlantDetectionFeedback.cs - UI feedback component

**3. Battle System Integration** ✅ **COMPLETE**
- [x] CombatManager.cs - Core battle loop
  - Turn management (player → draw → detect → enemy → repeat)
  - Drawing input during player turn
  - Win/lose conditions
  - HP tracking
- [x] BattleUI integration with drawing canvas
- [x] Move detection integrated into battle flow

**4. Moveset Detection System** ✅ **COMPLETE**
- [x] MovesetDetector.cs - Recognize attacks from drawings
  - Fireball (circle) - Fire Type
  - Flame Wave (horizontal wavy) - Fire Type
  - Burn (zigzag) - Fire Type
  - Vine Whip (curved line) - Grass Type
  - Leaf Storm (multiple strokes) - Grass Type
  - Root Attack (vertical lines) - Grass Type
  - Water Splash (upward waves) - Water Type
  - Bubble (circles) - Water Type
  - Healing Wave (horizontal wave) - Water Type
- [x] Real-time analysis during player turn
- [x] Success/failure feedback system

**5. Move Execution System** ✅ **COMPLETE**
- [x] MoveExecutor.cs - Execute recognized moves
  - Attack animation system
  - Damage calculation (attack stat × type multiplier)
  - Apply damage to enemy unit
  - Visual effects (fireballs, water splashes, vines)
- [x] Handle failed recognition (no attack, wasted turn)
- [x] Turn end transition
- [x] Type advantage system (Water > Fire > Grass > Water)
- [x] MoveData.cs - Move definitions with properties
