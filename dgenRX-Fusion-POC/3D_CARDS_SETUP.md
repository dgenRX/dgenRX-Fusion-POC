# 3D Cards Setup Guide

## Overview
This guide will help you set up the 3D card rendering system using your `playingcard.fbx` model and card textures.

## Step 1: Create Card Model Prefab

1. **In Unity, find your `playingcard.fbx` file:**
   - It should be in `Assets/CardAssets/.fbx/` (or wherever you imported it)

2. **Create a Prefab:**
   - Drag `playingcard.fbx` from Project window into the Scene (temporarily)
   - In Hierarchy, select the card GameObject
   - Drag it from Hierarchy into `Assets/Prefabs/` folder
   - This creates a prefab
   - Delete the card from the Scene (we'll spawn it via code)

3. **Name the prefab:** `CardPrefab` (or keep the original name)

## Step 2: Set Up Card Textures

1. **Create Resources folder structure:**
   - Create folder: `Assets/Resources/`
   - Inside Resources, create: `CardAssets/`
   - Inside CardAssets, create: `playing card images/`

2. **Move all card images:**
   - Copy all 56 PNG files from `Assets/CardAssets/playing card images/`
   - Paste them into `Assets/Resources/CardAssets/playing card images/`
   - Final path: `Assets/Resources/CardAssets/playing card images/`

## Step 3: Create CardRenderer3D GameObject

1. **In your Scene, create an empty GameObject:**
   - Right-click Hierarchy → Create Empty
   - Name it: `CardRenderer3D`

2. **Add the CardRenderer3D component:**
   - Select `CardRenderer3D` GameObject
   - In Inspector, click "Add Component"
   - Search for: `Card Renderer 3D`
   - Add it

3. **Configure the component:**
   - **Card Model Prefab:** Drag your `CardPrefab` (from Step 1) into this field
   - **Textures Folder Path:** `CardAssets/playing card images` (no "Resources/" prefix)
   - **Card Spacing:** 0.15
   - **Card Height:** 0.01
   - **Face Down Rotation:** (0, 180, 0)
   - **Face Up Rotation:** (0, 0, 0)

## Step 4: Update CommunityCards GameObject

1. **Find `CommunityBoardDisplay` in Hierarchy:**
   - This is the GameObject with the `CommunityCards` script

2. **In Inspector, find `Community Cards` component:**
   - **Card Renderer:** Drag the `CardRenderer3D` GameObject (from Step 3) into this field
   - **Community Cards Position:** Set to where you want cards on the table
     - Example: (0, 0.01, 0) for center of table
     - Adjust X, Z to position cards where you want them

## Step 5: Test

1. **Enter Play mode**
2. **Start Host and Client**
3. **Play a hand** - when the flop is dealt, you should see 3D cards appear!

## Troubleshooting

### Cards don't appear:
- Check that `CardRenderer3D` component has `Card Model Prefab` assigned
- Check that textures are in the correct folder
- Check Console for errors about missing textures
- Verify `CommunityCards` has `Card Renderer` assigned

### Cards appear but have wrong textures:
- Check texture file names match the pattern: `PlayingCards_{Suit}_{Rank}_Dif_tga.png`
- Verify all 52 card textures are present
- Check Console for warnings about missing textures

### Cards are in wrong position:
- Adjust `Community Cards Position` in `CommunityCards` component
- Adjust `Card Spacing` in `CardRenderer3D` component
- Cards spawn relative to `communityCardsPosition`

### Textures not loading:
- Make sure textures are in `Assets/Resources/CardAssets/playing card images/` folder
- Check that texture names match exactly (case-sensitive)
- Verify `Textures Folder Path` is set to: `CardAssets/playing card images`

## Next Steps

Once community cards are working, we can:
1. Add player cards (face up for you, face down for opponents)
2. Add card animations (flip, deal)
3. Position cards at player seats


