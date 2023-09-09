using Autofac;
using PeopleDieGame.ClientModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeopleDieGame.ClientModule.Autofac
{
    public class PluginAutoFacRegistrar : IAutoFacRegistrar
    {
        public PluginAutoFacRegistrar() { }

        public void RegisterComponents(ContainerBuilder builder)
        {
            AutowirePropertySelector autowirePropertySelector = new AutowirePropertySelector();

            IEnumerable<Type> services = Assembly.GetCallingAssembly()
                .GetTypes().Where(x => typeof(IService).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract);

            foreach (Type serviceType in services)
            {
                builder.RegisterType(serviceType).InstancePerLifetimeScope().PropertiesAutowired(autowirePropertySelector, true);
            }
        }
    }
}
