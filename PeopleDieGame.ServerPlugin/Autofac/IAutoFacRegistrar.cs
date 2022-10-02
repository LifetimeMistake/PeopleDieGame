using Autofac;

namespace PeopleDieGame.ServerPlugin.Autofac
{
    public interface IAutoFacRegistrar
    {
        void RegisterComponents(ContainerBuilder builder);
    }
}
