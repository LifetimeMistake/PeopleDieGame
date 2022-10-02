using PeopleDieGame.ServerPlugin.Enums;

namespace PeopleDieGame.ServerPlugin.Helpers
{
    public static class GameStateHelper
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
