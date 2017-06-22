using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Functions.Func_GUI;
using System;
using System.Xml.Serialization;
using Windows.UI.Xaml;
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

      public event EventHandler OnRemove;

      public Function Func
      {
         get { return _Func; }
         set { _Func = (value as Func_RELAY); }
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

         RemoveButton.Click += RemoveButton_Click;
      }

      private void RemoveButton_Click(object sender, RoutedEventArgs e)
      {
         OnRemove?.Invoke(this, EventArgs.Empty);
      }

      private void TextTitle_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
      {
         if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
         {
            textTitle.Text = FuncGUIHelper.SetCustomName(textTitle.Text).Result;
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
            textBlock_StartDelay.Text = "Start Delay: " + Func.Delay_ms.ToString() + " (ms)";
         }
      }

      #region XML Handling
      public void ReadXml(System.Xml.XmlReader reader)
      {
         _Func.ReadXml(reader);

         textTitle.Text = reader.GetAttribute("CustomName");

         textBlock_StartDelay.Text = "Start Delay: " + _Func.Delay_ms.ToString() + " (ms)";
         textBlock_Duration.Text = "Duration: " + _Func.Duration_ms.ToString() + " (ms)";

         /* Ignore MIN/MAX limits. */
         try
         {
            slider_Duration.Value = (int)_Func.Duration_ms;
            slider_StartDelay.Value = (int)_Func.Delay_ms;
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
         writer.WriteAttributeString("CustomName", textTitle.Text);

         _Func.WriteXml(writer);
      }

      #endregion

      public void Initialise()
      {
      }
   }
}
