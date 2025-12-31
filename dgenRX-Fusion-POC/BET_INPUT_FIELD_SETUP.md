# Bet Input Field Setup

## Issue
Slider is visible but bet input field is not visible.

## Solution

### Step 1: Hide the Slider in Unity
1. Exit Play mode
2. In Hierarchy, find the **BetSlider** GameObject (under Canvas)
3. Select it
4. In Inspector, uncheck the checkbox at the top (next to GameObject name) to **disable** it
   - OR right-click → **Deactivate**

### Step 2: Find/Create Bet Input Field
1. In Hierarchy, look for **BetInputField** (should be under Canvas)
2. If it doesn't exist, create it:
   - Right-click Canvas → **UI → Input Field - TextMeshPro**
   - Name it **BetInputField**
   - Position it where you want (near the betting buttons)

### Step 3: Assign Input Field to Script
1. In Hierarchy, select **PokerTableUI** GameObject (or wherever PokerTableUI script is)
2. In Inspector, find **Poker Table UI** component
3. Find **Bet Input Field** field
4. Drag **BetInputField** from Hierarchy into this field

### Step 4: Make Input Field Visible
1. Select **BetInputField** in Hierarchy
2. In Inspector, check:
   - **GameObject is enabled** (checkbox at top)
   - **RectTransform** - make sure it's positioned on screen (not at 0,0 or off-screen)
   - **Image component** - Alpha should be > 0 (not transparent)
   - **TextMeshPro - Input Field** component - should be enabled

### Step 5: Position Input Field
1. Select **BetInputField**
2. In Inspector → **RectTransform**:
   - Set **Pos X** and **Pos Y** to visible position (e.g., X: 0, Y: -200)
   - Set **Width**: 200
   - Set **Height**: 40

### Step 6: Test
1. Enter Play mode
2. Start Host and Client
3. The slider should be hidden
4. The input field should be visible (you can type in it when it's your turn)

---

**Note**: The code now automatically hides the slider and shows the input field, but you need to make sure the input field exists in your scene and is assigned to the script.





