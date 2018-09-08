using HalloweenControllerRPi.Device.Controllers.Providers;
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
        private MenuButton _buttonFunc;

        public delegate void EventHandlerButton(object sender, ButtonStateEventArgs e);

        public event EventHandlerButtonAction ButtonPushed;
        public event EventHandlerButtonAction ButtonReleased;
        public event EventHandlerButtonAction ButtonLongPush;
        public event EventHandlerButtonAction ButtonLongReleased;

        public MenuButton ButtonFunction { get; set; } = MenuButton.Invalid;

        public ChannelFunction_BUTTON(IChannelHost host, uint chan, IIOPin pin)
        {
            Index = chan;
            ChannelHost = host;

            _Pin = pin;

            _Pin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            _Pin.Read();
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

        private void ButtonTimer_Tick(object sender, object e)
        {
            //ButtonLongPush?.Invoke(sender, new ButtonActionEventArgs());
        }

        private void Pin_ValueChanged(IIOPin sender, InputPinValueChangedEventArgs args)
        {
            if (_buttonFunc != MenuButton.Invalid)
            {
                if (args.Edge == GpioPinEdge.RisingEdge)
                {
                    ButtonPushed?.Invoke(this, new ButtonActionEventArgs(_buttonFunc, ButtonAction.Pushed));

                    //        buttonTimers[(int)e.PinNumber] = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
                    //        buttonTimers[(int)e.PinNumber].Tick += ButtonTimer_Tick;
                    //        buttonTimers[(int)e.PinNumber].Start();
                    //    }
                    //    else
                    //    {
                    //        //if (buttonTimers[(int)e.PinNumber] == null)
                    //        {
                    //            //    ButtonLongReleased?.Invoke(sender, buttonActionEventArgs);
                    //        }
                    //        //else
                    //        {
                    //            ButtonReleased?.Invoke(sender, buttonActionEventArgs);
                    //        }
                    //        buttonTimers[(int)e.PinNumber].Stop();
                    //        buttonTimers[(int)e.PinNumber] = null;
                }
                else
                {
                    ButtonReleased?.Invoke(this, new ButtonActionEventArgs(_buttonFunc, ButtonAction.Released));
                }
            }
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