using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

/// <summary>
/// Style settings for UI text elements. Configure these in the Inspector to control appearance.
/// </summary>
[System.Serializable]
public class UITextStyle
{
    [Tooltip("Font size for the text")]
    public int fontSize = 24;
    
    [Tooltip("Text color")]
    public Color color = Color.white;
    
    [Tooltip("Anchor position (0-1 range, where 0.5,0.5 is center)")]
    public Vector2 anchorPosition = new Vector2(0.1f, 0.8f);
    
    [Tooltip("Size of the text element (width, height)")]
    public Vector2 size = new Vector2(200, 30);
    
    [Tooltip("Text alignment")]
    public TextAlignmentOptions alignment = TextAlignmentOptions.Left;
}

/// <summary>
/// UI controller for the poker table. Shows game state, player info, and action buttons.
/// This is a local script (not networked) - each player sees their own UI.
/// </summary>
public class PokerTableUI : MonoBehaviour
{
    [Header("UI References (Auto-Created - Do Not Assign in Inspector)")]
    [Tooltip("These text elements are created programmatically at runtime. Do NOT assign them in Inspector - they will be auto-created.")]
    public TextMeshProUGUI gameStateText;
    public TextMeshProUGUI potText;
    public TextMeshProUGUI currentBetText;
    public TextMeshProUGUI yourChipsText;
    public TextMeshProUGUI yourCardsText;
    public TextMeshProUGUI turnIndicatorText;

    [Header("Action Buttons (Must Be Assigned in Inspector)")]
    public Button foldButton;
    public Button checkButton;
    public Button callButton;
    public Button betButton;
    public Button raiseButton;

    [Header("Betting UI (Must Be Assigned in Inspector)")]
    [Tooltip("betSlider is hidden at runtime but should exist in scene. betAmountText is optional.")]
    public Slider betSlider;
    public TextMeshProUGUI betAmountText;
    [Tooltip("The active betting input field - MUST be assigned in Inspector")]
    public TMP_InputField betInputField;

    [Header("UI Text Styling (Inspector-Configurable)")]
    [Tooltip("Style settings for Pot Text")]
    public UITextStyle potTextStyle = new UITextStyle { fontSize = 24, color = Color.white, anchorPosition = new Vector2(0.15f, 0.65f), size = new Vector2(200, 30) };
    
    [Tooltip("Style settings for Current Bet Text")]
    public UITextStyle currentBetTextStyle = new UITextStyle { fontSize = 24, color = Color.white, anchorPosition = new Vector2(0.15f, 0.7f), size = new Vector2(200, 30) };
    
    [Tooltip("Style settings for Your Chips Text")]
    public UITextStyle yourChipsTextStyle = new UITextStyle { fontSize = 24, color = Color.white, anchorPosition = new Vector2(0.1f, 0.7f), size = new Vector2(200, 30) };
    
    [Tooltip("Style settings for Game State Text")]
    public UITextStyle gameStateTextStyle = new UITextStyle { fontSize = 24, color = Color.white, anchorPosition = new Vector2(0.5f, 0.95f), size = new Vector2(300, 30) };
    
    [Tooltip("Style settings for Your Cards Text")]
    public UITextStyle yourCardsTextStyle = new UITextStyle { fontSize = 24, color = Color.white, anchorPosition = new Vector2(0.1f, 0.65f), size = new Vector2(300, 30) };
    
    [Tooltip("Style settings for Turn Indicator Text")]
    public UITextStyle turnIndicatorTextStyle = new UITextStyle { fontSize = 32, color = Color.green, anchorPosition = new Vector2(0.5f, 0.85f), size = new Vector2(300, 40), alignment = TextAlignmentOptions.Center };

    private PokerGameManager _gameManager;
    private PokerPlayer _localPlayer;
    private NetworkRunner _runner;
    private bool _wasMyTurn = false; // Track turn changes to set default value only once

    private void Start()
    {
        // Find the game manager
        _gameManager = FindAnyObjectByType<PokerGameManager>();
        
        // Find the network runner
        _runner = FindAnyObjectByType<NetworkRunner>();

        // Clean up any existing test text (if it exists from previous runs)
        GameObject existingTest = GameObject.Find("TEST_TEXT_VISIBILITY");
        if (existingTest != null)
        {
            Destroy(existingTest);
        }

        // Create text elements in Start() - same approach as test text that works
        // This ensures they're created once, not conditionally in UpdateUI
        CreateUITextElements();

        // Setup button listeners
        if (foldButton != null) foldButton.onClick.AddListener(OnFoldClicked);
        if (checkButton != null) checkButton.onClick.AddListener(OnCheckClicked);
        if (callButton != null) callButton.onClick.AddListener(OnCallClicked);
        if (betButton != null) betButton.onClick.AddListener(OnBetClicked);
        if (raiseButton != null) raiseButton.onClick.AddListener(OnRaiseClicked);

        // Hide the slider (we're using input field now)
        if (betSlider != null)
        {
            betSlider.gameObject.SetActive(false);
        }

        // Setup betting input field
        if (betInputField != null)
        {
            betInputField.onEndEdit.AddListener(OnBetInputChanged);
            betInputField.gameObject.SetActive(true); // Ensure it's visible
            
            // Make sure the input field is visible
            var image = betInputField.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                var color = image.color;
                color.a = 1f; // Full opacity
                image.color = color;
            }
            
            // Fix text area positioning - make it fill the input field
            var textArea = betInputField.transform.Find("Text Area");
            if (textArea != null)
            {
                var rectTransform = textArea.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Set anchors to stretch and fill
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = new Vector2(5, 5); // Left, Bottom padding
                    rectTransform.offsetMax = new Vector2(-5, -5); // Right, Top padding
                }
            }
        }
        else
        {
            Debug.LogWarning("[PokerTableUI] Bet input field is NULL! Make sure it's assigned in Inspector.");
        }

        // Verify all required UI elements are assigned
        VerifyUIAssignments();

        // IGNORE Inspector assignments - create all text programmatically to avoid build issues
        // This is a known Unity issue where Inspector-assigned TextMeshPro doesn't work in builds
        // Since programmatic creation works (test text proves it), we'll create everything that way

        // Force Canvas to be active and visible - try multiple methods to find it
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            // Try finding it in the scene if not in parent hierarchy
            canvas = FindAnyObjectByType<Canvas>();
        }
        if (canvas == null)
        {
            // Try finding by name
            GameObject canvasObj = GameObject.Find("MainCanvas");
            if (canvasObj != null)
            {
                canvas = canvasObj.GetComponent<Canvas>();
            }
        }
        
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
            canvas.enabled = true;
            
            // Ensure Canvas is in Screen Space - Overlay mode (most reliable)
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            // Check for Canvas Group that might be blocking visibility
            CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            Debug.LogError("[PokerTableUI] NO CANVAS FOUND! Text will not be visible! Make sure PokerTableUI is a child of Canvas or Canvas exists in scene.");
        }
    }

    private void Update()
    {
        // Update UI every frame (you could optimize this to only update when values change)
        UpdateUI();
    }

    /// <summary>
    /// Updates all UI elements based on current game state.
    /// </summary>
    private void UpdateUI()
    {
        // Retry finding game manager if not found yet (it might spawn after this script)
        if (_gameManager == null)
        {
            _gameManager = FindAnyObjectByType<PokerGameManager>();
            // Don't return early - create text elements even if game manager not found yet
            // This ensures text appears on client even if networking is slow
        }

        // Diagnostic section removed - no longer needed
        
        // Retry finding network runner if not found yet
        if (_runner == null)
        {
            _runner = FindAnyObjectByType<NetworkRunner>();
        }

        // Find the local player
        if (_localPlayer == null && _runner != null)
        {
            PokerPlayer[] players = FindObjectsByType<PokerPlayer>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.Object.HasInputAuthority)
                {
                    _localPlayer = player;
                    break;
                }
            }
        }

        // Update game state text (hide completely - not needed for gameplay)
        if (gameStateText != null)
        {
            // Hide state text completely - players don't need to see this
            gameStateText.text = "";
        }

        // Update pot - text created in Start(), just update value
        if (potText != null)
        {
            int potValue = _gameManager != null ? _gameManager.Pot : 0;
            potText.text = $"Pot: {potValue}";
        }

        // Update current bet - text created in Start(), just update value
        if (currentBetText != null)
        {
            string betTextStr = _localPlayer != null 
                ? $"Your Bet: {_localPlayer.CurrentBet}" 
                : $"Current Bet: {(_gameManager != null ? _gameManager.CurrentBet : 0)}";
            currentBetText.text = betTextStr;
        }

        // Update local player info - text created in Start(), just update value
        if (yourChipsText != null)
        {
            string chipsTextStr = _localPlayer != null 
                ? $"Your Chips: {_localPlayer.Chips}" 
                : "Your Chips: --";
            yourChipsText.text = chipsTextStr;
        }
        
        if (_localPlayer != null)
        {

            if (yourCardsText != null)
            {
                // Hide text - cards are now visual (handled by CardVisualizationManager)
                yourCardsText.text = "";
            }
        }
        else
        {
            // Chips text already handled above
        }

        // Update turn indicator
        if (turnIndicatorText != null)
        {
            // Only show turn indicator if game has started
            if (_gameManager != null && _gameManager.CurrentState != PokerGameManager.GameState.WaitingForPlayers)
            {
                // Check if player won (opponent folded)
                if (_localPlayer != null && _gameManager.WinnerRef != PlayerRef.None && 
                    _localPlayer.Object.InputAuthority == _gameManager.WinnerRef)
                {
                    turnIndicatorText.text = "YOU WIN!";
                    turnIndicatorText.color = Color.yellow;
                }
                // Check if it's this player's turn
                else if (_localPlayer != null)
                {
                    bool isMyTurn = _gameManager.IsPlayerTurn(_localPlayer);
                    turnIndicatorText.text = isMyTurn ? "YOUR TURN!" : "";
                    turnIndicatorText.color = isMyTurn ? Color.green : Color.white;
                }
                else
                {
                    turnIndicatorText.text = "";
                }
            }
            else
            {
                turnIndicatorText.text = ""; // Hide until game starts
            }
        }

        // Update button states
        UpdateButtonStates();
    }

    /// <summary>
    /// Updates button states based on game state and player's situation.
    /// </summary>
    private void UpdateButtonStates()
    {
        if (_localPlayer == null || _gameManager == null) return;

        // Use the same turn check as the turn indicator (uses CurrentPlayerRef)
        bool isMyTurn = _gameManager.IsPlayerTurn(_localPlayer);
        bool canCheck = _localPlayer.CurrentBet >= _gameManager.CurrentBet;
        int callAmount = _gameManager.CurrentBet - _localPlayer.CurrentBet;
        bool canCall = callAmount > 0 && callAmount <= _localPlayer.Chips;

        // Enable/disable buttons based on turn and game state
        if (foldButton != null) foldButton.interactable = isMyTurn && !_localPlayer.IsFolded;
        if (checkButton != null) checkButton.interactable = isMyTurn && canCheck;
        if (callButton != null) callButton.interactable = isMyTurn && canCall;
        if (betButton != null) betButton.interactable = isMyTurn && _gameManager.CurrentBet == 0; // Can only bet if no one has bet yet
        if (raiseButton != null) raiseButton.interactable = isMyTurn && _gameManager.CurrentBet > 0; // Can raise if someone has bet
        
        // Enable/disable input field when it's player's turn
        if (betInputField != null)
        {
            betInputField.interactable = isMyTurn;
            betInputField.gameObject.SetActive(true); // Always visible, just enabled/disabled
            
            // Set default value ONLY when turn changes to player (not every frame)
            if (isMyTurn && !_wasMyTurn)
            {
                // Turn just changed to this player - set default value once
                int defaultValue = 0;
                if (_gameManager.CurrentBet == 0)
                {
                    // No bet yet - default to big blind
                    defaultValue = _gameManager.BigBlind;
                }
                else
                {
                    // Someone has bet - default to minimum raise
                    defaultValue = _gameManager.CurrentBet + _gameManager.BigBlind;
                }
                
                betInputField.text = defaultValue.ToString();
            }
            
            // Update turn tracking
            _wasMyTurn = isMyTurn;
        }
        else
        {
            _wasMyTurn = false;
        }
        
        // Ensure slider is hidden
        if (betSlider != null)
        {
            betSlider.gameObject.SetActive(false);
        }

        // Update bet input field (no longer using slider)
        // The input field allows players to type in their bet amount directly
    }

    // Button click handlers
    private void OnFoldClicked()
    {
        if (_localPlayer != null)
        {
            _localPlayer.MakeAction(PokerPlayer.PlayerAction.Fold);
        }
    }

    private void OnCheckClicked()
    {
        if (_localPlayer != null)
        {
            _localPlayer.MakeAction(PokerPlayer.PlayerAction.Check);
        }
    }

    private void OnCallClicked()
    {
        if (_localPlayer != null)
        {
            _localPlayer.MakeAction(PokerPlayer.PlayerAction.Call);
        }
    }

    private void OnBetClicked()
    {
        if (_localPlayer != null && betInputField != null && _gameManager != null)
        {
            string inputText = betInputField.text.Trim();
            if (string.IsNullOrEmpty(inputText)) return;
            
            if (int.TryParse(inputText, out int betAmount))
            {
                // Minimum bet is the big blind amount
                int minBet = _gameManager.BigBlind;
                int maxBet = _localPlayer.Chips + _localPlayer.CurrentBet;
                
                // Validate and use the exact amount (don't clamp silently - let server validate)
                if (betAmount < minBet)
                {
                    Debug.LogWarning($"[PokerTableUI] Bet amount {betAmount} is less than minimum {minBet}. Using minimum.");
                    betAmount = minBet;
                }
                else if (betAmount > maxBet)
                {
                    Debug.LogWarning($"[PokerTableUI] Bet amount {betAmount} exceeds maximum {maxBet}. Using maximum.");
                    betAmount = maxBet;
                }
                
                _localPlayer.MakeAction(PokerPlayer.PlayerAction.Bet, betAmount);
            }
            else
            {
                Debug.LogWarning($"[PokerTableUI] Could not parse bet amount: {inputText}");
            }
        }
    }

    private void OnRaiseClicked()
    {
        if (_localPlayer != null && betInputField != null && _gameManager != null)
        {
            string inputText = betInputField.text.Trim();
            if (string.IsNullOrEmpty(inputText)) return;
            
            if (int.TryParse(inputText, out int betAmount))
            {
                // Minimum raise is current bet + big blind amount (standard poker rule)
                int minRaise = _gameManager.CurrentBet + _gameManager.BigBlind;
                int maxBet = _localPlayer.Chips + _localPlayer.CurrentBet;
                
                // Validate and use the exact amount (don't clamp silently - let server validate)
                if (betAmount < minRaise)
                {
                    Debug.LogWarning($"[PokerTableUI] Raise amount {betAmount} is less than minimum {minRaise}. Using minimum.");
                    betAmount = minRaise;
                }
                else if (betAmount > maxBet)
                {
                    Debug.LogWarning($"[PokerTableUI] Raise amount {betAmount} exceeds maximum {maxBet}. Using maximum.");
                    betAmount = maxBet;
                }
                
                _localPlayer.MakeAction(PokerPlayer.PlayerAction.Raise, betAmount);
            }
            else
            {
                Debug.LogWarning($"[PokerTableUI] Could not parse raise amount: {inputText}");
            }
        }
    }

    private void OnBetSliderChanged(float value)
    {
        // Slider is hidden, but keep this for backwards compatibility
        // Don't sync to input field anymore to avoid conflicts
        // This prevents the slider from overwriting what the user types
    }

    private void OnBetInputChanged(string value)
    {
        // Input field changed - no need to sync with slider anymore (slider is hidden)
        // This prevents conflicts between slider and input field
    }


    /// <summary>
    /// Creates a text element programmatically using Inspector-configured style settings.
    /// This ensures elements work in builds while allowing Inspector styling control.
    /// </summary>
    private TextMeshProUGUI CreateTextElement(string name, string initialText, UITextStyle style)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindAnyObjectByType<Canvas>();
        }
        
        if (canvas == null)
        {
            Debug.LogError($"[PokerTableUI] Cannot create {name} - no Canvas found!");
            return null;
        }
        
        // Check if already exists
        Transform existing = canvas.transform.Find(name);
        if (existing != null)
        {
            return existing.GetComponent<TextMeshProUGUI>();
        }
        
        // Create new text element using Inspector-configured style
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(canvas.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = initialText;
        
        // Apply Inspector-configured style (or use defaults if not set)
        text.fontSize = style.fontSize;
        text.color = style.color;
        text.alignment = style.alignment;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = style.anchorPosition;
        rect.anchorMax = style.anchorPosition;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = style.size;
        
        // Force visibility - same as test text
        textObj.SetActive(true);
        text.enabled = true;
        
        // Force CanvasRenderer
        CanvasRenderer canvasRenderer = textObj.GetComponent<CanvasRenderer>();
        if (canvasRenderer != null)
        {
            canvasRenderer.cull = false;
        }
        
        return text;
    }

    /// <summary>
    /// Creates all UI text elements in Start() using Inspector-configured styles.
    /// Elements are created programmatically (works in builds) but styled via Inspector.
    /// </summary>
    private void CreateUITextElements()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindAnyObjectByType<Canvas>();
        }
        
        // Clean up old scene-based text elements (if they exist in the scene)
        // These are the manually created ones that might conflict with programmatic ones
        string[] oldElementNames = { "PotText", "CurrentBetText", "YourChipsText", "GameStateText", "YourCardsText", "TurnIndicatorText" };
        if (canvas != null)
        {
            foreach (string oldName in oldElementNames)
            {
                Transform oldElement = canvas.transform.Find(oldName);
                if (oldElement != null)
                {
                    Destroy(oldElement.gameObject);
                }
            }
        }
        
        // ALWAYS create new ones - don't trust Inspector assignments in builds
        // Destroy any existing ones first (if assigned in Inspector)
        if (potText != null) Destroy(potText.gameObject);
        if (currentBetText != null) Destroy(currentBetText.gameObject);
        if (yourChipsText != null) Destroy(yourChipsText.gameObject);
        if (gameStateText != null) Destroy(gameStateText.gameObject);
        if (yourCardsText != null) Destroy(yourCardsText.gameObject);
        if (turnIndicatorText != null) Destroy(turnIndicatorText.gameObject);
        
        // Create fresh ones using Inspector-configured styles
        potText = CreateTextElement("PotText_Dynamic", "Pot: 0", potTextStyle);
        currentBetText = CreateTextElement("CurrentBetText_Dynamic", "Current Bet: 0", currentBetTextStyle);
        yourChipsText = CreateTextElement("YourChipsText_Dynamic", "Your Chips: --", yourChipsTextStyle);
        gameStateText = CreateTextElement("GameStateText_Dynamic", "State: Waiting...", gameStateTextStyle);
        yourCardsText = CreateTextElement("YourCardsText_Dynamic", "", yourCardsTextStyle);
        turnIndicatorText = CreateTextElement("TurnIndicatorText_Dynamic", "", turnIndicatorTextStyle);
    }

    /// <summary>
    /// Verifies that all required UI elements are assigned in the Inspector.
    /// Logs warnings for any missing assignments.
    /// </summary>
    private void VerifyUIAssignments()
    {
        bool allAssigned = true;
        
        // Check action buttons
        if (foldButton == null) { Debug.LogWarning("[PokerTableUI] foldButton is NOT assigned in Inspector!"); allAssigned = false; }
        if (checkButton == null) { Debug.LogWarning("[PokerTableUI] checkButton is NOT assigned in Inspector!"); allAssigned = false; }
        if (callButton == null) { Debug.LogWarning("[PokerTableUI] callButton is NOT assigned in Inspector!"); allAssigned = false; }
        if (betButton == null) { Debug.LogWarning("[PokerTableUI] betButton is NOT assigned in Inspector!"); allAssigned = false; }
        if (raiseButton == null) { Debug.LogWarning("[PokerTableUI] raiseButton is NOT assigned in Inspector!"); allAssigned = false; }
        
        // Check betting UI
        if (betSlider == null) { Debug.LogWarning("[PokerTableUI] betSlider is NOT assigned in Inspector! (This is OK if you're not using it)"); }
        if (betInputField == null) { Debug.LogWarning("[PokerTableUI] betInputField is NOT assigned in Inspector!"); allAssigned = false; }
        if (betAmountText == null) { Debug.LogWarning("[PokerTableUI] betAmountText is NOT assigned in Inspector! (This is OK if you're not using it)"); }
        
        if (!allAssigned)
        {
            Debug.LogError("[PokerTableUI] ✗ Some required UI elements are missing! Check the warnings above and assign them in the Inspector.");
        }
    }

}

