using static HalloweenControllerRPi.Device.Controllers.Channels.ChannelFunction_INPUT;

namespace HalloweenControllerRPi.Device.Controllers
{
   public interface IHWController : ISupportedFunctions
   {
      void Connect();
      void Disconnect();

      void OnInputChannelNotification(object sender, EventArgsINPUT e);
   }
}
