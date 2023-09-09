using PeopleDieGame.ClientModule.Models;
using PeopleDieGame.NetMethods.Models;
using PeopleDieGame.NetMethods.Models.EventArgs;
using PeopleDieGame.NetMethods.RPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ClientModule
{
    internal class ClientDataService : IService, IDisposable
    {
        public ClientData ClientData { get; private set; }
        public event EventHandler<System.EventArgs> OnPlayerInfoUpdated;
        public event EventHandler<System.EventArgs> OnTeamInfoUpdated;

        public void Init()
        {
            ClientData = new ClientData();
            ClientDataRPC.OnUpdatePlayerInfo += ClientDataRPC_OnUpdatePlayerInfo;
            ClientDataRPC.OnUpdateTeamInfo += ClientDataRPC_OnUpdateTeamInfo;
        }

        public void Dispose()
        {
            ClientData = null;
        }

        private void UpdatePlayerInfo(PlayerInfo playerInfo)
        {
            ClientData.PlayerInfo = playerInfo;
            OnPlayerInfoUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateTeamInfo(TeamInfo? teamInfo)
        {
            ClientData.TeamInfo = teamInfo;
            OnTeamInfoUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void ClientDataRPC_OnUpdatePlayerInfo(object sender, NetMethods.Models.EventArgs.UpdatePlayerInfoEventArgs e)
        {
            UpdatePlayerInfo(e.PlayerInfo);
        }

        private void ClientDataRPC_OnUpdateTeamInfo(object sender, NetMethods.Models.EventArgs.UpdateTeamInfoEventArgs e)
        {
            UpdateTeamInfo(e.TeamInfo);
        }
    }
}
