using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// REFACTORED VERSION - Addresses architecture review concerns.
/// 
/// Key improvements:
/// - Cached references (no FindAnyObjectByType in Update)
/// - Throttled updates (reduces CPU overhead)
/// - Proper lifecycle management
/// - Fusion Render() pattern support
/// </summary>
public class CardVisualizationManager : MonoBehaviour
{
    [Header("Card Assets - 2D Sprites")]
    [Tooltip("Path to card textures folder (relative to Resources). Textures will be converted to sprites at runtime.")]
    public string texturesFolderPath = "CardAssets/playing card images";
    
    [Tooltip("Card sprite size in world units. INSPECTOR-ADJUSTABLE: X = Width, Y = Height. Adjust X to change width only, Y to change height only.")]
    public Vector2 cardSize = new Vector2(2.0f, 2.8f); // X = width, Y = height
    
    [Tooltip("Crop white border from card sprites. INSPECTOR-ADJUSTABLE: Increase values to crop more white space. X = left/right crop, Y = top/bottom crop (in pixels).")]
    public Vector2 cropWhiteBorder = new Vector2(50f, 50f); // Pixels to crop from each side
    
    [Header("Card Positioning")]
    [Tooltip("Spacing between cards (should be larger than card width to prevent overlap)")]
    public float cardSpacing = 2.5f; // Increased for larger cards (cards are ~2.0 units wide)
    
    [Tooltip("Height above table for cards. INSPECTOR-ADJUSTABLE: Increase this if cards are clipped, decrease if too high. Pivot is 1/8 from bottom.")]
    public float cardHeight = 1.5f; // INSPECTOR: Adjust this value to show more/less of card
    
    [Tooltip("Position for community cards (center of flop). INSPECTOR-ADJUSTABLE: Increase Y value if cards are clipped.")]
    public Vector3 communityCardsPosition = new Vector3(0, 1.5f, 0);
    
    [Tooltip("Position for local player's hole cards. INSPECTOR-ADJUSTABLE: Increase Y value if cards are clipped.")]
    public Vector3 localPlayerCardsPosition = new Vector3(-0.3f, 1.5f, -0.3f);
    
    [Tooltip("Card rotation when face up")]
    public Vector3 faceUpRotation = Vector3.zero;
    
    [Tooltip("Card rotation when face down")]
    public Vector3 faceDownRotation = new Vector3(0, 180, 0);

    // Cached references (set once, never null-checked in Update)
    private PokerGameManager _gameManager;
    private CommunityCards _communityCards;
    private NetworkRunner _runner;
    
    // Player tracking (updated when players spawn/despawn)
    private PokerPlayer _localPlayer;
    private Dictionary<PlayerRef, PokerPlayer> _allPlayers = new Dictionary<PlayerRef, PokerPlayer>();
    
    // Card tracking
    private List<GameObject> _communityCardObjects = new List<GameObject>();
    private Dictionary<PlayerRef, List<GameObject>> _playerCardObjects = new Dictionary<PlayerRef, List<GameObject>>();
    
    // State tracking (for change detection)
    private int _lastFlop1 = -1, _lastFlop2 = -1, _lastFlop3 = -1;
    private int _lastTurn = -1, _lastRiver = -1;
    private Dictionary<PlayerRef, (int card1, int card2)> _lastPlayerCards = new Dictionary<PlayerRef, (int, int)>();
    
    // Sprite cache (2D approach - simple and reliable)
    private Dictionary<int, Sprite> _cardSprites = new Dictionary<int, Sprite>();
    private Sprite _cardBackSprite;
    private bool _spritesLoaded = false;
    
    // Update throttling (only update visuals every N seconds instead of every frame)
    private float _lastUpdateTime = 0f;
    private const float UPDATE_INTERVAL = 0.1f; // Update max 10 times per second instead of 60+

    private void Start()
    {
        // Cache references ONCE - never call FindAnyObjectByType in Update()
        CacheReferences();
        
        // Load sprites ONCE
        LoadCardSprites();
        
        // Warn if cardSize is too small (cards will be invisible)
        if (cardSize.x < 0.5f || cardSize.y < 0.5f)
        {
            Debug.LogWarning($"[CardVisualizationManager] cardSize is very small ({cardSize}). Cards may be tiny! Recommended: (2.0, 2.8) or larger.");
        }
        
        Debug.Log($"[CardVisualizationManager] Start() called. Sprites loaded: {_spritesLoaded}, cardSize: {cardSize}, Instance ID: {GetInstanceID()}");
    }

    /// <summary>
    /// Cache all references once at startup.
    /// </summary>
    private void CacheReferences()
    {
        _gameManager = FindAnyObjectByType<PokerGameManager>();
        _communityCards = FindAnyObjectByType<CommunityCards>();
        _runner = FindAnyObjectByType<NetworkRunner>();
        
        // References will be retried in Update() if null
    }
    
    /// <summary>
    /// Retry finding references if they weren't found at startup (they spawn via networking).
    /// </summary>
    private void RetryFindingReferences()
    {
        if (_gameManager == null)
        {
            _gameManager = FindAnyObjectByType<PokerGameManager>();
        }
        
        if (_runner == null)
        {
            _runner = FindAnyObjectByType<NetworkRunner>();
        }
    }

    /// <summary>
    /// Throttled update - only checks for changes periodically, not every frame.
    /// This reduces CPU overhead significantly.
    /// </summary>
    private void Update()
    {
        // Throttle updates to reduce CPU overhead
        if (Time.time - _lastUpdateTime < UPDATE_INTERVAL)
        {
            return;
        }
        _lastUpdateTime = Time.time;
        
        // Retry finding references if they weren't found at startup (they spawn via networking)
        RetryFindingReferences();
        
        // Update local player reference if needed (players can spawn after Start())
        UpdateLocalPlayerReference();
        
        // Check for changes and update visuals
        UpdateCommunityCards();
        UpdatePlayerCards();
    }

    /// <summary>
    /// Updates local player reference. Called periodically, not every frame.
    /// </summary>
    private void UpdateLocalPlayerReference()
    {
        if (_localPlayer != null && _runner != null)
        {
            // Verify player still has input authority
            if (_localPlayer.Object != null && _localPlayer.Object.HasInputAuthority)
            {
                return; // Still valid
            }
        }
        
        // Find local player (only if not found or lost reference)
        if (_runner != null)
        {
            PokerPlayer[] players = FindObjectsByType<PokerPlayer>(FindObjectsSortMode.None);
            
            foreach (var player in players)
            {
                if (player.Object != null && player.Object.HasInputAuthority)
                {
                    if (_localPlayer != player) // Only log when player changes
                    {
                        Debug.Log($"[CardVisualizationManager] Found local player {player.Object.InputAuthority} (Cards: {player.Card1}, {player.Card2})");
                    }
                    _localPlayer = player;
                    _allPlayers[player.Object.InputAuthority] = player;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// PREFERRED: Call this from a NetworkBehaviour's Render() method.
    /// This is the Fusion-recommended pattern for visual updates.
    /// </summary>
    public void OnNetworkStateChanged()
    {
        UpdateCommunityCards();
        UpdatePlayerCards();
    }

    /// <summary>
    /// Updates community cards visualization.
    /// </summary>
    private void UpdateCommunityCards()
    {
        if (_communityCards == null) return;
        
        // CRITICAL: Networked properties can only be accessed after Spawned() is called
        if (_communityCards.Object == null || !_communityCards.Object.IsValid)
        {
            return; // Not spawned yet, skip this update
        }

        // Check if cards changed
        bool changed = (_communityCards.Flop1 != _lastFlop1 ||
                       _communityCards.Flop2 != _lastFlop2 ||
                       _communityCards.Flop3 != _lastFlop3 ||
                       _communityCards.Turn != _lastTurn ||
                       _communityCards.River != _lastRiver);
        
        if (!changed) return;

        // Destroy old cards FIRST to prevent duplicates
        if (_communityCardObjects.Count > 0)
        {
            DestroyCards(_communityCardObjects);
            _communityCardObjects.Clear();
        }

        // Create flop ONLY if all 3 cards are valid (>= 0)
        if (_communityCards.Flop1 >= 0 && _communityCards.Flop2 >= 0 && _communityCards.Flop3 >= 0)
        {
            int[] flop = { _communityCards.Flop1, _communityCards.Flop2, _communityCards.Flop3 };
            List<GameObject> flopCards = CreateCardRow(flop, communityCardsPosition, true);
            _communityCardObjects.AddRange(flopCards);
        }

        // Create turn
        if (_communityCards.Turn >= 0)
        {
            Vector3 turnPos = communityCardsPosition + Vector3.right * (3 * cardSpacing);
            GameObject turnCard = CreateCard(_communityCards.Turn, turnPos, true);
            if (turnCard != null) _communityCardObjects.Add(turnCard);
        }

        // Create river
        if (_communityCards.River >= 0)
        {
            Vector3 riverPos = communityCardsPosition + Vector3.right * (4 * cardSpacing);
            GameObject riverCard = CreateCard(_communityCards.River, riverPos, true);
            if (riverCard != null) _communityCardObjects.Add(riverCard);
        }

        // Update tracking
        _lastFlop1 = _communityCards.Flop1;
        _lastFlop2 = _communityCards.Flop2;
        _lastFlop3 = _communityCards.Flop3;
        _lastTurn = _communityCards.Turn;
        _lastRiver = _communityCards.River;
    }

    /// <summary>
    /// Updates player hole cards visualization.
    /// </summary>
    private void UpdatePlayerCards()
    {
        if (_localPlayer == null)
        {
            // DIAGNOSTIC: Log why we're skipping (only once per frame to avoid spam)
            if (Time.frameCount % 60 == 0) // Log once per second
            {
                Debug.Log($"[CardVisualizationManager] UpdatePlayerCards: _localPlayer is null. Runner: {_runner != null}, Sprites: {_spritesLoaded}");
            }
            return;
        }
        
        // CRITICAL: Networked properties can only be accessed after Spawned() is called
        if (_localPlayer.Object == null || !_localPlayer.Object.IsValid)
        {
            return; // Not spawned yet, skip this update
        }

        // Check if local player cards changed
        int currentCard1 = _localPlayer.Card1;
        int currentCard2 = _localPlayer.Card2;
        
        
        PlayerRef playerRef = _localPlayer.Object.InputAuthority;
        bool hasLastState = _lastPlayerCards.ContainsKey(playerRef);
        bool changed = !hasLastState || 
                      _lastPlayerCards[playerRef].card1 != currentCard1 || 
                      _lastPlayerCards[playerRef].card2 != currentCard2;

        if (changed)
        {
            // Destroy old cards FIRST to prevent duplicates
            if (_playerCardObjects.ContainsKey(playerRef))
            {
                DestroyCards(_playerCardObjects[playerRef]);
                _playerCardObjects[playerRef].Clear();
            }
            else
            {
                _playerCardObjects[playerRef] = new List<GameObject>();
            }

            // Create new local player cards (face up - you can see your own cards)
            int[] cards = { currentCard1, currentCard2 };
            
            // Use Inspector values directly (no auto-fix)
            List<GameObject> playerCards = CreateCardRow(cards, localPlayerCardsPosition, true);
            _playerCardObjects[playerRef].AddRange(playerCards);
            
            Debug.Log($"[CardVisualizationManager] Created {playerCards.Count} cards (IDs: {currentCard1}, {currentCard2}) at positions spaced {cardSpacing:F2} units apart");

            // Update tracking
            _lastPlayerCards[playerRef] = (currentCard1, currentCard2);
        }
        // Removed unchanged logging - too verbose
    }

    /// <summary>
    /// Loads all card textures and converts them to sprites.
    /// 2D approach: Simple, reliable, and easy to upgrade to 3D later.
    /// </summary>
    private void LoadCardSprites()
    {
        // Clear existing sprites
        if (_spritesLoaded)
        {
            _cardSprites.Clear();
            _cardBackSprite = null;
            _spritesLoaded = false;
        }

        // Load ALL textures at once
        Texture2D[] allTextures = Resources.LoadAll<Texture2D>(texturesFolderPath);
        
        if (allTextures.Length == 0)
        {
            Debug.LogError($"[CardVisualizationManager] No textures found! Check Resources/{texturesFolderPath}/");
            return;
        }

        // Create a dictionary of texture names for quick lookup
        Dictionary<string, Texture2D> textureLookup = new Dictionary<string, Texture2D>();
        foreach (Texture2D tex in allTextures)
        {
            textureLookup[tex.name] = tex;
        }

        // Load card back sprite
        Texture2D backTexture = null;
        string[] backNames = { 
            "PlayingCards_Back_Dif.tga",
            "PlayingCards_Back_Dif", 
            "PlayingCards_Back_Dif_tga", 
            "PlayingCards_Back" 
        };
        foreach (string name in backNames)
        {
            if (textureLookup.ContainsKey(name))
            {
                backTexture = textureLookup[name];
                break;
            }
        }
        
        if (backTexture != null)
        {
            _cardBackSprite = TextureToSprite(backTexture);
        }
        else
        {
            Debug.LogError($"[CardVisualizationManager] Card back texture not found!");
        }

        // Load all 52 cards and convert to sprites
        int loadedCount = 0;
        for (int cardID = 0; cardID < 52; cardID++)
        {
            string baseName = GetTextureNameForCard(cardID);
            
            // Try variations (Unity keeps .tga extension in texture names)
            string[] nameVariations = {
                baseName + ".tga",
                baseName,
                baseName.Replace("_Dif_tga", ".tga"),
                baseName.Replace("_tga", ".tga")
            };
            
            Texture2D texture = null;
            foreach (string name in nameVariations)
            {
                if (textureLookup.ContainsKey(name))
                {
                    texture = textureLookup[name];
                    break;
                }
            }
            
            if (texture != null)
            {
                _cardSprites[cardID] = TextureToSprite(texture);
                loadedCount++;
            }
        }

        _spritesLoaded = true;
        
        if (loadedCount == 0 || _cardBackSprite == null)
        {
            Debug.LogError($"[CardVisualizationManager] Sprite loading failed! Loaded {loadedCount}/52 cards, Back: {_cardBackSprite != null}. Check Resources/{texturesFolderPath}/");
        }
        else
        {
            Debug.Log($"[CardVisualizationManager] ✓ Sprites loaded successfully: {loadedCount}/52 cards + back. Ready to render cards.");
        }
    }
    
    /// <summary>
    /// Converts a Texture2D to a Sprite for 2D rendering.
    /// Crops white background using Inspector settings.
    /// </summary>
    private Sprite TextureToSprite(Texture2D texture)
    {
        if (texture == null) return null;
        
        // Use high pixelsPerUnit so sprites are initially small, then we scale them up
        // For 1024px texture: 1000 pixelsPerUnit = ~1 world unit, which we'll scale to cardSize
        float pixelsPerUnit = 1000f;
        
        // Crop white border using Inspector values
        float cropX = Mathf.Clamp(cropWhiteBorder.x, 0, texture.width / 2);
        float cropY = Mathf.Clamp(cropWhiteBorder.y, 0, texture.height / 2);
        
        Rect cardBounds = new Rect(
            cropX,                                    // Start X (crop from left)
            cropY,                                    // Start Y (crop from bottom)
            texture.width - (cropX * 2),             // Width (crop from both sides)
            texture.height - (cropY * 2)              // Height (crop from top and bottom)
        );
        
        // Create sprite from texture (cropped to remove white border, 1/8 from bottom pivot)
        return Sprite.Create(
            texture,
            cardBounds,
            new Vector2(0.5f, 0.125f), // Center X, 1/8 from bottom Y
            pixelsPerUnit
        );
    }
    

    /// <summary>
    /// Creates a single card GameObject using 2D sprites.
    /// Clean, simple, and reliable. Easy to upgrade to 3D later by replacing this method.
    /// </summary>
    private GameObject CreateCard(int cardID, Vector3 position, bool faceUp)
    {
        // Ensure sprites are loaded
        if (!_spritesLoaded)
        {
            LoadCardSprites();
            if (!_spritesLoaded)
            {
                Debug.LogError($"[CardVisualizationManager] Card {cardID}: Failed to load sprites!");
                return null;
            }
        }
        
        // Get sprite for this card
        Sprite cardSprite = faceUp && cardID >= 0 && cardID < 52
            ? _cardSprites.GetValueOrDefault(cardID)
            : _cardBackSprite;

        if (cardSprite == null)
        {
            Debug.LogError($"[CardVisualizationManager] Card {cardID}: Sprite not found! FaceUp: {faceUp}");
            return null;
        }

        // Create GameObject with SpriteRenderer
        GameObject card = new GameObject(faceUp ? $"Card_{cardID}" : "Card_Back");
        card.transform.position = position;
        
        // Add SpriteRenderer component
        SpriteRenderer spriteRenderer = card.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = cardSprite;
        spriteRenderer.sortingOrder = 10; // Ensure cards render above table
        
        // Set card size - Use Inspector values directly (no auto-fix, full user control)
        // With pixelsPerUnit=1000, a 1024px texture is ~1.024 world units
        // Scale to make it the desired size
        float pixelsPerUnit = cardSprite.pixelsPerUnit;
        float currentWorldWidth = cardSprite.texture.width / pixelsPerUnit;
        float currentWorldHeight = cardSprite.texture.height / pixelsPerUnit;
        
        // Calculate scale - use Inspector values directly (non-uniform scaling for independent width/height)
        float scaleX = cardSize.x / currentWorldWidth;
        float scaleY = cardSize.y / currentWorldHeight;
        
        // Use non-uniform scaling so X (width) and Y (height) can be adjusted independently
        card.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        
        // For 2D sprites in 3D space, position them flat on the table (facing up)
        // Rotate 90 degrees on X axis to lay flat
        // Flip Y rotation 180 degrees to fix mirror image issue
        card.transform.rotation = Quaternion.Euler(90f, faceUp ? 180f : 0f, 0f);
        
        // Apply any additional rotation from Inspector
        Vector3 additionalRotation = faceUp ? faceUpRotation : faceDownRotation;
        card.transform.Rotate(additionalRotation);

        return card;
    }

    /// <summary>
    /// Creates multiple cards in a row.
    /// </summary>
    private List<GameObject> CreateCardRow(int[] cardIDs, Vector3 startPosition, bool faceUp, float spacing = -1f)
    {
        List<GameObject> cards = new List<GameObject>();
        
        // Use provided spacing, or fall back to class field
        float actualSpacing = spacing > 0 ? spacing : cardSpacing;

        for (int i = 0; i < cardIDs.Length; i++)
        {
            if (cardIDs[i] < 0) continue;

            Vector3 pos = startPosition + Vector3.right * (i * actualSpacing);
            pos.y = startPosition.y + cardHeight; // Keep Y position, add height offset
            
            GameObject card = CreateCard(cardIDs[i], pos, faceUp);
            if (card != null) cards.Add(card);
        }

        return cards;
    }

    /// <summary>
    /// Destroys a list of card GameObjects.
    /// </summary>
    private void DestroyCards(List<GameObject> cards)
    {
        if (cards == null) return;
        foreach (GameObject card in cards)
        {
            if (card != null) Destroy(card);
        }
        cards.Clear();
    }

    /// <summary>
    /// Gets texture name for a card ID.
    /// </summary>
    private string GetTextureNameForCard(int cardID)
    {
        if (cardID < 0 || cardID > 51) return null;

        string[] suits = { "Spades", "Hearts", "Diamonds", "Clubs" };
        string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

        int suitIndex = cardID / 13;
        int rankIndex = cardID % 13;

        return $"PlayingCards_{suits[suitIndex]}_{ranks[rankIndex]}_Dif_tga";
    }



    /// <summary>
    /// Cleanup on destroy - prevents memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        // Destroy all spawned cards
        DestroyCards(_communityCardObjects);
        
        foreach (var playerCards in _playerCardObjects.Values)
        {
            DestroyCards(playerCards);
        }
        _playerCardObjects.Clear();
        
        // Clear sprite cache (sprites will be garbage collected)
        _cardSprites.Clear();
        _cardBackSprite = null;
        
        Debug.Log("[CardVisualizationManager] Cleaned up on destroy");
    }
}
