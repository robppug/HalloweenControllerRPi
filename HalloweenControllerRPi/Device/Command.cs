using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device
{
   public class Command
   {
      public string Key { get; protected set; }
      public char Value { get; protected set; }

      public Command() { }

      public Command(string key, char value)
      {
         this.Key = key;
         this.Value = value;
      }

      public override string ToString()
      {
         return this.Key;
      }
   }
}
