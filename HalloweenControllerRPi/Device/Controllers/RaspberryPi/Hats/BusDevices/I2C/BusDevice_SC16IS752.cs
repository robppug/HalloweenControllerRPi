using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevices;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Channels;
using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.SC16IS752;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.SC16IS752
{

   /// <summary>
   /// SC16IS752 Access Type (Read/Write)
   /// </summary>
   public enum AccessType : byte
   {
      WRITE = 0x00,
      READ = 0x01
   }

   /// <summary>
   /// SC16IS752 register sub-addresses
   /// </summary>
   public enum Registers
   {
      [AccessType("R")]
      RHR = 0x00,  // Recv Holding Register is 0x00 in READ Mode 
      [AccessType("W")]
      THR = 0x00,  // Xmit Holding Register is 0x00 in WRITE Mode 
      [AccessType("RW")]
      DLL = 0x00,  // Divisor Latch LSB  0x00 
      [AccessType("RW")]
      DLH = 0x01,  // Divisor Latch MSB  0x01 
      [AccessType("RW")]
      IER = 0x01,  // Interrupt Enable Register 
      [AccessType("RW")]
      EFR = 0x02,  // Enhanced Function Register 
      [AccessType("R")]
      IIR = 0x02,  // Interrupt Identification Register in READ Mode 
      [AccessType("W")]
      FCR = 0x02,  // FIFO Control Register in WRITE Mode 
      [AccessType("RW")]
      LCR = 0x03,  // Line Control Register 
      [AccessType("RW")]
      MCR = 0x04,  // Modem Control Register 
      [AccessType("R")]
      LSR = 0x05,  // Line status Register 
      [AccessType("R")]
      MSR = 0x06,  // Modem Status Register 
      [AccessType("RW")]
      SPR = 0x07,  // ScratchPad Register 
      [AccessType("RW")]
      TCR = 0x06,  // Transmission Control Register 
      [AccessType("RW")]
      TLR = 0x07,  // Trigger Level Register (FIFO Interrupt Trigger)
      [AccessType("R")]
      TXLVL = 0x08,  // Xmit FIFO Level Register 
      [AccessType("R")]
      RXLVL = 0x09,  // Recv FIFO Level Register 
      [AccessType("RW")]
      IODir = 0x0A,  // I/O Pins Direction Register 
      [AccessType("RW")]
      IOState = 0x0B,  // I/O Pins State Register 
      [AccessType("RW")]
      IOIntEna = 0x0C,  // I/O Interrupt Enable Register 
      [AccessType("RW")]
      IOControl = 0x0E,  // I/O Pins Control Register 
      [AccessType("RW")]
      EFCR = 0x0F  // Extra Features Control Register 
   };

   /// <summary>
   /// Enum Attribute - Read/Write Flags
   /// </summary>
   internal class AccessTypeAttribute : Attribute
   {
      private string v;

      public bool Read
      {
         get { return this.v.Contains("R"); }
      }
      public bool Write
      {
         get { return this.v.Contains("W"); }
      }
      public AccessTypeAttribute(string v)
      {
         this.v = v;
      }
   }

   /// <summary>
   /// Register ENUM extension class
   /// </summary>
   internal static class RegisterEnumExtensions
   {
      private static AccessTypeAttribute GetModeAttribute(Registers reg)
      {
         Type enumType = typeof(Registers);
         AccessTypeAttribute mde = enumType.GetTypeInfo().GetCustomAttribute<AccessTypeAttribute>();
         return mde;
      }

      public static bool CanRead(this Registers reg)
      {
         AccessTypeAttribute mde = GetModeAttribute(reg);
         if (mde != null)
            return mde.Read;
         else
            return true;
      }

      public static bool CanWrite(this Registers reg)
      {
         AccessTypeAttribute mde = GetModeAttribute(reg);
         if (mde != null)
            return mde.Write;
         else
            return true;
      }
   }

   public class BusDeviceStream_SC16IS752 : DeviceComms_I2C
   {
      public int StreamIndex { get; set; }

      public BusDeviceStream_SC16IS752(I2cDevice dev, int channel) : base(dev)
      {
         StreamIndex = channel;
      }

      public override int Read(byte[] buffer)
      {
         throw new NotImplementedException();
      }

      public override void Write(byte[] buffer)
      {
         foreach (byte b in buffer)
         {
            i2cDevice.Write(new byte[2] { (byte)(((byte)AccessType.WRITE << 7) | ((byte)Registers.THR << 3) | ((byte)StreamIndex << 1)), b });
         }
      }
   }
   
   public class BusDevice_SC16IS752<T> : IDeviceCommsProvider<T>, IChannelProvider, IGpioChannelProvider where T : IDeviceComms
   {
      private T _stream;
      private List<BusDeviceStream_SC16IS752> _uartStreams;
      private uint _preScaler = 1; /* Default of 1 (MCR[7] set to 0 - Divide-by-1 clock) */

      #region EVENTS
      public event EventHandler DataReceived;
      #endregion

      #region PROPERTIES
      public bool Initialised { get; private set; }

      /// <summary>
      /// SC16IS752 UART Channels
      /// </summary>
      public enum UartChannels : byte
      {
         ChannelA = 0x00,
         ChannelB = 0x01
      }
            
      public uint CrystalFreq { get; set; } = 11059200; /* Default of 11.0592Mhz */
      
      public T BusDeviceComms
      {
         get { return _stream; }
         private set { _stream = value; }
      }

      public List<BusDeviceStream_SC16IS752> UARTStreams
      {
         get { return _uartStreams; }
      }

      public uint NumberOfChannels
      {
         get { return (uint)_uartStreams.Count + NumberOfGpioChannels; }
      }

      public uint NumberOfUARTChannels
      {
         get { return (uint)_uartStreams.Count; }
      }

      public uint NumberOfGpioChannels
      {
         get { return 8; }
      }
      #endregion

      private ushort CalculateDivisor(BaudRates rate)
      {
         return (ushort)((CrystalFreq / _preScaler) / ((uint)rate * 16));
      }

      #region PUBLIC Methods
      public BusDevice_SC16IS752()
      {
          _uartStreams = new List<BusDeviceStream_SC16IS752>(2);
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
         Initialised = false;
         _stream = default(T);
      }

      public void InitialiseDriver(bool proceedOnFail = false)
      {
         BaudRates baudRate = BaudRates.Baud_9600bps;
         ushort divisor;

         if (Initialised == true)
         {
            //Channel A Setup
            _uartStreams.Add(new BusDeviceStream_SC16IS752((_stream as DeviceComms_I2C).i2cDevice, (int)UartChannels.ChannelA));
            divisor = CalculateDivisor(baudRate);

            //Prescaler in MCR defaults on MCU reset to the value of 1 
            WriteRegister(UartChannels.ChannelA, Registers.LCR, 0x80); // 0x80 to program baud rate divisor 
            WriteRegister(UartChannels.ChannelA, Registers.DLL, (byte)(divisor & 0xFF)); // 9600 with X1=11.0592MHz = 11059200/(9600*16) = 72 (0x0048)
            WriteRegister(UartChannels.ChannelA, Registers.DLH, (byte)((divisor >> 8) & 0xFF)); // 
            WriteRegister(UartChannels.ChannelA, Registers.LCR, 0xBF); // access EFR register 
            WriteRegister(UartChannels.ChannelA, Registers.EFR, 0X10); // enable enhanced registers 
            WriteRegister(UartChannels.ChannelA, Registers.LCR, 0x03); // 8 data bits, 1 stop bit, no parity 
            WriteRegister(UartChannels.ChannelA, Registers.FCR, 0x07); // reset TXFIFO, reset RXFIFO, ENABLE FIFO mode 

            //Channel B Setup
            _uartStreams.Add(new BusDeviceStream_SC16IS752((_stream as DeviceComms_I2C).i2cDevice, (int)UartChannels.ChannelB));
            divisor = CalculateDivisor(baudRate);

            //Prescaler R defauin MClts on MCU reset to the value of 1 
            WriteRegister(UartChannels.ChannelB, Registers.LCR, 0x80); // 0x80 to program baud rate divisor 
            WriteRegister(UartChannels.ChannelB, Registers.DLL, (byte)(divisor & 0xFF)); // 9600 with X1=11.0592MHz = 11059200/(9600*16) = 72 (0x0048)
            WriteRegister(UartChannels.ChannelB, Registers.DLH, (byte)((divisor >> 8) & 0xFF)); // 
            WriteRegister(UartChannels.ChannelB, Registers.LCR, 0xBF); // access EFR register 
            WriteRegister(UartChannels.ChannelB, Registers.EFR, 0X10); // enable enhanced registers 
            WriteRegister(UartChannels.ChannelB, Registers.LCR, 0x03); // 8 data bits, 1 stop bit, no parity 
            WriteRegister(UartChannels.ChannelB, Registers.FCR, 0x07); // reset TXFIFO, reset RXFIFO, ENABLE FIFO mode 
         }
         else
         {
            throw new Exception("Driver needs to be Opened first.");
         }
      }

      public void RefreshChannel(IChannel chan)
      {
         List<byte> data = new List<byte>();

         ReadRegister((UartChannels)chan.Index, Registers.LSR, ref data);

         /* Is DATA waiting? */
         if ((byte)(data[0] & 0x01) == 0x01)
         {
            ReadRegister((UartChannels)chan.Index, Registers.RHR, ref data);

            DataReceived?.Invoke(this, EventArgs.Empty);
         }
      }

      public void ReadRegister(UartChannels uartChan, Registers reg, ref List<byte> data)
      {
         if (reg.CanRead() == false)
            throw new Exception("Register " + reg.ToString() + " is WRITE only!");

         _stream.Write(new byte[1] { (byte)(((byte)AccessType.READ << 7) | ((byte)reg << 3) | ((byte)uartChan << 1)) });

         _stream.Read(data.ToArray());
      }

      private void WriteRegister(UartChannels uartChan, Registers reg, byte data)
      {
         if (reg.CanWrite() == false)
            throw new Exception("Register " + reg.ToString() + " is READ only!");

         _stream.Write(new byte[2] { (byte)(((byte)AccessType.WRITE << 7) | ((byte)reg << 3) | ((byte)uartChan << 1)), (byte)data });
      }
      public byte ReadByte(UartChannels chan)
      {
         List<byte> data = new List<byte>();

         ReadRegister(chan, Registers.RHR, ref data);

         return data[0];
      }

      public List<byte> ReadBytes(UartChannels chan, ushort length)
      {
         List<byte> data = new List<byte>();

         ReadRegister(chan, Registers.RHR, ref data);

         return data;
      }

      public IIOPin GetPin(ushort pin)
      {
         throw new NotImplementedException();
      }

      public void WritePin(IIOPin pin, GpioPinValue value)
      {
         throw new NotImplementedException();
      }

      public GpioPinValue ReadPin(IIOPin pin)
      {
         throw new NotImplementedException();
      }

      #endregion
   }
}
