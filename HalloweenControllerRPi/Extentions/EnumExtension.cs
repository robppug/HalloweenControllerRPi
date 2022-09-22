using System;
using System.Reflection;

namespace HalloweenControllerRPi.Extentions
{
    internal static class EnumExtension<R, T> where R : Attribute
    {
        internal static R GetModeAttribute(T type)
        {
            R mde;
            Type enumType = type.GetType();

            try
            {
                mde = enumType.GetMember(type.ToString())[0].GetCustomAttribute<R>();
            }
            catch (IndexOutOfRangeException)
            {
                mde = null;
            }
            return mde;
        }
    }
}
