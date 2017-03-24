﻿using System;
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function
{
   interface IIOPin
   {
      TimeSpan DebounceTimeout { get; set; }
      UInt32 PinNumber { get; }

      event TypedEventHandler<IIOPin, GpioPinValueChangedEventArgs> ValueChanged;

      GpioPinDriveMode GetDriveMode();
      Boolean IsDriveModeSupported(GpioPinDriveMode driveMode);
      GpioPinValue Read();
      void SetDriveMode(GpioPinDriveMode value);
      void Write(GpioPinValue value);
   }
}