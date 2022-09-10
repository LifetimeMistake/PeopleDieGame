using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Commands.Admin
{
    public class ManageArenasCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "arenas";

        public string Help => "";

        public string Syntax => "<inspect/setname/setboss/setactdist/setdeactdist/setreward/setbounty/setbossspawn/setrewardspawn> <arenaName/arenaId> <name/boss/distance/amount>";

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

            string[] verbArgs = command.Skip(1).ToArray();
            switch (command[0].ToLowerInvariant())
            {
                case "inspect":
                    VerbInspect(caller, verbArgs);
                    break;
                case "setname":
                    VerbSetName(caller, verbArgs);
                    break;
                case "setboss":
                    VerbSetBoss(caller, verbArgs);
                    break;
                case "setactdist":
                    VerbSetActDist(caller, verbArgs);
                    break;
                case "setdeactdist":
                    VerbSetDeactDist(caller, verbArgs);
                    break;
                case "setreward":
                    VerbSetReward(caller, verbArgs);
                    break;
                case "setbounty":
                    VerbSetBounty(caller, verbArgs);
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

        private void VerbInspect(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID areny");
                return;
            }
            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                BossArena arena = arenaManager.ResolveArena(command[0], false);

                if (arena == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono areny \"{command[0]}\"");
                    return;
                }

                ChatHelper.Say(caller, arenaManager.GetArenaSummary(arena));
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się pobrać informacji o arenie z powodu błędu serwera: {ex.Message}");
            }
        }
        private void VerbSetName(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID areny oraz nową nazwę");
                return;
            }

            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                BossArena arena = arenaManager.ResolveArena(command[0], false);

                if (arena == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono areny \"{command[0]}\"");
                    return;
                }

                string name = string.Join(" ", command.Skip(1));
                arena.SetName(name);

                ChatHelper.Say(caller, $"Ustawiono nazwę areny na {name}");
            }
            catch (ArgumentException)
            {
                ChatHelper.Say(caller, "Nazwa areny nie może być pusta");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się utworzyć areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetBoss(IRocketPlayer caller, string[] command)
        {
            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                BossArena arena = arenaManager.ResolveArena(command[0], false);

                if (arena == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono areny \"{command[0]}\"");
                    return;
                }

                string searchTerm = string.Join(" ", command.Skip(1));
                IBoss boss = ServiceLocator.Instance.LocateServicesOfType<IBoss>()
                    .FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()));

                if (boss == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono podanego boss'a");
                    return;
                }

                arena.SetBoss(boss);

                ChatHelper.Say(caller, "Ustawiono boss'a areny");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić boss'a areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetActDist(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID areny oraz dystans aktywacji");
                return;
            }

            double distance;
            if (!double.TryParse(command[1], out distance))
            {
                ChatHelper.Say(caller, "Artykuł 13 paragraf 7 - kto defekuje się do paczkomatu");
                return;
            }

            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                BossArena arena = arenaManager.ResolveArena(command[0], false);

                if (arena == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono areny \"{command[0]}\"");
                    return;
                }

                arena.SetActivationDistance(distance);
                ChatHelper.Say(caller, "Ustawiono dystans aktywacji areny");
            }
            catch (ArgumentOutOfRangeException)
            {
                ChatHelper.Say(caller, "Dystans nie może być ujemny");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić dystansu aktywacji areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetDeactDist(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID areny oraz dystans aktywacji");
                return;
            }

            double distance;
            if (!double.TryParse(command[1], out distance))
            {
                ChatHelper.Say(caller, "Artykuł 13 paragraf 7 - kto defekuje się do paczkomatu");
                return;
            }

            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                BossArena arena = arenaManager.ResolveArena(command[0], false);

                if (arena == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono areny \"{command[0]}\"");
                    return;
                }

                arena.SetDeactivationDistance(distance);
                ChatHelper.Say(caller, "Ustawiono dystans dezaktywacji areny");
            }
            catch (ArgumentOutOfRangeException)
            {
                ChatHelper.Say(caller, "Dystans nie może być ujemny");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić dystansy dezaktywacji areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetReward(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID areny oraz dystans aktywacji");
                return;
            }

            double reward;
            if (!double.TryParse(command[1], out reward))
            {
                ChatHelper.Say(caller, "Artykuł 13 paragraf 7 - kto defekuje się do paczkomatu");
                return;
            }

            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                BossArena arena = arenaManager.ResolveArena(command[0], false);

                if (arena == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono areny \"{command[0]}\"");
                    return;
                }

                arena.SetCompletionReward(reward);
                ChatHelper.Say(caller, "Ustawiono nagrodę areny");
            }
            catch (ArgumentOutOfRangeException)
            {
                ChatHelper.Say(caller, "Nagroda nie może być ujemna");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić nagrody areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetBounty(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID areny oraz dystans aktywacji");
                return;
            }

            double bounty;
            if (!double.TryParse(command[1], out bounty))
            {
                ChatHelper.Say(caller, "Artykuł 13 paragraf 7 - kto defekuje się do paczkomatu");
                return;
            }

            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                BossArena arena = arenaManager.ResolveArena(command[0], false);

                if (arena == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono areny \"{command[0]}\"");
                    return;
                }

                arena.SetCompletionBounty(bounty);
                ChatHelper.Say(caller, "Ustawiono bounty areny");
            }
            catch (ArgumentOutOfRangeException)
            {
                ChatHelper.Say(caller, "Bounty nie może być ujemne");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić bounty areny z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
