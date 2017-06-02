using System;
using System.Reflection;

namespace HalloweenControllerRPi.Extentions
{
   internal static class EnumExtension<R, T> where R : Attribute
   {
      internal static R GetModeAttribute(T type)
      {
         Type enumType = type.GetType();
         R mde = enumType.GetMember(type.ToString())[0].GetCustomAttribute<R>();
         return mde;
      }
   }
}
