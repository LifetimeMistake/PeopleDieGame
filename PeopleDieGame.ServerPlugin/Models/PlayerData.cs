using PeopleDieGame.ServerPlugin.Services.Managers;
using System;
using static Rocket.Unturned.Events.UnturnedPlayerEvents;
using System.Text;

namespace PeopleDieGame.ServerPlugin.Models
{
    public class PlayerData
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public int? TeamId { get; set; }
        public float WalletBalance { get; set; }
        public float Bounty { get; set; }

        public PlayerData(ulong id, string name, string bio = "", int? teamId = null, float walletBalance = 0)
        {
            Id = id;
            Name = name;
            Bio = bio ?? throw new ArgumentNullException(nameof(bio));
            TeamId = teamId;
            WalletBalance = walletBalance;
            Bounty = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Profil gracza \"{Name}\"");
            sb.AppendLine($"ID drużyny: {(TeamId.HasValue ? TeamId.Value.ToString() : "Brak")}");
            if (Bio != "")
                sb.AppendLine($"Bio: \"{Bio}\"");

            return sb.ToString();
        }
    }
}
