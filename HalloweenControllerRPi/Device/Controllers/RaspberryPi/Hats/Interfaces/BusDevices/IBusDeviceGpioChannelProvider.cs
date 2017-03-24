using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
using Windows.Devices.Gpio;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   interface IBusDeviceGpioChannelProvider
   {
      IIOPin GetPin(ushort pin);
      void WritePin(IIOPin pin, GpioPinValue value);
      GpioPinValue ReadPin(IIOPin pin);
   }
}