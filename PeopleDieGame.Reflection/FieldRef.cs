using HarmonyLib;
using System;
using System.Reflection;

namespace PeopleDieGame.Reflection
{
    public class FieldRef<T>
    {
        private FieldInfo field;
        private object instance;

        public T Value { get { return (T)field.GetValue(instance); } set { field.SetValue(instance, value); } }

        public FieldRef(FieldInfo field, object instance = null)
        {
            this.field = field ?? throw new ArgumentNullException(nameof(field));
            this.instance = instance;
        }
    }

    public static class FieldRef
    {
        public static FieldRef<F> GetFieldRef<I, F>(I instance, string fieldName)
        {
            FieldInfo fieldInfo = AccessTools.Field(typeof(I), fieldName);
            return new FieldRef<F>(fieldInfo, instance);
        }

        public static FieldRef<F> GetFieldRef<F>(Type type, string fieldName)
        {
            FieldInfo fieldInfo = AccessTools.Field(type, fieldName);
            return new FieldRef<F>(fieldInfo, null);
        }
    }
}
