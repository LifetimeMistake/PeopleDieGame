using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Autofac
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectDependencyAttribute : Attribute
    {
    }
}
