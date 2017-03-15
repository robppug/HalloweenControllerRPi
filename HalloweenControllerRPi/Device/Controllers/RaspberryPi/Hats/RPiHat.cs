using HalloweenControllerRPi.Functions;
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
      #region /* ENUMS */
      public enum SupportedHATs : byte
      {
         MOSTFET_v1 = 0,
         RELAY_v1,
         INPUT_v1,
         NoOfSupportedHATs
      };
      #endregion

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

      /// <summary>
      /// RPiHat object if the HAT is supported and successfully initialised.
      /// </summary>
      /// <param name="i2cDevice"></param>
      /// <param name="hatAddress"></param>
      /// <returns>If successful an initialised RPiHat object otherwise null.</returns>
      public static RPiHat Open(I2cDevice i2cDevice, UInt16 hatAddress)
      {
         II2CBusDevice busDevice = null;
         SupportedHATs hat = SupportedHATs.NoOfSupportedHATs;
         RPiHat rpiHat = null;

         /* Discover the type of HAT connected */
         hat = SupportedHATs.MOSTFET_v1;

         switch (hat)
         {
            case SupportedHATs.MOSTFET_v1:
               busDevice = new BusDevice_PCA9685();
               break;
            case SupportedHATs.INPUT_v1:
               break;
            case SupportedHATs.RELAY_v1:
               break;
            case SupportedHATs.NoOfSupportedHATs:
            default:
               throw new Exception("Hat not supported.");
         }

         if (busDevice != null)
         {
            rpiHat = new RPiHat(i2cDevice, hatAddress, busDevice);
         }

         return rpiHat;
      }

      public void Close()
      {

      }

      private RPiHat(I2cDevice i2cDevice, UInt16 hatAddress, II2CBusDevice busDevice)
      {
         /* Get the Interfaces used by the HAT */
         m_HatInterface = new HatInterface_I2C(i2cDevice, hatAddress, busDevice);

         /* Get the available CHANNELS offered by the HAT */
         Channels = m_HatInterface.Open();
      }

      /// <summary>
      /// TASK execution call for HAT channel processing.
      /// </summary>
      public void ProcessTask()
      {
         /* Process each CHANNEL available on this HAT */
         foreach (IChannel c in Channels)
         {
            if ((c as IProcessTick) != null)
            {
               (c as IProcessTick).Tick();

               if ((c as ChannelFunction_PWM) != null)
               {
                  ChannelFunction_PWM pwm = (c as ChannelFunction_PWM);
                  IBusDevicePwmChannelProvider pwmDevice = (IBusDevicePwmChannelProvider)m_HatInterface.BusDevice;

                  if (pwm.Function != Func_PWM.tenFUNCTION.FUNC_OFF)
                  {
                     pwmDevice.SetChannel((ushort)pwm.Index, (ushort)pwm.Level);
                  }
                  else
                  {
                     pwmDevice.SetChannel((ushort)pwm.Index, 0x00);
                  }
               }
            }
         }
      }
   }
}
