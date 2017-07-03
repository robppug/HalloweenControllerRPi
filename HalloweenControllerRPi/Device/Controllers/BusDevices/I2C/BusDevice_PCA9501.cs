using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Device.Controllers.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.BusDevices
{
   public class BusDevice_PCA9501<T> : IDeviceCommsProvider<T>, IChannelProvider, IGpioChannelProvider, IEepromChannelProvider where T : IDeviceComms
   {
      private T _stream;
      private List<IIOPin> m_GpioPins = new List<IIOPin>(8);

      public bool Initialised { get; private set; }  
      
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
         get { return NumberOfGpioChannels; }
      }

      public uint NumberOfGpioChannels
      {
         get { return (uint)m_GpioPins.Count; }
      }

      public ushort EEPROMSize
      {
         get { return 256; }
      }

      public T BusDeviceComms
      {
         get { return _stream; }
         private set { _stream = value; }
      }

      public BusDevice_PCA9501()
      {
         Initialised = false;
      }

      public void Open(T stream)
      {
         if (Initialised == false)
         {
            _stream = stream;
            Initialised = true;
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
            _stream = default(T);

            Initialised = false;
         }
      }

      /// <summary>
      /// Initialises the available CHANNELS provided by the BusDevice.
      /// </summary>
      public void InitialiseDriver(bool proceedOnFail = false)
      {
         /* Initialise GPIO channels */
         for (uint i = 0; i < 8; i++)
         {
            m_GpioPins.Add(new IOPin(i));
         }

         Initialised = true;
      }

      /// <summary>
      /// Will READ/WRITE the BUS device and physical update each Channel
      /// </summary>
      public void RefreshChannel(IChannel chan)
      {
         IIOPin pin = GetPin((ushort)chan.Index);

         if (pin.GetDriveMode() == GpioPinDriveMode.InputPullUp)
         {
            ReadPin(pin);
         }
         else if (pin.GetDriveMode() == GpioPinDriveMode.Output)
         {
            WritePin(pin, (GpioPinValue)chan.GetValue());
         }
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
         if (address < EEPROMSize)
         {
            /* Write the EEPROM_WRITE register, address and data */
            _stream.Write(new byte[3] { (byte)Registers.WRITE_EEPROM, (byte)address, data });
         }
      }

      public void WriteBytes(ushort address, List<byte> data)
      {
         foreach (byte b in data)
         {
            WriteByte((ushort)(address + data.IndexOf(b)), b);
         }
      }

      #endregion EEPROM Handling

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
         if (pin.GetDriveMode() == GpioPinDriveMode.Output)
         {
            pin.Write(value);

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
         //byte[] data = new byte[NumberOfChannels];
         byte[] data = new byte[1] { 0x00 };

         switch (reg)
         {
            /* WRITE TO PIN */
            case Registers.WRITE_IO:
               foreach (IIOPin pin in m_GpioPins)
               {
                  data[0] |= (byte)((byte)pin.Read() << (byte)pin.PinNumber);
               }

               /* Write to the DEVICE - Address = b0xxxxxx0 */
               (_stream as DeviceComms_I2C).i2cDevice.ConnectionSettings.SlaveAddress &= ~(byte)reg;

               _stream.Write(data);
               break;

            /* READ FROM PIN */
            case Registers.READ_IO:
               /* Read from the DEVICE - Address = b0xxxxxx1 */
               (_stream as DeviceComms_I2C).i2cDevice.ConnectionSettings.SlaveAddress |= (byte)reg;

               while ((_stream as DeviceComms_I2C).i2cDevice.ReadPartial(data).Status != I2cTransferStatus.FullTransfer) { }

               //Debug.WriteLine(Convert.ToString(data[0], 2).PadLeft(8, '0'));
               foreach (IIOPin pin in m_GpioPins)
               {
                  pin.Write((GpioPinValue)((data[0] >> (byte)(NumberOfChannels - (pin.PinNumber + 1))) & 0x01));
               }
               break;

            default:
               break;
         }
      }
      #endregion IO PIN Handling
   }
}