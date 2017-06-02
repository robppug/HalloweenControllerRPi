namespace HalloweenControllerRPi.Device.Controllers.Channels
{
   public interface IChannel
   {
      IChannelHost ChannelHost { get; }
      uint Index { get; set; }

      object GetValue();
   }
}