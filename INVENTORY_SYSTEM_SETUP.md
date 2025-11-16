# Plant Inventory System - Setup Guide

## Overview

The Plant Inventory System has been implemented for Sketch Blossom! This system allows players to:
- **Collect plants** by drawing them and defeating enemies
- **View their collection** in an Inventory Scene
- **Select plants** for battle from their roster
- **Upgrade plants** using Wild Growth
- **Tame defeated enemies** to add them to the collection

## Architecture

### Core Components

1. **PlantInventoryEntry** (`Scripts/Progression/PlantInventoryEntry.cs`)
   - Represents a single plant with all its data (stats, level, drawing texture)
   - Handles Wild Growth upgrades
   - Serializable for saving/loading

2. **PlayerInventory** (`Scripts/Progression/PlayerInventory.cs`)
   - Singleton manager for all plants
   - Handles adding, removing, selecting plants
   - Persists data using PlayerPrefs + JSON

3. **EnemyUnitData** (`Scripts/Battle/EnemyUnitData.cs`)
   - Singleton that stores defeated enemy data
   - Used when player chooses "Tame" option

### Scene Managers

4. **InventorySceneManager** (`Scripts/UI/InventorySceneManager.cs`)
   - Displays all plants in a grid
   - Shows detailed plant information on click

5. **PlantSelectionSceneManager** (`Scripts/UI/PlantSelectionSceneManager.cs`)
   - Lets players choose which plant to use in battle
   - Shows plant stats and health

6. **PlantCardUI** (`Scripts/UI/PlantCardUI.cs`)
   - Component for individual plant cards
   - Displays plant name, level, stats, health bar

## Game Flow

```
DrawingScene → Battle → PostBattle → PlantSelection → Battle → PostBattle → ...
     ↓            ↓          ↓              ↓
  Add to      Use from   Tame/Upgrade    Choose from
  Inventory   Inventory   Inventory       Inventory
```

### Detailed Flow

1. **Drawing First Plant**
   - Player draws in DrawingScene
   - Plant is recognized and added to inventory
   - Plant is selected as active plant
   - Proceeds to first battle

2. **Battle**
   - Uses selected plant from inventory
   - On victory: stores enemy data, records victory
   - Loads PostBattleScene

3. **Post-Battle Choices**
   - **Wild Growth**: Upgrades selected plant's stats (+ATK, +HP, +DEF)
   - **Tame**: Adds defeated enemy to inventory

4. **Plant Selection**
   - View all plants in roster
   - Select which plant to use in next battle
   - See health status and stats

## Unity Editor Setup Required

### Scene: InventoryScene

**Create this UI hierarchy:**

```
Canvas
├── InventorySceneManager (attach InventorySceneManager.cs)
├── PlantGrid (Scroll View)
│   └── Content (Grid Layout Group)
│       └── [PlantCards will spawn here]
├── DetailPanel (Panel)
│   ├── BackgroundImage
│   ├── PlantImage (RawImage)
│   ├── NameText (TextMeshProUGUI)
│   ├── StatsText (TextMeshProUGUI)
│   ├── InfoText (TextMeshProUGUI)
│   └── CloseButton
├── EmptyStatePanel (Panel)
│   ├── EmptyStateText (TextMeshProUGUI)
│   └── DrawNewPlantButton
└── BackButton
```

**Inspector Setup:**
- Attach `InventorySceneManager` to a GameObject
- Assign all UI references in the Inspector
- Set `plantCardPrefab` (see below)

### Prefab: PlantCard

**Create a prefab with this structure:**

```
PlantCard (Panel + PlantCardUI.cs + Button)
├── BackgroundImage (Image)
├── PlantImage (RawImage)
├── NameText (TextMeshProUGUI)
├── LevelText (TextMeshProUGUI)
├── StatsText (TextMeshProUGUI)
├── HealthBar (Slider)
│   ├── Background
│   ├── Fill Area
│   └── Fill (colored based on health)
├── HealthText (TextMeshProUGUI)
├── ElementIcon (Image)
└── SelectedIndicator (Image/Panel - hidden by default)
```

**Component Setup:**
- Add `PlantCardUI.cs` component
- Add `Button` component
- Assign all UI references in PlantCardUI inspector

### Scene: PlantSelectionScene

**Create similar to InventoryScene with:**

```
Canvas
├── PlantSelectionSceneManager (attach PlantSelectionSceneManager.cs)
├── TitleText ("Choose Your Plant")
├── PlantGrid (Scroll View)
│   └── Content (Grid Layout Group)
│       └── [PlantCards spawn here]
├── SelectedPlantPanel (Panel)
│   ├── SelectedPlantImage (RawImage)
│   ├── SelectedPlantNameText
│   ├── SelectedPlantStatsText
│   └── SelectionInfoText
├── ConfirmButton ("Enter Battle")
├── BackButton
├── EmptyStatePanel (Panel)
│   ├── EmptyStateText
│   └── DrawNewPlantButton
```

**Inspector Setup:**
- Assign all references
- Use same PlantCard prefab as InventoryScene

### Existing Scene Updates

#### PostBattleScene
- **Already working** - No changes needed in Unity Editor
- The script now automatically saves plants and applies upgrades

#### DrawingBattleScene
- **Already working** - Now loads PostBattleScene on victory
- Enemy data is automatically stored

#### DrawingScene
- **Already working** - Now adds plants to inventory automatically

## Data Persistence

Plants are saved to `PlayerPrefs` using JSON serialization:
- **Key**: `"PlayerInventory_v1"`
- **Format**: JSON with all plant entries
- **Includes**: Stats, textures (base64), levels, metadata

To clear inventory (for testing):
```csharp
PlayerInventory.Instance.ClearInventory();
```

## Testing the System

### Manual Test Flow

1. **Test First Plant**
   ```
   1. Start game → DrawingScene
   2. Draw a plant
   3. Confirm → Should be added to inventory
   4. Check console: "Added [PlantName] to inventory! Total plants: 1"
   ```

2. **Test Battle & Tame**
   ```
   1. Win battle
   2. PostBattleScene appears
   3. Choose "Tame"
   4. Enemy plant should be added to inventory
   5. Check console: "Tamed [EnemyName] and added to inventory!"
   ```

3. **Test Wild Growth**
   ```
   1. Win battle
   2. Choose "Wild Growth"
   3. Selected plant stats should increase
   4. Check console: "Wild Growth applied to [PlantName]! Now level X"
   ```

4. **Test Plant Selection**
   ```
   1. After PostBattle → PlantSelectionScene loads
   2. See all plants in grid
   3. Click a plant to select
   4. Click "Confirm" → Loads battle with that plant
   ```

5. **Test Inventory View**
   ```
   1. From MainMenu → Inventory button
   2. See all collected plants
   3. Click a plant → Detail panel opens
   4. View stats, moves, image, metadata
   ```

### Debug Commands

Add these to your MainMenu or a debug panel:

```csharp
// View inventory count
Debug.Log($"Plants in inventory: {PlayerInventory.Instance.GetPlantCount()}");

// List all plants
foreach (var plant in PlayerInventory.Instance.GetAllPlants())
{
    Debug.Log($"{plant.plantName} - Lv.{plant.level} - HP:{plant.maxHealth}");
}

// Clear inventory
PlayerInventory.Instance.ClearInventory();
```

## Main Menu Integration

Add buttons to your MainMenu:

```csharp
public void OnInventoryButtonClicked()
{
    SceneManager.LoadScene("InventoryScene");
}

public void OnStartBattleClicked()
{
    // Check if player has plants
    if (PlayerInventory.Instance.GetPlantCount() > 0)
    {
        // Go to plant selection
        SceneManager.LoadScene("PlantSelectionScene");
    }
    else
    {
        // Go to drawing scene for first plant
        SceneManager.LoadScene("DrawingScene");
    }
}
```

## Wild Growth Stats

Plants increase stats based on element when upgraded:

| Element | HP  | ATK | DEF |
|---------|-----|-----|-----|
| Fire    | +3  | +5  | +2  |
| Grass   | +4  | +3  | +3  |
| Water   | +5  | +2  | +4  |

## Plant Metadata Tracked

Each plant stores:
- ✅ Name, Type, Element
- ✅ Current & Max HP
- ✅ Attack & Defense
- ✅ Level (increases with Wild Growth)
- ✅ Drawing Texture (base64)
- ✅ Plant Color
- ✅ Acquired Date
- ✅ Battles Won
- ✅ Wild Growth Count
- ✅ Original Detection Confidence

## Troubleshooting

### "PlayerInventory instance not found"
- Ensure PlayerInventory GameObject exists in scene OR
- The script will auto-create it as a singleton

### Plants not saving between sessions
- Check PlayerPrefs is saving: `PlayerPrefs.Save()`
- Check console for JSON serialization errors
- Texture data is stored as base64 - very large inventories may hit PlayerPrefs limits

### PlantCard not displaying correctly
- Verify all UI references are assigned in PlantCardUI inspector
- Check that TextMeshProUGUI is used (not legacy Text)
- Ensure RawImage is used for plant textures

### Enemy plant not being tamed
- Check console for "Stored enemy plant data" message after victory
- Verify EnemyUnitData singleton exists
- Check that PostBattleManager has the updated code

## Next Steps

1. **Create InventoryScene in Unity Editor**
   - Set up UI hierarchy as described above
   - Create PlantCard prefab
   - Assign all references

2. **Create PlantSelectionScene in Unity Editor**
   - Similar to InventoryScene
   - Add confirm button and selection panel

3. **Update MainMenu**
   - Add "Inventory" button
   - Update "Start Battle" to go to PlantSelectionScene if plants exist

4. **Test Complete Flow**
   - Draw → Battle → Tame → Select → Battle → Wild Growth

5. **Polish**
   - Add animations for plant cards
   - Add sound effects for taming/upgrading
   - Add visual feedback for selection
   - Consider adding plant rarity/special abilities

## Additional Features to Consider

- **Plant Healing**: Add a "Heal All Plants" button in inventory
- **Plant Release**: Allow removing plants from inventory
- **Sorting**: Sort plants by level, type, element, name
- **Filtering**: Filter by element type
- **Search**: Search plants by name
- **Favorites**: Mark favorite plants
- **Battle History**: Track more detailed battle statistics
- **Plant Evolution**: Special upgrades at certain levels
- **Trading**: (Multiplayer feature) Trade plants with friends

---

## Files Created/Modified

### New Files
- ✅ `Scripts/Progression/PlantInventoryEntry.cs`
- ✅ `Scripts/Progression/PlayerInventory.cs`
- ✅ `Scripts/Battle/EnemyUnitData.cs`
- ✅ `Scripts/UI/InventorySceneManager.cs`
- ✅ `Scripts/UI/PlantSelectionSceneManager.cs`
- ✅ `Scripts/UI/PlantCardUI.cs`

### Modified Files
- ✅ `Scripts/PostBattleManager.cs` - Added inventory integration
- ✅ `Scripts/Battle/DrawingBattleSceneManager.cs` - Victory handling & enemy storage
- ✅ `Scripts/Drawing/DrawingSceneManager.cs` - Add first plant to inventory

The inventory system is now **fully functional** from a code perspective!

You just need to set up the UI in Unity Editor following this guide.
