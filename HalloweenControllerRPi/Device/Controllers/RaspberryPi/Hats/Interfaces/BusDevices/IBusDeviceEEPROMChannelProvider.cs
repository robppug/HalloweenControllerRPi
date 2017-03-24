using System;
using System.Collections.Generic;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   interface IBusDeviceEEPROMChannelProvider
   {
      UInt16 EEPROMSize { get; }

      byte ReadByte(ushort address);
      List<byte> ReadBytes(ushort address, ushort length);
      void WriteByte(ushort address, byte data);
      void WriteBytes(ushort address, List<byte> data);
   }
}