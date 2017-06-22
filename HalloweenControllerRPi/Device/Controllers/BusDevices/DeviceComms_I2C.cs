using System;
using System.Collections.Generic;
using Windows.Devices.I2c;
using Windows.UI.Xaml;

namespace HalloweenControllerRPi.Device.Controllers.BusDevices
{
   public class DeviceComms_I2C : IDeviceComms
   {
      protected object _Lock = new object();
      protected I2cDevice _i2cDevice;
      protected DispatcherTimer bgRxTask;

      public virtual event EventHandler<DeviceCommsEventArgs> DataReceived;

      public I2cDevice i2cDevice
      {
         get { return _i2cDevice; }
      }

      public bool DeviceReady { get; set; }

      public DeviceComms_I2C(I2cDevice dev)
      {
         _i2cDevice = dev;

         bgRxTask = new DispatcherTimer();
         bgRxTask.Tick += BgRxTask_Tick;
         bgRxTask.Interval = TimeSpan.FromMilliseconds(50);
         bgRxTask.Start();
      }

      protected virtual void BgRxTask_Tick(object sender, object e)
      {
         byte[] rxData;

         lock (_Lock)
         {
            if (DeviceReady)
            {
               rxData = Read();

               if (rxData != null)
               {
                  DataReceived?.Invoke(this, new DeviceCommsEventArgs(rxData));
               }
            }
         }
      }

      public virtual void Write(byte[] buffer)
      {
         _i2cDevice.Write(buffer);
      }

      public virtual byte[] Read(int bytes = 1)
      {
         byte[] rx = new byte[bytes];

         _i2cDevice.Read(rx);

         return rx;
      }

      public virtual byte[] WriteRead(byte[] txBuffer)
      {
         _i2cDevice.Write(txBuffer);

         return null;
      }
   }
}
