using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Function_GUI
{
   /// <summary>
   /// Controller FUNCTION GUI interface definition
   /// </summary>
   interface IFunctionGUI
   {
      Function Func { get; set; }

      void SetCustomName();
   }
}
