# Drawing Scene Architecture

## Overview
The Drawing Scene has been completely rebuilt with a clean, modular architecture that eliminates redundant systems and provides clear separation of concerns.

## Core Components

### 1. DrawingSceneManager
**Location**: `Scripts/Drawing/DrawingSceneManager.cs`

**Purpose**: Main game flow controller for the Drawing Scene

**Responsibilities**:
- Coordinates all systems (drawing, recognition, UI)
- Handles the drawing-to-results workflow
- Manages scene transitions to battle
- Processes player actions (finish, redraw, continue)

**Key Features**:
- Auto-discovers all required components
- Event-driven architecture
- Clean initialization and cleanup

### 2. DrawingSceneUIController
**Location**: `Scripts/UI/DrawingSceneUIController.cs`

**Purpose**: Controls ALL UI elements in the Drawing Scene

**Responsibilities**:
- Manages all UI panels (instructions, drawing, errors)
- Handles all button interactions
- Updates stroke counter and hints
- Displays error messages
- Provides visual feedback to player

**Key Features**:
- Real-time stroke counter updates
- Context-aware hints
- Auto-discovers UI elements
- Event-based communication with manager

### 3. SimpleDrawingCanvas
**Location**: `Scripts/Drawing/SimpleDrawingCanvas.cs`

**Purpose**: Handles drawing input and stroke rendering

**Responsibilities**:
- Captures mouse/touch input
- Creates and manages LineRenderer strokes
- Tracks drawing colors
- Provides stroke data for analysis

**Features**:
- Configurable max strokes (default: 20)
- Minimum point distance to prevent jitter
- Drawing area boundary detection
- Color-based drawing system

### 4. PlantRecognitionSystem
**Location**: `Scripts/Drawing/PlantRecognitionSystem.cs`

**Purpose**: Analyzes drawings and recognizes plant types

**Responsibilities**:
- Analyzes stroke patterns and colors
- Determines plant type and element
- Calculates confidence scores
- Provides plant stats (HP, ATK, DEF)

**Features**:
- 9 unique plant types (3 per element)
- Color-based element detection (Red=Fire, Green=Grass, Blue=Water)
- Comprehensive plant database

### 5. DrawnUnitData
**Location**: `Scripts/Drawing/DrawnUnitData.cs`

**Purpose**: Singleton that persists plant data between scenes

**Responsibilities**:
- Stores recognized plant information
- Maintains plant stats
- Survives scene transitions

**Features**:
- DontDestroyOnLoad singleton pattern
- Clean data interface

### 6. PlantResultPanel
**Location**: `Scripts/UI/PlantResultPanel.cs`

**Purpose**: Displays recognition results to the player

**Responsibilities**:
- Shows detected plant name and type
- Displays stats and confidence
- Provides Continue/Redraw options

### 7. GuideBookManager
**Location**: `Scripts/Battle/GuideBookManager.cs`

**Purpose**: Provides drawing guides and help

**Responsibilities**:
- Shows plant drawing examples
- Page navigation
- Visual reference for players

## System Flow

```
Player starts scene
    ↓
DrawingSceneManager initializes
    ↓
DrawingSceneUIController shows instructions
    ↓
Player clicks "Start Drawing"
    ↓
DrawingSceneUIController shows drawing panel
    ↓
Player draws with SimpleDrawingCanvas
    ↓
DrawingSceneUIController updates stroke counter
    ↓
Player clicks "Finish"
    ↓
DrawingSceneManager.HandleFinishDrawing()
    ├─→ Validates drawing (has strokes?)
    ├─→ Analyzes with PlantRecognitionSystem
    ├─→ Stores result in DrawnUnitData
    └─→ Shows PlantResultPanel
    ↓
Player chooses:
    ├─→ "Continue" → Load battle scene
    └─→ "Redraw" → Clear and restart
```

## Event System

The architecture uses C# events for clean communication:

### DrawingSceneUIController Events
- `OnFinishDrawing` - Raised when finish button clicked
- `OnRedrawRequested` - Raised when redraw button clicked
- `OnContinueRequested` - Raised when continue button clicked

### DrawingSceneManager Handlers
- `HandleFinishDrawing()` - Processes finish action
- `HandleRedraw()` - Clears and restarts
- `HandleContinue()` - Loads battle scene

## Setup Instructions

### Automatic Setup (Recommended)
1. Open Unity Editor
2. Go to **Tools > Sketch Blossom > Setup Drawing Scene (Complete)**
3. Click "Setup Complete Drawing Scene"
4. The system will auto-create and wire up all components

### Manual Setup
1. Create empty GameObject named "DrawingSceneManager"
2. Add `DrawingSceneManager` component
3. Create empty GameObject named "DrawingSceneUIController"
4. Add `DrawingSceneUIController` component
5. Ensure SimpleDrawingCanvas exists in scene
6. Run the editor tool to wire up references

## Removed Components

The following legacy components have been **removed**:
- ❌ `DrawingManager.cs` (replaced by DrawingSceneManager)
- ❌ `DrawingSceneUI.cs` (replaced by DrawingSceneUIController)
- ❌ `DrawingCanvas.cs` (replaced by SimpleDrawingCanvas)
- ❌ `PlantAnalyzer.cs` (replaced by PlantRecognitionSystem)

## Benefits of New Architecture

✅ **Clear Separation**: Each component has ONE job
✅ **Easy to Maintain**: Clean, documented code
✅ **Event-Driven**: Loosely coupled systems
✅ **Auto-Discovery**: Components find each other automatically
✅ **No Redundancy**: Single source of truth for each system
✅ **Editor Tools**: Automated setup and validation

## Troubleshooting

### "SimpleDrawingCanvas not found"
- Ensure the canvas exists in your scene
- Run the setup tool to auto-create it
- Check that it's not disabled

### "UI elements not found"
- Verify your Canvas has the required panels:
  - InstructionsPanel
  - DrawingOverlay
  - DrawingPanel
- Use the editor tool to auto-wire references

### "No strokes drawn" error
- Ensure drawing area is properly configured
- Check that DrawingPanel is visible and active
- Verify mouse input is working

## API Reference

### DrawingSceneManager Public Methods
- `ShowInstructions()` - Shows instruction panel
- `StartDrawing()` - Shows drawing panel

### DrawingSceneUIController Public Methods
- `ShowInstructionsPanel()` - Display instructions
- `ShowDrawingPanel()` - Display drawing UI
- `HideDrawingPanel()` - Hide drawing UI
- `UpdateHint(string)` - Update hint text
- `ShowError(string)` - Display error message

### SimpleDrawingCanvas Public Methods
- `ClearAll()` - Clear all strokes
- `ForceEndStroke()` - End current stroke
- `GetDominantColor()` - Get most-used color
- `GetStrokeCount()` - Get number of strokes
- `SetColor(Color)` - Change drawing color

## Future Enhancements

Possible improvements:
- Undo/Redo functionality
- Stroke smoothing
- More plant types
- Difficulty settings
- Drawing tutorials
- Save/load drawings

---

**Last Updated**: 2025-01-14
**Architecture Version**: 2.0
**Status**: Production Ready ✅
