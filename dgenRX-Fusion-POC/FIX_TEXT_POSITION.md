# Fix Text Position in Input Field

## Issue
Text appears in top-left corner. Text Area child exists but has no TextMeshPro component.

## Solution: Fix Text Area RectTransform

The text position is controlled by the **Text Area** child object's RectTransform, not alignment.

### Steps:

1. **Select BetInputField** in Hierarchy
2. **Expand BetInputField** to see "Text Area" child
3. **Select "Text Area"** child object
4. In Inspector, find **RectTransform** component
5. **Set the anchors to fill the input field:**
   - Click the **Anchor Presets** square (top-left of RectTransform)
   - Hold **Alt + Shift**
   - Click the **stretch-stretch** preset (bottom-right icon in the grid)
   - This makes Text Area fill the entire input field

6. **OR manually set these values:**
   - **Anchor Min**: (0, 0)
   - **Anchor Max**: (1, 1)
   - **Left**: 5 (small padding from left edge)
   - **Right**: -5 (small padding from right edge)
   - **Top**: -5 (small padding from top)
   - **Bottom**: 5 (small padding from bottom)
   - **Width**: Should be 0 (since it's stretched)
   - **Height**: Should be 0 (since it's stretched)

7. **Set Position to (0, 0):**
   - **Pos X**: 0
   - **Pos Y**: 0

### Alternative: If Text Area Has No Components

If Text Area has no components at all, it might be just a container. Try:

1. **Select Text Area**
2. **Add Component** → **RectTransform** (if missing)
3. Then follow steps above to set anchors

---

## What This Does

By setting the Text Area to stretch and fill the input field with proper anchors, the text will be positioned correctly inside the input field instead of appearing in the top-left corner.

---

**The key is making the Text Area fill the entire input field using stretch anchors.**



