using Rocket.API;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PeopleDieGame.ServerPlugin.Helpers;

namespace PeopleDieGame.ServerPlugin.Commands.Admin
{
    public class MeasureCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "measure";

        public string Help => "Pozwala na mierzenie odległości";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public Dictionary<CSteamID, Vector3> sessions = new Dictionary<CSteamID, Vector3>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            CSteamID callerId = player.CSteamID;
            if (command.Length != 0 && command[0].ToLowerInvariant() == "cancel")
            {
                if (sessions.ContainsKey(callerId))
                    sessions.Remove(callerId);

                ChatHelper.Say(caller, "Anulowano pomiar.");
                return;
            }

            Vector3 playerPosition = player.Position;
            if (sessions.ContainsKey(callerId))
            {
                Vector3 firstPosition = sessions[callerId];
                double distance = Math.Round(Vector3.Distance(firstPosition, playerPosition), 2);
                double heightDifference = Math.Round(Math.Abs(firstPosition.y - playerPosition.y), 2);
                double horizontalDifference = Math.Round(Vector2.Distance(new Vector2(firstPosition.x, firstPosition.z), new Vector2(playerPosition.x, playerPosition.z)), 2);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Oznaczono drugi punkt: {playerPosition}");
                sb.AppendLine($"Odległość: {distance}m");
                sb.AppendLine($"Odległość pozioma: {horizontalDifference}m");
                sb.AppendLine($"Różnica wysokości pomiędzy pomiarami: {heightDifference}m");
                ChatHelper.Say(caller, sb);
                sessions.Remove(callerId);
            }
            else
            {
                sessions.Add(callerId, playerPosition);
                ChatHelper.Say(caller, $"Oznaczono pierwszy punkt: {playerPosition}");
            }
        }
    }
}
