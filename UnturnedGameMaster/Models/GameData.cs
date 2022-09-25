using System.Collections.Generic;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models
{
    public class GameData
    {
        public Dictionary<int, Team> Teams { get; set; }
        public Dictionary<int, Loadout> Loadouts { get; set; }
        public Dictionary<ushort, ShopItem> ShopItems { get; set; }
        public Dictionary<ulong, PlayerData> PlayerData { get; set; }
        public Dictionary<int, BossArena> Arenas { get; set; }
        public Dictionary<byte, int> ManagedZombiePools { get; set; }
        public List<ushort> ObjectiveItems { get; set; }
        public int LastLoadoutId { get; set; }
        public int LastTeamId { get; set; }
        public int LastArenaId { get; set; }
        public VectorPAR? DefaultRespawnPoint { get; set; }
        public GameState State { get; set; }
        public double PlayerKillReward { get; set; }
        public double ZombieKillReward { get; set; }
        public double MegaZombieKillReward { get; set; }
        public double Bounty { get; set; }

        public GameData()
        {
            Teams = new Dictionary<int, Team>();
            Loadouts = new Dictionary<int, Loadout>();
            ShopItems = new Dictionary<ushort, ShopItem>();
            PlayerData = new Dictionary<ulong, PlayerData>();
            Arenas = new Dictionary<int, BossArena>();
            ManagedZombiePools = new Dictionary<byte, int>();
            ObjectiveItems = new List<ushort>();
            LastLoadoutId = 0;
            LastTeamId = 0;
            LastArenaId = 0;
            DefaultRespawnPoint = null;
            State = GameState.InLobby;
            PlayerKillReward = 100;
            ZombieKillReward = 10;
            MegaZombieKillReward = 150;
            Bounty = 100;
        }
    }
}
