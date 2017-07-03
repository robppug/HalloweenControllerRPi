using System;
using Windows.ApplicationModel.Core;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using static HalloweenControllerRPi.Functions.Func_INPUT;

namespace HalloweenControllerRPi.Device.Controllers.Channels
{
   public class ChannelFunction_BUTTON : IChannel, IProcessTick
   {
      public class ButtonStateEventArgs : EventArgs
      {
         private uint _index;
         private GpioPinEdge _edge;

         public ButtonStateEventArgs(IIOPin pin, GpioPinEdge edge)
         {
            _index = pin.PinNumber;
            _edge = edge;
         }

         public uint PinNumber => _index;
         public GpioPinEdge ButtonState => _edge;
      }

      private TimeSpan _debTime;
      private IIOPin _Pin;
      private bool _waitForRetrigger;
      private IChannelHost _channelHost;

      public delegate void EventHandlerButton(object sender, ButtonStateEventArgs e);

      public event EventHandlerButton ButtonStateChanged;

      public ChannelFunction_BUTTON(IChannelHost host, uint chan, IIOPin pin)
      {
         Index = chan;
         ChannelHost = host;

         _Pin = pin;

         _Pin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
         _Pin.ValueChanged += Pin_ValueChanged;

         _waitForRetrigger = false;
      }

      public uint Index { get; set; }

      public IChannelHost ChannelHost
      {
         get { return _channelHost; }
         private set { _channelHost = value; }
      }

      public uint Level
      {
         get { return (uint)_Pin.Read(); }
         set { _Pin.Write((GpioPinValue)value); }
      }

      private async void Pin_ValueChanged(IIOPin sender, InputPinValueChangedEventArgs args)
      {
         if (_waitForRetrigger == false)
         {
            _waitForRetrigger = true;

            /* MUST run in the UI thread */
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () => OnInputLevelChanged(sender, args));
         }
      }

      private void OnInputLevelChanged(IIOPin sender, InputPinValueChangedEventArgs args)
      {
         GpioPinEdge gpEdge = args.Edge;

         ButtonStateChanged?.Invoke(sender, new ButtonStateEventArgs(sender, args.Edge));
      }

      public void Tick()
      {
         _Pin.Read();
      }

      public uint GetValue()
      {
         return Level;
      }
   }
}