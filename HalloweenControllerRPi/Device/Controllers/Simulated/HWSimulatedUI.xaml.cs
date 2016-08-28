using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.Device.Controllers
{
   public sealed partial class HWSimulatedUI : UserControl
   {
      List<Rectangle> lRelays = new List<Rectangle>();
      List<Rectangle> lPwms = new List<Rectangle>();
      public event EventHandler OnInputTrigger;

      static public Stopwatch sWatch;
      static public long TriggerTime;

      public HWSimulatedUI()
      {
         this.InitializeComponent();

         lRelays.Add(Relay_1);
         lRelays.Add(Relay_2);
         lRelays.Add(Relay_3);
         lRelays.Add(Relay_4);

         lPwms.Add(PWM_1);
         lPwms.Add(PWM_2);
         lPwms.Add(PWM_3);
         lPwms.Add(PWM_4);
      }

      public void Update(Command function, Command subFunction, uint index, uint value)
      {
         switch (function.Value)
         {
            case 'R':
               if(value == 1)
                  lRelays[(int)index].Fill = new SolidColorBrush(Colors.Green);
               else
                  lRelays[(int)index].Fill = new SolidColorBrush(Colors.Red);
               break;
            case 'T':
               switch (subFunction.Value)
               {
                  case 'M':
                     if (value >= 1)
                        lPwms[(int)index].Fill = new SolidColorBrush(Colors.Green);
                     else
                        lPwms[(int)index].Fill = new SolidColorBrush(Colors.Red);

                     lPwms[(int)index].Opacity = (double)((double)value / 100);
                     break;
                  case 'R':
                     break;
                  case 'F':
                     switch((Func_PWM.tenFUNCTION)value)
                     {
                        case Func_PWM.tenFUNCTION.FUNC_OFF:
                           lPwms[(int)index].Fill = new SolidColorBrush(Colors.Red);
                           lPwms[(int)index].Opacity = 1;
                           break;
                        case Func_PWM.tenFUNCTION.FUNC_CONSTANT:
                           break;
                        case Func_PWM.tenFUNCTION.FUNC_FLICKER_OFF:
                           break;
                        case Func_PWM.tenFUNCTION.FUNC_FLICKER_ON:
                           break;
                        case Func_PWM.tenFUNCTION.FUNC_RANDOM:
                           break;
                        case Func_PWM.tenFUNCTION.FUNC_SIGNWAVE:
                           break;
                        case Func_PWM.tenFUNCTION.FUNC_STROBE:
                           break;
                        case Func_PWM.tenFUNCTION.FUNC_SWEEP_DOWN:
                           break;
                        case Func_PWM.tenFUNCTION.FUNC_SWEEP_UP:
                           break;
                        default:
                           break;
                     }
                     break;
                  default:
                     break;
               }
               break;
            default:
               break;
         }
      }

      private void FireInputTrigger(object index)
      {
         if (OnInputTrigger != null)
         {
            OnInputTrigger(index, EventArgs.Empty);
         }
      }
      private void buttonInput_Click(object sender, RoutedEventArgs e)
      {
         FireInputTrigger((sender as Button).Tag);
      }
   }
}
