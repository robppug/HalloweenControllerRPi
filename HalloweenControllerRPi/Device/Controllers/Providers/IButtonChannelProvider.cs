using HalloweenControllerRPi.Device.Controllers.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.Providers
{
    interface IButtonChannelProvider
    {
        Dictionary<MenuButton, IChannel> ButtonList { get; set; }

        void Initialise(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress);
    }

    public delegate void EventHandlerButtonAction(object sender, ButtonActionEventArgs e);
    public enum MenuButton : byte
    {
        Left,
        Right,
        Up,
        Down,
        Enter,
        FunctionLeft,
        FunctionRight,
        Invalid
    };
    public enum ButtonAction : byte
    {
        Pushed,
        Released,
        LongPush,
        LongRelease,
        Invalid
    };

    public class ButtonActionEventArgs : EventArgs
    {
        public ButtonActionEventArgs(MenuButton button, ButtonAction action)
        {
            Button = button;
            Action = action;
        }

        public MenuButton Button { get; }

        public ButtonAction Action { get; }
    }
}
