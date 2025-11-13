# Current State & What Needs To Be Done

## ✅ What's Already Implemented (Commit: 5860162)

### 1. Battle Move Detection System ⚔️
**Location:** `UnityGameFiles/Assets/Scripts/Combat/`

**Files:**
- ✅ `MoveData.cs` - 9 battle moves (3 per plant type)
- ✅ `MovesetDetector.cs` - Shape recognition for attack moves
- ✅ `MoveExecutor.cs` - Execute moves with type advantages
- ✅ `CombatManager.cs` - Updated with move detection integration

**Works:**
- Sunflower (Fire): Fireball, Flame Wave, Burn
- Cactus (Grass): Vine Whip, Leaf Storm, Root Attack
- Water Lily (Water): Water Splash, Bubble, Healing Wave
- Type advantages: Water > Fire > Grass > Water (1.5x damage)

---

### 2. Plant Guide Book System 📖
**Location:** `UnityGameFiles/Assets/Scripts/UI/`

**Files:**
- ✅ `PlantGuideBook.cs` - 5-page interactive guide with animations
- ✅ `DrawingSceneUI.cs` - Enhanced UI manager
- ✅ `GuideBookButtonSetup.cs` - Button styling component

**Works:**
- 5 pages of plant drawing instructions
- Keyboard shortcuts (H to open, ESC to close, arrows to navigate)
- Slide-in/out animation
- Color-coded pages

---

## ❌ What Needs To Be Fixed (3 Issues)

### Issue #1: Can Draw Before Clicking "Start Drawing" ❌
**Problem:** Drawing canvas accepts input immediately on scene load

**What Needs To Be Done:**
1. Add `isDrawingEnabled` flag to `DrawingCanvas.cs` (default: `false`)
2. Check this flag in `HandleInput()` before accepting mouse/touch input
3. Add code to `DrawingSceneUI.OnStartDrawing()` to enable the canvas

**Files to Modify:**
- `UnityGameFiles/Assets/Scripts/Drawing/DrawingCanvas.cs`
- `UnityGameFiles/Assets/Scripts/UI/DrawingSceneUI.cs`

---

### Issue #2: Guide Book Button Doesn't Respond to Clicks ❌
**Problem:** Button is visible but clicking it doesn't open the guide book

**What Needs To Be Done:**
1. Create an Editor script to:
   - Find the guide button in the scene
   - Ensure it's active and interactable
   - Connect it to `PlantGuideBook.OpenBook()` method
   - Enable raycastTarget on the button's Image component
   - Add/configure CanvasGroup for proper click detection
   - Move button to front in hierarchy to prevent blocking

**Files to Create:**
- `UnityGameFiles/Assets/Scripts/Editor/DrawingSceneCompleteFixer.cs`

---

### Issue #3: Background Doesn't Fit Plant Theme ❌
**Problem:** Background is generic, needs nature/plant aesthetic

**What Needs To Be Done:**
1. Editor script should:
   - Create/replace background GameObject
   - Try to load nature sprites from `Assets/Pixel Adventure 1/Background/` (Green.png, Blue.png, Brown.png)
   - If no sprites found, use soft sage green color (#D2F0D2)
   - Apply light green tint for plant theme

**Included in:**
- Same `DrawingSceneCompleteFixer.cs` Editor script

---

## 📋 Implementation Checklist

- [ ] **Modify DrawingCanvas.cs:**
  - [ ] Add `[Header("Drawing Control")]` section
  - [ ] Add `public bool isDrawingEnabled = false;` field
  - [ ] Add check at start of `HandleInput()`: `if (!isDrawingEnabled) return;`

- [ ] **Modify DrawingSceneUI.cs:**
  - [ ] Add code to `OnStartDrawing()` to find DrawingCanvas
  - [ ] Set `canvas.isDrawingEnabled = true;`
  - [ ] Add debug log

- [ ] **Create DrawingSceneCompleteFixer.cs:**
  - [ ] Create `UnityGameFiles/Assets/Scripts/Editor/` folder if needed
  - [ ] Add `[MenuItem("Tools/Sketch Blossom/Complete Drawing Scene Fix")]`
  - [ ] Implement `FixGuideBookButton()` - connect button to PlantGuideBook
  - [ ] Implement `ApplyPlantThemedBackground()` - create/style background
  - [ ] Implement `ConnectAllReferences()` - auto-wire UI references
  - [ ] Implement `EnsureEventSystem()` - verify EventSystem exists

- [ ] **Test in Unity:**
  - [ ] Run "Tools > Sketch Blossom > Complete Drawing Scene Fix"
  - [ ] Test: Can't draw until "Start Drawing" clicked
  - [ ] Test: Guide button opens guide book
  - [ ] Test: Background looks plant-themed

---

## 📁 Current File Structure

```
UnityGameFiles/Assets/Scripts/
├── Combat/
│   ├── MoveData.cs ✅
│   ├── MovesetDetector.cs ✅
│   ├── MoveExecutor.cs ✅
│   └── CombatManager.cs ✅ (modified)
│
├── Drawing/
│   └── DrawingCanvas.cs ⚠️ (needs modification)
│
├── UI/
│   ├── PlantGuideBook.cs ✅
│   ├── DrawingSceneUI.cs ⚠️ (needs modification)
│   └── GuideBookButtonSetup.cs ✅
│
└── Editor/ (folder doesn't exist yet)
    └── DrawingSceneCompleteFixer.cs ❌ (needs to be created)
```

---

## 🎯 Next Steps

1. Create the 3 fixes listed above
2. Test in Unity Editor
3. Commit and push changes
4. User tests in their Unity project

---

**Current Branch:** `claude/add-battle-scene-detection-011CV66nqeV6dx6r6qtaUXe1`
**Current Commit:** `5860162` - "Add visible guide book button and remove setup files"
