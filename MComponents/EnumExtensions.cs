using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MComponents
{
    public static class EnumExtensions
    {
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());

            if (memberInfo.Length <= 0)
                return null;

            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0 ? (T)attributes[0] : null;
        }

        public static string ToName(this Enum value)
        {
            if (value.GetType().GetCustomAttribute<FlagsAttribute>() != null)
            {
                string str = string.Empty;

                foreach (Enum flagVal in Enum.GetValues(value.GetType()))
                {
                    if ((int)(object)flagVal == 0 || !value.HasFlag(flagVal))
                        continue;

                    var flagAttr = flagVal.GetAttribute<DisplayAttribute>();
                    str += flagAttr == null ? flagVal.ToString() : flagAttr.GetName();
                    str += ", ";
                }

                return str.Trim(',', ' ');
            }

            var attribute = value.GetAttribute<DisplayAttribute>();
            return attribute == null ? value.ToString() : attribute.GetName();
        }
    }
}
