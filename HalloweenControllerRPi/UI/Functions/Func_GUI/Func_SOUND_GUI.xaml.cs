using HalloweenControllerRPi.Controls;
using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Functions.Func_GUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.Function_GUI
{
    public partial class Func_Sound_GUI : UserControl, IXmlFunction, IFunctionGUI
    {
        private Func_SOUND _Func;
        private bool _boInitialised = false;
        private readonly string _TrackPrefix = "Track #";

        public event EventHandler OnRemove;

        public uint MaxDuration
        {
            get { textBlock_MaxDuration.Text = "Max Duration: " + _Func.MaxDuration_ms.ToString() + " (ms)"; return _Func.MaxDuration_ms; }
            set { _Func.MaxDuration_ms = value; textBlock_MaxDuration.Text = "Max Duration: " + value.ToString() + " (ms)"; }
        }
        public uint MinDuration
        {
            get { textBlock_MinDuration.Text = "Min Duration: " + _Func.MinDuration_ms.ToString() + " (ms)"; return _Func.MinDuration_ms; }
            set { _Func.MinDuration_ms = value; textBlock_MinDuration.Text = "Min Duration: " + value.ToString() + " (ms)"; }
        }

        public List<String> Tracks
        {
            get; set;
        }


        public Function Func
        {
            get { return _Func; }
            set { _Func = (value as Func_SOUND); }
        }

        public Func_Sound_GUI()
        {
            InitializeComponent();

            Tracks = new List<string>();
            Tracks.Add("N/A");

            comboBox_Track.DataContext = this;
            comboBox_Track.ItemsSource = Tracks;

            _boInitialised = true;
        }

        private void TextTitle_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                textTitle.Text = FuncGUIHelper.SetCustomName(textTitle.Text).Result;
            }
        }

        public Func_Sound_GUI(IHostApp host, uint index, Function.tenTYPE entype) : this()
        {
            _Func = new Func_SOUND(host, entype);
            _Func.Index = index;
            _Func.Volume = (uint)slider_Volume.Value;
            _Func.evOnFunctionUpdated += UpdateGUI;
            _Func.Loop = false;

            _Func.evOnTrigger += (sender, e) => { Grid.Background = new SolidColorBrush(Colors.LightYellow); };
            _Func.evOnDelayEnd += (sender, e) => { Grid.Background = new SolidColorBrush(Colors.LightGreen); };
            _Func.evOnDurationEnd += (sender, e) => { Grid.Background = new SolidColorBrush(Colors.LightGray); };

            textTitle.DoubleTapped += TextTitle_DoubleTapped;
            textTitle.Text = "Sound #" + index;

            if (entype == Function.tenTYPE.TYPE_CONSTANT)
            {
                _Func.Loop = true;
                slider_Duration.IsEnabled = false;
                slider_StartDelay.IsEnabled = false;
                textBlock_MinDuration.Visibility = Visibility.Collapsed;
                textBlock_MaxDuration.Visibility = Visibility.Collapsed;
                textBlock_StartDelay.Visibility = Visibility.Collapsed;
            }

            _Func.FuncButtonType = typeof(Function_Button_SOUND);

            this.RemoveButton.Click += RemoveButton_Click;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            OnRemove?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateGUI(object sender, EventArgs e)
        {
            Tracks.Clear();

            for (uint i = _Func.AvailableTracks; i > 0; i--)
            {
                Tracks.Add(_TrackPrefix + i.ToString());
            }
        }

        private void slider_Duration_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_boInitialised == true)
            {
                _Func.MinDuration_ms = (uint)(sender as RangeSlider).RangeMin;
                _Func.MaxDuration_ms = (uint)(sender as RangeSlider).RangeMax;
                textBlock_MinDuration.Text = "Min Duration: " + _Func.Duration_ms.ToString() + " (ms)";
                textBlock_MaxDuration.Text = "Max Duration: " + _Func.Duration_ms.ToString() + " (ms)";
            }
        }

        private void slider_StartDelay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_boInitialised == true)
            {
                _Func.MinDelay_ms = (uint)(sender as Slider).Value;
                textBlock_StartDelay.Text = "Start Delay: " + _Func.MinDelay_ms.ToString() + " (ms)";
            }
        }

        private void slider_Volume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_boInitialised == true)
            {
                _Func.Volume = (uint)(sender as Slider).Value;
                textBlock_Volume.Text = "Volume: " + (sender as Slider).Value.ToString() + " (%)";
            }
        }

        private void comboBox_Track_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_boInitialised == true)
            {
                _Func.Track = (uint)(sender as ComboBox).SelectedIndex + 1;
            }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }


        public void ReadXML(XElement element)
        {
            _Func.ReadXML(element);

            textTitle.Text = element.Attribute("CustomName").Value;
            textBlock_Volume.Text = "Volume: " + _Func.Volume.ToString() + " (%)";
            textBlock_StartDelay.Text = "Start Delay: " + _Func.MinDelay_ms.ToString() + " (ms)";
            textBlock_MinDuration.Text = "Min Duration: " + _Func.MinDuration_ms.ToString() + " (ms)";
            textBlock_MaxDuration.Text = "Max Duration: " + _Func.MaxDuration_ms.ToString() + " (ms)";

            /* Ignore MIN/MAX limits. */
            try
            {
                //comboBox_Track.SelectedIndex = (int)_Func.Track;
                slider_Duration.RangeMin = _Func.MinDuration_ms;
                slider_Duration.RangeMax = _Func.MaxDuration_ms;
                slider_StartDelay.Value = _Func.MinDelay_ms;
                slider_Volume.Value = _Func.Volume;
                radioButton_Random.IsChecked = _Func.Randomise;
            }
            catch { }

        }

        public void ReadXml(XmlReader reader)
        {

        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Type", GetType().ToString());
            writer.WriteAttributeString("CustomName", textTitle.Text);

            _Func.WriteXml(writer);
        }

        public void Initialise()
        {
            _Func.Initialise();
        }

        private async void radioButton_Random_Click(object sender, RoutedEventArgs e)
        {
            if (_Func.Randomise == false)
            {
                ContentDialogResult cdr;
                ContentDialog cd = new ContentDialog()
                {
                    Title = "Select tracks to randomise",
                    PrimaryButtonText = "OK",
                    SecondaryButtonText = "CANCEL",
                    IsPrimaryButtonEnabled = true,
                    IsSecondaryButtonEnabled = true,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                StackPanel sp = new StackPanel();
                ListBox trackList = new ListBox();

                trackList.SelectionMode = SelectionMode.Multiple;
                foreach (string s in Tracks)
                {
                    trackList.Items.Add(s);
                }
                sp.Orientation = Orientation.Vertical;
                sp.Children.Add(trackList);
                cd.Content = sp;

                cdr = await cd.ShowAsync();

                if ( (cdr == ContentDialogResult.Primary) && (trackList.SelectedItems.Count > 0) )
                {
                    foreach (string s in trackList.SelectedItems)
                    {
                        _Func.RandomTracks.Add(Convert.ToInt32(s.Remove(0, _TrackPrefix.Length)));
                    }

                    _Func.Randomise = true;
                }
            }
            else
            {
                _Func.RandomTracks.Clear();
                _Func.Randomise = false;
                UpdateGUI(this, EventArgs.Empty);

            }

            comboBox_Track.IsEnabled = !_Func.Randomise;
            radioButton_Random.IsChecked = _Func.Randomise;
        }
    }
}
