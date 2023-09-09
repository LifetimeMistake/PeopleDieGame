using PeopleDieGame.ClientModule.Models;
using PeopleDieGame.NetMethods;
using PeopleDieGame.ClientModule.Autofac;
using SDG.Framework.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.ClientModule
{
    public class ModuleEntrypoint : IModuleNexus
    {
        private ServiceLocator serviceLocator;

        public void initialize()
        {
            try
            {
                NetMethodsLoader.Load();
                LoadServices();
            }
            catch(Exception ex)
            {
                Debug.Log(ex);
            }
        }

        public void shutdown()
        {
            try
            {
                NetMethodsLoader.Unload();
                UnloadServices();
            }
            catch(Exception ex)
            {
                Debug.Log(ex);
            }
        }

        private void LoadServices()
        {
            Debug.Log("Loading services...");
            PluginAutoFacRegistrar pluginAutoFacRegistrar = new PluginAutoFacRegistrar();
            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.Initialize(pluginAutoFacRegistrar);
            serviceLocator.BeginLifetimeScope();

            // Init all services
            foreach (IService service in serviceLocator.LocateServicesOfType<IService>())
            {
                Debug.Log($"Loading service {service.GetType().Name}");
                service.Init();
            }
        }

        private void UnloadServices()
        {
            Debug.Log("Unloading services...");

            if (serviceLocator != null)
            {
                // Dispose all services
                foreach (IDisposable service in serviceLocator.LocateServicesOfType<IDisposable>())
                {
                    service.Dispose();
                }
            }
        }
    }
}
