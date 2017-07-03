using HalloweenControllerRPi.Device.Controllers.BusDevices;
using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Device.Controllers.Providers;
using HalloweenControllerRPi.Device.Drivers;
using System;
using System.Collections.Generic;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.System.Threading;
using Windows.UI.Xaml;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
   public class RPiHat_DISPLAY_v1 : RPiHat, IDisplayChannel
   {
      private BusDevice_PCA9501<DeviceComms_I2C> busDevice;
      private SSD1306<DeviceComms_I2C> displayDriver;
      private UInt16 address;
      
      public IDriverDisplayProvider Device => displayDriver;

      public RPiHat_DISPLAY_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
      {
         HatType = SupportedHATs.DISPLAY_v1;
         busDevice = new BusDevice_PCA9501<DeviceComms_I2C>();
         displayDriver = new SSD1306<DeviceComms_I2C>(128, 64);
         address = hatAddress;

         /* Open the BUS DEVICE */
         busDevice.Open(new DeviceComms_I2C(i2cDevice));

         /* Initialise the BUS DEVICE */
         busDevice.InitialiseDriver();

         /* Open the DISPLAY driver */
         displayDriver.Open(new DeviceComms_I2C(i2cDevice));

         /* Initialise the DISPLAY DRIVER */
         displayDriver.InitialiseDriver();

         Channels = new List<IChannel>();

         /* Initialise availble channels on attached HAT */
         for (uint i = 0; i < 7; i++)
         {
            ChannelFunction_BUTTON chan = null;
            IIOPin pin = null;

            pin = busDevice.GetPin((ushort)i);

            pin.SetDriveMode(GpioPinDriveMode.InputPullUp);

            chan = new ChannelFunction_BUTTON(this, i, pin);

            chan.ButtonStateChanged += Chan_ButtonStateChanged;

            if (chan != null)
            {
               Channels.Add(chan);
            }
         }
      }

      private void Chan_ButtonStateChanged(object sender, ChannelFunction_BUTTON.ButtonStateEventArgs e)
      {
         switch(e.PinNumber)
         {
            //Display (Left)
            case 0:
               break;
            //Display (Right)
            case 1:
               break;
            //Left
            case 2:
               break;
            //Right
            case 3:
               break;
            //Up
            case 4:
               break;
            //Down
            case 5:
               break;
            //Enter
            case 6:
               break;
         }
      }
      private void Chan_InputLevelChanged(object sender, ChannelFunction_INPUT.EventArgsINPUT e)
      {
         //HostController.OnChannelNotification(this, new CommandEventArgs('I', 'G', e.Index + 1, (uint)e.TriggerLevel));
      }

      public override void RefreshChannel(IChannel chan)
      {
         busDevice.RefreshChannel(chan);
      }
   }
}
