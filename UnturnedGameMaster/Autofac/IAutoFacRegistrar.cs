using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Autofac
{
    public interface IAutoFacRegistrar
    {
        void RegisterComponents(ContainerBuilder builder);
    }
}
