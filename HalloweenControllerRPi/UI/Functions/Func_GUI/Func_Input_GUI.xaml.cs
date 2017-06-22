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
   public sealed partial class Func_Input_GUI : UserControl, IXmlSerializable, IFunctionGUI
   {
      private Func_INPUT _Func;
      private bool _boInitialised = false;

      public event EventHandler OnRemove;

      public Function Func
      {
         get { return _Func; }
         set { _Func = (value as Func_INPUT); }
      }
      
      public Func_Input_GUI()
      {
         InitializeComponent();

         _boInitialised = true;
      }

      public Func_Input_GUI(IHostApp host, uint index, Function.tenTYPE entype) : this()
      {
         _Func = new Func_INPUT(host, entype);
         _Func.Index = index;
         _Func.FuncButtonType = typeof(Function_Button_INPUT);

         textTitle.Text = "Input #" + index;
         textTitle.DoubleTapped += TextTitle_DoubleTapped;

         comboBox_TrigEdge.Items.Add("Low (GND)");
         comboBox_TrigEdge.Items.Add("High (VCC)");
         comboBox_TrigEdge.SelectedIndex = 1;
         _Func.TriggerLevel = Func_INPUT.tenTriggerLvl.tHigh;

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

      private void slider_Debounce_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            _Func.DebounceTime_ms = (uint)(sender as Slider).Value;
            textBlock_Debounce.Text = "Debounce Time: " + _Func.DebounceTime_ms.ToString() + " (ms)";
         }
      }

      private void slider_PostDelay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            _Func.PostTriggerDelay_ms = (uint)(sender as Slider).Value;
            textBlock_PostDelay.Text = "Post Trigger Time: " + _Func.PostTriggerDelay_ms.ToString() + " (ms)";
         }
      }

      #region XML Handling
      private void comboBox_TrigEdge_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         _Func.TriggerLevel = (Func_INPUT.tenTriggerLvl)(sender as ComboBox).SelectedIndex;
      }

      public void ReadXml(System.Xml.XmlReader reader)
      {
         _Func.ReadXml(reader);

         textTitle.Text = reader.GetAttribute("CustomName");

         textBlock_Debounce.Text = "Debounce Time: " + _Func.DebounceTime_ms.ToString() + " (ms)";
         textBlock_PostDelay.Text = "Post Trigger Time: " + _Func.PostTriggerDelay_ms.ToString() + " (ms)";

         /* Ignore MIN/MAX limits. */
         try
         {
            comboBox_TrigEdge.SelectedIndex = (int)_Func.TriggerLevel;
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

      private void UserControl_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
      {
         //this._Func.TriggerLevel = (Func_INPUT.tenTriggerLvl)(sender as ComboBox).SelectedIndex;
         //this._Func.DebounceTime_ms = (uint)(sender as Slider).Value;
      }

      public void Initialise()
      {
         
      }
   }
}
