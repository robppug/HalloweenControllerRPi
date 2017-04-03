using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats;
using System;
using Windows.Devices.Gpio;
using static HalloweenControllerRPi.Functions.Func_RELAY;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi
{
   internal class ChannelFunction_RELAY : IChannel
   {
      private uint _channelIdx;
      private tenOutputLevel _outputLevel;
      private IIOPin _Pin;
      private IHat _hostHat;

      public ChannelFunction_RELAY(IHat host, uint chan, IIOPin pin)
      {
         _outputLevel = tenOutputLevel.tLow;

         Index = chan;

         HostHat = host;

         _Pin = pin;
      }

      public uint Index
      {
         set { _channelIdx = value; }
         get { return _channelIdx; }
      }

      public IHat HostHat
      {
         get { return _hostHat; }
         private set { _hostHat = value; }
      }

      public uint Level
      {
         get { return (uint)_outputLevel; }
         set { _outputLevel = (tenOutputLevel)value; Tick(); }
      }

      public GpioPinValue CurrentPinLevel
      {
         get { return _Pin.Read(); }
         set { _Pin.Write(value); }
      }

      public void Tick()
      {
         if ((tenOutputLevel)Level == tenOutputLevel.tHigh)
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