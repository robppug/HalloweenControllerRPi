﻿using System;
using System.Collections.Generic;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   public interface IHatInterface
   {
      /// <summary>
      /// This is the DEVICE which is connected to a HAT
      /// </summary>
      II2CBusDevice BusDevice { get; }

      void Open();
      void Close();
   }
}