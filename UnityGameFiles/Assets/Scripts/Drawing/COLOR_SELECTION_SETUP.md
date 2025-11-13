# Color Selection System Setup Guide

This guide explains how to set up and use the new color selection system for plant type detection in Sketch Blossom.

## Overview

The color selection system allows players to choose between Red, Green, and Blue colors while drawing. The dominant color influences which plant type is detected:

- **Red (Majority)** → Sunflower (Fire Type)
- **Green (Majority)** → Cactus (Grass Type)
- **Blue (Majority)** → Water Lily (Water Type)

The final plant type is determined by combining:
- **Geometric analysis** (60%): Shape, aspect ratio, stroke patterns
- **Color analysis** (40%): Dominant color used in the drawing

## Move Sets by Plant Type

Each plant type has specific moves:

### Sunflower (Fire Type)
- Fireball (Power: 15)
- Flame Wave (Power: 20)
- Burn (Power: 25)

### Cactus (Grass Type)
- Vine Whip (Power: 15)
- Leaf Storm (Power: 20)
- Root Attack (Power: 25)

### Water Lily (Water Type)
- Water Splash (Power: 15)
- Bubble (Power: 20)
- Healing Wave (Power: 15, Healing Move)

## Unity UI Setup Instructions

### 1. Add Color Selection Buttons to DrawingPanel

In your DrawingScene, add three buttons to the DrawingPanel:

```
Canvas
└── DrawingPanel
    ├── DrawingArea (existing)
    ├── StrokeCounter (existing)
    ├── FinishButton (existing)
    └── ColorSelector (NEW)
        ├── RedButton
        ├── GreenButton
        └── BlueButton
```

### 2. Configure the Buttons

For each color button:
- **RedButton**:
  - Image Component: Set color to Red (255, 0, 0)
  - Add a TextMeshProUGUI child with text "Red" or "Sunflower"

- **GreenButton**:
  - Image Component: Set color to Green (0, 255, 0)
  - Add a TextMeshProUGUI child with text "Green" or "Cactus"

- **BlueButton**:
  - Image Component: Set color to Blue (0, 0, 255)
  - Add a TextMeshProUGUI child with text "Blue" or "Water Lily"

### 3. Add DrawingColorSelector Component

1. Create an empty GameObject under DrawingPanel called "ColorSelector"
2. Add the `DrawingColorSelector` component to it
3. Assign references:
   - Red Button → RedButton
   - Green Button → GreenButton
   - Blue Button → BlueButton
   - Red Button Image → RedButton's Image component
   - Green Button Image → GreenButton's Image component
   - Blue Button Image → BlueButton's Image component
   - Drawing Canvas → DrawingCanvas object in scene

The DrawingCanvas reference will auto-find if not assigned.

### 4. Optional: Add Tooltips/Labels

Add TextMeshProUGUI labels under each button explaining the plant type:
- "Red = Sunflower (Fire)"
- "Green = Cactus (Grass)"
- "Blue = Water Lily (Water)"

## How It Works

### DrawingCanvas.cs
- Tracks the currently selected color (`currentDrawingColor`)
- Applies the selected color to each stroke's LineRenderer
- Counts stroke usage per color
- Calculates total drawing length per color
- Provides methods: `SelectRedColor()`, `SelectGreenColor()`, `SelectBlueColor()`
- Calculates dominant color: `GetDominantColorByCount()` or `GetDominantColorByLength()`

### PlantAnalyzer.cs
- Receives the dominant color as a parameter
- Performs geometric analysis (shape, strokes, patterns)
- Applies color influence using `ApplyColorInfluence()`
- Returns final plant type with confidence score

### DrawingManager.cs
- Gets dominant color from DrawingCanvas
- Passes it to PlantAnalyzer along with strokes
- Stores result in DrawnUnitData

### DrawingColorSelector.cs
- Manages the color selection UI
- Calls DrawingCanvas color selection methods
- Updates button visuals to show selected color
- Dimmed buttons = not selected
- Bright button = currently selected

## Testing the System

1. Start the DrawingScene
2. Click "Start Drawing"
3. Select a color (Red/Green/Blue)
4. Draw your plant
5. You can change colors mid-drawing
6. Click "Finish" when done
7. Check the Console logs to see:
   - Color usage statistics
   - Geometric analysis scores
   - Color-adjusted scores
   - Final plant type detection

## Customization

### Adjust Color Influence Weight

In `PlantAnalyzer.cs`, modify the `ApplyColorInfluence()` method:

```csharp
float colorWeight = 0.4f; // Color contributes 40% to final decision
float colorBonus = 0.35f; // Bonus for matching color
```

- Increase `colorWeight` to make color more important
- Decrease `colorWeight` to rely more on geometric analysis
- Adjust `colorBonus` to change the strength of color matching

### Change Default Color

In `DrawingCanvas.cs`:

```csharp
public Color currentDrawingColor = Color.green; // Change to Color.red or Color.blue
```

### Custom Colors

In `DrawingCanvas.cs`, you can customize the exact RGB values:

```csharp
public Color redColor = new Color(1f, 0.2f, 0.2f); // Softer red
public Color greenColor = new Color(0.2f, 1f, 0.2f); // Bright green
public Color blueColor = new Color(0.2f, 0.5f, 1f); // Sky blue
```

## Architecture

```
Player draws with selected color
        ↓
DrawingCanvas tracks strokes & colors
        ↓
DrawingManager analyzes on "Finish"
        ↓
Geometric Analysis (60%) + Color Analysis (40%)
        ↓
PlantAnalyzer determines plant type
        ↓
DrawnUnitData stores plant type + stats
        ↓
MoveData.GetMovesForPlant() assigns move set
        ↓
Battle scene loads with correct moves
```

## Debugging

Enable detailed logging by checking the Console:

- `DrawingCanvas` logs: Color selection, stroke counts, color usage
- `PlantAnalyzer` logs: Base scores, color-adjusted scores, final detection
- `DrawingManager` logs: Dominant color, plant result

Look for these messages:
- "Selected [COLOR] color for drawing"
- "Color Usage: [color] - Count: X, Length: Y"
- "Dominant color by count: [color]"
- "Color suggests: [PLANT TYPE]"
- "Plant Detected: [TYPE] ([ELEMENT]) - Confidence: X%"

## Known Issues

- Make sure the LineRenderer prefab in DrawingCanvas supports color changes
- If colors don't appear, check that the Material on the LineRenderer uses a shader that supports vertex colors (e.g., "Sprites/Default" or "Unlit/Color")

## Future Enhancements

Possible improvements:
- Color mixing (purple = red+blue, etc.)
- Gradient support for multi-color strokes
- Color intensity affecting stats
- Visual preview of selected color
- Color-based elemental strength modifiers
