using Fusion;
using UnityEngine;

public class NetworkedCardDeck : NetworkBehaviour
{
    // Define the progression of a poker hand
    public enum GameRound { Waiting, Flop, Turn, River }

    [Networked, Capacity(52)] 
    private NetworkArray<int> _deck { get; }

    [Networked] 
    private int _nextCardIndex { get; set; }

    [Networked]
    private GameRound _currentRound { get; set; }

    private CommunityCards _communityBoard;
    private GlobalManager _globalManager;

    public override void Spawned()
    {
        _communityBoard = FindAnyObjectByType<CommunityCards>();
        _globalManager = FindAnyObjectByType<GlobalManager>();

        if (Object.HasStateAuthority)
        {
            InitializeDeck();
            ShuffleDeck();
            _currentRound = GameRound.Waiting;
            Debug.Log("[Deck] Initialized and Shuffled by Host.");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && _globalManager != null)
        {
            if (_globalManager.DealRequested)
            {
                // Reset the trigger immediately on the networked property
                _globalManager.DealRequested = false;
                
                // Advance to the next stage of the hand
                AdvanceGame();
            }
        }
    }

    private void AdvanceGame()
    {
        switch (_currentRound)
        {
            case GameRound.Waiting:
                DealFlop();
                _currentRound = GameRound.Flop;
                break;
            case GameRound.Flop:
                DealTurn();
                _currentRound = GameRound.Turn;
                break;
            case GameRound.Turn:
                DealRiver();
                _currentRound = GameRound.River;
                break;
            case GameRound.River:
                ResetHand();
                break;
        }
    }

    private void InitializeDeck()
    {
        for (int i = 0; i < 52; i++)
        {
            _deck.Set(i, i);
        }
        _nextCardIndex = 0;
    }

    public void ShuffleDeck()
    {
        if (!Object.HasStateAuthority) return;

        for (int i = 51; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            int temp = _deck[i];
            _deck.Set(i, _deck[randomIndex]);
            _deck.Set(randomIndex, temp);
        }
        _nextCardIndex = 0;
    }

    public int GetNextCard()
    {
        if (!Object.HasStateAuthority) return -1;

        if (_nextCardIndex >= 52) ShuffleDeck();

        int card = _deck[_nextCardIndex];
        _nextCardIndex++;
        return card;
    }

    private void DealFlop()
    {
        if (_communityBoard == null) return;
        _communityBoard.Flop1 = GetNextCard();
        _communityBoard.Flop2 = GetNextCard();
        _communityBoard.Flop3 = GetNextCard();
        Debug.Log($"[Deck] Flop Dealt: {_communityBoard.Flop1}, {_communityBoard.Flop2}, {_communityBoard.Flop3}");
    }

    private void DealTurn()
    {
        if (_communityBoard == null) return;
        _communityBoard.Turn = GetNextCard();
        Debug.Log($"[Deck] Turn Dealt: {_communityBoard.Turn}");
    }

    private void DealRiver()
    {
        if (_communityBoard == null) return;
        _communityBoard.River = GetNextCard();
        Debug.Log($"[Deck] River Dealt: {_communityBoard.River}");
    }

    private void ResetHand()
    {
        if (_communityBoard == null) return;
        
        // Reset all board properties to -1 for synchronization
        _communityBoard.Flop1 = -1;
        _communityBoard.Flop2 = -1;
        _communityBoard.Flop3 = -1;
        _communityBoard.Turn = -1;
        _communityBoard.River = -1;
        
        ShuffleDeck();
        _currentRound = GameRound.Waiting;
        Debug.Log("[Deck] Hand Reset and Deck Shuffled.");
    }
}