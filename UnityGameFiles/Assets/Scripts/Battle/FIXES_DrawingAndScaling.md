# Drawing Battle Scene - Fixes for Drawing and Scaling Issues

## Issues Fixed

### 1. ✅ Screen Scaling Not Working
**Problem:** UI elements don't scale properly with different screen sizes.

**Root Cause:** CanvasScaler was created but not configured.

**Fix Applied:**
- Set `uiScaleMode` to `ScaleWithScreenSize`
- Set `referenceResolution` to 1920x1080
- Set `screenMatchMode` to `MatchWidthOrHeight` (0.5 balance)

**Location:** `DrawingBattleSceneBuilder.cs:59-95`

### 2. ✅ Drawing Not Working
**Problem:** Can't draw on the canvas - no strokes appear.

**Root Causes:**
1. Drawing area Image not set as raycast target (clicks weren't detected)
2. BattleDrawingCanvas looking for Canvas on wrong GameObject
3. No default line material (lines invisible)
4. Line width too large for coordinate system

**Fixes Applied:**

#### A. Enable Raycasting on Drawing Area
```csharp
// DrawingBattleSceneBuilder.cs:285-288
Image bg = drawingArea.AddComponent<Image>();
bg.color = new Color(0.95f, 0.95f, 0.95f);
bg.raycastTarget = true; // ✅ NOW ENABLED
```

#### B. Fix Canvas Reference
```csharp
// BattleDrawingCanvas.cs:47-48
// BEFORE: canvas = GetComponent<Canvas>(); ❌
// AFTER:  canvas = GetComponentInParent<Canvas>(); ✅
```

#### C. Create Default Line Material
```csharp
// BattleDrawingCanvas.cs:59-65
if (lineMaterial == null)
{
    lineMaterial = new Material(Shader.Find("Sprites/Default"));
    lineMaterial.color = Color.white;
}
```

#### D. Adjust Line Width and Color
```csharp
// BattleDrawingCanvas.cs:16-17
// BEFORE: lineWidth = 5f, drawingColor = Color.white
// AFTER:  lineWidth = 0.1f, drawingColor = Color.black ✅
```

#### E. Improve LineRenderer Configuration
```csharp
// BattleDrawingCanvas.cs:213-219
currentLine.alignment = LineAlignment.TransformZ;
currentLine.textureMode = LineTextureMode.Tile;
currentLine.sortingLayerName = "Default";
currentLine.sortingOrder = 100; // Render on top
```

### 3. ✅ Border Not Showing
**Problem:** Border outline not visible.

**Root Cause:** Outline component needs an Image to attach to.

**Fix Applied:**
```csharp
// DrawingBattleSceneBuilder.cs:303-305
Image borderImage = border.AddComponent<Image>();
borderImage.color = new Color(1, 1, 1, 0); // Transparent
borderImage.raycastTarget = false; // Don't block drawing
```

## How to Apply Fixes

### If You Already Built the Scene:

**Option 1: Rebuild the Scene (Easiest)**
1. Delete the DrawingBattleScene
2. Create a new scene
3. Use the updated DrawingBattleSceneBuilder
4. Toggle "Build Scene" checkbox

**Option 2: Manual Fix (For Existing Scene)**

1. **Fix Canvas Scaler:**
   - Select `BattleCanvas`
   - In CanvasScaler component:
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 x 1080`
     - Screen Match Mode: `Match Width Or Height`
     - Match: `0.5`

2. **Fix Drawing Area:**
   - Select `DrawingArea` GameObject
   - In Image component:
     - Check ✅ `Raycast Target`
   - In Border child GameObject:
     - Add `Image` component
     - Set color alpha to 0 (transparent)
     - Uncheck `Raycast Target`

3. **Fix BattleDrawingCanvas Settings:**
   - Select `DrawingArea`
   - In BattleDrawingCanvas component:
     - Line Width: `0.1`
     - Drawing Color: Black (or your preference)

4. **The code fixes are automatic** - Just update your scripts from git

## Testing Your Scene

### Step 1: Verify Canvas Scaling
1. Enter Play mode
2. Change Game view resolution
3. UI should scale proportionally

### Step 2: Verify Drawing Works
1. Enter Play mode
2. Wait for "YOUR TURN"
3. Click and drag in the white drawing area
4. You should see black lines appearing

### Step 3: Verify Full Battle Flow
1. Draw a shape (circle, line, etc.)
2. Click "Finish Drawing"
3. Move should be detected
4. Enemy should attack
5. Battle should continue

## Common Issues After Fix

### Issue: Lines still not appearing
**Solutions:**
1. Check Console for errors
2. Verify EventSystem exists (GameObject → UI → Event System)
3. Check that Main Camera exists
4. Ensure drawing area Image has `Raycast Target` checked
5. Try changing Line Width in Inspector (0.05 - 0.2 range)

### Issue: Lines appear but are very faint
**Solutions:**
1. Change Drawing Color to Black
2. Increase Line Width slightly (try 0.15)
3. Check that sorting order is high (100+)

### Issue: Can't click Finish Drawing button
**Solutions:**
1. Verify EventSystem exists in scene
2. Check Canvas has GraphicRaycaster component
3. Ensure button is not behind other UI elements

### Issue: Move not recognized
**Solutions:**
1. Check that MovesetDetector is attached to BattleManager
2. Verify MoveRecognitionSystem is attached
3. Try drawing more distinct shapes:
   - Block: Circle
   - Fireball: Circle
   - Vine Whip: Curved line
   - Water Splash: Wavy vertical line

## Technical Details

### Why Line Width Changed
LineRenderer uses world/local space units, not pixels. The original value of 5.0 was too large for the local coordinate system of the canvas. 0.1 works well for typical canvas sizes.

### Why Color Changed to Black
The drawing area has a light gray background (0.95, 0.95, 0.95). White lines on light background are barely visible. Black provides maximum contrast.

### Why Raycast Target Matters
Unity's UI system uses raycasting to detect mouse/touch input. If the Image doesn't have `Raycast Target` enabled, clicks pass through it to whatever is behind (or nothing), so the drawing system never receives the input events.

### Why Canvas Reference Was Wrong
The BattleDrawingCanvas component is attached to the DrawingArea GameObject, which is a child of the main Canvas. Calling `GetComponent<Canvas>()` looks for a Canvas on the same GameObject (DrawingArea), which doesn't have one. `GetComponentInParent<Canvas>()` correctly searches up the hierarchy.

## Files Modified

1. **DrawingBattleSceneBuilder.cs**
   - Fixed CanvasScaler configuration
   - Fixed drawing area raycasting
   - Fixed border Image component

2. **BattleDrawingCanvas.cs**
   - Fixed Canvas reference finding
   - Added default material creation
   - Adjusted line width and color
   - Improved LineRenderer setup

## Performance Notes

These fixes do not impact performance. The changes are:
- Configuration improvements (one-time setup)
- Material creation (one instance, reused)
- Better default values

## Next Steps

After applying these fixes, you should be able to:
✅ Draw moves during your turn
✅ See your drawings clearly
✅ Have UI scale properly on any screen size
✅ Complete full battle sequences

If you still have issues, check the main README_DrawingBattleScene.md for additional troubleshooting.

---

**Last Updated:** 2025-11-14
**Fixes:** Drawing input + Screen scaling
