using System;
using Windows.Devices.Gpio;
using static HalloweenControllerRPi.Functions.Func_INPUT;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi
{
   class HWRaspberryPI_INPUT : IFunctionHandler
   {
      public class EventArgsINPUT : EventArgs
      {
         private tenTriggerLvl _trgLvl;
         private uint _index;

         public EventArgsINPUT(tenTriggerLvl trgLvl, uint index)
         {
            _trgLvl = trgLvl;
            _index = index;
         }

         public tenTriggerLvl TriggerLevel { get { return _trgLvl; } }
         public uint Index { get { return _index; } }
      }

      private uint _channelIdx;
      private TimeSpan _debTime;
      private GpioPin _Pin;

      public delegate void EventHandlerInput(object sender, EventArgsINPUT e);
      public event EventHandlerInput InputLevelChanged;

      public HWRaspberryPI_INPUT(uint chan, GpioPin pin)
      {
         Channel = chan;

         _Pin = pin;

         _Pin.ValueChanged += Pin_ValueChanged;
      }

      public uint Channel
      {
         set { _channelIdx = value;  }
         get { return _channelIdx; }
      }

      public GpioPinValue CurrentPinLevel
      {
         get { return _Pin.Read(); }
         set { _Pin.Write(value); }
      }

      public TimeSpan DebounceTime
      {
         get { return _debTime; }
         set { _debTime = value; if(_Pin != null) _Pin.DebounceTimeout = _debTime; }
      }

      private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
      {
         GpioPinEdge gpEdge = args.Edge;

         InputLevelChanged.Invoke(this, new EventArgsINPUT((gpEdge == GpioPinEdge.RisingEdge ? tenTriggerLvl.tHigh : tenTriggerLvl.tLow), Channel));
      }
   }
}
