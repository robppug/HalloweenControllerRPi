using HalloweenControllerRPi.Device.Controllers;
using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Device.Controllers.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.UI.ExternalDisplay
{
    public class MenuControl
    {
        private List<ChannelFunction_BUTTON> _buttonList;

        public event EventHandlerButtonAction ButtonStateChanged;

        public MenuControl(Dictionary<MenuButton, IChannel> buttonList)
        {
            _buttonList = new List<ChannelFunction_BUTTON>();

            foreach (IChannel b in buttonList.Values)
            {
                ChannelFunction_BUTTON button = (b as ChannelFunction_BUTTON);

                button.ButtonPushed += Button_ButtonPushed;
                button.ButtonReleased += Button_ButtonReleased;
                button.ButtonLongPush += Button_ButtonLongPush;
                button.ButtonLongReleased += Button_ButtonLongReleased;

                _buttonList.Add(button);
            }
        }

        private void Button_ButtonLongPush(object sender, ButtonActionEventArgs e)
        {
            ButtonStateChanged?.Invoke(sender, e);
        }

        private void Button_ButtonLongReleased(object sender, ButtonActionEventArgs e)
        {
            ButtonStateChanged?.Invoke(sender, e);
        }

        private void Button_ButtonReleased(object sender, ButtonActionEventArgs e)
        {
            ButtonStateChanged?.Invoke(sender, e);
        }

        private void Button_ButtonPushed(object sender, ButtonActionEventArgs e)
        {
            ButtonStateChanged?.Invoke(sender, e);
        }
    }
}
