using System;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
{
    public class TeamBankEventArgs : System.EventArgs
    {
        public Team Team;
        public double Amount;

        public TeamBankEventArgs(Team team, double amount)
        {
            Team = team ?? throw new ArgumentNullException(nameof(team));
            Amount = amount;
        }
    }
}
