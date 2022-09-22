using HalloweenControllerRPi.Device.Controllers.Channels;
using System;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device.Controllers
{
    public interface IHWController : ISupportedFunctions
    {
        Task Connect();
        void Disconnect();

        void OnChannelNotification(IChannel sender, CommandEventArgs e);
    }
}
