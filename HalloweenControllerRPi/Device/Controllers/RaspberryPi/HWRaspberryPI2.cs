using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats;
using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.ExternalDisplay;
using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Devices.Spi;
using Windows.System.Threading;
using Windows.UI.Xaml;

namespace HalloweenControllerRPi.Device.Controllers
{
   internal class HWRaspberryPI2 : HWController
   {
      #region /* CONSTANTS */

      private const int MaxI2CAddresses = 128;

      #endregion /* ENUMS */

      #region /* PRIVATE */
      private static I2cDevice _i2cDevice;
      private static I2cConnectionSettings _i2cSettings;

      private static SpiDevice _spiDevice;

      //private static Stopwatch sWatch;
      private static DispatcherTimer CycleTimer;

      private static List<IHat> _lHats = new List<IHat>();
      private static List<IChannel> _lAllFunctions = new List<IChannel>();
      private static List<IChannel> _lPWMFunctions = new List<IChannel>();
      private static List<IChannel> _lRELAYFunctions = new List<IChannel>();
      private static List<IChannel> _lINPUTFunctions = new List<IChannel>();
      private static List<IChannel> _lSOUNDFunctions = new List<IChannel>();
      #endregion /* PRIVATE */

      #region /* CONSTRUCTORS */
      public HWRaspberryPI2()
      {
      }
      #endregion /* CONSTRUCTORS */

      #region /* HW Bus Devices */
      public I2cDevice I2CBusDevice
      {
         get { return _i2cDevice; }
         protected set { _i2cDevice = value; }
      }

      public SpiDevice SPIBusDevice
      {
         get { return _spiDevice; }
         protected set { _spiDevice = value; }
      }
      #endregion

      #region /* COMMAND LIST & HANDLING */
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
         get { return _Commands; }
      }
      #endregion /* COMMAND LIST & HANDLING */

      #region /* AVAILABLE FUNCTIONS */

      public override uint Inputs
      {
         get
         {
            return (uint)_lINPUTFunctions.Count;
         }
      }

      public override uint PWMs
      {
         get
         {
            return (uint)_lPWMFunctions.Count;
         }
      }

      public override uint Relays
      {
         get
         {
            return (uint)_lRELAYFunctions.Count;
         }
      }

      public override uint SoundChannels
      {
         get
         {
            return (uint)_lSOUNDFunctions.Count;
         }
      }

      public override bool HasDisplay
      {
         get
         {
            return (Display != null);
         }
      }
      #endregion /* AVAILABLE FUNCTIONS */

      private void PopulateChannelList()
      {
         foreach (IChannel c in _lAllFunctions)
         {
            if (c is ChannelFunction_PWM)
            {
               _lPWMFunctions.Add(c);
            }
            else if (c is ChannelFunction_INPUT)
            {
               _lINPUTFunctions.Add(c);
            }
            else if (c is ChannelFunction_RELAY)
            {
               _lRELAYFunctions.Add(c);
            }
            else if (c is ChannelFunction_SOUND)
            {
               _lSOUNDFunctions.Add(c);
            }
         }
      }

      /// <summary>
      /// Task which calls the CHANNEL UPDATE for each of the discovered channels
      /// </summary>
      private void ControllerTask(object sender, object e)
      {
         //System.Diagnostics.Debug.WriteLine("Cyclic Trigger - " + sWatch.ElapsedMilliseconds.ToString());
         //sWatch.Restart();

         foreach (IHat hat in _lHats)
         {
            hat.HatTask();
         }
      }

      /// <summary>
      ///
      /// </summary>
      /// <returns></returns>
      private async Task DiscoverHats()
      {
         DeviceInformationCollection i2cDeviceControllers = null;
         I2cController i2cController = null;
         RPiHat rpiHat;
         int Address = 0x00;


         //RPUGLIESE - LightningI2cProvider is not working (Doesn't like the Address of 0x00)!
         if (LightningProvider.IsLightningEnabled == true)
         {
            LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            i2cController = await I2cController.GetDefaultAsync();

            //i2cController = (await I2cController.GetControllersAsync(LightningI2cProvider.GetI2cProvider()))[0];
         }
         else
         {
            string deviceSelector = I2cDevice.GetDeviceSelector("I2C1");
            i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector).AsTask();

            if (i2cDeviceControllers == null)
            {
               throw new Exception("Device not found (" + deviceSelector + ")");
            }
         }

         _i2cSettings = new I2cConnectionSettings(RPiHat.DisplayHatAddress);
         _i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
         _i2cSettings.SharingMode = I2cSharingMode.Shared;

         if (LightningProvider.IsLightningEnabled == true)
            _i2cDevice = i2cController.GetDevice(_i2cSettings);
         else
            _i2cDevice = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, _i2cSettings);

         if (_i2cDevice.ReadPartial(new byte[1] { 0x00 }).Status != I2cTransferStatus.SlaveAddressNotAcknowledged)
         {
            rpiHat = RPiHat.Open(this, RPiHat.DisplayHatAddress);

            if (rpiHat != null)
            {
               /* There can ONLY be 1 DISPLAY channel */
               if (rpiHat is IDisplayChannel)
               {
                  Display = new GraphicsProvider((rpiHat as IDisplayChannel).Device);

                  OnDisplayInitialised();
               }

               _lHats.Add(rpiHat);

               /* Store a collection of all the available Channels */
               _lAllFunctions.AddRange(_lHats.Last().Channels);
            }
         }

         //Find all other HATs
         while (Address < MaxI2CAddresses)
         {
            _i2cSettings.SlaveAddress = Address;

            if (LightningProvider.IsLightningEnabled == true)
               _i2cDevice = i2cController.GetDevice(_i2cSettings);
            else
               _i2cDevice = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, _i2cSettings);
            
            if (_i2cDevice.ReadPartial(new byte[1] { 0x00 }).Status != I2cTransferStatus.SlaveAddressNotAcknowledged)
            {
               /* Device found, store the HAT and it's Address then establish communication with the HAT and initialise the HATs available CHANNELS */
               rpiHat = RPiHat.Open(this, (UInt16)Address);

               if (rpiHat != null)
               {
                  //System.Diagnostics.Debug.WriteLine(Address.ToString("x") + " - Device found (" + rpiHat.HatType.ToString() + ").");

                  _lHats.Add(rpiHat);

                  /* Store a collection of all the available Channels */
                  _lAllFunctions.AddRange(_lHats.Last().Channels);
               }
            }

            if (++Address == RPiHat.DisplayHatAddress)
               Address++;

            OnDiscoveryProgressUpdated((uint)((double)(Address + 1) / (double)MaxI2CAddresses * 100));
         }

         await Task.Delay(500);

         /* Initialise available channels (PWM, RELAY, INPUT) */
         PopulateChannelList();

         OnControllerInitialised();

         //sWatch = new Stopwatch();
         //sWatch.Start();

         /* Create the Background Task handle */
         CycleTimer = new DispatcherTimer();
         CycleTimer.Tick += ControllerTask;
         CycleTimer.Interval = new TimeSpan(0, 0, 0, 0, 1); //Unlikely to run this fast, but trigger as fast as possible
         CycleTimer.Start();
      }


      /// <summary>
      /// Initialise any drivers (ie. I2C)
      /// </summary>
      public override void Connect()
      {
         /* Discover 'HATs' that are connected */
         DiscoverHats();
      }

      public override void Disconnect()
      {
         throw new NotImplementedException();
      }
      
      /// <summary>
      ///
      /// </summary>
      /// <param name="cmd"></param>
      public override void TransmitCommand(string cmd)
      {
         Command function;
         Command subFunction;
         char[] decodedData = new char[cmd.Length];
         uint channel;

         /* Decode the received COMMAND */
         DecodeCommand(cmd, out function, out subFunction, ref decodedData);

         /* Check if the received COMMAND is supported */
         if (GetFunctionCommand(function.Key) != null)
         {
            channel = GetChannelIndex(String.Join(null, decodedData));

            if ((channel <= _lAllFunctions.Count) && (channel != 0))
            {
               switch (function.Value)
               {
                  #region /* INPUT HANDLING */
                  case 'I':
                     ChannelFunction_INPUT cINPUT = (_lINPUTFunctions[(int)channel - 1] as ChannelFunction_INPUT);

                     if (cINPUT != null)
                     {
                        uint value = GetValue(String.Join(null, decodedData));

                        switch (subFunction.Value)
                        {
                           case 'D':
                              cINPUT.DebounceTime = TimeSpan.FromMilliseconds((double)value);
                              break;

                           case 'P':
                              cINPUT.PostTriggerTime = TimeSpan.FromMilliseconds((double)value);
                              break;

                           default:
                              break;
                        }

                        return;
                     }
                     break;
                  #endregion /* INPUT HANDLING */

                  #region /* RELAY HANDLING */
                  case 'R':
                     ChannelFunction_RELAY cRELAY = (_lRELAYFunctions[(int)channel - 1] as ChannelFunction_RELAY);

                     if (cRELAY != null)
                     {
                        switch (subFunction.Value)
                        {
                           case 'S':
                              cRELAY.Level = UInt32.Parse(new string(decodedData));
                              cRELAY.ChannelHost.UpdateChannel(cRELAY);
                              break;

                           case 'G':
                              break;

                           default:
                              break;
                        }

                        return;
                     }
                     break;
                  #endregion /* RELAY HANDLING */

                  #region /* PWM HANDLING */
                  case 'T':
                     ChannelFunction_PWM cPWM = (_lPWMFunctions[(int)channel - 1] as ChannelFunction_PWM);

                     if (cPWM != null)
                     {
                        uint value = GetValue(String.Join(null, decodedData));

                        switch (subFunction.Value)
                        {
                           case 'S':
                              cPWM.Level = value;
                              cPWM.ChannelHost.UpdateChannel(cPWM);
                              break;

                           case 'A':
                              cPWM.RampRate = value;
                              break;

                           case 'G':
                              break;

                           case 'F':
                              cPWM.Function = (PWMFunctions)value;
                              break;

                           case 'N':
                              cPWM.MinLevel = value;
                              break;

                           case 'M':
                              cPWM.MaxLevel = value;
                              break;

                           case 'R':
                              cPWM.UpdateCount = value;
                              break;

                           case 'D':
                              foreach(uint val in GetValues(String.Join(null, decodedData)))
                              {
                                 cPWM.CustomLevel.Add(val);
                              }
                              break;

                           default:
                              break;
                        }

                        return;
                     }
                     break;
                  #endregion /* PWM HANDLING */

                  #region /* SOUND HANDLING */
                  case 'S':
                     ChannelFunction_SOUND cSOUND = (_lSOUNDFunctions[(int)channel - 1] as ChannelFunction_SOUND);

                     if (cSOUND != null)
                     {
                        uint value = GetValue(String.Join(null, decodedData));

                        switch (subFunction.Value)
                        {
                           case 'P':
                              cSOUND.Play();
                              break;

                           case 'S':
                              cSOUND.Stop();
                              break;

                           case 'T':
                              cSOUND.Track = (byte)value;
                              break;

                           case 'V':
                              cSOUND.Volume = (byte)value;
                              break;

                           case 'L':
                              cSOUND.Loop = (value != 0 ? true : false);
                              break;

                           case 'A':
                              TransmitCommand(new CommandEventArgs(function.Value, subFunction.Value, channel, cSOUND.AvailableTracks));
                              break;

                           default:
                              break;
                        }

                        return;
                     }
                     break;
                  #endregion /* SOUND HANDLING */

                  default:
                     break;
               }
            }
         }
         else
         {
            /* COMMAND not supported */
         }
      }

      public override void OnChannelNotification(object sender, CommandEventArgs e)
      {
         TransmitCommand(e);
      }
   }
}