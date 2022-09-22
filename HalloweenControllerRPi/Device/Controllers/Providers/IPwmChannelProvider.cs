namespace HalloweenControllerRPi.Device.Controllers.Providers
{
    interface IPwmChannelProvider
    {
        uint NumberOfPwmChannels { get; }

        void SetChannel(ushort channel, ushort value);
    }
}