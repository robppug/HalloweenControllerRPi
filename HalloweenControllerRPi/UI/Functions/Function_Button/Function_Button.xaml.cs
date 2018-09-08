using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using System.Xml.Serialization;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.UI.Functions.Function_Button
{
   /// <summary>
   /// Class handling the available FUNCTION GUI processes.
   /// </summary>
   public partial class Function_Button : UserControl, IXmlSerializable
   {
      public Type GUIType;

      public bool IsRemoveable { get; set; }
      public bool OneOnly { get; set; }
      public bool TriggerOnly { get; set; }
      public uint Index { get; set; }

      public static readonly DependencyProperty ToolTipProperty =
         DependencyProperty.Register("ToolTip", typeof(String), typeof(Function_Button), new PropertyMetadata(""));

      public string ToolTip
      {
         get;// { return (String)GetValue(ToolTipProperty); }
         set;// { SetValue(ToolTipProperty, value); }
      }

      public static readonly DependencyProperty FillColourProperty =
         DependencyProperty.Register("FillColour", typeof(Brush), typeof(Function_Button), new PropertyMetadata(null));


      public Brush FillColour
      {
         get;// { return (Brush)GetValue(FillColourProperty); }
         set;// { SetValue(FillColourProperty, value); }
      }

      public Function_Button()
      {
         InitializeComponent();
      }


      /// <summary>
      /// Function_Button Constructor
      /// </summary>
      /// <param name="guitype"></param>
      /// <param name="text"></param>
      /// <param name="color"></param>
      public Function_Button(Type guitype, string text, Color color) : this()
      {
         GUIType = guitype;

         Index = 0;
      }

      /// <summary>
      /// Function_Button Constructor
      /// </summary>
      /// <param name="guitype"></param>
      /// <param name="text"></param>
      /// <param name="index"></param>
      /// <param name="color"></param>
      public Function_Button(Type guitype, string text, uint index, Color color)
         : this(guitype, text, color)
      {
         Index = index;

         FillColour = new SolidColorBrush(color);
         ToolTip = text + " #" + index.ToString("00");

         /* Set BINDING of objects */
         FunctionButtonBackground.DataContext = this;
         FunctionButtonText.DataContext = this;
      }

      public System.Xml.Schema.XmlSchema GetSchema()
      {
         throw new NotImplementedException();
      }

      virtual public void ReadXml(XmlReader reader)
      {
      }

      virtual public void WriteXml(XmlWriter writer)
      {
         writer.WriteAttributeString("Type", GetType().ToString());
         writer.WriteAttributeString("Index", this.Index.ToString("00"));
      }

      internal void OnDragStarting(object sender, DragItemsStartingEventArgs args)
      {
         args.Data.RequestedOperation = DataPackageOperation.Copy;
         args.Data.Properties.Add("Object", this);
      }
   }
}
