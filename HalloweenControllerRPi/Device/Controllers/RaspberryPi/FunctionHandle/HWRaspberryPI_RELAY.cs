using System;
using Windows.Devices.Gpio;
using static HalloweenControllerRPi.Functions.Func_RELAY;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi
{
   class HWRaspberryPI_RELAY : IFunctionHandler, IProcessTick
   {
      private uint _channelIdx;
      private tenOutputLevel _outputLevel;
      private GpioPin _Pin;

      public HWRaspberryPI_RELAY(uint chan, GpioPin pin)
      {
         _outputLevel = tenOutputLevel.tLow;

         Channel = chan;
         
         _Pin = pin;
      }

      public uint Channel
      {
         set { _channelIdx = value;  }
         get { return _channelIdx; }
      }

      public tenOutputLevel OutputLevel
      {
         get { return _outputLevel; }
         set { _outputLevel = value; }
      }

      public GpioPinValue CurrentPinLevel
      {
         get { return _Pin.Read(); }
         set { _Pin.Write(value); }
      }

      public void Tick()
      {
         if (OutputLevel == tenOutputLevel.tHigh)
         {
            CurrentPinLevel = GpioPinValue.Low;
         }
         else
         {
            CurrentPinLevel = GpioPinValue.High;
         }
      }
   }
}
