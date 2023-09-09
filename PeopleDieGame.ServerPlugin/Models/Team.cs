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
        public float BankBalance { get; private set; }
        public VectorPAR? RespawnPoint { get; set; }

        public Team(int id, string name, string description = "", int? defaultLoadoutId = null, ulong? leaderId = null, CSteamID? groupId = null, float bankBalance = 1000, VectorPAR? respawnPoint = null)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            DefaultLoadoutId = defaultLoadoutId;
            LeaderId = leaderId;
            BankBalance = bankBalance;
            RespawnPoint = respawnPoint;
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

        public void SetBalance(float amount)
        {
            BankBalance = amount;
        }

        public void Deposit(float amount)
        {
            BankBalance += amount;
        }

        public void Withdraw(float amount)
        {
            BankBalance = Math.Max(0, BankBalance - amount);
        }
    }
}
