# Inspector Styling Guide for UI Text Elements

## Overview

The UI text elements (Pot, Current Bet, Your Chips, etc.) are now created **programmatically** (which ensures they work in builds), but their **styling is controlled via the Inspector**. This gives you the best of both worlds:
- ✅ **Reliability**: Programmatic creation works in builds
- ✅ **Flexibility**: Inspector styling lets you adjust colors, positions, sizes without code changes

## How It Works

1. **Text elements are created at runtime** using the `CreateTextElement()` method
2. **Style settings are read from Inspector** fields in the `PokerTableUI` component
3. **You can adjust all styling in the Inspector** and see changes immediately

## Configuring Styles in Inspector

### Step 1: Find the PokerTableUI Component

1. **In Hierarchy**, select the GameObject with the `PokerTableUI` script
   - Usually named "PokerTableUI" or attached to the Canvas
2. **In Inspector**, scroll down to the **"UI Text Styling (Inspector-Configurable)"** section

### Step 2: Adjust Style Settings

Each text element has its own style configuration:

#### **Pot Text Style**
- **Font Size**: Text size (default: 24)
- **Color**: Text color (default: White)
- **Anchor Position**: Where on screen (X: 0.1 = left, Y: 0.8 = upper-middle)
- **Size**: Width and height of text box (default: 200x30)
- **Alignment**: Left, Center, Right, etc. (default: Left)

#### **Current Bet Text Style**
- Same settings as Pot Text
- Default position: X: 0.1, Y: 0.75 (slightly below Pot)

#### **Your Chips Text Style**
- Same settings as Pot Text
- Default position: X: 0.1, Y: 0.7 (below Current Bet)

#### **Game State Text Style**
- Default position: X: 0.5, Y: 0.95 (top center)
- Default size: 300x30

#### **Your Cards Text Style**
- Default position: X: 0.1, Y: 0.65 (below Your Chips)
- Default size: 300x30

#### **Turn Indicator Text Style**
- Default font size: 32 (larger for visibility)
- Default color: Green
- Default position: X: 0.5, Y: 0.85 (center-top)
- Default alignment: Center
- Default size: 300x40

### Step 3: Understanding Anchor Position

**Anchor Position** uses normalized coordinates (0-1 range):
- **X: 0.0** = Left edge of screen
- **X: 0.5** = Center of screen
- **X: 1.0** = Right edge of screen
- **Y: 0.0** = Bottom of screen
- **Y: 0.5** = Middle of screen
- **Y: 1.0** = Top of screen

**Examples:**
- `(0.1, 0.8)` = Left side, upper-middle
- `(0.5, 0.95)` = Top center
- `(0.9, 0.1)` = Right side, bottom

### Step 4: Testing Your Changes

1. **Make changes in Inspector** (while in Edit mode or Play mode)
2. **Press Play** (or restart if already playing)
3. **Text elements will be recreated** with your new style settings
4. **Adjust as needed** - changes take effect on next play

## Common Adjustments

### Moving Text to Different Positions

**To move text to top-left:**
- Set Anchor Position to `(0.05, 0.95)`

**To move text to bottom-right:**
- Set Anchor Position to `(0.95, 0.05)`

**To center text horizontally:**
- Set Anchor Position X to `0.5`

### Changing Text Size

**To make text larger:**
- Increase Font Size (e.g., 32, 36, 48)
- Increase Size width to accommodate larger text

**To make text smaller:**
- Decrease Font Size (e.g., 18, 16, 14)
- Decrease Size width if needed

### Changing Text Color

**For dark backgrounds:**
- Use light colors: White, Yellow, Light Green

**For light backgrounds:**
- Use dark colors: Black, Dark Blue, Dark Red

**For visibility:**
- Turn Indicator: Green or Yellow (high contrast)
- Game State: White or Yellow
- Other text: White or Light Gray

### Adjusting Text Alignment

- **Left**: Text starts at anchor position (default for most)
- **Center**: Text centered on anchor position (good for Turn Indicator)
- **Right**: Text ends at anchor position

## Important Notes

1. **Changes take effect on Play**: Style changes are applied when the game starts (in `Start()`)
2. **No need to assign text elements**: The old Inspector fields (`potText`, `currentBetText`, etc.) are ignored - styles are used instead
3. **Works in builds**: Programmatic creation ensures this works in standalone builds
4. **Can adjust during Play mode**: Changes to Inspector values will apply on next play/restart

## Troubleshooting

### Text Not Visible
- Check **Color Alpha** is 1.0 (not transparent)
- Check **Size** is not (0, 0)
- Check **Anchor Position** is within screen bounds (0-1 range)

### Text in Wrong Position
- Adjust **Anchor Position** values
- Remember: X=0 is left, X=1 is right, Y=0 is bottom, Y=1 is top

### Text Too Small/Large
- Adjust **Font Size**
- Adjust **Size** width/height to match

### Changes Not Applying
- Make sure you're editing the **PokerTableUI** component in Inspector
- Restart Play mode to recreate elements with new styles

---

**Summary**: You now have full Inspector control over text styling while maintaining the reliability of programmatic creation!



