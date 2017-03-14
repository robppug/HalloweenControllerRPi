namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   interface IBusDeviceRelayChannelProvider
   {
      void SetChannel(ushort channel, bool value);
      void SetRegister(byte register, byte value);
   }
}