using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Evaluates poker hands and determines the winner.
/// This is a static utility class - no networking needed, just pure logic.
/// </summary>
public static class HandEvaluator
{
    // Poker hand rankings (higher number = better hand)
    public enum HandRank
    {
        HighCard = 0,
        Pair = 1,
        TwoPair = 2,
        ThreeOfAKind = 3,
        Straight = 4,
        Flush = 5,
        FullHouse = 6,
        FourOfAKind = 7,
        StraightFlush = 8,
        RoyalFlush = 9
    }

    /// <summary>
    /// Evaluates a poker hand given 2 hole cards and 5 community cards.
    /// Returns the best possible hand rank.
    /// </summary>
    public static HandRank EvaluateHand(int card1, int card2, int flop1, int flop2, int flop3, int turn, int river)
    {
        // Convert card IDs to a list of all 7 cards
        List<int> allCards = new List<int> { card1, card2, flop1, flop2, flop3, turn, river };
        
        // Remove invalid cards (-1 means card not dealt yet)
        allCards = allCards.Where(c => c >= 0 && c < 52).ToList();

        if (allCards.Count < 5) return HandRank.HighCard;

        // Try all possible 5-card combinations from the 7 cards
        HandRank bestRank = HandRank.HighCard;
        
        // Generate all combinations of 5 cards from 7
        for (int i = 0; i < allCards.Count; i++)
        {
            for (int j = i + 1; j < allCards.Count; j++)
            {
                List<int> fiveCards = new List<int>
                {
                    allCards[i],
                    allCards[j],
                    allCards[(j + 1) % allCards.Count],
                    allCards[(j + 2) % allCards.Count],
                    allCards[(j + 3) % allCards.Count]
                };

                // Remove duplicates and ensure we have exactly 5 unique cards
                fiveCards = fiveCards.Distinct().ToList();
                if (fiveCards.Count != 5) continue;

                HandRank rank = EvaluateFiveCards(fiveCards);
                if (rank > bestRank)
                {
                    bestRank = rank;
                }
            }
        }

        return bestRank;
    }

    /// <summary>
    /// Evaluates exactly 5 cards and returns the hand rank.
    /// </summary>
    private static HandRank EvaluateFiveCards(List<int> cards)
    {
        if (cards.Count != 5) return HandRank.HighCard;

        // Extract ranks and suits
        List<int> ranks = new List<int>();
        List<int> suits = new List<int>();

        foreach (int card in cards)
        {
            ranks.Add(card % 13); // Rank: 0-12 (2-Ace)
            suits.Add(card / 13); // Suit: 0-3 (Spades, Hearts, Diamonds, Clubs)
        }

        ranks.Sort();

        // Check for flush (all same suit)
        bool isFlush = suits.All(s => s == suits[0]);

        // Check for straight
        bool isStraight = IsStraight(ranks);

        // Check for pairs, three of a kind, etc.
        var rankGroups = ranks.GroupBy(r => r).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Key).ToList();

        // Royal Flush: A-K-Q-J-10 of same suit
        if (isFlush && isStraight && ranks.Contains(12) && ranks.Contains(11) && ranks.Contains(10) && ranks.Contains(9) && ranks.Contains(8))
        {
            return HandRank.RoyalFlush;
        }

        // Straight Flush: Straight of same suit
        if (isFlush && isStraight)
        {
            return HandRank.StraightFlush;
        }

        // Four of a Kind: Four cards of same rank
        if (rankGroups[0].Count() == 4)
        {
            return HandRank.FourOfAKind;
        }

        // Full House: Three of a kind + pair
        if (rankGroups[0].Count() == 3 && rankGroups.Count > 1 && rankGroups[1].Count() == 2)
        {
            return HandRank.FullHouse;
        }

        // Flush: Five cards of same suit
        if (isFlush)
        {
            return HandRank.Flush;
        }

        // Straight: Five consecutive ranks
        if (isStraight)
        {
            return HandRank.Straight;
        }

        // Three of a Kind: Three cards of same rank
        if (rankGroups[0].Count() == 3)
        {
            return HandRank.ThreeOfAKind;
        }

        // Two Pair: Two pairs
        if (rankGroups[0].Count() == 2 && rankGroups.Count > 1 && rankGroups[1].Count() == 2)
        {
            return HandRank.TwoPair;
        }

        // Pair: Two cards of same rank
        if (rankGroups[0].Count() == 2)
        {
            return HandRank.Pair;
        }

        // High Card: Nothing special
        return HandRank.HighCard;
    }

    /// <summary>
    /// Checks if the ranks form a straight (5 consecutive cards).
    /// </summary>
    private static bool IsStraight(List<int> ranks)
    {
        if (ranks.Count != 5) return false;

        ranks.Sort();

        // Check for normal straight (e.g., 2-3-4-5-6)
        bool normalStraight = true;
        for (int i = 1; i < 5; i++)
        {
            if (ranks[i] != ranks[i - 1] + 1)
            {
                normalStraight = false;
                break;
            }
        }

        // Check for wheel straight (A-2-3-4-5, where Ace is low)
        bool wheelStraight = ranks[0] == 0 && ranks[1] == 1 && ranks[2] == 2 && ranks[3] == 3 && ranks[4] == 12;

        return normalStraight || wheelStraight;
    }

    /// <summary>
    /// Gets a human-readable name for a hand rank.
    /// </summary>
    public static string GetHandName(HandRank rank)
    {
        switch (rank)
        {
            case HandRank.HighCard: return "High Card";
            case HandRank.Pair: return "Pair";
            case HandRank.TwoPair: return "Two Pair";
            case HandRank.ThreeOfAKind: return "Three of a Kind";
            case HandRank.Straight: return "Straight";
            case HandRank.Flush: return "Flush";
            case HandRank.FullHouse: return "Full House";
            case HandRank.FourOfAKind: return "Four of a Kind";
            case HandRank.StraightFlush: return "Straight Flush";
            case HandRank.RoyalFlush: return "Royal Flush";
            default: return "Unknown";
        }
    }
}

