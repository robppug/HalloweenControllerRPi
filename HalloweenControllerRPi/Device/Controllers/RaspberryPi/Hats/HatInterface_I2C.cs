using System;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   class HatInterface_I2C : IHatInterface
   {
      private UInt16 m_Address = 0;
      private I2cDevice m_I2CDevice;
      private II2CBusHW m_I2CBusHW;

      public UInt16 Address
      {
         get { return m_Address; }
      }

      public HatInterface_I2C(I2cDevice i2cDevice, UInt16 i2cAddress, II2CBusHW busHW)
      {
         m_I2CDevice = i2cDevice;
         m_Address = i2cAddress;
         m_I2CBusHW = busHW;
      }

      public async Task Open()
      {
         await m_I2CBusHW.Open(m_I2CDevice);

         m_I2CBusHW.InitialiseChannels();
      }

      public async Task Close()
      {
         throw new NotImplementedException();
      }

      public void Read(out byte[] data)
      {
         throw new NotImplementedException();
      }

      public void Write(byte[] data)
      {
         m_I2CDevice.Write(data);
      }
   }
}