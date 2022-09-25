using HalloweenControllerRPi.Device.Controllers.BusDevices;
using HalloweenControllerRPi.Device.Controllers.Channels;
using System;
using System.Collections.Generic;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats
{
    /// <summary>
    /// Raspberry Pi HAT - MOSFET v1
    /// </summary>
    public class RPiHat_MOSFET_v1 : RPiHat
    {
        public BusDevice_PCA9685<DeviceComms_I2C> busDevice;
        public UInt16 address;

        public RPiHat_MOSFET_v1(IHWController host, I2cDevice i2cDevice, UInt16 hatAddress) : base(host)
        {
            HatType = SupportedHATs.MOSFET_v1;
            busDevice = new BusDevice_PCA9685<DeviceComms_I2C>();
            address = hatAddress;

            /* Open the BUS DEVICE */
            busDevice.Open(new DeviceComms_I2C(i2cDevice));

            /* Initialise the BUS DEVICE */
            busDevice.InitialiseDriver();

            Channels = new List<IChannel>();

            /* Initialise availble channels on attached HAT */
            for (uint i = 0; i < 5; i++)
            {
                IChannel chan = null;

                chan = new ChannelFunction_PWM(this, i);

                if (chan != null)
                {
                    Channels.Add(chan);
                }
            }
        }

        public override void RefreshChannel(IChannel chan)
        {
            busDevice.RefreshChannel(chan);
        }
    }
}