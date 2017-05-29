using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.Drivers;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.SC16IS752;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using static HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.SC16IS752.BusDevice_SC16IS752;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   public class RPiHat_SOUND_v1 : RPiHat
   {
      List<IDriverSoundProvider> soundDrivers;
      BusDevice_SC16IS752 busDevice;
      public UInt16 address;

      public RPiHat_SOUND_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
      {
         HatType = SupportedHATs.SOUND_v1;
         busDevice = new BusDevice_SC16IS752();
         address = hatAddress;

         /* Open the BUS DEVICE */
         busDevice.Open(i2cDevice);

         /* Initialise the BUS DEVICE */
         busDevice.InitialiseChannels();

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

      private async void InitialiseSoundDriversAsync()
      {
         List<byte> data = new List<byte>();

         soundDrivers = new List<IDriverSoundProvider>((int)busDevice.NumberOfUARTChannels);

         for (int i = 0; i < busDevice.NumberOfUARTChannels; i++)
         {
            soundDrivers.Add(new Catalex_YX5300());

            soundDrivers[i].InitialiseDriver();

            (soundDrivers[i] as Catalex_YX5300).BuildCommand(ref data, Catalex_YX5300.COMMANDS.SEL_DEV, 0x02);

            busDevice.WriteBytes((UartChannels)i, data);
            data.Clear();

            await Task.Delay(200);

            (soundDrivers[i] as Catalex_YX5300).BuildCommand(ref data, Catalex_YX5300.COMMANDS.PLAY_W_VOL, 0x0F01);

            busDevice.WriteBytes((UartChannels)i, data);
            data.Clear();
         }
      }

      public override void RefreshChannel(IChannel chan)
      {
         busDevice.RefreshChannel(chan);
      }
   }
}
