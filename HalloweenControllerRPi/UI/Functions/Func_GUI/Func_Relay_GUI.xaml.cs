using HalloweenControllerRPi.Functions;
using System;
using System.Xml.Serialization;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.Function_GUI
{
   public partial class Func_Relay_GUI : UserControl, IXmlSerializable, IFunctionGUI
   {
      private Func_RELAY _Func;
      private bool _boInitialised = false;

      public Function Func
      {
         get { return this._Func; }
         set { this._Func = (value as Func_RELAY); }
      }

      public Func_Relay_GUI()
      {
         this.InitializeComponent();

         _boInitialised = true;
      }

      public Func_Relay_GUI(IHostApp host, uint index, Function.tenTYPE entype) : this()
      {
         _Func = new Func_RELAY(host, entype);

         _Func.Index = index;
         
         textTitle.Text = "Relay #" + index;
         textTitle.DoubleTapped += TextTitle_DoubleTapped;
         _Func.Duration_ms = (uint)slider_Duration.Value;
         _Func.Delay_ms = (uint)slider_StartDelay.Value;

         /* If function is in an ALWAYS_ACTIVE group, disable pointless controls */
         if (entype == Func_RELAY.tenTYPE.TYPE_CONSTANT)
         {
            slider_Duration.IsEnabled = false;
            slider_StartDelay.IsEnabled = false;
         }

         _Func.FuncButtonType = typeof(Function_Button_RELAY);
      }

      private void TextTitle_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
      {
         if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
         {
            SetCustomName();
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

      private void slider_StartDelay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            this._Func.Delay_ms = (uint)(sender as Slider).Value;
            this.textBlock_StartDelay.Text = "Start Delay: " + this._Func.Delay_ms.ToString() + " (ms)";
         }
      }

      #region XML Handling
      public void ReadXml(System.Xml.XmlReader reader)
      {
         this._Func.Delay_ms = Convert.ToUInt16(reader.GetAttribute("Delay"));
         this._Func.Duration_ms = Convert.ToUInt16(reader.GetAttribute("Duration"));
         this.textTitle.Text = reader.GetAttribute("CustomName");

         this.textBlock_StartDelay.Text = "Start Delay: " + this._Func.Delay_ms.ToString() + " (ms)";
         this.textBlock_Duration.Text = "Duration: " + this._Func.Duration_ms.ToString() + " (ms)";

         /* Ignore MIN/MAX limits. */
         try
         {
            this.slider_Duration.Value = (int)this._Func.Duration_ms;
            this.slider_StartDelay.Value = (int)this._Func.Delay_ms;
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
      public async void SetCustomName()
      {
         ContentDialog cd = new ContentDialog();
         StackPanel panel = new StackPanel();
         TextBox tb = new TextBox() { Text = this.textTitle.Text };
         panel.Orientation = Orientation.Vertical;
         panel.Children.Add(tb);

         cd.Title = "Enter Custom Name";
         cd.PrimaryButtonText = "OK";
         cd.PrimaryButtonClick += (sender, e) =>
         {
            this.textTitle.Text = tb.Text;
         };
         cd.Content = panel;
         await cd.ShowAsync();
      }

      public void Initialise()
      {
         throw new NotImplementedException();
      }
   }
}
