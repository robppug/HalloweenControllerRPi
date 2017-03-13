using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   /// <summary>
   /// Raspberry Pi HAT class for the Raspberry Pi 2/3
   /// </summary>
   public class RPiHat : IHat, ISupportedFunctions
   {
      #region Declarations
      private IHatInterface m_HatInterface;

      public List<IChannel> Channels
      {
         get;
         protected set;
      }

      public uint Inputs { get { return (uint)Channels.Count; } }
      public uint PWMs { get { return (uint)Channels.Count; } }
      public uint Relays { get { return (uint)Channels.Count; } }
      #endregion

      public void Open()
      {
         m_HatInterface.Open();
      }

      public void Close()
      {

      }

      public RPiHat(I2cDevice i2cDevice, UInt16 hatAddress)
      {
         m_HatInterface = new HatInterface_I2C(i2cDevice, hatAddress, new I2CBusHW_PCA9685());

         Channels = new List<IChannel>();
      }
   }
}
