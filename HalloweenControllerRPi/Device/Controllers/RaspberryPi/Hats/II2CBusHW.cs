using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   interface II2CBusHW
   {
      Task Open(I2cDevice i2cDevice);
      Task Close();
      void InitialiseChannels();
   }
}