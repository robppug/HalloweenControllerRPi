using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi
{
   public interface IChannel
   {
      IHat HostHat { get; }
      uint Index { get; set; }
      uint Level { get; set; }
   }
}