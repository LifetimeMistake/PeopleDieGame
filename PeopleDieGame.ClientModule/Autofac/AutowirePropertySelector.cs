using Autofac.Core;
using System.Linq;
using System.Reflection;

namespace PeopleDieGame.ClientModule.Autofac
{
    public class AutowirePropertySelector : IPropertySelector
    {
        public bool InjectProperty(PropertyInfo propertyInfo, object instance)
        {
            return (propertyInfo.CustomAttributes.Any(a => a.AttributeType == typeof(InjectDependencyAttribute)));
        }
    }
}
