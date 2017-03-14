﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   interface II2CBusDevice
   {
      List<IChannel> Channels { get; }

      void Open(I2cDevice i2cDevice);
      void Close();
      void InitialiseChannels();
   }
}