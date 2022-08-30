﻿using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Commands.Admin
{
    public class ManageTeamsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "teams";

        public string Help => "";

        public string Syntax => "<list/inspect/create/remove/getSpawn/setSpawn/setName/setDescription/setLoadout> <teamName/teamId> [<name/description/spawnpoint/loadoutName/loadoutId>]";
        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, $"Musisz podać argument.");
                ShowSyntax(caller);
                return;
            }

            string[] verbArgs = command.Skip(1).ToArray();
            switch (command[0].ToLowerInvariant())
            {
                case "list":
                    VerbList(caller, verbArgs);
                    break;
                case "inspect":
                    VerbInspect(caller, verbArgs);
                    break;
                case "create":
                    VerbCreate(caller, verbArgs);
                    break;
                case "remove":
                    VerbRemove(caller, verbArgs);
                    break;
                case "getspawn":
                    VerbGetSpawn(caller, verbArgs);
                    break;
                case "setspawn":
                    VerbSetSpawn(caller, verbArgs);
                    break;
                case "setname":
                    VerbSetName(caller, verbArgs);
                    break;
                case "setdescription":
                    VerbSetDescription(caller, verbArgs);
                    break;
                case "setloadout":
                    VerbSetLoadout(caller, verbArgs);
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

        private void VerbList(IRocketPlayer caller, string[] command)
        {
            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"Lista drużyn:");
                foreach (Team team in teamManager.GetTeams().OrderBy(x => x.Id))
                {
                    sb.AppendLine($"ID: {team.Id} | Nazwa: {team.Name}");
                }

                UnturnedChat.Say(caller, sb.ToString());
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać informacji o liście drużyn z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbInspect(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                StringBuilder sb = new StringBuilder();

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                sb.AppendLine($"ID: {team.Id}");
                sb.AppendLine($"Nazwa: {team.Name}");

                if (team.Description == "")
                {
                    sb.AppendLine("Opis: Brak");
                }
                else
                {
                    sb.AppendLine($"Opis: \"{team.Description}\"");
                }

                if (!team.DefaultLoadoutId.HasValue)
                {
                    sb.AppendLine("ID wyposażenia drużyny: Brak");
                }
                else
                {
                    sb.AppendLine($"ID wyposażenia drużyny: {team.DefaultLoadoutId}");
                }

                //leader info

                RespawnPoint? teamRespawnPoint = team.RespawnPoint;
                if (teamRespawnPoint == null)
                {
                    sb.AppendLine("Respawn point: Brak");
                }
                else
                {
                    sb.AppendLine("Respawn point:");
                    sb.AppendLine($"\t{teamRespawnPoint.Value.Position}");
                    sb.AppendLine($"\t{teamRespawnPoint.Value.Rotation}");
                }

                //member info

                UnturnedChat.Say(caller, sb.ToString());

            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać informacji o drużynie z powodu błędu serwera: {ex.Message}");
            }

        }

        private void VerbCreate(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string name = string.Join(" ", command);
                if (teamManager.GetTeamByName(name) != null)
                {
                    UnturnedChat.Say(caller, "Drużyna o tej nazwie już istnieje");
                    return;
                }

                Team team = teamManager.CreateTeam(name);
                UnturnedChat.Say(caller, $"Utworzono drużynę o ID {team.Id}");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się utworzyć drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbRemove(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                if (!teamManager.DeleteTeam(team.Id))
                {
                    UnturnedChat.Say(caller, "Nie udało się usunąć drużyny");
                    return;
                }

                UnturnedChat.Say(caller, "Usunięto drużynę");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się usunąć drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbGetSpawn(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                RespawnPoint? teamRespawnPoint = team.RespawnPoint;
                if (teamRespawnPoint == null)
                {
                    UnturnedChat.Say(caller, "Drużyna nie posiada respawn pointu");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Respawn point drużyny \"{searchTerm}\":");
                sb.AppendLine($"\t{teamRespawnPoint.Value.Position}");
                sb.AppendLine($"\t{teamRespawnPoint.Value.Rotation}");

                UnturnedChat.Say(caller, sb.ToString());

            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać respawn pointu drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetSpawn(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);
                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                RespawnPoint? respawnPoint = new RespawnPoint(callerPlayer.Position, (byte)callerPlayer.Rotation);
                team.RespawnPoint = respawnPoint;
                UnturnedChat.Say(caller, "Ustawiono respawn point drużyny");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić respawn pointu z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetName(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                string name = string.Join(" ", command.Skip(1));
                team.SetName(name);

                UnturnedChat.Say(caller, $"Ustawiono nazwę drużyny na \"{name}\"");
            }
            catch (ArgumentException)
            {
                UnturnedChat.Say(caller, "Nazwa drużyny nie może być pusta");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić nazwy drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetDescription(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                string desc = string.Join(" ", command.Skip(1));
                team.SetDescription(desc);

                UnturnedChat.Say(caller, $"Ustawiono opis drużyny na \"{desc}\"");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić opisu z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetLoadout(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny i nazwę lub ID zestawu wyposażenia.");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();

                Team team = teamManager.ResolveTeam(command[0], false);
                if (team == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                Loadout loadout = loadoutManager.ResolveLoadout(command[1], false);
                if (loadout == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono zestawu wyposażenia");
                    return;
                }

                team.SetDefaultLoadout(loadout);
                UnturnedChat.Say(caller, "Ustawiono nowy zestaw wyposażenia!");
            }
            catch(Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zmienić domyślnego zestawu wyposażenia z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
