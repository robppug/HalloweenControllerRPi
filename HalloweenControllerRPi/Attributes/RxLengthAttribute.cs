using System;

namespace HalloweenControllerRPi.Attributes
{
   /// <summary>
   /// </summary>
   internal class RxLengthAttribute : Attribute
   {
      private int len;

      public int ExpectedLength
      {
         get { return this.len; }
      }
      public RxLengthAttribute(int v)
      {
         this.len = v;
      }
   }
}
