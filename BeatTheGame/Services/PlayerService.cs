using BeatTheGame.Data;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeatTheGame.Services
{
    public class PlayerService : IPlayerService
    {
        private static readonly Dictionary<Guid, Player> players = new Dictionary<Guid, Player>();
        private readonly ProtectedLocalStorage protectedLocalStorage;
        private const string playerSessionKey = "playerId";

        public PlayerService(ProtectedLocalStorage protectedLocalStorage)
        {
            this.protectedLocalStorage = protectedLocalStorage;
        }

        public async Task<Player> GetOrUpdatePlayerAsync(string name)
        {
            var storagePlayerId = await protectedLocalStorage.GetAsync<Guid>(playerSessionKey);
            var playerId = storagePlayerId.Success ? storagePlayerId.Value : Guid.NewGuid();
            if (!players.TryGetValue(playerId, out var player))
            {
                player = new Player(Guid.NewGuid(), name);
            }
            if (!storagePlayerId.Success)
            {
                await protectedLocalStorage.SetAsync(playerSessionKey, playerId);
            }
            if (player.Name != name)
            {
                player = player with { Name = name };
            }
            players[playerId] = player;

            return player;
        }

        public Player? GetPlayer(Guid playerId)
        {
            players.TryGetValue(playerId, out var player);
            return player;
        }

        public async Task<Player?> GetMyPlayerAsync()
        {
            try
            {
                var storagePlayerId = await protectedLocalStorage.GetAsync<Guid>(playerSessionKey);
                if (!storagePlayerId.Success)
                {
                    return null;
                }
                return GetPlayer(storagePlayerId.Value);
            }
            catch
            {
                // Player Id cannot be retrieved from the local storage because the app was redeployed
                // and the key was changed
                return null;
            }
        }
    }
}
