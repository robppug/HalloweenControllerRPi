using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace HalloweenControllerRPi.Function_GUI
{
   public class Function_Button_PWM : Function_Button
   {
      public Function_Button_PWM(uint idx)
         : base(typeof(Func_PWM_GUI), "PWM", idx, Colors.Orange)
      {
         IsRemoveable = true;
         OneOnly = false;

         SetImage(new Uri("ms-appx:///Assets/FunctionButtonPWM.png"));
      }

      public Function_Button_PWM(uint idx, Function.tenTYPE enType)
         : base(typeof(Func_PWM_GUI), "PWM", idx, Colors.Orange)
      {
         IsRemoveable = true;
         OneOnly = false;
      }
   }
}
