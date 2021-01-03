using System.Collections.Generic;

namespace BeatTheGame.Data
{
    public record Deck(Stack<Card> Cards, DeckType Type);
}
