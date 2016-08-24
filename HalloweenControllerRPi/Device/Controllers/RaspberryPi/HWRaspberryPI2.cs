using HalloweenControllerRPi.Device.Controllers.RaspberryPi;
using HalloweenControllerRPi.Functions;
using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers
{
   class HWRaspberryPI2 : HWInterface
   {
      private enum tenPWMChannels
      {
         enChan1 = 0,
         enChan2,
         enChan3,
         enChan4,
         enChan5,
         enChan6,
         enChan7,
         enChan8,
         enChan9,
         enChan10,
         enChan11,
         enChan12,
         enChan13,
         enChan14,
         enChan15,
         enChan16
      };


      public enum tenInputPins
      {
         INPUT_PIN_04 = 4,
         INPUT_PIN_05 = 5,
         INPUT_PIN_06 = 6
      };

      private static I2cDevice i2cDevice;
      private static I2cController i2cController;
      private static I2cConnectionSettings i2cSettings;

      private static GpioController gpioController;

      private static Stopwatch sWatch;
      private static long TriggerTime;

      private static List<HWRaspberryPI_PWM> lPWMs = new List<HWRaspberryPI_PWM>();
      private static List<HWRaspberryPI_INPUT> lINPUTs = new List<HWRaspberryPI_INPUT>();

      private static byte[] bMODE1 = new byte[1] { 0x00 };
      private static byte[] bMODE2 = new byte[1] { 0x01 };
      private static byte[] LED_ON_L = new byte[1] { 0x06 };
      private static byte[] LED_ON_H = new byte[1] { 0x07 };
      private static byte[] LED_OFF_L = new byte[1] { 0x08 };
      private static byte[] LED_OFF_H = new byte[1] { 0x09 };


      public HWRaspberryPI2()
      {
         if (LightningProvider.IsLightningEnabled == true)
         {
            GetControllers();
         }
         else
         {
            //throw new Exception("No supported devices found.");
         }
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
               new Command("VERSION", 'S')
            }
         },
         /* Command : INPUT */
         {  new Command("INPUT", 'I'),
            new List<Command>
            {
               new Command("GET", 'G')
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
            return (uint)(tenPWMChannels.enChan16 + 1);
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

      private async void GetControllers()
      {
         gpioController = await GpioController.GetDefaultAsync();

         i2cController = (await I2cController.GetControllersAsync(LightningI2cProvider.GetI2cProvider()))[0];
      }

      public async void OnConnect()
      {
         i2cDevice = i2cController.GetDevice(i2cSettings);

         byte[] buffer = new byte[10];

         /* Change to NORMAL mode */
         i2cDevice.Write(new byte[2] { 0x00, 0x00 });

         await Task.Delay(1);

         //Turn OFF all PWM channels
         foreach (tenPWMChannels c in Enum.GetValues(typeof(tenPWMChannels)))
         {
            lPWMs.Add(new HWRaspberryPI_PWM((uint)c));

            i2cDevice.Write(new byte[2] { (byte)(LED_ON_L[0] + ((byte)lPWMs[(int)c].Channel * 4)), 0x00 });
            i2cDevice.Write(new byte[2] { (byte)(LED_ON_H[0] + ((byte)lPWMs[(int)c].Channel * 4)), 0x00 });
            i2cDevice.Write(new byte[2] { (byte)(LED_OFF_L[0] + ((byte)lPWMs[(int)c].Channel * 4)), 0x00 });
            i2cDevice.Write(new byte[2] { (byte)(LED_OFF_H[0] + ((byte)lPWMs[(int)c].Channel * 4)), 0x00 });
         }

         for (uint i = Inputs; i > 0; i--)
         {
            lINPUTs.Add(new HWRaspberryPI_INPUT(i, gpioController.OpenPin((int)tenInputPins.INPUT_PIN_04)));
            lINPUTs[(int)i].InputLevelChanged += HWRaspberryPI2_InputLevelChanged;
         }

         //Create the Background Task
         TaskFactory tTaskFactory = new TaskFactory(TaskScheduler.Current);

         sWatch = new Stopwatch();
         sWatch.Start();

         await tTaskFactory.StartNew(new Action(ControllerTask), TaskCreationOptions.RunContinuationsAsynchronously);
      }

      private void HWRaspberryPI2_InputLevelChanged(object sender, HWRaspberryPI_INPUT.EventArgsINPUT e)
      {
         //this.ProcessCommandRecieved();
      }

      public override void Connect()
      {
         //Setup the GPIO, PWM and I2C drivers
         if (LightningProvider.IsLightningEnabled)
         {
            LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            //Setup the I2C bus for access to the PWM channels
            i2cSettings = new I2cConnectionSettings(0x40);
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            i2cSettings.SharingMode = I2cSharingMode.Exclusive;

            OnConnect();
         }
      }

      private void ControllerTask()
      {
         while (sWatch.IsRunning == true)
         {
            TriggerTime = sWatch.ElapsedMilliseconds;

            if (TriggerTime >= 1)
            {
               //System.Diagnostics.Debug.WriteLine(TriggerTime.ToString());

               sWatch.Restart();
               
               foreach (HWRaspberryPI_INPUT c in lINPUTs)
               {
                  c.Tick();
               }

               foreach (HWRaspberryPI_PWM c in lPWMs)
               {
                  if (c.Function != Func_PWM.tenFUNCTION.FUNC_OFF)
                  {
                     //Call the PWM channels 'tick' function
                     c.Tick();

                     i2cDevice.Write(new byte[2] { (byte)(LED_ON_L[0] + ((byte)c.Channel * 4)), 0x00 });
                     i2cDevice.Write(new byte[2] { (byte)(LED_ON_H[0] + ((byte)c.Channel * 4)), 0x00 });
                     i2cDevice.Write(new byte[2] { (byte)(LED_OFF_L[0] + ((byte)c.Channel * 4)), (byte)(c.Level & 0xFF) });
                     i2cDevice.Write(new byte[2] { (byte)(LED_OFF_H[0] + ((byte)c.Channel * 4)), (byte)((c.Level >> 8) & 0xFF) });
                  }
               }
            }
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
         throw new NotImplementedException();
      }

      public override void FireCommand(string cmd)
      {
         Command function;
         Command subFunction;
         char[] decodedData = new char[20];
         uint index;
         uint value = 0;

         DecodeCommand(cmd.ToList<char>(), out function, out subFunction, ref decodedData);

         switch (function.Value)
         {
            case 'I': //INPUT
               break;
            case 'R': //RELAY
               index = UInt32.Parse(decodedData[0].ToString());

               switch (subFunction.Value)
               {
                  case 'S':
                     new string(decodedData).Remove(0, 2).ToCharArray().CopyTo(decodedData, 0);

                     for (uint i = 0; i < Inputs; i++)
                     {
                        //if(index == lRelays.Channel)
                        {
                           value = UInt32.Parse(decodedData[0].ToString());
                        }
                     }
                     break;
                  case 'G':
                     break;
                  default:
                     break;
               }
               break;
            case 'T': //PWM
               index = UInt32.Parse(decodedData[0].ToString());

               foreach(HWRaspberryPI_PWM c in lPWMs)
               {
                  if(index == c.Channel)
                  {
                     //Remove the Function and Index from the string
                     new string(decodedData).Remove(0, 2).ToCharArray().CopyTo(decodedData, 0);

                     switch (subFunction.Value)
                     {
                        case 'S':
                           c.Level = UInt32.Parse(new string(decodedData));
                           break;
                        case 'G':
                           break;
                        case 'F':
                           c.Function = (Func_PWM.tenFUNCTION)UInt32.Parse(new string(decodedData));
                           break;
                        case 'M':
                           c.MaxLevel = UInt32.Parse(new string(decodedData));
                           break;
                        case 'R':
                           c.UpdateCount = UInt32.Parse(new string(decodedData));
                           break;
                        default:
                           break;
                     }

                     c.Tick();
                     return;
                  }
               }
               
               break;
            case 'A': //ADC
               break;
            case 'C':
               break;
            default:
               break;
         }

      }


      public override bool ProcessCommandRecieved(List<char> data)
      {
         throw new NotImplementedException();
      }
   }
}
