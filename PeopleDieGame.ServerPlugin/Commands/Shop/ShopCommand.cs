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
using System.Runtime.CompilerServices;
using PeopleDieGame.NetMethods.RPCs;
using Mono.Cecil.Cil;
using SDG.Unturned;
using Steamworks;

namespace PeopleDieGame.ServerPlugin.Commands.Shop
{
    public class ShopCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "shop";

        public string Help => "Polecenia dotyczące sklepu przedmiotów";

        public string Syntax => "<list/search/inspect/buy> <itemId/itemName> [<amount>/page>]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                OpenShopGUI(caller);
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
                case "search":
                    VerbSearch(caller, verbArgs);
                    break;
            }
        }

        private void OpenShopGUI(IRocketPlayer caller)
        {
            try
            {
                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InGame)
                {
                    ChatHelper.Say(caller, "Nie można korzystać z sklepu w poczekalni!");
                    return;
                }

                UnturnedPlayer unturnedPlayer = (UnturnedPlayer)caller;
                SteamPlayer steamPlayer = unturnedPlayer.SteamPlayer();
                PlayerData playerData = playerDataManager.GetData((ulong)unturnedPlayer.CSteamID);

                if (playerData == null)
                {
                    ChatHelper.Say("Nie udało się odnaleźć profilu gracza");
                    return;
                }

                if (!playerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Musisz należeć do drużyny, by korzystać z sklepu.");
                    return;
                }

                Team team = teamManager.GetTeam(playerData.TeamId.Value);
                if (team == null)
                {
                    throw new Exception("Failed to find specified team.");
                }

                Dictionary<ushort, float> items = shopManager.GetSerializableItemList();
                ShopRPC.UpdateShopItems(steamPlayer, items);
            }
            catch(Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się otworzyć sklepu z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbList(IRocketPlayer caller, string[] command)
        {
            try
            {
                int page = 1;

                if (command.Length > 0)
                {
                    if (!int.TryParse(command[0], out page))
                    {
                        ChatHelper.Say(caller, "Podano niepoprawną stronę w sklepie");
                        return;
                    }
                }

                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                ShopItem[] items = shopManager.GetItemList();
                if (items.Length == 0)
                {
                    ChatHelper.Say(caller, "W sklepie nie ma żadnych przedmiotów");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Lista przedmiotów w sklepie:");
               

                int pageCount = Math.Max(1, (int)Math.Ceiling((float)items.Length / 7));

                if (page < 1 || page > pageCount)
                {
                    ChatHelper.Say(caller, $"Podano stronę z poza zakresu dostępnych stron (1-{pageCount}");
                    return;
                }

                IEnumerable<ShopItem> pageItems = items.Skip((page - 1) * 7).Take(7);

                foreach (ShopItem item in pageItems)
                {
                    sb.AppendLine($"ID: {item.UnturnedItemId} | Nazwa: \"{item.Name}\" | Cena: ${item.Price}");
                }

                sb.AppendLine($"Strona {page} z {pageCount}");
                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać listy przedmiotów z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSearch(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0 || command[0] == "")
            {
                ChatHelper.Say("Musisz podać kryterium wyszukiwania");
                return;
            }

            try
            {
                int page = 1;
                string searchTerm = command[0];

                if (command.Length > 1)
                {
                    if (!int.TryParse(command[1], out page))
                    {
                        ChatHelper.Say(caller, "Podano niepoprawną stronę w sklepie");
                        return;
                    }
                }

                ShopManager shopManager = ServiceLocator.Instance.LocateService<ShopManager>();
                IEnumerable<ShopItem> items = shopManager.GetItemList().Where(x => x.Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()));
                if (items.Count() == 0)
                {
                    ChatHelper.Say(caller, "Nie znaleziono żadnych przedmiotów");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Lista przedmiotów w sklepie:");

                int pageCount = Math.Max(1, (int)Math.Ceiling((float)items.Count() / 7));

                if (page < 1 || page > pageCount)
                {
                    ChatHelper.Say(caller, $"Podano stronę z poza zakresu dostępnych stron (1-{pageCount}");
                    return;
                }

                IEnumerable<ShopItem> pageItems = items.Skip((page - 1) * 7).Take(7);

                foreach (ShopItem item in pageItems)
                {
                    sb.AppendLine($"ID: {item.UnturnedItemId} | Nazwa: \"{item.Name}\" | Cena: ${item.Price}");
                }

                sb.AppendLine($"Strona {page} z {pageCount}");
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
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();

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

                PlayerData callerPlayerData = playerDataManager.GetData((ulong)((UnturnedPlayer)caller).CSteamID);
                if (!callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Musisz należeć do drużyny, by korzystać z sklepu.");
                    return;
                }

                Team callerTeam = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID((CSteamID)callerPlayerData.Id);

                if (!teamManager.HasClaim(callerTeam))
                {
                    ChatHelper.Say(caller, "Twoja drużyna nie posiada bazy");
                    return;
                }

                if (!teamManager.IsInClaimRadius(callerTeam, callerPlayer.Position))
                {
                    ChatHelper.Say(caller, "Znajdujesz się poza zasięgiem bazy");
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
