# What You Still Need To Do - Quick Checklist

I've fixed the code integration, but Unity scenes can't be modified from code. Here's what you need to do in Unity:

---

## ✅ DONE FOR YOU (Automatically)

- ✅ Added `MoveExecutor` field to `DrawingBattleSceneManager`
- ✅ Added screen shake integration code
- ✅ Added `PlayScreenShake()` helper method
- ✅ Created `BattleSceneSetupHelper` editor tool

---

## 🔧 DO THIS IN UNITY (5-10 minutes)

### Step 1: Open the Battle Scene
1. Open Unity
2. Open the **DrawingBattleScene**

### Step 2: Run the Auto-Setup Tool
1. In Unity menu: **Sketch Blossom → Battle Scene Setup Helper**
2. Click **"Check Battle Scene Setup"** button
3. The tool will auto-fix most issues!

**What it does automatically:**
- ✅ Adds MoveExecutor component to BattleManager
- ✅ Assigns the Main Camera
- ✅ Wires up text UI references (if found)
- ✅ Checks guide book setup

### Step 3: Manual Checks (if auto-setup didn't work)

#### 3a. Verify MoveExecutor Setup

**Find BattleManager GameObject:**
- In Hierarchy, find **BattleManager** (or the object with DrawingBattleSceneManager)

**Check MoveExecutor component:**
- Select BattleManager
- In Inspector, scroll down to **MoveExecutor** component
- Verify **Main Camera** field is assigned (should show "Main Camera" or your camera name)
- If empty, drag your Main Camera into this field

**Check DrawingBattleSceneManager references:**
- Still in Inspector, find **DrawingBattleSceneManager** component
- Under **Attack Animations** section
- Verify **Move Executor** field is assigned
- If empty, drag the MoveExecutor component into this field (from same GameObject)

#### 3b. Test Screen Shake

**Quick test:**
1. Play the game
2. Enter a battle
3. Draw a powerful move (like a big zigzag for "Inferno Wave")
4. **Expected:** Camera should shake when move hits
5. **If not shaking:** Camera not assigned - go back to step 3a

---

## 🎨 OPTIONAL: Move Guide Book Setup (30-60 minutes)

**If you want the interactive move guide (press M to see all moves):**

### Option A: Use the Setup Tool
1. In **Battle Scene Setup Helper** window
2. Click **"Create MoveGuideBook GameObject"**
3. Follow the instructions in **INTEGRATION_TODO.md** to create the UI

### Option B: Skip the guide book
- You can skip this entirely
- The moves will still have colors and screen shake
- You just won't have an in-game guide to view moves

---

## 🎯 Priority Actions

**Minimum to get working (5 minutes):**
```
1. Open DrawingBattleScene
2. Sketch Blossom → Battle Scene Setup Helper
3. Click "Check Battle Scene Setup"
4. Done! Test in play mode
```

**Full experience (60 minutes):**
```
1. Do minimum setup above
2. Read INTEGRATION_TODO.md section 2
3. Create Move Guide Book UI
4. Wire up all references
5. Test guide book (press M)
```

---

## ✅ Verification Checklist

After running the setup tool, verify these:

**In Unity Hierarchy:**
- [ ] BattleManager GameObject exists
- [ ] BattleManager has MoveExecutor component
- [ ] BattleManager has DrawingBattleSceneManager component

**In MoveExecutor Inspector:**
- [ ] Main Camera field is assigned (not None)
- [ ] Screen Shake Multiplier = 1.0

**In DrawingBattleSceneManager Inspector:**
- [ ] Under "Attack Animations"
- [ ] Move Executor field is assigned (not None)

**Test in Play Mode:**
- [ ] Enter a battle
- [ ] Draw a move
- [ ] See screen shake (even subtle)
- [ ] See unique colors (not just generic orange/green/blue)

---

## 🐛 Troubleshooting

### "Setup Helper menu item not found"
- Wait for Unity to compile the editor script
- Check Console for compilation errors
- Try closing and reopening Unity

### "Auto-setup button does nothing"
- Check Console for errors
- Make sure you're in DrawingBattleScene (not another scene)
- Try manual setup instead

### "Camera still not assigned after auto-setup"
- Manually drag Main Camera to MoveExecutor.mainCamera field
- Make sure your camera is tagged "MainCamera"

### "No screen shake happening"
- Print statement in Console should say "Playing enhanced move visual effects"
- If not appearing: MoveExecutor field not assigned in DrawingBattleSceneManager
- If appearing but no shake: Camera not assigned in MoveExecutor

---

## 📚 Additional Resources

- **INTEGRATION_TODO.md** - Complete Unity setup guide
- **BattleMoves.md** - How the move system works
- **SETUP_ISSUES.md** - Detailed explanation of what was wrong

---

## 🚀 Quick Start Commands

**In Unity Editor:**
1. Open DrawingBattleScene
2. Menu: Sketch Blossom → Battle Scene Setup Helper
3. Click "Check Battle Scene Setup" (with auto-fix enabled)
4. Play test!

**Expected Result:**
- Screen shakes on powerful moves
- Moves show unique colors
- Everything just works!

---

## What's Left After This?

**If you want move colors and screen shake:** Nothing! You're done.

**If you want the move guide book:**
- Create the UI (see INTEGRATION_TODO.md)
- Wire up the references
- Test with M key

---

## Need Help?

If the auto-setup tool doesn't work:
1. Check the Console for errors
2. Read SETUP_ISSUES.md for manual setup
3. Verify you're using Unity 2021+ (for FindFirstObjectByType)

The editor script will tell you exactly what's missing!
