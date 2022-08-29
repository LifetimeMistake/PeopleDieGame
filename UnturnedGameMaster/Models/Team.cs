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
        public RespawnPoint? RespawnPoint { get; set; }

        public Team(int id, string name, string description = "", int? defaultLoadoutId = null, ulong? leaderId = null, RespawnPoint? respawnPoint = null)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            DefaultLoadoutId = defaultLoadoutId;
            LeaderId = leaderId;
            RespawnPoint = respawnPoint;
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

        public void SetTeamLeader(UnturnedPlayer player)
        {
            if (player == null)
                LeaderId = null;
            else
                LeaderId = (ulong)player.CSteamID;
        }
    }
}
