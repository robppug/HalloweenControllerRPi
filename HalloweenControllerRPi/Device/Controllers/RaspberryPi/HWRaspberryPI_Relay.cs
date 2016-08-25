using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using static HalloweenControllerRPi.Functions.Func_RELAY;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi
{
   class HWRaspberryPI_RELAY
   {
      private uint _channelIdx;
      private OutputLevel _outputLevel;
      private GpioPin _Pin;
      private GpioPin _LastPin;

      public HWRaspberryPI_RELAY(uint chan, GpioPin pin)
      {
         _outputLevel = OutputLevel.tLow;

         Channel = chan;
         
         _Pin = pin;
         _LastPin = _Pin;
      }

      public uint Channel
      {
         private set { _channelIdx = value;  }
         get { return _channelIdx; }
      }

      public OutputLevel OutputLevel
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
         bool validTrigger = false;

         if (OutputLevel == OutputLevel.tHigh)
         {
            if ((_LastPin.Read() == GpioPinValue.Low) && (_Pin.Read() == GpioPinValue.High))
            {
               validTrigger = true;
            }
         }
         else if (OutputLevel == OutputLevel.tLow)
         {
            if ((_LastPin.Read() == GpioPinValue.High) && (_Pin.Read() == GpioPinValue.Low))
            {
               validTrigger = true;
            }
         }

         _LastPin = _Pin;
      }
   }
}
