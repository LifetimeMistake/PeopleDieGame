using Autofac;

namespace UnturnedGameMaster.Autofac
{
    public interface IAutoFacRegistrar
    {
        void RegisterComponents(ContainerBuilder builder);
    }
}
