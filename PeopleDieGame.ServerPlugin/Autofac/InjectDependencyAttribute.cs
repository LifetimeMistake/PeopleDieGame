using System;

namespace PeopleDieGame.ServerPlugin.Autofac
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectDependencyAttribute : Attribute
    {
    }
}
