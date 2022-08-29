using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Autofac
{
    public class PluginAutoFacRegistrar : IAutoFacRegistrar
    {
        private IDatabaseProvider<GameData> databaseProvider;

        public PluginAutoFacRegistrar(IDatabaseProvider<GameData> databaseProvider)
        {
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        public void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<GameManager>().InstancePerLifetimeScope();
            builder.RegisterType<LoadoutManager>().InstancePerLifetimeScope();
            builder.RegisterType<DataManager>().InstancePerLifetimeScope();
            builder.RegisterType<TimerManager>().InstancePerLifetimeScope();
            builder.RegisterType<RespawnManager>().InstancePerLifetimeScope();
            builder.RegisterType<TeamManager>().InstancePerLifetimeScope();
            builder.RegisterType<PlayerDataManager>().InstancePerLifetimeScope();

            builder.RegisterType<LoadoutIdProvider>().InstancePerLifetimeScope();
            builder.RegisterType<TeamIdProvider>().InstancePerLifetimeScope();

            builder.RegisterInstance(databaseProvider).As<IDatabaseProvider<GameData>>().ExternallyOwned();
        }
    }
}
