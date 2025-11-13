# 🔧 Guide Book Troubleshooting

Having issues with the Guide Book not appearing or the button not working? Follow these steps!

---

## 🚀 Quick Fix (3 Easy Steps!)

### Step 1: Run the Diagnostic Tool

In Unity Editor menu:
```
Tools → Sketch Blossom → Debug Guide Book
```

Click **"Run Full Diagnostic"**

This will scan your scene and tell you exactly what's wrong.

---

### Step 2: Apply Automatic Fixes

In the same Debug Guide Book window, click:

**"Fix Common Issues"**

This will automatically fix:
- Missing references
- Button visibility
- Panel positioning
- EventSystem setup
- Component assignments

---

### Step 3: Test in Play Mode

1. Press **Play** in Unity
2. Look for the **"📖 Guide"** button (should be in top-right of Drawing Panel)
3. Click it to open the guide book
4. Press **H** key as alternative

**Still not working?** Continue below...

---

## 🔍 Common Issues & Solutions

### Issue 1: "I don't see the Guide Book button at all"

**Solution:**
1. Open: `Tools → Sketch Blossom → Debug Guide Book`
2. Click: **"Find All Guide Book Components"**
3. Check Console - does it find "GuideBookButton"?

**If NOT found:**
- Run the setup script again: `Tools → Sketch Blossom → Setup Drawing Scene UI`
- Make sure you're in the DrawingScene

**If found but not visible:**
1. In Hierarchy, search for "GuideBookButton"
2. Select it
3. In Inspector, make sure:
   - ✅ GameObject is Active (checkbox enabled)
   - ✅ Button → Interactable is checked
   - ✅ Parent panels are also active

---

### Issue 2: "Button is there but doesn't do anything when clicked"

**Solution A - Check EventSystem:**
1. In Hierarchy, search for "EventSystem"
2. If not found: Click **"Fix Common Issues"** in Debug window
3. Or manually: GameObject → UI → Event System

**Solution B - Check Button Listener:**
1. Select GuideBookButton in Hierarchy
2. In Inspector → Button component
3. Check OnClick() section
4. Should have `PlantGuideBook.OpenBook` assigned

**Solution C - Reconnect References:**
1. Find "GuideBookManager" in Hierarchy
2. Select it
3. In Inspector → PlantGuideBook component
4. Drag "GuideBookButton" from Hierarchy to "Open Book Button" field
5. Drag "GuideBookPanel" to "Book Panel" field

---

### Issue 3: "Button works but panel doesn't appear"

**Solution:**

In Unity Editor menu:
```
Tools → Sketch Blossom → Debug Guide Book
```

While in **Play Mode**, click:
**"Force Show Guide Book"**

This will:
- Activate the panel
- Move it to visible position
- Show you if it exists

**If panel appears:**
- Issue is with positioning or animation
- Panel might be off-screen

**Fix the position:**
1. Stop Play Mode
2. Find "GuideBookPanel" in Hierarchy
3. Select it
4. In Inspector → RectTransform:
   - Anchor Min: X: 0.4, Y: 0.1
   - Anchor Max: X: 0.95, Y: 0.9
   - Anchored Position: X: 0, Y: 0

---

### Issue 4: "Setup script didn't create anything"

**Possible causes:**
- Canvas missing from scene
- Script compilation error
- Wrong scene open

**Solution:**
1. Make sure you have **DrawingScene** open
2. Check Console for any script errors (red messages)
3. If errors exist, fix them first
4. Then run: `Tools → Sketch Blossom → Setup Drawing Scene UI`

---

## 🎯 Manual Setup (If Automated Fix Fails)

If all else fails, set it up manually:

### 1. Create Guide Book Button

1. In Hierarchy → Right-click on DrawingPanel/TopBar
2. UI → Button (create button)
3. Rename to "GuideBookButton"
4. Position it in top-right corner
5. Change button text to "📖 Guide"

### 2. Create Guide Book Panel

1. In Hierarchy → Right-click on Canvas
2. UI → Panel
3. Rename to "GuideBookPanel"
4. Set RectTransform:
   - Anchor Min: (0.4, 0.1)
   - Anchor Max: (0.95, 0.9)
5. Add children: Title text, Description text, Close button, Navigation buttons

### 3. Connect Components

1. Find "GuideBookManager" in Hierarchy (create if missing)
2. Add Component → PlantGuideBook script
3. Drag all UI elements to the script fields:
   - Book Panel → GuideBookPanel
   - Open Book Button → GuideBookButton
   - Close Book Button → CloseButton (inside GuideBookPanel)
   - Next/Previous buttons
   - Text fields

---

## 🛠️ Advanced Debugging

### Check Console Logs

When you press Play, look for these logs:
- ✅ `PlantGuideBook: Starting initialization...`
- ✅ `PlantGuideBook: Open button listener added`
- ✅ `Guide Book positions - Open: ..., Closed: ...`

If you see ❌ error logs:
- Read the error message
- It tells you exactly what's NULL
- Use Debug window to fix it

### Enable Debug Mode

In PlantGuideBook.cs script, the Start() method has Debug.Log statements that will help you see what's happening.

### Test Button Manually

While in Play Mode:
1. Select GuideBookManager in Hierarchy
2. In Inspector → PlantGuideBook component
3. Right-click on the component header
4. Select "OpenBook" from the dropdown
5. This calls the method directly

---

## 📦 Download Free Assets

Want to make your UI look prettier? Use the Asset Downloader:

```
Tools → Sketch Blossom → Download Free Assets
```

This opens a window with:
- Direct links to free UI packs
- Font recommendations
- Icon resources
- Background textures
- Import instructions

**Best Free Assets:**
1. **Kenney.nl UI Pack** - Complete UI sprites
2. **Google Fonts** - Fredoka One or Nunito
3. **Font Awesome** - Free icons

---

## ✅ Verification Checklist

After applying fixes, verify:

**In Editor (Not Playing):**
- [ ] GuideBookManager exists in scene
- [ ] PlantGuideBook component is attached
- [ ] All references are assigned (no "None" fields)
- [ ] GuideBookPanel exists
- [ ] GuideBookButton exists and is active
- [ ] EventSystem exists in scene

**In Play Mode:**
- [ ] No errors in Console
- [ ] Guide button is visible
- [ ] Clicking button opens panel
- [ ] Panel slides in from right
- [ ] Close button works
- [ ] Page navigation works
- [ ] H key opens guide
- [ ] ESC key closes guide

---

## 🆘 Still Not Working?

### Option 1: Start Fresh
1. Delete DrawingScene
2. Create new scene
3. Run setup script again: `Tools → Sketch Blossom → Setup Drawing Scene UI`

### Option 2: Check Script Compilation
1. Look at Console for red errors
2. Make sure all scripts compiled successfully
3. Try: Assets → Reimport All (takes a while)

### Option 3: Check Unity Version
- Project requires Unity 2020.3 or newer
- TextMeshPro package must be installed
- UI package must be installed

---

## 📞 Debug Window Features

The Debug Guide Book window has 4 useful buttons:

1. **Run Full Diagnostic**
   - Scans entire scene
   - Lists all issues
   - Outputs to Console

2. **Fix Common Issues**
   - Automatically fixes most problems
   - Reconnects references
   - Sets up EventSystem

3. **Force Show Guide Book** (Play Mode only)
   - Makes panel visible immediately
   - Useful for testing

4. **Find All Guide Book Components**
   - Lists everything related to guide book
   - Shows full hierarchy paths
   - Helps identify what exists

---

## 🎯 Expected Behavior

**When working correctly:**

1. **Scene Start:**
   - Guide button visible in top-right
   - Guide panel hidden

2. **Click Guide Button:**
   - Panel slides in from right
   - Shows "Welcome to Plant Guide" page
   - Previous button disabled (on first page)

3. **Navigate Pages:**
   - Click Next → Shows Sunflower guide
   - Click Next → Shows Cactus guide
   - Click Next → Shows Water Lily guide
   - Click Next → Shows Tips page

4. **Close Guide:**
   - Click ✕ button → Panel slides away
   - Press ESC → Panel slides away
   - Guide button reappears

5. **Keyboard Shortcuts:**
   - H → Open guide
   - ESC → Close guide
   - Arrow keys or A/D → Navigate pages

---

## 💡 Pro Tips

1. **Check Console First**
   - Always look at Console window
   - Debug logs tell you exactly what's wrong
   - Red errors must be fixed

2. **Use Debug Window**
   - It's your best friend
   - "Fix Common Issues" solves 90% of problems
   - Run diagnostic to see everything

3. **Verify in Play Mode**
   - Some things only show up when playing
   - Test after every fix

4. **Keep References Assigned**
   - If you delete/rename objects, references break
   - Re-run "Fix Common Issues" to reconnect

---

## 📝 Summary

**Quick Fix Steps:**
1. Tools → Sketch Blossom → Debug Guide Book
2. Click "Run Full Diagnostic"
3. Click "Fix Common Issues"
4. Press Play and test

**If that doesn't work:**
1. Check Console for errors
2. Use "Find All Components" to see what exists
3. Manually verify references in Inspector
4. Force show in Play Mode to test

**Still broken?**
- Start fresh with setup script
- Check for script compilation errors
- Verify Unity version and packages

---

**The debug tools are designed to fix everything automatically. 99% of issues are solved by clicking "Fix Common Issues"!**

Good luck! 🍀
