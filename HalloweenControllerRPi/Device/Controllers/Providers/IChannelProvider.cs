using HalloweenControllerRPi.Device.Controllers.Channels;

namespace HalloweenControllerRPi.Device.Controllers.Providers
{ 
   public interface IChannelProvider
   {
      uint NumberOfChannels { get; }

      void RefreshChannel(IChannel chan);
   }
}