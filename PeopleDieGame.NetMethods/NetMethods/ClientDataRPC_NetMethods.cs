using PeopleDieGame.NetMethods.Models;
using PeopleDieGame.NetMethods.RPCs;
using SDG.NetPak;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

            if (!reader.ReadBit(out bool hasLeader))
                return;

            ulong? leaderId = null;
            string leaderName = null;
            if (hasLeader)
            {
                if (!reader.ReadUInt64(out ulong _leaderId))
                    return;

                leaderId = _leaderId;

                if (!reader.ReadString(out leaderName))
                    return;
            }

            if (!reader.ReadBit(out bool hasClaim))
                return;

            ClaimInfo? claim = null;

            if (hasClaim)
            {
                if (!reader.ReadNormalVector3(out Vector3 position))
                    return;

                if (!reader.ReadFloat(out float squareRadius))
                    return;

                claim = new ClaimInfo(position, squareRadius);
            }

            TeamInfo teamInfo = new TeamInfo(id, name, description, bankBalance, leaderId, leaderName, claim);
            ClientDataRPC.ReceiveUpdateTeamInfo(teamInfo);
        }

        [NetInvokableGeneratedMethod("ReceiveUpdatePlayerInfo", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveUpdateTeamInfo_Write(NetPakWriter writer, TeamInfo? teamInfo)
        {
            writer.WriteBit(teamInfo.HasValue);
            if (!teamInfo.HasValue)
                return;

            TeamInfo info = teamInfo.Value;

            writer.WriteInt32(info.Id);
            writer.WriteString(info.Name);
            writer.WriteString(info.Description);
            writer.WriteFloat(info.BankBalance);

            writer.WriteBit(info.LeaderId.HasValue);
            if (info.LeaderId.HasValue)
            {
                writer.WriteUInt64(info.LeaderId.Value);
                writer.WriteString(info.LeaderName);
            }

            writer.WriteBit(info.Claim.HasValue);
            if (info.Claim.HasValue)
            {
                ClaimInfo claim = info.Claim.Value;
                writer.WriteNormalVector3(claim.Position);
                writer.WriteFloat(claim.SquareRadius);
            }
        }
    }
}
