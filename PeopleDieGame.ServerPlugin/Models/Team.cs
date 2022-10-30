using System;
using System.Collections.Generic;
using System.Linq;
using PeopleDieGame.ServerPlugin.Models;
using SDG.Unturned;
using Steamworks;

namespace PeopleDieGame.ServerPlugin
{
    public class Team
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int? DefaultLoadoutId { get; private set; }
        public ulong? LeaderId { get; private set; }
        public CSteamID? GroupID { get; set; }
        public double BankBalance { get; private set; }
        public VectorPAR? RespawnPoint { get; set; }
        public List<TeamInvitation> Invitations { get; private set; }

        public Team(int id, string name, string description = "", int? defaultLoadoutId = null, ulong? leaderId = null, CSteamID? groupId = null, double bankBalance = 1000, VectorPAR? respawnPoint = null)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            DefaultLoadoutId = defaultLoadoutId;
            LeaderId = leaderId;
            BankBalance = bankBalance;
            RespawnPoint = respawnPoint;
            Invitations = new List<TeamInvitation>();
            GroupID = groupId;
        }

        public void SetName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            GroupInfo teamGroup = GroupManager.getGroupInfo(GroupID.Value);
            teamGroup.name = name;

            Name = name;
        }

        public void SetDescription(string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));

            Description = description;
        }

        public void SetDefaultLoadout(Loadout loadout)
        {
            DefaultLoadoutId = loadout?.Id ?? null;
        }

        public void SetTeamLeader(PlayerData player)
        {
            LeaderId = player?.Id ?? null;
        }

        public void AddInvitation(TeamInvitation teamInvitation)
        {
            if (teamInvitation == null)
                throw new ArgumentNullException(nameof(teamInvitation));

            if (teamInvitation.IsExpired())
                throw new ArgumentOutOfRangeException("The invitation was expired.");

            if (Invitations.Any(x => x.TargetId == teamInvitation.TargetId))
                throw new ArgumentException("Invitation already exists.");

            Invitations.Add(teamInvitation);
        }

        public bool RemoveInvitation(ulong targetPlayerId)
        {
            return Invitations.RemoveAll(x => x.TargetId == targetPlayerId) > 0;
        }

        public void SetBalance(double amount)
        {
            BankBalance = amount;
        }

        public void Deposit(double amount)
        {
            BankBalance += amount;
        }

        public void Withdraw(double amount)
        {
            BankBalance = Math.Max(0, BankBalance - amount);
        }

        public List<TeamInvitation> GetInvitations()
        {
            Invitations.RemoveAll(x => x.IsExpired());
            return Invitations;
        }
    }
}
