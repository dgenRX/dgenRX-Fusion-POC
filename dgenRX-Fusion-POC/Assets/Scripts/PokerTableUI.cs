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

    private void Start()
    {
        // Find the game manager
        _gameManager = FindAnyObjectByType<PokerGameManager>();
        
        // Find the network runner
        _runner = FindAnyObjectByType<NetworkRunner>();

        // Setup button listeners
        if (foldButton != null) foldButton.onClick.AddListener(OnFoldClicked);
        if (checkButton != null) checkButton.onClick.AddListener(OnCheckClicked);
        if (callButton != null) callButton.onClick.AddListener(OnCallClicked);
        if (betButton != null) betButton.onClick.AddListener(OnBetClicked);
        if (raiseButton != null) raiseButton.onClick.AddListener(OnRaiseClicked);

        // Setup betting slider
        if (betSlider != null)
        {
            betSlider.onValueChanged.AddListener(OnBetSliderChanged);
        }

        if (betInputField != null)
        {
            betInputField.onEndEdit.AddListener(OnBetInputChanged);
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
            if (_gameManager == null) return; // Still not found, skip this frame
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
            string stateText = $"State: {_gameManager.CurrentState}";
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

        // Update pot
        if (potText != null)
        {
            potText.text = $"Pot: {_gameManager.Pot}";
        }
        else
        {
            if (Time.frameCount < 10)
            {
                Debug.LogWarning("[PokerTableUI] potText is null!");
            }
        }

        // Update current bet
        if (currentBetText != null)
        {
            currentBetText.text = $"Current Bet: {_gameManager.CurrentBet}";
        }

        // Update local player info
        if (_localPlayer != null)
        {
            if (yourChipsText != null)
            {
                yourChipsText.text = $"Your Chips: {_localPlayer.Chips}";
            }

            if (yourCardsText != null)
            {
                if (_localPlayer.Card1 >= 0 && _localPlayer.Card2 >= 0)
                {
                    yourCardsText.text = $"Your Cards: {GetCardName(_localPlayer.Card1)}, {GetCardName(_localPlayer.Card2)}";
                }
                else
                {
                    yourCardsText.text = "Waiting for cards...";
                }
            }

            // Update turn indicator
            if (turnIndicatorText != null)
            {
                bool isMyTurn = _gameManager.CurrentPlayerIndex >= 0 && 
                               _localPlayer.Object.HasInputAuthority;
                turnIndicatorText.text = isMyTurn ? "YOUR TURN!" : "Waiting...";
                turnIndicatorText.color = isMyTurn ? Color.green : Color.white;
            }

            // Update button states
            UpdateButtonStates();
        }
    }

    /// <summary>
    /// Updates button states based on game state and player's situation.
    /// </summary>
    private void UpdateButtonStates()
    {
        if (_localPlayer == null || _gameManager == null) return;

        bool isMyTurn = _gameManager.CurrentPlayerIndex >= 0 && _localPlayer.Object.HasInputAuthority;
        bool canCheck = _localPlayer.CurrentBet >= _gameManager.CurrentBet;
        int callAmount = _gameManager.CurrentBet - _localPlayer.CurrentBet;
        bool canCall = callAmount > 0 && callAmount <= _localPlayer.Chips;

        // Enable/disable buttons based on turn and game state
        if (foldButton != null) foldButton.interactable = isMyTurn && !_localPlayer.IsFolded;
        if (checkButton != null) checkButton.interactable = isMyTurn && canCheck;
        if (callButton != null) callButton.interactable = isMyTurn && canCall;
        if (betButton != null) betButton.interactable = isMyTurn && _gameManager.CurrentBet == 0; // Can only bet if no one has bet yet
        if (raiseButton != null) raiseButton.interactable = isMyTurn && _gameManager.CurrentBet > 0; // Can raise if someone has bet

        // Update bet slider range
        if (betSlider != null && _localPlayer.Chips > 0)
        {
            int minBet = _gameManager.CurrentBet + 1;
            int maxBet = _localPlayer.Chips + _localPlayer.CurrentBet;
            betSlider.minValue = minBet;
            betSlider.maxValue = maxBet;
        }
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
        if (_localPlayer != null && betSlider != null)
        {
            int betAmount = Mathf.RoundToInt(betSlider.value);
            _localPlayer.MakeAction(PokerPlayer.PlayerAction.Bet, betAmount);
        }
    }

    private void OnRaiseClicked()
    {
        if (_localPlayer != null && betSlider != null)
        {
            int betAmount = Mathf.RoundToInt(betSlider.value);
            _localPlayer.MakeAction(PokerPlayer.PlayerAction.Raise, betAmount);
        }
    }

    private void OnBetSliderChanged(float value)
    {
        if (betAmountText != null)
        {
            betAmountText.text = $"Bet: {Mathf.RoundToInt(value)}";
        }
        if (betInputField != null)
        {
            betInputField.text = Mathf.RoundToInt(value).ToString();
        }
    }

    private void OnBetInputChanged(string value)
    {
        if (int.TryParse(value, out int betAmount) && betSlider != null)
        {
            betSlider.value = Mathf.Clamp(betAmount, betSlider.minValue, betSlider.maxValue);
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

