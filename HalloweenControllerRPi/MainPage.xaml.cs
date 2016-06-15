using Microsoft.IoT.Lightning.Providers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Devices.Pwm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HalloweenControllerRPi
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class MainPage : Page
   {
      private enum tenPWMChannel
      {
         enChan1 = 0,
         enChan2,
         enChan3,
         enChan4,
         enChan5,
         enChan6,
         enChan7,
         enChan8,
         enChan9,
         enChan10,
         enChan11,
         enChan12,
         enChan13,
         enChan14,
         enChan15,
         enChan16
      };

      static private GpioPin GPIO_4;
      static private PwmPin PWM_4;
      static private Double pwmDutySetting;
      static private bool pwmToggle = false;
      private static I2cDevice sensor;

      private static byte[] bMODE1 = new byte[1] { 0x00 };
      private static byte[] bMODE2 = new byte[1] { 0x01 };
      private static byte[] LED0_ON_L = new byte[1] { 0x06 };
      private static byte[] LED0_ON_H = new byte[1] { 0x07 };
      private static byte[] LED0_OFF_L = new byte[1] { 0x08 };
      private static byte[] LED0_OFF_H = new byte[1] { 0x09 };
      static public Stopwatch sWatch;
      static public long TriggerTime;

      public MainPage()
      {
         this.InitializeComponent();

         this.Available_Statics.Children.Add(new Function_Button());

         this.Available_Board.Children.Add(new Function_Button());

         this.Loaded += OnLoaded;
      }

      private async void OnLoaded(object sender, RoutedEventArgs e)
      {

         //Setup the GPIO, PWM and I2C drivers
         if (LightningProvider.IsLightningEnabled)
         {
            LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            I2cController controller = (await I2cController.GetControllersAsync(LightningI2cProvider.GetI2cProvider()))[0];
            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(0x40);
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            i2cSettings.SharingMode = I2cSharingMode.Exclusive;
            sensor = controller.GetDevice(i2cSettings);

            textBox_GPIOStatus.Text = "GPIO pin initialized correctly.";

            byte[] buffer = new byte[10];

            /* Change to NORMAL mode */
            sensor.Write(new byte[2] { 0x00, 0x00 });

            await Task.Delay(1);

            foreach (tenPWMChannel c in Enum.GetValues(typeof(tenPWMChannel)))
            {
               sensor.Write(new byte[2] { (byte)(LED0_ON_L[0] + ((byte)c * 4)), 0x00 });
               sensor.Write(new byte[2] { (byte)(LED0_ON_H[0] + ((byte)c * 4)), 0x00 });
               sensor.Write(new byte[2] { (byte)(LED0_OFF_L[0] + ((byte)c * 4)), 0x00 });
               sensor.Write(new byte[2] { (byte)(LED0_OFF_H[0] + ((byte)c * 4)), 0x00 });
            }
         }

         //Create the Background Task
         TaskFactory tTaskFactory = new TaskFactory(TaskScheduler.Current);

         sWatch = new Stopwatch();
         sWatch.Start();

         await tTaskFactory.StartNew(new Action(DoStuff), TaskCreationOptions.PreferFairness);
      }

      private static uint bPWMOutput = 0;

      private void DoStuff()
      {
         uint[] xCoords = new uint[5] { 0, 300, 600, 2400, 4096 };
         uint[] yCoords = new uint[5] { 0, 1000, 2000, 3000, 4096 };

         while (sWatch.IsRunning == true)
         {
            TriggerTime = MainPage.sWatch.ElapsedMilliseconds;

            if (TriggerTime >= 10)
            {
               //System.Diagnostics.Debug.WriteLine(TriggerTime.ToString());

               sWatch.Restart();

               if (LightningProvider.IsLightningEnabled)
               {
                  foreach (tenPWMChannel c in Enum.GetValues(typeof(tenPWMChannel)))
                  {
                     uint uMax;

                     bPWMOutput++;

                     if (bPWMOutput > 4096)
                        bPWMOutput = 0;

                     sensor.Write(new byte[2] { (byte)(LED0_ON_L[0] + ((byte)c * 4)), 0x00 });
                     sensor.Write(new byte[2] { (byte)(LED0_ON_H[0] + ((byte)c * 4)), 0x00 });

                     uMax = bPWMOutput;
                     switch (c)
                     {
                        case tenPWMChannel.enChan1:
                        case tenPWMChannel.enChan2:
                        case tenPWMChannel.enChan3:
                           uMax %= (uint)(4096 * pwmDutySetting);
                           break;

                        default:
                           break;
                     }

                     sensor.Write(new byte[2] { (byte)(LED0_OFF_L[0] + ((byte)c * 4)), (byte)(uMax & 0xFF) });
                     sensor.Write(new byte[2] { (byte)(LED0_OFF_H[0] + ((byte)c * 4)), (byte)((uMax >> 8) & 0xFF) });
                  }
               }
            }
         }
      }

      private void LedBrightness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         pwmDutySetting = ledBrightness.Value * .01;
         if (this.ledPercent != null)
            this.ledPercent.Text = pwmDutySetting.ToString() + "%";
         pwmToggle = true;
      }
   }
}