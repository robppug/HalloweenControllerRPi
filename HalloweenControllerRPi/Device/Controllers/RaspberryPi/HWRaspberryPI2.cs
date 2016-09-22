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
using static HalloweenControllerRPi.Device.Controllers.RaspberryPi.HWRaspberryPI_INPUT;
using static HalloweenControllerRPi.Functions.Func_RELAY;

namespace HalloweenControllerRPi.Device.Controllers
{
   class HWRaspberryPI2 : HWInterface
   {
      #region /* ENUMS */
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
         INPUT_PIN_12 = 12,
         INPUT_PIN_16 = 16,
         INPUT_PIN_19 = 19,
         INPUT_PIN_20 = 20,
         INPUT_PIN_21 = 21,
         INPUT_PIN_26 = 26
      };

      public enum tenOutputPins
      {
         OUTPUT_PIN_18 = 18,
         OUTPUT_PIN_23 = 23,
         OUTPUT_PIN_24 = 24,
         OUTPUT_PIN_25 = 25
      };
      #endregion

      #region /* PRIVATE */
      private static I2cDevice i2cDevice;
      private static I2cController i2cController;
      private static I2cConnectionSettings i2cSettings;

      private static GpioController gpioController;

      private static Stopwatch sWatch;
      private static long TriggerTime;

      private static List<HWRaspberryPI_PWM> lPWMs = new List<HWRaspberryPI_PWM>();
      private static List<HWRaspberryPI_INPUT> lINPUTs = new List<HWRaspberryPI_INPUT>();
      private static List<HWRaspberryPI_RELAY> lRELAYs = new List<HWRaspberryPI_RELAY>();
      private static List<IFunctionHandler> lAllFunctions = new List<IFunctionHandler>();

      private static byte[] bMODE1 = new byte[1] { 0x00 };
      private static byte[] bMODE2 = new byte[1] { 0x01 };
      private static byte[] LED_ON_L = new byte[1] { 0x06 };
      private static byte[] LED_ON_H = new byte[1] { 0x07 };
      private static byte[] LED_OFF_L = new byte[1] { 0x08 };
      private static byte[] LED_OFF_H = new byte[1] { 0x09 };
      #endregion

      #region /* HW MAPPING */
      public class InputMap
      {
         public uint Index;
         public tenInputPins Pin;

         public InputMap(uint i, tenInputPins pin)
         {
            Index = i;
            Pin = pin;
         }
      };
      public class OutputMap
      {
         public uint Index;
         public tenOutputPins Pin;

         public OutputMap(uint i, tenOutputPins pin)
         {
            Index = i;
            Pin = pin;
         }
      };
      private static List<InputMap> lInputMap = new List<InputMap>()
      {
         new InputMap(1, tenInputPins.INPUT_PIN_12),
         new InputMap(2, tenInputPins.INPUT_PIN_16),
         new InputMap(3, tenInputPins.INPUT_PIN_20),
         new InputMap(4, tenInputPins.INPUT_PIN_21),
         new InputMap(5, tenInputPins.INPUT_PIN_19),
         new InputMap(6, tenInputPins.INPUT_PIN_26)
      };

      private static List<OutputMap> lOutputMap = new List<OutputMap>()
      {
         new OutputMap(1, tenOutputPins.OUTPUT_PIN_18),
         new OutputMap(2, tenOutputPins.OUTPUT_PIN_23),
         new OutputMap(3, tenOutputPins.OUTPUT_PIN_24),
         new OutputMap(4, tenOutputPins.OUTPUT_PIN_25)
      };
      #endregion

      #region /* CONSTRUCTORS */
      public HWRaspberryPI2()
      {
         if (LightningProvider.IsLightningEnabled == true)
         {
            LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            GetControllers();
         }
         else
         {
            //throw new Exception("No supported devices found.");
         }
      }
      #endregion

      #region /* COMMAND LIST & HANDLING */
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
               new Command("MINLEVEL", 'N'),
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
         get{ return _Commands; }
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
      #endregion

      #region /* AVAILABLE FUNCTIONS */
      public override uint Inputs
      {
         get
         {
            return (uint)lInputMap.Count;
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
            return (uint)lOutputMap.Count;
         }
      }
      #endregion

      private async void GetControllers()
      {
         gpioController = (await GpioController.GetControllersAsync(LightningGpioProvider.GetGpioProvider()))[0];

         i2cController = (await I2cController.GetControllersAsync(LightningI2cProvider.GetI2cProvider()))[0];
      }

      private async void OnConnect()
      {
         i2cDevice = i2cController.GetDevice(i2cSettings);

         byte[] buffer = new byte[10];

         /* Change to NORMAL mode */
         i2cDevice.Write(new byte[2] { 0x00, 0x00 });

         await Task.Delay(1);

         /* Initialise PWM channels */
         for (uint i = 0; i < PWMs; i++)
         {
            lPWMs.Add(new HWRaspberryPI_PWM(i));

            i2cDevice.Write(new byte[2] { (byte)(LED_ON_L[0] + ((byte)lPWMs[(int)i].Channel * 4)), 0x00 });
            i2cDevice.Write(new byte[2] { (byte)(LED_ON_H[0] + ((byte)lPWMs[(int)i].Channel * 4)), 0x00 });
            i2cDevice.Write(new byte[2] { (byte)(LED_OFF_L[0] + ((byte)lPWMs[(int)i].Channel * 4)), 0x00 });
            i2cDevice.Write(new byte[2] { (byte)(LED_OFF_H[0] + ((byte)lPWMs[(int)i].Channel * 4)), 0x00 });
         }

         /* Initialise INPUT channels */
         for (uint i = 0; i < Inputs; i++)
         {
            HWRaspberryPI_INPUT piInput;
            GpioPin pin = gpioController.OpenPin((int)lInputMap[(int)i].Pin);

            if (pin != null)
            {
               GpioPinDriveMode gpioDriveMode;

               gpioDriveMode = GpioPinDriveMode.InputPullUp;
               if (pin.IsDriveModeSupported(gpioDriveMode) == true)
               {
                  pin.SetDriveMode(gpioDriveMode);
               }

               piInput = new HWRaspberryPI_INPUT(i, pin);
               piInput.InputLevelChanged += HWRaspberryPI2_InputLevelChanged;

               lINPUTs.Add(piInput);
            }
         }

         /* Initialise RELAY channels */
         for (uint i = 0; i < Relays; i++)
         {
            HWRaspberryPI_RELAY piRelay;
            GpioPin pin = gpioController.OpenPin((int)lOutputMap[(int)i].Pin);

            if (pin != null)
            {
               GpioPinDriveMode gpioDriveMode;

               gpioDriveMode = GpioPinDriveMode.Output;
               if (pin.IsDriveModeSupported(gpioDriveMode) == true)
               {
                  pin.Write(GpioPinValue.High);
                  pin.SetDriveMode(gpioDriveMode);
               }

               piRelay = new HWRaspberryPI_RELAY(i, pin);

               lRELAYs.Add(piRelay);
            }
         }

         lAllFunctions.AddRange(lINPUTs);
         lAllFunctions.AddRange(lPWMs);
         lAllFunctions.AddRange(lRELAYs);

         /* Create the Background Task handle */
         TaskFactory tTaskFactory = new TaskFactory(TaskScheduler.Current);

         sWatch = new Stopwatch();
         sWatch.Start();

         await tTaskFactory.StartNew(new Action(ControllerTask), TaskCreationOptions.RunContinuationsAsynchronously);
      }


      public override void Connect()
      {
         /* Initialise any drivers (ie. I2C) */
         if (LightningProvider.IsLightningEnabled)
         {
            LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            /* Setup the I2C bus for access to the PWM channels */
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
               
               foreach (IFunctionHandler c in lAllFunctions)
               {
                  if ((c as IProcessTick) != null)
                  {
                     (c as IProcessTick).Tick();

                     if ((c as HWRaspberryPI_PWM) != null)
                     {
                        HWRaspberryPI_PWM pwm = (c as HWRaspberryPI_PWM);

                        if (pwm.Function != Func_PWM.tenFUNCTION.FUNC_OFF)
                        {
                           i2cDevice.Write(new byte[2] { (byte)(LED_ON_L[0] + ((byte)pwm.Channel * 4)), 0x00 });
                           i2cDevice.Write(new byte[2] { (byte)(LED_ON_H[0] + ((byte)pwm.Channel * 4)), 0x00 });
                           i2cDevice.Write(new byte[2] { (byte)(LED_OFF_L[0] + ((byte)pwm.Channel * 4)), (byte)(pwm.Level & 0xFF) });
                           i2cDevice.Write(new byte[2] { (byte)(LED_OFF_H[0] + ((byte)pwm.Channel * 4)), (byte)((pwm.Level >> 8) & 0xFF) });
                        }
                        else
                        {
                           i2cDevice.Write(new byte[2] { (byte)(LED_ON_L[0] + ((byte)pwm.Channel * 4)), 0x00 });
                           i2cDevice.Write(new byte[2] { (byte)(LED_ON_H[0] + ((byte)pwm.Channel * 4)), 0x00 });
                           i2cDevice.Write(new byte[2] { (byte)(LED_OFF_L[0] + ((byte)pwm.Channel * 4)), 0x00 });
                           i2cDevice.Write(new byte[2] { (byte)(LED_OFF_H[0] + ((byte)pwm.Channel * 4)), 0x00 });
                        }
                     }
                  }
               }
            }
         }
      }

      private void HWRaspberryPI2_InputLevelChanged(object sender, EventArgsINPUT e)
      {
         TriggerCommandReceived(new CommandEventArgs('I', e.Index + 1, (uint)e.TriggerLevel));
      }

      public override void Disconnect()
      {
         throw new NotImplementedException();
      }

      public override void TransmitCommand(string cmd)
      {
         Command function;
         Command subFunction;
         char[] decodedData = new char[20];
         uint channel;

         /* Decode the received COMMAND */
         DecodeCommand(cmd.ToList<char>(), out function, out subFunction, ref decodedData);

         /* Check if the received COMMAND is supported */
         if (this.GetFunctionCommand(function.Key) != null)
         {
            /* The the CHANNEL of the request */
            channel = UInt32.Parse(new string(decodedData).Substring(0, 2));

            switch (function.Value)
            {
               #region /* INPUT HANDLING */
               case 'I': 
                  foreach (HWRaspberryPI_INPUT c in lINPUTs)
                  {
                     if (channel == c.Channel + 1)
                     {
                        /* Remove the Function and Channel from the string */
                        new string(decodedData).Remove(0, 2).ToCharArray().CopyTo(decodedData, 0);

                        switch (subFunction.Value)
                        {
                           case 'D':
                              c.DebounceTime = TimeSpan.FromMilliseconds((double)UInt32.Parse(new string(decodedData)));
                              break;
                           default:
                              break;
                        }
                     }
                  }
                  break;
               #endregion

               #region /* RELAY HANDLING */
               case 'R': 
                  foreach (HWRaspberryPI_RELAY c in lRELAYs)
                  {
                     if (channel == c.Channel + 1)
                     {
                        /* Remove the Function and Channel from the string */
                        new string(decodedData).Remove(0, 2).ToCharArray().CopyTo(decodedData, 0);

                        switch (subFunction.Value)
                        {
                           case 'S':
                              c.OutputLevel = (tenOutputLevel)UInt32.Parse(new string(decodedData));
                              break;
                           case 'G':
                              break;
                           default:
                              break;
                        }
                     }
                  }
                  break;
               #endregion

               #region /* PWM HANDLING */
               case 'T':
                  foreach (HWRaspberryPI_PWM c in lPWMs)
                  {
                     if (channel == c.Channel + 1)
                     {
                        /* Remove the Function and Channel from the string */
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
                           case 'N':
                              c.MinLevel = UInt32.Parse(new string(decodedData));
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
               #endregion

               case 'A': /* ADC */
                  break;
               case 'C':
                  break;
               default:
                  break;
            }
         }
         else
         {
            /* COMMAND not supported */
         }
      }
   }
}
