using PeopleDieGame.NetMethods.RPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ClientModule.InviteRequest
{
    public class InviteRequestService : IService, IDisposable
    {
        private InviteRequestUI inviteRequestUI;

        public void Init()
        {
            inviteRequestUI = new InviteRequestUI();
            inviteRequestUI.OnSubmitInvite += InviteRequestUI_OnSubmitInvite;
            InviteRPC.OnInviteSent += InviteRPC_OnInviteSent    ;
        }

        public void Dispose()
        {
            if (inviteRequestUI.Active)
                inviteRequestUI.Close();

            inviteRequestUI.OnSubmitInvite -= InviteRequestUI_OnSubmitInvite;
            inviteRequestUI = null;
            InviteRPC.OnInviteSent -= InviteRPC_OnInviteSent;
        }

        public void OpenInvite(string inviterName, string teamName, float inviteTTL)
        {
            inviteRequestUI.Open(inviterName, teamName, inviteTTL);
        }

        private void InviteRequestUI_OnSubmitInvite(object sender, EventArgs.OnSubmitInviteResponse e)
        {
            InviteRPC.SendInviteResponse(e.Result);
        }

        private void InviteRPC_OnInviteSent(object sender, NetMethods.Models.EventArgs.InviteSentEventArgs e)
        {
            OpenInvite(e.InviterName, e.TeamName, e.InviteTTL);
        }
    }
}
