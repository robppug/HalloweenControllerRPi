using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
   public sealed partial class Func_Input_GUI : UserControl
   {
      private Func_INPUT _Func;

      public Function Func
      {
         get { return this._Func; }
         set { this._Func = (value as Func_INPUT); }
      }

      public Func_Input_GUI()
      {
         this.InitializeComponent();
      }
      public Func_Input_GUI(IHostApp host, uint index, Function.tenTYPE entype)
      {
         this._Func = new Func_INPUT(host, entype);
         this._Func.Index = index;
         this._Func.FuncButtonType = typeof(Function_Button_INPUT);

         this.InitializeComponent();

         //foreach (Control c in this.Controls)
         //{
         //   c.MouseDown += OnMouseDown;
         //}

         //this.gb_FunctionName.Text = "Input #" + index;
         //this.gb_FunctionName.MouseClick += gb_FunctionName_MouseClick;

         //this.comboBox_TrigEdge.SelectedIndex = 0;
      }
      //private void gb_FunctionName_MouseClick(object sender, MouseEventArgs e)
      //{
         //if (e.Button == System.Windows.Forms.MouseButtons.Right)
         //{
         //   this.SetCustomName();
         //}
      //}
      private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
      {
         //this._Func.triggerLevel = (Func_INPUT.TriggerLvl)(sender as ComboBox).SelectedIndex;
      }

   }
}
