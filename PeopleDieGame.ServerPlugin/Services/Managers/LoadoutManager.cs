using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Models.EventArgs;
using PeopleDieGame.ServerPlugin.Models.Exception;
using PeopleDieGame.ServerPlugin.Services.Providers;

namespace PeopleDieGame.ServerPlugin.Services.Managers
{
    public class LoadoutManager : IService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private LoadoutIdProvider loadoutIdProvider { get; set; }

        public event EventHandler<LoadoutAppliedEventArgs> OnLoadoutApplied;
        public event EventHandler<LoadoutEventArgs> OnLoadoutDropped;
        public event EventHandler<LoadoutEventArgs> OnLoadoutCreated;
        public event EventHandler<LoadoutEventArgs> OnLoadoutRemoved;

        public void Init()
        { }

        public Loadout CreateLoadout(string name, string description = "")
        {
            Dictionary<int, Loadout> loadouts = dataManager.GameData.Loadouts;
            if (GetLoadoutByName(name) != null)
                return null;

            int loadoutId = loadoutIdProvider.GenerateId();
            Loadout loadout = new Loadout(loadoutId, name, description);

            loadouts.Add(loadoutId, loadout);
            OnLoadoutCreated?.Invoke(this, new LoadoutEventArgs(loadout));

            return loadout;
        }

        public void GiveLoadout(PlayerData playerData, Loadout loadout)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID((CSteamID)playerData.Id);
            if (player == null)
                throw new PlayerOfflineException(playerData.Name);

            foreach (KeyValuePair<int, int> item in loadout.Items)
            {
                if (!player.GiveItem((ushort)item.Key, (byte)item.Value))
                    throw new Exception($"Failed to give item {item.Key} ({item.Value}x) to player");
            }

            OnLoadoutApplied?.Invoke(this, new LoadoutAppliedEventArgs(playerData, loadout));
        }

        public void DropLoadout(Loadout loadout, Vector3 position)
        {
            foreach (KeyValuePair<int, int> kvp in loadout.Items)
            {
                Item item = new Item((ushort)kvp.Key, true);

                for (int i = 0; i < kvp.Value; i++)
                    ItemManager.dropItem(item, position, true, true, true);
            }

            OnLoadoutDropped?.Invoke(this, new LoadoutEventArgs(loadout));
        }

        public bool DeleteLoadout(int id)
        {
            TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
            Dictionary<int, Loadout> loadouts = dataManager.GameData.Loadouts;
            if (!loadouts.ContainsKey(id))
                return false;

            foreach (Team team in teamManager.GetTeams())
            {
                if (team.DefaultLoadoutId == id)
                {
                    team.SetDefaultLoadout(null);
                }
            }

            Loadout loadout = loadouts[id];
            loadouts.Remove(id);
            OnLoadoutRemoved?.Invoke(this, new LoadoutEventArgs(loadout));
            return true;
        }

        public Loadout GetLoadout(int id)
        {
            Dictionary<int, Loadout> loadouts = dataManager.GameData.Loadouts;
            if (!loadouts.ContainsKey(id))
                return null;

            return loadouts[id];
        }

        public Loadout GetLoadoutByName(string name, bool exactMatch = true)
        {
            Dictionary<int, Loadout> loadouts = dataManager.GameData.Loadouts;
            if (exactMatch)
                return loadouts.Values.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            else
                return loadouts.Values.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
        }

        public Loadout ResolveLoadout(string loadoutNameOrId, bool exactMatch)
        {
            int id;
            if (int.TryParse(loadoutNameOrId, out id))
            {
                // might be an ID but idk
                Loadout loadout = GetLoadout(id);
                if (loadout != null)
                    return loadout;
            }

            //otherwise try matching by name
            return GetLoadoutByName(loadoutNameOrId, exactMatch);
        }

        public Loadout[] GetLoadouts()
        {
            return dataManager.GameData.Loadouts.Values.ToArray();
        }
    }
}
