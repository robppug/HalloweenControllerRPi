using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   class HatInterface_I2C : IHatInterface
   {
      private UInt16 m_Address = 0;
      private I2cDevice m_I2CDevice;
      private II2CBusDevice m_I2CBusDevice;

      public UInt16 Address
      {
         get { return m_Address; }
      }

      public II2CBusDevice BusDevice
      {
         get { return m_I2CBusDevice; }
      }

      public HatInterface_I2C(I2cDevice i2cDevice, UInt16 i2cAddress, II2CBusDevice busDevice)
      {
         m_I2CDevice = i2cDevice;
         m_Address = i2cAddress;
         m_I2CBusDevice = busDevice;
      }

      public void Open()
      {
         /* Open the BUS DEVICE */
         m_I2CBusDevice.Open(m_I2CDevice);

         /* Initialise the BUS DEVICE */
         m_I2CBusDevice.InitialiseChannels();
      }

      public void Close()
      {
         throw new NotImplementedException();
      }
   }
}