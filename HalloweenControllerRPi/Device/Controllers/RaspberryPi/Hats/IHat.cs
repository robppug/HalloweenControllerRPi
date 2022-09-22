using HalloweenControllerRPi.Device.Controllers.Channels;
using System.Collections.Generic;
using static HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.RPiHat;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
    public interface IHat
    {
        SupportedHATs HatType { get; }
        List<IChannel> Channels { get; }

        void HatTask();
    }
}