using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevices;
using System.IO;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.Drivers
{
   public interface IDriverSoundProvider
   {
      void Play(byte track, byte volume = 30);
      void Stop();
   }
}