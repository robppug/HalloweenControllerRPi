using System;
using System.Collections.Generic;

namespace HalloweenControllerRPi.Device.Controllers.BusDevices
{
   public interface IDeviceComms
   {
      List<byte> rxData { get; }

      event EventHandler<EventArgs> DataReceived;

      byte[] Read(ushort bytes);

      void Write(byte[] buffer);
   }
}
