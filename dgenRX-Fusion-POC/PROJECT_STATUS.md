# dgenRX POC - Current Status & Development Plan

## 📊 Current Status Assessment

### ✅ What's Already Built (Foundation - ~5-10%)

1. **Networking Infrastructure** ✅
   - Photon Fusion integration working
   - Host/Client connection system
   - NetworkRunner setup with callbacks
   - Basic player spawning

2. **Card System (Basic)** ✅
   - 52-card deck with shuffling
   - Networked card dealing
   - Community cards (Flop, Turn, River) synchronized
   - Card ID system (0-51)

3. **Basic UI** ⚠️
   - Simple OnGUI connection window
   - TextMeshPro for card display
   - Very minimal - needs complete overhaul

### ❌ What's Missing (Critical Features - ~90-95%)

#### **MUST-HAVE Features (POC Requirements):**

1. **Rivalry Booking Calendar** ❌
   - Calendar UI
   - Opponent search/friends list
   - Date/time booking system
   - Countdown timers
   - Notification placeholders

2. **Full Poker Game Logic** ❌
   - Betting system (bet, call, raise, fold)
   - Pot management
   - Hand evaluation (who wins)
   - Turn management
   - Blinds/ante system
   - All-in logic
   - Showdown logic

3. **Lifetime Head-to-Head Records** ❌
   - Player profiles
   - Win/loss tracking per opponent
   - Streak counters
   - Statistics display

4. **$RX Loyalty Rewards** ❌
   - Balance display
   - Point earning system
   - Rewards tab UI
   - Rakeback calculations

5. **Gold Coin + Sweeps Economy** ❌
   - Store UI
   - Purchase packs ($4.99-$999)
   - Mock checkout flow
   - Sweeps bonus system

6. **Spectator Rail** ❌
   - Spectator mode
   - Real-time table viewing
   - Emoji reactions/chat

7. **Login & Wallet** ❌
   - Username/Google sign-in
   - Mock MetaMask connect
   - User authentication

8. **Proper Poker Table UI** ❌
   - Professional table design
   - Card visuals
   - Chip stacks
   - Betting buttons
   - Player positions

---

## 🎯 Development Plan (Prioritized)

### **Phase 1: Core Poker Game (Weeks 1-3)**
**Goal:** Get a playable 1v1 poker game working

#### Week 1: Poker Game Logic Foundation
- [ ] Create `PokerGameManager` script
- [ ] Implement betting system (bet, call, raise, fold, check)
- [ ] Add pot management
- [ ] Create hand evaluator (determine winner)
- [ ] Implement turn/action system
- [ ] Add blinds system

#### Week 2: Player Actions & Game Flow
- [ ] Create `PokerPlayer` networked component
- [ ] Implement chip stacks per player
- [ ] Add action buttons UI (Fold, Check, Bet, Raise)
- [ ] Create betting UI (slider/input for bet amounts)
- [ ] Implement all-in logic
- [ ] Add showdown logic (reveal cards, determine winner)

#### Week 3: Polish & Testing
- [ ] Test multiplayer flow (2-6 players)
- [ ] Fix networking bugs
- [ ] Add game state management (pre-flop, flop, turn, river, showdown)
- [ ] Add basic animations/transitions

---

### **Phase 2: Rivalry System (Weeks 4-5)**
**Goal:** The "star feature" - booking calendar and records

#### Week 4: Booking Calendar
- [ ] Create calendar UI (Unity UI Toolkit or UGUI)
- [ ] Build opponent search/friends list (dummy data for POC)
- [ ] Implement date/time picker
- [ ] Create booking confirmation system
- [ ] Add countdown timer
- [ ] Notification placeholder system

#### Week 5: Head-to-Head Records
- [ ] Create `PlayerProfile` system
- [ ] Build win/loss tracking per opponent
- [ ] Implement streak counter
- [ ] Create profile UI screen
- [ ] Add statistics display ("You own @CryptoChad 27-3")

---

### **Phase 3: Economy & Rewards (Week 6)**
**Goal:** $RX and coin systems

- [ ] Create `EconomyManager` singleton
- [ ] Implement $RX balance system
- [ ] Add point earning (wins, streaks, bookings)
- [ ] Create rewards tab UI
- [ ] Build Gold Coin store
- [ ] Implement Sweeps Coin system
- [ ] Create mock purchase flow ($4.99-$999 packs)
- [ ] Add "Purchase successful" feedback

---

### **Phase 4: Login & Spectator (Week 7)**
**Goal:** Authentication and viewing

- [ ] Create login screen UI
- [ ] Implement username system (local storage for POC)
- [ ] Add Google sign-in placeholder button
- [ ] Create mock MetaMask "connect" button
- [ ] Build spectator mode
- [ ] Add spectator camera/view
- [ ] Implement emoji reactions/chat (basic)

---

### **Phase 5: UI/UX Polish (Week 8)**
**Goal:** Make it look professional

- [ ] Design proper poker table UI
- [ ] Create card visuals/sprites
- [ ] Add chip stack visuals
- [ ] Polish animations
- [ ] Add sound effects (optional)
- [ ] Mobile-responsive design
- [ ] Android build testing

---

### **Phase 6: Integration & Demo Prep (Weeks 9-10)**
**Goal:** Everything works together

- [ ] Connect all systems
- [ ] End-to-end testing
- [ ] Create demo flow: Login → Calendar → Book → Play → Win → See Records
- [ ] Record demo video
- [ ] Bug fixes
- [ ] Performance optimization

---

## 🛠️ Technical Architecture Recommendations

### **Script Organization:**
```
Assets/Scripts/
├── Core/
│   ├── PokerGameManager.cs (main game logic)
│   ├── PokerPlayer.cs (player state/actions)
│   ├── HandEvaluator.cs (determine winning hand)
│   └── EconomyManager.cs (rewards/coins)
├── Networking/
│   ├── GlobalManager.cs (existing - keep)
│   └── NetworkedCardDeck.cs (existing - enhance)
├── UI/
│   ├── PokerTableUI.cs
│   ├── CalendarUI.cs
│   ├── ProfileUI.cs
│   └── StoreUI.cs
└── Data/
    ├── PlayerProfile.cs
    ├── RivalryRecord.cs
    └── EconomyData.cs
```

### **Key Design Patterns:**
- **Singleton** for managers (EconomyManager, PokerGameManager)
- **NetworkBehaviour** for all networked game objects
- **ScriptableObjects** for game data (card sprites, store items)
- **Event System** for UI updates (UnityEvents or C# events)

---

## 📝 Next Immediate Steps

1. **Start with Poker Game Logic** - This is the foundation
2. **Build incrementally** - Get 1v1 working first, then expand
3. **Test early and often** - Use Unity's Play mode with multiple instances
4. **Keep it simple** - POC doesn't need perfect code, just working features

---

## ⚠️ Important Notes

- **You're using Photon Fusion** (not PUN 2 as mentioned in goal doc) - this is fine, Fusion is actually better
- **Focus on Android** - iOS is out of scope
- **Mock data is OK** - Don't need real backend for POC
- **UI can be simple** - Doesn't need to be beautiful, just functional
- **6-10 weeks is tight** - Prioritize must-haves, skip nice-to-haves if needed

---

## 🎬 Success Metric Reminder

The demo video should show:
1. Login → 
2. Browse calendar → 
3. Book rematch against rival → 
4. Play heads-up poker → 
5. Win → 
6. See streak update + head-to-head record change + earn $RX

**If this flow works, the POC succeeds!**





