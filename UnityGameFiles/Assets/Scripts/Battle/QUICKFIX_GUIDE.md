# Quick Fix Guide - Drawing Battle Scene

## Problem
Your DrawingBattleScene is built but:
- ❌ Drawing doesn't work
- ❌ Action text doesn't appear
- ❌ References in BattleManager are null

## Solution: 3 Easy Ways to Fix

---

## Method 1: Use the Editor Window (RECOMMENDED)

**Steps:**
1. In Unity, go to menu: `Tools → Sketch Blossom → Fix Drawing Battle Scene`
2. Click the **"Fix Scene"** button
3. Done! Check the output log to verify all connections

**Pros:**
- Nice UI with detailed output
- Shows exactly what was found/connected
- Visual feedback

---

## Method 2: Quick Menu Command

**Steps:**
1. In Unity, go to menu: `Tools → Sketch Blossom → Fix Drawing Battle Scene (Quick)`
2. That's it!

**Pros:**
- Fastest method (one click)
- No window to close
- Same results as Method 1

---

## Method 3: Right-Click the Builder

**Steps:**
1. In your scene, find the GameObject with `DrawingBattleSceneBuilder` component
2. Right-click the component in Inspector
3. Select **"Fix Existing Scene (No Rebuild)"**
4. Check Console for results

**Pros:**
- No need to use menus
- Works directly from scene
- Good if builder is already in scene

---

## What Gets Fixed

All of these methods automatically:

✅ **Finds BattleManager** in your scene
✅ **Locates all UI components** by name:
   - DrawingArea (BattleDrawingCanvas)
   - PlayerHPBar
   - EnemyHPBar
   - FinishDrawingButton
   - ClearDrawingButton
   - TurnIndicator (text)
   - ActionText (text)
   - AvailableMovesText (text)

✅ **Wires up all references** using reflection
✅ **Connects move detection systems** (MovesetDetector, MoveRecognitionSystem)
✅ **Marks scene as dirty** so you can save

---

## After Fixing

1. **Save your scene** (Ctrl+S / Cmd+S)
2. **Enter Play mode**
3. **Test:**
   - Can you draw in the white area? ✅
   - Does action text appear? ✅
   - Do buttons work? ✅

---

## Troubleshooting

### "BattleManager not found"
**Solution:** Make sure you have a GameObject with `DrawingBattleSceneManager` component in your scene.

### "GameObject 'DrawingArea' not found"
**Solution:** Your UI components need specific names. Make sure you have:
- DrawingArea (GameObject with BattleDrawingCanvas)
- ActionText (GameObject with TextMeshProUGUI)
- etc.

If names are different, rename them to match or rebuild the scene.

### Drawing still doesn't work
**Check:**
1. Is there an EventSystem in the scene? (GameObject → UI → Event System)
2. Is the DrawingArea Image set as Raycast Target? (Inspector → Image component → check "Raycast Target")
3. Is there a Main Camera?

### Action text still doesn't appear
**Check:**
1. Was ActionText found? (Check Console logs)
2. Is the text color set to white? (Inspector → TextMeshProUGUI → Color)
3. Is it positioned correctly? (Should be at Y anchor 0.38)

---

## When to Rebuild Instead

Only rebuild the entire scene if:
- Major UI changes needed
- GameObjects have wrong names
- Missing essential components
- Scene is too broken to fix

Otherwise, use the quick fix!

---

## Technical Details

### What the Fix Does

```
1. Finds DrawingBattleSceneManager in scene
2. Searches for UI components by GameObject name
3. Uses C# reflection to access private [SerializeField] fields
4. Assigns component references directly
5. Marks scene dirty so Unity saves changes
```

### How References are Set

The tools use reflection to set private fields:
```csharp
var field = type.GetField("drawingCanvas", BindingFlags.NonPublic | BindingFlags.Instance);
field.SetValue(battleManager, foundDrawingCanvas);
```

This is exactly what Unity Inspector does when you drag-and-drop, but automated!

---

## File Locations

**Editor Script:**
`/Assets/Scripts/Battle/Editor/DrawingBattleSceneFixer.cs`

**Builder Script:**
`/Assets/Scripts/Battle/DrawingBattleSceneBuilder.cs`

---

## Quick Reference

| Method | How to Use | Speed | UI |
|--------|-----------|-------|-----|
| Editor Window | Tools menu → Fix Drawing Battle Scene | Medium | ✅ Full |
| Quick Command | Tools menu → Fix... (Quick) | ⚡ Fast | Console only |
| Right-Click Builder | Right-click component → Fix Existing Scene | ⚡ Fast | Console only |

**All methods give the same result!** Choose whichever is most convenient.

---

## NEW: Fix White Line Color Issue

If your lines are invisible (white on white), use:

**Method 1:** `Tools → Sketch Blossom → Fix Drawing Canvas Colors`
**Method 2:** `Tools → Sketch Blossom → Fix Drawing Canvas Colors (Quick)`
**Method 3:** Select DrawingArea in Hierarchy → Inspector → Click **"Fix Colors (Make Lines BLACK)"** button

This automatically fixes:
- ✅ Drawing color: BLACK (visible on white background)
- ✅ Line width: 0.15 (good visibility)
- ✅ Material color: BLACK (not white!)

---

**Last Updated:** 2025-11-14
**Quick Fix for Drawing & Action Text + Line Color Issues**
