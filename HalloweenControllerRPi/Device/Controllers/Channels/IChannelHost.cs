namespace HalloweenControllerRPi.Device.Controllers.Channels
{
   public interface IChannelHost
   {
      IHWController HostController { get; }
      void UpdateChannel(IChannel chan);
   }
}