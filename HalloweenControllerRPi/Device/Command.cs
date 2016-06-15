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

      public /*protected*/ Command(string key, char value)
      {
         this.Key = key;
         this.Value = value;
      }

      public void SendCommand(uint command)
      {
      }

      public override string ToString()
      {
         return this.Key;
      }
   }
}
