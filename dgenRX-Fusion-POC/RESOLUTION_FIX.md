# Fix Client Screen Resolution & Window Mode

## Issue
Client window is maximized even though "Windowed" is selected in build settings.

## Solution

### Step 1: Add WindowManager Script to Scene
1. In Unity Hierarchy, find or create an empty GameObject (e.g., "WindowManager")
2. Add the `WindowManager` component to it
3. In Inspector, you can adjust:
   - **Window Width**: 1920
   - **Window Height**: 1080
   - **Start Windowed**: ✓ (checked)
   - **Make Resizable**: ✓ (checked)

### Step 2: Unity Build Settings (CRITICAL)
1. Go to **File → Build Settings**
2. Click **Player Settings**
3. Under **Resolution and Presentation**:
   - **Fullscreen Mode**: Select **Windowed** (NOT "Maximized Window" - this is the key!)
   - **Default Screen Width**: 1920
   - **Default Screen Height**: 1080
   - **Resizable Window**: ✓ (check this box)
   - **Allow Fullscreen Switch**: ✓ (optional, allows Alt+Enter)

**IMPORTANT**: If you see "Maximized Window" selected, change it to "Windowed". "Maximized Window" will always start maximized and may not be resizable.

### Step 3: Test
- The `WindowManager` script will force windowed mode on startup
- Window should be resizable and not maximized
- Press **F11** to toggle fullscreen if needed

### Alternative: Manual Code Fix
If the script doesn't work, you can add this directly to `GlobalManager.cs` in `Start()`:
```csharp
#if !UNITY_EDITOR
Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
#endif
```

---

**Note**: The `WindowManager` script only runs in builds (not in Editor), so it won't affect your Editor testing.

