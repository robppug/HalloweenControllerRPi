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
   /// Raspberry Pi HAT - MOSFET v1
   /// </summary>
   public class RPiHat_MOSFET_v1 : RPiHat
   {
      public BusDevice_PCA9685 busDevice;
      public UInt16 address;

      public RPiHat_MOSFET_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
      {
         HatType = SupportedHATs.MOSFET_v1;
         busDevice = new BusDevice_PCA9685();
         address = hatAddress;

         /* Open the BUS DEVICE */
         busDevice.Open(i2cDevice);

         /* Initialise the BUS DEVICE */
         busDevice.InitialiseChannels();

         Channels = new List<IChannel>();

         /* Initialise availble channels on attached HAT */
         for (uint i = 0; i < 5; i++)
         {
            IChannel chan = null;

            chan = new ChannelFunction_PWM(this, i);

            if (chan != null)
            {
               Channels.Add(chan);
            }
         }
      }

      public override void RefreshChannel(IChannel chan)
      {
         busDevice.RefreshChannel(chan);
      }
   }
}