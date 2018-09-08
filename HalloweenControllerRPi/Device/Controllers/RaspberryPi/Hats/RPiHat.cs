using HalloweenControllerRPi.Device.Controllers.Channels;
using System;
using System.Collections.Generic;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
    /// <summary>
    /// Raspberry Pi HAT class for the Raspberry Pi 2/3
    /// </summary>
    public abstract class RPiHat : IHat, IChannelHost
    {
        #region /* ENUMS */

        public enum SupportedHATs : byte
        {
            MOSFET_v1 = 0,
            RELAY_v1,
            RELAY_EEPROM_v1,
            INPUT_v1,
            INPUT_EEPROM_v1,
            SOUND_v1,
            DISPLAY_v1,
            NoOfSupportedHATs,
        };

        #endregion /* ENUMS */

        #region Declarations

        protected IHWController m_hostController;

        public static readonly ushort DisplayHatAddress = 0x3C;

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

        public uint NumberOfChannels
        {
            get { return (uint)Channels.Count; }
        }

        #endregion Declarations

        protected RPiHat(IHWController host)
        {
            HostController = host;
        }

        /// <summary>
        /// RPiHat object if the HAT is supported and successfully initialised.
        /// </summary>
        /// <param name="hatAddress"></param>
        /// <returns>If successful an initialised RPiHat object otherwise null.</returns>
        public static RPiHat Open(IHWController host, UInt16 hatAddress)
        {
            SupportedHATs hatType;
            RPiHat rpiHat = null;

            hatType = RPiHat.GetHatType(hatAddress);

            if (hatType != SupportedHATs.NoOfSupportedHATs)
            {
                switch (hatType)
                {
                    case SupportedHATs.MOSFET_v1:
                        rpiHat = new RPiHat_MOSFET_v1(host, (host as HWRaspberryPI2).I2CBusDevice, hatAddress);
                        break;

                    case SupportedHATs.INPUT_v1:
                        rpiHat = new RPiHat_INPUT_v1(host, (host as HWRaspberryPI2).I2CBusDevice, hatAddress);
                        break;

                    case SupportedHATs.RELAY_v1:
                        rpiHat = new RPiHat_RELAY_v1(host, (host as HWRaspberryPI2).I2CBusDevice, hatAddress);
                        break;

                    case SupportedHATs.DISPLAY_v1:
                        rpiHat = new RPiHat_DISPLAY_v1(host, (host as HWRaspberryPI2).I2CBusDevice, hatAddress);
                        break;

                    case SupportedHATs.SOUND_v1:
                        rpiHat = new RPiHat_SOUND_v1(host, (host as HWRaspberryPI2).I2CBusDevice, hatAddress);
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
            /* PCA9501 - DISPLAY (0x3C or 0x3D <Not Used>) */
            else if (hatAddress == DisplayHatAddress)
            {
                hat = SupportedHATs.DISPLAY_v1;
            }
            /* SC16IS752 - SOUND (0x48 - 0x4F) */
            else if ((hatAddress >= 0x48) && (hatAddress <= 0x4F))
            {
                hat = SupportedHATs.SOUND_v1;
            }
            /* PCA9501 - PUSH BUTTONS and EEPROM (0x30, 0x70 is EEPROM) */
            //else if (hatAddress == 0x30)
            //{
            //    hat = SupportedHATs.DISPLAYBUTTONS_v1;
            //}
            /* PCA9685 - PWM Driver */
            else if ((hatAddress >= 0x60) && (hatAddress <= 0x6F))
            {
                hat = SupportedHATs.MOSFET_v1;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(hatAddress.ToString("x") + " - Unsupported Device Found.");
            }

            return hat;
        }

        /// <summary>
        ///
        /// </summary>
        public virtual void HatTask()
        {
            foreach (IChannel c in Channels)
            {
                UpdateChannel(c);
            }
        }

        public virtual void UpdateChannel(IChannel chan)
        {
            if ((chan as IProcessTick) != null)
            {
                (chan as IProcessTick).Tick();

                RefreshChannel(chan);
            }
        }

        public abstract void RefreshChannel(IChannel chan);
    }
}