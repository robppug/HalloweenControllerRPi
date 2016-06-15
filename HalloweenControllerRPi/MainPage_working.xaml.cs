using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Devices.Pwm;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HalloweenControllerRPi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
      static private GpioPin GPIO_4;
      static private PwmPin PWM_4;
      static private Double pwmDutySetting;
      static private bool pwmToggle = false;
      static I2cDevice sensor;


      private TimerElapsedHandler tElapsedHandler = ThreadPoolTimerHandler;

      public MainPage()
      {
         this.InitializeComponent();

         this.Available_Statics.Children.Add(new Function_Button());

         this.Loaded += OnLoaded;
      }


      private async void OnLoaded(object sender, RoutedEventArgs e)
      {
         if (LightningProvider.IsLightningEnabled)
         {
            LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            //var gpioControllers = await GpioController.GetControllersAsync(LightningGpioProvider.GetGpioProvider());
            //var pwmControllers = await PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider());
            //var i2cControllers = await I2cController.GetControllersAsync(LightningI2cProvider.GetI2cProvider());

            I2cController controller = (await I2cController.GetControllersAsync(LightningI2cProvider.GetI2cProvider()))[0];
            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(0x40);
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            i2cSettings.SharingMode = I2cSharingMode.Exclusive;
            sensor = controller.GetDevice(i2cSettings);

            //List<GpioController> lgpioControllers = gpioControllers.ToList();
            //List<PwmController> lpwmControllers = pwmControllers.ToList();
            //List<I2cController> li2cControllers = i2cControllers.ToList();

            //lpwmControllers[1].SetDesiredFrequency(100);

            //I2cDevice i2cBus = li2cControllers[1].GetDevice(new I2cConnectionSettings(0x40));

            //PWM_4 = lpwmControllers[1].OpenPin(4);
            //pwmDutySetting = ledBrightness.Value * .01;
            //PWM_4.SetActiveDutyCyclePercentage(pwmDutySetting);
            //PWM_4.Start();

            textBox_GPIOStatus.Text = "GPIO pin initialized correctly.";

            byte[] bMODE1 = new byte[1] { 0x00 };
            byte[] bMODE2 = new byte[1] { 0x01 };
            byte[] LED0_ON_L = new byte[1] { 0x06 };
            byte[] LED0_ON_H = new byte[1] { 0x07 };
            byte[] LED0_OFF_L = new byte[1] { 0x08 };
            byte[] LED0_OFF_H = new byte[1] { 0x09 };
            byte[] buffer = new byte[10];

            sensor.Read(buffer);

            sensor.Write(new byte[1] { 0x30 });

            await Task.Delay(5);

            sensor.Read(buffer);
            sensor.Write(new byte[2] { LED0_ON_L[0], 0x00 });
            sensor.Write(new byte[2] { LED0_ON_H[0], 0x00 });
            sensor.Write(new byte[2] { LED0_OFF_L[0], 0x00 });
            sensor.Write(new byte[2] { LED0_OFF_H[0], 0x00 });

            ThreadPoolTimer tPoolTimer = ThreadPoolTimer.CreatePeriodicTimer(ThreadPoolTimerHandler, TimeSpan.FromMilliseconds(500));
         }

      }

      private void LedBrightness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         pwmDutySetting = ledBrightness.Value * .01;
         ledPercent.Text = pwmDutySetting.ToString() + "%";
         pwmToggle = true;
      }

      static byte bPWMOutputLvl = 0;
      static byte bPWMOutLvl_H = 0;

      private static void ThreadPoolTimerHandler(ThreadPoolTimer timer)
      {
         //PWM_4.SetActiveDutyCyclePercentage((pwmToggle ? 0.0 : pwmDutySetting));
         byte[] outBuf = new byte[2];

         //u16PWMOutputLvl = (UInt16)(pwmDutySetting * (double)4095);
         //if (pwmToggle)
         {
            sensor.Write(new byte[2] { 0x06, 0x00 });
            sensor.Write(new byte[2] { 0x07, 0x00 });
            bPWMOutputLvl += 51;
            sensor.Write(new byte[2] { 0x08, bPWMOutputLvl });
            sensor.Write(new byte[2] { 0x09, bPWMOutLvl_H });

            if (bPWMOutputLvl >= 200)
            {
               bPWMOutLvl_H++;
               if (bPWMOutLvl_H >= 0x10)
                  bPWMOutLvl_H = 0;
            }

            pwmToggle = false;
         }
         //else
         //{
         //   sensor.Write(new byte[2] { 0x06, 0x00 });
         //   sensor.Write(new byte[2] { 0x07, 0x10 });
         //   sensor.Write(new byte[2] { 0x08, 0x00 });
         //   sensor.Write(new byte[2] { 0x09, 0x00 });
         //}

         pwmToggle = !pwmToggle;
      }
   }
}
