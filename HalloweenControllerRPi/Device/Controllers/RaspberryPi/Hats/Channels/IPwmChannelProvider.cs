namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Channels
{
   interface IPwmChannelProvider
   {
      uint NumberOfPwmChannels { get; }

      void SetChannel(ushort channel, ushort value);
   }
}