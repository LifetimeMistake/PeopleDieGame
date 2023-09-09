using PeopleDieGame.NetMethods.Models;
using PeopleDieGame.NetMethods.RPCs;
using SDG.NetPak;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.NetMethods
{
    [NetInvokableGeneratedClass(typeof(RPCs.ClientDataRPC))]
    public static class ClientDataRPC_NetMethods
    {
        [NetInvokableGeneratedMethod("ReceiveUpdatePlayerInfo", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveUpdatePlayerInfo_Read(in ClientInvocationContext context)
        {
            NetPakReader reader = context.reader;

            if (!reader.ReadUInt64(out ulong id))
                return;

            if (!reader.ReadString(out string name))
                return;

            if (!reader.ReadString(out string bio))
                return;

            if (!reader.ReadInt32(out int _teamId))
                return;

            int? teamId;
            if (_teamId == -1)
                teamId = null;
            else
                teamId = _teamId;

            if (!reader.ReadFloat(out float walletBalance))
                return;

            if (!reader.ReadFloat(out float bounty))
                return;

            PlayerInfo playerInfo = new PlayerInfo(id, name, bio, teamId, walletBalance, bounty);
            ClientDataRPC.ReceiveUpdatePlayerInfo(playerInfo);
        }

        [NetInvokableGeneratedMethod("ReceiveUpdatePlayerInfo", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveUpdatePlayerInfo_Write(NetPakWriter writer, PlayerInfo info)
        {
            writer.WriteUInt64(info.Id);
            writer.WriteString(info.Name);
            writer.WriteString(info.Bio);
            if (info.TeamId.HasValue)
                writer.WriteInt32(info.TeamId.Value);
            else
                writer.WriteInt32(-1);
            writer.WriteFloat(info.WalletBalance);
            writer.WriteFloat(info.Bounty);
        }

        [NetInvokableGeneratedMethod("ReceiveUpdateTeamInfo", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveUpdateTeamInfo_Read(in ClientInvocationContext context)
        {
            NetPakReader reader = context.reader;

            if (!reader.ReadBit(out bool inTeam))
                return;

            if (!inTeam)
            {
                ClientDataRPC.ReceiveUpdateTeamInfo(null);
                return;
            }
                
            if (!reader.ReadInt32(out int id))
                return;

            if (!reader.ReadString(out string name))
                return;

            if (!reader.ReadString(out string description))
                return;

            if (!reader.ReadFloat(out float bankBalance))
                return;

            if (!reader.ReadUInt64(out ulong leaderId))
                return;

            if (!reader.ReadString(out string leaderName))
                return;

            TeamInfo teamInfo = new TeamInfo(id, name, description, bankBalance, leaderId, leaderName);
            ClientDataRPC.ReceiveUpdateTeamInfo(teamInfo);
        }

        [NetInvokableGeneratedMethod("ReceiveUpdatePlayerInfo", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveUpdateTeamInfo_Write(NetPakWriter writer, TeamInfo? teamInfo)
        {
            writer.WriteBit(teamInfo.HasValue);
            if (!teamInfo.HasValue)
                return;

            writer.WriteInt32(teamInfo.Value.Id);
            writer.WriteString(teamInfo.Value.Name);
            writer.WriteString(teamInfo.Value.Description);
            writer.WriteFloat(teamInfo.Value.BankBalance);
            writer.WriteUInt64(teamInfo.Value.LeaderId);
            writer.WriteString(teamInfo.Value.LeaderName);
        }
    }
}
