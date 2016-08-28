﻿using HalloweenControllerRPi.Functions;
using System;
using System.Xml.Serialization;
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

      public Function Func
      {
         get { return this._Func; }
         set { this._Func = (value as Func_INPUT); }
      }
      
      public Func_Input_GUI()
      {
         this.InitializeComponent();

         _boInitialised = true;
      }

      public Func_Input_GUI(IHostApp host, uint index, Function.tenTYPE entype) : this()
      {
         this._Func = new Func_INPUT(host, entype);
         this._Func.Index = index;
         this._Func.FuncButtonType = typeof(Function_Button_INPUT);

         this.textTitle.Text = "Input #" + index;
         this.textTitle.DoubleTapped += TextTitle_DoubleTapped;

         this.comboBox_TrigEdge.Items.Add("Low (GND)");
         this.comboBox_TrigEdge.Items.Add("High (5V)");
         this.comboBox_TrigEdge.SelectedIndex = 0;
      }

      private void TextTitle_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
      {
         if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
         {
            this.SetCustomName();
         }
      }

      private void slider_Debounce_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
      {
         if (_boInitialised == true)
         {
            this._Func.DebounceTime_ms = (uint)(sender as Slider).Value;
            this.textDebounce.Text = "Debounce Time: " + this._Func.DebounceTime_ms.ToString() + " (ms)";
         }
      }

      #region XML Handling
      private void comboBox_TrigEdge_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         this._Func.TriggerLevel = (Func_INPUT.tenTriggerLvl)(sender as ComboBox).SelectedIndex;
      }

      public void ReadXml(System.Xml.XmlReader reader)
      {
         this.textTitle.Text = reader.GetAttribute("CustomName");

         this._Func.ReadXml(reader);

         this.comboBox_TrigEdge.SelectedIndex = (int)this._Func.TriggerLevel;
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

      private void UserControl_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
      {
         //this._Func.TriggerLevel = (Func_INPUT.tenTriggerLvl)(sender as ComboBox).SelectedIndex;
         //this._Func.DebounceTime_ms = (uint)(sender as Slider).Value;
      }
   }
}
