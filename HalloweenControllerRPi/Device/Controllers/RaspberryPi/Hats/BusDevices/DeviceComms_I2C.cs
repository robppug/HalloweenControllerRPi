using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevices
{
   public class DeviceComms_I2C : IDeviceComms
   { 
      private I2cDevice _i2cDevice;

      public I2cDevice i2cDevice
      {
         get { return _i2cDevice; }
      }

      public DeviceComms_I2C(I2cDevice dev)
      {
         _i2cDevice = dev;
      }

      public virtual void Write(byte[] buffer)
      {
         _i2cDevice.Write(buffer);
      }

      public virtual int Read(byte[] buffer)
      {
         return 0;
      }
   }
}
