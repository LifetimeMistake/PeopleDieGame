﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class TeamInvitationEventArgs : System.EventArgs
    {
        public Team Team;
        public TeamInvitation TeamInvitation;

        public TeamInvitationEventArgs(Team team, TeamInvitation teamInvitation)
        {
            Team = team ?? throw new ArgumentNullException(nameof(team));
            TeamInvitation = teamInvitation ?? throw new ArgumentNullException(nameof(teamInvitation));
        }
    }
}