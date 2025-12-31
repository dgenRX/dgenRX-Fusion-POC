using Fusion;
using UnityEngine;

/// <summary>
/// Networked state for community cards (flop, turn, river).
/// Cards are represented as integers (0-51 for a standard 52-card deck).
/// </summary>
public class CommunityCards : NetworkBehaviour
{
    [Networked] public int Flop1 { get; set; } = -1;
    [Networked] public int Flop2 { get; set; } = -1;
    [Networked] public int Flop3 { get; set; } = -1;
    [Networked] public int Turn { get; set; } = -1;
    [Networked] public int River { get; set; } = -1;
}