using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevices
{
   public interface IDeviceComms
   {
      int Read(byte[] buffer);

      void Write(byte[] buffer);
   }
}
