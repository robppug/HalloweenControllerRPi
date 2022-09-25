using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Functions.Function_Button;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace HalloweenControllerRPi.Function_GUI
{
    public class Function_Button_RELAY : Function_Button
    {
        public Function_Button_RELAY(uint idx)
           : base(typeof(Func_Relay_GUI), "RELAY", idx, Colors.Red)
        {
            IsRemoveable = true;
            OneOnly = false;
        }

        public Function_Button_RELAY(uint idx, Function.tenTYPE enType)
           : base(typeof(Func_Relay_GUI), "RELAY", idx, Colors.Red)
        {
            IsRemoveable = true;
            OneOnly = false;
        }
    }
}
