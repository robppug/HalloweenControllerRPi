using Windows.Devices.Gpio;
using static HalloweenControllerRPi.Functions.Func_RELAY;

namespace HalloweenControllerRPi.Device.Controllers.Channels
{
   internal class ChannelFunction_RELAY : IChannel, IProcessTick
   {
      private tenOutputLevel _outputLevel;
      private IIOPin _Pin;
      private IChannelHost _channelHost;

      public ChannelFunction_RELAY(IChannelHost host, uint chan, IIOPin pin)
      {
         _outputLevel = tenOutputLevel.tLow;

         Index = chan;

         ChannelHost = host;

         _Pin = pin;
      }

      public uint Index { get; set; }

      public IChannelHost ChannelHost
      {
         get { return _channelHost; }
         private set { _channelHost = value; }
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

      public uint GetValue()
      {
         return Level;
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