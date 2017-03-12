using HalloweenControllerRPi.Function_GUI;
using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi
{
   /// <summary>
   /// Class handling the available FUNCTION GUI processes.
   /// </summary>
   public partial class Function_Button : UserControl, IXmlSerializable
   {
      public Type GUIType;

      public bool IsRemoveable { get; set; }
      public bool OneOnly { get; set; }
      public uint Index { get; set; }
      public string ToolTip { get; set; }
      public Brush FillColour { get; set; }

      public Function_Button()
      {
         this.InitializeComponent();
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

         ToolTip = text + " #" + index.ToString("00");
         FillColour = new SolidColorBrush(color);

         this.Loaded += Function_Button_Loaded;
      }

      private void Function_Button_Loaded(object sender, RoutedEventArgs e)
      {
         /* Set BINDING of objects */
         buttonText.DataContext = this;
         rectBackground.DataContext = this;
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
         args.Data.SetData("Type", this.GetType().ToString());
         args.Data.SetData("Index", this.Index);
         args.Data.SetData("OnlyOne", this.OneOnly);
         args.Data.SetData("IsRemovable", this.IsRemoveable);
      }
   }
}
