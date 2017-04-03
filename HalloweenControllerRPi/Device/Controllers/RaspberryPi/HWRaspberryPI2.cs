using HalloweenControllerRPi.Device.Controllers.RaspberryPi;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats;
using HalloweenControllerRPi.Functions;
using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.UI.Xaml;
using static HalloweenControllerRPi.Device.Controllers.RaspberryPi.ChannelFunction_INPUT;
using static HalloweenControllerRPi.Functions.Func_RELAY;

namespace HalloweenControllerRPi.Device.Controllers
{
   internal class HWRaspberryPI2 : HWController
   {
      #region /* ENUMS */

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

      private const int MaxI2CAddresses = 128;

      #endregion /* ENUMS */

      #region /* PRIVATE */

      private static I2cDevice i2cDevice;
      private static I2cController i2cController;
      private static I2cConnectionSettings i2cSettings;

      private static GpioController gpioController;

      //private static Stopwatch sWatch;
      //private static long TriggerTime;

      private static List<IHat> lHats = new List<IHat>();
      private static UInt16 m_PWMs = 0;
      private static UInt16 m_Inputs = 0;
      private static UInt16 m_Relays = 0;
      private static List<IChannel> lAllFunctions = new List<IChannel>();

      #endregion /* PRIVATE */

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

      #endregion /* HW MAPPING */

      #region /* CONSTRUCTORS */

      public HWRaspberryPI2()
      {
         if (LightningProvider.IsLightningEnabled == true)
         {
            LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
         }

         var getCtrlTask = Task.Run(async () => { await GetControllers(); });
      }

      #endregion /* CONSTRUCTORS */

      private async Task GetControllers()
      {
         if (LightningProvider.IsLightningEnabled == true)
         {
            gpioController = (await GpioController.GetControllersAsync(LightningGpioProvider.GetGpioProvider()))[0];
            i2cController = (await I2cController.GetControllersAsync(LightningI2cProvider.GetI2cProvider()))[0];
         }
      }

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
               new Command("RATE", 'R')
            }
         }
      };

      /// <summary>
      /// Dictionary containing available Functions and Sub-Functions.
      /// </summary>
      public override Dictionary<Command, List<Command>> Commands
      {
         get { return _Commands; }
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

      #endregion /* COMMAND LIST & HANDLING */

      #region /* AVAILABLE FUNCTIONS */

      public override uint Inputs
      {
         get
         {
            return m_Inputs;
         }
      }

      public override uint PWMs
      {
         get
         {
            return m_PWMs;
         }
      }

      public override uint Relays
      {
         get
         {
            return m_Relays;
         }
      }

      #endregion /* AVAILABLE FUNCTIONS */

      /// <summary>
      ///
      /// </summary>
      /// <returns></returns>
      private async Task OnConnect()
      {
         /* Allow the HW to initialise */
         await Task.Delay(500);

         /* Discover 'HATs' that are connected */
         if (LightningProvider.IsLightningEnabled == true)
         {
            i2cDevice = i2cController.GetDevice(i2cSettings);
         }
         else
         {
            await DiscoverHats();
         }

         /* Initialise available channels (PWM, RELAY, INPUT) */
         foreach (IChannel c in lAllFunctions)
         {
            if (c is ChannelFunction_PWM)
            {
               m_PWMs++;
            }
            else if (c is ChannelFunction_INPUT)
            {
               m_Inputs++;
            }
            else if (c is ChannelFunction_RELAY)
            {
               m_Relays++;
            }
         }

         OnControllerInitialised();

         //sWatch = new Stopwatch();
         //sWatch.Start();

         /* Create the Background Task handle */
         DispatcherTimer dispatcher = new DispatcherTimer();
         dispatcher.Tick += ControllerTask;
         dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);
         dispatcher.Start();
      }

      /// <summary>
      /// Task which calls the CHANNEL UPDATE for each of the discovered channels
      /// </summary>
      private void ControllerTask(object sender, object e)
      {
         //System.Diagnostics.Debug.WriteLine("Cyclic Trigger - " + sWatch.ElapsedMilliseconds.ToString());

         //sWatch.Restart();

         foreach (IChannel c in lAllFunctions)
         {
            if ((c as IProcessTick) != null)
            {
               (c as IProcessTick).Tick();

               UpdateChannel(c);
            }
         }
      }

      /// <summary>
      ///
      /// </summary>
      /// <returns></returns>
      private async Task DiscoverHats()
      {
         RPiHat rpiHat;
         string deviceSelector = I2cDevice.GetDeviceSelector("I2C1");
         var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector).AsTask();

         if (i2cDeviceControllers == null)
         {
            throw new Exception("Device not found (" + deviceSelector + ")");
         }

         int Address = 0x00;

         while (Address < MaxI2CAddresses)
         {
            i2cSettings = new I2cConnectionSettings(Address);
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            i2cSettings.SharingMode = I2cSharingMode.Exclusive;

            i2cDevice = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);

            OnDiscoveryProgressUpdated((uint)((double)Address / (double)MaxI2CAddresses * 100));

            if (i2cDevice.ReadPartial(new byte[1] { 0x00 }).Status == I2cTransferStatus.SlaveAddressNotAcknowledged)
            {
               System.Diagnostics.Debug.WriteLine(Address.ToString("x") + " - No device found.");

               /* No device found */
               Address++;

               continue;
            }

            /* Device found, store the HAT and it's Address then establish communication with the HAT and initialise the HATs available CHANNELS */
            rpiHat = RPiHat.Open(this, i2cDevice, (UInt16)Address);

            if (rpiHat != null)
            {
               System.Diagnostics.Debug.WriteLine(Address.ToString("x") + " - Device found (" + rpiHat.HatType.ToString() + ").");

               lHats.Add(rpiHat);

               /* Store a collection of all the available Channels */
               lAllFunctions.AddRange(lHats.Last().Channels);
            }

            Address++;
         }

         OnDiscoveryProgressUpdated(100);
      }

      /// <summary>
      /// Initialise any drivers (ie. I2C)
      /// </summary>
      public override void Connect()
      {
         /* Setup the I2C bus for access to the PWM channels */
         i2cSettings = new I2cConnectionSettings(0x40);
         i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
         i2cSettings.SharingMode = I2cSharingMode.Exclusive;

         /* Wait for the 'OnConnect' to complete without blocking the UI */
         OnConnect();
      }

      public override void Disconnect()
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Updates am available CHANNEL (ie. Read/Write to the HAT)
      /// </summary>
      /// <param name="chan"></param>
      private void UpdateChannel(IChannel chan)
      {
         chan.HostHat.UpdateChannel(chan);
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="cmd"></param>
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
                  foreach (IChannel chan in lAllFunctions)
                  {
                     ChannelFunction_INPUT c = (chan as ChannelFunction_INPUT);

                     if (c != null)
                     {
                        if (channel == chan.Index + 1)
                        {
                           /* Remove the Function and Channel from the string */
                           new string(decodedData).Remove(0, 2).ToCharArray().CopyTo(decodedData, 0);

                           switch (subFunction.Value)
                           {
                              case 'D':
                                 c.DebounceTime = TimeSpan.FromMilliseconds((double)UInt32.Parse(new string(decodedData)));
                                 break;

                              case 'P':
                                 c.PostTriggerTime = TimeSpan.FromMilliseconds((double)UInt32.Parse(new string(decodedData)));
                                 break;

                              default:
                                 break;
                           }

                           return;
                        }
                     }
                  }
                  break;

               #endregion /* INPUT HANDLING */

               #region /* RELAY HANDLING */

               case 'R':
                  foreach (IChannel chan in lAllFunctions)
                  {
                     ChannelFunction_RELAY c = (chan as ChannelFunction_RELAY);

                     if (c != null)
                     {
                        if (channel == chan.Index + 1)
                        {
                           /* Remove the Function and Channel from the string */
                           new string(decodedData).Remove(0, 2).ToCharArray().CopyTo(decodedData, 0);

                           switch (subFunction.Value)
                           {
                              case 'S':
                                 c.Level = UInt32.Parse(new string(decodedData));
                                 UpdateChannel(c);
                                 break;

                              case 'G':
                                 break;

                              default:
                                 break;
                           }

                           return;
                        }
                     }
                  }
                  break;

               #endregion /* RELAY HANDLING */

               #region /* PWM HANDLING */

               case 'T':
                  foreach (IChannel chan in lAllFunctions)
                  {
                     ChannelFunction_PWM c = (chan as ChannelFunction_PWM);

                     if (c != null)
                     {
                        if (channel == chan.Index + 1)
                        {
                           /* Remove the Function and Channel from the string */
                           new string(decodedData).Remove(0, 2).ToCharArray().CopyTo(decodedData, 0);

                           switch (subFunction.Value)
                           {
                              case 'S':
                                 c.Level = UInt32.Parse(new string(decodedData));
                                 UpdateChannel(c);
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

                           return;
                        }
                     }
                  }
                  break;

               #endregion /* PWM HANDLING */

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