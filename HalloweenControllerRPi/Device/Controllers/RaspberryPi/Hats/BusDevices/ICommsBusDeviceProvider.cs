using System.Collections.Generic;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevices
{
   public interface ICommsBusDeviceProvider
   {
      byte ReadByte();
      List<byte> ReadBytes(ushort length);
      void WriteByte(byte data);
      void WriteBytes(List<byte> data);
   }
}