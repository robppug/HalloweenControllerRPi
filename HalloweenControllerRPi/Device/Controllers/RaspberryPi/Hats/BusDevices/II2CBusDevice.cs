using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevices
{
   public interface II2CBusDevice
   {
      I2cDevice _i2cDevice { get; }

      void Open(I2cDevice i2cDevice);
      void Close();
   }
}