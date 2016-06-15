using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device
{
   public class CommandEventArgs
   {
      public char Commamd
      {
         get;
         private set;
      }

      public char Par1
      {
         get;
         private set;
      }

      public char Par2
      {
         get;
         private set;
      }

      public CommandEventArgs(char cmd, char par1, char par2)
      {
         this.Commamd = cmd;
         this.Par1 = par1;
         this.Par2 = par2;
      }
   }
}
