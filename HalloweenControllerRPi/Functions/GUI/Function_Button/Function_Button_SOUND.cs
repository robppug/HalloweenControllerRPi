using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace HalloweenControllerRPi.Function_GUI
{
   class Function_Button_SOUND : Function_Button
   {
      public Function_Button_SOUND(uint idx)
         : base(typeof(Func_Sound_GUI), "SOUND", Colors.Lavender)
      {
         IsRemoveable = false;
         OneOnly = false;
      }

      public Function_Button_SOUND(uint idx, Function.tenTYPE enType)
         : base(typeof(Func_Sound_GUI), "SOUND", Colors.Lavender)
      {
         IsRemoveable = false;
         OneOnly = false;
      }
   }
}
