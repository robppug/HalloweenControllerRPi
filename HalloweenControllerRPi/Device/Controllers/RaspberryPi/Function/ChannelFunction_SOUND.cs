using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function
{
   public class ChannelFunction_SOUND : IChannel
   {
      public delegate void EventHandlerSound(object sender, EventArgs e);

      public event EventHandlerSound SoundStart;

      public IHat HostHat
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public uint Index
      {
         get
         {
            throw new NotImplementedException();
         }

         set
         {
            throw new NotImplementedException();
         }
      }

      public uint Level
      {
         get
         {
            throw new NotImplementedException();
         }

         set
         {
            throw new NotImplementedException();
         }
      }
   }
}
