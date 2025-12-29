using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

/// <summary>
/// UI controller for the poker table. Shows game state, player info, and action buttons.
/// This is a local script (not networked) - each player sees their own UI.
/// </summary>
public class PokerTableUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI gameStateText;
    public TextMeshProUGUI potText;
    public TextMeshProUGUI currentBetText;
    public TextMeshProUGUI yourChipsText;
    public TextMeshProUGUI yourCardsText;
    public TextMeshProUGUI turnIndicatorText;

    [Header("Action Buttons")]
    public Button foldButton;
    public Button checkButton;
    public Button callButton;
    public Button betButton;
    public Button raiseButton;

    [Header("Betting UI")]
    public Slider betSlider;
    public TextMeshProUGUI betAmountText;
    public TMP_InputField betInputField;

    private PokerGameManager _gameManager;
    private PokerPlayer _localPlayer;
    private NetworkRunner _runner;
    private bool _wasMyTurn = false; // Track turn changes to set default value only once
    private bool _hasLoggedDiagnostics = false; // Track if we've logged diagnostic info

    private void Start()
    {
        // Find the game manager
        _gameManager = FindAnyObjectByType<PokerGameManager>();
        
        // Find the network runner
        _runner = FindAnyObjectByType<NetworkRunner>();

        // CRITICAL FIX: Create a test text element to verify Canvas is working
        // This will help us see if the issue is Canvas rendering or text element specific
        CreateTestTextElement();
        
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
            Debug.Log("[PokerTableUI] Bet slider hidden");
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
                    Debug.Log("[PokerTableUI] Fixed Text Area positioning to fill input field");
                }
            }
            
            Debug.Log($"[PokerTableUI] Bet input field set up. Position: {betInputField.transform.position}, Active: {betInputField.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning("[PokerTableUI] Bet input field is NULL! Make sure it's assigned in Inspector.");
        }

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
                Debug.Log("[PokerTableUI] Changed Canvas to ScreenSpaceOverlay mode");
            }

            // Check for Canvas Group that might be blocking visibility
            CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            Debug.Log($"[PokerTableUI] Canvas found: {canvas.name}, Active: {canvas.gameObject.activeSelf}, Enabled: {canvas.enabled}");
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

        // Log diagnostic info once when game manager is first found
        if (!_hasLoggedDiagnostics && _gameManager != null)
        {
            _hasLoggedDiagnostics = true;
            Debug.Log("[PokerTableUI] ===== DIAGNOSTIC INFO (Client) =====");
            
            // Check Canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"[PokerTableUI] Canvas: {canvas.name}, Active: {canvas.gameObject.activeSelf}, Enabled: {canvas.enabled}, RenderMode: {canvas.renderMode}");
            }
            else
            {
                Debug.LogWarning("[PokerTableUI] NO CANVAS FOUND in parent hierarchy!");
            }

            // Check each text element
            CheckTextElement(potText, "PotText", canvas);
            CheckTextElement(currentBetText, "CurrentBetText", canvas);
            CheckTextElement(yourChipsText, "YourChipsText", canvas);
            CheckTextElement(gameStateText, "GameStateText", canvas);
            
            Debug.Log("[PokerTableUI] ===== END DIAGNOSTIC INFO =====");
        }
        
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

        // Update game state text
        if (gameStateText != null)
        {
            string stateText = _gameManager != null ? $"State: {_gameManager.CurrentState}" : "State: Waiting...";
            gameStateText.text = stateText;
            // Debug log first few times to verify it's working
            if (Time.frameCount < 10)
            {
                Debug.Log($"[PokerTableUI] Setting gameStateText to: {stateText}");
            }
        }
        else
        {
            if (Time.frameCount < 10)
            {
                Debug.LogWarning("[PokerTableUI] gameStateText is null!");
            }
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
                if (_localPlayer.Card1 >= 0 && _localPlayer.Card2 >= 0)
                {
                    yourCardsText.text = $"Your Cards: {GetCardName(_localPlayer.Card1)}, {GetCardName(_localPlayer.Card2)}";
                }
                else
                {
                    // Only show waiting message if game has actually started
                    if (_gameManager != null && _gameManager.CurrentState != PokerGameManager.GameState.WaitingForPlayers)
                    {
                        yourCardsText.text = "Waiting for cards...";
                    }
                    else
                    {
                        yourCardsText.text = ""; // Hide until game starts
                    }
                }
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
        
        // Additional safety check
        if (_gameManager == null) return;

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
                Debug.Log($"[PokerTableUI] Turn changed - Set default bet value to {defaultValue} (CurrentBet: {_gameManager.CurrentBet}, BigBlind: {_gameManager.BigBlind}, SmallBlind: {_gameManager.SmallBlind})");
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
                
                Debug.Log($"[PokerTableUI] Betting {betAmount} (input was: {inputText})");
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
                
                Debug.Log($"[PokerTableUI] Raising to {betAmount} (input was: {inputText}, current bet: {_gameManager.CurrentBet})");
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
    /// Checks and logs diagnostic info for a text element.
    /// </summary>
    private void CheckTextElement(TextMeshProUGUI textElement, string name, Canvas canvas)
    {
        if (textElement != null)
        {
            RectTransform rect = textElement.GetComponent<RectTransform>();
            if (rect != null)
            {
                string canvasName = canvas != null ? canvas.name : "NO CANVAS";
                Vector2 screenPos = canvas != null && canvas.worldCamera != null 
                    ? RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rect.position)
                    : Vector2.zero;
                
                Debug.Log($"[PokerTableUI] {name} - Active: {textElement.gameObject.activeSelf}, " +
                         $"Enabled: {textElement.enabled}, Color Alpha: {textElement.color.a}, " +
                         $"Position: {rect.position}, Size: {rect.sizeDelta}, " +
                         $"AnchoredPos: {rect.anchoredPosition}, ScreenPos: {screenPos}, " +
                         $"Canvas: {canvasName}, Text: '{textElement.text}'");
            }
        }
        else
        {
            Debug.LogWarning($"[PokerTableUI] {name} is NULL!");
        }
    }

    /// <summary>
    /// Ensures a text element is visible and active.
    /// </summary>
    private void EnsureTextVisibility(TextMeshProUGUI textElement, string name)
    {
        if (textElement != null)
        {
            // Ensure GameObject is active
            if (!textElement.gameObject.activeSelf)
            {
                textElement.gameObject.SetActive(true);
                Debug.Log($"[PokerTableUI] Activated {name} GameObject");
            }
            
            // Ensure text is visible (color alpha > 0)
            if (textElement.color.a < 0.1f)
            {
                var color = textElement.color;
                color.a = 1f; // Full opacity
                textElement.color = color;
                Debug.Log($"[PokerTableUI] Fixed {name} color alpha to 1.0");
            }
            
            // Ensure text component is enabled
            if (!textElement.enabled)
            {
                textElement.enabled = true;
                Debug.Log($"[PokerTableUI] Enabled {name} component");
            }

            // Force text to be visible - ensure it has valid size and position
            RectTransform rect = textElement.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Ensure rect has valid size (not zero)
                if (rect.sizeDelta.x < 1 || rect.sizeDelta.y < 1)
                {
                    rect.sizeDelta = new Vector2(Mathf.Max(rect.sizeDelta.x, 100), Mathf.Max(rect.sizeDelta.y, 30));
                    Debug.Log($"[PokerTableUI] Fixed {name} size to {rect.sizeDelta}");
                }

                // Ensure parent is active
                if (rect.parent != null && !rect.parent.gameObject.activeSelf)
                {
                    rect.parent.gameObject.SetActive(true);
                    Debug.Log($"[PokerTableUI] Activated {name} parent");
                }

                // Force text color to be visible (not white on white or transparent)
                if (textElement.color.r > 0.9f && textElement.color.g > 0.9f && textElement.color.b > 0.9f)
                {
                    // Text is very light/white - change to dark for visibility
                    textElement.color = new Color(0f, 0f, 0f, 1f); // Black text
                    Debug.Log($"[PokerTableUI] Changed {name} color to black for visibility");
                }
                else if (textElement.color.a < 0.5f)
                {
                    // Text is too transparent
                    var color = textElement.color;
                    color.a = 1f;
                    textElement.color = color;
                    Debug.Log($"[PokerTableUI] Fixed {name} alpha to 1.0");
                }

                // Ensure text is in front (set sibling index to last)
                rect.SetAsLastSibling();
            }

            // Force enable the CanvasRenderer (critical for visibility)
            CanvasRenderer canvasRenderer = textElement.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
            {
                canvasRenderer.cull = false; // Don't cull this element
            }
        }
        else
        {
            Debug.LogWarning($"[PokerTableUI] {name} is NULL! Make sure it's assigned in Inspector.");
        }
    }

    /// <summary>
    /// Creates a text element programmatically (used when Inspector assignment is missing).
    /// </summary>
    private TextMeshProUGUI CreateTextElement(string name, Vector2 anchorPosition, string initialText)
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
            Debug.Log($"[PokerTableUI] {name} already exists, reusing it");
            return existing.GetComponent<TextMeshProUGUI>();
        }
        
        // Create new text element - EXACTLY like test text that works
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(canvas.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = initialText;
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TMPro.TextAlignmentOptions.Left;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorPosition;
        rect.anchorMax = anchorPosition;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(200, 30);
        
        // Force visibility - same as test text
        textObj.SetActive(true);
        text.enabled = true;
        
        // Force CanvasRenderer
        CanvasRenderer canvasRenderer = textObj.GetComponent<CanvasRenderer>();
        if (canvasRenderer != null)
        {
            canvasRenderer.cull = false;
        }
        
        Debug.Log($"[PokerTableUI] Created {name} - Parent: {canvas.name}, Position: {rect.position}, AnchoredPos: {rect.anchoredPosition}, Active: {textObj.activeSelf}");
        
        return text;
    }

    /// <summary>
    /// Creates all UI text elements in Start() - ALWAYS CREATE (ignore Inspector assignments).
    /// This ensures they work in builds where Inspector assignments fail.
    /// </summary>
    private void CreateUITextElements()
    {
        // ALWAYS create new ones - don't trust Inspector assignments in builds
        // Destroy any existing ones first
        if (potText != null) Destroy(potText.gameObject);
        if (currentBetText != null) Destroy(currentBetText.gameObject);
        if (yourChipsText != null) Destroy(yourChipsText.gameObject);
        
        // Create fresh ones - same method as test text that works
        potText = CreateTextElement("PotText_Dynamic", new Vector2(0.1f, 0.8f), "Pot: 0");
        currentBetText = CreateTextElement("CurrentBetText_Dynamic", new Vector2(0.1f, 0.75f), "Current Bet: 0");
        yourChipsText = CreateTextElement("YourChipsText_Dynamic", new Vector2(0.1f, 0.7f), "Your Chips: --");
        
        Debug.Log($"[PokerTableUI] Created all text elements in Start() - Pot: {potText != null}, Bet: {currentBetText != null}, Chips: {yourChipsText != null}");
    }

    /// <summary>
    /// Creates a test text element to verify Canvas rendering works.
    /// If this shows up, Canvas is working. If not, Canvas is the issue.
    /// </summary>
    private void CreateTestTextElement()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindAnyObjectByType<Canvas>();
        }
        
        if (canvas != null)
        {
            // Check if test text already exists
            GameObject existingTest = GameObject.Find("TEST_TEXT_VISIBILITY");
            if (existingTest != null)
            {
                Destroy(existingTest);
            }
            
            // Create a simple test text that MUST be visible
            GameObject testObj = new GameObject("TEST_TEXT_VISIBILITY");
            testObj.transform.SetParent(canvas.transform, false);
            
            TextMeshProUGUI testText = testObj.AddComponent<TMPro.TextMeshProUGUI>();
            testText.text = "TEST - IF YOU SEE THIS, CANVAS WORKS";
            testText.fontSize = 36;
            testText.color = Color.red; // Bright red so it's impossible to miss
            testText.alignment = TMPro.TextAlignmentOptions.Center;
            
            RectTransform rect = testObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.9f);
            rect.anchorMax = new Vector2(0.5f, 0.9f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(600, 50);
            
            // Force it to be visible
            testObj.SetActive(true);
            testText.enabled = true;
            
            Debug.Log("[PokerTableUI] Created TEST_TEXT_VISIBILITY - if you see red text at top center, Canvas is working");
        }
    }

    /// <summary>
    /// Converts a card ID (0-51) to a human-readable name.
    /// </summary>
    private string GetCardName(int cardID)
    {
        if (cardID < 0 || cardID > 51) return "???";

        string[] suits = { "♠", "♥", "♦", "♣" };
        string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        int suitIndex = cardID / 13;
        int rankIndex = cardID % 13;

        return $"{ranks[rankIndex]}{suits[suitIndex]}";
    }
}

