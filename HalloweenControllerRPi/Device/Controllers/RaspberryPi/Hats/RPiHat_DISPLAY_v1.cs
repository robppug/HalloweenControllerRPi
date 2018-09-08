using HalloweenControllerRPi.Device.Controllers.BusDevices;
using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Device.Controllers.Providers;
using HalloweenControllerRPi.Device.Drivers;
using System;
using System.Collections.Generic;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Microsoft.IoT.Lightning.Providers;
using static HalloweenControllerRPi.Device.Controllers.Channels.ChannelFunction_BUTTON;
using static HalloweenControllerRPi.Device.Controllers.Channels.ChannelFunction_INPUT;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
    public class RPiHat_DISPLAY_v1 : RPiHat, IDisplayChannel, IButtonChannelProvider
    {
        static readonly uint NO_OF_BUTTONS = 7;

        private BusDevice_PCA9501<DeviceComms_I2C> _busDevice;
        private SSD1306<DeviceComms_I2C> displayDriver;
        private readonly UInt16? buttonAddress;
        private readonly UInt16 displayAddress;
        private DispatcherTimer[] buttonTimers = new DispatcherTimer[NO_OF_BUTTONS];
        
        public IDriverDisplayProvider Device => displayDriver;

        public Dictionary<MenuButton, IChannel> ButtonList { get; set; }

        public RPiHat_DISPLAY_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
        {
            HatType = SupportedHATs.DISPLAY_v1;
            _busDevice = null;
            displayDriver = new SSD1306<DeviceComms_I2C>(128, 64);
            buttonAddress = null;
            displayAddress = hatAddress;

            /* Open the DISPLAY driver */
            displayDriver.Open(new DeviceComms_I2C(i2cDevice));

            /* Initialise the DISPLAY DRIVER */
            displayDriver.InitialiseDriver();

            Channels = new List<IChannel>();
        }

        public void Initialise(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress)
        {
            _busDevice = new BusDevice_PCA9501<DeviceComms_I2C>();
            ButtonList = new Dictionary<MenuButton, IChannel>();

            /* Open the BUS DEVICE */
            _busDevice.Open(new DeviceComms_I2C(i2cDevice));

            /* Initialise the BUS DEVICE */
            _busDevice.InitialiseDriver();

            IIOPin PWPin = _busDevice.GetPin(7);

            PWPin.SetDriveMode(GpioPinDriveMode.Output);
            PWPin.Write(GpioPinValue.Low);

            /* Initialise available channels on attached HAT */
            for (uint i = 0; i < NO_OF_BUTTONS; i++)
            {
                ChannelFunction_BUTTON chan = null;
                IIOPin pin = null;

                pin = _busDevice.GetPin((ushort)i);

                pin.SetDriveMode(GpioPinDriveMode.InputPullUp);

                chan = new ChannelFunction_BUTTON(this, i, pin);
                chan.ButtonFunction = GetButtonFunction(i);

                if (chan != null)
                {
                    Channels.Add(chan);
                    ButtonList.Add(GetButtonFunction(i), chan);
                }
            }
        }

        private MenuButton GetButtonFunction(uint i)
        {
            switch (i)
            {
                case 0:
                    return MenuButton.Left;
                case 1:
                    return MenuButton.Enter;
                case 2:
                    return MenuButton.Down;
                case 3:
                    return MenuButton.Up;
                case 4:
                    return MenuButton.Right;
                case 5:
                    return MenuButton.FunctionLeft;
                case 6:
                    return MenuButton.FunctionRight;

                default:
                    return MenuButton.Invalid;
            }
        }

        public override void RefreshChannel(IChannel chan)
        {
            _busDevice?.RefreshChannel(chan);
        }
    }
}