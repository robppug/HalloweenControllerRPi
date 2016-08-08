using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalloweenControllerRPi.Functions;

namespace HalloweenControllerRPi.Device
{
   public class Controller : IHostApp
   {
      public string BuildCommand(string function, string subFunc, params string[] data)
      {
         throw new NotImplementedException();
      }

      public void FireCommand(string cmd)
      {
         throw new NotImplementedException();
      }

      public List<Command> GetSubFunctionCommandsList(Command functionKey)
      {
         throw new NotImplementedException();
      }

      public void TriggerEnd(Function func)
      {
         throw new NotImplementedException();
      }
   }
}
