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
   /// Raspberry Pi HAT - INPUT v1
   /// </summary>
   public class RPiHat_INPUT_v1 : RPiHat
   {
      public RPiHat_INPUT_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
      {
         BusDevice_PCA9501 busDevice = new BusDevice_PCA9501();

         HatType = SupportedHATs.INPUT_v1;
         
         /* Initialise the HATs Interface (SPI, I2C, etc...) */
         m_HatInterface = new HatInterface_I2C(i2cDevice, hatAddress, busDevice);

         /* Open communcation interface */
         m_HatInterface.Open();

         Channels = new List<IChannel>();

         /* Initialise availble channels on attached HAT */
         for (uint i = 0; i < 8; i++)
         {
            ChannelFunction_INPUT chan = null;
            IIOPin pin = null;

            pin = busDevice.GetPin((ushort)i);

            pin.SetDriveMode(GpioPinDriveMode.InputPullUp);

            chan = new ChannelFunction_INPUT(this, i, pin);

            chan.InputLevelChanged += HostController.OnInputChannelNotification;
               
            if (chan != null)
            {
               Channels.Add(chan);
            }
         }
      }
   }
}