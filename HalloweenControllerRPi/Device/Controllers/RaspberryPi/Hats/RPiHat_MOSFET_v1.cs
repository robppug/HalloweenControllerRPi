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
      public RPiHat_MOSFET_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
      {
         HatType = SupportedHATs.MOSFET_v1;
         
         /* Initialise the HATs Interface (SPI, I2C, etc...) */
         m_HatInterface = new HatInterface_I2C(i2cDevice, hatAddress, new BusDevice_PCA9685());

         /* Open communcation interface */
         m_HatInterface.Open();

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
   }
}