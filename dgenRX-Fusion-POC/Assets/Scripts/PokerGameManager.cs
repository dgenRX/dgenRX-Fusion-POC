using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main controller for the poker game. Manages game state, betting rounds, and pot.
/// This runs on the Host (server) and synchronizes to all clients.
/// </summary>
public class PokerGameManager : NetworkBehaviour
{
    // Game state enum - tracks what phase of the hand we're in
    public enum GameState
    {
        WaitingForPlayers,  // Waiting for enough players to start
        PreFlop,            // Cards dealt, first betting round
        Flop,               // 3 community cards, second betting round
        Turn,               // 4th community card, third betting round
        River,              // 5th community card, final betting round
        Showdown,           // Reveal cards, determine winner
        HandComplete        // Hand finished, reset for next hand
    }

    [Networked] public GameState CurrentState { get; private set; }
    [Networked] public int CurrentPlayerIndex { get; private set; } = -1;
    [Networked] public PlayerRef CurrentPlayerRef { get; private set; } // Networked player reference for current turn
    [Networked] public PlayerRef WinnerRef { get; private set; } // Networked reference to the winner of the current hand
    [Networked] public int Pot { get; private set; } = 0;
    [Networked] public int CurrentBet { get; private set; } = 0; // Highest bet this round
    [Networked] public int SmallBlind { get; private set; } = 1;
    [Networked] public int BigBlind { get; private set; } = 3;
    [Networked] public int BlindPosition { get; private set; } = 0; // Tracks which player posts small blind (rotates each hand)

    // List of all players in the game (we'll populate this when players join)
    // Using a simple list - we'll find players dynamically, no need to network this
    private NetworkedCardDeck _deck;
    private CommunityCards _communityCards;
    private List<PokerPlayer> _players = new List<PokerPlayer>();

    public override void Spawned()
    {
        // Find the deck and community cards in the scene
        _deck = FindAnyObjectByType<NetworkedCardDeck>();
        _communityCards = FindAnyObjectByType<CommunityCards>();

        if (Object.HasStateAuthority)
        {
            CurrentState = GameState.WaitingForPlayers;
            Debug.Log("[PokerGameManager] Spawned and waiting for players...");
            
            // Check for existing players that might have spawned before us
            StartCoroutine(CheckForExistingPlayers());
        }
    }

    private System.Collections.IEnumerator CheckForExistingPlayers()
    {
        yield return new WaitForSeconds(0.5f);
        
        // Find all existing PokerPlayer instances and register them
        PokerPlayer[] existingPlayers = FindObjectsByType<PokerPlayer>(FindObjectsSortMode.None);
        foreach (var player in existingPlayers)
        {
            if (!_players.Contains(player))
            {
                RegisterPlayer(player);
            }
        }
    }

    /// <summary>
    /// Called when a player joins the game. Adds them to the player list.
    /// </summary>
    public void RegisterPlayer(PokerPlayer player)
    {
        if (!Object.HasStateAuthority) return;
        
        // Check if player is already registered
        if (_players.Contains(player))
        {
            Debug.LogWarning($"[PokerGameManager] Player {player.Object.InputAuthority} already registered");
            return;
        }

        // Add player to list
        _players.Add(player);
        Debug.Log($"[PokerGameManager] Player {player.Object.InputAuthority} registered. Total players: {_players.Count}");
        
        // If we have 2+ players, start the game
        if (_players.Count >= 2 && CurrentState == GameState.WaitingForPlayers)
        {
            StartNewHand();
        }
    }

    /// <summary>
    /// Checks if it's the given player's turn. Can be called from clients.
    /// Uses the networked CurrentPlayerRef for reliable client-side checking.
    /// </summary>
    public bool IsPlayerTurn(PokerPlayer player)
    {
        if (player == null || CurrentPlayerRef == PlayerRef.None)
            return false;
        
        // Compare the networked current player reference with this player's reference
        return player.Object.InputAuthority == CurrentPlayerRef;
    }

    /// <summary>
    /// Starts a new poker hand. Deals cards, posts blinds, begins pre-flop betting.
    /// </summary>
    public void StartNewHand()
    {
        if (!Object.HasStateAuthority) return;
        if (_players.Count < 2) return;

        Debug.Log("[PokerGameManager] Starting new hand...");

        // Reset pot and bets
        Pot = 0;
        CurrentBet = 0;
        WinnerRef = PlayerRef.None; // Clear previous winner

        // Post blinds (small blind and big blind)
        // Rotate blinds each hand - BlindPosition tracks who posts small blind
        if (_players.Count >= 2)
        {
            int smallBlindIndex = BlindPosition % _players.Count;
            int bigBlindIndex = (BlindPosition + 1) % _players.Count;
            
            _players[smallBlindIndex].PostBlind(SmallBlind);
            _players[bigBlindIndex].PostBlind(BigBlind);
            CurrentBet = BigBlind;
            Pot = SmallBlind + BigBlind;
            
            // Rotate blinds for next hand
            BlindPosition = (BlindPosition + 1) % _players.Count;
        }

        // Deal hole cards to each player
        if (_deck == null)
        {
            Debug.LogError("[PokerGameManager] Deck is null! Cannot deal cards.");
            _deck = FindAnyObjectByType<NetworkedCardDeck>();
            if (_deck == null)
            {
                Debug.LogError("[PokerGameManager] Still cannot find deck after search!");
                return;
            }
        }

        foreach (var player in _players)
        {
            int card1 = _deck.GetNextCard();
            int card2 = _deck.GetNextCard();
            Debug.Log($"[PokerGameManager] Dealing cards to player: {card1}, {card2}");
            player.DealHoleCards(card1, card2);
        }

        // Start pre-flop betting round
        CurrentState = GameState.PreFlop;
        CurrentPlayerIndex = 2 % _players.Count; // Start with player after big blind
        if (CurrentPlayerIndex >= 0 && CurrentPlayerIndex < _players.Count)
        {
            CurrentPlayerRef = _players[CurrentPlayerIndex].Object.InputAuthority;
        }
        Debug.Log($"[PokerGameManager] Pre-flop betting started. Current player: {CurrentPlayerIndex}");
    }

    /// <summary>
    /// Processes a player action (fold, check, call, bet, raise).
    /// </summary>
    public void ProcessPlayerAction(PlayerRef playerRef, PokerPlayer.PlayerAction action, int betAmount = 0)
    {
        if (!Object.HasStateAuthority) return;
        if (CurrentState == GameState.WaitingForPlayers || CurrentState == GameState.Showdown || CurrentState == GameState.HandComplete)
            return;

        // Find the player who made the action
        PokerPlayer actingPlayer = null;
        int playerIndex = -1;
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].Object.InputAuthority == playerRef)
            {
                actingPlayer = _players[i];
                playerIndex = i;
                break;
            }
        }

        if (actingPlayer == null)
        {
            Debug.LogWarning($"[PokerGameManager] Acting player not found for PlayerRef {playerRef}");
            return;
        }
        
        // Check if it's this player's turn using the networked reference
        if (actingPlayer.Object.InputAuthority != CurrentPlayerRef)
        {
            Debug.LogWarning($"[PokerGameManager] Not player's turn. Current: {CurrentPlayerRef}, Acting: {actingPlayer.Object.InputAuthority}");
            return;
        }

        Debug.Log($"[PokerGameManager] Processing {action} from player {playerIndex} (PlayerRef: {playerRef})");

        // Process the action
        switch (action)
        {
            case PokerPlayer.PlayerAction.Fold:
                actingPlayer.Fold();
                Debug.Log($"[PokerGameManager] Player {playerIndex} folded");
                break;

            case PokerPlayer.PlayerAction.Check:
                if (actingPlayer.CurrentBet < CurrentBet)
                {
                    Debug.LogWarning("[PokerGameManager] Cannot check - must call or fold");
                    return;
                }
                Debug.Log($"[PokerGameManager] Player {playerIndex} checked");
                break;

            case PokerPlayer.PlayerAction.Call:
                int callAmount = CurrentBet - actingPlayer.CurrentBet;
                if (callAmount > 0)
                {
                    actingPlayer.Bet(callAmount);
                    Pot += callAmount;
                }
                Debug.Log($"[PokerGameManager] Player {playerIndex} called {callAmount}");
                break;

            case PokerPlayer.PlayerAction.Bet:
            case PokerPlayer.PlayerAction.Raise:
                // Minimum raise must be at least the big blind amount more than current bet
                int minRaise = CurrentBet + BigBlind;
                if (betAmount < minRaise)
                {
                    Debug.LogWarning($"[PokerGameManager] Bet amount {betAmount} must be at least {minRaise} (current bet {CurrentBet} + big blind {BigBlind})");
                    return;
                }
                int raiseAmount = betAmount - actingPlayer.CurrentBet;
                actingPlayer.Bet(betAmount);
                Pot += raiseAmount;
                CurrentBet = betAmount;
                Debug.Log($"[PokerGameManager] Player {playerIndex} bet/raised to {betAmount}");
                break;
        }

        // Move to next player
        AdvanceToNextPlayer();
    }

    /// <summary>
    /// Moves to the next player's turn. If betting round is complete, advances game state.
    /// </summary>
    private void AdvanceToNextPlayer()
    {
        // Find next active (not folded) player
        int attempts = 0;
        do
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % _players.Count;
            attempts++;
        } while (_players[CurrentPlayerIndex].IsFolded && attempts < _players.Count);
        
        // Update the networked player reference
        if (CurrentPlayerIndex >= 0 && CurrentPlayerIndex < _players.Count)
        {
            CurrentPlayerRef = _players[CurrentPlayerIndex].Object.InputAuthority;
        }

        // Check if betting round is complete (all players have matched the bet or folded)
        bool bettingRoundComplete = true;
        foreach (var player in _players)
        {
            if (!player.IsFolded && player.CurrentBet < CurrentBet)
            {
                bettingRoundComplete = false;
                break;
            }
        }

        // If only one player left (all others folded), they win
        int activePlayers = 0;
        PokerPlayer winner = null;
        foreach (var player in _players)
        {
            if (!player.IsFolded)
            {
                activePlayers++;
                winner = player;
            }
        }

        if (activePlayers == 1 && winner != null)
        {
            // Single winner - award pot
            int potAmount = Pot;
            winner.AddChips(potAmount);
            WinnerRef = winner.Object.InputAuthority; // Set winner for UI display
            Pot = 0;
            Debug.Log($"[PokerGameManager] Player {winner.Object.InputAuthority} won by default (others folded). Awarded {potAmount} chips");
            
            // Wait a moment before starting next hand so players can see "You Win!" message
            StartCoroutine(DelayedResetForNextHand());
            return;
        }

        // If betting round complete, advance to next phase
        if (bettingRoundComplete)
        {
            AdvanceGamePhase();
        }
    }

    /// <summary>
    /// Advances to the next phase of the hand (Flop, Turn, River, or Showdown).
    /// </summary>
    private void AdvanceGamePhase()
    {
        // Reset all player bets for next round
        foreach (var player in _players)
        {
            player.ResetBet();
        }
        CurrentBet = 0;

        switch (CurrentState)
        {
            case GameState.PreFlop:
                // Deal the flop
                if (_deck != null && _communityCards != null)
                {
                    _communityCards.Flop1 = _deck.GetNextCard();
                    _communityCards.Flop2 = _deck.GetNextCard();
                    _communityCards.Flop3 = _deck.GetNextCard();
                }
                CurrentState = GameState.Flop;
                CurrentPlayerIndex = 0; // Start with first player
                if (CurrentPlayerIndex >= 0 && CurrentPlayerIndex < _players.Count)
                {
                    CurrentPlayerRef = _players[CurrentPlayerIndex].Object.InputAuthority;
                }
                Debug.Log("[PokerGameManager] Flop dealt");
                break;

            case GameState.Flop:
                // Deal the turn
                if (_deck != null && _communityCards != null)
                {
                    _communityCards.Turn = _deck.GetNextCard();
                }
                CurrentState = GameState.Turn;
                CurrentPlayerIndex = 0;
                if (CurrentPlayerIndex >= 0 && CurrentPlayerIndex < _players.Count)
                {
                    CurrentPlayerRef = _players[CurrentPlayerIndex].Object.InputAuthority;
                }
                Debug.Log("[PokerGameManager] Turn dealt");
                break;

            case GameState.Turn:
                // Deal the river
                if (_deck != null && _communityCards != null)
                {
                    _communityCards.River = _deck.GetNextCard();
                }
                CurrentState = GameState.River;
                CurrentPlayerIndex = 0;
                if (CurrentPlayerIndex >= 0 && CurrentPlayerIndex < _players.Count)
                {
                    CurrentPlayerRef = _players[CurrentPlayerIndex].Object.InputAuthority;
                }
                Debug.Log("[PokerGameManager] River dealt");
                break;

            case GameState.River:
                // Go to showdown
                CurrentState = GameState.Showdown;
                DetermineWinner();
                break;
        }
    }

    /// <summary>
    /// Determines the winner at showdown and awards the pot.
    /// </summary>
    private void DetermineWinner()
    {
        if (!Object.HasStateAuthority) return;

        // Find all active players (not folded)
        List<PokerPlayer> activePlayers = new List<PokerPlayer>();
        foreach (var player in _players)
        {
            if (!player.IsFolded)
            {
                activePlayers.Add(player);
            }
        }

        if (activePlayers.Count == 0) return;

        // Use HandEvaluator to find the best hand
        PokerPlayer winner = activePlayers[0];
        HandEvaluator.HandRank bestRank = HandEvaluator.EvaluateHand(
            winner.Card1, winner.Card2,
            _communityCards.Flop1, _communityCards.Flop2, _communityCards.Flop3,
            _communityCards.Turn, _communityCards.River
        );

        for (int i = 1; i < activePlayers.Count; i++)
        {
            HandEvaluator.HandRank rank = HandEvaluator.EvaluateHand(
                activePlayers[i].Card1, activePlayers[i].Card2,
                _communityCards.Flop1, _communityCards.Flop2, _communityCards.Flop3,
                _communityCards.Turn, _communityCards.River
            );

            if (rank > bestRank)
            {
                bestRank = rank;
                winner = activePlayers[i];
            }
        }

        // Award pot to winner
        winner.AddChips(Pot);
        Debug.Log($"[PokerGameManager] Showdown complete. Winner awarded {Pot} chips. Hand: {bestRank}");
        Pot = 0;

        // Reset for next hand after a delay
        ResetForNextHand();
    }

    /// <summary>
    /// Resets the game state for the next hand.
    /// </summary>
    private System.Collections.IEnumerator DelayedResetForNextHand()
    {
        // Wait 3 seconds so players can see "You Win!" message
        yield return new WaitForSeconds(3f);
        ResetForNextHand();
    }

    private void ResetForNextHand()
    {
        CurrentState = GameState.HandComplete;
        WinnerRef = PlayerRef.None; // Clear winner
        
        // Reset all players
        foreach (var player in _players)
        {
            player.ResetForNewHand();
        }

        // Reset community cards
        if (_communityCards != null)
        {
            _communityCards.Flop1 = -1;
            _communityCards.Flop2 = -1;
            _communityCards.Flop3 = -1;
            _communityCards.Turn = -1;
            _communityCards.River = -1;
        }

        // Shuffle deck for next hand
        if (_deck != null)
        {
            _deck.ShuffleDeck();
        }

        // Start next hand after a short delay (you can add a timer here)
        StartNewHand();
    }
}

