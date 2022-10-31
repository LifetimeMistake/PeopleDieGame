using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Commands.Teams
{
    public class RejectInviteCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "reject";

        public string Help => "Odrzuca oczekujące zaproszenie do drużyny.";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    ChatHelper.Say(caller, "Nie można odrzucać zaproszeń do drużyn po rozpoczęciu gry!");
                    return;
                }

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Już należysz do drużyny!");
                    return;
                }

                TeamInvite invite = teamManager.GetInvite(callerPlayerData);
                if (invite == null)
                {
                    ChatHelper.Say(caller, "Nie posiadasz oczekującego zaproszenia do drużyny.");
                    return;
                }

                if (!teamManager.RejectInvite(callerPlayerData))
                {
                    ChatHelper.Say(caller, "Nie udało się odrzucić zaproszenia z powodu błedu systemu.");
                    return;
                }

                ChatHelper.Say(caller, "Odrzucono zaproszenie.");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się odrzucić zaproszenia z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
