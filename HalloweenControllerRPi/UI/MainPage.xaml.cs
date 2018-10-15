using HalloweenControllerRPi.Container;
using HalloweenControllerRPi.Device;
using HalloweenControllerRPi.Device.Controllers;
using HalloweenControllerRPi.Function_GUI;
using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Functions.Function_Button;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using HalloweenControllerRPi.UI.ExternalDisplay;
using System.Xml.Linq;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HalloweenControllerRPi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : Page, IHostApp
    {
        public List<HWController> lHWControllers = new List<HWController>();
        public List<GroupContainer> lGroupContainers = new List<GroupContainer>();

        static public IHostApp HostApp;
        static public MenuHandler Menu;

        static public List<Task> Tasks;

        public MainPage()
        {
            InitializeComponent();

            HostApp = this;

            buttonStart.IsEnabled = false;
            buttonStop.IsEnabled = false;

            Tasks = new List<Task>();

            lGroupContainers.Add(groupContainer_AlwaysActive);
            lGroupContainers.Add(groupContainer_Triggered);

            buttonStart.Background = new SolidColorBrush(Colors.Red);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (lHWControllers.Count > 0)
            {
                foreach (IHWController hw in lHWControllers)
                {
                    hw.Disconnect();
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            HWController HWController;

            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT")
            {
                HWController = new HWRaspberryPI2();
            }
            else
            {
                HWController = new HWSimulated();
            }

            HWController.CommandReceived += ReceiveCommandFromDevice;

            ControllerProgressBar.Visibility = Visibility.Visible;
            textControllerProgressBar.Visibility = Visibility.Visible;

            HWController.DiscoveryComplete += HWController_OnDiscoveryComplete;
            HWController.DiscoveryProgress += HWController_DiscoveryProgress;
            HWController.Initialised += HWController_DisplayInitialised;


            HWController.Connect();
            //Task loadController = Task.Run(() =>
            //{
            //   HWController.Connect();
            //});

            //Tasks.Add(loadController);

            lHWControllers.Add(HWController);
        }

        public static DetectingScreen _screen_Detecting = new DetectingScreen();
        public static MainScreen _screen_Main = new MainScreen();

        public static Task _displayUpdate;

        public XDocument xmlDocument { get; private set; }

        public void HWController_DisplayInitialised(object sender, EventArgs e)
        {
            if (HWController.Display != null)
            {
                HWController.Display.ActiveCanvas = ExternalDisplayCanvas;
                HWController.Display.OutputImage = OutputImage;

                Menu = new MenuHandler(HWController.Display, new MenuNode<UserControl>(null, _screen_Main));

                Menu.BlockButtons = true;
                Menu.DisplayPopup(_screen_Detecting);

                _displayUpdate = Menu.Update();
            }
        }

        /// <summary>
        /// Discovery of available functions progress bar.
        /// </summary>
        /// <param name="data"></param>
        public async void HWController_DiscoveryProgress(uint data)
        {
            textControllerProgressBar.Text = "Detecting available functions... " + data.ToString() + "%";
            ControllerProgressBar.Value = (double)data;

            if (HWController.Display != null)
            {
                if (_displayUpdate.IsCompleted)
                {
                    _screen_Detecting.UpdateProgressBar((double)data);
                    _screen_Detecting.UpdateLayout();

                    _displayUpdate = Menu.Update();

                    await _displayUpdate;
                }
            }
        }

        private async void HWController_OnDiscoveryComplete(object sender, EventArgs e)
        {
            HWController HWController = (sender as HWController);

            ControllerProgressBar.Visibility = Visibility.Collapsed;
            textControllerProgressBar.Visibility = Visibility.Collapsed;
            checkBox_LoadOnStart.IsChecked = false;

            // Start the Tasks
            Tasks.Add(loadSettingsFileAsync());
            Tasks.Add(UpdateControllerDisplay(_screen_Detecting));

            /* Populate the available Functions the HWDevice provides. */
            //this.Available_Statics.Items.Add(new Function_Button_SOUND(1));

            for (uint i = 0; i < HWController.Inputs; i++)
            {
                this.Available_Board.Items.Add(new Function_Button_INPUT(i + 1));
            }
            for (uint i = 0; i < HWController.PWMs; i++)
            {
                this.Available_Board.Items.Add(new Function_Button_PWM(i + 1));
            }
            for (uint i = 0; i < HWController.Relays; i++)
            {
                this.Available_Board.Items.Add(new Function_Button_RELAY(i + 1));
            }
            for (uint i = 0; i < HWController.SoundChannels; i++)
            {
                this.Available_Board.Items.Add(new Function_Button_SOUND(i + 1));
            }

            //Func_SOUND.GetAvailableSounds();

            if (HWController.GetUIPanel() != null)
            {
                HWSimulatedGrid.Items.Add(HWController.GetUIPanel());
            }
            else
            {
                MainArea.Children.Remove(SimulatedArea);
            }

            System.Diagnostics.Debug.WriteLine("Task: " + Task.CurrentId);

            await Task.WhenAll(Tasks.ToArray());

            Tasks.Clear();

            System.Diagnostics.Debug.WriteLine("Continue");

            if (checkBox_LoadOnStart.IsChecked == true)
            {
                await LoadSequenceFile();

                buttonStart_Click(buttonStart, null);

                groupContainer_Triggered.EnableAllInputs();

            }

            buttonStart.IsEnabled = true;
            buttonStop.IsEnabled = true;
        }

        public async Task UpdateControllerDisplay(UserControl screen)
        {
            System.Diagnostics.Debug.WriteLine("Update Display " + Task.CurrentId.ToString());

            if (HWController.Display != null)
            {
                // Wait for the current Task to complete.
                await _displayUpdate;

                Menu.EndPopup(screen);

                /* Activate Menu Button Control */
                Menu.BlockButtons = false;

                _displayUpdate = Menu.Update();
            }
        }

        /// <summary>
        /// COMMAND received from HW Device that needs processing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ReceiveCommandFromDevice(object sender, CommandEventArgs args)
        {
            /* HW Device has received a COMMAND that needs processing, inform all Functions */
            groupContainer_AlwaysActive.ProcessCommandEvent(args);
            groupContainer_Triggered.ProcessCommandEvent(args);
        }

        /// <summary>
        /// Transmit the COMMAND received from the UI to the HW Device/s.
        /// </summary>
        /// <param name="cmd"></param>
        public void TransmitCommandToDevice(string cmd)
        {
            lHWControllers[0].TransmitCommand(cmd);
        }

        public string BuildCommand(string function, string subFunc, params string[] data)
        {
            return lHWControllers[0].BuildCommand(function, subFunc, data);
        }

        public List<Command> GetSubFunctionCommandsList(Command functionKey)
        {
            List<Command> availableSubFuncCommands = null;

            foreach (Command c in lHWControllers[0].Commands.Keys)
            {
                if (c.Key == functionKey.Key)
                {
                    availableSubFuncCommands = lHWControllers[0].Commands[c];
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
            this.groupContainer_Triggered.ProcessCommandEvent(new CommandEventArgs('I', 'G', 1, 1));
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            buttonStart.Background = new SolidColorBrush(Colors.Green);
            groupContainer_AlwaysActive.ProcessAlwaysActives(true);
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            buttonStart.Background = new SolidColorBrush(Colors.Red);
            TriggerEnd(new Func_INPUT());

            groupContainer_AlwaysActive.ProcessAlwaysActives(false);
        }

        private void DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            foreach (Function_Button c in e.Items)
            {
                c.OnDragStarting(sender, e);
            }
        }
        #endregion

    }
}