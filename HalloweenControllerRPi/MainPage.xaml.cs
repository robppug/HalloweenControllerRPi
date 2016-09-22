﻿using HalloweenControllerRPi.Container;
using HalloweenControllerRPi.Device;
using HalloweenControllerRPi.Device.Controllers;
using HalloweenControllerRPi.Function_GUI;
using HalloweenControllerRPi.Functions;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
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
using Windows.Media.Playback;
using Windows.Storage;
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
      public List<GroupContainer> lGroupContainers = new List<GroupContainer>();

      static public IHostApp HostApp;

      public static MainPage Current;

      public MainPage()
      {
         this.InitializeComponent();

         HostApp = this;

         lGroupContainers.Add(groupContainer_AlwaysActive);
         lGroupContainers.Add(groupContainer_Triggered);

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

      private void OnLoaded(object sender, RoutedEventArgs e)
      {
         HWInterface HWDevice;

         if (LightningProvider.IsLightningEnabled == true)
         {
            HWDevice = new HWRaspberryPI2();
         }
         else
         { 
            HWDevice = new HWSimulated();
         }

         HWDevice.CommandReceived += HWDevice_CommandReceived;
         //HWDevice.VersionInfoUpdated += this.ev_VersionInfoUpdated;
         //HWDevice.FunctionAdded += this.ev_FunctionAdded;

         try
         {
            HWDevice.Connect();

            HWDevice.DevicePID = 0xAAAA;

            lHWInterfaces.Add(HWDevice);

            if (HWDevice.GetUIPanel() != null)
            {
               HWSimulatedGrid.Items.Add(HWDevice.GetUIPanel());
            }

            /* Populate the available Functions the HWDevice provides. */
            this.Available_Statics.Items.Add(new Function_Button_SOUND(1));

            for (uint i = 0; i < HWDevice.Inputs; i++)
            {
               this.Available_Board.Items.Add(new Function_Button_INPUT(i + 1));
            }
            for (uint i = 0; i < HWDevice.PWMs; i++)
            {
               this.Available_Board.Items.Add(new Function_Button_PWM(i + 1));
            }
            for (uint i = 0; i < HWDevice.Relays; i++)
            {
               this.Available_Board.Items.Add(new Function_Button_RELAY(i + 1));
            }
         }
         catch { }
      }

      /// <summary>
      /// COMMAND received from HW Device that needs processing.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="args"></param>
      private void HWDevice_CommandReceived(object sender, CommandEventArgs args)
      {
         /* HW Device has received a COMMAND that needs processing */
         groupContainer_Triggered.ProcessTrigger(args);
      }

      /// <summary>
      /// Transmit the COMMAND received from the UI to the HW Device/s.
      /// </summary>
      /// <param name="cmd"></param>
      public void TransmitCommandToDevice(string cmd)
      {
         lHWInterfaces[0].TransmitCommand(cmd);
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
         /* Go through all Always Actives and check if control of used functions has completed */
         groupContainer_AlwaysActive.TriggerEnd(func);

         groupContainer_Triggered.CheckTriggerEnd();
      }

      #region /* UI HANDLING */
      private void buttonAdd_Click(object sender, RoutedEventArgs e)
      {
         this.groupContainer_Triggered.AddTriggerGroup();
      }

      private void buttonTrigger_Click(object sender, RoutedEventArgs e)
      {
         this.groupContainer_Triggered.ProcessTrigger(new CommandEventArgs('I', 1, 1));
      }

      private void buttonStart_Click(object sender, RoutedEventArgs e)
      {
         groupContainer_AlwaysActive.ProcessAlwaysActives(true);
      }

      private void buttonStop_Click(object sender, RoutedEventArgs e)
      {
         TriggerEnd(new Func_INPUT());

         groupContainer_AlwaysActive.ProcessAlwaysActives(false);
      }

      private void DragItemsStarting(object sender, DragItemsStartingEventArgs e)
      {
         foreach(Function_Button c in e.Items)
         {
            c.OnDragStarting(sender, e);
         }
      }
      #endregion
   }
}