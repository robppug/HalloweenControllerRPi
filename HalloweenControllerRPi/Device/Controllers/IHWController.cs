using System;

namespace HalloweenControllerRPi.Device.Controllers
{
   public interface IHWController : ISupportedFunctions
   {
      void Connect();
      void Disconnect();

      void OnChannelNotification(object sender, CommandEventArgs e);
   }
}
