using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   class HatInterface_SPI : IHatInterface
   {
      public II2CBusDevice BusDevice
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public void Open()
      {
      }

      public void Close()
      {
         throw new NotImplementedException();
      }

      public void Read(out byte[] data)
      {
         throw new NotImplementedException();
      }

      public void Write(byte[] data)
      {
         throw new NotImplementedException();
      }
   }
}