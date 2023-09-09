using PeopleDieGame.NetMethods.RPCs;
using Rocket.Unturned.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Services.Providers
{
    public class MarkerSyncProvider : IDisposableService
    {
        public void Init()
        {
            UnturnedEvents.Instance.OnPlayerConnected += Instance_OnPlayerConnected;
            MapMarkerManager.ClearMarkers();
        }

        public void Dispose()
        {
            UnturnedEvents.Instance.OnPlayerConnected -= Instance_OnPlayerConnected;
            MapMarkerManager.ClearMarkers();
        }

        private void Instance_OnPlayerConnected(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            MapMarkerManager.SyncMarkersToPlayer(player.SteamPlayer());
        }
    }
}
