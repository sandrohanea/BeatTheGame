using BeatTheGame.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTheGame.Services
{
    public class GameSessionService : IGameSessionService
    {
        private static readonly Dictionary<string, GameSession> activeSessions = new Dictionary<string, GameSession>();
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        private static readonly Random random = new Random();

        private const string codeChars = "ABCDEFGHIJKLMNOPQRTSTUVWXYZ";
        private readonly IDeckService deckService;
        private readonly INotificationService notificationService;

        public GameSessionService(IDeckService deckService, INotificationService notificationService)
        {
            this.deckService = deckService;
            this.notificationService = notificationService;
        }

        public GameSession? GetGameSession(string code)
        {
            activeSessions.TryGetValue(code.ToUpper(), out var gameSession);
            return gameSession;
        }

        public async Task<GameSession> CreateGameSessionAsync(
            Player organizer,
            GameSessionConfiguration configuration)
        {
            try
            {
                await semaphoreSlim.WaitAsync();
                var code = GenerateGameSessionCode();
                var emptyDecks = Enumerable.Range(1, configuration.NumberOfDecks)
                    .Select(n => new Deck(new Stack<Card>(), n % 2 == 0 ? DeckType.Ascending : DeckType.Descending))
                    .OrderBy(d => d.Type)
                    .ToList();

                var gameSession = new GameSession(organizer.PlayerId,
                                                  code,
                                                  new List<Player>() { organizer },
                                                  new List<Hand>(),
                                                  configuration,
                                                  deckService.GenerateDeck(configuration),
                                                  emptyDecks,
                                                  GameSessionStatus.Created,
                                                  DateTime.Now,
                                                  new List<int>(),
                                                  new List<int>());
                activeSessions.Add(code, gameSession);
                return gameSession;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public void StartGameSession(GameSession gameSession)
        {
            gameSession = gameSession with { GameSessionStatus = GameSessionStatus.Started };
            var hands = gameSession.Hands;
            foreach (var player in gameSession.Players)
            {
                var cards = new List<Card>();
                for (var indexCard = 0; indexCard < gameSession.Config.CardsInHand; indexCard++)
                {
                    cards.Add(gameSession.MainDeck.Cards.Pop());
                }

                hands.Add(new Hand(player.PlayerId, cards));
            }
            activeSessions[gameSession.Code] = gameSession;
            notificationService.Notify(gameSession);
        }

        public void JoinGameSession(Player player, GameSession gameSession)
        {
            var oldPlayer = gameSession.Players.FirstOrDefault(p => p.PlayerId == player.PlayerId);
            if (oldPlayer != null)
            {
                gameSession.Players.Remove(oldPlayer);
            }
            gameSession.Players.Add(player);
            notificationService.Notify(gameSession);
        }
        public void RemoveFromGameSession(Player player, GameSession gameSession)
        {
            var oldPlayer = gameSession.Players.FirstOrDefault(p => p.PlayerId == player.PlayerId);
            if (oldPlayer != null)
            {
                gameSession.Players.Remove(oldPlayer);
            }
            notificationService.Notify(gameSession);
        }

        public void StartWithMyTurn(Player player, GameSession gameSession)
        {
            var indexOfPlayer = gameSession.Players.IndexOf(player);
            gameSession = gameSession with { PlayerTurn = indexOfPlayer };
            activeSessions[gameSession.Code] = gameSession;
            notificationService.Notify(gameSession);
        }

        public bool AddCard(Deck deck, Card card, GameSession gameSession, Hand hand)
        {
            if (deck.Cards.Count > 0)
            {
                if (!CanTheCardBeAddedOnTheDeck(gameSession, card, deck))
                {
                    return false;
                }
                var oldCard = deck.Cards.Peek();
                if (gameSession.RedCardPendings.Any(r => r == oldCard.Value))
                {
                    gameSession.RedCardPendings.Remove(oldCard.Value);
                }
            }
            hand.Cards.Remove(card);
            deck.Cards.Push(card);
            gameSession.CardsAddedThisTurn.Add(card.Value);

            if (CheckIfGameIsLostOrWon(gameSession))
            {
                return true;
            }
            notificationService.Notify(gameSession);
            return true;
        }

        public bool CanFinishTurn(GameSession gameSession, Player? player)
        {
            return (gameSession.PlayerTurn >= 0 && player?.PlayerId == gameSession.Players[gameSession.PlayerTurn].PlayerId)
                &&
                ((gameSession.CardsAddedThisTurn.Count >= gameSession.Config.CardsPerTurn)
                || gameSession.MainDeck.Cards.Count == 0 && gameSession.CardsAddedThisTurn.Count == gameSession.Config.CardsPerTurn - 1)

                && gameSession.RedCardPendings.Count == 0;
        }

        public void FinishTurn(GameSession gameSession)
        {
            Hand currentHand;
            do
            {
                var nextPlayerIndex = gameSession.PlayerTurn + 1;
                if (nextPlayerIndex == gameSession.Players.Count)
                {
                    nextPlayerIndex = 0;
                }

                currentHand = gameSession.Hands[gameSession.PlayerTurn];
                while (currentHand.Cards.Count < gameSession.Config.CardsInHand && gameSession.MainDeck.Cards.Count > 0)
                {
                    currentHand.Cards.Add(gameSession.MainDeck.Cards.Pop());
                }

                gameSession = gameSession with
                {
                    PlayerTurn = nextPlayerIndex,
                    RedCardPendings = gameSession.Decks
                                            .Where(d => d.Cards.Count > 0)
                                            .Select(d => d.Cards.Peek())
                                            .Where(c => c.IsRed)
                                            .Select(c => c.Value)
                                            .ToList(),
                    CardsAddedThisTurn = new List<int>()
                };

                if (CheckIfGameIsLostOrWon(gameSession))
                {
                    return;
                }

                currentHand = gameSession.Hands[gameSession.PlayerTurn];
                // We need to repeat the process in case the current player finished all the cards.
            } while (currentHand.Cards.Count == 0);

            activeSessions[gameSession.Code] = gameSession;
            notificationService.Notify(gameSession);
        }

        private bool CheckIfGameIsLostOrWon(GameSession gameSession)
        {
            var isGameOver = false;
            //The game is won if there are no more cards in the deck or hands.
            if (gameSession.MainDeck.Cards.Count == 0 && gameSession.Hands.All(h => h.Cards.Count == 0))
            {
                gameSession = gameSession with { GameSessionStatus = GameSessionStatus.Won };
                isGameOver = true;
            }
            else
            {
                // If the player can finish turn, the show must go on.
                if (CanFinishTurn(gameSession, gameSession.Players[gameSession.PlayerTurn]))
                {
                    return false;
                }

                // The game is lost if the current player have no more cards to add
                var currentHand = gameSession.Hands[gameSession.PlayerTurn];
                var isAnyMove = false;
                foreach (var card in currentHand.Cards)
                {
                    foreach (var deck in gameSession.Decks)
                    {
                        if (CanTheCardBeAddedOnTheDeck(gameSession, card, deck))
                        {
                            isAnyMove = true;
                            break;
                        }
                    }

                    if (isAnyMove)
                    {
                        break;
                    }
                }
                if (!isAnyMove)
                {
                    gameSession = gameSession with { GameSessionStatus = GameSessionStatus.Lost };
                    isGameOver = true;
                }
            }

            if (isGameOver)
            {
                activeSessions[gameSession.Code] = gameSession;
                notificationService.Notify(gameSession);
            }

            return isGameOver;
        }

        private bool CanTheCardBeAddedOnTheDeck(GameSession gameSession, Card card, Deck deck)
        {
            if (deck.Cards.Count == 0)
            {
                return true;
            }

            var oldCard = deck.Cards.Peek();
            if ((oldCard.Value > card.Value && deck.Type == DeckType.Ascending && oldCard.Value != card.Value + 10)
                || oldCard.Value < card.Value && deck.Type == DeckType.Descending && oldCard.Value != card.Value - 10)
            {
                return false;
            }
            return true;
        }

        private string GenerateGameSessionCode()
        {
            string code;
            do
            {
                var stringBuilder = new StringBuilder();
                for (var i = 0; i < 5; i++)
                {
                    stringBuilder.Append(codeChars[random.Next(0, codeChars.Length - 1)]);
                }
                code = stringBuilder.ToString();
            } while (activeSessions.ContainsKey(code));
            return code;
        }
    }
}
