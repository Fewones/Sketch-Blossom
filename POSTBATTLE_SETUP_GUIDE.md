# PostBattleScene Setup Guide

This guide will help you set up the PostBattleScene UI in Unity Editor.

## Overview

The PostBattleScene now features:
- Two main options: **Wild Growth** (shows plant image) and **Tame** (adds unit to roster)
- A **Guide Button** at the bottom right
- A **Guidebook** with 2 pages explaining each option

## Step-by-Step Setup

### 1. Open the Scene

1. Open Unity Editor
2. Navigate to `Assets/Scenes/PostBattleScene.unity`
3. Double-click to open the scene

### 2. Create the Canvas

1. Right-click in Hierarchy → UI → Canvas
2. Name it "Canvas"
3. Set Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080
   - Match: 0.5

### 3. Create Background Panel

1. Right-click Canvas → UI → Panel
2. Name it "BackgroundPanel"
3. Set color to a nice background (e.g., soft green: R: 144, G: 238, B: 144)

### 4. Create Wild Growth Button

1. Right-click Canvas → UI → Button
2. Name it "WildGrowthButton"
3. Position:
   - Anchor: Middle Left
   - Pos X: 350, Pos Y: 0
   - Width: 400, Height: 500
4. Add an Image child:
   - Right-click WildGrowthButton → UI → Image
   - Name it "PlantImage"
   - Set to preserve aspect
   - Set color to white
5. Add a Text (TMP) child to the button:
   - Name it "WildGrowthLabel"
   - Text: "Wild Growth"
   - Font Size: 36
   - Alignment: Center Bottom
   - Pos Y: -200

### 5. Create Tame Button

1. Right-click Canvas → UI → Button
2. Name it "TameButton"
3. Position:
   - Anchor: Middle Right
   - Pos X: -350, Pos Y: 0
   - Width: 400, Height: 500
4. Add a Text (TMP) child:
   - Name it "TameDescriptionText"
   - Text: "Add a new unit to the roster"
   - Font Size: 28
   - Alignment: Center
   - Enable Word Wrapping

### 6. Create Guide Button (Bottom Right)

1. Right-click Canvas → UI → Button
2. Name it "GuideButton"
3. Position:
   - Anchor: Bottom Right
   - Pos X: -100, Pos Y: 80
   - Width: 150, Height: 60
4. Change button text to "Guide" (font size: 24)

### 7. Create Guidebook Panel

1. Right-click Canvas → UI → Panel
2. Name it "GuideBookPanel"
3. Set to fill the screen (Anchor: Stretch, all offsets to 0)
4. Set color to semi-transparent dark (R: 0, G: 0, B: 0, A: 200)

### 8. Create Guidebook Content

Inside GuideBookPanel:

#### 8.1 Content Background
1. Right-click GuideBookPanel → UI → Panel
2. Name it "ContentPanel"
3. Position: Center, Width: 800, Height: 600
4. Color: White or light beige

#### 8.2 Page Title
1. Right-click ContentPanel → UI → Text - TextMeshPro
2. Name it "GuidePageTitle"
3. Position: Top, Width: 700, Height: 80
4. Font Size: 48, Bold, Center alignment

#### 8.3 Page Content
1. Right-click ContentPanel → UI → Text - TextMeshPro
2. Name it "GuidePageContent"
3. Position: Center, Width: 700, Height: 400
4. Font Size: 24, Left alignment, Enable Word Wrapping

#### 8.4 Page Number
1. Right-click ContentPanel → UI → Text - TextMeshPro
2. Name it "PageNumberText"
3. Position: Bottom Center, Pos Y: -250
4. Font Size: 20, Center alignment
5. Text: "Page 1 / 2"

#### 8.5 Previous Button
1. Right-click ContentPanel → UI → Button
2. Name it "PrevPageButton"
3. Position: Bottom Left, Pos X: -250, Pos Y: -250
4. Width: 120, Height: 50
5. Button text: "< Previous"

#### 8.6 Next Button
1. Right-click ContentPanel → UI → Button
2. Name it "NextPageButton"
3. Position: Bottom Right, Pos X: 250, Pos Y: -250
4. Width: 120, Height: 50
5. Button text: "Next >"

#### 8.7 Close Button
1. Right-click ContentPanel → UI → Button
2. Name it "CloseGuideButton"
3. Position: Top Right, Pos X: 350, Pos Y: 250
4. Width: 80, Height: 60
5. Button text: "X"
6. Color: Light red

### 9. Wire Up the PostBattleManager

1. Select the "PostBattleManager" GameObject in the Hierarchy
2. In the Inspector, find the PostBattleManager script component
3. Drag and drop the UI elements to their corresponding fields:

   **Main UI:**
   - Wild Growth Button → WildGrowthButton
   - Tame Button → TameButton
   - Plant Image → PlantImage (the Image inside WildGrowthButton)
   - Tame Description Text → TameDescriptionText

   **Guide System:**
   - Guide Button → GuideButton
   - Guide Book Panel → GuideBookPanel
   - Guide Page Title → GuidePageTitle
   - Guide Page Content → GuidePageContent
   - Page Number Text → PageNumberText
   - Next Page Button → NextPageButton
   - Prev Page Button → PrevPageButton
   - Close Guide Button → CloseGuideButton

   **Scene Settings:**
   - Next Scene Name: "DrawingScene" (or your desired next scene)

### 10. Set Initial State

1. Select GuideBookPanel in the Hierarchy
2. In the Inspector, **uncheck the checkbox** next to the GameObject name to disable it
   - This ensures the guide is hidden when the scene starts

## Testing

### In Play Mode:

1. Click Play in Unity Editor
2. You should see:
   - Wild Growth button on the left (will show plant image from battle)
   - Tame button on the right with "Add a new unit to the roster" text
   - Guide button at the bottom right
3. Click the Guide button:
   - Guidebook should open
   - Page 1 explains Wild Growth
   - Click "Next >" to see Page 2 (explains Tame)
   - Click "< Previous" to go back
   - Click "X" or press Escape to close
4. Keyboard shortcuts:
   - Press "G" to open the guide
   - Press "Escape" to close the guide
   - Use Left/Right arrow keys to navigate pages

### With Battle Data:

To see the plant image:
1. Go through a battle in DrawingBattleScene
2. Win the battle
3. The PostBattleScene should load
4. The Wild Growth button should display your drawn plant

## Styling Tips

- Use matching colors for Wild Growth (green theme) and Tame (blue theme)
- Add nice sprites/images to the buttons for visual appeal
- Adjust font sizes and positions to fit your game's aesthetic
- Consider adding button hover effects (change colors on highlight)

## Troubleshooting

**Plant image not showing:**
- Make sure DrawnUnitData.Instance has valid drawingTexture data
- Check that the Image component has "Preserve Aspect" enabled

**Guide not opening:**
- Verify GuideBookPanel is assigned in the Inspector
- Check that the GuideButton has the onClick listener set up

**Buttons not responding:**
- Ensure there's an EventSystem in the scene
- Check that the Canvas has a GraphicRaycaster component

## Next Steps

After setting up the UI, you can:
- Customize the guidebook content in PostBattleManager.cs (lines 37-56)
- Adjust button positions and sizes to match your design
- Add animations or transitions for better UX
- Implement the actual Wild Growth and Tame mechanics in your game

---

## Quick Reference: Object Hierarchy

```
Canvas
├── BackgroundPanel
├── WildGrowthButton
│   ├── PlantImage
│   └── WildGrowthLabel (Text)
├── TameButton
│   └── TameDescriptionText (Text)
├── GuideButton
│   └── Text
└── GuideBookPanel (initially disabled)
    └── ContentPanel
        ├── GuidePageTitle (Text)
        ├── GuidePageContent (Text)
        ├── PageNumberText (Text)
        ├── PrevPageButton
        ├── NextPageButton
        └── CloseGuideButton
```

## Code Files Modified

- `UnityGameFiles/Assets/Scripts/PostBattleManager.cs` - Main scene controller
- `UnityGameFiles/Assets/Scenes/PostBattleScene.unity` - Scene file (cleaned)

The scene is ready to use once you complete the UI setup in Unity Editor!
