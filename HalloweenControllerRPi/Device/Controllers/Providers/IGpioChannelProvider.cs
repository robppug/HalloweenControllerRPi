using HalloweenControllerRPi.Device.Controllers.Channels;
using Windows.Devices.Gpio;

namespace HalloweenControllerRPi.Device.Controllers.Providers
{
   interface IGpioChannelProvider
   {
      uint NumberOfGpioChannels { get; }

      IIOPin GetPin(ushort pin);
      void WritePin(IIOPin pin, GpioPinValue value);
      GpioPinValue ReadPin(IIOPin pin);
   }
}