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
            m_i2cDevice = null;

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

      /// <summary>
      /// Write's to a PIN on the PCA9501 device.
      /// </summary>
      /// <param name="pin"></param>
      /// <param name="value"></param>
      public void WritePin(IIOPin pin, GpioPinValue value)
      {
         IIOPin gpioPin = m_GpioPins.Find(x => x == pin);

         if (gpioPin.GetDriveMode() == GpioPinDriveMode.Output)
         {
            gpioPin.Write(value);

            UpdateDeviceIO(Registers.WRITE_IO);
         }
         else
         {
            throw new Exception("Pin is READ ONLY");
         }
      }

      public GpioPinValue ReadPin(IIOPin pin)
      {
         UpdateDeviceIO(Registers.READ_IO);

         return m_GpioPins.Find(x => x == pin).Read();
      }

      private void UpdateDeviceIO(Registers reg)
      {
         byte[] data = new byte[NumberOfChannels];
         List<byte> dataBuffer = new List<byte>();

         switch (reg)
         {
            /* WRITE TO PIN */
            case Registers.WRITE_IO:
               foreach (IIOPin pin in m_GpioPins)
               {
                  data[pin.PinNumber] = (byte)pin.Read();
               }

               /* Write to the DEVICE - Address = b0xxxxxx1 */
               m_i2cDevice.ConnectionSettings.SlaveAddress |= (byte)reg;

               dataBuffer.Add((byte)reg);
               dataBuffer.AddRange(data);
               m_i2cDevice.Write(dataBuffer.ToArray());
               break;

            /* READ FROM PIN */
            case Registers.READ_IO:
               /* Read from the DEVICE - Address = b0xxxxxx0 */
               m_i2cDevice.ConnectionSettings.SlaveAddress &= ~(byte)reg;
               m_i2cDevice.Read(data);

               foreach (IIOPin pin in m_GpioPins)
               {
                  pin.Write((GpioPinValue)data[pin.PinNumber]);
               }
               break;

            default:
               break;
         }

      }

      /// <summary>
      /// Will READ/WRITE the BUS device and physical update each Channel
      /// </summary>
      public void RefreshChannel(ushort chan)
      {
         IIOPin pin = GetPin(chan);

         if (pin.GetDriveMode() == GpioPinDriveMode.Input)
         {
            ReadPin(pin);
         }
         else if (pin.GetDriveMode() == GpioPinDriveMode.Output)
         {
            WritePin(pin, pin.Read());
         }
      }
      #endregion
   }
}