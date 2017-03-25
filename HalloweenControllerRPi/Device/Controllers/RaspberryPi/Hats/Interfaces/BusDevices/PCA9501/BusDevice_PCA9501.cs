using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
using System;
using System.Collections.Generic;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   class BusDevice_PCA9501 : II2CBusDevice, IBusDeviceGpioChannelProvider, IBusDeviceEEPROMChannelProvider
   {
      /// <summary>
      /// PCA9685 register addresses
      /// </summary>
      public enum Registers : byte
      {
         WRITE_IO = 0x00,     //b0xxxxxx0
         READ_IO = 0x01,      //b0xxxxxx1
         WRITE_EEPROM = 0x40, //b1xxxxxx0
         READ_EEPROM = 0x41,  //b1xxxxxx1
      };

      public uint NumberOfChannels
      {
         get { return 0x08; }
      }

      private I2cDevice m_i2cDevice;
      private List<IIOPin> m_GpioPins;
      
      public bool Initialised { get; private set; }

      public ushort EEPROMSize
      {
         get { return 256; }
      }

      public BusDevice_PCA9501()
      {
         Initialised = false;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="i2cDevice"></param>
      public void Open(I2cDevice i2cDevice)
      {
         if (Initialised == false)
         {
            m_i2cDevice = i2cDevice;
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
         /* Create a list of GPIO objects */
         m_GpioPins = new List<IIOPin>();

         /* Initialise GPIO channels */
         for (uint i = 0; i < NumberOfChannels; i++)
         {
            m_GpioPins.Add(new IOPin_PCA9501(i));
         }

         Initialised = true;
      }

      #region EEPROM Handling
      public byte ReadByte(ushort address)
      {
         throw new NotImplementedException();
      }

      public List<byte> ReadBytes(ushort address, ushort length)
      {
         throw new NotImplementedException();
      }

      public void WriteByte(ushort address, byte data)
      {
         if (m_i2cDevice != null)
         {
            if (address < EEPROMSize)
            {
               /* Write the EEPROM_WRITE register, address and data */
               m_i2cDevice.Write(new byte[3] { (byte)Registers.WRITE_EEPROM, (byte)address, data });
            }
         }
         else
         {
            throw new Exception("I2C Device not initialised.");
         }
      }

      public void WriteBytes(ushort address, List<byte> data)
      {
         if (m_i2cDevice != null)
         {
            foreach(byte b in data)
            {
               WriteByte((ushort)(address + data.IndexOf(b)), b);
            }
         }
         else
         {
            throw new Exception("I2C Device not initialised.");
         }
      }
      #endregion  

      #region IO PIN Handling
      public IIOPin GetPin(ushort pin)
      {
         if (pin < NumberOfChannels)
         {
            return m_GpioPins[pin];
         }

         return null;
      }

      public void WritePin(IIOPin pin, GpioPinValue value)
      {
         m_GpioPins.Find(x => x == pin).Write(value);

         UpdateDeviceIO();
      }

      public GpioPinValue ReadPin(IIOPin pin)
      {
         UpdateDeviceIO();

         return m_GpioPins.Find(x => x == pin).Read();
      }

      public void UpdateDeviceIO()
      {
         byte[] data = new byte[1 + NumberOfChannels];

         data[0] = (byte)Registers.WRITE_IO;

         foreach ( IIOPin pin in m_GpioPins )
         {
            data[pin.PinNumber + 1] = (byte)pin.Read();
         }

         m_i2cDevice.Write(data);
      }
      #endregion
   }
}