using System;

namespace HalloweenControllerRPi.Attributes
{
   /// <summary>
   /// Enum Attribute - Function Attribute
   /// </summary>
   internal class FunctionAttribute : Attribute
   {
      [Flags]
      public enum FunctionType
      {
         CONSTANT = 1,
         TRIGGERED = 2
      };

      private FunctionType flags;

      public bool IsConstant
      {
         get { return (flags & FunctionType.CONSTANT) == FunctionType.CONSTANT; }
      }
      public bool IsTriggered
      {
         get { return (flags & FunctionType.TRIGGERED) == FunctionType.TRIGGERED; }
      }
      public FunctionAttribute(FunctionType v)
      {
         this.flags = v;
      }
   }
}
