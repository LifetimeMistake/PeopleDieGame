using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Models.EventArgs
{
    public class UpdateTeamInfoEventArgs : System.EventArgs
    {
        public TeamInfo? TeamInfo { get; set; }

        public UpdateTeamInfoEventArgs(TeamInfo? teamInfo)
        {
            TeamInfo = teamInfo;
        }
    }
}
