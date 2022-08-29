using Rocket.API;
using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Enums;
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
            if(command.Length == 0)
            {
                UnturnedChat.Say(caller, $"Musisz podać argument.");
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
                    UnturnedChat.Say(caller, $"Nieprawidłowy argument.");
                    ShowSyntax(caller);
                    break;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            UnturnedChat.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbStartGame(IRocketPlayer caller)
        {
            GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();
            try
            {
                gameManager.StartGame();
                UnturnedChat.Say(caller, "Rozpoczęto grę.");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się rozpocząć gry z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbEndGame(IRocketPlayer caller)
        {
            GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();
            try
            {
                gameManager.EndGame();
                UnturnedChat.Say(caller, "Zakończono grę.");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zakończyć gry z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbGetState(IRocketPlayer caller)
        {
            GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();
            try
            {
                GameState state = gameManager.GetGameState();
                string stateName = $"[{state}] {GameStateFriendlyNameProvider.GetFriendlyName(state)}";
                UnturnedChat.Say(caller, $"Aktualny stan gry: {stateName}");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się odczytać stanu gry z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbSetState(IRocketPlayer caller, string[] command)
        { 
            if(command.Length != 1)
            {
                UnturnedChat.Say(caller, "Musisz podać stan gry.");
                ShowSyntax(caller);
            }

            try
            {
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();
                GameState gameState;
                if (!Enum.TryParse(command[0], out gameState))
                    throw new ArgumentException(nameof(command));

                gameManager.SetGameState(gameState);
                UnturnedChat.Say(caller, "Ustawiono nowy stan gry.");
            }
            catch(ArgumentException)
            {
                UnturnedChat.Say(caller, $"Niepoprawny stan gry, dozwolone wartości: {string.Join(", ", Enum.GetNames(typeof(GameState)))}");
            }
            catch(Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić stanu gry z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
