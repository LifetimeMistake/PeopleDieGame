using Pathfinding.RVO.Sampled;
using PeopleDieGame.NetMethods.RPCs;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Services.Providers
{
    public class ArenaMarkerProvider : IDisposableService
    {
        private static readonly Color COLOR_ACTIVE = new Color(87 / 255f, 242 / 255f, 250 / 255f);
        private static readonly Color COLOR_INACTIVE = new Color(87 / 255f, 155 / 255f, 250 / 255f);
        private static readonly Color COLOR_CONQUERED = new Color(105 / 255f, 105 / 255f, 105 / 255f);

        private Dictionary<int, Guid> markers = new Dictionary<int, Guid>();

        [InjectDependency]
        public ArenaManager arenaManager { get; set; }

        public void Init()
        {
            arenaManager.OnArenaCreated += ArenaManager_OnArenaCreated;
            arenaManager.OnArenaRemoved += ArenaManager_OnArenaRemoved;
            arenaManager.OnBossFightCreated += ArenaManager_OnBossFightCreated;
            arenaManager.OnBossFightRemoved += ArenaManager_OnBossFightRemoved;
            LoadMarkers();
        }

        public void Dispose()
        {
            arenaManager.OnArenaCreated -= ArenaManager_OnArenaCreated;
            arenaManager.OnArenaRemoved -= ArenaManager_OnArenaRemoved;
            arenaManager.OnBossFightCreated -= ArenaManager_OnBossFightCreated;
            arenaManager.OnBossFightRemoved -= ArenaManager_OnBossFightRemoved;
            DisposeMarkers();
        }

        private string GetMarkerDescription(BossArena arena)
        {
            string status;
            if (arena.Conquered)
                status = " (Pokonana)";
            else
            {
                BossFight bossFight = arenaManager.GetBossFights().FirstOrDefault(x => x.Arena == arena);
                if (bossFight != null)
                    status = $" (\"{bossFight.DominantTeam.Name}\" w trakcie walki)";
                else
                    status = "";
            }

            return $"{arena.Name} {status}";
        }

        private Color GetMarkerColor(BossArena arena)
        {
            if (arena.Conquered)
                return COLOR_CONQUERED;
            else if (arenaManager.GetBossFights().Any(x => x.Arena == arena))
                return COLOR_ACTIVE;
            else return COLOR_INACTIVE;
        }

        private void LoadMarkers()
        {
            foreach(BossArena arena in arenaManager.GetArenas())
            {
                Guid markerId = MapMarkerManager.CreateMarker(arena.ActivationPoint, GetMarkerDescription(arena), GetMarkerColor(arena)).Id;
                markers.Add(arena.Id, markerId);
            }
        }

        private void DisposeMarkers()
        {
            foreach(Guid markerId in markers.Values)
            {
                MapMarkerManager.RemoveMarker(markerId);
            }

            markers.Clear();
        }

        private void UpdateMarker(BossArena arena)
        {
            if (!markers.ContainsKey(arena.Id))
                return;

            Guid markerId = markers[arena.Id];
            MapMarkerManager.UpdateMarkerLabel(markerId, GetMarkerDescription(arena));
            MapMarkerManager.UpdateMarkerColor(markerId, GetMarkerColor(arena));
        }

        private void ArenaManager_OnArenaCreated(object sender, Models.EventArgs.ArenaEventArgs e)
        {
            Guid markerId = MapMarkerManager.CreateMarker(e.Arena.ActivationPoint, GetMarkerDescription(e.Arena), GetMarkerColor(e.Arena)).Id;
            markers.Add(e.Arena.Id, markerId);
        }

        private void ArenaManager_OnArenaRemoved(object sender, Models.EventArgs.ArenaEventArgs e)
        {
            int arenaId = e.Arena.Id;
            if (!markers.ContainsKey(arenaId))
                return;

            Guid markerId = markers[arenaId];
            MapMarkerManager.RemoveMarker(markerId);
            markers.Remove(arenaId);
        }

        private void ArenaManager_OnBossFightCreated(object sender, Models.EventArgs.BossFightEventArgs e)
        {
            UpdateMarker(e.BossFight.Arena);
        }

        private void ArenaManager_OnBossFightRemoved(object sender, Models.EventArgs.BossFightEventArgs e)
        {
            UpdateMarker(e.BossFight.Arena);
        }
    }
}
