using Autofac;

namespace PeopleDieGame.ClientModule.Autofac
{
    public interface IAutoFacRegistrar
    {
        void RegisterComponents(ContainerBuilder builder);
    }
}
