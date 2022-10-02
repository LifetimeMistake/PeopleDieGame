using Rocket.API;

namespace PeopleDieGame.ServerPlugin.Models
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
