using HalloweenControllerRPi.Device;
using HalloweenControllerRPi.Device.Controllers;
using HalloweenControllerRPi.Function_GUI;
using HalloweenControllerRPi.Functions;
using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.Gpio;
using Windows.Devices.Gpio.Provider;
using Windows.Devices.I2c;
using Windows.Devices.I2c.Provider;
using Windows.Devices.Pwm.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HalloweenControllerRPi
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public partial class MainPage : Page, IHostApp
   {
      public List<HWInterface> lHWInterfaces = new List<HWInterface>();

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

      static public IHostApp HostApp;

      static private Double pwmDutySetting;
      static private bool pwmToggle = false;
      private static I2cDevice device;

      private static byte[] bMODE1 = new byte[1] { 0x00 };
      private static byte[] bMODE2 = new byte[1] { 0x01 };
      private static byte[] LED0_ON_L = new byte[1] { 0x06 };
      private static byte[] LED0_ON_H = new byte[1] { 0x07 };
      private static byte[] LED0_OFF_L = new byte[1] { 0x08 };
      private static byte[] LED0_OFF_H = new byte[1] { 0x09 };
      static public Stopwatch sWatch;
      static public long TriggerTime;
      public static MainPage Current;

      public MainPage()
      {
         this.InitializeComponent();

         HostApp = this;

         this.Available_Statics.Items.Add(new Function_Button_SOUND(4));

         this.Available_Board.Items.Add(new Function_Button_INPUT(1));
         this.Available_Board.Items.Add(new Function_Button_PWM(2));
         this.Available_Board.Items.Add(new Function_Button_RELAY(3));

         foreach (Function_Button fb in Available_Board.Items)
         {
            fb.Height = Available_Board.Height;
         }

         this.Loaded += OnLoaded;
         this.Unloaded += OnUnloaded;
      }

      private void OnUnloaded(object sender, RoutedEventArgs e)
      {
         if( lHWInterfaces.Count > 0 )
         {
            foreach( IHWInterface hw in lHWInterfaces)
            {
               hw.Disconnect();
            }
         }
      }

      private async void OnLoaded(object sender, RoutedEventArgs e)
      {
         HWInterface HWDevice = new HWSimulated();

         //HWDevice.CommandReceived += this.ev_CommandReceived_SerialPort;
         //HWDevice.VersionInfoUpdated += this.ev_VersionInfoUpdated;
         //HWDevice.FunctionAdded += this.ev_FunctionAdded;

         HWDevice.Connect();
         HWDevice.DevicePID = 0xAAAA;

         lHWInterfaces.Add(HWDevice);

         //Setup the GPIO, PWM and I2C drivers
         if (LightningProvider.IsLightningEnabled)
         {
            LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(0x40);
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            i2cSettings.SharingMode = I2cSharingMode.Exclusive;

            I2cController i2cTest = (await I2cController.GetControllersAsync(LightningI2cProvider.GetI2cProvider()))[0];
            
            device = i2cTest.GetDevice(i2cSettings);

            textBox_GPIOStatus.Text = "GPIO pin initialized correctly.";

            byte[] buffer = new byte[10];

            /* Change to NORMAL mode */
            device.Write(new byte[2] { 0x00, 0x00 });

            await Task.Delay(1);

            foreach (tenPWMChannel c in Enum.GetValues(typeof(tenPWMChannel)))
            {
               device.Write(new byte[2] { (byte)(LED0_ON_L[0] + ((byte)c * 4)), 0x00 });
               device.Write(new byte[2] { (byte)(LED0_ON_H[0] + ((byte)c * 4)), 0x00 });
               device.Write(new byte[2] { (byte)(LED0_OFF_L[0] + ((byte)c * 4)), 0x00 });
               device.Write(new byte[2] { (byte)(LED0_OFF_H[0] + ((byte)c * 4)), 0x00 });
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

                     device.Write(new byte[2] { (byte)(LED0_ON_L[0] + ((byte)c * 4)), 0x00 });
                     device.Write(new byte[2] { (byte)(LED0_ON_H[0] + ((byte)c * 4)), 0x00 });

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

                     device.Write(new byte[2] { (byte)(LED0_OFF_L[0] + ((byte)c * 4)), (byte)(uMax & 0xFF) });
                     device.Write(new byte[2] { (byte)(LED0_OFF_H[0] + ((byte)c * 4)), (byte)((uMax >> 8) & 0xFF) });
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

      private void pivotContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         Pivot pItem = (sender as Pivot);

         switch(pItem.SelectedIndex)
         {
            case 0:
               break;
            case 1:
               break;
            default:
               break;
         }
      }

      private void buttonAdd_Click(object sender, RoutedEventArgs e)
      {
         this.groupContainer_Trigged.AddTriggerGroup();
      }

      private void buttonTrigger_Click(object sender, RoutedEventArgs e)
      {
         CommandEventArgs args = new CommandEventArgs('I', (char)1, (char)1);

         this.groupContainer_Trigged.ProcessTrigger(args.Commamd, args.Par1, args.Par2);
      }

      public void FireCommand(string cmd)
      {
         lHWInterfaces[0].FireCommand(cmd);
      }

      public string BuildCommand(string function, string subFunc, params string[] data)
      {
         return lHWInterfaces[0].BuildCommand(function, subFunc, data);
      }

      public List<Command> GetSubFunctionCommandsList(Command functionKey)
      {
         List<Command> availableSubFuncCommands = null;

         foreach (Command c in lHWInterfaces[0].Commands.Keys)
         {
            if (c.Key == functionKey.Key)
            {
               availableSubFuncCommands = lHWInterfaces[0].Commands[c];
               break;
            }
         }

         return availableSubFuncCommands;
      }

      public void TriggerEnd(Function func)
      {
         //throw new NotImplementedException();
      }

      private void buttonStart_Click(object sender, RoutedEventArgs e)
      {
         groupContainer_AlwaysActive.ProcessAlwaysActives(true);
      }

      private void buttonStop_Click(object sender, RoutedEventArgs e)
      {
         groupContainer_AlwaysActive.ProcessAlwaysActives(false);
      }
   }
}