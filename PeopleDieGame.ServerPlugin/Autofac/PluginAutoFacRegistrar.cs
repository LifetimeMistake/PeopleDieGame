using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services;
using PeopleDieGame.ServerPlugin.Services.Providers;

namespace PeopleDieGame.ServerPlugin.Autofac
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

            IEnumerable<Type> managers = Assembly.GetCallingAssembly()
                .GetTypes().Where(x => typeof(IService).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract);

            IEnumerable<Type> bosses = Assembly.GetCallingAssembly()
                .GetTypes().Where(x => typeof(IZombieModel).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract);

            foreach (Type managerType in managers)
            {
                builder.RegisterType(managerType).InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            }

            foreach (Type bossType in bosses)
            {
                builder.RegisterType(bossType).InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            }

            builder.RegisterInstance(databaseProvider).As<IDatabaseProvider<GameData>>().ExternallyOwned();
        }
    }
}
