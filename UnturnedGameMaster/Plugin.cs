using HarmonyLib;
using Rocket.Core.Plugins;
using System.IO;
using System.Linq;
using UnityEngine;
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
        private Harmony harmony;
        protected override void Load()
        {
            harmony = new Harmony("UnturnedGameMaster");
            harmony.PatchAll();
            Debug.Log($"Patched {harmony.GetPatchedMethods().Count()} game methods");

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

            harmony.UnpatchAll();
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
