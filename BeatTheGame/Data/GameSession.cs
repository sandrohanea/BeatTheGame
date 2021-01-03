using System;
using System.Collections.Generic;

namespace BeatTheGame.Data
{
    public record GameSession(
        Guid OrganizerId,
        string Code,
        List<Player> Players,
        List<Hand> Hands,
        bool AllowRedCards,
        int CardsInHand,
        int CardsPerTurn,
        int NumberOfDecks,
        Deck MainDeck,
        List<Deck> Decks,
        GameSessionStatus GameSessionStatus,
        DateTime CreatedDateTime,
        List<int> RedCardPendings,
        List<int> CardsAddedThisTurn,
        int PlayerTurn = -1);
}
