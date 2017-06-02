using HalloweenControllerRPi.Attributes;
using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Device.Controllers.Providers;
using HalloweenControllerRPi.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.BusDevices
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
      THR = 0x10,  // Xmit Holding Register is 0x00 in WRITE Mode 
      [AccessType("RW")]
      DLL = 0x20,  // Divisor Latch LSB  0x00 
      [AccessType("RW")]
      DLH = 0x01,  // Divisor Latch MSB  0x01 
      [AccessType("RW")]
      IER = 0x11,  // Interrupt Enable Register 
      [AccessType("RW")]
      EFR = 0x02,  // Enhanced Function Register 
      [AccessType("R")]
      IIR = 0x12,  // Interrupt Identification Register in READ Mode 
      [AccessType("W")]
      FCR = 0x22,  // FIFO Control Register in WRITE Mode 
      [AccessType("RW")]
      LCR = 0x03,  // Line Control Register 
      [AccessType("RW")]
      MCR = 0x04,  // Modem Control Register 
      [AccessType("R")]
      LSR = 0x05,  // Line status Register 
      [AccessType("R")]
      MSR = 0x06,  // Modem Status Register 
      [AccessType("RW")]
      TCR = 0x16,  // Transmission Control Register 
      [AccessType("RW")]
      SPR = 0x07,  // ScratchPad Register 
      [AccessType("RW")]
      TLR = 0x17,  // Trigger Level Register (FIFO Interrupt Trigger)
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
      EFCR = 0x0F,  // Extra Features Control Register 
      REG_VAL_MASK = 0x0F
   };

   /// <summary>
   /// Register ENUM extension class
   /// </summary>
   internal static class RegisterEnumExtensions
   {
      public static bool CanRead(this Registers reg)
      {
         AccessTypeAttribute mde = EnumExtension<AccessTypeAttribute, Registers>.GetModeAttribute(reg);
         if (mde != null)
            return mde.Read;
         else
            return true;
      }

      public static bool CanWrite(this Registers reg)
      {
         AccessTypeAttribute mde = EnumExtension<AccessTypeAttribute, Registers>.GetModeAttribute(reg);
         if (mde != null)
            return mde.Write;
         else
            return true;
      }

      public static byte GetRegisterValue(this Registers reg)
      {
         return (byte)(reg & Registers.REG_VAL_MASK);
      }
   }


   public class BusDeviceStreamEventArgs_SC16IS752 : EventArgs
   {
      public int Channel;
      List<byte> DataPacket;

      public BusDeviceStreamEventArgs_SC16IS752(int chan, List<byte> data)
      {
         DataPacket = data;
         Channel = chan;
      }
   }

   public class BusDeviceStream_SC16IS752 : DeviceComms_I2C
   {
      public int StreamIndex { get; set; }

      public BusDeviceStream_SC16IS752(I2cDevice dev, int channel) : base(dev)
      {
         StreamIndex = channel;
      }

      public override byte[] Read(ushort bytes)
      {
         byte[] data = new byte[bytes];

         rxData.Clear();

         i2cDevice.WriteRead(new byte[1] { (byte)(((byte)AccessType.READ << 7) | (Registers.RHR.GetRegisterValue() << 3) | ((byte)StreamIndex << 1)) }, data);

         rxData.AddRange(data);

         OnDataReceivedEvent(this, EventArgs.Empty);

         return data;
      }

      public override void Write(byte[] buffer)
      {
         List<byte> data = new List<byte>();

         data.Add((byte)(((byte)AccessType.WRITE << 7) | Registers.THR.GetRegisterValue() << 3 | ((byte)StreamIndex << 1)));
         data.AddRange(buffer);

         i2cDevice.Write(data.ToArray());
      }
   }


   public class BusDevice_SC16IS752<T> : IDeviceCommsProvider<T>, IChannelProvider, IGpioChannelProvider where T : IDeviceComms
   {
      private T _stream;
      private List<BusDeviceStream_SC16IS752> _uartStreams;
      private uint _preScaler = 1; /* Default of 1 (MCR[7] set to 0 - Divide-by-1 clock) */
      private BackgroundWorker bgRxTask;
      private ObservableCollection<BusDeviceStreamEventArgs_SC16IS752> _rxDataFifo;

      #region EVENTS
      //public event EventHandler<BusDeviceStreamEventArgs_SC16IS752> StreamPacketReceived;
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
         _rxDataFifo = new ObservableCollection<BusDeviceStreamEventArgs_SC16IS752>();
         //_rxDataFifo.CollectionChanged += _rxDataFifo_CollectionChanged;
         bgRxTask = new BackgroundWorker();
      }

      private async void BgRxTask_DoWork(object sender, DoWorkEventArgs e)
      {
         List<byte> rxData = new List<byte>();

         while(Initialised)
         {
            foreach(BusDeviceStream_SC16IS752 bus in _uartStreams)
            {
               ReadRegister(bus, Registers.LSR, ref rxData);

               if((rxData[0] & 0x01) == 0x01)
               {
                  rxData.Clear();
                  //There is some data in the RHR Register
                  ReadRegister(bus, Registers.RXLVL, ref rxData);

                  rxData = ReadBytes(bus, rxData[0]);

                  _rxDataFifo.Add(new BusDeviceStreamEventArgs_SC16IS752(bus.StreamIndex, rxData));
               }
               rxData.Clear();
            }

            await Task.Delay(10);
         }
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
         bgRxTask.CancelAsync();
      }

      public void InitialiseDriver(bool proceedOnFail = false)
      {
         BusDeviceStream_SC16IS752 streamBusDevice;
         BaudRates baudRate = BaudRates.Baud_9600bps;
         ushort divisor;

         if (Initialised == true)
         {
            //Channel A Setup
            streamBusDevice = new BusDeviceStream_SC16IS752((_stream as DeviceComms_I2C).i2cDevice, (int)UartChannels.ChannelA);
            //streamBusDevice.DataReceived += StreamBusDevice_DataReceived;
            _uartStreams.Add(streamBusDevice);
            divisor = CalculateDivisor(baudRate);

            //Prescaler in MCR defaults on MCU reset to the value of 1 
            WriteRegister(streamBusDevice, Registers.LCR, 0x80); // 0x80 to program baud rate divisor 
            WriteRegister(streamBusDevice, Registers.DLL, (byte)(divisor & 0xFF)); // 9600 with X1=11.0592MHz = 11059200/(9600*16) = 72 (0x0048)
            WriteRegister(streamBusDevice, Registers.DLH, (byte)((divisor >> 8) & 0xFF)); // 
            WriteRegister(streamBusDevice, Registers.LCR, 0xBF); // access EFR register 
            WriteRegister(streamBusDevice, Registers.EFR, 0X10); // enable enhanced registers 
            WriteRegister(streamBusDevice, Registers.LCR, 0x03); // 8 data bits, 1 stop bit, no parity 
            WriteRegister(streamBusDevice, Registers.FCR, 0x07); // reset TXFIFO, reset RXFIFO, ENABLE FIFO mode 

            //Channel B Setup
            streamBusDevice = new BusDeviceStream_SC16IS752((_stream as DeviceComms_I2C).i2cDevice, (int)UartChannels.ChannelB);
            //streamBusDevice.DataReceived += StreamBusDevice_DataReceived;
            _uartStreams.Add(streamBusDevice);
            divisor = CalculateDivisor(baudRate);

            //Prescaler R defauin MClts on MCU reset to the value of 1 
            WriteRegister(streamBusDevice, Registers.LCR, 0x80); // 0x80 to program baud rate divisor 
            WriteRegister(streamBusDevice, Registers.DLL, (byte)(divisor & 0xFF)); // 9600 with X1=11.0592MHz = 11059200/(9600*16) = 72 (0x0048)
            WriteRegister(streamBusDevice, Registers.DLH, (byte)((divisor >> 8) & 0xFF)); // 
            WriteRegister(streamBusDevice, Registers.LCR, 0xBF); // access EFR register 
            WriteRegister(streamBusDevice, Registers.EFR, 0X10); // enable enhanced registers 
            WriteRegister(streamBusDevice, Registers.LCR, 0x03); // 8 data bits, 1 stop bit, no parity 
            WriteRegister(streamBusDevice, Registers.FCR, 0x07); // reset TXFIFO, reset RXFIFO, ENABLE FIFO mode 

            bgRxTask.DoWork += BgRxTask_DoWork;
            bgRxTask.RunWorkerAsync();
         }
         else
         {
            throw new Exception("Driver needs to be Opened first.");
         }
      }


      public void RefreshChannel(IChannel chan)
      {
         List<byte> data = new List<byte>();

         ReadRegister(_uartStreams[(int)chan.Index], Registers.LSR, ref data);

         /* Is DATA waiting? */
         if ((byte)(data[0] & 0x01) == 0x01)
         {
            ReadRegister(_uartStreams[(int)chan.Index], Registers.RHR, ref data);

            //DataReceived?.Invoke(this, EventArgs.Empty);
         }
      }

      private void ReadRegister(BusDeviceStream_SC16IS752 bus, Registers reg, ref List<byte> data)
      {
         byte[] rxData = new byte[1];

         if (reg.CanRead() == false)
            throw new Exception("Register " + reg.ToString() + " is WRITE only!");

         bus.Write(new byte[1] { (byte)(((byte)AccessType.READ << 7) | ((byte)reg.GetRegisterValue() << 3) | ((byte)bus.StreamIndex << 1)) });

         data.AddRange(bus.Read(1));
      }

      private void WriteRegister(BusDeviceStream_SC16IS752 bus, Registers reg, byte data)
      {
         if (reg.CanWrite() == false)
            throw new Exception("Register " + reg.ToString() + " is READ only!");

         bus.Write(new byte[2] { (byte)(((byte)AccessType.WRITE << 7) | ((byte)reg.GetRegisterValue() << 3) | ((byte)bus.StreamIndex << 1)), (byte)data });
      }

      public byte ReadByte(BusDeviceStream_SC16IS752 bus)
      {
         List<byte> data = new List<byte>();

         ReadRegister(bus, Registers.RHR, ref data);

         return data[0];
      }

      public List<byte> ReadBytes(BusDeviceStream_SC16IS752 bus, ushort length)
      {
         List<byte> data = new List<byte>();

         for (int i = 0; i < length; i++)
         {
            ReadRegister(bus, Registers.RHR, ref data);
         }

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
