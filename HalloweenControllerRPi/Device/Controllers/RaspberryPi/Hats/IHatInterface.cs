using System;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   interface IHatInterface
   {
      Task Open();
      Task Close();
      void Write(byte[] data);
      void Read(out byte[] data);
   }
}