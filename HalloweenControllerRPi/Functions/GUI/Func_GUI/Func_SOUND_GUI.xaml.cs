using HalloweenControllerRPi.Functions;
using System;
using System.Xml.Serialization;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.Function_GUI
{
   public sealed partial class Func_Sound_GUI : UserControl, IXmlSerializable, IFunctionGUI
   {
      private Func_SOUND _Func;

      public Function Func
      {
         get { return _Func; }
         set { this._Func = (value as Func_SOUND); }
      }

      public Func_Sound_GUI()
      {
         InitializeComponent();
      }

      public Func_Sound_GUI(IHostApp host, uint index, Function.tenTYPE entype) : this()
      {
         Func = new Func_SOUND(host, entype);
         this.Func.Index = index;

         //this.textTitle.MouseClick += gb_FunctionName_MouseClick;
         this.textTitle.Text = "Sound";

         this.Func.FuncButtonType = typeof(Function_Button_SOUND);

         //foreach (IWavePlayer p in Func_SOUND.playbackDevices)
         //{
         //   this.comboBox_Device.Items.Add(p);
         //   if (this.comboBox_Device.DropDownWidth < TextRenderer.MeasureText(p.ToString(), this.comboBox_Device.Font).Width)
         //   {
         //      this.comboBox_Device.DropDownWidth = TextRenderer.MeasureText(p.ToString(), this.comboBox_Device.Font).Width;
         //   }

         //   for (int j = 0; j < Func_SOUND.availableChannels; j++)
         //   {
         //      this.comboBox_Channel.Items.Add(j + 1);
         //   }
         //}
         //this._Func.evOnTimerTick += UpdatePosition;
      }

      //private void gb_FunctionName_MouseClick(object sender, MouseEventArgs e)
      //{
         //if (e.Button == System.Windows.Forms.MouseButtons.Right)
         //{
         //   this.SetCustomName();
         //}
      //}

      delegate void UpdateProgressbarCallback(int position);
      //private void UpdateProgressBar(int position)
      //{
      //   if (this.progressBar_Position.InvokeRequired)
      //   {
      //      this.Invoke(new UpdateProgressbarCallback(this.UpdateProgressBar), position);
      //   }
      //   else
      //   {
      //      progressBar_Position.Maximum = this._Func.SoundDuration_s;
      //      progressBar_Position.Value = position;
      //   }
      //}

      protected void UpdatePosition(object sender, EventArgs e)
      {
         //UpdateProgressBar((this._Func.soundStream.CurrentTime.Minutes * 60) + this._Func.soundStream.CurrentTime.Seconds);
      }

      private void trackBar_Duration_Scroll(object sender, EventArgs e)
      {
         //this.Func.Duration_ms = (uint)(sender as TrackBar).Value;
         //this.label_Duration.Text = "Duration: " + this.Func.Duration_ms.ToString() + " (ms)";
      }

      private void trackBar_StartDelay_Scroll(object sender, EventArgs e)
      {
         //this.Func.Delay_ms = (uint)(sender as TrackBar).Value;
         //this.label_StartDelay.Text = "Start Delay: " + this.Func.Delay_ms.ToString() + " (ms)";
      }

      private void textBoxOpenFile_DoubleClick(object sender, EventArgs e)
      {
         //if (openFileDialog_Sound.ShowDialog() == DialogResult.OK)
         //{
         //   this._Func.fileSOUND = openFileDialog_Sound.FileName;

         //   try
         //   {
         //      this._Func.OpenFile(this._Func.fileSOUND);
         //   }
         //   catch (Exception ex)
         //   {
         //      MessageBox.Show(ex.Message, "Problem opening file.");
         //      this._Func.CloseFile();
         //   }

         //   if (this._Func.soundStream != null)
         //   {
         //      this._Func.SoundDuration_s = (this._Func.soundStream.TotalTime.Minutes * 60) + this._Func.soundStream.TotalTime.Seconds;

         //      textBox_fileName.Text = Path.GetFileName(this._Func.fileSOUND);

         //      this.label_Duration.Text = "Duration: " + this._Func.SoundDuration_s.ToString() + " (ms)";
         //   }
         //   else
         //   {
         //      MessageBox.Show("Please assign a DEVICE and CHANNEL first before opening a file.", "Error");
         //   }
         //}
      }

      private void checkBox_Loop_CheckedChanged(object sender, EventArgs e)
      {
         //this._Func.Loop = (sender as CheckBox).Checked;

         //if ((sender as CheckBox).Checked)
         //   trackBar_NoOfRepeats.Enabled = true;
         //else
         //   trackBar_NoOfRepeats.Enabled = false;
      }

      private void trackBar_NoOfRepeats_Scroll(object sender, EventArgs e)
      {
         //this._Func.Repeats = (uint)(sender as TrackBar).Value;
         //this.label_Repeats.Text = "Repeats: " + this._Func.Repeats.ToString();
      }

      private void comboBox_Channel_SelectedIndexChanged(object sender, EventArgs e)
      {
         //if ((this._Func.selectedChannel != null) && (this._Func.selectedChannel != (sender as ComboBox).SelectedIndex))
         //{
         //   /* Clear the WAV/MP3 stream from the Output channel. */
         //   Func_SOUND.RemoveChannelAssignment((int)this._Func.selectedChannel);
         //}

         //this._Func.selectedChannel = (sender as ComboBox).SelectedIndex;
      }

      private void comboBox_Device_SelectedIndexChanged(object sender, EventArgs e)
      {
         //this._Func.activePlaybackDevice = Func_SOUND.playbackDevices[(sender as ComboBox).SelectedIndex];
      }

      private void trackBar_Volume_Scroll(object sender, EventArgs e)
      {
         //this._Func.activePlaybackDevice.Volume = (float)((sender as TrackBar).Value / 100f);
         //this.label_Volume.Text = "Volume: " + (sender as TrackBar).Value.ToString() + " (%)";
      }

      public System.Xml.Schema.XmlSchema GetSchema()
      {
         throw new NotImplementedException();
      }

      public void ReadXml(System.Xml.XmlReader reader)
      {
         //this._Func.Delay_ms = Convert.ToUInt16(reader.GetAttribute("Delay"));
         //this._Func.Duration_ms = Convert.ToUInt16(reader.GetAttribute("Duration"));
         //this._Func.Repeats = Convert.ToUInt16(reader.GetAttribute("Repeats"));
         //this.checkBox_Loop.Checked = Convert.ToBoolean(reader.GetAttribute("Loop"));
         this.textTitle.Text = reader.GetAttribute("CustomName");
         //this.checkBox_Loop_CheckedChanged(this.checkBox_Loop, EventArgs.Empty);

         //this._Func.activePlaybackDevice = reader.GetAttribute("ActivePBDevice");
         //if (this._Func.activePlaybackDevice != null)
         //{
         //   this._Func.selectedChannel = Convert.ToUInt16(reader.GetAttribute("SelectedChannel"));
         //   this._Func.activePlaybackDevice.Volume = Convert.ToSingle(reader.GetAttribute("Volume"));
         //   this.label_Volume.Text = "Volume: " + (this._Func.activePlaybackDevice.Volume / 100f).ToString() + " (%)";
         //}

         //this.label_StartDelay.Text = "Start Delay: " + this._Func.Delay_ms.ToString() + " (ms)";
         //this.label_Duration.Text = "Duration: " + this._Func.Duration_ms.ToString() + " (ms)";
         //this.label_Repeats.Text = "Repeats: " + this._Func.Repeats.ToString();

         /* Ignore MIN/MAX limits. */
         try
         {
            //this.trackBar_Duration.Value = (int)this._Func.Duration_ms;
            //this.trackBar_StartDelay.Value = (int)this._Func.Delay_ms;
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
