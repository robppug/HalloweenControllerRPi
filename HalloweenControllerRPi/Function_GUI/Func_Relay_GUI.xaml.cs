using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.Function_GUI
{
   public partial class Func_Relay_GUI : UserControl, IXmlSerializable, IFunctionGUI
   {
      private Func_RELAY _Func;

      public Function Func
      {
         get { return this._Func; }
         set { this._Func = (value as Func_RELAY); }
      }

      public Func_Relay_GUI() { }

      public Func_Relay_GUI(IHostApp host, uint index, Function.tenTYPE entype)
      {
         _Func = new Func_RELAY(host, entype);
         this._Func.Index = index;

         InitializeComponent();

         //foreach (Control c in this.Controls)
         //{
         //   c.MouseDown += OnMouseDown;
         //}

         //this.gb_FunctionName.Text = "Relay #" + index;
         //this.gb_FunctionName.MouseClick += gb_FunctionName_MouseClick;
         this._Func.Duration_ms = (uint)slider_Duration.Value;
         this._Func.Delay_ms = (uint)slider_StartDelay.Value;

         /* If function is in an ALWAYS_ACTIVE group, disable pointless controls */
         if (entype == Func_RELAY.tenTYPE.TYPE_CONSTANT)
         {
            //this.label_Duration.Enabled = false;
            //this.label_StartDelay.Enabled = false;
            this.slider_Duration.IsEnabled = false;
            this.slider_StartDelay.IsEnabled = false;
         }

         this._Func.FuncButtonType = typeof(Function_Button_RELAY);
      }

      //private void gb_FunctionName_MouseClick(object sender, MouseEventArgs e)
      //{
      //   if (e.Button == System.Windows.Forms.MouseButtons.Right)
      //   {
      //      this.SetCustomName();
      //   }
      //}

      //private void OnMouseDown(object sender, MouseEventArgs e)
      //{
         //MessageBox.Show(e.ToString());
      //}

      private void trackBar_Duration_Scroll(object sender, EventArgs e)
      {
         this._Func.Duration_ms = (uint)(sender as Slider).Value;
         //this.label_Duration.Text = "Duration: " + this._Func.Duration_ms.ToString() + " (ms)";
      }

      private void trackBar_StartDelay_Scroll(object sender, EventArgs e)
      {
         this._Func.Delay_ms = (uint)(sender as Slider).Value;
         //this.label_StartDelay.Text = "Start Delay: " + this._Func.Delay_ms.ToString() + " (ms)";
      }

      public void ReadXml(System.Xml.XmlReader reader)
      {
         this._Func.Delay_ms = Convert.ToUInt16(reader.GetAttribute("Delay"));
         this._Func.Duration_ms = Convert.ToUInt16(reader.GetAttribute("Duration"));
         //this.gb_FunctionName.Text = reader.GetAttribute("CustomName");

         //this.label_StartDelay.Text = "Start Delay: " + this._Func.Delay_ms.ToString() + " (ms)";
         //this.label_Duration.Text = "Duration: " + this._Func.Duration_ms.ToString() + " (ms)";

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
         //writer.WriteAttributeString("CustomName", this.gb_FunctionName.Text);

         this._Func.WriteXml(writer);
      }

      public void SetCustomName()
      {
         //new PopupTextBox().SetCustomName(gb_FunctionName);
      }
   }
}
