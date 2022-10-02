using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Text;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;

namespace PeopleDieGame.ServerPlugin.Commands
{
    public class TestLocateCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "locate";

        public string Help => "";

        public string Syntax => "<id>";

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

            if (!ushort.TryParse(command[0], out ushort id))
            {
                ChatHelper.Say(caller, "Musisz podać odpowiednie ID przedmiotu");
                return;
            }

            List<UnturnedPlayer> players = ItemLocator.GetPlayersWithItem(id);
            List<RegionItem> items = ItemLocator.GetDroppedItems(id);
            List<InteractableStorage> storages = ItemLocator.GetStoragesWithItem(id);
            List<InteractableVehicle> vehicles = ItemLocator.GetVehiclesWithItem(id);

            StringBuilder sb = new StringBuilder();

            if (items.Count == 0 && players.Count == 0 && storages.Count == 0 && vehicles.Count == 0)
            {
                ChatHelper.Say(caller, $"Przedmiot z ID: {id} nie został znaleziony");
                return;
            }

            if (players.Count != 0)
            {
                sb.AppendLine($"Gracze posiadający przedmiot z ID: {id}");
                foreach (UnturnedPlayer player in players)
                {
                    sb.AppendLine($"{player.CharacterName} | ID: {player.Id}");
                }
            }
            if (items.Count != 0)
            {
                sb.AppendLine($"Znalezione przedmioty z ID: {id}");
                foreach (RegionItem item in items)
                {
                    sb.AppendLine($"ID: {item.ItemData.instanceID} | {item.ItemData.point}");
                }
            }
            if (storages.Count != 0)
            {
                sb.AppendLine($"Pojemniki posiadające przedmiot z ID: {id}");
                foreach (InteractableStorage storage in storages)
                {
                    sb.AppendLine($"ID: {storage.name} | {storage.gameObject.transform.position}");
                }
            }
            if (vehicles.Count != 0)
            {
                sb.AppendLine($"Pojazdy posiadające przedmiot z ID: {id}");
                foreach (InteractableVehicle vehicle in vehicles)
                {
                    sb.AppendLine($"ID: {vehicle.name} | {vehicle.gameObject.transform.position}");
                }
            }

            ChatHelper.Say(caller, sb);
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }
    }
}
