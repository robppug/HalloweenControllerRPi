using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices;
using Windows.Devices.Gpio;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Channels
{
   interface IGpioChannelProvider
   {
      uint NumberOfGpioChannels { get; }

      IIOPin GetPin(ushort pin);
      void WritePin(IIOPin pin, GpioPinValue value);
      GpioPinValue ReadPin(IIOPin pin);
   }
}