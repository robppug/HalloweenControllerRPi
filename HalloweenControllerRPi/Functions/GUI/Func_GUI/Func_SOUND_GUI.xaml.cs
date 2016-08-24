using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

      public Function Func
      {
         get { return _Func; }
         set { _Func = (value as Func_SOUND); }
      }

      public Func_Sound_GUI()
      {
         this.InitializeComponent();

         _boInitialised = true;
      }

      public Func_Sound_GUI(IHostApp host, uint index, Function.tenTYPE entype) : this()
      {
         _Func = new Func_SOUND(host, entype, mediaElement);
         _Func.Index = index;

         //this.textTitle.MouseClick += gb_FunctionName_MouseClick;
         textTitle.Text = "Sound";

         if(entype == Function.tenTYPE.TYPE_CONSTANT)
         {
            slider_Duration.IsEnabled = false;
            slider_StartDelay.IsEnabled = false;
            slider_Repeats.IsEnabled = false;
            checkBox_Loop.IsChecked = true;
            checkBox_Loop.IsEnabled = false;
            textBlock_Duration.Visibility = Visibility.Collapsed;
            textBlock_StartDelay.Visibility = Visibility.Collapsed;
            textBlock_Repeats.Visibility = Visibility.Collapsed;
         }

         _Func.FuncButtonType = typeof(Function_Button_SOUND);

         GetListofSounds();
         
         _Func.TimerTick += UpdatePosition;
      }
      
      private async void GetListofSounds()
      {
         int noOfSounds;

         noOfSounds = await _Func.GetAvailableSounds();

         if(noOfSounds > 0)
         {
            foreach (StorageFile file in _Func.lSoundFiles)
            {
               comboBox_Sounds.Items.Add(file.DisplayName + " (" + file.DisplayType + ")");
            }
         }
         else
         {
            comboBox_Sounds.Items.Add("No sound files found");
         }
      }

      //private void gb_FunctionName_MouseClick(object sender, MouseEventArgs e)
      //{
      //if (e.Button == System.Windows.Forms.MouseButtons.Right)
      //{
      //   this.SetCustomName();
      //}
      //}

      private void UpdatePosition(object sender, EventArgs e)
      {
         progressBar_Position.Maximum = _Func.SoundDuration_ms;
         progressBar_Position.Value = (int)_Func.activePlaybackDevice.Position.TotalMilliseconds;
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

      private void checkBox_Loop_Checked(object sender, RoutedEventArgs e)
      {
         _Func.Loop = (sender as CheckBox).IsChecked;

         if ((_Func.Loop == true) && (_Func.Type != Function.tenTYPE.TYPE_CONSTANT))
            slider_Repeats.IsEnabled = true;
         else
            slider_Repeats.IsEnabled = false;
      }

      private void slider_Repeats_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            _Func.Repeats = (uint)(sender as Slider).Value;
            textBlock_Repeats.Text = "Repeats: " + _Func.Repeats.ToString();
         }
      }

      private void slider_Volume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            _Func.activePlaybackDevice.Volume = (float)((sender as Slider).Value / 100f);
            textBlock_Volume.Text = "Volume: " + (sender as Slider).Value.ToString() + " (%)";
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
         _Func.Repeats = Convert.ToUInt16(reader.GetAttribute("Repeats"));
         checkBox_Loop.IsChecked = Convert.ToBoolean(reader.GetAttribute("Loop"));
         textTitle.Text = reader.GetAttribute("CustomName");
         checkBox_Loop_Checked(checkBox_Loop, null);

         //_Func.activePlaybackDevice = reader.GetAttribute("ActivePBDevice");
         //if (this._Func.activePlaybackDevice != null)
         //{
         //   this._Func.activePlaybackDevice.Volume = Convert.ToSingle(reader.GetAttribute("Volume"));
         //   this.label_Volume.Text = "Volume: " + (this._Func.activePlaybackDevice.Volume / 100f).ToString() + " (%)";
         //}

         textBlock_StartDelay.Text = "Start Delay: " + _Func.Delay_ms.ToString() + " (ms)";
         textBlock_Duration.Text = "Duration: " + _Func.Duration_ms.ToString() + " (ms)";
         textBlock_Repeats.Text = "Repeats: " + _Func.Repeats.ToString();

         /* Ignore MIN/MAX limits. */
         try
         {
            slider_Duration.Value = (int)this._Func.Duration_ms;
            slider_StartDelay.Value = (int)this._Func.Delay_ms;
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

      private void comboBox_Sounds_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (_Func.lSoundFiles.Count > 0)
         {
            _Func.OpenFile((sender as ComboBox).SelectedIndex);
         }
      }
   }
}
