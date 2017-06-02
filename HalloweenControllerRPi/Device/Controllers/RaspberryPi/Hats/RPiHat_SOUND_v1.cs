using HalloweenControllerRPi.Device.Controllers.BusDevices;
using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Device.Controllers.Providers;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.Drivers;
using System;
using System.Collections.Generic;
using Windows.Devices.I2c;
using static HalloweenControllerRPi.Device.Controllers.Channels.SoundEventArgs;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   public class RPiHat_SOUND_v1 : RPiHat
   {
      List<ISoundProvider> soundDrivers;
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

         InitialiseSoundDrivers();

         Channels = new List<IChannel>();

         /* Initialise availble channels on attached HAT */
         for (uint i = 0; i < soundDrivers.Count; i++)
         {
            ChannelFunction_SOUND chan = null;

            chan = new ChannelFunction_SOUND(this, i);
            chan.StateChange += Chan_StateChange;

            if (chan != null)
            {
               Channels.Add(chan);
            }
         }
      }

      private void Chan_StateChange(object sender, SoundEventArgs e)
      {
         switch(e.NewState)
         {
            case State.Play:
               soundDrivers[(int)(sender as ChannelFunction_SOUND).Index].Play(e.Track, e.Volume);
               break;

            case State.Stop:
            default:
               soundDrivers[(int)(sender as ChannelFunction_SOUND).Index].Stop();
               break;
         }
      }

      private void InitialiseSoundDrivers()
      {
         List<byte> data = new List<byte>();

         soundDrivers = new List<ISoundProvider>((int)busDevice.NumberOfUARTChannels);

         //for (int i = 0; i < busDevice.NumberOfUARTChannels; i++)
         int i = 0;
         {
            Catalex_YX5300<BusDeviceStream_SC16IS752> sndDrv = new Catalex_YX5300<BusDeviceStream_SC16IS752>();
            sndDrv.Open(busDevice.UARTStreams[i]);
            sndDrv.InitialiseDriver();
            //sndDrv.GetNumberOfTracks();
            soundDrivers.Add(sndDrv);
         }
      }

      public override void RefreshChannel(IChannel chan)
      {
         busDevice.RefreshChannel(chan);
      }
   }
}
