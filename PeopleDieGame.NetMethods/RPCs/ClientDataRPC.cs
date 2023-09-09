using PeopleDieGame.NetMethods.Models;
using PeopleDieGame.NetMethods.Models.EventArgs;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.RPCs
{
    public static class ClientDataRPC
    {
        public static event EventHandler<UpdatePlayerInfoEventArgs> OnUpdatePlayerInfo;
        public static event EventHandler<UpdateTeamInfoEventArgs> OnUpdateTeamInfo;
        private static readonly ClientStaticMethod<PlayerInfo> sendUpdatePlayerInfo = ClientStaticMethod<PlayerInfo>.Get(new ClientStaticMethod<PlayerInfo>.ReceiveDelegate(ReceiveUpdatePlayerInfo));
        private static readonly ClientStaticMethod<TeamInfo?> sendUpdateTeamInfo = ClientStaticMethod<TeamInfo?>.Get(new ClientStaticMethod<TeamInfo?>.ReceiveDelegate(ReceiveUpdateTeamInfo));

        public static void UpdatePlayerInfo(SteamPlayer player, PlayerInfo info)
        {
            if (!Provider.isServer)
                return;

            sendUpdatePlayerInfo.Invoke(SDG.NetTransport.ENetReliability.Reliable, player.transportConnection, info);
        }

        public static void UpdateTeamInfo(SteamPlayer player, TeamInfo? info)
        {
            if (!Provider.isServer)
                return;

            sendUpdateTeamInfo.Invoke(SDG.NetTransport.ENetReliability.Reliable, player.transportConnection, info);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveUpdatePlayerInfo(PlayerInfo info)
        {
            OnUpdatePlayerInfo?.Invoke(null, new UpdatePlayerInfoEventArgs(info));
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveUpdateTeamInfo(TeamInfo? teamInfo)
        {
            OnUpdateTeamInfo?.Invoke(null, new UpdateTeamInfoEventArgs(teamInfo));
        }
    }
}
