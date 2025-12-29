using Fusion;
using TMPro;
using UnityEngine;

public class CommunityCards : NetworkBehaviour
{
    [Networked] public int Flop1 { get; set; } = -1;
    [Networked] public int Flop2 { get; set; } = -1;
    [Networked] public int Flop3 { get; set; } = -1;
    [Networked] public int Turn { get; set; } = -1;
    [Networked] public int River { get; set; } = -1;

    [SerializeField] private TextMeshProUGUI _communityLabel;

    public override void Spawned()
    {
        if (_communityLabel != null)
        {
            _communityLabel.text = "";
        }
    }

    // Fusion 2.0 pattern: Render handles local visual updates based on networked state
    public override void Render()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_communityLabel == null) return;

        // Display "Waiting" only if no cards have been dealt yet
        if (Flop1 == -1)
        {
            _communityLabel.text = "";
            return;
        }

        // Standard string formatting for the board
        string flopText = $"Flop: {GetCardName(Flop1)}, {GetCardName(Flop2)}, {GetCardName(Flop3)}";
        string turnText = Turn == -1 ? "Turn: [?]" : $"Turn: {GetCardName(Turn)}";
        string riverText = River == -1 ? "River: [?]" : $"River: {GetCardName(River)}";

        _communityLabel.text = $"{flopText}\n{turnText}\n{riverText}";
    }

    private string GetCardName(int cardID)
    {
        if (cardID < 0 || cardID > 51) return "???";

        string[] suits = { "Spades", "Hearts", "Diamonds", "Clubs" };
        string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

        int suitIndex = cardID / 13;
        int rankIndex = cardID % 13;

        return $"{ranks[rankIndex]} of {suits[suitIndex]}";
    }
}