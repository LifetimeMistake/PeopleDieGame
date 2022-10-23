using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Commands.Shop
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
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbList(IRocketPlayer caller, string[] command)
        {
            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();

                StringBuilder sb = new StringBuilder();
                ChatHelper.Say(caller, "Lista przedmiotów w sklepie:");
                foreach (ShopItem item in shopManager.GetItemList())
                {
                    sb.AppendLine($"ID: {item.UnturnedItemId} | Nazwa: \"{item.Name}\" | Cena: ${item.Price}");
                }
                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać listy przedmiotów z powodu błędu serwera: {ex.Message}");
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
                ShopItem shopItem = shopManager.ResolveItem(command[0], false);

                if (shopItem == null)
                {
                    ChatHelper.Say(caller, $"Przedmiot {command[0]} nie znajduje się w sklepie lub nie istnieje");
                    return;
                }

                ChatHelper.Say(caller, shopManager.GetItemSummary(shopItem));
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać informacji o przedmiocie z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbBuy(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID przedmiotu");
                return;
            }

            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InGame)
                {
                    ChatHelper.Say(caller, "Nie można korzystać z sklepu w poczekalni!");
                    return;
                }

                byte amount;
                if (command.Length == 1)
                {
                    amount = 1;
                }
                else if (!byte.TryParse(command[1], out amount))
                {
                    ChatHelper.Say(caller, "Artykuł 13 paragraf 7 - kto defekuje się do paczkomatu");
                    return;
                }

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (!callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Musisz należeć do drużyny, by korzystać z sklepu.");
                    return;
                }

                ShopItem shopItem = shopManager.ResolveItem(command[0], false);

                if (shopItem == null)
                {
                    ChatHelper.Say(caller, $"Przedmiot {command[0]} nie znajduje się w sklepie lub nie istnieje");
                    return;
                }

                if (!shopManager.CanBuyItem(shopItem, callerPlayerData, amount))
                {
                    ChatHelper.Say(caller, "Bank twojej drużyny posiada niewystarczającą ilość środków by zakupić przedmiot(y)");
                    return;
                }

                if (!shopManager.BuyItem(shopItem, callerPlayerData, amount))
                {
                    ChatHelper.Say(caller, "Nie udało się zakupić przedmiotu/ów z powodu błędu systemu.");
                    return;
                }

                ChatHelper.Say(caller, $"Zakupiono {shopItem.Name} (x{amount})");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zakupić przedmiotu z powodu błędu serwera: {ex}");
            }
        }
    }
}
