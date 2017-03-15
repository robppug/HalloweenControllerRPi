using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   class BusDevice_PCA9685 : II2CBusDevice, IBusDevicePwmChannelProvider
   {
      /// <summary>
      /// PCA9685 register addresses
      /// </summary>
      private enum Registers : byte
      {
         MODE1 = 0x00,
         MODE2 = 0x01,
         SUBADR1 = 0x02,
         SUBADR2 = 0x03,
         SUBADR3 = 0x04,
         ALLCALLADR = 0x05,
         PIN0_ON_L = 0x06,
         PIN0_ON_H = 0x07,
         PIN0_OFF_L = 0x08,
         PIN0_OFF_H = 0x09,
         PIN1_ON_L = 0x0A,
         PIN1_ON_H = 0x0B,
         PIN1_OFF_L = 0x0C,
         PIN1_OFF_H = 0x0D,
         PIN2_ON_L = 0x0E,
         PIN2_ON_H = 0x0F,
         PIN2_OFF_L = 0x10,
         PIN2_OFF_H = 0x11,
         PIN3_ON_L = 0x12,
         PIN3_ON_H = 0x13,
         PIN3_OFF_L = 0x14,
         PIN3_OFF_H = 0x15,
         PIN4_ON_L = 0x16,
         PIN4_ON_H = 0x17,
         PIN4_OFF_L = 0x18,
         PIN4_OFF_H = 0x19,
         PIN5_ON_L = 0x1A,
         PIN5_ON_H = 0x1B,
         PIN5_OFF_L = 0x1C,
         PIN5_OFF_H = 0x1D,
         PIN6_ON_L = 0x1E,
         PIN6_ON_H = 0x1F,
         PIN6_OFF_L = 0x20,
         PIN6_OFF_H = 0x21,
         PIN7_ON_L = 0x22,
         PIN7_ON_H = 0x23,
         PIN7_OFF_L = 0x24,
         PIN7_OFF_H = 0x25,
         PIN8_ON_L = 0x26,
         PIN8_ON_H = 0x27,
         PIN8_OFF_L = 0x28,
         PIN8_OFF_H = 0x29,
         PIN9_ON_L = 0x2A,
         PIN9_ON_H = 0x2B,
         PIN9_OFF_L = 0x2C,
         PIN9_OFF_H = 0x2D,
         PIN10_ON_L = 0x2E,
         PIN10_ON_H = 0x2F,
         PIN10_OFF_L = 0x30,
         PIN10_OFF_H = 0x31,
         PIN11_ON_L = 0x32,
         PIN11_ON_H = 0x33,
         PIN11_OFF_L = 0x34,
         PIN11_OFF_H = 0x35,
         PIN12_ON_L = 0x36,
         PIN12_ON_H = 0x37,
         PIN12_OFF_L = 0x38,
         PIN12_OFF_H = 0x39,
         PIN13_ON_L = 0x3A,
         PIN13_ON_H = 0x3B,
         PIN13_OFF_L = 0x3C,
         PIN13_OFF_H = 0x3D,
         PIN14_ON_L = 0x3E,
         PIN14_ON_H = 0x3F,
         PIN14_OFF_L = 0x40,
         PIN14_OFF_H = 0x41,
         PIN15_ON_L = 0x42,
         PIN15_ON_H = 0x43,
         PIN15_OFF_L = 0x44,
         PIN15_OFF_H = 0x45,
         ALL_LED_ON_L = 0xFA,
         ALL_LED_ON_H = 0xFB,
         ALL_LED_OFF_L = 0xFC,
         ALL_LED_OFF_H = 0xFD,
         PRESCALE = 0xFE,
      };

      public static uint NumberOfChannels
      {
         get { return 16; }
      }

      private I2cDevice m_i2cDevice;
      private List<IChannel> m_Channels;

      public List<IChannel> Channels
      {
         get { return m_Channels; }
      }

      public bool Initialised { get; private set; }

      public BusDevice_PCA9685()
      {
         Initialised = false;
      }

      public void Open(I2cDevice i2cDevice)
      {
         if (Initialised == false)
         {
            m_i2cDevice = i2cDevice;

            /* Set MODE 1 Register - Change to NORMAL mode */
            SetRegister((byte)Registers.MODE1, 0x00);

            Task.Delay(1);

            /* Set MODE 2 Register */
            //i2cDevice.Write(new byte[2] { 0x00, 0x00 });

            //await Task.Delay(1);

            /* Set MODE 1 Register - Change to SLEEP mode */
            SetRegister((byte)Registers.MODE1, 0x90);

            /* Adjust the PWM Frequency - 1526Hz - Must be before being set to NORMAL mode */
            SetPWMFrequency(0x03);

            /* Set MODE 1 Register - Change to NORMAL mode */
            SetRegister((byte)Registers.MODE1, 0x00);
         }
         else
         {
            throw new Exception("Bus Device (" + this + ") is already Open.");
         }
      }

      public void Close()
      {
         if (Initialised == true)
         {
            Initialised = false;
         }
      }


      /// <summary>
      /// Initialises the available CHANNELS provided by the BusDevice.
      /// </summary>
      public void InitialiseChannels()
      {
         m_Channels = new List<IChannel>();

         /* Initialise PWM channels */
         for (uint i = 0; i < NumberOfChannels; i++)
         {
            m_Channels.Add(new ChannelFunction_PWM(i));

            SetChannel((ushort)m_Channels[(int)i].Index, 0x00);
         }

         Initialised = true;
      }

      public void SetChannel(ushort channel, ushort value)
      {
         if((channel >= NumberOfChannels) || (channel >= Channels.Count))
         {
            throw new Exception("Requested CHANNEL #" + channel + " is out of range (MAX #" + Channels.Count + ")");
         }

         m_i2cDevice.Write(new byte[2] { (byte)((byte)Registers.PIN1_ON_L + ((byte)channel * 4)), 0x00 });
         m_i2cDevice.Write(new byte[2] { (byte)((byte)Registers.PIN1_ON_H + ((byte)channel * 4)), 0x00 });
         m_i2cDevice.Write(new byte[2] { (byte)((byte)Registers.PIN1_OFF_L + ((byte)channel * 4)), (byte)(value & 0xFF) });
         m_i2cDevice.Write(new byte[2] { (byte)((byte)Registers.PIN1_OFF_H + ((byte)channel * 4)), (byte)((value >> 8) & 0xFF) });
      }

      public void SetRegister(byte register, byte value)
      {
         m_i2cDevice.Write(new byte[2] { register, value });
      }

      public void SetPWMFrequency(byte value)
      {
         m_i2cDevice.Write(new byte[2] { (byte)Registers.PRESCALE, value });
      }
   }
}