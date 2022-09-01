using Rocket.API;
using Rocket.Unturned.Chat;
using SDG.Unturned;
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
            UnturnedChat.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbList(IRocketPlayer caller, string[] command)
        {
            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();

                UnturnedChat.Say(caller, "Lista przedmiotów w sklepie:");
                foreach (ShopItem item in shopManager.GetItemList())
                {
                    UnturnedChat.Say(caller, $"ID: {item.UnturnedItemId} | Nazwa: {item.Name} | Cena: {item.Price}");
                }
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać listy przedmiotów z powodu błędu serwera: {ex.Message}");
            }
        }
        private void VerbAddItem(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID przedmiotu oraz jego cenę");
                return;
            }

            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                ShopItem shopItem = shopManager.ResolveItem(command[0], true);

                double price;
                if (!double.TryParse(command[1], out price))
                {
                    UnturnedChat.Say(caller, "Artykuł 13 paragraf 7 - kto defekuje się do paczkomatu");
                    return;
                }

                if (shopItem != null)
                {
                    UnturnedChat.Say(caller, $"Przedmiot {command[0]} znajduje się już w sklepie");
                    return;
                }

                ItemAsset item;
                ushort id;
                if (ushort.TryParse(command[0], out id)) // find item by id
                {
                    item = Assets.find(EAssetType.ITEM, id) as ItemAsset;
                    if (item == null)
                    {
                        UnturnedChat.Say(caller, $"Przedmiot z ID {id} nie istnieje");
                        return;
                    }
                }
                else // find item by name
                {
                    item = Assets.find(EAssetType.ITEM).FirstOrDefault(x => x.FriendlyName != null && x.FriendlyName.ToLowerInvariant().Contains(command[0].ToLowerInvariant())) as ItemAsset;
                    if (item == null)
                    {
                        UnturnedChat.Say(caller, $"Przedmiot o nazwie {command[0]} nie istnieje");
                        return;
                    }
                }

                shopItem = shopManager.AddItem(item.id, double.Parse(command[1]));
                UnturnedChat.Say(caller, $"Dodano przedmiot z ID: {shopItem.UnturnedItemId}, cena: ${shopItem.Price}");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się dodać przedmiotu do sklepu z powodu błędu serwera: {ex}");
            }
        }

        private void VerbRemoveItem(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID przedmiotu");
                return;
            }

            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                ShopItem shopItem = shopManager.ResolveItem(command[0], true);

                if (shopItem == null)
                {
                    UnturnedChat.Say(caller, $"Przedmiot {command[0]} nie znajduje się w sklepie lub nie istnieje");
                    return;
                }

                shopManager.RemoveItem(shopItem.UnturnedItemId);
                UnturnedChat.Say(caller, "Usunięto przedmiot ze sklepu");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się usunąć przedmiotu ze sklepu z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetPrice(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID przedmiotu oraz jego cenę");
                return;
            }

            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                ShopItem shopItem = shopManager.ResolveItem(command[0], true);

                if (shopItem == null)
                {
                    UnturnedChat.Say(caller, $"Przedmiot {command[0]} nie znajduje się w sklepie lub nie istnieje");
                    return;
                }

                shopManager.SetItemPrice(shopItem, double.Parse(command[1]));
            }
            catch (ArgumentOutOfRangeException)
            {
                UnturnedChat.Say(caller, "Cena przedmiotu nie może być ujemna");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić ceny przedmiotu z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbInspect(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID przedmiotu");
                return;
            }

            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                ShopItem shopItem = shopManager.ResolveItem(command[0], true);

                if (shopItem == null)
                {
                    UnturnedChat.Say(caller, $"Przedmiot {command[0]} nie znajduje się w sklepie lub nie istnieje");
                    return;
                }

                foreach (string line in shopManager.GetItemSummary(shopItem).Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    UnturnedChat.Say(caller, line);
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać informacji o przedmiocie z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
