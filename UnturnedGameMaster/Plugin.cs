using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedGameMaster.Autofac;
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

            UnturnedChat.Say("Game manager loaded!");
        }

        protected override void Unload()
        {
            UnturnedChat.Say("Game manager unloading!");

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
