﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   interface IHat
   {
      List<IChannel> Channels { get; }

      void ProcessTask();
   }
}
