using Rocket.API;

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
