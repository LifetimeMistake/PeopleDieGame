using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Providers
{
    public static class GameStateFriendlyNameProvider
    {
        public static string GetFriendlyName(GameState state)
        {
            switch (state)
            {
                case GameState.InLobby:
                    return "W lobby";
                case GameState.Intermission:
                    return "W grze (okres przygotowania)";
                case GameState.InGame:
                    return "W grze";
                case GameState.Finished:
                    return "Gra zakończona";
                default:
                    return "Nieznany";
            }
        }
    }
}
