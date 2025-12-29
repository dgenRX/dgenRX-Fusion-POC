# How to Resize the Bet Input Field

## Step-by-Step:

1. **Select BetInputField** in Hierarchy

2. **In Inspector**, find **RectTransform** component

3. **Change the size:**
   - **Width**: Change from current value to desired width (e.g., 200, 300, etc.)
   - **Height**: Change from current value to desired height (e.g., 50, 60, etc.)

4. **You can also adjust the text size:**
   - Find **TextMeshPro - Input Field** component
   - Expand it
   - Find **Point Size** field (this is the font/text size)
   - Increase it (e.g., from 18 to 24 or 30)

5. **IMPORTANT: Set Character Limit:**
   - In **TextMeshPro - Input Field** component
   - Find **Character Limit** field
   - Change it from **0** to a number like **10** or **20** (0 means no limit, but can cause issues)
   - This allows you to type in the field

---

## Quick Tips:

- **Width**: 200-300 is usually good for a bet input
- **Height**: 40-60 is usually readable
- **Point Size**: 24-30 is usually comfortable
- **Character Limit**: Set to 10-20 (not 0) to allow typing
- The input field will scale with your Canvas settings

## Troubleshooting:

**Can't type in the field?**
- Check **Character Limit** is NOT 0 (set it to 10 or 20)
- Make sure the Input Field component is enabled
- Check that the GameObject itself is enabled

---

**Note**: The code doesn't hard-code the size - you can change it freely in Unity!

