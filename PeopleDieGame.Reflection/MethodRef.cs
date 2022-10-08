using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.Reflection
{
    public class MethodRef
    {
        private MethodInfo method;
        private int parameterCount;
        private object instance;

        public MethodRef(MethodInfo method, object instance = null)
        {
            this.method = method ?? throw new ArgumentNullException(nameof(method));
            parameterCount = method.GetParameters().Length;
            this.instance = instance;
        }

        public object Invoke(params object[] arguments)
        {
            if (arguments.Length != parameterCount)
                throw new ArgumentException($"Method expected {parameterCount} parameters, but {arguments.Length} parameters have been provided.");
            
            return method.Invoke(instance, arguments);
        }

        public static MethodRef GetMethodRef<I>(I instance, string methodName)
        {
            MethodInfo methodInfo = AccessTools.Method(typeof(I), methodName);
            return new MethodRef(methodInfo, instance);
        }

        public static MethodRef GetMethodRef(Type type, string methodName)
        {
            MethodInfo methodInfo = AccessTools.Method(type, methodName);
            return new MethodRef(methodInfo, null);
        }
    }
}
