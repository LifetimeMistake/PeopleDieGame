using HarmonyLib;
using Rocket.Core.Plugins;
using SDG.Framework.Devkit;
using SDG.Unturned;
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

            LevelHierarchy.ready += LevelHierarchy_ready;
        }

        private void LevelHierarchy_ready()
        {
            LoadManagers();
        }

        private void LoadManagers()
        {
            Debug.Log("Loading managers...");
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

        private void UnloadManagers()
        {
            Debug.Log("Unloading managers...");
            ChatHelper.Say("Game manager unloading!");

            // Dispose all managers
            foreach (IDisposableManager manager in serviceLocator.LocateServicesOfType<IDisposableManager>())
            {
                manager.Dispose();
            }
        }

        protected override void Unload()
        {
            LevelHierarchy.ready -= LevelHierarchy_ready;
            UnloadManagers();
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
