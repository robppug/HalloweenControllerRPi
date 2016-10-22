using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
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
      private TimeSpan _postTriggerTime;
      private GpioPin _Pin;
      private DispatcherTimer _reenableTimer;

      public delegate void EventHandlerInput(object sender, EventArgsINPUT e);
      public event EventHandlerInput InputLevelChanged;

      public HWRaspberryPI_INPUT(uint chan, GpioPin pin)
      {
         Channel = chan;

         _Pin = pin;

         _Pin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
         _Pin.ValueChanged += Pin_ValueChanged;

         _postTriggerTime = TimeSpan.FromMilliseconds(50);

         _reenableTimer = new DispatcherTimer();
         _reenableTimer.Tick += _reenableTimer_Tick;
         _reenableTimer.Interval = _postTriggerTime;
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

      public TimeSpan PostTriggerTime
      {
         get { return _postTriggerTime; }
         set { _postTriggerTime = value; }
      }
      
      private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
      {
         GpioPinEdge gpEdge = args.Edge;

         if (_reenableTimer.IsEnabled == false)
         {
            if (InputLevelChanged != null)
            {
               InputLevelChanged.Invoke(this, new EventArgsINPUT((gpEdge == GpioPinEdge.RisingEdge ? tenTriggerLvl.tHigh : tenTriggerLvl.tLow), Channel));

               _reenableTimer.Start();
            }
         }
      }

      private void _reenableTimer_Tick(object sender, object e)
      {
         _reenableTimer.Stop();
      }
   }
}
