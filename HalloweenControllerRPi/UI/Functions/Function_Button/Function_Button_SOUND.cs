using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Functions.Function_Button;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace HalloweenControllerRPi.Function_GUI
{
   public class Function_Button_SOUND : Function_Button
   {
      public Function_Button_SOUND(uint idx)
         : base(typeof(Func_Sound_GUI), "SOUND", idx, Colors.Orchid)
      {
         IsRemoveable = false;
         OneOnly = false;
         TriggerOnly = false;//RPUGLIESE - true;
      }

      public Function_Button_SOUND(uint idx, Function.tenTYPE enType)
         : base(typeof(Func_Sound_GUI), "SOUND", idx, Colors.Orchid)
      {
         IsRemoveable = false;
         OneOnly = false;
         TriggerOnly = false;//RPUGLIESE - true;
      }
   }
}
