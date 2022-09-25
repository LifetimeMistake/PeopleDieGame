using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Commands.General
{
    public class BountyCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "bountylist";

        public string Help => "Wyświetla tabelę bounty graczy";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
            List<PlayerData> playerDataList = playerDataManager.GetPlayers().OrderBy(x => x.Bounty).ToList();
            StringBuilder sb = new StringBuilder();

            double totalBounty = 0;
            sb.AppendLine("Tabela bounty");
            foreach (PlayerData playerData in playerDataList.ToList())
            {
                if (playerData.Bounty == 0)
                {
                    playerDataList.Remove(playerData);
                    continue;
                }

                sb.AppendLine($"{playerData.Name} : ${playerData.Bounty}");
                totalBounty += playerData.Bounty;
            }
            if (playerDataList.Count == 0)
                sb.AppendLine("Brak wyników");
            else
                sb.AppendLine($"Suma bounty wszystkich graczy: {totalBounty}");

            ChatHelper.Say(caller, sb);
        }

    }
}