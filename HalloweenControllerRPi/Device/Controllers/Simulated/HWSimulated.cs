using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.ExternalDisplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace HalloweenControllerRPi.Device.Controllers
{
   public partial class HWSimulated : HWController
   {
      HWSimulatedUI UIPanel;

      public HWSimulated()
      {
         UIPanel = new HWSimulatedUI();

         UIPanel.OnInputTrigger += UIPanel_OnInputTrigger;

         //HWController.Display = new GraphicsProvider(null);
      }

      private void UIPanel_OnInputTrigger(object sender, EventArgs e)
      {
         TransmitCommand(new CommandEventArgs('I', 'G', UInt32.Parse((sender as string)), 1));
      }

      /// <summary>
      /// Dictionary containing a list of all supported COMMANDS and SUB-COMMANDS.
      /// </summary>
      private Dictionary<Command, List<Command>> _Commands = new Dictionary<Command, List<Command>>
      {
         /* Command : INPUT */
         {  new Command("INPUT", 'I'),
            new List<Command>
            {
               new Command("GET", 'G'),
               new Command("DEBTIME", 'D'),
               new Command("POSTDEBTIME", 'P')
            }
         },
         /* Command : RELAY */
         {  new Command("RELAY", 'R'),
            new List<Command>
            {
               new Command("GET", 'G'),
               new Command("SET", 'S')
            }
         },
         /* Command : PWM */
         {  new Command("PWM", 'T'),
            new List<Command>
            {
               new Command("GET", 'G'),
               new Command("SET", 'S'),
               new Command("FUNCTION", 'F'),
               new Command("MINLEVEL", 'N'),
               new Command("MAXLEVEL", 'M'),
               new Command("RATE", 'R'),
               new Command("DATA", 'D'),
               new Command("RAMPRATE", 'A')
            }
         },
         /* Command : SOUND */
         {  new Command("SOUND", 'S'),
            new List<Command>
            {
               new Command("PLAY", 'P'),
               new Command("TRACK", 'T'),
               new Command("STOP", 'S'),
               new Command("LOOP", 'L'),
               new Command("VOLUME", 'V'),
               new Command("FEEDBACK", 'F'),
               new Command("AVAILABLE TRACKS", 'A')
            }
         }
      };

      /// <summary>
      /// Dictionary containing available Functions and Sub-Functions.
      /// </summary>
      public override Dictionary<Command, List<Command>> Commands
      {
         get
         {
            return _Commands;
         }
      }

      public override uint Inputs
      {
         get
         {
            return 4;
         }
      }

      public override uint PWMs
      {
         get
         {
            return 4;
         }
      }

      public override uint Relays
      {
         get
         {
            return 4;
         }
      }

      public override uint SoundChannels
      {
         get
         {
            return 4;
         }
      }
      public override bool HasDisplay
      {
         get
         {
            return false;
         }
      }

      public override void Connect()
      {
         CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
         {
            await Discovery();
         }).AsTask();
      }

      private async Task Discovery()
      {
         uint discovery = 0;

         OnDisplayInitialised();

         while (discovery < 100)
         {
            System.Diagnostics.Debug.WriteLine("\n\nUPDATE: " + discovery.ToString());

            OnDiscoveryProgressUpdated(discovery);

            await Task.Delay(50);

            discovery++;
         }

         OnControllerInitialised();
      }

      public override void Disconnect()
      {
      }

      public override void TransmitCommand(string cmd)
      {
         Command function;
         Command subFunction;
         char[] decodedData = new char[cmd.Length];
         uint index;
         uint value = 0;

         DecodeCommand(cmd, out function, out subFunction, ref decodedData);

         index = UInt32.Parse(new string(decodedData).Substring(0, 2));

         switch (function.Value)
         {
            case 'I': //INPUT
               break;
            case 'R': //RELAY
               switch (subFunction.Value)
               {
                  case 'S':
                     new string(decodedData).Remove(0, 2).ToCharArray().CopyTo(decodedData, 0);

                     value = UInt32.Parse(decodedData.ToString());
                     break;
                  case 'G':
                     break;
                  default:
                     break;
               }

               UIPanel.Update(function, subFunction, index, value);
               break;
            case 'T': //PWM
               //Remove the Function and Index from the string
               new string(decodedData).Remove(0, 2).ToCharArray().CopyTo(decodedData, 0);

               switch (subFunction.Value)
               {
                  case 'S':
                     value = UInt32.Parse(new string(decodedData));
                     break;
                  case 'G':
                     break;
                  case 'F':
                     value = UInt32.Parse(new string(decodedData));

                     switch ((PWMFunctions)value)
                     {
                        case PWMFunctions.FUNC_OFF:
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
                        case PWMFunctions.FUNC_CUSTOM:
                           break;
                        case PWMFunctions.FUNC_RAMP_ON:
                           break;
                        case PWMFunctions.FUNC_RAMP_OFF:
                           break;
                        case PWMFunctions.FUNC_RAMP_BOTH:
                           break;
                        default:
                           break;
                     }
                     break;
                  case 'M':
                     value = UInt32.Parse(new string(decodedData));
                     break;
                  case 'R':
                     value = UInt32.Parse(new string(decodedData));
                     break;
                  case 'A':
                     break;
                  case 'D':
                     break;
                  default:
                     break;
               }

               UIPanel.Update(function, subFunction, index, value);
               break;
            case 'S':
               //Remove the Function and Index from the string
               new string(decodedData).Remove(0, 2).ToCharArray().CopyTo(decodedData, 0);

               switch (subFunction.Value)
               {
                  case 'P':
                     value = 1;
                     break;
                  case 'S':
                     value = 0;
                     break;
                  case 'V':
                     value = UInt32.Parse(new string(decodedData));
                     break;

                  case 'A':
                     TransmitCommand(new CommandEventArgs(function.Value, subFunction.Value, index, (uint)new Random().Next(2, 10)));
                     break;
               }

               UIPanel.Update(function, subFunction, index, value);
               break;
            default:
               break;
         }

      }

      /// <summary>
      /// Handling of Rx SERIAL command interpretation
      /// </summary>
      /// <param name="data"></param>
      /// <returns>True if COMMAND was successfully handled</returns>
      public override bool ReceivedCommand(List<char> data)
      {
         bool l_Result = false;
         Command function;
         Command subFunction;
         char[] decodedData = new char[20];

         if (data.Count > 0)
         {
            while (data[0] == 0x00) { data.RemoveAt(0); }

            DecodeCommand(String.Join(null, data), out function, out subFunction, ref decodedData);

            switch (function.Value)
            {
               case 'I':
                  if (data.Count >= 8)
                  {
                     char cInputIdx = (char)(data[3] - 0x30);
                     char cInputLevel = (char)(data[5] - 0x30);
                     data.RemoveRange(0, 8);

                     //Packet received, allow active groups to process.
                     TransmitCommand(new CommandEventArgs(function.Value, subFunction.Value, cInputIdx, cInputLevel));
                     l_Result = true;
                  }
                  break;
               case 'R':
                  break;
               case 'P':
                  break;
               case 'A':
                  break;
               case 'C':
                  //if (data.Count >= (5 + 12))
                  //{
                  //   string sVersion = "Version: v" + (data[6] - 0x30) + "." + (data[7] - 0x30);

                  //   base.OnVersionInfoUpdated(sVersion);

                  //   if (data[12] == 'I')
                  //   {
                  //      uint u32InputCnt = (uint)(data[13] - 0x30);

                  //      for (uint i = 0; i < u32InputCnt; i++)
                  //      {
                  //         base.OnFunctionAdded(typeof(Func_INPUT));
                  //      }
                  //   }

                  //   if (data[8] == 'R')
                  //   {
                  //      uint u32RelayCnt = (uint)(data[9] - 0x30);

                  //      for (uint i = 0; i < u32RelayCnt; i++)
                  //      {
                  //         base.OnFunctionAdded(typeof(Func_RELAY));
                  //      }
                  //   }

                  //   if (data[10] == 'P')
                  //   {
                  //      uint u32RelayCnt = (uint)(data[11] - 0x30);

                  //      for (uint i = 0; i < u32RelayCnt; i++)
                  //      {
                  //         base.OnFunctionAdded(typeof(Func_PWM));
                  //      }
                  //   }

                  //   data.RemoveRange(0, 5 + 12);
                  //   l_Result = true;
                  //}
                  break;
               default:
                  break;
            }
         }
         return l_Result;
      }
      public override UserControl GetUIPanel()
      {
         return UIPanel;
      }

      public override void OnChannelNotification(IChannel sender, CommandEventArgs e)
      {
         throw new NotImplementedException();
      }
   }
}
