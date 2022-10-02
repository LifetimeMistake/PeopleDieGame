using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Commands.Admin
{
    public class ManageArenasCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "arenas";

        public string Help => "";

        public string Syntax => "<inspect/list/remove/setname/setboss/setactdist/setdeactdist/setreward/setbounty/setbossspawn/setrewardspawn/setrewardloadout> <arenaName/arenaId> <name/boss/distance/amount>";

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
                case "list":
                    VerbList(caller);
                    break;
                case "remove":
                    VerbRemove(caller, verbArgs);
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
                case "setpoolsize":
                    VerbSetPoolSize(caller, verbArgs);
                    break;
                case "setrewardloadout":
                    VerbSetRewardLoadout(caller, verbArgs);
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

        private void VerbList(IRocketPlayer caller)
        {
            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();

                StringBuilder sb = new StringBuilder();
                ChatHelper.Say(caller, $"Lista aren:");
                foreach (BossArena arena in arenaManager.GetArenas().OrderBy(x => x.Id))
                {
                    sb.AppendLine($"ID: {arena.Id} | Nazwa: {arena.Name}");
                }

                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać informacji o liście aren z powodu błędu serwera: {ex.Message}");
            }
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
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać informacji o arenie z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbRemove(IRocketPlayer caller, string[] command)
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

                if (!arenaManager.DeleteArena(arena.Id))
                {
                    ChatHelper.Say(caller, $"Nie udało się usunąć areny z powodu błędu systemu");
                    return;
                }
                ChatHelper.Say(caller, "Usunięto arenę");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się usunąć areny z powodu błędu serwera: {ex.Message}");
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
                ExceptionHelper.Handle(ex, caller, $"Nie udało się utworzyć areny z powodu błędu serwera: {ex.Message}");
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
                IZombieModel boss = ServiceLocator.Instance.LocateServicesOfType<IZombieModel>()
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
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić boss'a areny z powodu błędu serwera: {ex.Message}");
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
                ChatHelper.Say(caller, "Odległość nie może być ujemna");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić dystansu aktywacji areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetDeactDist(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID areny oraz dystans deaktywacji");
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
                ChatHelper.Say(caller, "Odległość nie może być ujemna");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić dystansy dezaktywacji areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetReward(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID areny oraz wartość nagrody");
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
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić nagrody areny z powodu błędu serwera: {ex.Message}");
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
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić bounty areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetPoolSize(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID areny oraz rozmiar puli spawnów");
                return;
            }

            if (!int.TryParse(command[1], out int poolSize) || poolSize < 0)
            {
                ChatHelper.Say(caller, "Podałeś niepoprawny rozmiar puli");
                return;
            }

            bool force = false;
            if (command.Length >= 3 && !bool.TryParse(command[2], out force))
            {
                ChatHelper.Say(caller, "Podałeś niepoprawną wartość parametru force");
                return;
            }

            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                ZombiePoolManager zombiePoolManager = ServiceLocator.Instance.LocateService<ZombiePoolManager>();
                BossArena arena = arenaManager.ResolveArena(command[0], false);

                if (arena == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono areny \"{command[0]}\"");
                    return;
                }

                if (!zombiePoolManager.ResizeZombiePool(arena.BoundId, poolSize, force))
                {
                    ChatHelper.Say(caller, $"Nie udało się zmienić rozmiaru puli");
                    return;
                }

                ChatHelper.Say(caller, $"Ustawiono pulę zombie na {poolSize}");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić wielkości puli spawnów areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetRewardLoadout(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID areny oraz nazwę lub ID zestawu nagród");
                return;
            }

            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();
                BossArena arena = arenaManager.ResolveArena(command[0], false);

                if (arena == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono areny \"{command[0]}\"");
                    return;
                }

                string searchTerm = string.Join(" ", command.Skip(1));
                Loadout loadout = loadoutManager.ResolveLoadout(searchTerm, false);
                if (loadout == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono zestawu nagród \"{searchTerm}\"");
                    return;
                }

                arena.SetRewardLoadout(loadout);
                ChatHelper.Say(caller, "Ustawiono zestaw nagród");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić bounty areny z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
