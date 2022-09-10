using System;

namespace UnturnedGameMaster.Autofac
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectDependencyAttribute : Attribute
    {
    }
}
