using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models
{
    public class GameData
    {
        public Dictionary<int, Team> Teams { get; set; }
        public Dictionary<int, Loadout> Loadouts { get; set; }
        public Dictionary<ushort, ShopItem> ShopItems { get; set; }
        public List<PlayerData> PlayerData { get; set; }
        public List<BossArena> Arenas { get; set; }
        public int LastLoadoutId { get; set; }
        public int LastTeamId { get; set; }
        public RespawnPoint? DefaultRespawnPoint { get; set; }
        public GameState State { get; set; }

        public GameData()
        {
            Teams = new Dictionary<int, Team>();
            Loadouts = new Dictionary<int, Loadout>();
            ShopItems = new Dictionary<ushort, ShopItem>();
            PlayerData = new List<PlayerData>();
            Arenas = new List<BossArena>();
            LastLoadoutId = 0;
            LastTeamId = 0;
            DefaultRespawnPoint = null;
            State = GameState.InLobby;
        }
    }
}
