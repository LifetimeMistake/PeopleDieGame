using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models
{
    public class PlayerData
    {
        public ulong Id { get; private set; }
        public string Name { get; private set; }
        public string Bio { get; private set; }
        public int? TeamId { get; set; }
        public double WalletFunds { get; private set; }

        public PlayerData(ulong id, string name, string bio = "", int? teamId = null, double walletFunds = 0)
        {
            Id = id;
            Name = name;
            Bio = bio ?? throw new ArgumentNullException(nameof(bio));
            TeamId = teamId;
            WalletFunds = walletFunds;
        }

        public void SetBio(string bio)
        {
            if (bio == null)
                throw new ArgumentNullException(nameof(bio));

            Bio = bio;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public void SetWalletFunds(double amount)
        {
            WalletFunds = amount;
        }

        public void AddWalletFunds(double amount)
        {
            WalletFunds += amount;
        }

        public void RemoveWalletFunds(double amount)
        {
            WalletFunds = Math.Max(0, WalletFunds - amount);
        }
    }
}
