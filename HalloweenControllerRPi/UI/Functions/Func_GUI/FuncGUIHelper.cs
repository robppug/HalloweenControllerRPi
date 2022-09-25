using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HalloweenControllerRPi.UI.Functions.Func_GUI
{
    static class FuncGUIHelper
    {
        public static async Task<string> SetCustomName(string currentVal)
        {
            ContentDialog cd = new ContentDialog();
            StackPanel panel = new StackPanel();
            TextBox tb = new TextBox() { Text = currentVal };
            String retVal = currentVal;

            panel.Orientation = Orientation.Vertical;
            panel.Children.Add(tb);

            cd.Title = "Enter Custom Name";
            cd.PrimaryButtonText = "OK";
            cd.PrimaryButtonClick += (sender, e) =>
            {
                retVal = tb.Text;
            };
            cd.Content = panel;

            await cd.ShowAsync();

            return retVal;
        }

        public static PWMFunctions GetFunctionEnum(string val)
        {
            PWMFunctions retVal = PWMFunctions.FUNC_NO_OF_FUNCTIONS;

            foreach (PWMFunctions p in Enum.GetValues(typeof(PWMFunctions)))
            {
                if (p.ToString() == val)
                {
                    retVal = p;
                    break;
                }
            }

            return retVal;
        }
    }
}
