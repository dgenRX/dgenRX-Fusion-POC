# Fix Missing UI Text on Client

## Issue
"Your Chips", "Pot", and "Your Bet" text labels are missing on client (but work on host).

## Most Likely Cause
The text elements aren't assigned to the PokerTableUI script in the Inspector.

## Solution

### Step 1: Check Console for Warnings
1. Start Host and Client
2. Check Console on **client side**
3. Look for warnings like:
   - `[PokerTableUI] potText is null!`
   - `[PokerTableUI] currentBetText is null!`
   - `[PokerTableUI] yourChipsText is null!`

If you see these, the text elements aren't assigned.

### Step 2: Assign Text Elements in Inspector

**Important:** You need to do this in the **scene file**, not just in Play mode!

1. **Exit Play mode** (if in Play mode)
2. **In Hierarchy**, find the GameObject with **PokerTableUI** script
   - It might be named "PokerTableUI" or be on the Canvas
3. **Select that GameObject**
4. **In Inspector**, find **Poker Table UI** component
5. **Check each field:**
   - **Pot Text**: Should show [PotText] or [None]
   - **Current Bet Text**: Should show [CurrentBetText] or [None]
   - **Your Chips Text**: Should show [YourChipsText] or [None]
6. **If any show [None]:**
   - In Hierarchy, find the text element (e.g., "PotText")
   - **Drag it** from Hierarchy into the corresponding field in Inspector
   - Repeat for all missing text elements

### Step 3: Verify Text Elements Exist

1. **In Hierarchy**, under Canvas, look for:
   - **PotText** (or similar name)
   - **CurrentBetText** (or similar name)
   - **YourChipsText** (or similar name)
2. **If they don't exist**, create them:
   - Right-click Canvas → UI → Text - TextMeshPro
   - Name it appropriately
   - Position it on screen
   - Assign it to PokerTableUI script

### Step 4: Check Text Visibility

Even if assigned, text might be invisible:

1. **Select each text element** in Hierarchy
2. **In Inspector**, check:
   - **GameObject is enabled** (checkbox at top)
   - **RectTransform Position** - should be on screen (not 0,0 or off-screen)
   - **TextMeshPro component** → **Vertex Color** → Alpha should be 255 (or 1.0)
   - **TextMeshPro component** → **Font Asset** is assigned

### Step 5: Test

1. **Save the scene** (Ctrl+S)
2. **Enter Play mode**
3. **Start Host and Client**
4. **Check Console** - should NOT see null warnings
5. **Text should appear** on both host and client

---

## Why This Happens

The PokerTableUI script exists in the scene, so both host and client have it. But if the text element references aren't assigned in the Inspector, the script can't find them to update their text.

**The fix is simple: assign the text elements in the Inspector!**



