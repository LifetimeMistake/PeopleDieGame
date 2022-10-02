using Rocket.API;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Commands.Admin
{
    public class ManageShopCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "manageshop";

        public string Help => "";

        public string Syntax => "<list/additem/removeitem/setprice/inspect> <itemId/itemName> <price>";
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
                    VerbList(caller, verbArgs);
                    break;
                case "additem":
                    VerbAddItem(caller, verbArgs);
                    break;
                case "removeitem":
                    VerbRemoveItem(caller, verbArgs);
                    break;
                case "setprice":
                    VerbSetPrice(caller, verbArgs);
                    break;
                case "inspect":
                    VerbInspect(caller, verbArgs);
                    break;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbList(IRocketPlayer caller, string[] command)
        {
            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();

                ChatHelper.Say(caller, "Lista przedmiotów w sklepie:");
                foreach (ShopItem item in shopManager.GetItemList())
                {
                    ChatHelper.Say(caller, $"ID: {item.UnturnedItemId} | Nazwa: \"{item.Name}\" | Cena: ${item.Price}");
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać listy przedmiotów z powodu błędu serwera: {ex.Message}");
            }
        }
        private void VerbAddItem(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID przedmiotu oraz jego cenę");
                return;
            }

            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                ShopItem shopItem = shopManager.ResolveItem(command[0], false);

                double price;
                if (!double.TryParse(command[1], out price))
                {
                    ChatHelper.Say(caller, "Artykuł 13 paragraf 7 - kto defekuje się do paczkomatu");
                    return;
                }

                if (shopItem != null)
                {
                    ChatHelper.Say(caller, $"Przedmiot \"{shopItem.Name}\" znajduje się już w sklepie");
                    return;
                }

                ItemAsset item;
                ushort id;
                if (ushort.TryParse(command[0], out id)) // find item by id
                {
                    item = Assets.find(EAssetType.ITEM, id) as ItemAsset;
                    if (item == null)
                    {
                        ChatHelper.Say(caller, $"Przedmiot z ID {id} nie istnieje");
                        return;
                    }
                }
                else // find item by name
                {
                    item = Assets.find(EAssetType.ITEM).FirstOrDefault(x => x.FriendlyName != null && x.FriendlyName.ToLowerInvariant().Contains(command[0].ToLowerInvariant())) as ItemAsset;
                    if (item == null)
                    {
                        ChatHelper.Say(caller, $"Przedmiot o nazwie \"{command[0]}\" nie istnieje");
                        return;
                    }
                }

                shopItem = shopManager.AddItem(item.id, price);
                if (shopItem == null)
                {
                    ChatHelper.Say(caller, "Nie udało się dodać przedmiotu do sklepu");
                    return;
                }

                ChatHelper.Say(caller, $"Dodano przedmiot z ID: {shopItem.UnturnedItemId}, cena: ${shopItem.Price}");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się dodać przedmiotu do sklepu z powodu błędu serwera: {ex}");
            }
        }

        private void VerbRemoveItem(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID przedmiotu");
                return;
            }

            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                ShopItem shopItem = shopManager.ResolveItem(command[0], false);

                if (shopItem == null)
                {
                    ChatHelper.Say(caller, $"Przedmiot \"{command[0]}\" nie znajduje się w sklepie lub nie istnieje");
                    return;
                }

                if (!shopManager.RemoveItem(shopItem.UnturnedItemId))
                {
                    ChatHelper.Say(caller, "Nie udało się usunąć przedmiotu z sklepu z powodu błedu systemu");
                    return;
                }

                ChatHelper.Say(caller, "Usunięto przedmiot ze sklepu");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się usunąć przedmiotu ze sklepu z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetPrice(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID przedmiotu oraz jego cenę");
                return;
            }

            double price;
            if (!double.TryParse(command[1], out price))
            {
                ChatHelper.Say(caller, "Artykuł 13 paragraf 7 - kto defekuje się do paczkomatu");
                return;
            }

            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                ShopItem shopItem = shopManager.ResolveItem(command[0], false);

                if (shopItem == null)
                {
                    ChatHelper.Say(caller, $"Przedmiot \"{command[0]}\" nie znajduje się w sklepie lub nie istnieje");
                    return;
                }

                shopManager.SetItemPrice(shopItem, price);
                ChatHelper.Say(caller, "Ustawiono cenę przedmiotu");
            }
            catch (ArgumentOutOfRangeException)
            {
                ChatHelper.Say(caller, "Cena przedmiotu nie może być ujemna");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić ceny przedmiotu z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbInspect(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID przedmiotu");
                return;
            }

            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                ShopItem shopItem = shopManager.ResolveItem(command[0], true);

                if (shopItem == null)
                {
                    ChatHelper.Say(caller, $"Przedmiot \"{command[0]}\" nie znajduje się w sklepie lub nie istnieje");
                    return;
                }

                ChatHelper.Say(caller, shopManager.GetItemSummary(shopItem));
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać informacji o przedmiocie z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
