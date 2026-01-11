# FINAL GAME IDEA: Sketch Blossom

Engine: Unity
Platforms: PC/Mac (Steam), Tablet, Mobile
Input Methods: Mouse (PC), Touch/Stylus (Tablet/Mobile)
Genre: Drawing-Based Battle Game
Theme: Draw to Fight

### Team Members
- Michael Dieterle - Project Lead
- Sanja Nikolic - Gameplay Programmer 
- Stefan - ..
- Marwa - ..

## How to use the TinyCLIP Model
The TinyCLIP Model is the model we use for zero-shot-image-classification. That means that you can draw anything and it will be classified to a label (e.g. plant types). This model uses a python script to run so the user has to download python and some packages for the game to work. All installation steps are planned to be automated. You currently also need to `git checkout Issue34`.
### windows
1. Open the project in Unity. The packages will be automatically installed when you open the project for the first time (you won't have to wait for the installation again). While the packages are not installed, the game will not start and there will be a debug message telling you to wait until the installation is done. <br />
How it works: The installation is handled by the `PythonDownloader` script in `UnityGameFiles/Assets/Editor`. This script takes the asset from `https://github.com/Fewones/Sketch-Blossom/releases/tag/sketchblossom-python-win`, downloads `windows-latest.zip` and extracts it to `Sketch-Blossom\UnityGameFiles\Assets\Python`.
2. Run the project when the packages are installed. You will see a terminal pop up, where you can see the python server requests. The transition to the drawing screen will only succeed if the server has fully started. <br />
How it works: The `DrawingSceneManagerScript` has a `PythonServerManager` object. You can find the scripts in `Assets/Scripts/Model`. The `PythonServerManager` script handles starting the python server by running the `TinyClip.py` script which you can find in `Assets/Python/shared`. This script contains the model used to classify the drawings. The `PythonServerManager` will automatically choose the previously installed python including packages and run the `TinyClip.py` scripts using it (running the script will start the server so it can handle web requests). It might take some time to start up, you will see a loading debug message every 2 seconds. When it is done you will see a debug message that tells you that the server was started. When the application is quit, there will be a debug message saying the server was deactivated.
3. Draw something and submit your drawing. In the terminal you can see each label with its score. The highest ranked label is returned. <br />
The `DrawingSceneManagerScript` has a `ModelManager` object.The `ModelManager` handles web requests and calls the function in the `TinyClip.py` script. When you submit a drawing, the `DrawingSceneManager` will use its `ModelManager` object to send a web request for the classification function to the python server. The method to run this is called `SendImage` and takes a 2D Texture as well as a key. This key will determine which labelMap the Model will use for classification. You can add labelMaps by editing `Assets/Python/shared/labelMaps.json` (currently there are only plant labels). The labelMaps contain as keys detailed descriptions for the model to interpret and as items the label that they will get in the game.
   
### Unix (not tested)
Note: The release assets in https://github.com/Fewones/Sketch-Blossom/releases/tag/sketchblossom-python might also work same as on Windows.
1. If you haven't already, install python and run `pip install virtualenv`.
2. In `Sketch-Blossom\UnityGameFiles\Assets\Python` create a virtualenv named macos-latest or ubuntu-latest (`virtualenv macos-latest` on macOS; `python3 -m venv ubuntu-latest` on Linux)
3. Activate the virtualenv: `source macos-latest/bin/activate`;`source ubuntu-latest/bin/activate`;
4. Run `pip install torch torchvision`
5. Run `pip install -r ../../../requirements.txt`
6. Open the project in Unity. Due to the packages, this might take a while for the first time.
7. Run the project. You might see a terminal pop up, where you can see the python server requests. The transition to the drawing screen will only succeed if the server has fully started.
8. Draw something and submit your drawing. In the terminal you can see each label with its score. The highest ranked label is returned.

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

## 🚀 **PROTOTYPE DEVELOPMENT PRIORITIES** (Target: February 2026)

The core game is **feature-complete and playable**. The focus now is on **refining the core gameplay loop** for a polished prototype. Audio, content expansion, and meta features can wait until after the prototype is validated.

### **Priority 1: Move Detection Refinement**
**Impact:** Critical | **Effort:** Medium | **Target:** December 2025

**Current System:**
- Moves detected by shape only (circles, lines, curves)
- Color is ignored during move detection
- 27 moves with shape-based patterns

**Required Enhancements:**

- [ ] **Color-Based Move Detection**
  - **Problem:** Players can't express creativity with colors during battle
  - **Solution:** Integrate color analysis into move recognition
  - Example: Red fireball vs orange fireball (intensity variation)
  - Example: Light blue vs dark blue water splash (different variants)
  - **Implementation:**
    - Extend `MovesetDetector.cs` to analyze drawing colors
    - Add color scoring to move confidence calculation
    - Update `MoveData.cs` to include expected color ranges per move

- [ ] **Refined Move Patterns**
  - **Problem:** Some moves are too similar (hard to distinguish)
  - **Solution:** Make each move more distinct in pattern + color
  - Add color requirements to move validation
  - Tighten pattern matching for clearer feedback
  - **Files to update:**
    - `MovesetDetector.cs` - Pattern scoring functions
    - `MoveRecognitionSystem.cs` - Quality calculation with color

- [ ] **Visual Feedback During Drawing**
  - Show color indicator while drawing moves
  - Real-time preview of detected color/pattern match
  - "Too red - try lighter" or "Good color match!" feedback

---

### **Priority 2: Plant Recognition Enhancement (ML Model)** 🌺
**Impact:** Critical | **Effort:** High | **Target:** January 2026

**Current System:**
- Rule-based pattern matching (simple thresholds)
- Limited to 9 plant types with rigid requirements
- Binary validation (pass/fail)

**Required Transformation:**

- [ ] **Machine Learning Model Integration**
  - **Problem:** Current system is too rigid, rejects valid creative drawings
  - **Solution:** Train ML model to recognize plants from full-color drawings
  - **Goals:**
    - Accept wider variety of drawing styles
    - Encourage better/more artistic drawings
    - Use entire color palette (not just red/green/blue)
  - **Implementation Steps:**
    1. **Data Collection Phase:**
       - Create data collection mode in DrawingScene
       - Collect 50-100 drawings per plant type from playtesters
       - Label drawings with plant type + quality rating
    2. **Model Training:**
       - Research Unity Barracuda for on-device ML inference
       - Alternative: Train model externally, import to Unity
       - Model inputs: Drawing image (texture) + stroke features
       - Model outputs: Plant type probabilities + confidence
    3. **Integration:**
       - Replace rule-based `PlantRecognitionSystem.cs` pattern matching
       - Keep color analysis as fallback/validation
       - Gradual rollout: ML + rules hybrid system first

- [ ] **Full Color Palette Support**
  - **Current:** Only red/green/blue primary colors
  - **Target:** Support entire color spectrum
  - Purple flowers, orange tulips, pink roses, yellow sunflowers
  - Gradient coloring, shading, artistic expression
  - **Implementation:**
    - Remove color restrictions from drawing canvas
    - Train ML model on diverse colored plant drawings
    - Update validation to accept varied color schemes

- [ ] **Drawing Quality Encouragement**
  - **Problem:** System accepts minimal effort drawings
  - **Solution:** Reward detailed, artistic drawings with better stats
  - Bonus stats for high-detail drawings
  - Visual feedback during drawing ("Looking good!", "Add more detail?")
  - Optional: Show "inspiration gallery" of well-drawn plants

---

### **Priority 3: Battle Scene Animation Refinement**
**Impact:** High | **Effort:** Medium | **Target:** January 2026

**Current System:**
- Color flashes for move effects
- Screen shake for impact
- Basic sprite animations

**Required Enhancements:**

- [ ] **Better Move Animations**
  - **Problem:** Moves lack visual impact and clarity
  - **Solution:** Improve animation system to show what's happening
  - Projectile trajectories (fireballs arc, water splashes)
  - Impact animations (hit effects at target)
  - Plant reaction animations (taking damage, blocking)
  - **Implementation:**
    - Enhance `MoveExecutor.cs:144-162` animation system
    - Add animation curves for projectile motion
    - Create impact prefabs (simple shapes/sprites, not particles yet)

- [ ] **Battle Flow Clarity**
  - Clear turn indicators (whose turn it is)
  - Damage number display (floating text showing damage dealt)
  - Status indicators (HP changes, type advantage feedback)
  - Smooth transitions between turns

- [ ] **Plant Battle Sprites**
  - Idle animation (subtle movement)
  - Attack animation (lunge forward)
  - Hit animation (recoil backward)
  - Victory/defeat poses

**Note:** Full particle effects are post-prototype. Focus on clear, functional animations.

---

### **Priority 4: Combat Balancing & Polish** ⚖️
**Impact:** Critical | **Effort:** Medium | **Target:** January-February 2026

**Current System:**
- Fixed damage values per move
- Simple type advantage (1.5x/0.5x)
- Random AI enemies

**Required Balancing:**

- [ ] **Damage Value Adjustment**
  - **Problem:** Combat may be too easy or too hard
  - **Solution:** Playtest and tune all 27 move damage values
  - **Process:**
    1. Playtest battles with current values
    2. Track: Average battle length, close matches vs blowouts
    3. Adjust move powers to make battles ~5-8 turns
    4. Ensure comeback potential (behind player can win)
  - **Target Balance:**
    - Low-power moves: 15-20 damage
    - Medium moves: 22-26 damage
    - High-power moves: 28-32 damage
    - Healing: 20-25 HP restored
  - **Files:** `MoveData.cs:107-362` - Power values

- [ ] **Enemy Design & Balancing**
  - **Problem:** Need variety in enemy difficulty
  - **Solution:** Create distinct enemy archetypes
  - **Enemy Types:**
    - **Easy (1-2 stars):** Low HP, weak attacks, predictable
    - **Medium (3 stars):** Balanced stats, uses type advantage
    - **Hard (4-5 stars):** High HP, strong attacks, blocks
  - **Implementation:**
    - Update `EncounterData.cs` with proper stat scaling
    - Create 3-5 distinct enemy plants per element
    - Test difficulty curve progression

- [ ] **Player Plant Stat Balancing**
  - Review base stats for all 9 plant types
  - Ensure each plant has viable strategy
  - Balance drawing difficulty vs power (harder to draw = stronger?)
  - Test Wild Growth upgrade balance (+50% may be too much?)

- [ ] **Type Advantage Refinement**
  - Current: 1.5x super effective, 0.5x not effective
  - Test if these multipliers feel impactful
  - Consider: 2.0x/0.5x for clearer advantage?
  - Ensure type matchup matters but isn't auto-win

---

### **Priority 5: Enemy AI Enhancement** 🤖
**Impact:** High | **Effort:** Low-Medium | **Target:** February 2026

**Current AI** (`DrawingBattleSceneManager.cs:484-502`):
- Picks random offensive move
- No strategy or adaptation
- Perfect execution (1.0 quality always)

**Required Improvements:**

- [ ] **Difficulty-Based AI Strategy**
  - **Easy (1-2 stars):**
    - Keep random selection
    - Lower quality execution (0.5-0.7)
    - Never blocks
  - **Medium (3 stars):**
    - Check type matchup, prefer super-effective
    - Medium quality (0.7-0.9)
    - Block if HP < 40%
  - **Hard (4-5 stars):**
    - Full type advantage awareness
    - High quality (0.9-1.0)
    - Block if HP < 50% or predicting big attack
    - Counter-strategy (react to player patterns)

- [ ] **Challenge Without Frustration**
  - AI should feel fair, not cheap
  - Provide tells/patterns players can learn
  - Hard enemies should be beatable with skill
  - **Implementation:** `DrawingBattleSceneManager.cs:ExecuteEnemyTurn()`

---

### **Post-Prototype Features** 🔮
*(Deprioritized until after February prototype validation)*

These features are important for the final game but not critical for prototype:

**Polish & Feel:**
- Audio system (sound effects, music)
- Particle effects & advanced VFX
- Tutorial system
- Accessibility features

**Content Expansion:**
- More moves per plant (currently 3, eventually 5-7)
- Status effects (poison, burn, freeze)
- Multiple world map regions
- Boss battles

**Meta Features:**
- Achievement system
- Daily challenges
- Player stats tracking
- Garden/collection view

**Technical:**
- Platform optimization (mobile)
- Performance profiling
- Unit testing
- Code refactoring

---

## 📈 **Development Timeline to Prototype**

### **Phase 1: Move & Plant Refinement** (December 2025)
**Focus:** Core recognition systems
1. Implement color-based move detection
2. Begin ML model research and data collection
3. Refine move patterns to be more distinct
4. Initial playtesting and feedback collection

**Deliverable:** Moves feel more expressive with color integration

---

### **Phase 2: ML Integration & Balancing** (January 2026)
**Focus:** Plant recognition ML + combat balance
1. Train and integrate ML plant recognition model
2. Enable full color palette for plant drawing
3. Playtest and balance all move damage values
4. Design and implement enemy difficulty tiers
5. Enhance battle animations (projectiles, impacts)

**Deliverable:** Plants can be drawn artistically, combat feels balanced

---

### **Phase 3: Polish & AI Enhancement** (February 2026)
**Focus:** Battle polish + enemy intelligence
1. Implement difficulty-based AI strategies
2. Refine battle scene animations and clarity
3. Balance Wild Growth and progression systems
4. Final playtesting and iteration
5. Bug fixes and optimization

**Deliverable:** Complete playable prototype ready for validation

---

### **Prototype Validation Goals** (February 2026)

The prototype should demonstrate:
- ✅ **Drawing is fun and expressive** (color palette, artistic freedom)
- ✅ **Recognition feels fair** (ML model accepts creative drawings)
- ✅ **Combat is engaging** (balanced, clear animations, strategic)
- ✅ **Difficulty curve works** (easy to learn, challenging to master)
- ✅ **Core loop is satisfying** (draw → battle → progress)

**Post-prototype decision:** Proceed to full production if validation succeeds

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
