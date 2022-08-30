using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models
{
    public class TeamBank
    {
        public int TeamId { get; private set; }
        public double Funds { get; private set; }

        public TeamBank(int teamId, double startFunds = 1000)
        {
            TeamId = teamId;
            Funds = startFunds;
        }

        public void SetFunds(double amount)
        {
            Funds = amount;
        }

        public void AddFunds(double amount)
        {
            Funds += amount;
        }

        public void RemoveFunds(double amount)
        {
            Funds = Math.Max(0, Funds - amount);
        }
    }
}
