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

         textBlock.Text = text;
         //button_Function.MouseDown += new MouseEventHandler(b_MouseDown);
         //button_Function.MouseMove += new MouseEventHandler(b_MouseMove);
         //MouseDown += Function_Button_MouseDown;
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
         textBlock.Text = text + Convert.ToString(" #" + index.ToString());
      }

      public void SetImage(Image img)
      {
         //button_Function.Image = img;
      }

      public System.Xml.Schema.XmlSchema GetSchema()
      {
         throw new NotImplementedException();
      }

      virtual public void ReadXml(XmlReader reader)
      {
         /* Load and process the data in the handling XML Reader */
         //(_funcGUI as IXmlSerializable).ReadXml(reader);
      }

      virtual public void WriteXml(XmlWriter writer)
      {
         writer.WriteAttributeString("Type", GetType().ToString());
         writer.WriteAttributeString("Index", this.Index.ToString());

         //(_funcGUI as IXmlSerializable).WriteXml(writer);
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
