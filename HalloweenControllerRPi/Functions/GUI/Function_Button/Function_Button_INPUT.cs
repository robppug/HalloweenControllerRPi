using HalloweenControllerRPi.Functions;
using Windows.UI;

namespace HalloweenControllerRPi.Function_GUI
{
   public class Function_Button_INPUT : Function_Button
   {
      public Function_Button_INPUT(uint idx)
         : base(typeof(Func_Input_GUI), "INPUT", idx, Colors.Green)
      {
         IsRemoveable = false;
         OneOnly = true;

         //SetImage((Image)HalloweenController.Properties.Resources.input);
      }

      public Function_Button_INPUT(uint idx, Function.tenTYPE enType)
         : base(typeof(Func_Input_GUI), "INPUT", idx, Colors.Green)
      {
         IsRemoveable = false;
         OneOnly = true;
      }
   }
}