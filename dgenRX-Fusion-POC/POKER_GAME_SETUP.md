# Poker Game Setup Guide

## 🎮 What We Just Built

I've created the core poker game logic for your dgenRX POC. Here's what each script does:

### **1. PokerGameManager.cs** 
**The "Brain" of the Game**
- Manages the entire poker game flow
- Tracks game state (PreFlop, Flop, Turn, River, Showdown)
- Handles betting rounds
- Manages the pot
- Determines winners
- Runs on the Host (server) and syncs to all clients

**Key Features:**
- ✅ Blinds system (Small Blind: 10, Big Blind: 20)
- ✅ Betting round management
- ✅ Turn-based player actions
- ✅ Automatic game progression (PreFlop → Flop → Turn → River → Showdown)
- ✅ Winner determination at showdown

---

### **2. PokerPlayer.cs**
**Represents Each Player**
- Tracks each player's chips, cards, and status
- Handles player actions (Fold, Check, Call, Bet, Raise)
- Manages hole cards (the 2 cards each player gets)
- Networked so all players see the same state

**Key Features:**
- ✅ Starting chips: 1000
- ✅ Betting system
- ✅ Fold/Check/Call/Bet/Raise actions
- ✅ Chip management

---

### **3. HandEvaluator.cs**
**Determines Winners**
- Evaluates poker hands (Pair, Flush, Straight, etc.)
- Compares hands to find the winner
- Pure logic (no networking needed)

**Hand Rankings (Best to Worst):**
1. Royal Flush
2. Straight Flush
3. Four of a Kind
4. Full House
5. Flush
6. Straight
7. Three of a Kind
8. Two Pair
9. Pair
10. High Card

---

### **4. PokerTableUI.cs**
**User Interface Controller**
- Shows game state, pot, chips
- Displays your cards
- Action buttons (Fold, Check, Call, Bet, Raise)
- Betting slider/input
- Turn indicator

**Note:** This script expects UI elements to be set up in Unity. See setup instructions below.

---

## 🛠️ How to Set This Up in Unity

### **Step 1: Create the PokerGameManager GameObject**

1. In Unity, go to your scene (Baseline.unity)
2. Create an empty GameObject: Right-click in Hierarchy → Create Empty
3. Name it "PokerGameManager"
4. Add the `PokerGameManager` script to it
5. Make sure it has a `NetworkObject` component (add it if needed)

**Important:** The PokerGameManager should be spawned by the Host when the game starts. You'll need to update your `GlobalManager` to spawn it.

---

### **Step 2: Update PlayerController to Use PokerPlayer**

Your current `PlayerController.cs` is for movement. For poker, you need `PokerPlayer` instead.

**Option A: Replace PlayerController with PokerPlayer**
- Remove `PlayerController` from your Player prefab
- Add `PokerPlayer` component instead

**Option B: Keep Both (if you want movement + poker)**
- Add `PokerPlayer` component alongside `PlayerController`

---

### **Step 3: Create the Poker Table UI**

1. Create a Canvas: Right-click in Hierarchy → UI → Canvas
2. Create UI elements:

   **Text Elements:**
   - Game State Text (TextMeshPro)
   - Pot Text
   - Current Bet Text
   - Your Chips Text
   - Your Cards Text
   - Turn Indicator Text

   **Buttons:**
   - Fold Button
   - Check Button
   - Call Button
   - Bet Button
   - Raise Button

   **Betting Controls:**
   - Slider (for bet amount)
   - Input Field (for typing bet amount)
   - Text showing current bet amount

3. Add the `PokerTableUI` script to the Canvas (or a child GameObject)
4. Drag all the UI elements into the script's fields in the Inspector

---

### **Step 4: Update GlobalManager to Spawn PokerGameManager**

You'll need to modify `GlobalManager.cs` to spawn the `PokerGameManager` when the game starts. Add this to your `OnPlayerJoined` method:

```csharp
// Spawn PokerGameManager (only once, by the Host)
if (player == runner.LocalPlayer)
{
    // Spawn PokerGameManager prefab
    // (You'll need to create a prefab for this)
}
```

---

## 🎯 How the Game Flow Works

1. **Players Join** → `PokerGameManager` registers them
2. **2+ Players** → Game automatically starts a new hand
3. **Blinds Posted** → Small blind (10) and Big blind (20) are posted
4. **Cards Dealt** → Each player gets 2 hole cards
5. **Pre-Flop Betting** → Players bet, starting after the big blind
6. **Flop Dealt** → 3 community cards revealed, betting round
7. **Turn Dealt** → 4th community card, betting round
8. **River Dealt** → 5th community card, final betting round
9. **Showdown** → Best hand wins, pot awarded
10. **Next Hand** → Process repeats

---

## 🐛 Testing Tips

1. **Test Locally First:**
   - Open Unity
   - Press Play
   - Use the Host/Client buttons to test with 2 players
   - Watch the Console for debug messages

2. **Check Network Sync:**
   - Make sure all `[Networked]` properties are syncing
   - Test that actions from one player appear on others

3. **UI Testing:**
   - Make sure buttons are connected
   - Test that betting amounts update correctly
   - Verify turn indicators work

---

## 📝 Next Steps

1. **Set up the UI in Unity** (Step 3 above)
2. **Test the basic game flow** (2 players, one hand)
3. **Fix any bugs** you find
4. **Add polish** (animations, sounds, better visuals)

---

## ⚠️ Important Notes

- **Networked Properties:** Only properties marked `[Networked]` sync across the network
- **State Authority:** Only the Host (StateAuthority) can modify game state
- **RPCs:** Use RPCs to send actions from clients to the server
- **Testing:** Test with 2+ Unity instances to see multiplayer in action

---

## 🆘 Common Issues

**Q: Game doesn't start**
- Make sure `PokerGameManager` is spawned
- Check that players are registering
- Look at Console for errors

**Q: Actions don't work**
- Make sure `PokerPlayer` has `NetworkObject` component
- Check that RPCs are being called
- Verify StateAuthority is set correctly

**Q: UI doesn't update**
- Make sure UI elements are assigned in Inspector
- Check that `PokerTableUI` script is on a GameObject in the scene
- Verify `Update()` is being called

---

## 🎉 You're Ready!

You now have a working poker game foundation! The next phase is to:
1. Test it thoroughly
2. Add the Rivalry Booking Calendar
3. Add Head-to-Head Records
4. Build the economy systems

Good luck! 🚀

