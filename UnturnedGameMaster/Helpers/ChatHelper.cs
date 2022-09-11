using Rocket.API;
using Rocket.Unturned.Chat;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Helpers
{
    public static class ChatHelper
    {
        public static bool Say(PlayerData playerData, string text)
        {
            List<string> lines = UnturnedChat.wrapMessage(text);
            CSteamID playerId = (CSteamID)playerData.Id;

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

        public static void Say(IEnumerable<IRocketPlayer> players, string text)
        {
            foreach (IRocketPlayer player in players)
                Say(player, text);
        }

        public static void Say(IEnumerable<PlayerData> players, string text)
        {
            foreach (PlayerData player in players)
                Say(player, text);
        }

        public static void Say(IEnumerable<IRocketPlayer> players, StringBuilder sb)
        {
            foreach (IRocketPlayer player in players)
                Say(player, sb);
        }

        public static void Say(IEnumerable<PlayerData> players, StringBuilder sb)
        {
            foreach (PlayerData player in players)
                Say(player, sb);
        }
    }
}
