using HalloweenControllerRPi.Device.Controllers.Channels;
using System;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Xaml;

namespace HalloweenControllerRPi.Device.Controllers.Channels
{
    public class InputPinValueChangedEventArgs : EventArgs
    {
        public GpioPinEdge Edge { get; private set; }

        public InputPinValueChangedEventArgs(GpioPinEdge edge)
        {
            Edge = edge;
        }
    }

    class IOPin : IIOPin
    {
        private GpioPinEdge m_lastEdge;
        private GpioPinValue m_lastValue;
        private GpioPinValue m_value;
        private GpioPinDriveMode m_driveMode;
        private uint m_pin;
        private DispatcherTimer DebounceTimer;

        public event TypedEventHandler<IIOPin, InputPinValueChangedEventArgs> ValueChanged;

        public TimeSpan DebounceTimeout { get; set; } = TimeSpan.FromMilliseconds(50);

        public uint PinNumber
        {
            get
            {
                return m_pin;
            }
        }

        public IOPin(uint pin)
        {
            m_pin = pin;
            m_value = GpioPinValue.High;
            m_lastEdge = GpioPinEdge.FallingEdge;
            m_lastValue = GpioPinValue.High;

            DebounceTimer = new DispatcherTimer();
            DebounceTimer.Tick += DebounceTimer_Tick;
            DebounceTimer.Interval = DebounceTimeout;
        }

        public IOPin(uint pin, GpioPinValue activeState) : this(pin)
        {
            m_value = (activeState == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
            m_lastEdge = (activeState == GpioPinValue.High ? GpioPinEdge.FallingEdge : GpioPinEdge.RisingEdge);
            m_lastValue = (activeState == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
        }

        private void DebounceTimer_Tick(object sender, object e)
        {
            DebounceTimer.Stop();

            ValueChanged?.Invoke(this, new InputPinValueChangedEventArgs(m_lastEdge));
        }

        protected void OnValueChanged(InputPinValueChangedEventArgs e)
        {
            bool boStartTimer = true;

            if (DebounceTimer.IsEnabled)
            {
                if (e.Edge != m_lastEdge)
                {
                    DebounceTimer.Stop();
                }
                else
                    boStartTimer = false;
            }

            m_lastEdge = e.Edge;

            if (boStartTimer)
            {
                DebounceTimer.Interval = DebounceTimeout;
                DebounceTimer.Start();
            }
        }

        public bool IsDriveModeSupported(GpioPinDriveMode driveMode)
        {
            bool boSupported = false;

            switch (driveMode)
            {
                case GpioPinDriveMode.InputPullUp:
                case GpioPinDriveMode.Output:
                    boSupported = true;
                    break;
                case GpioPinDriveMode.Input:
                case GpioPinDriveMode.InputPullDown:
                case GpioPinDriveMode.OutputOpenDrain:
                case GpioPinDriveMode.OutputOpenDrainPullUp:
                case GpioPinDriveMode.OutputOpenSource:
                case GpioPinDriveMode.OutputOpenSourcePullDown:
                default:
                    break;
            }
            return boSupported;
        }

        public GpioPinValue Read()
        {
            return m_value;
        }

        public void SetDriveMode(GpioPinDriveMode value)
        {
            if (IsDriveModeSupported(value) == true)
            {
                m_driveMode = value;
            }
            else
            {
                throw new Exception("Drive mode (" + value.ToString() + ") is not supported.");
            }
        }

        public GpioPinDriveMode GetDriveMode()
        {
            return m_driveMode;
        }


        public void Write(GpioPinValue value)
        {
            /* If configured as an INPUT, we have some additional processing */
            if (m_driveMode == GpioPinDriveMode.InputPullUp)
            {
                if (m_lastValue != m_value)
                {
                    GpioPinEdge edge;

                    switch (m_value)
                    {
                        case GpioPinValue.High:
                            edge = GpioPinEdge.RisingEdge;
                            break;
                        case GpioPinValue.Low:
                        default:
                            edge = GpioPinEdge.FallingEdge;
                            break;
                    }

                    m_lastValue = m_value;

                    OnValueChanged(new InputPinValueChangedEventArgs(edge));
                }
            }

            m_value = value;
        }
    }
}
