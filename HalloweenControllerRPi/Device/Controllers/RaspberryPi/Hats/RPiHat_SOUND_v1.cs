using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevices;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.Drivers;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.SC16IS752;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   public class RPiHat_SOUND_v1 : RPiHat
   {
      List<IDriverSoundProvider> soundDrivers;
      BusDevice_SC16IS752<DeviceComms_I2C> busDevice;
      public UInt16 address;

      public RPiHat_SOUND_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
      {
         HatType = SupportedHATs.SOUND_v1;
         busDevice = new BusDevice_SC16IS752<DeviceComms_I2C>();
         address = hatAddress;

         /* Open the BUS DEVICE */
         busDevice.Open(new DeviceComms_I2C(i2cDevice));

         /* Initialise the BUS DEVICE */
         busDevice.InitialiseDriver();

         InitialiseSoundDriversAsync();

         Channels = new List<IChannel>();

         /* Initialise availble channels on attached HAT */
         for (uint i = 0; i < soundDrivers.Count; i++)
         {
            ChannelFunction_SOUND chan = null;

            chan = new ChannelFunction_SOUND();

            if (chan != null)
            {
               Channels.Add(chan);
            }
         }
      }

      private void InitialiseSoundDriversAsync()
      {
         List<byte> data = new List<byte>();

         soundDrivers = new List<IDriverSoundProvider>((int)busDevice.NumberOfUARTChannels);

         for (int i = 0; i < busDevice.NumberOfUARTChannels; i++)
         {
            Catalex_YX5300<BusDeviceStream_SC16IS752> sndDrv = new Catalex_YX5300<BusDeviceStream_SC16IS752>();
            sndDrv.Open(busDevice.UARTStreams[i]);
            sndDrv.InitialiseDriver();
            soundDrivers.Add(sndDrv);
         }
      }

      public override void RefreshChannel(IChannel chan)
      {
         busDevice.RefreshChannel(chan);
      }
   }
}
