using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster
{
    public class Team
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int? DefaultLoadoutId { get; private set; }
        public ulong? LeaderId { get; private set; }
        public double BankBalance { get; private set; }
        public VectorPAR? RespawnPoint { get; set; }
        public List<TeamInvitation> Invitations { get; private set; }

        public Team(int id, string name, string description = "", int? defaultLoadoutId = null, ulong? leaderId = null, VectorPAR? respawnPoint = null, double bankBalance = 1000)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            DefaultLoadoutId = defaultLoadoutId;
            LeaderId = leaderId;
            BankBalance = bankBalance;
            RespawnPoint = respawnPoint;
            Invitations = new List<TeamInvitation>();
        }

        public void SetName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

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
            if (loadout == null)
                DefaultLoadoutId = null;
            else
                DefaultLoadoutId = loadout.Id;
        }

        public void SetTeamLeader(PlayerData player)
        {
            if (player == null)
                LeaderId = null;
            else
                LeaderId = player.Id;
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
