using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HalloweenControllerRPi.Device.Controllers.RaspberryPi.ChannelFunction_INPUT;

namespace HalloweenControllerRPi.Device
{
   public interface IHWController : ISupportedFunctions
   {
      void Connect();
      void Disconnect();

      void OnInputChannelNotification(object sender, EventArgsINPUT e);
   }
}
