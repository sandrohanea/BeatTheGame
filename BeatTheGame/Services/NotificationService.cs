using BeatTheGame.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatTheGame.Services
{
    public class NotificationService : INotificationService
    {
        private static readonly Dictionary<string, List<WeakReference<Action<GameSession>>>> subscribers = new Dictionary<string, List<WeakReference<Action<GameSession>>>>();

        public void Subscribe(string code, Action<GameSession> action)
        {
            subscribers.TryGetValue(code, out var list);
            if (list == null)
            {
                list = new List<WeakReference<Action<GameSession>>>();
                subscribers.Add(code, list);
            }
            list.Add(new WeakReference<Action<GameSession>>(action));
        }

        public void Notify(GameSession gameSession)
        {
            subscribers.TryGetValue(gameSession.Code, out var list);
            if (list == null)
            {
                return;
            }


            foreach (var refAction in list.ToList())
            {
                if (refAction.TryGetTarget(out var action))
                {
                    action(gameSession);
                }
                else
                {
                    list.Remove(refAction);
                }
            }

        }

        public void Clear(string code)
        {
            if (subscribers.ContainsKey(code))
            {
                subscribers.Remove(code);
            }
        }
    }
}
