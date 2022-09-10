using System;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class TeamEventArgs : System.EventArgs
    {
        public Team Team;

        public TeamEventArgs(Team team)
        {
            Team = team ?? throw new ArgumentNullException(nameof(team));
        }
    }
}
