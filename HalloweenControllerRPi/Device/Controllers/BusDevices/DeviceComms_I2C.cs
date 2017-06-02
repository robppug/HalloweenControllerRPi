using System;
using System.Collections.Generic;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.BusDevices
{
   public class DeviceComms_I2C : IDeviceComms
   {
      private I2cDevice _i2cDevice;
      private List<byte> _rxData;

      public event EventHandler<EventArgs> DataReceived;

      public I2cDevice i2cDevice
      {
         get { return _i2cDevice; }
      }

      public List<byte> rxData
      {
         get { return _rxData; }
      }

      public DeviceComms_I2C(I2cDevice dev)
      {
         _i2cDevice = dev;
         _rxData = new List<byte>();
      }

      public virtual void Write(byte[] buffer)
      {
         _i2cDevice.Write(buffer);
      }

      public virtual byte[] Read(ushort bytes)
      {
         byte[] rxData = new byte[bytes];

         _rxData.Clear();

         _i2cDevice.Read(rxData);

         _rxData.AddRange(rxData);

         OnDataReceivedEvent(this, EventArgs.Empty);

         return rxData;
      }

      public void OnDataReceivedEvent(object sender, EventArgs e)
      {
         DataReceived?.Invoke(sender, e);
      }
   }
}
