# Integration TODO - Battle Move Enhancements

This document outlines the steps needed to integrate the enhanced battle move system into your Unity project.

## Overview of Changes

Three files were modified/created to enhance the battle system:
- ✅ `MoveData.cs` - Enhanced with colors, visual effects, and animation properties
- ✅ `MoveExecutor.cs` - Updated to use unique move colors and screen shake
- ✅ `MoveGuideBook.cs` - **NEW** Interactive guide book for all moves

## Integration Steps

### 1. MoveExecutor Setup (Required)

The `MoveExecutor.cs` now has a new required field that must be assigned in Unity:

**In Unity Editor:**
1. Open the **DrawingBattleScene**
2. Find the GameObject with the `MoveExecutor` component
3. In the Inspector, locate the new **"Screen Shake Settings"** section
4. **Assign the Main Camera** to the `mainCamera` field
   - Drag your battle scene camera into this field
   - Or use the picker (circle icon) to select it
5. Optionally adjust `screenShakeMultiplier` (default: 1.0)
   - Higher = more intense shake
   - Lower = more subtle shake

**⚠️ IMPORTANT:** Without the camera assigned, screen shake won't work (but moves will still execute normally).

---

### 2. MoveGuideBook Setup (New Feature - Optional but Recommended)

This is a completely new interactive guide book that shows all 27 moves.

#### 2a. Create the UI in Unity

**Create the Guide Book Panel:**

1. In your **DrawingBattleScene**, create a UI structure:
   ```
   Canvas (or existing battle canvas)
   └── MoveGuidePanel (GameObject with Image component)
       ├── BackgroundPanel (Image - for colored backgrounds)
       ├── ContentArea (Vertical Layout Group)
       │   ├── PageTitle (TextMeshProUGUI - large, bold)
       │   ├── MoveColorDisplay (Image - shows gradient colors)
       │   └── PageDescription (TextMeshProUGUI - scrollable text)
       ├── NavigationButtons
       │   ├── PreviousPageButton (Button)
       │   └── NextPageButton (Button)
       ├── PageIndicator (TextMeshProUGUI - "Page 1 / 11")
       └── CloseButton (Button - X in corner)

   Canvas (battle UI)
   └── MoveGuideBookButton (Button - visible during battle)
   ```

#### 2b. Recommended Layout Settings

**MoveGuidePanel:**
- Anchor: Center
- Width: 800, Height: 600
- Background color: Light gray (will change per page)

**PageTitle:**
- Font Size: 36
- Alignment: Center
- Color: Black
- Top margin: 20

**MoveColorDisplay:**
- Width: 100, Height: 150
- Will display gradient of move colors
- Place on left or right side

**PageDescription:**
- Font Size: 20
- Alignment: Top Left
- Enable Rich Text: ✅ (for colored text)
- Recommended: Add Content Size Fitter

**NavigationButtons:**
- Place at bottom
- PreviousButton: "← Previous"
- NextButton: "Next →"

#### 2c. Attach the Script

1. Create an empty GameObject in your battle scene named **"MoveGuideBookManager"**
2. Add the `MoveGuideBook` component to it
3. In the Inspector, assign all the UI references:
   - `bookPanel` → MoveGuidePanel GameObject
   - `openBookButton` → MoveGuideBookButton
   - `closeBookButton` → CloseButton inside panel
   - `nextPageButton` → NextPageButton
   - `previousPageButton` → PreviousPageButton
   - `pageTitle` → PageTitle TextMeshProUGUI
   - `pageDescription` → PageDescription TextMeshProUGUI
   - `moveColorDisplay` → MoveColorDisplay Image
   - `backgroundPanel` → BackgroundPanel Image (optional)
   - `pageNumberText` → PageIndicator TextMeshProUGUI

4. Set `useSlideAnimation` to `true` for smooth open/close animation

#### 2d. Keyboard Shortcut

Players can press **M** to toggle the guide book during battle!

---

### 3. Testing the Changes

#### Test Move Colors

1. Enter a battle
2. Draw any move
3. Verify the move name appears in a **unique color** (not just generic fire/grass/water)
4. Watch for the **gradient flash** on the target (primary → secondary color)

#### Test Screen Shake

1. Draw a powerful move (like "Inferno Wave" - highest shake: 0.9)
2. Camera should shake when the move hits
3. Draw a gentle move (like "Block" - low shake: 0.1)
4. Camera should barely shake

#### Test Move Guide Book

1. Click the "Move Guide Book" button (or press M)
2. Guide should slide in from the right
3. Navigate through all 11 pages:
   - Page 1: Welcome
   - Pages 2-10: Each plant's moves (9 plants)
   - Page 11: Master tips
4. Verify colors display in the gradient box
5. Verify background changes color per element type
6. Close the guide (button or ESC key)

---

### 4. Expected Behavior

#### All 27 Moves Now Feature:

**Unique Visual Identity:**
- Each move has its own primary and secondary color
- Fire moves: Orange/red/yellow gradients
- Grass moves: Green/brown earth tones
- Water moves: Blue/cyan/turquoise tones

**Animation Differences:**
- Block moves: Subtle (0.8x intensity)
- Normal attacks: Standard (1.0x intensity)
- Ultimate moves: Dramatic (1.5-1.8x intensity)

**Screen Shake:**
- Defensive moves: Almost none (0.1)
- Light attacks: Mild (0.3-0.5)
- Heavy attacks: Strong (0.6-0.9)

#### Move Examples:

| Move | Primary Color | Secondary Color | Shake | Intensity |
|------|---------------|-----------------|-------|-----------|
| Fireball | Bright Orange | Yellow | 0.4 | 1.0 |
| Solar Flare | Deep Orange-Red | Bright Yellow | 0.7 | 1.5 |
| Vine Lash | Fresh Green | Dark Green | 0.5 | 1.1 |
| Strangling Roots | Forest Green | Brown | 0.7 | 1.3 |
| Bubble Barrage | Medium Blue | Pale Blue | 0.5 | 1.2 |
| Inferno Wave | Deep Flame | Bright Fire | **0.9** | **1.8** |

---

### 5. Troubleshooting

#### Screen Shake Not Working
- ✅ Verify `mainCamera` is assigned in MoveExecutor Inspector
- ✅ Check that the camera's Z position doesn't get locked by another script
- ✅ Ensure `screenShakeMultiplier` > 0

#### Move Guide Book Won't Open
- ✅ Verify all UI references are assigned
- ✅ Check that `MoveGuidePanel` is not disabled in hierarchy
- ✅ Look for errors in Console related to MoveGuideBook
- ✅ Ensure TextMeshPro is imported (Component → UI → TextMeshPro)

#### Moves Showing Wrong Colors
- ✅ The old `GetElementColor()` method is now a fallback
- ✅ Verify MoveData.cs changes compiled without errors
- ✅ Check Console for any move initialization errors

#### Missing Drawing Hints in Guide
- ✅ All moves should show hints automatically
- ✅ If blank, check that MoveData constructor is passing hints correctly

---

### 6. Optional Enhancements

#### Add Sound Effects
Each move now has a `VisualEffect` enum that you can use to trigger sounds:
```csharp
// In MoveExecutor, add:
private void PlayMoveSound(MoveData.VisualEffect effect)
{
    switch (effect)
    {
        case MoveData.VisualEffect.Flames:
            audioSource.PlayOneShot(fireSound);
            break;
        case MoveData.VisualEffect.Bubbles:
            audioSource.PlayOneShot(bubbleSound);
            break;
        // ... etc
    }
}
```

#### Add Particle Systems
Use the `visualEffect` property to spawn different particle systems:
```csharp
// Example in MoveExecutor:
if (move.visualEffect == MoveData.VisualEffect.Flames)
{
    Instantiate(fireParticles, target.transform.position, Quaternion.identity);
}
```

#### Custom Animations Per Move
Use `animationIntensity` to scale particle emission rates or sizes.

---

### 7. Summary Checklist

Before marking this as complete, verify:

- [ ] MoveExecutor has main camera assigned
- [ ] Screen shake works on powerful moves
- [ ] All moves display unique colors (not generic element colors)
- [ ] Move guide book UI is created (if implementing this feature)
- [ ] Move guide book opens/closes smoothly
- [ ] All 11 pages display correctly with proper colors
- [ ] Drawing hints appear for each move
- [ ] Gradient colors display in the color box
- [ ] Tested at least 3 different moves in battle
- [ ] No console errors related to moves

---

## Files Modified

### UnityGameFiles/Assets/Scripts/Combat/MoveData.cs
- Added: `primaryColor`, `secondaryColor`, `visualEffect`, `animationIntensity`, `screenShakeAmount`, `drawingHint`
- Updated: All 27 move definitions with unique colors and properties
- Enhanced: Constructor to accept all new visual parameters

### UnityGameFiles/Assets/Scripts/Combat/MoveExecutor.cs
- Added: `mainCamera` field (⚠️ **MUST ASSIGN IN UNITY**)
- Added: `screenShakeMultiplier` for global shake control
- Added: `FlashEffectWithGradient()` for two-color flash
- Added: `ScreenShake()` coroutine for camera shake
- Modified: Uses `move.primaryColor` instead of generic element colors
- Modified: Animation speed scales with `move.animationIntensity`

### UnityGameFiles/Assets/Scripts/Battle/MoveGuideBook.cs ⭐ NEW
- Complete interactive guide book system
- 11 pages covering all moves
- Color gradient display
- Keyboard shortcuts (M to toggle)
- Automatic color-coded backgrounds per element

---

## Quick Start

**Minimum to get it working:**
1. Assign camera to MoveExecutor
2. Test a battle - moves should have unique colors

**Full experience:**
1. Assign camera to MoveExecutor
2. Create Move Guide Book UI
3. Attach MoveGuideBook script and wire references
4. Test guide book opens and displays all moves

---

## Questions?

- All move definitions are in `MoveData.GetMovesForPlant()`
- Color values are defined as `new Color(r, g, b)` - adjust if desired
- Screen shake can be disabled by setting `screenShakeMultiplier = 0`
- Guide book can be skipped if you don't want that feature

Good luck with integration! 🎮✨
