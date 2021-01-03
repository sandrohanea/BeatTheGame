using BeatTheGame.Data;

namespace BeatTheGame.Services
{
    public interface IDeckService
    {
        Deck GenerateDeck(GameSessionConfiguration gameSessionConfiguration);
    }
}