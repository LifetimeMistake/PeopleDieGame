using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Helpers;

namespace UnturnedGameMaster.Commands
{
    public class TestCommand : IRocketCommand
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
            List<ItemData> items = ItemLocator.GetDroppedItems(id);

            StringBuilder sb = new StringBuilder();

            if (items == null && players == null)
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
                foreach (ItemData item in items)
                {
                    sb.AppendLine($"ID: {item.instanceID} | {item.point}");
                }
            }

            ChatHelper.Say(sb);
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }
    }
}
