using System;

namespace PeopleDieGame.ClientModule.Autofac
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectDependencyAttribute : Attribute
    {
    }
}
