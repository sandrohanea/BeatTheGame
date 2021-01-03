using BeatTheGame.Data;
using System;

namespace BeatTheGame.Services
{
    public interface INotificationService
    {
        void Clear(string code);
        void Notify(GameSession gameSession);
        void Subscribe(string code, Action<GameSession> action);
    }
}