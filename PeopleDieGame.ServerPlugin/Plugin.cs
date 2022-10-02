using HarmonyLib;
using Rocket.Core.Plugins;
using SDG.Framework.Devkit;
using System.IO;
using System.Linq;
using UnityEngine;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services;
using PeopleDieGame.ServerPlugin.Services.Providers;

namespace PeopleDieGame.ServerPlugin
{
    public class Plugin : RocketPlugin<PluginConfig>
    {
        private ServiceLocator serviceLocator;
        private Harmony harmony;
        protected override void Load()
        {
            harmony = new Harmony("PeopleDieGame.ServerPlugin");
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
            Debug.Log("Loading services...");
            IDatabaseProvider<GameData> databaseProvider = InitDatabase();
            PluginAutoFacRegistrar pluginAutoFacRegistrar = new PluginAutoFacRegistrar(databaseProvider);
            serviceLocator = new ServiceLocator();
            serviceLocator.Initialize(pluginAutoFacRegistrar);
            serviceLocator.BeginLifetimeScope();

            // Init all services
            foreach (IService service in serviceLocator.LocateServicesOfType<IService>())
            {
                service.Init();
            }

            ChatHelper.Say("Game manager loaded!");
        }

        private void UnloadManagers()
        {
            Debug.Log("Unloading services...");
            ChatHelper.Say("Game manager unloading!");

            // Dispose all services
            foreach (IDisposableService service in serviceLocator.LocateServicesOfType<IDisposableService>())
            {
                service.Dispose();
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
