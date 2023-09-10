using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Models
{
    public struct TeamInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float BankBalance { get; set; }
        public ulong? LeaderId { get; set; }
        public string LeaderName { get; set; }
        public ClaimInfo? Claim { get; set; }

        public TeamInfo(int id, string name, string description, float bankBalance, ulong? leaderId, string leaderName, ClaimInfo? claim)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            BankBalance = bankBalance;
            LeaderId = leaderId;
            LeaderName = leaderName;
            Claim = claim;
        }
    }
}
