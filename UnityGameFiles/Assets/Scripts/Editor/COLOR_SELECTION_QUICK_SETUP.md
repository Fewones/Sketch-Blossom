# Quick Setup Guide for Color Selection UI

## Automatic Setup (Recommended)

The easiest way to set up the color selection UI is to use the automated setup script.

### Step 1: Open Your Scene
Open your DrawingScene in Unity Editor (the scene must have a Canvas with a DrawingPanel).

### Step 2: Run the Setup Script
In Unity Editor, go to the menu:
```
Tools > Sketch Blossom > Setup Color Selection UI
```

That's it! The script will automatically:
- Create a ColorSelector GameObject under DrawingPanel
- Create three color buttons (Red, Green, Blue)
- Configure all button visuals and positions
- Add the DrawingColorSelector component
- Assign all references automatically
- Link to DrawingCanvas (if found)

### Step 3: Verify Setup
To make sure everything is configured correctly:
```
Tools > Sketch Blossom > Validate Color Selection Setup
```

This will check all components and references and show you a report.

## What Gets Created

After running the setup, your hierarchy will look like this:

```
Canvas
└── DrawingPanel
    ├── DrawingArea (existing)
    ├── StrokeCounter (existing)
    ├── FinishButton (existing)
    └── ColorSelector (NEW)
        ├── RedButton
        │   └── Label (TextMeshProUGUI: "Red\n(Sunflower)")
        ├── GreenButton
        │   └── Label (TextMeshProUGUI: "Green\n(Cactus)")
        └── BlueButton
            └── Label (TextMeshProUGUI: "Blue\n(Water Lily)")
```

## Button Layout

The buttons will be positioned in the top-left area of the DrawingPanel:
- **Red Button** (leftmost): Selects red color for Sunflower
- **Green Button** (middle): Selects green color for Cactus
- **Blue Button** (rightmost): Selects blue color for Water Lily

Each button is 60x60 pixels with a small label underneath.

## Customization After Setup

After the automatic setup, you can customize:

1. **Button Position**: Select ColorSelector in the hierarchy and adjust its RectTransform position
2. **Button Size**: Select individual buttons and change their sizeDelta
3. **Button Colors**: The Image components on each button can be adjusted
4. **Label Text**: Edit the TextMeshProUGUI components to change button labels
5. **Selected/Unselected Tints**: Adjust in the DrawingColorSelector component inspector

## Manual Adjustments

If you need to manually adjust button positions:
1. Select the button in the hierarchy
2. In the Inspector, find the RectTransform component
3. Adjust the "Anchored Position" values

Default positions:
- Red: X=0, Y=0
- Green: X=70, Y=0
- Blue: X=140, Y=0

## Removing the UI

If you want to start over or remove the color selection UI:
```
Tools > Sketch Blossom > Remove Color Selection UI
```

This will delete the entire ColorSelector GameObject and all its children.

## Troubleshooting

### "No Canvas found in scene"
- Create a Canvas first (GameObject > UI > Canvas)

### "DrawingPanel not found in Canvas"
- Make sure you have a GameObject named "DrawingPanel" under your Canvas
- This should already exist if you've set up the drawing scene

### "DrawingCanvas not assigned"
- This is usually okay - the component will auto-find DrawingCanvas at runtime
- If you want to assign it manually, find the DrawingCanvas GameObject in your scene and drag it to the DrawingColorSelector component

### Buttons don't respond
- Make sure the Canvas has an EventSystem (should be created automatically with Canvas)
- Check that the buttons have the Button component attached
- Verify that DrawingCanvas.isDrawingEnabled is set to true when you want drawing to work

### Colors don't show on strokes
- Make sure your LineRenderer prefab's material supports vertex colors
- Recommended materials: "Sprites/Default" or "Unlit/Color"
- The material should have the "Vertex Color" option enabled

## Testing the Setup

1. Enter Play Mode
2. Click "Start Drawing" (if you have the start button)
3. Click one of the color buttons (Red, Green, or Blue)
4. Draw some strokes - they should appear in the selected color
5. Switch colors mid-drawing to test color tracking
6. Click "Finish" and check the Console logs for:
   - Color usage statistics
   - Dominant color detection
   - Plant type analysis results

## Expected Console Output

When you finish drawing, you should see logs like:
```
Color Usage: RGBA(1.000, 0.000, 0.000, 1.000) - Count: 3, Length: 12.45
Dominant color by count: RGBA(1.000, 0.000, 0.000, 1.000) (3 strokes)
Color suggests: SUNFLOWER (Red majority)
Plant Detected: Sunflower (Fire) - Confidence: 85%
```

## Next Steps

After setup is complete:
1. Test in Play Mode to ensure buttons work
2. Adjust button positions if needed for your UI layout
3. Customize button colors or labels to match your game's art style
4. See COLOR_SELECTION_SETUP.md for detailed information on how the system works

## Advanced Options

The setup script creates a basic layout, but you can enhance it further:
- Add button icons/sprites instead of solid colors
- Add tooltips to buttons
- Create a color palette UI with more colors
- Add visual feedback when hovering over buttons
- Display current selected color indicator

All the scripts support these enhancements - just modify the button GameObjects after running the setup script.
