using BeatTheGame.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatTheGame.Services
{
    public class DeckService : IDeckService
    {
        private readonly Random random = new Random();
        public Deck GenerateDeck(GameSessionConfiguration gameSessionConfiguration)
        {
            var cards = Enumerable.Range(0, gameSessionConfiguration.NumberOfCardsInDeck - 1)
                .Select(v => 
                    new Card(
                        v,
                        v % gameSessionConfiguration.RedCardsFrequency == 0 
                        && v / gameSessionConfiguration.RedCardsFrequency != 0
                        && v / gameSessionConfiguration.RedCardsFrequency != gameSessionConfiguration.NumberOfCardsInDeck / gameSessionConfiguration.RedCardsFrequency))
                .OrderBy(r => random.Next(0, 1000));

            return new Deck(new Stack<Card>(cards), DeckType.Random);
        }
    }
}
