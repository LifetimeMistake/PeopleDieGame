using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Commands.Admin
{
    public class ArenaBuilderCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "arena";

        public string Help => "";

        public string Syntax => "<create/cancel/setname/setactdist/setdeactdist/setreward/setbounty/setactpoint/setbossspawn/setrewardspawn/setboss/setpoolsize/setrewardloadout> <value>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public Dictionary<string, ArenaBuilder> Builders = new Dictionary<string, ArenaBuilder>();

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
                case "create":
                    VerbCreate(caller);
                    break;
                case "cancel":
                    VerbCancel(caller);
                    break;
                case "setname":
                    VerbSetName(caller, verbArgs);
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
                case "setactpoint":
                    VerbSetActPoint(caller);
                    break;
                case "setbossspawn":
                    VerbSetBossSpawn(caller);
                    break;
                case "setrewardspawn":
                    VerbSetRewardSpawn(caller);
                    break;
                case "setboss":
                    VerbSetBoss(caller, verbArgs);
                    break;
                case "setpoolsize":
                    VerbSetPoolSize(caller, verbArgs);
                    break;
                case "setrewardloadout":
                    VerbSetRewardLoadout(caller, verbArgs);
                    break;
                case "submit":
                    VerbSubmit(caller);
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

        private void VerbCreate(IRocketPlayer caller)
        {
            try
            {
                ArenaBuilder arenaBuilder = new ArenaBuilder();

                if (Builders.ContainsKey(caller.Id))
                {
                    ChatHelper.Say(caller, "Rozpocząłeś już proces tworzenia areny");
                    return;
                }

                Builders.Add(caller.Id, arenaBuilder);
                ChatHelper.Say(caller, "Rozpoczęto proces tworzenia areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się rozpocząć procesu tworzenia areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbCancel(IRocketPlayer caller)
        {
            try
            {
                if (!Builders.Remove(caller.Id))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }
                ChatHelper.Say(caller, "Anulowano proces tworzenia areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się anulować procesu tworzenia areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetName(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać nazwę areny");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }

                string name = string.Join(" ", command);
                arenaBuilder.SetName(name);

                ChatHelper.Say(caller, "Ustawiono nazwę areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić nazwy areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetActDist(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać dystans aktywacji areny");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }

                if (!double.TryParse(command[0], out double distance))
                {
                    ChatHelper.Say(caller, "Musisz podać odpowiedni dystans aktywacji areny");
                    return;
                }

                arenaBuilder.SetActivationDistance(distance);

                ChatHelper.Say(caller, "Ustawiono dystans aktywacji areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić dystansu aktywacji areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetDeactDist(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać dystans dezaktywacji areny");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }

                if (!double.TryParse(command[0], out double distance))
                {
                    ChatHelper.Say(caller, "Musisz podać odpowiedni dystans dezaktywacji areny");
                    return;
                }

                arenaBuilder.SetDeactivationDistance(distance);

                ChatHelper.Say(caller, "Ustawiono dystans dezaktywacji areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić dystansu dezaktywacji areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetReward(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać wyskość nagrody areny");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }

                if (!float.TryParse(command[0], out float amount))
                {
                    ChatHelper.Say(caller, "Musisz podać odpowiednią wyskość nagrody areny");
                    return;
                }

                arenaBuilder.SetCompletionReward(amount);

                ChatHelper.Say(caller, "Ustawiono wysokość nagrody areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić wysokości nagrody areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetBounty(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać wyskość bounty areny");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }

                if (!float.TryParse(command[0], out float amount))
                {
                    ChatHelper.Say(caller, "Musisz podać odpowiednią wyskość bounty areny");
                    return;
                }

                arenaBuilder.SetCompletionBounty(amount);

                ChatHelper.Say(caller, "Ustawiono wysokość bounty areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić wysokości bounty areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetActPoint(IRocketPlayer caller)
        {
            try
            {
                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }

                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);

                arenaBuilder.SetActivationPoint(callerPlayer.Position);

                ChatHelper.Say(caller, "Ustawiono punkt aktywacji areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić punktu aktywacji areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetBossSpawn(IRocketPlayer caller)
        {
            try
            {
                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }

                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);
                VectorPAR playerPos = new VectorPAR(callerPlayer.Position, (byte)callerPlayer.Rotation);

                arenaBuilder.SetBossSpawnPoint(playerPos);

                ChatHelper.Say(caller, "Ustawiono punkt spawnu boss'a areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić punktu spawnu boss'a areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetRewardSpawn(IRocketPlayer caller)
        {
            try
            {
                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }

                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);
                arenaBuilder.SetRewardSpawnPoint(callerPlayer.Position);

                ChatHelper.Say(caller, "Ustawiono punkt spawnu nagrody areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić punktu spawnu nagrody areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetBoss(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać nazwę boss'a");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }

                string searchTerm = string.Join(" ", command);
                IZombieModel boss = ServiceLocator.Instance.LocateServicesOfType<IZombieModel>()
                    .FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()));

                if (boss == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono podanego boss'a");
                    return;
                }

                arenaBuilder.SetBoss(boss);

                ChatHelper.Say(caller, "Ustawiono boss'a areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić nazwy boss'a areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetPoolSize(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                ChatHelper.Say(caller, $"Musisz podać rozmiar puli spawnów");
                return;
            }

            if (!int.TryParse(command[0], out int poolSize) || poolSize < 0)
            {
                ChatHelper.Say(caller, "Podałeś niepoprawny rozmiar puli");
                return;
            }

            try
            {
                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }

                arenaBuilder.SetZombiePoolSize(poolSize);
                ChatHelper.Say(caller, $"Ustawiono pulę zombie na {poolSize}");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić wielkości puli spawnów areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetRewardLoadout(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID zestawu nagród");
                return;
            }

            try
            {
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpcząłeś procesu tworzenia areny");
                    return;
                }

                string searchTerm = string.Join(" ", command);
                Loadout loadout = loadoutManager.ResolveLoadout(searchTerm, false);

                if (loadout == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono zestawu wyposażenia \"{searchTerm}\"");
                    return;
                }

                arenaBuilder.SetRewardLoadout(loadout);
                ChatHelper.Say(caller, $"Ustawiono nagrodę na \"{loadout.Name}\"");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić wielkości puli spawnów areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSubmit(IRocketPlayer caller)
        {
            try
            {
                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpocząłeś procesu tworzenia areny");
                    return;
                }

                if (string.IsNullOrWhiteSpace(arenaBuilder.ArenaName))
                {
                    ChatHelper.Say(caller, "Nie ustawiono nazwy areny");
                    return;
                }

                if (arenaBuilder.ActivationPoint == default(Vector3))
                {
                    ChatHelper.Say(caller, "Nie ustawiono punktu aktywacji areny");
                    return;
                }

                if (arenaBuilder.BossSpawnpoint.Position == default(Vector3))
                {
                    ChatHelper.Say(caller, "Nie ustawiono punktu spawnu bossa");
                    return;
                }

                if (arenaBuilder.BossModel == null)
                {
                    ChatHelper.Say(caller, "Nie ustawiono bossa areny");
                    return;
                }

                if (arenaBuilder.ZombiePoolSize == 0)
                {
                    ChatHelper.Say(caller, "Ostrzeżenie: Rozmiar puli zombie jest ustawiony na 0");
                }

                if (arenaBuilder.ActivationDistance == 0)
                {
                    ChatHelper.Say(caller, "Ostrzeżenie: Odległość aktywacji jest ustawiona na 0");
                }

                if (arenaBuilder.DeactivationDistance == 0)
                {
                    ChatHelper.Say(caller, "Ostrzeżenie: Odległość deaktywacji jest ustawiona na 0");
                }

                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                arenaManager.CreateArena(arenaBuilder);
                Builders.Remove(caller.Id);

                ChatHelper.Say(caller, "Utworzono arenę");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zatwierdzić areny z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
