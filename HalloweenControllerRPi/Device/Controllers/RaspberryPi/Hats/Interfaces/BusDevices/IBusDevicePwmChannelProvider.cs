using static HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevice_PCA9685;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   interface IBusDevicePwmChannelProvider
   {
      void SetChannel(ushort channel, ushort value);
      void SetRegister(Registers register, byte value);
   }
}