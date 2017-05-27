﻿using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
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
      public BusDevice_PCA9501 busDevice;
      public UInt16 address;

      public RPiHat_INPUT_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
      {
         HatType = SupportedHATs.INPUT_v1;
         busDevice = new BusDevice_PCA9501();
         address = hatAddress;

         /* Open the BUS DEVICE */
         busDevice.Open(i2cDevice);

         /* Initialise the BUS DEVICE */
         busDevice.InitialiseChannels();

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

      public override void RefreshChannel(IChannel chan)
      {
         busDevice.RefreshChannel(chan);
      }
   }
}