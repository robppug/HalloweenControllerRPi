using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device
{
   public interface ISupportedFunctions
   {
      uint Inputs { get; }
      uint PWMs { get; }
      uint Relays { get; }
   }
}
