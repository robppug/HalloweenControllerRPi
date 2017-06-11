using HalloweenControllerRPi.Device.Controllers.BusDevices;
using HalloweenControllerRPi.Device.Controllers.Channels;
using System;
using System.Collections.Generic;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   /// <summary>
   /// Raspberry Pi HAT - INPUT v1
   /// </summary>
   public class RPiHat_INPUT_v1 : RPiHat
   {
      public BusDevice_PCA9501<DeviceComms_I2C> busDevice;
      public UInt16 address;

      public RPiHat_INPUT_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
      {
         HatType = SupportedHATs.INPUT_v1;
         busDevice = new BusDevice_PCA9501<DeviceComms_I2C>();
         address = hatAddress;

         /* Open the BUS DEVICE */
         busDevice.Open(new DeviceComms_I2C(i2cDevice));

         /* Initialise the BUS DEVICE */
         busDevice.InitialiseDriver();

         Channels = new List<IChannel>();

         /* Initialise availble channels on attached HAT */
         for (uint i = 0; i < 8; i++)
         {
            ChannelFunction_INPUT chan = null;
            IIOPin pin = null;

            pin = busDevice.GetPin((ushort)i);

            pin.SetDriveMode(GpioPinDriveMode.InputPullUp);

            chan = new ChannelFunction_INPUT(this, i, pin);

            chan.InputLevelChanged += Chan_InputLevelChanged;
               
            if (chan != null)
            {
               Channels.Add(chan);
            }
         }
      }

      private void Chan_InputLevelChanged(object sender, ChannelFunction_INPUT.EventArgsINPUT e)
      {
         HostController.OnChannelNotification(this, new CommandEventArgs('I', 'G', e.Index + 1, (uint)e.TriggerLevel));
      }

      public override void RefreshChannel(IChannel chan)
      {
         busDevice.RefreshChannel(chan);
      }
   }
}