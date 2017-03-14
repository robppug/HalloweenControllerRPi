using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HalloweenControllerRPi.Device.Controllers
{
   public partial class HWSimulated : HWController
   {
      HWSimulatedUI UIPanel;

      event EventHandler Connected;

      public HWSimulated()
      {
         UIPanel = new HWSimulatedUI();

         UIPanel.OnInputTrigger += UIPanel_OnInputTrigger;
      }

      private void UIPanel_OnInputTrigger(object sender, EventArgs e)
      {
         TriggerCommandReceived(new CommandEventArgs('I', UInt32.Parse((sender as string)), 1));
      }

      /// <summary>
      /// Dictionary containing a list of all supported COMMANDS and SUB-COMMANDS.
      /// </summary>
      private Dictionary<Command, List<Command>> _Commands = new Dictionary<Command, List<Command>>
      {
         /* Command : DATA */
         {  new Command("DATA", 'C'),
            new List<Command>
            {
               new Command("VERSION", 'S'),
               new Command("FREERAM", 'F')
            }
         },
         /* Command : INPUT */
         {  new Command("INPUT", 'I'),
            new List<Command>
            {
               new Command("GET", 'G'),
               new Command("DEBTIME", 'D')
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
               new Command("MAXLEVEL", 'M'),
               new Command("RATE", 'R')
            }
         }
      };
      private bool _Connected;

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

      private Command GetSubFunctionCommand(Command function, string subFunc)
      {
         Command command = null;

         foreach (Command c in this.Commands[function].ToList())
         {
            if (c.Key == subFunc)
            {
               command = c;
            }
         }

         return command;
      }

      private Command GetFunctionCommand(string p)
      {
         Command command = null;

         foreach (Command c in this.Commands.Keys)
         {
            if (c.Key == p)
            {
               command = c;
               break;
            }
         }

         return command;
      }

      /// <summary>
      /// Builds the TX string based on the Function, Sub-Function and data provided.
      /// </summary>
      /// <param name="func">Function</param>
      /// <param name="subFunc">Sub-Function (if any)</param>
      /// <param name="data">Data (if any)</param>
      /// <returns></returns>
      public override string BuildCommand(string func, string subFunc, params string[] data)
      {
         StringBuilder fullCommand = new StringBuilder();

         Command function = this.GetFunctionCommand(func);
         Command subFunction = this.GetSubFunctionCommand(function, subFunc);

         if (function == null)
         {
            throw new HWInterfaceException("Function " + func + "  not available.");
         }

         fullCommand.Append(function.Value.ToString() + ": ");

         if (subFunc != null)
            fullCommand.Append(subFunction.Value.ToString());

         if (data.Length != 0)
         {
            foreach (string s in data)
               fullCommand.Append(" " + s);
         }

         fullCommand.Append(commandTerminator);

         return fullCommand.ToString();
      }

      public override void Connect()
      {
         _Connected = true;

         if (this.Connected != null)
         {

            //this.Connected.EndInvoke(this.Connected.BeginInvoke(this, EventArgs.Empty, null, null));
         }
      }

      /// <summary>
      /// Processed RX'ed commands and decodes the byte array, returning the Function, Sub-Function and Data (if any).
      /// </summary>
      /// <param name="command">List of bytes (actual RX'ed data)</param>
      /// <param name="function">Decoded FUNCTION (out param nullable)</param>
      /// <param name="subFunction">Decoded SUBFUNCTION (out param nullable)</param>
      /// <param name="data">Decoded Data Array (ref param)</param>
      public override void DecodeCommand(List<char> fullCmd, out Command function, out Command subFunction, ref char[] data)
      {
         char l_FuncCommand = (char)fullCmd[0];
         char l_SubCommand = (char)fullCmd[3];

         function = null;
         foreach (Command c in this.Commands.Keys)
         {
            if (c.Value == l_FuncCommand)
            {
               function = c;
               break;
            }
         }

         subFunction = null;
         foreach (Command c in this.Commands[function].ToList())
         {
            if (c.Value == l_SubCommand)
            {
               subFunction = c;
            }
         }

         fullCmd.CopyTo(5, data, 0, fullCmd.Count - 5);
      }

      public override void Disconnect()
      {
         _Connected = false;
      }

      public override void TransmitCommand(string cmd)
      {
         Command function;
         Command subFunction;
         char[] decodedData = new char[20];
         uint index;
         uint value = 0;

         DecodeCommand(cmd.ToList<char>(), out function, out subFunction, ref decodedData);

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

                     value = UInt32.Parse(decodedData[0].ToString());
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

                     switch ((Func_PWM.tenFUNCTION)value)
                     {
                        case Func_PWM.tenFUNCTION.FUNC_OFF:
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
                  case 'M':
                     value = UInt32.Parse(new string(decodedData));
                     break;
                  case 'R':
                     value = UInt32.Parse(new string(decodedData));
                     break;
                  default:
                     break;
               }

               UIPanel.Update(function, subFunction, index, value);
               break;
            case 'A': //ADC
               break;
            case 'C':
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

            DecodeCommand(data, out function, out subFunction, ref decodedData);

            switch (function.Value)
            {
               case 'I':
                  if (data.Count >= 8)
                  {
                     char cInputIdx = (char)(data[3] - 0x30);
                     char cInputLevel = (char)(data[5] - 0x30);
                     data.RemoveRange(0, 8);

                     //Packet received, allow active groups to process.
                     TriggerCommandReceived(new CommandEventArgs(function.Value, cInputIdx, cInputLevel));
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
   }
}
