# Fix Canvas Scaling for Different Resolutions

## Issue
Text and UI elements appear much bigger on client than host when resolutions differ.

## Root Cause
The Canvas Scaler is set to "Constant Pixel Size" mode, which doesn't scale with screen size. It should be "Scale With Screen Size" to maintain consistent UI appearance across different resolutions.

## Solution

### Step 1: Fix Canvas Scaler Settings
1. In Unity Hierarchy, select **MainCanvas**
2. In Inspector, find **Canvas Scaler** component
3. Change these settings:
   - **UI Scale Mode**: Change from "Constant Pixel Size" to **"Scale With Screen Size"**
   - **Reference Resolution**: Set to **1920 x 1080** (match your target resolution)
   - **Screen Match Mode**: **"Match Width Or Height"**
   - **Match**: **0.5** (balances width and height)
   - **Reference Pixels Per Unit**: **100** (keep as is)

### Step 2: Verify Settings
After changing, your Canvas Scaler should show:
- UI Scale Mode: **Scale With Screen Size**
- Reference Resolution: **X: 1920, Y: 1080**
- Screen Match Mode: **Match Width Or Height**
- Match: **0.5**

### Step 3: Test
- Build and test with different window sizes
- UI should now scale proportionally
- Text should appear the same size relative to screen on both host and client

---

**Why This Works:**
- "Scale With Screen Size" makes UI scale based on screen resolution
- Reference Resolution (1920x1080) is the "base" size everything scales from
- Match Width Or Height ensures UI scales proportionally whether window is wider or taller




