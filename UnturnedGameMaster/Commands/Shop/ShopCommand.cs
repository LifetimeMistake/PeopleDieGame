using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Commands.Shop
{
    public class ShopCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "shop";

        public string Help => "Polecenia dotyczące sklepu przedmiotów";

        public string Syntax => "<list/inspect/buy> <itemId/itemName> [<amount>]";

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
                case "buy":
                    VerbBuy(caller, verbArgs);
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
                    UnturnedChat.Say(caller, $"ID: {item.UnturnedItemId} | Nazwa: {item.Name} | Cena: ${item.Price}");
                }
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać listy przedmiotów z powodu błędu serwera: {ex.Message}");
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
                ShopItem shopItem = shopManager.ResolveItem(command[0], false);

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

        private void VerbBuy(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID przedmiotu i jego ilość");
                return;
            }

            byte amount;
            if (!byte.TryParse(command[1], out amount))
            {
                UnturnedChat.Say(caller, "Artykuł 13 paragraf 7 - kto defekuje się do paczkomatu");
                return;
            }

            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                ShopItem shopItem = shopManager.ResolveItem(command[0], false);
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);

                if (shopItem == null)
                {
                    UnturnedChat.Say(caller, $"Przedmiot {command[0]} nie znajduje się w sklepie lub nie istnieje");
                    return;
                }

                if (!shopManager.BuyItem(shopItem, callerPlayerData, amount))
                {
                    UnturnedChat.Say(caller, "Bank twojej drużyny posiada niewystarczającą ilość środków by zakupić przedmiot(y)");
                    return;
                }

                UnturnedChat.Say(caller, $"Zakupiono {shopItem.Name} (x{amount})");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zakupić przedmiotu z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
