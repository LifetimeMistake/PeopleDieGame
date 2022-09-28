﻿using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Commands
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

            if (items == null && players == null && storages == null && vehicles == null)
            {
                ChatHelper.Say(caller, $"Przedmiot z ID: {id} nie został znaleziony");
                return;
            }

            if (players != null)
            {
                sb.AppendLine($"Gracze posiadający przedmiot z ID: {id}");
                foreach (UnturnedPlayer player in players)
                {
                    sb.AppendLine($"{player.CharacterName} | ID: {player.Id}");
                }
            }
            if (items != null)
            {
                sb.AppendLine($"Znalezione przedmioty z ID: {id}");
                foreach (RegionItem item in items)
                {
                    sb.AppendLine($"ID: {item.ItemData.instanceID} | {item.ItemData.point}");
                }
            }
            if (storages != null)
            {
                sb.AppendLine($"Pojemniki posiadające przedmiot z ID: {id}");
                foreach (InteractableStorage storage in storages)
                {
                    sb.AppendLine($"ID: {storage.name} | {storage.gameObject.transform.position}");
                }
            }
            if (vehicles != null)
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
