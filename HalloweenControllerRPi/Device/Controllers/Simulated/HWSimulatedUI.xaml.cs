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
      List<TextBlock> lSounds = new List<TextBlock>();
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

         lSounds.Add(SND_1);
         lSounds.Add(SND_2);
         lSounds.Add(SND_3);
         lSounds.Add(SND_4);
      }

      public void Update(Command function, Command subFunction, uint index, uint value)
      { 
         if(index > 0)
         {
            index--;
         }

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
                  case 'S':
                     lPwms[(int)index].Opacity = (double)((double)value / 100);
                     break;

                  case 'N':
                  case 'M':
                     lPwms[(int)index].Opacity = (double)((double)value / 100);

                     if (value >= 1)
                        lPwms[(int)index].Fill = new SolidColorBrush(Colors.Green);
                     else
                        lPwms[(int)index].Fill = new SolidColorBrush(Colors.Red);
                     break;
                  case 'R':
                     break;
                  case 'F':
                     switch((PWMFunctions)value)
                     {
                        case PWMFunctions.FUNC_OFF:
                           lPwms[(int)index].Fill = new SolidColorBrush(Colors.Red);
                           lPwms[(int)index].Opacity = 1;
                           break;
                        case PWMFunctions.FUNC_ON:
                           break;
                        case PWMFunctions.FUNC_FLICKER_OFF:
                           break;
                        case PWMFunctions.FUNC_FLICKER_ON:
                           break;
                        case PWMFunctions.FUNC_RANDOM:
                           break;
                        case PWMFunctions.FUNC_SIGNWAVE:
                           break;
                        case PWMFunctions.FUNC_STROBE:
                           break;
                        case PWMFunctions.FUNC_SWEEP_DOWN:
                           break;
                        case PWMFunctions.FUNC_SWEEP_UP:
                           break;
                        default:
                           break;
                     }
                     break;
                  default:
                     break;
               }
               break;
            case 'S':
               switch (subFunction.Value)
               {
                  case 'P':
                     lSounds[(int)index].Foreground = new SolidColorBrush(Colors.Green);
                     break;
                  case 'S':
                     lSounds[(int)index].Foreground = new SolidColorBrush(Colors.Black);
                     lSounds[(int)index].Opacity = 1.0;
                     break;
                  case 'V':
                     lSounds[(int)index].Opacity = (double)((double)value / 100);
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
