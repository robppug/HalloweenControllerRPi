using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Functions.Func_GUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.Function_GUI
{
   public partial class Func_Sound_GUI : UserControl, IXmlSerializable, IFunctionGUI
   {
      private Func_SOUND _Func;
      private bool _boInitialised = false;

      public event EventHandler OnRemove;

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
         this.InitializeComponent();
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

         textTitle.DoubleTapped += TextTitle_DoubleTapped;
         textTitle.Text = "Sound #" + index;

         if(entype == Function.tenTYPE.TYPE_CONSTANT)
         {
            _Func.Loop = true;
            slider_Duration.IsEnabled = false;
            slider_StartDelay.IsEnabled = false;
            textBlock_Duration.Visibility = Visibility.Collapsed;
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
            Tracks.Add("Track #" + i.ToString());
         }
      }

      private void slider_Duration_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            _Func.Duration_ms = (uint)(sender as Slider).Value;
            textBlock_Duration.Text = "Duration: " + _Func.Duration_ms.ToString() + " (ms)";
         }
      }

      private void slider_StartDelay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            _Func.Delay_ms = (uint)(sender as Slider).Value;
            textBlock_StartDelay.Text = "Start Delay: " + _Func.Delay_ms.ToString() + " (ms)";
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

      public void ReadXml(System.Xml.XmlReader reader)
      {
         _Func.ReadXml(reader);

         textTitle.Text = reader.GetAttribute("CustomName");

         textBlock_Volume.Text = "Volume: " + _Func.Volume.ToString() + " (%)";
         textBlock_StartDelay.Text = "Start Delay: " + _Func.Delay_ms.ToString() + " (ms)";
         textBlock_Duration.Text = "Duration: " + _Func.Duration_ms.ToString() + " (ms)";

         /* Ignore MIN/MAX limits. */
         try
         {
            //comboBox_Track.SelectedIndex = (int)_Func.Track;
            slider_Duration.Value = _Func.Duration_ms;
            slider_StartDelay.Value = _Func.Delay_ms;
            slider_Volume.Value = _Func.Volume;
            radioButton_Random.IsChecked = _Func.Randomise;
         }
         catch { }
      }

      public void WriteXml(System.Xml.XmlWriter writer)
      {
         writer.WriteAttributeString("Type", GetType().ToString());
         writer.WriteAttributeString("CustomName", textTitle.Text);
         
         _Func.WriteXml(writer);
      }

      public void Initialise()
      {
         _Func.Initialise();
      }

      private void radioButton_Random_Click(object sender, RoutedEventArgs e)
      {
         _Func.Randomise = !_Func.Randomise;

         radioButton_Random.IsChecked = _Func.Randomise;

         //if (_Func.Randomise == false)
         //{
         //   ContentDialogResult cdr;
         //   ContentDialog cd = new ContentDialog()
         //   {
         //      Title = "Select tracks to randomise",
         //      IsPrimaryButtonEnabled = true,
         //      IsSecondaryButtonEnabled = true,
         //      HorizontalContentAlignment = HorizontalAlignment.Center,
         //      VerticalContentAlignment = VerticalAlignment.Center
         //   };

         //   StackPanel sp = new StackPanel();
         //   ListBox trackList = new ListBox();

         //   trackList.Items.Add(Tracks);
         //   sp.Children.Add(new TextBlock() { Text = "Available tracks..." });
         //   sp.Children.Add(trackList);
         //   Content = sp;

         //   cdr = cd.ShowAsync().GetResults();

         //   if (cdr == ContentDialogResult.Primary)
         //   {
         //      _Func.Randomise = true;
         //   }
         //}
         //else
         //   _Func.Randomise = false;
      }
   }
}
