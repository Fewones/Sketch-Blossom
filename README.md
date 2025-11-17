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

## Core Gameplay Loop

The game features **turn-based drawing combat** where what you draw directly determines what happens in battle. Drawing quality affects damage output, and plant choice matters due to type advantages and permadeath consequences.

### 1. Draw Your Starting Plant

**Game Start:**
- Player draws a plant using limited strokes with **specific color requirements**
- **Strict Validation System** analyzes:
  - **Color**: Red = Fire, Green = Grass, Blue = Water
  - **Shape Features**: Circles, vertical/horizontal lines, curves, overlaps
  - **Plant-Specific Patterns**:
    - **Fire Plants**: Sunflower (4+ red circles + green stem), Fire Rose (overlapping petals), Flame Tulip (vertical strokes)
    - **Grass Plants**: Cactus (vertical green lines), Vine Flower (curved strokes), Grass Sprout (many short strokes)
    - **Water Plants**: Water Lily (horizontal blue strokes), Coral Bloom (overlapping), Bubble Flower (blue circles)
- **Invalid drawings are rejected** - must redraw until validation passes
- Each plant type has unique base stats (HP: 28-40, Attack: 8-20, Defense: 6-16)

### 2. Explore & Prepare

**World Map Navigation:**
- Explore the world map to find enemies
- Click enemies to preview their type and difficulty (1-5 stars)
- Choose when to engage in battle

**Plant Selection:**
- Before each battle, select which plant from your inventory to use
- Strategic choice based on type matchup and plant health
- **Permadeath risk**: Losing a battle permanently removes that plant

### 3. Battle System - Draw to Attack

**Turn-Based Combat:**
- **Player Turn**:
  1. Draw your attack/move on the battle canvas
  2. Click "Finish Drawing" to submit
  3. System analyzes pattern and recognizes move (or fails)
  4. Recognized move executes with quality-based damage
  5. Failed recognition = wasted turn, no damage
- **Enemy Turn**: AI opponent executes a random offensive move

**27 Unique Moves** (3 per plant type):
- **Fire Plants** (Sunflower, Fire Rose, Flame Tulip): Block, Fireball, Solar Flare/Inferno/Flame Burst
- **Grass Plants** (Cactus, Vine Flower, Grass Sprout): Block, Vine Whip, Needle Storm/Root Bind/Leaf Shield
- **Water Plants** (Water Lily, Coral Bloom, Bubble Flower): Block, Water Splash, Tidal Wave/Coral Strike/Bubble Blast

**Drawing Quality Matters:**
- System scores how well your drawing matches the intended move (0.0 - 1.0)
- Quality multipliers affect damage:
  - **Perfect** (≥0.9): 1.5x damage
  - **Excellent** (≥0.75): ~1.3x damage
  - **Good** (≥0.6): ~1.1x damage
  - **Decent** (≥0.4): 1.0x damage
  - **Poor** (<0.4): 0.5x damage minimum
- Visual feedback shows recognition quality after each turn

**Type Advantage System:**
- **Water > Fire**: 1.5x damage (super effective)
- **Fire > Grass**: 1.5x damage (super effective)
- **Grass > Water**: 1.5x damage (super effective)
- Reverse matchups: 0.5x damage (not very effective)
- Same type: 1.0x (neutral)

**Damage Formula:**
```
damage = (movePower + attackStat) × qualityMultiplier × typeAdvantage × defenseReduction
if blocking: damage × 0.5
```

### 4. Victory, Defeat & Progression

**Victory Path:**
- Enemy HP reaches 0 → Battle won
- **Post-Battle Choice**:
  - **Wild Growth**: Upgrade current plant (+50% to all stats permanently)
  - **Tame**: Add defeated enemy to your plant inventory
- Return to world map with upgraded/expanded roster

**Defeat Path (Rogue-like Permadeath):**
- Your plant's HP reaches 0 → **Plant dies permanently** and is removed from inventory
- **If you have other plants**: Return to plant selection, choose another, continue journey
- **If no plants remain**: **GAME OVER** → Return to main menu, start fresh

**Progression Strategy:**
- Build a diverse plant collection through taming
- Upgrade key plants through Wild Growth for difficult battles
- Manage type matchups strategically
- Risk vs reward: Use strong plants (safer) or weaker plants (preserve stronger for harder fights)

## Key Design Pillars

**Drawing Recognition is Core:**
- Success depends on drawing recognizable moves
- Intuitive plant-to-type mapping (visual characteristics + color matter)
- Real-time feedback on detection quality
- Skill-based combat through drawing accuracy

**Type System:**
- Water > Fire > Grass > Water (rock-paper-scissors with 1.5x/0.5x multipliers)
- 9 unique plant types (3 per element) with distinct stats
- 27 unique moves (3 per plant type)

**Rogue-like Risk:**
- Permadeath: Losing a battle permanently removes that plant
- Strategic resource management: Use strong plants or preserve them?
- Progression through collection (taming) and upgrades (wild growth)

## Development Status

**✅ PHASE 1 - CORE LOOP: COMPLETE**
All fundamental systems are implemented and playable:
1. ✅ Plant drawing & strict validation system (9 plant types)
2. ✅ Color + shape-based type detection
3. ✅ Turn-based battle system with drawing input
4. ✅ Moveset detection with quality scoring (27 moves)
5. ✅ Attack execution with type advantages and animations
6. ✅ Failure recognition handling
7. ✅ World map exploration and enemy encounters
8. ✅ Progression systems (Wild Growth upgrades, Taming)
9. ✅ Permadeath and game over mechanics

**🎯 NEXT PHASES:**
See "Next Implementation Priorities" section below for detailed roadmap to full release.

## Detailed Gameplay Diagram
```
┌─────────────────────────────────────────────────┐
│           MAIN MENU SCENE                       │
│  → Start New Game / Continue                    │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  DRAWING SCENE - Create Your First Plant       │
│  → Drawing canvas with guidebook available      │
│  → Draw with RED/GREEN/BLUE colors              │
│  → Examples:                                    │
│     • Sunflower: 4+ red circles + green stem   │
│     • Cactus: 2+ vertical green lines          │
│     • Water Lily: 3+ horizontal blue strokes   │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  STRICT PLANT VALIDATION                       │
│  → Color Analysis: Red=Fire, Green=Grass, Blue=Water │
│  → Shape Analysis:                              │
│     • Circle count, line direction, overlaps    │
│     • Aspect ratio, compactness, curviness      │
│  → Pattern Matching (9 plant types):            │
│     • Fire: Sunflower, Fire Rose, Flame Tulip  │
│     • Grass: Cactus, Vine Flower, Grass Sprout │
│     • Water: Water Lily, Coral Bloom, Bubble Flower │
│                                                  │
│  ✓ VALID: Plant created with stats & moves     │
│  ✗ INVALID: Must redraw (feedback provided)    │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  WORLD MAP SCENE - Explore & Find Battles      │
│  → Navigate map to discover enemies             │
│  → Click enemy for preview:                     │
│     • Enemy type (Fire/Grass/Water)            │
│     • Difficulty (1-5 stars)                   │
│     • Stats preview                             │
│  → Choose when to engage                        │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  PLANT SELECTION SCENE                         │
│  → View your plant inventory                    │
│  → Each plant shows:                            │
│     • Type, HP, Attack, Defense                │
│     • Current condition                         │
│  → Select plant for upcoming battle             │
│  → Strategic choice based on type matchup       │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  DRAWING BATTLE SCENE - TURN-BASED COMBAT      │
│  → Player plant vs Enemy plant displayed        │
│  → HP bars, stats, turn indicator               │
│  → Move guidebook available                     │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  ╔═══════════════════════════════════════════╗ │
│  ║         PLAYER TURN LOOP                  ║ │
│  ╚═══════════════════════════════════════════╝ │
│                                                  │
│  1. DRAW YOUR MOVE                              │
│     → Drawing canvas activates                  │
│     → Draw attack pattern (circle, wave, etc.)  │
│     → Click "Finish Drawing"                    │
│                                                  │
│  2. MOVE RECOGNITION & QUALITY SCORING          │
│     → System analyzes:                          │
│       • Shape features (circular, vertical, curved) │
│       • Pattern matching against 27 moves       │
│       • Confidence threshold (≥0.5 required)    │
│     → Quality calculation (0.0 - 1.0):          │
│       • Perfect (≥0.9) → 1.5x damage           │
│       • Good (≥0.6) → 1.1x damage              │
│       • Poor (<0.4) → 0.5x damage              │
│                                                  │
│     ✓ RECOGNIZED:                               │
│       • "Fireball - Excellent!" (quality shown) │
│       • Move proceeds to execution              │
│                                                  │
│     ✗ NOT RECOGNIZED:                           │
│       • "Move not recognized! Try again."       │
│       • Turn wasted, no damage                  │
│       • Must redraw                             │
│                                                  │
│  3. MOVE EXECUTION (if recognized)              │
│     → Calculate damage:                         │
│       damage = (movePower + attack) × quality  │
│              × typeAdvantage × defenseReduction │
│     → Type advantage check:                     │
│       • Water>Fire, Fire>Grass, Grass>Water: 1.5x │
│       • Reverse matchups: 0.5x                  │
│     → Animation: Drawing becomes projectile     │
│     → Apply damage to enemy                     │
│     → Screen shake (intensity by move power)    │
│     → Effectiveness feedback displayed          │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  ╔═══════════════════════════════════════════╗ │
│  ║          ENEMY TURN                       ║ │
│  ╚═══════════════════════════════════════════╝ │
│                                                  │
│  → Simple AI selects random offensive move      │
│  → Perfect execution (1.0 quality always)       │
│  → Same damage calculation with type advantage  │
│  → Apply damage to player plant                 │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
         ┌────────┴────────┐
         │   Battle Over?  │
         └────────┬────────┘
                  │
        ┌─────────┴─────────┐
        │                   │
        ▼                   ▼
    Enemy HP ≤ 0        Player HP ≤ 0
    (VICTORY)           (DEFEAT)
        │                   │
        │                   ▼
        │         ┌─────────────────────────────────────┐
        │         │  ROGUE-LIKE PERMADEATH              │
        │         │  → Plant dies PERMANENTLY           │
        │         │  → Removed from inventory           │
        │         │                                      │
        │         │  Check Remaining Plants:            │
        │         │  • 0 plants → GAME OVER             │
        │         │    └→ Return to Main Menu           │
        │         │  • 1+ plants → Continue             │
        │         │    └→ Return to Plant Selection     │
        │         └─────────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────────────────────┐
│  POST-BATTLE SCENE - Victory Rewards            │
│  → Defeated enemy displayed                     │
│  → Choose reward path:                          │
│                                                  │
│  Option 1: WILD GROWTH                          │
│    → Upgrade current plant permanently          │
│    → +50% to HP, Attack, Defense                │
│    → Plant becomes significantly stronger       │
│                                                  │
│  Option 2: TAME                                 │
│    → Add defeated enemy to your inventory       │
│    → Grows your plant collection                │
│    → Future strategic options                   │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  WILD GROWTH SCENE (if chosen)                  │
│  → Visual upgrade animation                     │
│  → Stats increased: HP × 1.5, ATK × 1.5, DEF × 1.5 │
│  → Plant sprite updated                         │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  TAME SCENE (if chosen)                         │
│  → Enemy joins your collection                  │
│  → Added to plant inventory                     │
│  → Can be selected for future battles           │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
         Return to WORLD MAP SCENE
         (Loop continues: Explore → Battle → Grow)

```

## 🛠️ **UNITY TECHNICAL ARCHITECTURE**

### **Current Implementation Status**

```
UnityGameFiles/
├── Assets/
│   ├── Scripts/
│   │   ├── Drawing/
│   │   │   ├── SimpleDrawingCanvas.cs        ✅ Mouse/touch drawing with stroke tracking
│   │   │   ├── BattleDrawingCanvas.cs        ✅ Battle-specific drawing (thick lines)
│   │   │   ├── DrawingSceneManager.cs        ✅ Initial plant creation flow
│   │   │   ├── PlantGuideBook.cs             ✅ Interactive hint system
│   │   │   └── DrawingSceneUI.cs             ✅ Enhanced UX with feedback
│   │   │
│   │   ├── Recognition/
│   │   │   ├── PlantRecognitionSystem.cs     ✅ 9 plant types with strict validation
│   │   │   │                                      - Color analysis (Red/Green/Blue)
│   │   │   │                                      - Shape features (circles, lines, curves)
│   │   │   │                                      - Pattern matching per plant
│   │   │   ├── MovesetDetector.cs            ✅ 27 move patterns (3 per plant type)
│   │   │   │                                      - Feature extraction
│   │   │   │                                      - Confidence scoring (≥0.5 threshold)
│   │   │   └── MoveRecognitionSystem.cs      ✅ Quality scoring (0.5x - 1.5x damage)
│   │   │
│   │   ├── Combat/
│   │   │   ├── DrawingBattleSceneManager.cs  ✅ Main battle controller
│   │   │   │                                      - Turn-based state machine
│   │   │   │                                      - Drawing → Recognition → Execution
│   │   │   │                                      - Damage calculation with type advantage
│   │   │   │                                      - Victory/Defeat/Permadeath
│   │   │   ├── BattleUnit.cs                 ✅ Plant stats, HP tracking, blocking
│   │   │   ├── MoveData.cs                   ✅ All 27 moves defined with properties
│   │   │   │                                      - Unique colors & effects per move
│   │   │   │                                      - Type advantage system (1.5x/0.5x)
│   │   │   ├── MoveExecutor.cs               ✅ Move execution with animations
│   │   │   └── MoveGuideBook.cs              ✅ In-battle move reference
│   │   │
│   │   ├── World/
│   │   │   ├── WorldMapSceneManager.cs       ✅ Enemy exploration & battle preview
│   │   │   ├── PlantSelectionSceneManager.cs ✅ Choose plant before battle
│   │   │   ├── PostBattleManager.cs          ✅ Wild Growth / Tame choice
│   │   │   ├── WildGrowthManager.cs          ✅ +50% stat upgrade
│   │   │   └── TameSceneManager.cs           ✅ Add enemy to inventory
│   │   │
│   │   ├── Data/
│   │   │   ├── PlayerInventory.cs            ✅ Plant collection management
│   │   │   ├── DrawnPlantData.cs             ✅ Serialized plant data
│   │   │   └── EncounterData.cs              ✅ Enemy difficulty & stats
│   │   │
│   │   └── UI/
│   │       ├── MainMenuManager.cs            ✅ Game start
│   │       ├── PlantDetectionFeedback.cs     ✅ Validation feedback
│   │       └── (Various battle UI components) ✅ HP bars, turn indicators
│   │
│   ├── Scenes/
│   │   ├── MainMenuScene.unity               ✅ Entry point
│   │   ├── DrawingScene.unity                ✅ First plant creation
│   │   ├── WorldMapScene.unity               ✅ Enemy exploration
│   │   ├── PlantSelectionScene.unity         ✅ Pre-battle plant choice
│   │   ├── DrawingBattleScene.unity          ✅ Main turn-based combat
│   │   ├── PostBattleScene.unity             ✅ Victory rewards
│   │   ├── WildGrowthScene.unity             ✅ Stat upgrade animation
│   │   └── TameScene.unity                   ✅ Add enemy to roster
│   │
│   └── Prefabs/
│       └── (Plant sprites, UI elements, etc.)
```

### **System Implementation Status**

**✅ COMPLETE SYSTEMS:**

1. **Drawing & Input**
   - Cross-platform drawing (mouse/touch)
   - Stroke recording and rendering
   - Color-based element detection
   - Interactive guidebook systems (plant & move guides)

2. **Plant Recognition**
   - 9 unique plant types with strict validation
   - Color analysis (Red=Fire, Green=Grass, Blue=Water)
   - Shape feature extraction (circles, lines, curves, overlaps)
   - Plant-specific pattern matching
   - Validation feedback system

3. **Battle System**
   - Turn-based combat state machine
   - Drawing → Recognition → Execution flow
   - HP tracking and damage calculation
   - Type advantage system (1.5x/0.5x multipliers)
   - Victory/Defeat conditions
   - Rogue-like permadeath mechanics

4. **Move System**
   - 27 unique moves (3 per plant type)
   - Pattern recognition with confidence scoring
   - Quality-based damage scaling (0.5x - 1.5x)
   - Move guidebook for battle reference
   - Attack animations using captured drawings

5. **Progression & World**
   - World map exploration
   - Enemy encounter preview system
   - Plant selection before battles
   - Post-battle rewards (Wild Growth & Tame)
   - Plant inventory management
   - Permanent stat upgrades (+50% from Wild Growth)

6. **Visual Polish**
   - Unique color gradients per move
   - Screen shake effects
   - HP bar animations
   - Death animations
   - Turn indicators and feedback messages

---

## 🚀 **NEXT IMPLEMENTATION PRIORITIES**

The core game is **feature-complete and playable**. The following enhancements would improve player experience and production quality:

### **Priority 1: Audio System** 🔊
**Impact:** High | **Effort:** Medium

- [ ] **Sound Effects**
  - Drawing strokes (brush sounds)
  - Move recognition success/failure
  - Attack impact sounds (fireball woosh, water splash, vine whip)
  - Type advantage indicators ("super effective" chime)
  - Plant selection and UI interactions

- [ ] **Music System**
  - Main menu theme
  - World map exploration music
  - Battle music (adaptive based on HP levels)
  - Victory/defeat stingers
  - Upgrade/tame celebration themes

- [ ] **Implementation Notes**
  - Use Unity AudioSource and AudioMixer
  - Integrate with existing `MoveExecutor.cs:99-112` (effectiveness feedback)
  - Add audio events to `DrawingBattleSceneManager.cs` state transitions

---

### **Priority 2: Particle Effects & VFX** ✨
**Impact:** High | **Effort:** Medium-High

- [ ] **Move Visual Effects**
  - Currently: Color flashes only (defined in `MoveData.cs:40-56`)
  - **Needed:** Actual particle systems per effect type:
    - **Flames**: Fire particles, ember trails, heat distortion
    - **Water**: Splash particles, droplets, ripple effects
    - **Grass**: Leaf particles, vine animations, pollen effects
    - **Lightning**: Electric arcs, spark bursts
    - **Crystals**: Shard explosions, glitter

- [ ] **Environmental Effects**
  - Battle background atmosphere (floating particles)
  - Type-based battlefield tints (red for fire, blue for water)
  - HP critical state effects (plant wilting animations)

- [ ] **UI Effects**
  - Damage number pop-ups (with scaling based on effectiveness)
  - Stat upgrade glow effects (Wild Growth scene)
  - Plant taming capture animation

- [ ] **Implementation Notes**
  - Create prefabs for each `VisualEffect` enum type
  - Integrate with `MoveExecutor.cs:144-162` (animation execution)
  - Add to `DrawingBattleSceneManager.cs:1195-1224` (move animation system)

---

### **Priority 3: Enhanced Enemy AI** 🤖
**Impact:** Medium | **Effort:** Low-Medium

**Current AI** (`DrawingBattleSceneManager.cs:484-502`):
- Picks random offensive move
- No strategy or adaptation

**Proposed Enhancements:**

- [ ] **Basic Strategy**
  - Check type matchup and prioritize super-effective moves
  - Use Block when HP is critical (<30%)
  - Avoid using moves the player has blocked before

- [ ] **Difficulty Scaling**
  - Easy (1-2 stars): Random moves (current behavior)
  - Medium (3 stars): Type-aware selection
  - Hard (4-5 stars): Full strategy with blocking and counters

- [ ] **Move Quality Variation**
  - Currently: Always 1.0 quality (perfect execution)
  - **Proposed:** Scale by difficulty (0.6-1.0 range)

- [ ] **Implementation Location**
  - Enhance `DrawingBattleSceneManager.cs:ExecuteEnemyTurn()`
  - Add `EnemyAIController.cs` class for strategy logic
  - Reference `EncounterData.cs` difficulty for scaling

---

### **Priority 4: Advanced Plant Recognition** 🌺
**Impact:** Medium | **Effort:** High

**Current System** (`PlantRecognitionSystem.cs:506-654`):
- Rule-based pattern matching
- 9 fixed plant types

**Future Enhancements:**

- [ ] **Machine Learning Integration**
  - Train model on player drawings
  - Adaptive recognition (learns player's drawing style)
  - Reduce false negatives for valid drawings

- [ ] **More Plant Varieties**
  - Add 3-6 more plants per type
  - Hybrid types (future: Fire/Grass dual-type plants)
  - Legendary/rare plants with unique stat distributions

- [ ] **Stroke Pressure & Speed Analysis**
  - Use drawing velocity for intensity detection
  - Pressure-sensitive input on supported tablets
  - Impacts move quality scoring

- [ ] **Implementation Notes**
  - Research Unity Barracuda for ML inference
  - Extend `PlantRecognitionSystem.cs:233-488` feature extraction
  - Add training data collection mode

---

### **Priority 5: Content Expansion** 📚
**Impact:** High (longevity) | **Effort:** Medium-High

- [ ] **More Moves**
  - Currently: 3 moves per plant (27 total)
  - **Goal:** 5-7 moves per plant type
  - Add variety: Multi-target, status effects (poison, burn, freeze)

- [ ] **Status Effects System**
  - Burning: Damage over time (2-3 turns)
  - Poisoned: Increasing damage each turn
  - Frozen: Skip next turn
  - Requires: Turn tracking, status UI, cure moves

- [ ] **Advanced Moves**
  - Charge moves (draw over 2 turns for massive damage)
  - Combo moves (specific sequence of drawings)
  - Ultimate moves (unlocked after Wild Growth upgrades)

- [ ] **World Map Expansion**
  - Multiple regions with themed enemies
  - Boss battles (require specific strategy)
  - Optional side encounters

- [ ] **Implementation Files**
  - Extend `MoveData.cs` with status effect properties
  - Add `StatusEffectManager.cs` for DOT tracking
  - Expand `MovesetDetector.cs:126-407` with new patterns

---

### **Priority 6: Player Progression & Meta** 🎯
**Impact:** High (retention) | **Effort:** High

- [ ] **Unlockable Content**
  - Achievement system (draw 100 plants, win 50 battles, etc.)
  - Unlock new plant types through achievements
  - Color palette unlocks (purple, orange for new types)

- [ ] **Persistent Player Stats**
  - Total battles won/lost
  - Favorite plant type analytics
  - Drawing quality improvement tracking

- [ ] **Daily Challenges**
  - "Defeat a Fire plant using only Grass moves"
  - "Win a battle with quality >0.8 on all moves"
  - Rewards: Special plant variants, stat boosts

- [ ] **Garden/Collection View**
  - Gallery of all collected plants
  - View stats and battle history per plant
  - Rename plants, assign favorites

---

### **Priority 7: User Experience Improvements** 💡
**Impact:** Medium | **Effort:** Low-Medium

- [ ] **Tutorial System**
  - First-time player onboarding
  - Guided first drawing (highlight areas to draw)
  - Battle mechanics explanation (type advantage tutorial)

- [ ] **Drawing Hints**
  - Show ghost outline for plant shapes (optional)
  - Real-time feedback during drawing (color indicator)
  - Undo last stroke button

- [ ] **Accessibility**
  - Colorblind mode (patterns + color labels)
  - Adjustable drawing sensitivity
  - Text size options

- [ ] **Quality of Life**
  - Fast battle mode (skip animations)
  - Plant quick-select favorites
  - Battle auto-save (resume interrupted battles)
  - Settings menu (volume, graphics quality)

---

### **Priority 8: Platform Optimization** 📱
**Impact:** High (mobile) | **Effort:** Medium

- [ ] **Mobile Performance**
  - Optimize stroke rendering (currently creates many LineRenderers)
  - Reduce memory usage for plant sprite storage
  - Battery optimization (reduce CPU usage during idle)

- [ ] **Touch Controls**
  - Already implemented, but test on various screen sizes
  - Add pinch-to-zoom for world map
  - Haptic feedback on move recognition

- [ ] **Platform-Specific Builds**
  - Test on tablets (iPad, Android tablets)
  - Stylus support optimization
  - Cloud save sync across devices

---

### **Technical Debt & Code Quality** 🔧
**Impact:** Medium (maintainability) | **Effort:** Low-Medium

- [ ] **Refactoring Opportunities**
  - Extract damage calculation to `DamageCalculator.cs` (currently inline in `DrawingBattleSceneManager.cs:867-890`)
  - Separate UI logic from game logic in scene managers
  - Create `TurnManager.cs` for turn state handling

- [ ] **Performance Profiling**
  - Profile `MovesetDetector.cs:53-121` (can be expensive with many strokes)
  - Optimize `PlantRecognitionSystem.cs:233-488` feature extraction
  - Cache frequently calculated values

- [ ] **Testing**
  - Unit tests for damage calculations
  - Unit tests for type advantage system
  - Integration tests for full battle flow

- [ ] **Documentation**
  - XML documentation comments for public APIs
  - Architecture decision records (ADRs)
  - Plant/move design guidelines for content expansion

---

## 📈 **Recommended Development Roadmap**

**Phase 2 - Polish & Feel (2-3 weeks)**
1. Audio System (Priority 1)
2. Particle Effects (Priority 2)
3. UX Improvements (Priority 7)

**Phase 3 - Depth & Strategy (3-4 weeks)**
4. Enhanced AI (Priority 3)
5. Content Expansion (Priority 5)
6. Status Effects System

**Phase 4 - Longevity & Retention (4-6 weeks)**
7. Player Progression (Priority 6)
8. Achievement System
9. Daily Challenges

**Phase 5 - Scale & Release (2-3 weeks)**
10. Platform Optimization (Priority 8)
11. Tutorial System
12. Final testing & bug fixes

**Total estimated development time: 11-16 weeks to full release**

---

## 📝 **Key File References**

For developers implementing the above features:

- **Battle Flow:** `DrawingBattleSceneManager.cs:52-1500`
- **Plant Detection:** `PlantRecognitionSystem.cs:190-654`
- **Move Detection:** `MovesetDetector.cs:53-574`
- **Move Database:** `MoveData.cs:107-379`
- **Damage System:** `DrawingBattleSceneManager.cs:867-890`
- **Type Advantages:** `MoveData.cs:368-379`
- **Progression:** `PostBattleManager.cs`, `WildGrowthManager.cs`, `TameSceneManager.cs`
