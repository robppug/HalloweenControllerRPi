using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   /// <summary>
   /// Raspberry Pi HAT class for the Raspberry Pi 2/3
   /// </summary>
   public abstract class RPiHat : IHat, ISupportedFunctions
   {
      #region Declarations
      private UInt16 m_Address = 0;

      public List<IChannel> Channels
      {
         get;
      }

      public UInt16 Address
      {
         get { return m_Address; }
         set { m_Address = value; }
      }

      public virtual uint Inputs { get { return (uint)Channels.Count; } }
      public virtual uint PWMs { get { return (uint)Channels.Count; } }
      public virtual uint Relays { get { return (uint)Channels.Count; } }
      #endregion

      public virtual void Open() { }
      public virtual void Close() { }
   }
}
