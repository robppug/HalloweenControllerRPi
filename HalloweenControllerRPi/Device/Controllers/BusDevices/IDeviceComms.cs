using System;
using System.Collections.Generic;

namespace HalloweenControllerRPi.Device.Controllers.BusDevices
{
   public interface IDeviceComms
   {
      event EventHandler<DeviceCommsEventArgs> DataReceived;

      byte[] Read(int bytes = 1);

      void Write(byte[] buffer);

      byte[] WriteRead(byte[] txBuffer);
   }

   public class DeviceCommsEventArgs : EventArgs
   {
      public byte[] Data { get; }

      public DeviceCommsEventArgs(byte[] data)
      {
         Data = data;
      }
   }
}
