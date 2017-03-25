using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
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
         MOSTFET_v1 = 0,
         RELAY_v1,
         INPUT_v1,
         NoOfSupportedHATs
      };

      #endregion /* ENUMS */

      #region Declarations

      private IHatInterface m_HatInterface;

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

      private RPiHat(I2cDevice i2cDevice, UInt16 hatAddress)
      {
         II2CBusDevice busDevice = null;
         SupportedHATs hat = SupportedHATs.NoOfSupportedHATs;

         hat = DiscoverHat();

         if (hat != SupportedHATs.NoOfSupportedHATs)
         {
            HatType = hat;

            switch (hat)
            {
               case SupportedHATs.MOSTFET_v1:
                  busDevice = new BusDevice_PCA9685();
                  break;

               case SupportedHATs.INPUT_v1:
                  break;

               case SupportedHATs.RELAY_v1:
                  busDevice = new BusDevice_PCA9501();
                  break;

               case SupportedHATs.NoOfSupportedHATs:
               default:
                  throw new Exception("Hat not supported.");
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
      public static RPiHat Open(I2cDevice i2cDevice, UInt16 hatAddress)
      {
         return new RPiHat(i2cDevice, hatAddress);
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

               if ((c as ChannelFunction_PWM) != null)
               {
                  ChannelFunction_PWM pwm = (c as ChannelFunction_PWM);
                  IBusDevicePwmChannelProvider pwmDevice = (IBusDevicePwmChannelProvider)m_HatInterface.BusDevice;

                  if (pwm.Function != Func_PWM.tenFUNCTION.FUNC_OFF)
                  {
                     pwmDevice.SetChannel((ushort)pwm.Index, (ushort)pwm.Level);
                  }
                  else
                  {
                     pwmDevice.SetChannel((ushort)pwm.Index, 0x00);
                  }
               }
            }
         }
      }

      private SupportedHATs DiscoverHat()
      {
         /* Discover the type of HAT connected */
         SupportedHATs hat = SupportedHATs.MOSTFET_v1;

         return hat;
      }

      private void OpenHat(I2cDevice i2cDevice, ushort hatAddress, II2CBusDevice busDevice)
      {
         /* Initialise the HATs Interface (SPI, I2C, etc...) */
         m_HatInterface = new HatInterface_I2C(i2cDevice, hatAddress, busDevice);

         /* Open communcation and populate the list of available Channels the HAT offers */
         m_HatInterface.Open();

         Channels = new List<IChannel>();

         /* Initialise availble channels on attached HAT */
         for (uint i = 0; i < busDevice.NumberOfChannels; i++)
         {
            IChannel chan = null;

            switch (HatType)
            {
               case SupportedHATs.MOSTFET_v1:
                  chan = new ChannelFunction_PWM(i);
                  break;

               case SupportedHATs.INPUT_v1:
                  Channels.Add(new ChannelFunction_INPUT(i, (busDevice as BusDevice_PCA9501).GetPin((ushort)i)));
                  break;

               case SupportedHATs.RELAY_v1:
                  Channels.Add(new ChannelFunction_RELAY(i, (busDevice as BusDevice_PCA9501).GetPin((ushort)i)));
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
   }
}