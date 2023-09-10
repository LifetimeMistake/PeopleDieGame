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
        public int Id { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? DefaultLoadoutId { get; set; }
        public ulong? LeaderId { get; set; }
        public CSteamID? GroupID { get; set; }
        public float BankBalance { get; set; }
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
    }
}
