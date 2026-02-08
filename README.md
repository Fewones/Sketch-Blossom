# Sketch Blossom

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

## Overview

Sketch Blossom is a turn-based drawing battle game where players draw plants and moves to fight. The core mechanic: **draw plants to collect, draw moves to attack**. It features roguelike permadeath, progressive collection/upgrade systems, and AI-powered plant recognition using TinyCLIP.

**Key highlights:**
- **CLIP AI-powered plant detection** using TinyCLIP zero-shot image classification for realistic plant recognition
- **Full color palette support** — draw with any color, not just primary RGB
- **27 unique battle moves** across 9 plant types with drawing quality-based damage
- **Roguelike permadeath** — lose a battle and your plant is gone forever
- **Working upgrade system** — Wild Growth screen lets you power up plants by drawing

## How to Use the TinyCLIP Model

The TinyCLIP Model is used for zero-shot image classification. Players draw freely and the AI classifies drawings against label descriptions (e.g. plant types, upgrade categories). The model runs as a Python FastAPI server alongside Unity.

### Windows
1. Open the project in Unity. Python packages are automatically installed on first launch (you'll see a debug message while installation is in progress). Installation is handled by the `PythonDownloader` script in `UnityGameFiles/Assets/Editor`, which downloads from `https://github.com/Fewones/Sketch-Blossom/releases/tag/sketchblossom-python-win` and extracts to `Sketch-Blossom/UnityGameFiles/Assets/Python`.
2. Run the project. A terminal window will appear showing Python server logs. The game waits for the TinyCLIP server to be ready before proceeding. The `PythonServerManager` script in `Assets/Scripts/Model` handles starting the server by running `TinyCLIP.py` from `Assets/Python/shared`.
3. Draw something and submit. The terminal shows each label with its confidence score. The highest-ranked label is returned to Unity via the `ModelManager` HTTP client.
4. **Plant classification:** After receiving the label and score from the server, `PlantRecognitionSystem.AnalyzeDrawing()` maps the label to a plant type. A score >= 0.2 is valid; >= 0.27 indicates a good result.

### Unix (not tested)
Note: The release assets at https://github.com/Fewones/Sketch-Blossom/releases/tag/sketchblossom-python might also work the same as on Windows.
1. If you haven't already, install Python and run `pip install virtualenv`.
2. In `Sketch-Blossom/UnityGameFiles/Assets/Python` create a virtualenv named `macos-latest` or `ubuntu-latest` (`virtualenv macos-latest` on macOS; `python3 -m venv ubuntu-latest` on Linux).
3. Activate the virtualenv: `source macos-latest/bin/activate` or `source ubuntu-latest/bin/activate`.
4. Run `pip install torch torchvision`.
5. Run `pip install -r ../../../requirements.txt`.
6. Open the project in Unity.
7. Run the project. A terminal may appear with server logs. The game waits for the server before proceeding.
8. Draw something and submit.

## CLIP AI Plant Detection

Plant recognition is powered by **TinyCLIP** (`wkcn/TinyCLIP-ViT-39M-16-Text-19M-YFCC15M`), a zero-shot image classification model. Unlike rule-based systems that check for specific shapes and colors, CLIP compares the player's drawing against natural language descriptions of each plant type, making detection much more true to reality.

**How it works:**
1. Player draws a plant using the **full color palette** (any colors, not limited to RGB primaries)
2. The drawing is sent to the TinyCLIP FastAPI server (running locally on port 8000)
3. The model computes image embeddings and compares them against text embeddings for each plant description
4. Cosine similarity scores determine which plant the drawing most closely resembles
5. The best match and its confidence score are returned to Unity

**Label Maps** (`Assets/Python/shared/labelMaps.json`):
- **plant_labels**: 9 plant descriptions (e.g., "a shining sunflower", "a cactus with many spines")
- **upgrade_labels** (per plant): 3 upgrade stat categories (power/defense/health) + 1 blank

This approach accepts a much wider variety of drawing styles and rewards artistic, detailed drawings rather than requiring rigid shape patterns.

## Core Gameplay Loop

The game features **turn-based drawing combat** where what you draw directly determines what happens in battle. Drawing quality affects damage output, and plant choice matters due to type advantages and permadeath consequences.

### 1. Draw Your Starting Plant

**Game Start:**
- Player draws a plant using the **full color palette** — any colors are accepted
- **TinyCLIP AI** analyzes the drawing and classifies it against 9 plant descriptions
- The dominant color determines the element: Red-toned = Fire, Green-toned = Grass, Blue-toned = Water
- **Invalid drawings are rejected** with feedback — must redraw until validation passes
- Each plant type has unique base stats (HP: 28-40, Attack: 8-20, Defense: 6-16)

**9 Plant Types:**

| Element | Plant | HP | ATK | DEF | Description |
|---------|-------|----|-----|-----|-------------|
| Fire | Sunflower | 30 | 18 | 8 | Radiant fire flower with circular petals |
| Fire | Fire Rose | 35 | 16 | 10 | Compact blazing rose with many layers |
| Fire | Flame Tulip | 28 | 20 | 6 | Tall elegant flame flower with simple shape |
| Grass | Cactus | 32 | 12 | 14 | Spiky vertical desert guardian |
| Grass | Vine Flower | 35 | 14 | 12 | Flowing curved vine with blooms |
| Grass | Grass Sprout | 30 | 10 | 16 | Bushy low-growing grass cluster |
| Water | Water Lily | 40 | 10 | 14 | Peaceful floating flower spreading wide |
| Water | Coral Bloom | 38 | 12 | 12 | Branching underwater coral flower |
| Water | Bubble Flower | 36 | 8 | 16 | Clustered bubble-like blooms |

### 2. Explore & Prepare

**World Map Navigation:**
- Explore the world map to find enemies
- Click enemies to preview their type and difficulty (1-5 stars)
- Choose when to engage in battle

**Plant Selection:**
- Before each battle, select which plant from your inventory to use
- Strategic choice based on type matchup and plant health
- **Permadeath risk**: Losing a battle permanently removes that plant

### 3. Battle System — Draw to Attack

**Turn-Based Combat:**
- **Player Turn**:
  1. Draw your attack/move on the battle canvas
  2. Click "Finish Drawing" to submit
  3. System analyzes pattern and recognizes move (or fails)
  4. Recognized move executes with quality-based damage
  5. Failed recognition = wasted turn, no damage
- **Enemy Turn**: AI opponent executes a random offensive move

**27 Unique Moves** (3 per plant type):
- **Fire Plants** (Sunflower, Fire Rose, Flame Tulip): Block, Fireball, Solar Flare / Inferno / Flame Burst
- **Grass Plants** (Cactus, Vine Flower, Grass Sprout): Block, Vine Whip, Needle Storm / Root Bind / Leaf Shield
- **Water Plants** (Water Lily, Coral Bloom, Bubble Flower): Block, Water Splash, Tidal Wave / Coral Strike / Bubble Blast

**Drawing Quality Matters:**
- System scores how well your drawing matches the intended move (0.0 - 1.0)
- Quality multipliers affect damage:
  - **Perfect** (>=0.9): 1.5x damage
  - **Excellent** (>=0.75): ~1.3x damage
  - **Good** (>=0.6): ~1.1x damage
  - **Decent** (>=0.4): 1.0x damage
  - **Poor** (<0.4): 0.5x damage minimum
- Visual feedback shows recognition quality after each turn

**Type Advantage System:**
- **Water > Fire**: 1.5x damage (super effective)
- **Fire > Grass**: 1.5x damage (super effective)
- **Grass > Water**: 1.5x damage (super effective)
- Reverse matchups: 0.5x damage (not very effective)
- Same type: 1.0x (neutral)
- **Normal**: 1.0x damage to any type (no advantage or disadvantage)

**Damage Formula:**
```
damage = (movePower + attackStat) x qualityMultiplier x typeAdvantage x defenseReduction
if blocking: damage x 0.5
```

### 4. Victory, Defeat & Progression

**Victory Path:**
- Enemy HP reaches 0 -> Battle won
- **Post-Battle Choice**:
  - **Wild Growth**: Upgrade current plant permanently (+50% to all stats)
  - **Tame**: Add defeated enemy to your plant inventory — the tamed plant is re-analyzed by CLIP AI to determine its type

**Defeat Path (Roguelike Permadeath):**
- Your plant's HP reaches 0 -> **Plant dies permanently** and is removed from inventory
- **If you have other plants**: Return to plant selection, choose another, continue
- **If no plants remain**: **GAME OVER** -> Return to main menu, start fresh

**Wild Growth Upgrade Screen:**
- After choosing Wild Growth, the player draws **one stroke** on the defeated plant
- **Geometric quality scoring** determines upgrade strength (1.3x - 1.8x multiplier based on stroke length/coverage)
- **Color-based stat bias**: The stroke's color influences which stat gets the most growth:
  - Red tint -> Attack bias
  - Blue tint -> HP bias
  - Green tint -> Defense bias
- Live preview shows HP/ATK/DEF before and after the upgrade
- The stroke is merged with the original plant drawing, visually evolving your plant

## Key Design Pillars

**Drawing Recognition is Core:**
- Success depends on drawing recognizable moves
- TinyCLIP AI allows natural, artistic plant drawings instead of rigid patterns
- Full color palette means creative freedom in how you draw
- Skill-based combat through drawing accuracy

**Type System:**
- Water > Fire > Grass > Water (rock-paper-scissors with 1.5x/0.5x multipliers)
- Normal attacks deal 1.0x damage to any type — consistent but no advantage
- 9 unique plant types (3 per element) with distinct stats
- 27 unique moves (3 per plant type)

**Roguelike Risk:**
- Permadeath: Losing a battle permanently removes that plant
- Strategic resource management: Use strong plants or preserve them?
- Progression through collection (taming) and upgrades (wild growth)

## Development Status

**Completed:**
1. Plant drawing with full color palette support
2. TinyCLIP AI-powered plant detection (9 plant types)
3. Turn-based battle system with drawing input
4. Moveset detection with quality scoring (27 moves)
5. Attack execution with type advantages and animations
6. Failure recognition handling
7. World map exploration and enemy encounters
8. Post-battle reward selection (Wild Growth / Tame)
9. Wild Growth upgrade screen with drawing-based stat upgrades
10. Taming system — add defeated enemies to inventory
11. Permadeath and game over mechanics
12. Plant inventory management with persistence
13. TinyCLIP Python server auto-setup (Windows)

**Still To Do:**
- [ ] **Switch plants in battle** — Allow players to swap to a different plant from their inventory mid-battle
- [ ] **Add more plants** — Expand beyond the current 9 plant types with new designs and stat profiles
- [ ] **Smarter enemy AI** — Current AI picks random moves with perfect execution. Needs strategic behavior: type awareness, defensive blocking, adaptation to player patterns
- [ ] **More enemies** — Increase the number and variety of enemy encounters on the world map
- [ ] **Difficulty levels** — Implement tiered difficulty where harder enemies have more health, access to more plants, or stronger moves:
  - Easy (1-2 stars): Low stats, random moves, no blocking
  - Medium (3 stars): Balanced stats, type awareness, blocks at low HP
  - Hard (4-5 stars): High stats, full strategy, counter-play
- [ ] **Move detection refinement** — Integrate color analysis into move recognition for more expressive combat drawing
- [ ] **Battle animation polish** — Projectile trajectories, impact animations, plant reaction animations, floating damage numbers
- [ ] **Audio system** — Sound effects and music
- [ ] **Tutorial system** — Guided introduction for new players

## Detailed Gameplay Diagram
```
+---------------------------------------------------+
|           MAIN MENU SCENE                         |
|  -> Start New Game / Continue                     |
+-----------------------+---------------------------+
                        |
                        v
+---------------------------------------------------+
|  DRAWING SCENE - Create Your First Plant          |
|  -> Drawing canvas with full color palette        |
|  -> Draw any plant freely (no color restrictions) |
|  -> TinyCLIP AI classifies your drawing           |
|  -> Guidebook available for reference             |
+-----------------------+---------------------------+
                        |
                        v
+---------------------------------------------------+
|  CLIP AI PLANT VALIDATION                         |
|  -> Drawing sent to TinyCLIP server               |
|  -> Zero-shot classification against 9 labels     |
|  -> Color analysis determines element type        |
|  -> Score >= 0.2: valid, >= 0.27: good result     |
|                                                   |
|  VALID: Plant created with stats & moves          |
|  INVALID: Must redraw (feedback provided)         |
+-----------------------+---------------------------+
                        |
                        v
+---------------------------------------------------+
|  WORLD MAP SCENE - Explore & Find Battles         |
|  -> Navigate map to discover enemies              |
|  -> Click enemy for preview:                      |
|     - Enemy type (Fire/Grass/Water)               |
|     - Difficulty (1-5 stars)                      |
|     - Stats preview                               |
|  -> Choose when to engage                         |
+-----------------------+---------------------------+
                        |
                        v
+---------------------------------------------------+
|  PLANT SELECTION SCENE                            |
|  -> View your plant inventory                     |
|  -> Each plant shows:                             |
|     - Type, HP, Attack, Defense                   |
|     - Current condition                           |
|  -> Select plant for upcoming battle              |
|  -> Strategic choice based on type matchup        |
+-----------------------+---------------------------+
                        |
                        v
+---------------------------------------------------+
|  DRAWING BATTLE SCENE - TURN-BASED COMBAT         |
|  -> Player plant vs Enemy plant displayed         |
|  -> HP bars, stats, turn indicator                |
|  -> Move guidebook available                      |
+-----------------------+---------------------------+
                        |
                        v
+---------------------------------------------------+
|  PLAYER TURN LOOP                                 |
|                                                   |
|  1. DRAW YOUR MOVE                                |
|     -> Drawing canvas activates                   |
|     -> Draw attack pattern                        |
|     -> Click "Finish Drawing"                     |
|                                                   |
|  2. MOVE RECOGNITION & QUALITY SCORING            |
|     -> Pattern matching against 27 moves          |
|     -> Confidence threshold (>= 0.5 required)     |
|     -> Quality calculation (0.0 - 1.0):           |
|        Perfect (>=0.9) -> 1.5x damage             |
|        Good (>=0.6) -> 1.1x damage                |
|        Poor (<0.4) -> 0.5x damage                 |
|                                                   |
|     RECOGNIZED: Move proceeds to execution        |
|     NOT RECOGNIZED: Turn wasted, no damage        |
|                                                   |
|  3. MOVE EXECUTION (if recognized)                |
|     -> Calculate damage with type advantage       |
|     -> Animation: Drawing becomes projectile      |
|     -> Screen shake (intensity by move power)     |
|     -> Apply damage to enemy                      |
+-----------------------+---------------------------+
                        |
                        v
+---------------------------------------------------+
|  ENEMY TURN                                       |
|  -> AI selects random offensive move              |
|  -> Perfect execution (1.0 quality always)        |
|  -> Same damage calculation with type advantage   |
|  -> Apply damage to player plant                  |
+-----------------------+---------------------------+
                        |
                        v
                  Battle Over?
                        |
            +-----------+-----------+
            |                       |
            v                       v
      Enemy HP <= 0           Player HP <= 0
       (VICTORY)               (DEFEAT)
            |                       |
            |                       v
            |         +-----------------------------+
            |         |  ROGUELIKE PERMADEATH       |
            |         |  -> Plant dies PERMANENTLY  |
            |         |  -> Removed from inventory  |
            |         |                             |
            |         |  0 plants -> GAME OVER      |
            |         |    -> Return to Main Menu   |
            |         |  1+ plants -> Continue      |
            |         |    -> Plant Selection       |
            |         +-----------------------------+
            |
            v
+---------------------------------------------------+
|  POST-BATTLE SCENE - Victory Rewards              |
|  -> Choose reward path:                           |
|                                                   |
|  Option 1: WILD GROWTH                            |
|    -> Draw one stroke on defeated plant           |
|    -> Stroke quality determines upgrade strength  |
|    -> Stroke color biases stat growth             |
|    -> +50% stats permanently                      |
|                                                   |
|  Option 2: TAME                                   |
|    -> Add defeated enemy to your inventory        |
|    -> CLIP AI re-analyzes the tamed plant         |
|    -> Grows your plant collection                 |
+-----------------------+---------------------------+
                        |
                        v
            Return to WORLD MAP SCENE
            (Loop: Explore -> Battle -> Grow)
```

## Unity Technical Architecture

```
UnityGameFiles/
+-- Assets/
|   +-- Scripts/
|   |   +-- Drawing/
|   |   |   +-- SimpleDrawingCanvas.cs        Mouse/touch drawing with stroke tracking
|   |   |   +-- BattleDrawingCanvas.cs        Battle-specific drawing (thick lines)
|   |   |   +-- DrawingSceneManager.cs        Initial plant creation flow
|   |   |   +-- PlantGuideBook.cs             Interactive hint system
|   |   |   +-- DrawingSceneUI.cs             Enhanced UX with feedback
|   |   |
|   |   +-- Recognition/
|   |   |   +-- PlantRecognitionSystem.cs     CLIP AI + color analysis (9 plant types)
|   |   |   +-- MovesetDetector.cs            27 move patterns (3 per plant type)
|   |   |   +-- MoveRecognitionSystem.cs      Quality scoring (0.5x - 1.5x damage)
|   |   |
|   |   +-- Combat/
|   |   |   +-- DrawingBattleSceneManager.cs  Main battle controller (turn-based loop)
|   |   |   +-- BattleUnit.cs                 Plant stats, HP tracking, blocking
|   |   |   +-- MoveData.cs                   All 27 moves with properties
|   |   |   +-- MoveExecutor.cs               Move execution with animations
|   |   |   +-- MoveGuideBook.cs              In-battle move reference
|   |   |
|   |   +-- World/
|   |   |   +-- WorldMapSceneManager.cs       Enemy exploration & battle preview
|   |   |   +-- PlantSelectionSceneManager.cs Choose plant before battle
|   |   |   +-- PostBattleManager.cs          Wild Growth / Tame choice
|   |   |   +-- WildGrowthSceneManager.cs     Upgrade screen with drawing input
|   |   |   +-- TameSceneManager.cs           Add enemy to inventory
|   |   |
|   |   +-- Model/
|   |   |   +-- PythonServerManager.cs        TinyCLIP server lifecycle management
|   |   |   +-- ModelManager.cs               HTTP client for Python backend
|   |   |
|   |   +-- Data/
|   |   |   +-- PlayerInventory.cs            Plant collection management
|   |   |   +-- DrawnPlantData.cs             Serialized plant data
|   |   |   +-- EncounterData.cs              Enemy difficulty & stats
|   |   |
|   |   +-- UI/
|   |       +-- MainMenuManager.cs            Game start
|   |       +-- PlantDetectionFeedback.cs     Validation feedback
|   |       +-- (Various battle UI components) HP bars, turn indicators
|   |
|   +-- Python/
|   |   +-- shared/
|   |       +-- TinyCLIP.py                   FastAPI server for CLIP classification
|   |       +-- labelMaps.json                Plant & upgrade label definitions
|   |
|   +-- Scenes/
|       +-- MainMenuScene.unity               Entry point
|       +-- DrawingScene.unity                First plant creation
|       +-- WorldMapScene.unity               Enemy exploration
|       +-- PlantSelectionScene.unity         Pre-battle plant choice
|       +-- DrawingBattleScene.unity          Main turn-based combat
|       +-- PostBattleScene.unity             Victory rewards
|       +-- WildGrowthScene.unity             Stat upgrade with drawing
|       +-- TameScene.unity                   Add enemy to roster
|       +-- InventoryScene.unity              View plant collection
```

## Key File References

| File | Purpose |
|------|---------|
| `DrawingBattleSceneManager.cs` | Main battle controller — turn-based loop, AI, damage |
| `PlantRecognitionSystem.cs` | CLIP AI plant detection & validation (9 types) |
| `MovesetDetector.cs` | Move pattern recognition & quality scoring |
| `MoveData.cs` | All 27 moves defined with stats, colors, effects |
| `BattleUnit.cs` | Unit stats, HP tracking, blocking |
| `PlayerInventory.cs` | Plant collection persistence |
| `WildGrowthSceneManager.cs` | Upgrade screen with drawing-based stat upgrades |
| `PythonServerManager.cs` | TinyCLIP Python server lifecycle |
| `ModelManager.cs` | HTTP client for classification requests |
| `TinyCLIP.py` | FastAPI server running TinyCLIP model |
| `labelMaps.json` | Plant & upgrade label definitions |
