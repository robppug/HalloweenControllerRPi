using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   interface IHat
   {
      List<IChannel> Channels { get; }

      UInt16 Address { get; set; }
   }
}
