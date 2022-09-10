using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Enums;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Commands.Admin
{
    public class ManageGameCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "game";

        public string Help => "";

        public string Syntax => "<start/end/getState/setState> [<state>]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, $"Musisz podać argument.");
                ShowSyntax(caller);
                return;
            }

            switch (command[0].ToLowerInvariant())
            {
                case "start":
                    VerbStartGame(caller);
                    break;
                case "end":
                    VerbEndGame(caller);
                    break;
                case "getstate":
                    VerbGetState(caller);
                    break;
                case "setstate":
                    VerbSetState(caller, command.Skip(1).ToArray());
                    break;
                default:
                    ChatHelper.Say(caller, $"Nieprawidłowy argument.");
                    ShowSyntax(caller);
                    break;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbStartGame(IRocketPlayer caller)
        {
            GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();
            try
            {
                gameManager.StartGame();
                ChatHelper.Say(caller, "Rozpoczęto grę.");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się rozpocząć gry z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbEndGame(IRocketPlayer caller)
        {
            GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();
            try
            {
                gameManager.EndGame();
                ChatHelper.Say(caller, "Zakończono grę.");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się zakończyć gry z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbGetState(IRocketPlayer caller)
        {
            GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();
            try
            {
                GameState state = gameManager.GetGameState();
                string stateName = $"[{state}] {GameStateFriendlyNameProvider.GetFriendlyName(state)}";
                ChatHelper.Say(caller, $"Aktualny stan gry: {stateName}");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się odczytać stanu gry z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbSetState(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                ChatHelper.Say(caller, "Musisz podać stan gry.");
                ShowSyntax(caller);
                return;
            }

            try
            {
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();
                GameState gameState;
                if (!Enum.TryParse(command[0], out gameState))
                    throw new ArgumentException(nameof(command));

                gameManager.SetGameState(gameState);
                ChatHelper.Say(caller, "Ustawiono nowy stan gry.");
            }
            catch (ArgumentException)
            {
                ChatHelper.Say(caller, $"Niepoprawny stan gry, dozwolone wartości: {string.Join(", ", Enum.GetNames(typeof(GameState)))}");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić stanu gry z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
