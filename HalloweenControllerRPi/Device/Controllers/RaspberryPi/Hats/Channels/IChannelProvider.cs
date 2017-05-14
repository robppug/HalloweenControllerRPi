using System.Collections.Generic;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Channels
{
   public interface IChannelProvider
   {
      uint NumberOfChannels { get; }

      void InitialiseChannels();
      void RefreshChannel(IChannel chan);
   }
}