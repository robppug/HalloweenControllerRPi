using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   public class RPiHat_DISPLAY_v1 : RPiHat
   {
      IDisplayProvider displayDriver;

      public RPiHat_DISPLAY_v1(IHWController host) : base(host)
      {
      }

      public override void RefreshChannel(IChannel chan)
      {
      }
   }
}
