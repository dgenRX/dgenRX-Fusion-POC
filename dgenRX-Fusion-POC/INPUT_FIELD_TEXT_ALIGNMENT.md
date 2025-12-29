# Fix Input Field Text Alignment

## Issue
Text appears in top left corner instead of centered/left-aligned properly.

## Solution

### Step 1: Find the Text Child Object
1. **Select BetInputField** in Hierarchy
2. **Expand BetInputField** (click the arrow/triangle next to it)
3. You should see child objects like:
   - **Text Area** (this contains the actual text)
   - Or just **Text** (some versions)
4. **Expand "Text Area"** if it exists
5. **Select the innermost text object** (the one with **TextMeshPro - Text (UI)** component)

### Step 2: Fix Text Alignment
1. **On the text child object** (not the Input Field itself)
2. In Inspector, find **TextMeshPro - Text (UI)** component
3. Look for **Alignment** - it's usually near the top, shows alignment icons
4. Click the alignment buttons to set:
   - **Left-Middle** (left aligned, vertically centered)
   - OR **Center-Middle** (centered both ways)

### Step 3: Fix Text Area RectTransform
1. **Still on the Text Area child object**
2. In **RectTransform**:
   - **Anchor Presets**: Hold Alt and click the **stretch-stretch** preset (bottom-right icon)
   - This makes the text area fill the input field
   - OR manually set:
     - **Anchor Min**: (0, 0)
     - **Anchor Max**: (1, 1)
     - **Left**: 5 (small padding)
     - **Right**: 5
     - **Top**: 5
     - **Bottom**: 5

### Step 4: Check Text Margins
1. In **TextMeshPro - Text (UI)** component
2. Find **Extra Settings** section
3. Check **Margin** values:
   - **Left**: 5-10
   - **Right**: 5-10
   - **Top**: 0-5
   - **Bottom**: 0-5

### Alternative: If You Can't Find the Text Object
1. **Select BetInputField**
2. In **TextMeshPro - Input Field** component, look for:
   - **Text Component** field (might be at the bottom)
   - OR **Text Area** field
3. If you see a reference, **click on it** - this will select the text object
4. Then you can set the alignment on that object

### If Still Can't Find It:
The text might be in a nested structure. Try:
- **BetInputField** → **Text Area** → **Text** (expand all levels)
- Look for any object with **TextMeshPro - Text (UI)** component
- That's where the alignment setting is

---

## Recommended Settings

- **Alignment**: Left-Middle (text on left, vertically centered)
- **Text Area Anchors**: Stretch to fill input field
- **Margins**: 5-10 pixels on sides, 0-5 on top/bottom
- **Point Size**: 36-48 for readability

---

**The text appearing in top-left means the text area isn't properly anchored/positioned within the input field.**

