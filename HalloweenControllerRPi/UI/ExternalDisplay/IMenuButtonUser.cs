using HalloweenControllerRPi.Device.Controllers.Providers;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace HalloweenControllerRPi.UI.ExternalDisplay
{
    public interface IMenuButtonUser
    {
        Dictionary<MenuButton, MenuNode<UserControl>> MenuFunctions { get; set; }
    }
}