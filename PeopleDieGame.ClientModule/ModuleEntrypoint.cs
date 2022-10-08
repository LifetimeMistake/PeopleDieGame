using PeopleDieGame.NetMethods;
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
        public void initialize()
        {
            try
            {
                Debug.Log("Registering custom RPC calls...");
                int customCallCount = CustomNetReflection.RegisterCustomRPCs();
                Debug.Log($"Registered {customCallCount} calls!");
            }
            catch(Exception ex)
            {
                Debug.Log(ex);
            }
        }

        public void shutdown()
        {
            
        }
    }
}
