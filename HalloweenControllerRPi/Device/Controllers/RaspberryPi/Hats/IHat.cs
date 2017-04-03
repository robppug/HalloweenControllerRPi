using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using static HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.RPiHat;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   public interface IHat
   {
      IHWController HostController { get; }

      SupportedHATs HatType { get; }
      List<IChannel> Channels { get; }

      void UpdateChannel(IChannel chan);
   }
}