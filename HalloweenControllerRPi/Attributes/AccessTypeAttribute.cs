using System;

namespace HalloweenControllerRPi.Attributes
{
   /// <summary>
   /// Enum Attribute - Read/Write Flags
   /// </summary>
   internal class AccessTypeAttribute : Attribute
   {
      private string v;

      public bool Read
      {
         get { return this.v.Contains("R"); }
      }
      public bool Write
      {
         get { return this.v.Contains("W"); }
      }
      public AccessTypeAttribute(string v)
      {
         this.v = v;
      }
   }
}
