using System;

namespace UnturnedGameMaster.Models
{
    public class PlayerData
    {
        public ulong Id { get; private set; }
        public string Name { get; private set; }
        public string Bio { get; private set; }
        public int? TeamId { get; set; }
        public double WalletBalance { get; private set; }
        public double Bounty { get; private set; }

        public PlayerData(ulong id, string name, string bio = "", int? teamId = null, double walletBalance = 0)
        {
            Id = id;
            Name = name;
            Bio = bio ?? throw new ArgumentNullException(nameof(bio));
            TeamId = teamId;
            WalletBalance = walletBalance;
            Bounty = 0;
        }

        public void SetBio(string bio)
        {
            Bio = bio ?? throw new ArgumentNullException(nameof(bio));
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public void SetBalance(double amount)
        {
            WalletBalance = amount;
        }

        public void Deposit(double amount)
        {
            WalletBalance += amount;
        }

        public void Withdraw(double amount)
        {
            WalletBalance = Math.Max(0, WalletBalance - amount);
        }

        public void ResetBounty()
        {
            Bounty = 0;
        }

        public void AddBounty(double amount)
        {
            Bounty += amount;
        }
    }
}
