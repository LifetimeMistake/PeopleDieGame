using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.Reflection
{
    public class EnumInfo<T>
    {
        private Type type;
        private Dictionary<string, T> members;

        public IEnumerable<KeyValuePair<string, T>> Members => members;
        public IEnumerable<string> Names => members.Keys;
        public IEnumerable<T> Values => members.Values;

        public EnumInfo(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!type.IsEnum)
                throw new ArgumentException("Type is not an enum");

            if (type.GetEnumUnderlyingType() != typeof(T))
                throw new ArgumentException("Underlying enum type does not match generic argument");

            this.type = type;
            members = type.GetEnumNames().Zip(
                type.GetEnumValues().Cast<T>(),
                (name, value) => new KeyValuePair<string, T>(name, value)
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public object MemberToValue(string key)
        {
            if (!Names.Contains(key))
                throw new ArgumentException("Member name does not belong to this enum");

            return Enum.ToObject(type, members[key]);
        }

        public string ValueToMember(T value)
        {
            foreach (KeyValuePair<string, T> kvp in members)
            {
                if (kvp.Value.Equals(value))
                    return kvp.Key;
            }

            throw new ArgumentException("Value does not belong to this enum or is a set of flags");
        }
    }
}
