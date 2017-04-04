using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   /// <summary>
   /// Raspberry Pi HAT class for the Raspberry Pi 2/3
   /// </summary>
   public abstract class RPiHat : IHat
   {
      #region /* ENUMS */

      public enum SupportedHATs : byte
      {
         MOSFET_v1 = 0,
         RELAY_v1,
         RELAY_EEPROM_v1,
         INPUT_v1,
         INPUT_EEPROM_v1,
         NoOfSupportedHATs
      };

      #endregion /* ENUMS */

      #region Declarations

      protected IHatInterface m_HatInterface;
      protected IHWController m_hostController;

      public IHWController HostController
      {
         get { return m_hostController; }
         protected set { m_hostController = value; }
      }

      public List<IChannel> Channels
      {
         get;
         protected set;
      }

      public SupportedHATs HatType
      {
         get;
         protected set;
      }

      #endregion Declarations

      protected RPiHat(IHWController host)
      {
         HostController = host;
      }

      /// <summary>
      /// RPiHat object if the HAT is supported and successfully initialised.
      /// </summary>
      /// <param name="i2cDevice"></param>
      /// <param name="hatAddress"></param>
      /// <returns>If successful an initialised RPiHat object otherwise null.</returns>
      public static RPiHat Open(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress)
      {
         SupportedHATs hatType;
         RPiHat rpiHat = null;

         hatType = RPiHat.GetHatType(hatAddress);

         if (hatType != SupportedHATs.NoOfSupportedHATs)
         {
            switch (hatType)
            {
               case SupportedHATs.MOSFET_v1:
                  rpiHat = new RPiHat_MOSFET_v1(host, i2cDevice, hatAddress);
                  break;

               case SupportedHATs.INPUT_v1:
                  rpiHat = new RPiHat_INPUT_v1(host, i2cDevice, hatAddress);
                  break;

               case SupportedHATs.RELAY_v1:
                  rpiHat = new RPiHat_RELAY_v1(host, i2cDevice, hatAddress);
                  break;

               case SupportedHATs.NoOfSupportedHATs:
               default:
                  break;
            }

         }

         return rpiHat;
      }

      public static void Close(RPiHat hat)
      {
      }

      /// <summary>
      /// Returns the type of HAT at the Address provided
      /// </summary>
      /// <param name="hatAddress"></param>
      /// <returns></returns>
      private static SupportedHATs GetHatType(ushort hatAddress)
      {
         SupportedHATs hat = SupportedHATs.NoOfSupportedHATs;

         /* PCA9501 - INPUT with EEPROM (0x40 - 0x4F is EEPROM) */
         if ((hatAddress >= 0x00) && (hatAddress <= 0x0F))
         {
            hat = SupportedHATs.INPUT_v1;
         }
         /* PCA9501 - RELAY with EEPROM (0x50 - 0x5F is EEPROM)*/
         else if ((hatAddress >= 0x10) && (hatAddress <= 0x1F))
         {
            hat = SupportedHATs.RELAY_v1;
         }
         /* PCA9685 - PWM Driver */
         else if ((hatAddress >= 0x60) && (hatAddress <= 0x6F))
         {
            hat = SupportedHATs.MOSFET_v1;
         }

         return hat;
      }

      /// <summary>
      ///
      /// </summary>
      public virtual void UpdateChannels()
      {
         foreach (IChannel c in Channels)
         {
            UpdateChannel(c);
         }
      }

      /// <summary>
      ///
      /// </summary>
      public virtual void UpdateChannel(IChannel chan)
      {
         if ((chan as IProcessTick) != null)
         {
            (chan as IProcessTick).Tick();

            m_HatInterface.BusDevice.RefreshChannel(chan);
         }
      }
   }
}