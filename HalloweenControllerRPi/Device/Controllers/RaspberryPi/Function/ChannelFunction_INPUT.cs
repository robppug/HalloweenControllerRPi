using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using static HalloweenControllerRPi.Functions.Func_INPUT;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi
{
   public class ChannelFunction_INPUT : IChannel
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
      private IIOPin _Pin;
      private DispatcherTimer _reenableTimer;
      private bool _waitForRetrigger;

      public delegate void EventHandlerInput(object sender, EventArgsINPUT e);
      public event EventHandlerInput InputLevelChanged;

      public ChannelFunction_INPUT(uint chan, IIOPin pin)
      {
         Index = chan;

         _Pin = pin;

         _Pin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
         _Pin.ValueChanged += Pin_ValueChanged;

         _postTriggerTime = TimeSpan.FromMilliseconds(50);

         _reenableTimer = new DispatcherTimer();
         _reenableTimer.Tick += _reenableTimer_Tick;
         _reenableTimer.Interval = _postTriggerTime;
         _waitForRetrigger = false;
      }

      public uint Index
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

      private async void Pin_ValueChanged(IIOPin sender, InputPinValueChangedEventArgs args)
      {
         if (_waitForRetrigger == false)
         {
            _waitForRetrigger = true;

            Task.Run(() => OnInputLevelChanged(sender, args));
         }
      }

      private async Task OnInputLevelChanged(IIOPin sender, InputPinValueChangedEventArgs args)
      {
         GpioPinEdge gpEdge = args.Edge;

         InputLevelChanged?.Invoke(sender, new EventArgsINPUT((gpEdge == GpioPinEdge.RisingEdge ? tenTriggerLvl.tHigh : tenTriggerLvl.tLow), Index));

         _reenableTimer.Interval = _postTriggerTime;
         _reenableTimer.Start();
      }

      private void _reenableTimer_Tick(object sender, object e)
      {
         _waitForRetrigger = false;
         _reenableTimer.Stop();
      }
   }
}
