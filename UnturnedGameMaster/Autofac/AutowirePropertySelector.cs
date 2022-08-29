using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Autofac
{
    public class AutowirePropertySelector : IPropertySelector
    {
        public bool InjectProperty(PropertyInfo propertyInfo, object instance)
        {
            return (propertyInfo.CustomAttributes.Any(a => a.AttributeType == typeof(InjectDependencyAttribute)));
        }
    }
}
