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
            AutowirePropertySelector autowirePropertySelector = new AutowirePropertySelector();
            builder.RegisterType<GameManager>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            builder.RegisterType<LoadoutManager>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            builder.RegisterType<DataManager>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            builder.RegisterType<TimerManager>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            builder.RegisterType<RespawnManager>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            builder.RegisterType<TeamManager>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            builder.RegisterType<PlayerDataManager>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);

            builder.RegisterType<LoadoutIdProvider>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            builder.RegisterType<TeamIdProvider>().InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);

            builder.RegisterInstance(databaseProvider).As<IDatabaseProvider<GameData>>().ExternallyOwned();
        }
    }
}
