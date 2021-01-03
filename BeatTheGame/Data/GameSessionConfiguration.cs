namespace BeatTheGame.Data
{
    public record GameSessionConfiguration(bool AllowRedCards,
        int CardsInHand,
        int CardsPerTurn,
        int NumberOfCardsInDeck,
        int NumberOfDecks,
        int RedCardsFrequency);
}
