using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device
{
   public interface IHWInterface
   {
      void GetBoardType();
      void Connect();
      void Disconnect();
   }
}
