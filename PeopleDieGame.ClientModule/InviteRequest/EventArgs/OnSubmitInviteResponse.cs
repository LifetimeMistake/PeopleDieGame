using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ClientModule.InviteRequest.EventArgs
{
    public class OnSubmitInviteResponse : System.EventArgs
    {
        public bool Result;

        public OnSubmitInviteResponse(bool isAccepted) 
        {
            Result = isAccepted;
        }
    }
}
