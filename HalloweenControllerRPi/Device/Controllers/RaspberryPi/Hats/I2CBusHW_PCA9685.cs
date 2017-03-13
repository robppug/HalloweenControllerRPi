using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   class I2CBusHW_PCA9685 : II2CBusHW
   {
      public static uint NumberOfChannels
      {
         get { return 5; }
      }

      private I2cDevice m_i2cDevice;
      private List<IChannel> m_Channels;

      private byte[] bMODE1 = new byte[1] { 0x00 };
      private byte[] bMODE2 = new byte[1] { 0x01 };
      private byte[] bPWM_CLK = new byte[1] { 0xFE };
      private byte[] LED_ON_L = new byte[1] { 0x06 };
      private byte[] LED_ON_H = new byte[1] { 0x07 };
      private byte[] LED_OFF_L = new byte[1] { 0x08 };
      private byte[] LED_OFF_H = new byte[1] { 0x09 };

      public async Task Open(I2cDevice i2cDevice)
      { 
         m_i2cDevice = i2cDevice;

         /* Set MODE 1 Register - Change to NORMAL mode */
         SetMode1Register(0x00);

         await Task.Delay(1);

         /* Set MODE 2 Register */
         //i2cDevice.Write(new byte[2] { 0x00, 0x00 });

         //await Task.Delay(1);

         /* Set MODE 1 Register - Change to SLEEP mode */
         SetMode1Register(0x90);
         
         /* Adjust the PWM Frequency - 1526Hz - Must be before being set to NORMAL mode */
         SetPWMFrequency(0x03);

         /* Set MODE 1 Register - Change to NORMAL mode */
         SetMode1Register(0x00);
      }

      public async Task Close()
      {
         throw new NotImplementedException();
      }

      public void InitialiseChannels()
      {
         m_Channels = new List<IChannel>();

         /* Initialise PWM channels */
         for (uint i = 0; i < NumberOfChannels; i++)
         {
            m_Channels.Add(new Channel_PWM(i));

            m_i2cDevice.Write(new byte[2] { (byte)(LED_ON_L[0] + ((byte)m_Channels[(int)i].Index * 4)), 0x00 });
            m_i2cDevice.Write(new byte[2] { (byte)(LED_ON_H[0] + ((byte)m_Channels[(int)i].Index * 4)), 0x00 });
            m_i2cDevice.Write(new byte[2] { (byte)(LED_OFF_L[0] + ((byte)m_Channels[(int)i].Index * 4)), 0x00 });
            m_i2cDevice.Write(new byte[2] { (byte)(LED_OFF_H[0] + ((byte)m_Channels[(int)i].Index * 4)), 0x00 });
         }
      }

      public void SetChannel(ushort channel, ushort value)
      {
         throw new NotImplementedException();
      }

      public void SetMode1Register(byte value)
      {
         m_i2cDevice.Write(new byte[2] { (byte)bMODE1[0], value });
      }

      public void SetMode2Register(byte value)
      {
         m_i2cDevice.Write(new byte[2] { (byte)bMODE2[0], value });
      }

      public void SetPWMFrequency(byte value)
      {
         m_i2cDevice.Write(new byte[2] { (byte)bPWM_CLK[0], value });
      }
   }
}