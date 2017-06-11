﻿using HalloweenControllerRPi.Device.Controllers.BusDevices;
using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Device.Drivers;
using System;
using System.Collections.Generic;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   public class RPiHat_DISPLAY_v1 : RPiHat
   {
      BusDevice_PCA9501<DeviceComms_I2C> busDevice;
      SSD1306<DeviceComms_I2C> displayDriver;
      UInt16 address;

      public RPiHat_DISPLAY_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
      {
         HatType = SupportedHATs.DISPLAY_v1;
         busDevice = new BusDevice_PCA9501<DeviceComms_I2C>();
         displayDriver = new SSD1306<DeviceComms_I2C>();
         address = hatAddress;

         /* Open the BUS DEVICE */
         busDevice.Open(new DeviceComms_I2C(i2cDevice));

         /* Initialise the BUS DEVICE */
         busDevice.InitialiseDriver();

         /* Open the DISPLAY driver */
         displayDriver.Open(new DeviceComms_I2C(i2cDevice));

         /* Initialise the DISPLAY DRIVER */
         displayDriver.InitialiseDriver();

         displayDriver.ClearDisplayBuf();

         // Row 0, and image
         displayDriver.WriteImage(DisplayImages.Connected, 0, 0);

         // Row 1 - 3
         displayDriver.WriteLine("Halloween", 0, 1);
         displayDriver.WriteLine("Controller RPi", 0, 2);

         displayDriver.Update();

         Channels = new List<IChannel>();

         /* Initialise availble channels on attached HAT */
         for (uint i = 0; i < 7; i++)
         {
            ChannelFunction_INPUT chan = null;
            IIOPin pin = null;

            pin = busDevice.GetPin((ushort)i);

            pin.SetDriveMode(GpioPinDriveMode.InputPullUp);

            chan = new ChannelFunction_INPUT(this, i, pin);

            chan.InputLevelChanged += Chan_InputLevelChanged;

            if (chan != null)
            {
               Channels.Add(chan);
            }
         }
      }

      private void Chan_InputLevelChanged(object sender, ChannelFunction_INPUT.EventArgsINPUT e)
      {
         HostController.OnChannelNotification(this, new CommandEventArgs('I', 'G', e.Index + 1, (uint)e.TriggerLevel));
      }

      public override void RefreshChannel(IChannel chan)
      {
      }
   }
}
