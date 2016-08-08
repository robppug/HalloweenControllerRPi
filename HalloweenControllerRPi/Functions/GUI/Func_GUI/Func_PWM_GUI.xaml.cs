using HalloweenControllerRPi.Functions;
using System;
using System.Xml.Serialization;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.Function_GUI
{
   public sealed partial class Func_PWM_GUI : UserControl, IXmlSerializable, IFunctionGUI
   {
      private Func_PWM _Func;
      private bool _boInitialised = false;
      
      public Function Func
      {
         get { return this._Func; }
         set { this._Func = (value as Func_PWM); }
      }

      public Func_PWM_GUI()
      {
         this.InitializeComponent();

         _boInitialised = true;
      }

      public Func_PWM_GUI(IHostApp host, uint index, Function.tenTYPE entype) : this()
      {
         _Func = new Func_PWM(host, entype);

         this._Func.Index = index;
         
         this.textTitle.Text = "PWM #" + index;
         this.textTitle.DoubleTapped += TextTitle_DoubleTapped;
         this._Func.Duration_ms = (uint)slider_Duration.Value;
         this._Func.Delay_ms = (uint)slider_MaxLevel.Value;

         /* Populate the comboBox list with available PWM Functions */
         foreach (Func_PWM.tenFUNCTION f in Enum.GetValues(typeof(Func_PWM.tenFUNCTION)))
         {
            if (f != Func_PWM.tenFUNCTION.FUNC_NO_OF_FUNCTIONS)
               comboBox_Functions.Items.Add(f.ToString());
         }
         comboBox_Functions.SelectedIndex = 0;

         if (entype == Func_PWM.tenTYPE.TYPE_CONSTANT)
         {
            this.slider_Duration.IsEnabled = false;
            this.slider_StartDelay.IsEnabled = false;
         }

         this._Func.FuncButtonType = typeof(Function_Button_PWM);
      }

      private void TextTitle_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
      {
         if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
         {
            this.SetCustomName();
         }
      }

      private void slider_Duration_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            this._Func.Duration_ms = (uint)(sender as Slider).Value;
            this.textBlock_Duration.Text = "Duration: " + this._Func.Duration_ms.ToString() + " (ms)";
         }
      }

      private void slider_MaxLevel_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            this._Func.MaxLevel = (uint)(sender as Slider).Value;
            this.textBlock_MaxLevel.Text = "Max Level: " + this._Func.MaxLevel.ToString() + " (%)";
         }
      }

      private void slider_StartDelay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            this._Func.Delay_ms = (uint)(sender as Slider).Value;
            this.textBlock_StartDelay.Text = "Start Delay: " + this._Func.Delay_ms.ToString() + " (ms)";
         }
      }

      private void slider_UpdateRate_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            this._Func.UpdateRate = (uint)(sender as Slider).Value;
            this.textBlock_UpdateRate.Text = "Update Rate: " + this._Func.UpdateRate.ToString() + " (ms)";
         }
      }

      private void comboBox_Functions_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            this._Func.Function = (Func_PWM.tenFUNCTION)(sender as ComboBox).SelectedIndex;
         }
      }

      #region XML Handling
      public void ReadXml(System.Xml.XmlReader reader)
      {
         this._Func.Delay_ms = Convert.ToUInt16(reader.GetAttribute("Delay"));
         this._Func.Duration_ms = Convert.ToUInt16(reader.GetAttribute("Duration"));
         this._Func.MaxLevel = Convert.ToUInt16(reader.GetAttribute("MaxLevel"));
         this._Func.UpdateRate = Convert.ToUInt16(reader.GetAttribute("UpdateRate"));
         this._Func.Function = (Func_PWM.tenFUNCTION)Convert.ToUInt16(reader.GetAttribute("Function"));
         //this.gb_FunctionName.Text = reader.GetAttribute("CustomName");

         this.textBlock_Duration.Text = "Duration: " + this._Func.Duration_ms.ToString() + " (ms)";
         this.textBlock_MaxLevel.Text = "Max Level: " + this._Func.MaxLevel.ToString() + " (%)";
         this.textBlock_UpdateRate.Text = "Update Rate: " + this._Func.UpdateRate.ToString() + " (ms)";
         this.textBlock_StartDelay.Text = "Start Delay: " + this._Func.Delay_ms.ToString() + " (ms)";

         /* Ignore MIN/MAX limits. */
         try
         {
            this.slider_Duration.Value = (int)this._Func.Duration_ms;
            this.slider_StartDelay.Value = (int)this._Func.Delay_ms;
            this.slider_MaxLevel.Value = (int)this._Func.MaxLevel;
            this.slider_UpdateRate.Value = (int)this._Func.UpdateRate;
            this.comboBox_Functions.SelectedIndex = (int)this._Func.Function;
         }
         catch { }
      }

      public System.Xml.Schema.XmlSchema GetSchema()
      {
         throw new NotImplementedException();
      }

      public void WriteXml(System.Xml.XmlWriter writer)
      {
         writer.WriteAttributeString("Type", GetType().ToString());
         writer.WriteAttributeString("CustomName", this.textTitle.Text);

         this._Func.WriteXml(writer);
      } 
      #endregion

      public void SetCustomName()
      {
         //new PopupTextBox().SetCustomName(gb_FunctionName);
      }

   }
}
