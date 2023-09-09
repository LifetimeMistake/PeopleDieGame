using System.Collections.Generic;
using PeopleDieGame.ServerPlugin.Enums;

namespace PeopleDieGame.ServerPlugin.Models
{
    public class GameData
    {
        public Dictionary<int, Team> Teams { get; set; }
        public Dictionary<int, Loadout> Loadouts { get; set; }
        public Dictionary<ushort, ShopItem> ShopItems { get; set; }
        public Dictionary<ulong, PlayerData> PlayerData { get; set; }
        public Dictionary<int, BossArena> Arenas { get; set; }
        public Dictionary<byte, int> ManagedZombiePools { get; set; }
        public Dictionary<ushort, ObjectiveItem> ObjectiveItems { get; set; }
        public Altar Altar { get; set; }
        public int LastLoadoutId { get; set; }
        public int LastTeamId { get; set; }
        public int LastArenaId { get; set; }
        public VectorPAR? DefaultRespawnPoint { get; set; }
        public GameState State { get; set; }
        public float PlayerKillReward { get; set; }
        public float ZombieKillReward { get; set; }
        public float MegaZombieKillReward { get; set; }
        public float Bounty { get; set; }
        public int IntermissionTime { get; set; }
        public int ClosingTime { get; set; }

        public GameData()
        {
            Teams = new Dictionary<int, Team>();
            Loadouts = new Dictionary<int, Loadout>();
            ShopItems = new Dictionary<ushort, ShopItem>();
            PlayerData = new Dictionary<ulong, PlayerData>();
            Arenas = new Dictionary<int, BossArena>();
            ManagedZombiePools = new Dictionary<byte, int>();
            ObjectiveItems = new Dictionary<ushort, ObjectiveItem>();
            Altar = new Altar();
            LastLoadoutId = 0;
            LastTeamId = 0;
            LastArenaId = 0;
            DefaultRespawnPoint = null;
            State = GameState.InLobby;
            PlayerKillReward = 100;
            ZombieKillReward = 10;
            MegaZombieKillReward = 150;
            Bounty = 100;
            IntermissionTime = 10;
            ClosingTime = 30;
        }
    }
}

//💀