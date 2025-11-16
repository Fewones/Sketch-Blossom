# World Map Scene Setup Guide

This guide will help you set up the World Map scene in Unity for Sketch Blossom.

## Overview

The World Map system allows players to:
- Move a character around with WASD keys
- Encounter enemy plants on the map
- Interact with enemies by pressing 'E'
- View battle preview information
- Select a plant to battle with
- Battle the enemy
- Collect rewards after victory
- Return to the world map

## Scripts Created

All scripts are located in `Assets/Scripts/WorldMap/`:

1. **PlayerController.cs** - Handles player movement with WASD
2. **WorldMapEnemy.cs** - Enemy entities on the map
3. **EnemyEncounterData.cs** - Singleton to store encounter data
4. **BattlePreviewUI.cs** - Battle preview popup UI
5. **WorldMapSceneManager.cs** - Main scene controller

## Unity Scene Setup

### Step 1: Create the World Map Scene

1. In Unity, create a new scene: `File > New Scene`
2. Save it as `WorldMapScene` in `Assets/Scenes/`

### Step 2: Create the Player GameObject

1. Create an empty GameObject: `GameObject > Create Empty`
2. Name it `Player`
3. Set Tag to `Player` (Create the tag if it doesn't exist)
4. Add a **SpriteRenderer** component
5. Add a **Rigidbody2D** component:
   - Gravity Scale: `0`
   - Constraints: Freeze Rotation Z
6. Add a **BoxCollider2D** component
7. Attach the **PlayerController** script
8. Set the player's position to `(0, 0, 0)`

**Optional:** Add a sprite for the player (temporary colored square is fine for testing)

### Step 3: Create Enemy Prefab

1. Create an empty GameObject: `GameObject > Create Empty`
2. Name it `EnemyPrefab`
3. Add a **SpriteRenderer** component
4. Add a **BoxCollider2D** component (for collision detection)
5. Attach the **WorldMapEnemy** script
6. Configure WorldMapEnemy settings in Inspector:
   - Interaction Range: `2.0`
   - Interaction Key: `E`
7. Create a Canvas as a child of EnemyPrefab:
   - Right-click EnemyPrefab > `UI > Canvas`
   - Set Canvas Render Mode to `World Space`
   - Set Canvas width/height to `100x30`
   - Position it above the enemy sprite (Y: 1.5)
8. Add a TextMeshProUGUI as a child of the Canvas:
   - Name it `InteractionPrompt`
   - Text: `"Press E to Interact"`
   - Font Size: `12`
   - Alignment: Center
   - Auto Size: Enabled
9. Link the `InteractionPrompt` GameObject to the WorldMapEnemy script's `interactionPrompt` field
10. Drag the EnemyPrefab to `Assets/Prefabs/` to create a prefab
11. Delete the original from the scene

### Step 4: Create Battle Preview UI

1. Create a Canvas in the scene: `GameObject > UI > Canvas`
2. Name it `BattlePreviewCanvas`
3. Set Canvas Scaler to `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`

4. Create a Panel as a child (this will be the popup):
   - Name: `BattlePreviewPanel`
   - Anchors: Center
   - Width: `600`, Height: `400`
   - Background: Dark semi-transparent color

5. Add the following UI elements as children of BattlePreviewPanel:

   **Title Text:**
   - TextMeshProUGUI: `EnemyNameText`
   - Position: Top center
   - Font Size: `36`
   - Alignment: Center

   **Element Text:**
   - TextMeshProUGUI: `ElementText`
   - Font Size: `24`

   **Difficulty Text:**
   - TextMeshProUGUI: `DifficultyText`
   - Font Size: `24`

   **Flavor Text:**
   - TextMeshProUGUI: `FlavorText`
   - Font Size: `18`
   - Alignment: Center
   - Word Wrap: Enabled

   **Go to Battle Button:**
   - Button: `GoToBattleButton`
   - Size: `200 x 50`
   - Text: "GO TO BATTLE"

   **Cancel Button:**
   - Button: `CancelButton`
   - Size: `150 x 50`
   - Text: "Cancel"

6. Attach the **BattlePreviewUI** script to BattlePreviewCanvas
7. Link all UI elements in the Inspector:
   - Popup Panel → `popupPanel`
   - Enemy Name Text → `enemyNameText`
   - Element Text → `elementText`
   - Difficulty Text → `difficultyText`
   - Flavor Text → `flavorText`
   - Go to Battle Button → `goToBattleButton`
   - Cancel Button → `cancelButton`

### Step 5: Create World Map Manager

1. Create an empty GameObject: `GameObject > Create Empty`
2. Name it `WorldMapManager`
3. Attach the **WorldMapSceneManager** script
4. Configure in Inspector:
   - **Player Prefab**: Drag the Player prefab (if you made one) or leave empty to use scene player
   - **Player Spawn Position**: `(0, 0, 0)`
   - **Enemy Prefab**: Drag the EnemyPrefab you created
   - **Number of Enemies**: `2`
   - **Spawn Enemies On Start**: ✓ (checked)

5. Optional: Create specific enemy spawn points:
   - Create empty GameObjects at positions like `(5, 3, 0)` and `(-4, -2, 0)`
   - Add them to the `Enemy Spawn Points` array

6. Link the Battle Preview UI:
   - Drag `BattlePreviewCanvas` to the `battlePreviewUI` field

### Step 6: Create EnemyEncounterData Singleton

1. Create an empty GameObject: `GameObject > Create Empty`
2. Name it `EnemyEncounterData`
3. Attach the **EnemyEncounterData** script
4. This object will persist between scenes automatically

### Step 7: Add the Scene to Build Settings

1. Go to `File > Build Settings`
2. Click `Add Open Scenes` to add WorldMapScene
3. Ensure the scene order is:
   - MainMenuScene
   - DrawingScene
   - PlantSelectionScene
   - **WorldMapScene** (NEW!)
   - DrawingBattleScene
   - PostBattleScene
   - InventoryScene

## Testing the Scene

### In-Editor Testing

1. Open the WorldMapScene
2. Press Play
3. Test the following:
   - **WASD Movement**: Player should move in all directions
   - **Enemy Proximity**: Walk near an enemy, you should see "Press E to Interact"
   - **Enemy Highlight**: Enemy should change color when in range
   - **Interaction**: Press 'E' near an enemy to open battle preview
   - **Battle Preview**: Should show enemy name, element, difficulty, and flavor text
   - **Cancel**: Click Cancel to close the popup
   - **Go to Battle**: Click to transition to PlantSelectionScene

### Full Flow Testing

1. Start from MainMenuScene
2. Play Game → DrawingScene (draw a plant)
3. Confirm → PlantSelectionScene (select a plant)
4. Confirm → **WorldMapScene** (NEW!)
5. Walk to an enemy and press 'E'
6. Click "Go to Battle"
7. PlantSelectionScene (select which plant to use)
8. Confirm → DrawingBattleScene
9. Win the battle
10. PostBattleScene (choose reward)
11. Should return to **WorldMapScene**

## Customization Options

### Enemy Spawn Configuration

In WorldMapSceneManager Inspector:
- **Number of Enemies**: Change how many enemies spawn
- **Spawn Enemies On Start**: Uncheck to manually place enemies in scene
- **Enemy Spawn Points**: Define exact positions for enemies

### Enemy Data Configuration

On each WorldMapEnemy:
- **Enemy Plant Type**: Select specific plant type
- **Difficulty**: Set 1-5 star difficulty
- **Flavor Text**: Write custom description
- **Interaction Range**: Adjust detection range
- **Highlight Color**: Change the hover color

### Player Movement

On PlayerController:
- **Move Speed**: Adjust player movement speed (default: 5)

## Camera Setup

For a top-down view:
1. Select Main Camera
2. Set Position: `(0, 0, -10)`
3. Set Rotation: `(0, 0, 0)`
4. Set Projection to Orthographic
5. Orthographic Size: `5` (adjust to fit your map)

Optional: Add **Cinemachine Virtual Camera** to follow the player:
1. Install Cinemachine package: `Window > Package Manager > Cinemachine > Install`
2. `GameObject > Cinemachine > 2D Camera`
3. Set Follow target to Player
4. Adjust Dead Zone and Soft Zone as needed

## Troubleshooting

### Player doesn't move
- Check that Rigidbody2D is attached
- Verify Gravity Scale is 0
- Check that PlayerController script is enabled

### Can't interact with enemy
- Verify Player has "Player" tag
- Check interaction range (default is 2.0)
- Ensure WorldMapSceneManager is in the scene

### Battle preview doesn't show
- Check that BattlePreviewUI script is attached to the canvas
- Verify all UI elements are linked in Inspector
- Check Console for errors

### Scene doesn't transition
- Verify scene names are correct: "PlantSelectionScene", "DrawingBattleScene", "WorldMapScene"
- Check that all scenes are added to Build Settings
- Look for errors in Console

### Enemy stats aren't scaling with difficulty
- Check that EnemyEncounterData singleton exists in scene
- Verify DrawingBattleSceneManager changes were saved
- Check Console logs for "World Map Encounter" message

## Next Steps

### Visual Polish
- Add sprites for player and enemies
- Add background tiles for the world map
- Add animations (walking, idle)
- Add particle effects for interactions

### Gameplay Enhancements
- Add more enemy spawn points
- Implement enemy respawn system
- Add items or collectibles on the map
- Add NPCs for dialogue
- Add different zones or areas

### UI Improvements
- Add minimap
- Add quest markers
- Add health bar for player
- Add inventory quick access

## File Structure

```
Assets/
├── Scripts/
│   ├── WorldMap/
│   │   ├── PlayerController.cs
│   │   ├── WorldMapEnemy.cs
│   │   ├── EnemyEncounterData.cs
│   │   ├── BattlePreviewUI.cs
│   │   ├── WorldMapSceneManager.cs
│   │   └── SETUP_GUIDE.md (this file)
│   ├── Battle/
│   │   └── DrawingBattleSceneManager.cs (modified)
│   ├── UI/
│   │   └── PlantSelectionSceneManager.cs (modified)
│   └── PostBattleManager.cs (modified)
├── Scenes/
│   └── WorldMapScene.unity (NEW!)
└── Prefabs/
    └── EnemyPrefab.prefab
```

## Modified Files

The following existing files were updated to support the world map:

1. **PlantSelectionSceneManager.cs**
   - Now transitions to WorldMapScene after plant selection
   - Or goes to BattleScene if it's a world map encounter

2. **PostBattleManager.cs**
   - Returns to WorldMapScene after battle rewards
   - Clears encounter data

3. **DrawingBattleSceneManager.cs**
   - Uses EnemyEncounterData for world map encounters
   - Scales enemy stats based on difficulty

## Support

If you encounter any issues, check:
1. Unity Console for error messages
2. All required scripts are attached
3. All scene references are linked
4. Scene names match exactly
5. All scenes are in Build Settings

Happy mapping! 🗺️
