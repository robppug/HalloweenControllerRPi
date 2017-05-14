using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.Drivers;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.SC16IS752;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   public class RPiHat_SOUND_v1 : RPiHat
   {
      IDriverSoundProvider soundDriver;
      BusDevice_SC16IS752 busDevice;
      public UInt16 address;

      public RPiHat_SOUND_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
      {
         HatType = SupportedHATs.SOUND_v1;
         soundDriver = new Catalex_YX5300();
         busDevice = new BusDevice_SC16IS752();
         address = hatAddress;

         /* Open the BUS DEVICE */
         busDevice.Open(i2cDevice);

         /* Initialise the BUS DEVICE */
         busDevice.InitialiseChannels();

         soundDriver.InitialiseDriver();

         Channels = new List<IChannel>();

         /* Initialise availble channels on attached HAT */
         for (uint i = 0; i < 2; i++)
         {
            ChannelFunction_SOUND chan = null;

            chan = new ChannelFunction_SOUND();

            if (chan != null)
            {
               Channels.Add(chan);
            }
         }
      }

      private void InitialiseDriver()
      {
         List<byte> data = new List<byte>();

         soundDriver.InitialiseDriver();

         (soundDriver as Catalex_YX5300).BuildCommand(ref data, Catalex_YX5300.COMMANDS.SEL_DEV, 0x02);

         //busDevice.WriteBytes(0, data);

         Task.Delay(500);
      }

      public override void RefreshChannel(IChannel chan)
      {
         busDevice.RefreshChannel(chan);
      }
   }
}
