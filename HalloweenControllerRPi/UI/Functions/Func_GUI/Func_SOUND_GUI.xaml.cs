using HalloweenControllerRPi.Functions;
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
   public sealed partial class Func_Sound_GUI : UserControl, IXmlSerializable, IFunctionGUI
   {
      private Func_SOUND _Func;
      private bool _boInitialised = false;

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
         Tracks.Add("Track 1");
         Tracks.Add("Track 2");
         Tracks.Add("Track 3");
         Tracks.Add("Track 4");
         Tracks.Add("Track 5");
         comboBox_Track.DataContext = this;
         comboBox_Track.ItemsSource = Tracks;

         _boInitialised = true;
      }

      public Func_Sound_GUI(IHostApp host, uint index, Function.tenTYPE entype) : this()
      {
         _Func = new Func_SOUND(host, entype);
         _Func.Index = index;
         _Func.Volume = (uint)slider_Volume.Value;

         //this.textTitle.MouseClick += gb_FunctionName_MouseClick;
         textTitle.Text = "Sound";

         if(entype == Function.tenTYPE.TYPE_CONSTANT)
         {
            slider_Duration.IsEnabled = false;
            slider_StartDelay.IsEnabled = false;
            textBlock_Duration.Visibility = Visibility.Collapsed;
            textBlock_StartDelay.Visibility = Visibility.Collapsed;
         }

         _Func.FuncButtonType = typeof(Function_Button_SOUND);
      }
      
      //private void gb_FunctionName_MouseClick(object sender, MouseEventArgs e)
      //{
      //if (e.Button == System.Windows.Forms.MouseButtons.Right)
      //{
      //   this.SetCustomName();
      //}
      //}

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
         _Func.Delay_ms = Convert.ToUInt16(reader.GetAttribute("Delay"));
         _Func.Duration_ms = Convert.ToUInt16(reader.GetAttribute("Duration"));
         _Func.Volume = Convert.ToUInt32(reader.GetAttribute("Volume"));
         textTitle.Text = reader.GetAttribute("CustomName");

         textBlock_Volume.Text = "Volume: " + (_Func.Volume * 100f).ToString() + " (%)";
         textBlock_StartDelay.Text = "Start Delay: " + _Func.Delay_ms.ToString() + " (ms)";
         textBlock_Duration.Text = "Duration: " + _Func.Duration_ms.ToString() + " (ms)";

         /* Ignore MIN/MAX limits. */
         try
         {
            slider_Duration.Value = _Func.Duration_ms;
            slider_StartDelay.Value = _Func.Delay_ms;
            slider_Volume.Value = _Func.Volume * 100;
         }
         catch { }
      }

      public void WriteXml(System.Xml.XmlWriter writer)
      {
         writer.WriteAttributeString("Type", GetType().ToString());
         writer.WriteAttributeString("CustomName", this.textTitle.Text);
         
         this._Func.WriteXml(writer);
      }

      public void SetCustomName()
      {
         //new PopupTextBox().SetCustomName(gb_FunctionName);
      }

   }
}
