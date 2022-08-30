﻿using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Commands.Admin;
using UnturnedGameMaster.Models.EventArgs;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class LoadoutManager : IManager
    {
        [InjectDependency]
        private DataManager dataManager{ get; set; }
        [InjectDependency]
        private LoadoutIdProvider loadoutIdProvider{ get; set; }

        public event EventHandler<LoadoutAppliedEventArgs> OnLoadoutApplied;
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

        public void GiveLoadout(UnturnedPlayer player, Loadout loadout)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (loadout == null)
                throw new ArgumentNullException(nameof(loadout));

            foreach(KeyValuePair<int, int> item in loadout.Items)
            {
                if (!player.GiveItem((ushort)item.Key, (byte)item.Value))
                    throw new Exception($"Failed to give item {item.Key} ({item.Value}x) to player");
            }

            OnLoadoutApplied?.Invoke(this, new LoadoutAppliedEventArgs(player, loadout));
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
