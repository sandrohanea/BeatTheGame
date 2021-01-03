using BeatTheGame.Data;
using System;
using System.Threading.Tasks;

namespace BeatTheGame.Services
{
    public interface IPlayerService
    {
        Task<Player?> GetMyPlayerAsync();
        Task<Player> GetOrUpdatePlayerAsync(string name);
        Player? GetPlayer(Guid playerId);
    }
}