using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models
{
    public class PluginConfig : IRocketPluginConfiguration
    {
        public string GameConfigPath { get; set; }
        public void LoadDefaults()
        {
            GameConfigPath = "gameConfig.json";
        }
    }
}
