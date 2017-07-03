using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Device.Controllers.Providers;
using HalloweenControllerRPi.Device.Drivers;

namespace HalloweenControllerRPi.Device.Controllers
{
   public interface IDisplayChannel
   {
      IDriverDisplayProvider Device { get; }
   }
}