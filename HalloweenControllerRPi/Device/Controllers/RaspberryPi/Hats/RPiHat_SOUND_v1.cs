using HalloweenControllerRPi.Device.Controllers.BusDevices;
using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Device.Controllers.Providers;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.Drivers;
using System;
using System.Collections.Generic;
using Windows.Devices.I2c;
using static HalloweenControllerRPi.Device.Controllers.Channels.SoundChannelEventArgs;

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
         for (int i = 0; i < soundDrivers.Count; i++)
         {
            ChannelFunction_SOUND chan;

            chan = new ChannelFunction_SOUND(this, (uint)i);
            chan.ChannelUpdated += Chan_ChannelUpdated;

            chan.AvailableTracks = (soundDrivers[i] as Catalex_YX5300<BusDeviceStream_SC16IS752>).GetNumberOfTracks();

            Channels.Add(chan);
         }
      }

      private void Chan_ChannelUpdated(object sender, SoundChannelEventArgs e)
      {
         ChannelFunction_SOUND sndChan = (sender as ChannelFunction_SOUND);

         switch (e.NewState)
         {
            case SoundState.Play:
               soundDrivers[(int)sndChan.Index].Play(sndChan.Track, sndChan.Loop);
               break;

            case SoundState.Volume:
               soundDrivers[(int)sndChan.Index].Volume(sndChan.Volume);
               break;

            case SoundState.Stop:
            default:
               soundDrivers[(int)sndChan.Index].Stop();
               break;
         }
      }

      private void InitialiseSoundDrivers()
      {
         soundDrivers = new List<ISoundProvider>((int)busDevice.NumberOfUARTChannels);

         for (int i = 0; i < busDevice.NumberOfUARTChannels; i++)
         {
            Catalex_YX5300<BusDeviceStream_SC16IS752> sndDrv = new Catalex_YX5300<BusDeviceStream_SC16IS752>();
            sndDrv.Open(busDevice.UARTStreams[i]);
            sndDrv.InitialiseDriver();
            sndDrv.StateChanged += SndDrv_StateChanged;
            soundDrivers.Add(sndDrv);
         }
      }

      private void SndDrv_StateChanged(object sender, SoundProviderEventArgs e)
      {
         uint index = (uint)soundDrivers.IndexOf((sender as ISoundProvider));

         switch (e.NewState)
         {
            case SoundProviderEventArgs.State.SoundFinished:
               HostController.OnChannelNotification(Channels[(int)index], new CommandEventArgs('S', 'F', index, (uint)e.NewState ));
               break;
         }
      }

      public override void RefreshChannel(IChannel chan)
      {
         busDevice.RefreshChannel(chan);
      }
   }
}
