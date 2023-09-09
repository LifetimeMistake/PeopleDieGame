using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.Models.EventArgs
{
    public class InviteSentEventArgs : System.EventArgs
    {
        public string InviterName { get; set; }
        public string TeamName { get; set; }
        public float InviteTTL { get; set; }

        public InviteSentEventArgs(string inviterName, string teamName, float inviteTTL)
        {
            InviterName = inviterName ?? throw new ArgumentNullException(nameof(inviterName));
            TeamName = teamName ?? throw new ArgumentNullException(nameof(teamName));
            InviteTTL = inviteTTL;
        }
    }
}
