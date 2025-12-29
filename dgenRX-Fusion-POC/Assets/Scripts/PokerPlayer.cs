using Fusion;
using UnityEngine;
using System.Collections;

/// <summary>
/// Represents a poker player in the game. Tracks chips, cards, and actions.
/// Each player has their own instance of this script.
/// </summary>
public class PokerPlayer : NetworkBehaviour
{
    // Player actions they can take
    public enum PlayerAction
    {
        Fold,
        Check,
        Call,
        Bet,
        Raise
    }

    [Networked] public int Chips { get; private set; } = 1000; // Starting chips
    [Networked] public int CurrentBet { get; set; } = 0; // How much they've bet this round
    [Networked] public bool IsFolded { get; private set; } = false;
    [Networked] public int Card1 { get; private set; } = -1; // First hole card
    [Networked] public int Card2 { get; private set; } = -1; // Second hole card

    private PokerGameManager _gameManager;

    public override void Spawned()
    {
        Debug.Log($"[PokerPlayer] Player {Object.InputAuthority} spawned with {Chips} chips");
        
        // Find the game manager (with retry since it might spawn after players)
        _gameManager = FindAnyObjectByType<PokerGameManager>();
        
        if (_gameManager != null)
        {
            _gameManager.RegisterPlayer(this);
        }
        else
        {
            // Retry after a short delay if game manager not found yet
            StartCoroutine(RegisterPlayerDelayed());
        }
    }

    private System.Collections.IEnumerator RegisterPlayerDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        
        _gameManager = FindAnyObjectByType<PokerGameManager>();
        if (_gameManager != null)
        {
            _gameManager.RegisterPlayer(this);
        }
        else
        {
            Debug.LogWarning($"[PokerPlayer] Could not find PokerGameManager after delay");
        }
    }

    /// <summary>
    /// Deals two hole cards to this player.
    /// </summary>
    public void DealHoleCards(int card1, int card2)
    {
        if (!Object.HasStateAuthority) return;
        
        Card1 = card1;
        Card2 = card2;
        IsFolded = false;
        CurrentBet = 0;
        
        Debug.Log($"[PokerPlayer] Player {Object.InputAuthority} dealt cards: {card1}, {card2}");
    }

    /// <summary>
    /// Player posts a blind (small or big blind).
    /// </summary>
    public void PostBlind(int amount)
    {
        if (!Object.HasStateAuthority) return;
        
        int actualAmount = Mathf.Min(amount, Chips); // Can't bet more than you have
        Chips -= actualAmount;
        CurrentBet = actualAmount;
        
        Debug.Log($"[PokerPlayer] Player {Object.InputAuthority} posted blind: {actualAmount}");
    }

    /// <summary>
    /// Player makes a bet (or call/raise).
    /// </summary>
    public void Bet(int amount)
    {
        if (!Object.HasStateAuthority) return;
        if (IsFolded) return;

        int actualAmount = Mathf.Min(amount, Chips + CurrentBet); // Can't bet more than you have
        int chipsToAdd = actualAmount - CurrentBet;
        
        if (chipsToAdd > 0)
        {
            Chips -= chipsToAdd;
            CurrentBet = actualAmount;
        }
        
        Debug.Log($"[PokerPlayer] Player {Object.InputAuthority} bet {actualAmount} (chips remaining: {Chips})");
    }

    /// <summary>
    /// Player folds (gives up this hand).
    /// </summary>
    public void Fold()
    {
        if (!Object.HasStateAuthority) return;
        
        IsFolded = true;
        Debug.Log($"[PokerPlayer] Player {Object.InputAuthority} folded");
    }

    /// <summary>
    /// Resets the player's bet for the next betting round.
    /// </summary>
    public void ResetBet()
    {
        if (!Object.HasStateAuthority) return;
        CurrentBet = 0;
    }

    /// <summary>
    /// Resets player state for a new hand.
    /// </summary>
    public void ResetForNewHand()
    {
        if (!Object.HasStateAuthority) return;
        
        IsFolded = false;
        CurrentBet = 0;
        Card1 = -1;
        Card2 = -1;
    }

    /// <summary>
    /// Adds chips to the player (when they win a pot).
    /// </summary>
    public void AddChips(int amount)
    {
        if (!Object.HasStateAuthority) return;
        
        Chips += amount;
        Debug.Log($"[PokerPlayer] Player {Object.InputAuthority} won {amount} chips. New total: {Chips}");
    }

    /// <summary>
    /// Called by the local player to make an action. This sends an RPC to the game manager.
    /// </summary>
    public void MakeAction(PlayerAction action, int betAmount = 0)
    {
        if (_gameManager == null) return;
        if (!Object.HasInputAuthority) return; // Only the local player can make actions

        // Send RPC to process the action on the server
        RPC_PlayerAction(action, betAmount);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PlayerAction(PlayerAction action, int betAmount)
    {
        Debug.Log($"[PokerPlayer] RPC_PlayerAction received: {action}, betAmount: {betAmount}");
        if (_gameManager != null)
        {
            _gameManager.ProcessPlayerAction(Object.InputAuthority, action, betAmount);
        }
        else
        {
            Debug.LogError("[PokerPlayer] RPC_PlayerAction: GameManager is null!");
        }
    }

    /// <summary>
    /// Check if it's this player's turn.
    /// </summary>
    public bool IsMyTurn()
    {
        if (_gameManager == null) return false;
        
        // Find our index in the game manager's player list
        // (This is a simplified check - you might want to improve this)
        return Object.HasInputAuthority && _gameManager.CurrentPlayerIndex >= 0;
    }
}

