using Rocket.Core.Plugins;
using System.IO;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster
{
    public class Plugin : RocketPlugin<PluginConfig>
    {
        private ServiceLocator serviceLocator;
        protected override void Load()
        {
            IDatabaseProvider<GameData> databaseProvider = InitDatabase();
            PluginAutoFacRegistrar pluginAutoFacRegistrar = new PluginAutoFacRegistrar(databaseProvider);
            serviceLocator = new ServiceLocator();
            serviceLocator.Initialize(pluginAutoFacRegistrar);
            serviceLocator.BeginLifetimeScope();

            // Init all managers
            foreach (IManager manager in serviceLocator.LocateServicesOfType<IManager>())
            {
                manager.Init();
            }

            ChatHelper.Say("Game manager loaded!");
        }

        protected override void Unload()
        {
            ChatHelper.Say("Game manager unloading!");

            // Dispose all managers
            foreach (IDisposableManager manager in serviceLocator.LocateServicesOfType<IDisposableManager>())
            {
                manager.Dispose();
            }
        }

        private IDatabaseProvider<GameData> InitDatabase()
        {
            string configPath = Configuration.Instance.GameConfigPath;
            if (File.Exists(configPath))
                return new JsonDatabaseProvider<GameData>(configPath);
            else
                return new JsonDatabaseProvider<GameData>(configPath, new GameData());
        }
    }
}
