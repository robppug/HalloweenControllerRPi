using static HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevice_PCA9685;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Channels
{
   interface IPwmChannelProvider
   {
      uint NumberOfPwmChannels { get; }

      void SetChannel(ushort channel, ushort value);
      void SetRegister(Registers register, byte value);
   }
}