# Drawing Battle Scene - Setup Guide

## Overview

The **DrawingBattleScene** is a clean, Pokemon-style turn-based combat system where players draw their battle moves. This scene features:

- Player plant vs Enemy plant combat
- Pokemon-style combat field layout
- HP bars displayed below each unit
- Turn-based gameplay
- Drawing-based move input system
- Move recognition and execution

## Scripts Created

### 1. **BattleDrawingCanvas.cs**
Location: `/Assets/Scripts/Battle/BattleDrawingCanvas.cs`

**Purpose:** Dedicated drawing canvas for battle moves only.

**Features:**
- Clean stroke capture system
- LineRenderer-based drawing
- Drawing enable/disable controls
- Clear canvas functionality
- Stroke data extraction for move detection
- Event system for drawing completion

**Key Methods:**
- `EnableDrawing()` - Start accepting drawing input
- `DisableDrawing()` - Stop drawing input
- `ClearCanvas()` - Clear all strokes
- `FinishDrawing()` - Complete drawing and trigger analysis
- `GetAllLineRenderers()` - Get strokes for move detection
- `SetDrawingColor(Color)` - Change drawing color

### 2. **BattleHPBar.cs**
Location: `/Assets/Scripts/Battle/BattleHPBar.cs`

**Purpose:** Display and animate unit health.

**Features:**
- Animated HP changes
- Color gradient (Green → Yellow → Red)
- HP text display (current/max)
- Damage pulse animation
- Unit name display

**Key Methods:**
- `Initialize(name, maxHP)` - Set up the HP bar
- `SetHP(newHP)` - Update HP value
- `ModifyHP(delta)` - Add/subtract HP
- `PlayDamageAnimation()` - Visual feedback for damage
- `IsAlive()` - Check if unit has HP remaining

### 3. **DrawingBattleSceneManager.cs**
Location: `/Assets/Scripts/Battle/DrawingBattleSceneManager.cs`

**Purpose:** Main battle orchestrator for turn-based combat.

**Features:**
- Turn-based battle loop
- Player turn: drawing phase
- Enemy turn: AI attack
- Move detection integration
- Damage calculation with type advantage
- Victory/Defeat handling
- State management

**Battle Flow:**
```
Start Battle
    ↓
┌─→ Player Turn
│   ├─ Enable Drawing Canvas
│   ├─ Player Draws Move
│   ├─ Detect Move (MovesetDetector)
│   ├─ Execute Move (Damage Calculation)
│   └─ Update HP
│
└─→ Enemy Turn
    ├─ AI Selects Move
    ├─ Execute Move
    ├─ Update HP
    └─ Check Win/Loss

Repeat until Victory or Defeat
```

**Key Features:**
- Integrates with existing `MovesetDetector` and `MoveRecognitionSystem`
- Uses `DrawnUnitData` to load player's plant from drawing scene
- Calculates damage with type advantage, defense, and drawing quality
- Random enemy selection
- Scene transitions to MainMenu after battle

### 4. **DrawingBattleSceneBuilder.cs**
Location: `/Assets/Scripts/Battle/DrawingBattleSceneBuilder.cs`

**Purpose:** Programmatically build the entire battle scene.

**Usage:**
1. Create a new scene in Unity: "DrawingBattleScene"
2. Create an empty GameObject
3. Attach `DrawingBattleSceneBuilder` script
4. In Inspector, toggle the "Build Scene" checkbox OR
5. Right-click component → "Build Battle Scene"

**What It Creates:**
- Main Canvas (Screen Space Overlay)
- Battle Manager with move detection systems
- Combat Field:
  - Player Area (left, 40% width)
    - Player sprite placeholder (green)
    - Player name text
    - Player HP bar
  - Enemy Area (right, 40% width)
    - Enemy sprite placeholder (red)
    - Enemy name text
    - Enemy HP bar
- Drawing Area (bottom 30% of screen)
  - White canvas with black border
  - BattleDrawingCanvas component
- UI Elements:
  - Turn Indicator ("YOUR TURN" / "ENEMY TURN")
  - Action Text (battle messages)
  - Available Moves list
  - "Finish Drawing" button
  - "Clear" button

**Additional Helper Methods:**
- `CreateEventSystem()` - Create input system if missing
- `CreateMainCamera()` - Create camera if missing

## Setup Instructions

### Quick Setup (Automated)

1. **Create the Scene:**
   - File → New Scene
   - Save as "DrawingBattleScene.unity" in `Assets/Scenes/`

2. **Build the Scene:**
   - GameObject → Create Empty
   - Add Component → `DrawingBattleSceneBuilder`
   - Click "Build Scene" checkbox in Inspector

3. **Create EventSystem & Camera:**
   - Right-click `DrawingBattleSceneBuilder` component
   - Select "Create EventSystem"
   - Select "Create Main Camera"

4. **Assign References (if needed):**
   - The builder auto-creates most objects
   - Check `BattleManager` GameObject
   - Verify `DrawingBattleSceneManager` has all references assigned

### Manual Setup (Advanced)

If you prefer manual setup or need to customize:

#### 1. Canvas Setup
```
Canvas (Screen Space Overlay)
├─ CombatField
│  ├─ PlayerArea
│  │  ├─ PlayerSprite (Image)
│  │  ├─ PlayerName (TextMeshProUGUI)
│  │  └─ PlayerHPBar (BattleHPBar component)
│  │     ├─ Background (Image)
│  │     ├─ Fill (Image - Filled)
│  │     └─ HPText (TextMeshProUGUI)
│  │
│  └─ EnemyArea
│     ├─ EnemySprite (Image)
│     ├─ EnemyName (TextMeshProUGUI)
│     └─ EnemyHPBar (BattleHPBar component)
│
├─ DrawingArea (BattleDrawingCanvas component)
│  └─ Border (Outline)
│
└─ UI
   ├─ TurnIndicator (TextMeshProUGUI)
   ├─ ActionText (TextMeshProUGUI)
   ├─ AvailableMovesText (TextMeshProUGUI)
   ├─ FinishDrawingButton (Button)
   └─ ClearDrawingButton (Button)
```

#### 2. Battle Manager
Create GameObject "BattleManager" with:
- `DrawingBattleSceneManager`
- `MovesetDetector`
- `MoveRecognitionSystem`

Connect references in `DrawingBattleSceneManager`:
- Player Unit Display
- Enemy Unit Display
- Player HP Bar
- Enemy HP Bar
- Drawing Canvas
- Buttons
- Text elements

#### 3. LineRenderer Prefab (Optional)
Create a prefab for drawing lines:
- GameObject with LineRenderer component
- Configure width, material, color
- Save as "LineRenderer.prefab"
- Assign to `BattleDrawingCanvas.lineRendererPrefab`

## How to Use

### In Unity Editor:

1. **Run the Scene:**
   - Open DrawingBattleScene
   - Press Play

2. **During Gameplay:**
   - Wait for "YOUR TURN" indicator
   - Draw your move in the drawing area
   - Click "Finish Drawing" to execute
   - Watch the battle unfold!

### Integration with Existing Game:

The scene automatically integrates with your existing systems:

- **Player Unit:** Loaded from `DrawnUnitData.Instance` (from DrawingScene)
- **Move Detection:** Uses existing `MovesetDetector` and `MoveRecognitionSystem`
- **Plant Types:** Uses existing `PlantRecognitionSystem.PlantType`
- **Moves:** Uses existing `MoveData.GetMovesForPlant()`

### Scene Flow:

```
MainMenu → DrawingScene → DrawingBattleScene → MainMenu
           (Draw Plant)   (Battle with Drawing)
```

## Customization

### Change Enemy Selection:
In `DrawingBattleSceneManager.LoadEnemyUnit()`:
```csharp
// Instead of random:
enemyPlantType = PlantRecognitionSystem.PlantType.Sunflower;
```

### Adjust Drawing Area:
Modify in `DrawingBattleSceneBuilder.CreateDrawingArea()`:
```csharp
rt.anchorMin = new Vector2(0.1f, 0.05f);  // X: 10%, Y: 5%
rt.anchorMax = new Vector2(0.9f, 0.35f);  // X: 90%, Y: 35%
```

### Change HP Bar Colors:
In `BattleHPBar` component Inspector:
- Full Health Color
- Medium Health Color
- Low Health Color

### Modify Turn Timing:
In `DrawingBattleSceneManager` Inspector:
- Turn Delay (delay between turns)
- Action Text Delay (how long messages display)

### Add Unit Sprites:
In `DrawingBattleSceneManager.BattleUnitDisplay.Initialize()`:
```csharp
// Load sprite based on plant type
Sprite plantSprite = Resources.Load<Sprite>($"Plants/{plantType}");
unitImage.sprite = plantSprite;
```

## Combat System Details

### Damage Calculation:
```
Base Damage = (Move Base Power + Attacker ATK) × Drawing Quality

Type Multiplier:
- Super Effective: 1.5x
- Not Very Effective: 0.5x
- Neutral: 1.0x

Defense Reduction = 1.0 - (Defender DEF / 100)

Blocking: 0.5x damage

Final Damage = Base Damage × Type Multiplier × Defense Reduction × Blocking
```

### Move Types:
- **Offensive:** Deal damage to enemy
- **Defensive:** Block (reduces next incoming damage by 50%)
- **Healing:** Restore own HP

### Drawing Quality:
Your drawing quality affects damage:
- Perfect: 1.5x damage
- Excellent: 1.3x damage
- Good: 1.1x damage
- Decent: 1.0x damage
- Poor: 0.7x damage
- Very Poor: 0.5x damage

## Troubleshooting

### Issue: "No player unit data found!"
**Solution:** Run DrawingScene first to create your plant, or manually set a default in `LoadPlayerUnit()`.

### Issue: Drawing not working
**Solution:**
- Check EventSystem exists in scene
- Verify BattleDrawingCanvas is enabled
- Check drawing area is visible and interactive

### Issue: Buttons not responding
**Solution:**
- Add EventSystem (use builder helper)
- Verify Canvas has GraphicRaycaster
- Check button onClick events are connected

### Issue: Moves not recognized
**Solution:**
- Ensure MovesetDetector and MoveRecognitionSystem are attached to BattleManager
- Check that references are assigned in Inspector
- Verify you're drawing the correct patterns for your plant's moves

### Issue: NullReferenceException errors
**Solution:**
- Run the DrawingBattleSceneBuilder to auto-create all objects
- Manually assign missing references in DrawingBattleSceneManager Inspector

## File Locations

```
UnityGameFiles/
└── Assets/
    └── Scripts/
        └── Battle/
            ├── BattleDrawingCanvas.cs
            ├── BattleHPBar.cs
            ├── DrawingBattleSceneManager.cs
            ├── DrawingBattleSceneBuilder.cs
            └── README_DrawingBattleScene.md (this file)
```

## Dependencies

Required existing scripts:
- `MovesetDetector.cs` - Detects which move was drawn
- `MoveRecognitionSystem.cs` - Scores drawing quality
- `MoveData.cs` - Move definitions and type advantage
- `PlantRecognitionSystem.cs` - Plant types and stats
- `DrawnUnitData.cs` - Player unit data persistence

## Future Enhancements

Potential additions:
- [ ] Unit sprite system (replace colored placeholders)
- [ ] Attack animations
- [ ] Particle effects for moves
- [ ] Sound effects
- [ ] Multiple enemy encounters
- [ ] Experience/leveling system
- [ ] Item system
- [ ] Status effects (burn, poison, etc.)
- [ ] Special moves/ultimates
- [ ] Battle backgrounds
- [ ] Victory rewards

## Notes

- This is a **clean implementation** separate from existing BattleScene and BattleScene2
- All core combat logic is self-contained
- Integrates seamlessly with existing plant and move systems
- Designed for easy extension and customization
- Uses Unity's UI system (Canvas, Image, TextMeshPro)

## Support

For issues or questions:
1. Check this README
2. Review the scripts' XML documentation comments
3. Examine the existing battle system in BattleScene for reference
4. Test with DrawingBattleSceneBuilder to ensure proper setup

---

**Created for Sketch-Blossom**
Battle scene where drawing meets combat!
