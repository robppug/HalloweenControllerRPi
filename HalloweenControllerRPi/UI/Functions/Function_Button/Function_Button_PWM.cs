﻿using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Functions.Function_Button;
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
      }

      public Function_Button_PWM(uint idx, Function.tenTYPE enType)
         : base(typeof(Func_PWM_GUI), "PWM", idx, Colors.Orange)
      {
         IsRemoveable = true;
         OneOnly = false;
      }
   }
}
