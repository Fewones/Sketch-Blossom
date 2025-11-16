# Menu System Setup Guide

This guide explains how to set up the enhanced menu system with settings panel and pause menu functionality in Unity.

## Overview

The new menu system includes:
1. **Exit Button** - Already implemented in MainMenuController
2. **Settings Panel** - Adjustable game settings (audio, display)
3. **Pause Menu** - ESC menu to return to main menu from any scene
4. **Confirmation Dialog** - Prevents accidental actions

## New Components Created

### 1. GameSettings Singleton
**Path:** `Assets/Scripts/Progression/GameSettings.cs`

- Manages game settings (audio, display)
- Persists settings using PlayerPrefs
- Automatically loads on game start

### 2. SettingsPanel
**Path:** `Assets/Scripts/UI/SettingsPanel.cs`

- UI panel for adjusting settings
- Can be used in main menu or pause menu
- Controls: Master/Music/SFX volume, Fullscreen, Resolution

### 3. PauseMenuManager
**Path:** `Assets/Scripts/UI/PauseMenuManager.cs`

- Pause menu accessible via ESC key
- Options: Resume, Settings, Main Menu, Quit
- Pauses game time when open

### 4. ConfirmationDialog
**Path:** `Assets/Scripts/UI/ConfirmationDialog.cs`

- Reusable confirmation dialog
- Prevents accidental "Return to Main Menu" clicks

## Setup Instructions

### Step 1: Create GameSettings GameObject (One-time setup)

1. In the **MainMenuScene**, create an empty GameObject named "GameSettings"
2. Add the `GameSettings` component to it
3. This GameObject will persist across all scenes automatically

### Step 2: Setup Main Menu Scene

#### A. Create Settings Panel UI

1. In **MainMenuScene**, create the following hierarchy:

```
Canvas
└── SettingsPanel (GameObject)
    ├── Overlay (Image) - Full screen, semi-transparent black
    └── Window (Panel)
        ├── Header
        │   └── Title (TextMeshProUGUI) - "Settings"
        ├── Content (Vertical Layout Group)
        │   ├── AudioSection
        │   │   ├── MasterVolumeControl
        │   │   │   ├── Label (TextMeshProUGUI) - "Master Volume"
        │   │   │   ├── Slider (Slider) - Min: 0, Max: 1
        │   │   │   └── ValueText (TextMeshProUGUI) - "100%"
        │   │   ├── MusicVolumeControl
        │   │   │   ├── Label (TextMeshProUGUI) - "Music Volume"
        │   │   │   ├── Slider (Slider) - Min: 0, Max: 1
        │   │   │   └── ValueText (TextMeshProUGUI) - "100%"
        │   │   └── SFXVolumeControl
        │   │       ├── Label (TextMeshProUGUI) - "SFX Volume"
        │   │       ├── Slider (Slider) - Min: 0, Max: 1
        │   │       └── ValueText (TextMeshProUGUI) - "100%"
        │   ├── DisplaySection
        │   │   ├── FullscreenControl
        │   │   │   ├── Label (TextMeshProUGUI) - "Fullscreen"
        │   │   │   └── Toggle (Toggle)
        │   │   └── ResolutionControl
        │   │       ├── Label (TextMeshProUGUI) - "Resolution"
        │   │       └── Dropdown (TMP_Dropdown)
        │   └── ButtonsSection
        │       ├── ResetButton (Button) - "Reset to Defaults"
        │       └── CloseButton (Button) - "Close"
        └── (Add SettingsPanel component here)
```

2. On the **SettingsPanel** GameObject, add the `SettingsPanel` component
3. Assign all the UI elements in the Inspector:
   - **Panel:** Overlay, Window
   - **Audio Controls:** All sliders and their value texts
   - **Display Controls:** Fullscreen toggle, Resolution dropdown
   - **Buttons:** Reset button, Close button

#### B. Update MainMenuController

1. Select the **MainMenuController** GameObject
2. In the Inspector, find the **Panels** section
3. Assign the **SettingsPanel** GameObject to the "Settings Panel" field

The settings button will now open the settings panel when clicked.

### Step 3: Setup Pause Menu (For Each Gameplay Scene)

Create a pause menu for each scene where you want the player to be able to return to the main menu (WorldMapScene, DrawingBattleScene, etc.)

#### A. Create Confirmation Dialog

1. In each scene, create this hierarchy:

```
Canvas
└── ConfirmationDialog (GameObject)
    ├── Overlay (Image) - Full screen, semi-transparent black
    └── Window (Panel)
        ├── Title (TextMeshProUGUI) - "Confirm"
        ├── Message (TextMeshProUGUI) - "Are you sure?"
        └── Buttons (Horizontal Layout Group)
            ├── ConfirmButton (Button)
            │   └── Text (TextMeshProUGUI) - "Confirm"
            └── CancelButton (Button)
                └── Text (TextMeshProUGUI) - "Cancel"
```

2. Add the `ConfirmationDialog` component to the ConfirmationDialog GameObject
3. Assign all UI elements in the Inspector

#### B. Create Settings Panel (in pause menu)

Follow the same structure as Step 2A, but you can reuse a prefab if you create one.

#### C. Create Pause Menu UI

1. Create this hierarchy:

```
Canvas
└── PauseMenu (GameObject)
    ├── Overlay (Image) - Full screen, semi-transparent black
    └── Window (Panel)
        ├── Title (TextMeshProUGUI) - "Paused"
        └── ButtonsContainer (Vertical Layout Group)
            ├── ResumeButton (Button) - "Resume"
            ├── SettingsButton (Button) - "Settings"
            ├── MainMenuButton (Button) - "Main Menu"
            └── QuitButton (Button) - "Quit Game"
```

2. Add the `PauseMenuManager` component to the PauseMenu GameObject
3. Assign all UI elements in the Inspector:
   - **Panels:** Overlay, Window, SettingsPanel, ConfirmationDialog
   - **Buttons:** Resume, Settings, Main Menu, Quit
   - **Settings:** Pause Key (default: Escape), Pause Time When Open (checked)

### Step 4: Create Prefabs (Recommended)

To make setup easier for multiple scenes:

1. Create prefabs for:
   - SettingsPanel
   - PauseMenu (including its SettingsPanel and ConfirmationDialog)
   - ConfirmationDialog

2. Drag these into scenes as needed

## Usage

### In-Game Controls

- **ESC Key** - Opens/closes pause menu
- **ESC in Settings** - Closes settings and returns to pause menu
- **Settings Button** (Main Menu) - Opens settings panel
- **Quit Button** (Main Menu) - Exits the game
- **Main Menu Button** (Pause Menu) - Shows confirmation, then returns to main menu

### For Developers

#### Access Settings from Code

```csharp
using SketchBlossom.Progression;

// Get current volume
float masterVol = GameSettings.Instance.MasterVolume;

// Set volume
GameSettings.Instance.MasterVolume = 0.5f;

// Listen to changes
GameSettings.Instance.OnMasterVolumeChanged += (volume) => {
    Debug.Log($"Master volume changed to {volume}");
};

// Get effective volumes (master * specific)
float effectiveMusicVol = GameSettings.Instance.GetEffectiveMusicVolume();
```

#### Show Confirmation Dialog

```csharp
using SketchBlossom.UI;

confirmationDialog.Show(
    title: "Delete Save?",
    message: "This action cannot be undone.",
    confirmText: "Delete",
    cancelText: "Cancel",
    onConfirmCallback: () => DeleteSave(),
    onCancelCallback: () => Debug.Log("Cancelled")
);
```

## Features

### GameSettings Features
- ✅ Persistent settings (saved to PlayerPrefs)
- ✅ Master, Music, and SFX volume controls
- ✅ Fullscreen toggle
- ✅ Resolution selection
- ✅ Reset to defaults
- ✅ Event system for reacting to changes

### Pause Menu Features
- ✅ ESC key to open/close
- ✅ Pauses game time (Time.timeScale = 0)
- ✅ Resume, Settings, Main Menu, Quit options
- ✅ Confirmation dialog for Main Menu
- ✅ Nested settings panel
- ✅ Automatic cleanup on scene change

### Settings Panel Features
- ✅ Volume sliders with percentage display
- ✅ Real-time settings application
- ✅ Resolution dropdown auto-populated
- ✅ Can be used in main menu or pause menu
- ✅ Close button to hide

## Scenes to Update

Add the PauseMenu to these scenes:
- ✅ WorldMapScene
- ✅ DrawingBattleScene
- ✅ DrawingScene
- ✅ PlantSelectionScene
- ✅ InventoryScene
- ✅ PostBattleScene
- ✅ TameScene
- ✅ WildGrowthScene

## Notes

- The **quit button** already exists in MainMenuController and is fully functional
- The **GameSettings** singleton persists across all scenes
- All settings are automatically saved to PlayerPrefs
- Time scale is reset to 1.0 when returning to main menu or quitting
- The pause menu can be customized per scene (e.g., hide quit button in some scenes)

## Troubleshooting

**Settings not saving?**
- Ensure GameSettings GameObject exists in the first loaded scene
- Check that PlayerPrefs is working (not disabled in build settings)

**Pause menu not appearing?**
- Check that the PauseMenuManager component is added
- Verify that the overlay and window references are assigned
- Make sure the Canvas is enabled

**ESC key not working?**
- Verify that PauseMenuManager is enabled
- Check that the Pause Key is set to Escape in Inspector
- Ensure no other script is consuming the ESC input

**Settings panel not showing?**
- Check that settingsPanel reference is assigned in MainMenuController
- Verify that the SettingsPanel component has all UI references assigned
- Check console for warnings

## Example Scene Setup Time

- Main Menu Scene: ~5 minutes (one-time setup)
- Gameplay Scene: ~3 minutes (using prefabs)

Once you create prefabs, adding the pause menu to new scenes takes less than 1 minute!
