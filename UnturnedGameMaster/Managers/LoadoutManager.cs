using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Models.EventArgs;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class LoadoutManager : IManager
    {
        private DataManager dataManager;
        private LoadoutIdProvider loadoutIdProvider;

        public event EventHandler<LoadoutAppliedEventArgs> OnLoadoutApplied;
        public event EventHandler<LoadoutEventArgs> OnLoadoutCreated;
        public event EventHandler<LoadoutEventArgs> OnLoadoutRemoved;

        public LoadoutManager(DataManager configManager, LoadoutIdProvider loadoutIdProvider)
        {
            this.dataManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            this.loadoutIdProvider = loadoutIdProvider ?? throw new ArgumentNullException(nameof(loadoutIdProvider));
        }

        public void Init()
        { }

        public Loadout CreateLoadout(string name, string description = "")
        {
            Dictionary<int, Loadout> loadouts = dataManager.GameData.Loadouts;
            if (loadouts.Values.Any(x => x.Name == name))
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
            Dictionary<int, Loadout> loadouts = dataManager.GameData.Loadouts;
            if (!loadouts.ContainsKey(id))
                return false;

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

        public Loadout GetLoadoutByName(string name)
        {
            Dictionary<int, Loadout> loadouts = dataManager.GameData.Loadouts;
            return loadouts.Values.FirstOrDefault(x => x.Name == name);
        }

        public Loadout[] GetLoadouts()
        {
            return dataManager.GameData.Loadouts.Values.ToArray();
        }
    }
}
