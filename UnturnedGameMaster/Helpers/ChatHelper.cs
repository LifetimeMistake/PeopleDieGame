using Rocket.API;
using Rocket.Unturned.Chat;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Helpers
{
    public static class ChatHelper
    {
        public static bool Say(PlayerData playerData, string text)
        {
            PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
            List<string> lines = UnturnedChat.wrapMessage(text);
            CSteamID playerId = (CSteamID)playerData.Id;

            if (playerDataManager.GetPlayer(playerData.Id) == null)
            {
                return false;
            }

            foreach (string line in lines)
            {
                UnturnedChat.Say(playerId, line);
            }
            return true;
        }

        public static void Say(IRocketPlayer player, string text)
        {
            List<string> lines = UnturnedChat.wrapMessage(text);

            foreach (string line in lines)
            {
                UnturnedChat.Say(player, line);
            }
        }

        public static void Say(string text)
        {
            List<string> lines = UnturnedChat.wrapMessage(text);

            foreach (string line in lines)
            {
                UnturnedChat.Say(line);
            }
        }

        public static bool Say(PlayerData playerData, StringBuilder sb)
        {
            List<string> sbLines = sb.ToString().Split('\n').ToList();

            if (sbLines.Count == 0)
            {
                return false;
            }

            foreach (string sbLine in sbLines)
            {
                if (!Say(playerData, sbLine))
                    return false;
            }
            return true;

        }

        public static void Say(IRocketPlayer player, StringBuilder sb)
        {
            List<string> sbLines = sb.ToString().Split('\n').ToList();

            if (sbLines.Count == 0)
            {
                return;
            }

            foreach (string sbLine in sbLines)
            {
                Say(player, sbLine);
            }
        }

        public static void Say(StringBuilder sb)
        {
            List<string> sbLines = sb.ToString().Split('\n').ToList();

            if (sbLines.Count == 0)
            {
                return;
            }

            foreach (string sbLine in sbLines)
            {
                Say(sbLine);
            }
        }
    }
}
