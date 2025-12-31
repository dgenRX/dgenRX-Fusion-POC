# How to Find Image Component Alpha

## Step-by-Step:

1. **Select BetInputField** in Hierarchy

2. **In Inspector**, scroll down to find the **Image** component
   - It should be below the RectTransform
   - Look for a component that says "Image (Script)" or just "Image"

3. **Click on the Image component** to expand it

4. **Find the "Color" field** - it looks like a colored square/rectangle

5. **Click on the Color field** - a color picker will open

6. **Look for the "A" (Alpha) slider** at the bottom of the color picker
   - It should be set to **255** (or 1.0 if using 0-1 scale)
   - If it's 0, the input field is invisible!

7. **Set Alpha to 255** (or 1.0) and close the color picker

---

## Alternative: Quick Check

If you can't find the Image component:
- The input field might not have an Image component
- Try adding one: Click "Add Component" → Search "Image" → Add it
- Then set the Alpha as above

---

## Other Things to Check:

1. **Is it behind other UI?**
   - Check the **Canvas** sorting order
   - Make sure BetInputField is a child of Canvas
   - Try moving it in Hierarchy (higher = on top)

2. **Is it positioned off-screen?**
   - Check RectTransform → **Pos X** and **Pos Y**
   - Should be something like X: 0, Y: -200 (not X: 10000, Y: 10000)

3. **Is the GameObject enabled?**
   - Check the checkbox at the very top of Inspector (next to GameObject name)

4. **Is the Canvas visible?**
   - Make sure the Canvas GameObject itself is enabled




