# Unity Setup Steps - Integrating Poker Game

## ✅ What's Already Done (In Code)

I've updated your `GlobalManager.cs` to spawn the `PokerGameManager` when the game starts. The code is ready!

---

## 🛠️ What You Need to Do in Unity

### **Step 1: Create PokerGameManager Prefab**

1. **In Unity, go to your scene** (Baseline.unity)

2. **Create a new GameObject:**
   - Right-click in Hierarchy → Create Empty
   - Name it "PokerGameManager"

3. **Add Components:**
   - Click on "PokerGameManager" GameObject
   - In Inspector, click "Add Component"
   - Add: `Network Object` (Fusion component)
   - Add: `Poker Game Manager` (your script)

4. **Configure Network Object:**
   - Set "Object Interest" to "Global" (this makes it visible to all players)
   - Make sure "State Authority" is set correctly (will be set by Host)

5. **Create the Prefab:**
   - Drag "PokerGameManager" from Hierarchy into `Assets/Prefabs/` folder
   - This creates a prefab
   - Delete the GameObject from the scene (we'll spawn it via code)

6. **Assign to GlobalManager:**
   - Select "GlobalManager" in the scene
   - In Inspector, find the "Poker Game Manager Prefab" field
   - Drag the prefab you just created into that field

---

### **Step 2: Update Player Prefab**

You have two options:

#### **Option A: Replace PlayerController with PokerPlayer** (Recommended)

1. **Open your PlayerPrefab:**
   - In `Assets/Prefabs/`, find `PlayerCube_NEW.prefab`
   - Double-click to open it

2. **Remove PlayerController:**
   - Select the prefab root
   - In Inspector, find `Player Controller` component
   - Click the three dots (⋮) → Remove Component

3. **Add PokerPlayer:**
   - Click "Add Component"
   - Add: `Poker Player` script
   - Make sure `Network Object` component is still there

4. **Save the prefab:**
   - Click "Overrides" → "Apply All" (if prompted)

#### **Option B: Keep Both Scripts** (If you want movement)

1. **Open PlayerPrefab** (same as above)

2. **Add PokerPlayer alongside PlayerController:**
   - Click "Add Component"
   - Add: `Poker Player` script
   - Both scripts will work together

---

### **Step 3: Set Up Poker Table UI** (Optional but Recommended)

1. **In your scene, find the Canvas:**
   - You should already have a "MainCanvas" GameObject

2. **Create UI Elements:**
   - Right-click on Canvas → UI → Text - TextMeshPro (for game state)
   - Right-click on Canvas → UI → Text - TextMeshPro (for pot)
   - Right-click on Canvas → UI → Text - TextMeshPro (for chips)
   - Right-click on Canvas → UI → Button - TextMeshPro (for Fold)
   - Right-click on Canvas → UI → Button - TextMeshPro (for Check)
   - Right-click on Canvas → UI → Button - TextMeshPro (for Call)
   - Right-click on Canvas → UI → Button - TextMeshPro (for Bet)
   - Right-click on Canvas → UI → Button - TextMeshPro (for Raise)
   - Right-click on Canvas → UI → Slider (for bet amount)
   - Right-click on Canvas → UI → Input Field - TextMeshPro (for typing bet)

3. **Add PokerTableUI Script:**
   - Create an empty GameObject under Canvas
   - Name it "PokerTableUI"
   - Add `Poker Table UI` component
   - Drag all the UI elements into the script's fields in Inspector

---

### **Step 4: Test It!**

1. **Press Play in Unity**

2. **Click "Start Host"** (first instance)

3. **Open a second Unity instance** (or build and run):
   - File → Build Settings → Build and Run
   - OR open Unity again and press Play, click "Start Client"

4. **Watch the Console:**
   - You should see messages like:
     - "[PokerGameManager] Spawned and waiting for players..."
     - "[PokerPlayer] Player X spawned with 1000 chips"
     - "[PokerGameManager] Starting new hand..."

5. **Test Actions:**
   - Use the UI buttons to Fold, Check, Call, Bet, Raise
   - Watch the game progress through PreFlop → Flop → Turn → River

---

## 🐛 Troubleshooting

### **"PokerGameManager not found"**
- Make sure you created the prefab
- Make sure it's assigned in GlobalManager's Inspector
- Check Console for errors

### **"Players not registering"**
- Make sure PlayerPrefab has `PokerPlayer` component
- Make sure `Network Object` is on the prefab
- Check that players are spawning

### **"UI not updating"**
- Make sure `PokerTableUI` script is in the scene
- Make sure all UI elements are assigned in Inspector
- Check that Canvas is active

### **"Actions not working"**
- Make sure you're clicking buttons (not just pressing keys)
- Check Console for RPC errors
- Verify StateAuthority is set correctly

---

## 📝 Quick Checklist

- [ ] PokerGameManager prefab created
- [ ] PokerGameManager prefab assigned to GlobalManager
- [ ] PlayerPrefab updated with PokerPlayer component
- [ ] UI elements created (optional)
- [ ] PokerTableUI script added and configured (optional)
- [ ] Tested with 2 players
- [ ] Game starts automatically when 2 players join
- [ ] Betting actions work

---

## 🎉 You're Done!

Once these steps are complete, you'll have:
- ✅ Full poker game logic working
- ✅ Players can join and play
- ✅ Betting rounds work
- ✅ Winners are determined
- ✅ Ready to add more features (Calendar, Records, etc.)

---

## 🆘 Need Help?

If something doesn't work:
1. Check the Console for error messages
2. Make sure all prefabs are assigned
3. Verify Network Object components are on networked GameObjects
4. Test with 2 Unity instances to see multiplayer

Good luck! 🚀

