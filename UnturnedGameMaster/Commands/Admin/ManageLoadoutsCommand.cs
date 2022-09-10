using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Managers;

namespace UnturnedGameMaster.Commands.Admin
{
    public class ManageLoadoutsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "loadouts";

        public string Help => "";

        public string Syntax => "<inspect/create/remove/additems/removeitem/setname/setdescription> <loadout name/loadout id> [<item id>] [<item amount>] <name/description>";

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
                case "create":
                    VerbCreate(caller, verbArgs);
                    break;
                case "remove":
                    VerbRemove(caller, verbArgs);
                    break;
                case "additems":
                    VerbAddItems(caller, verbArgs);
                    break;
                case "removeitems":
                    VerbRemoveItems(caller, verbArgs);
                    break;
                case "setname":
                    VerbSetName(caller, verbArgs);
                    break;
                case "setdescription":
                    VerbSetDescription(caller, verbArgs);
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

        public void VerbInspect(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID zestawu");
                return;
            }
            try
            {
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();
                string searchTerm = string.Join(" ", command);
                Loadout loadout = loadoutManager.ResolveLoadout(searchTerm, false);

                if (loadout == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono zestawu \"{searchTerm}\"");
                    return;
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"ID: {loadout.Id}");
                sb.AppendLine($"Nazwa: {loadout.Name}");
                sb.AppendLine($"Opis: {loadout.Description}");

                sb.AppendLine($"Przedmioty:");
                foreach (KeyValuePair<int, int> item in loadout.GetItems())
                {
                    sb.AppendLine($"\t{item.Key} x{item.Value}");
                }

                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się pobrać informacji o zestawie z powodu błędu serwera: {ex.Message}");
            }
        }

        public void VerbCreate(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę zestawu");
                return;
            }

            try
            {
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();
                if (loadoutManager.GetLoadoutByName(command[0]) != null)
                {
                    ChatHelper.Say(caller, "Zestaw wyposażenia o tej nazwie już istnieje!");
                    return;
                }

                Loadout loadout = loadoutManager.CreateLoadout(command[0]);
                foreach (string item in command.Skip(1))
                {
                    string itemIdString = item;
                    int itemId;
                    int amount = 1;
                    if (item.Contains("/"))
                    {
                        string[] parts = item.Split('/');
                        itemIdString = parts[0];

                        if (!int.TryParse(parts[1], out amount))
                        {
                            ChatHelper.Say(caller, $"Warn: Nie udało się przetworzyć liczby przedmiotów \"{parts[1]}\", ustawianie wartości na 1.");
                            amount = 1;
                        }
                    }

                    if (!int.TryParse(itemIdString, out itemId))
                    {
                        ChatHelper.Say(caller, $"Warn: Nie udało się przetworzyć przedmiotu o Id \"{itemIdString}\", przedmiot pominięty.");
                        continue; // failed to parse item Id
                    }

                    loadout.AddItem(itemId, amount);
                }

                ChatHelper.Say(caller, $"Utworzono zestaw wyposażenia z ID {loadout.Id}");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się utworzyć zestawu wyposażenia z powodu błedu serwera: {ex.Message}");
            }
        }

        public void VerbRemove(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID zestawu");
                return;
            }
            try
            {
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();
                string searchTerm = string.Join(" ", command);
                Loadout loadout = loadoutManager.ResolveLoadout(searchTerm, false);

                if (loadout == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono wyposażenia \"{searchTerm}\"");
                    return;
                }

                loadoutManager.DeleteLoadout(loadout.Id);
                ChatHelper.Say(caller, "Usunięto wyposażenie");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się usunąć zestawu wyposażenia z powodu błędu serwera: {ex.Message}");
            }
        }

        public void VerbAddItems(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID zestawu oraz ID przedmiotu");
                return;
            }

            try
            {
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();
                Loadout loadout = loadoutManager.ResolveLoadout(command[0], false);

                if (loadout == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono zestawu \"{command[0]}\"");
                    return;
                }

                foreach (string item in command.Skip(1))
                {
                    string itemIdString = item;
                    int itemId;
                    int amount = 1;
                    if (item.Contains("/"))
                    {
                        string[] parts = item.Split('/');
                        itemIdString = parts[0];

                        if (!int.TryParse(parts[1], out amount))
                        {
                            ChatHelper.Say(caller, $"Warn: Nie udało się przetworzyć liczby przedmiotów \"{parts[1]}\", ustawianie wartości na 1.");
                            amount = 1;
                        }
                    }

                    if (!int.TryParse(itemIdString, out itemId))
                    {
                        ChatHelper.Say(caller, $"Warn: Nie udało się przetworzyć przedmiotu o Id \"{itemIdString}\", przedmiot pominięty.");
                        continue; // failed to parse item Id
                    }

                    loadout.AddItemOrAddAmount(itemId, amount);
                }
                ChatHelper.Say(caller, "Dodano przedmioty do zestawu wyposażenia");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się dodać przedmiotu do zestawu z powodu błędu serwera: {ex.Message}");
            }
        }

        public void VerbRemoveItems(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                ChatHelper.Say(caller, $"Musisz podać nazwę lub ID zestawu oraz ID przedmiotu");
                return;
            }

            try
            {
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();
                Loadout loadout = loadoutManager.ResolveLoadout(command[0], false);

                if (loadout == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono zestawu \"{command[0]}\"");
                    return;
                }

                foreach (string item in command.Skip(1))
                {
                    int itemId;

                    if (!int.TryParse(item, out itemId))
                    {
                        ChatHelper.Say(caller, $"Warn: Nie udało się przetworzyć przedmiotu o Id \"{item}\", przedmiot pominięty.");
                        continue; // failed to parse item Id
                    }

                    loadout.RemoveItem(itemId);
                }
                ChatHelper.Say(caller, "Usunięto przedmioty z zestawu wyposażenia");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się usunąć przedmiotu z zestawu z powodu błędu serwera: {ex.Message}");
            }
        }

        public void VerbSetName(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID zestawu");
                return;
            }

            try
            {
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();
                Loadout loadout = loadoutManager.ResolveLoadout(command[0], false);

                if (loadout == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono zestawu \"{command[0]}\"");
                    return;
                }

                string name = string.Join(" ", command.Skip(1));
                loadout.SetName(name);

                ChatHelper.Say(caller, $"Ustawiono nazwę zestawu na \"{name}\"");
            }
            catch (ArgumentException)
            {
                ChatHelper.Say(caller, "Nazwa zestawu nie może być pusta");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić nazwy zestawu wyposażenia z powodu błędu serwera: {ex.Message}");
            }
        }

        public void VerbSetDescription(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID zestawu");
                return;
            }

            try
            {
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();
                Loadout loadout = loadoutManager.ResolveLoadout(command[0], false);

                if (loadout == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono zestawu \"{command[0]}\"");
                    return;
                }

                string desc = string.Join(" ", command.Skip(1));
                loadout.SetDescription(desc);

                ChatHelper.Say(caller, $"Ustawiono opis zestawu na \"{desc}\"");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić opisu zestawu z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
