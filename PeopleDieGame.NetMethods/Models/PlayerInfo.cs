using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.Models
{
    public struct PlayerInfo
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public int? TeamId { get; set; }
        public float WalletBalance { get; set; }
        public float Bounty { get; set; }

        public PlayerInfo(ulong id, string name, string bio, int? teamId, float walletBalance, float bounty)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Bio = bio ?? throw new ArgumentNullException(nameof(bio));
            TeamId = teamId;
            WalletBalance = walletBalance;
            Bounty = bounty;
        }
    }
}
