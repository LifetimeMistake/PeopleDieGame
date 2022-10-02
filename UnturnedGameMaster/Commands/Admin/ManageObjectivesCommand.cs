using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Commands.Admin
{
    public class ManageObjectivesCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "objectives";

        public string Help => "";

        public string Syntax => "<list/inspect/create/remove/map/resetstate> <itemId> <arenaId>";

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
                case "list":
                    VerbList(caller);
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
                case "map":
                    VerbMap(caller, verbArgs);
                    break;
                case "resetstate":
                    VerbResetState(caller, verbArgs);
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
                ObjectiveManager objectiveManager = ServiceLocator.Instance.LocateService<ObjectiveManager>();
                StringBuilder sb = new StringBuilder();

                ObjectiveItem[] objectiveItems = objectiveManager.GetObjectiveItems();


                if (objectiveItems.Length == 0)
                {
                    ChatHelper.Say(caller, "Brak artefaktów do wylistowania");
                    return;
                }

                sb.AppendLine("Lista artefaktów");
                foreach (ObjectiveItem objectiveItem in objectiveItems)
                {
                    sb.AppendLine($"ID: {objectiveItem.ItemId} | Stan: {objectiveItem.State}");
                }

                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać informacji o liście artefaktów z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbInspect(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać ID artefaktu");
                return;
            }

            if (!ushort.TryParse(command[0], out ushort itemId))
            {
                ChatHelper.Say(caller, "Musisz podać prawidłowe ID");
                return;
            }

            try
            {
                ObjectiveManager objectiveManager = ServiceLocator.Instance.LocateService<ObjectiveManager>();
                ObjectiveItem objectiveItem = objectiveManager.GetObjectiveItem(itemId);

                if (objectiveItem == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono artefaktu z ID: {itemId}");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"ID: {objectiveItem.ItemId}");
                sb.AppendLine($"ID areny: {objectiveItem.ArenaId}");
                sb.AppendLine($"Stan: {objectiveItem.State}");


                Vector3S? location = objectiveManager.GetObjectiveItemLocation(objectiveItem.ItemId);
                if (location != null)
                    sb.AppendLine($"Pozycja: {location}");

                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać informacji o artefakcie z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbCreate(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                ChatHelper.Say(caller, "Musisz podać ID artefaktu oraz ID lub nazwę areny");
                return;
            }

            if (!ushort.TryParse(command[0], out ushort itemId))
            {
                ChatHelper.Say(caller, "Musisz podać prawidłowe ID");
                return;
            }

            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                ObjectiveManager objectiveManager = ServiceLocator.Instance.LocateService<ObjectiveManager>();
                BossArena arena = arenaManager.ResolveArena(command[1], false);

                if (arena == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono areny \"{command[1]}\"");
                    return;
                }

                if (!objectiveManager.CreateObjectiveItem(itemId, arena))
                {
                    ChatHelper.Say(caller, "Nie udało się utworzyć artefaktu");
                    return;
                }

                ChatHelper.Say(caller, $"Utworzono artefakt z ID: {itemId}");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się utworzyć artefaktu z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbRemove(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać ID artefaktu");
                return;
            }

            if (!ushort.TryParse(command[0], out ushort itemId))
            {
                ChatHelper.Say(caller, "Musisz podać prawidłowe ID");
                return;
            }

            try
            {
                ObjectiveManager objectiveManager = ServiceLocator.Instance.LocateService<ObjectiveManager>();

                if (!objectiveManager.RemoveObjectiveItem(itemId))
                {
                    ChatHelper.Say(caller, $"Podany artefakt nie istnieje");
                    return;
                }
                ChatHelper.Say(caller, $"Usunięto artefakt z ID: {itemId}");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się usunąć artefaktu z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbMap(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                ChatHelper.Say(caller, "Musisz podać ID artefaktu oraz ID lub nazwę areny");
                return;
            }

            if (!ushort.TryParse(command[0], out ushort itemId))
            {
                ChatHelper.Say(caller, "Musisz podać prawidłowe ID");
                return;
            }

            try
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                ObjectiveManager objectiveManager = ServiceLocator.Instance.LocateService<ObjectiveManager>();
                BossArena arena = arenaManager.ResolveArena(command[1], false);

                if (arena == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono areny \"{command[1]}\"");
                    return;
                }

                if (!objectiveManager.MapObjectiveItemToArena(itemId, arena))
                {
                    ChatHelper.Say(caller, $"Nie znaleziono artefaktu z ID {itemId}");
                    return;
                }

                ChatHelper.Say(caller, $"Przydzielono artefakt z ID: {itemId} do areny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się przydzielić artefaktu do areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbResetState(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać ID artefaktu");
                return;
            }

            if (!ushort.TryParse(command[0], out ushort itemId))
            {
                ChatHelper.Say(caller, "Musisz podać prawidłowe ID");
                return;
            }

            try
            {
                ObjectiveManager objectiveManager = ServiceLocator.Instance.LocateService<ObjectiveManager>();

                if (!objectiveManager.ResetObjectiveItemState(itemId))
                {
                    ChatHelper.Say(caller, $"Podany artefakt nie istnieje");
                    return;
                }
                ChatHelper.Say(caller, $"Zresetowano stan artefaktu z ID: {itemId}");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zresetować stanu artefaktu z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
