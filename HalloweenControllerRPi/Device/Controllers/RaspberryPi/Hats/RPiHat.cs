using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   /// <summary>
   /// Raspberry Pi HAT class for the Raspberry Pi 2/3
   /// </summary>
   public class RPiHat : IHat
   {
      #region /* ENUMS */

      public enum SupportedHATs : byte
      {
         MOSFET_v1 = 0,
         RELAY_v1,
         RELAY_EEPROM_v1,
         INPUT_v1,
         INPUT_EEPROM_v1,
         NoOfSupportedHATs
      };

      #endregion /* ENUMS */

      #region Declarations

      private IHatInterface m_HatInterface;
      private IHWController m_hostController;

      public IHWController HostController
      {
         get { return m_hostController; }
         private set { m_hostController = value; }
      }

      public List<IChannel> Channels
      {
         get;
         protected set;
      }

      public SupportedHATs HatType
      {
         get;
         protected set;
      }
      #endregion Declarations

      private RPiHat(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress)
      {
         HostController = host;

         II2CBusDevice busDevice = null;

         HatType = GetHatType(hatAddress);

         if (HatType != SupportedHATs.NoOfSupportedHATs)
         {
            switch (HatType)
            {
               case SupportedHATs.MOSFET_v1:
                  busDevice = new BusDevice_PCA9685();
                  break;

               case SupportedHATs.INPUT_v1:
                  busDevice = new BusDevice_PCA9501();
                  break;

               case SupportedHATs.RELAY_v1:
                  busDevice = new BusDevice_PCA9501();
                  break;

               case SupportedHATs.NoOfSupportedHATs:
               default:
                  break;
            }

            if (busDevice != null)
            {
               OpenHat(i2cDevice, hatAddress, busDevice);
            }
         }
      }

      /// <summary>
      /// RPiHat object if the HAT is supported and successfully initialised.
      /// </summary>
      /// <param name="i2cDevice"></param>
      /// <param name="hatAddress"></param>
      /// <returns>If successful an initialised RPiHat object otherwise null.</returns>
      public static RPiHat Open(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress)
      {
         RPiHat rpiHat = new RPiHat(host, i2cDevice, hatAddress);

         if (rpiHat.HatType == SupportedHATs.NoOfSupportedHATs)
         {
            rpiHat = null;
         }

         return rpiHat;
      }

      public void Close()
      {
      }

      /// <summary>
      /// TASK execution call for HAT channel processing.
      /// </summary>
      public void ProcessTask()
      {
         /* Process each CHANNEL available on this HAT */
         foreach (IChannel c in Channels)
         {
            if ((c as IProcessTick) != null)
            {
               (c as IProcessTick).Tick();
            }
            UpdateChannel(c);
         }

         
      }

      /// <summary>
      /// Returns the type of HAT at the Address provided
      /// </summary>
      /// <param name="hatAddress"></param>
      /// <returns></returns>
      private SupportedHATs GetHatType(ushort hatAddress)
      {
         SupportedHATs hat = SupportedHATs.NoOfSupportedHATs;

         /* PCA9501 - INPUT with EEPROM (0x40 - 0x4F is EEPROM) */
         if ((hatAddress >= 0x00) && (hatAddress <= 0x0F))
         {
            hat = SupportedHATs.INPUT_v1;
         }
         /* PCA9501 - RELAY with EEPROM (0x50 - 0x5F is EEPROM)*/
         else if ((hatAddress >= 0x10) && (hatAddress <= 0x1F))
         {
            hat = SupportedHATs.RELAY_v1;
         }
         /* PCA9685 - PWM Driver */
         else if ((hatAddress >= 0x60) && (hatAddress <= 0x6F))
         {
            hat = SupportedHATs.MOSFET_v1;
         }

         return hat;
      }

      /// <summary>
      /// Open the HATs interface and populates the list of available CHANNELS
      /// </summary>
      /// <param name="i2cDevice"></param>
      /// <param name="hatAddress"></param>
      /// <param name="busDevice"></param>
      private void OpenHat(I2cDevice i2cDevice, ushort hatAddress, II2CBusDevice busDevice)
      {
         /* Initialise the HATs Interface (SPI, I2C, etc...) */
         m_HatInterface = new HatInterface_I2C(i2cDevice, hatAddress, busDevice);

         /* Open communcation interface */
         m_HatInterface.Open();

         Channels = new List<IChannel>();

         /* Initialise availble channels on attached HAT */
         for (uint i = 0; i < busDevice.NumberOfChannels; i++)
         {
            IChannel chan = null;
            IIOPin pin = null;

            switch (HatType)
            {
               case SupportedHATs.MOSFET_v1:
                  if (i < 5)
                     chan = new ChannelFunction_PWM(i);
                  break;

               case SupportedHATs.INPUT_v1:
                  pin = (busDevice as BusDevice_PCA9501).GetPin((ushort)i);

                  pin.SetDriveMode(GpioPinDriveMode.InputPullUp);

                  chan = new ChannelFunction_INPUT(i, pin);
                  (chan as ChannelFunction_INPUT).InputLevelChanged += HostController.OnInputChannelNotification;
                  break;

               case SupportedHATs.RELAY_v1:
                  if (i < 4)
                  {
                     pin = (busDevice as BusDevice_PCA9501).GetPin((ushort)i);

                     pin.SetDriveMode(GpioPinDriveMode.Output);

                     chan = new ChannelFunction_RELAY(i, pin);
                  }
                  break;

               case SupportedHATs.NoOfSupportedHATs:
               default:
                  throw new Exception("Hat not supported.");
            }

            if (chan != null)
            {
               Channels.Add(chan);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="chan"></param>
      public void UpdateChannel(IChannel chan)
      {
         m_HatInterface.BusDevice.RefreshChannel(chan);
      }
   }
}