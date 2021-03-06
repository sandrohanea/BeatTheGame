﻿using BeatTheGame.Data;
using System.Threading.Tasks;

namespace BeatTheGame.Services
{
    public interface IGameSessionService
    {
        bool AddCard(Deck deck, Card card, GameSession gameSession, Hand hand);
        bool CanFinishTurn(GameSession gameSession, Player? player);
        Task<GameSession> CreateGameSessionAsync(Player organizer, GameSessionConfiguration configuration);
        void FinishTurn(GameSession gameSession);
        GameSession? GetGameSession(string code);
        void JoinGameSession(Player player, GameSession gameSession);
        void CleanOldSessions();
        void RemoveFromGameSession(Player player, GameSession gameSession);
        void StartGameSession(GameSession gameSession);
        void StartWithMyTurn(Player player, GameSession gameSession);
    }
}