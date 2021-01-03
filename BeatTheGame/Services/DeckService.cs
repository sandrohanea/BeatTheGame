using BeatTheGame.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatTheGame.Services
{
    public class DeckService : IDeckService
    {
        private readonly Random random = new Random();
        public Deck GenerateDeck(bool allowRedCards)
        {
            var cards = Enumerable.Range(0, 100).Select(v => new Card(v, v % 9 == 0)).OrderBy(r => random.Next(0, 1000));
            return new Deck(new Stack<Card>(cards), DeckType.Random);
        }
    }
}
