﻿using System;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace HalloweenControllerRPi.Device.Controllers.Channels
{
   public interface IIOPin
   {
      TimeSpan DebounceTimeout { get; set; }
      UInt32 PinNumber { get; }

      event TypedEventHandler<IIOPin, InputPinValueChangedEventArgs> ValueChanged;

      GpioPinDriveMode GetDriveMode();
      Boolean IsDriveModeSupported(GpioPinDriveMode driveMode);
      GpioPinValue Read();
      void SetDriveMode(GpioPinDriveMode value);
      void Write(GpioPinValue value);
   }
}