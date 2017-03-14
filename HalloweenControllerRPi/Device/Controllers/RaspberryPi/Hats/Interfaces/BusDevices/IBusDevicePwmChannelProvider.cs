namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   interface IBusDevicePwmChannelProvider
   {
      void SetChannel(ushort channel, ushort value);
      void SetRegister(byte register, byte value);
   }
}