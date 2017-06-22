using HalloweenControllerRPi.Function_GUI;
using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Functions.Function_Button;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.Container
{
   /// <summary>
   ///  Class which defines the handling of a TRIGGER GROUP of FUNCTIONS.
   /// </summary>
   public partial class GroupContainerTriggered : UserControl, IXmlSerializable
   {
      public uint GroupIndex
      {
         get;
         private set;
      }

      public GroupContainerTriggered()
      {
         this.InitializeComponent();

         this.VerticalAlignment = VerticalAlignment.Top;
         this.HorizontalAlignment = HorizontalAlignment.Left;
         this.Margin = new Thickness(0);
      }

      public GroupContainerTriggered(uint idx) : this()
      {
         GroupIndex = idx;
      }

      private void Panel_Loaded(object sender, RoutedEventArgs e)
      {
         this.AllowDrop = true;
         this.Drop += this.Panel_DragDrop;
         this.DragEnter += this.Panel_DragEnter;
      }

      /// <summary>
      /// Function_Button ADD handling (via Drag)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Panel_DragDrop(object sender, DragEventArgs e)
      {
         Control FuncGUI;
         Function_Button draggedItem = (e.DataView.Properties["Object"] as Function_Button);

         /* Check if the dragged item is one of the allowed dragged item TYPES. */
         if (draggedItem != null)
         {
            /* Check if the dragged Function_Button has a count restriction. */
            if (draggedItem.OneOnly)
            {
               foreach (UIElement c in Container.Children)
               {
                  if (c.GetType() == draggedItem.GetType())
                     return;
               }
            }

            /* If Function Button is not STATIC, remove from sending flow panel (availables). */
            //if (draggedItem.IsRemoveable == true)
            //   draggedItem.Dispose();

            /* Only allow ONE of each function in the Always Actives group */
            //draggedItem = (Function_Button)Activator.CreateInstance(draggedItem.GetType(), draggedItem.Index, Function.tenTYPE.TYPE_TRIGGER);

            //Container.Children.Add(draggedItem);

            /* Create an instance of the FUNCTION GUI */
            FuncGUI = (Control)Activator.CreateInstance(draggedItem.GUIType, MainPage.HostApp, draggedItem.Index, Function.tenTYPE.TYPE_TRIGGER);
            
            AddFunctionToGroup(FuncGUI);
            return;
         }
      }

      /// <summary>
      /// UIElement DRAG Enter event, check if the dragged item is;
      ///   - Of type Function Button
      ///   - Has a Function that is configured as Only One
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Panel_DragEnter(object sender, DragEventArgs e)
      {
         Function_Button draggedObect = (e.DataView.Properties["Object"] as Function_Button);

         e.AcceptedOperation = DataPackageOperation.None;

         if (draggedObect != null)
         {
            if (draggedObect.OneOnly)
            {
               foreach (UIElement c in Container.Children)
               {
                  if (c is Func_Input_GUI)
                  {
                     return;
                  }
               }
            }

            e.AcceptedOperation = DataPackageOperation.Copy;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cFunc"></param>
      /// <param name="cFuncIndex"></param>
      /// <param name="u32FuncValue"></param>
      /// <returns></returns>
      public bool boProcessRequest(char func, char subFunc, uint index, uint value)
      {
         bool boValidTrigger = false;

         foreach (UIElement f in Container.Children)
         {
            /* Triggered - Compare triggering INPUT to assigned FUNCTION INPUTS and execute TRIGGER on matching INPUT number */
            if (f is Func_Input_GUI)
            {
               Func_Input_GUI inputGUI = (f as Func_Input_GUI);
               if ((inputGUI.Func.Index == index) && inputGUI.Func.boCheckTriggerConditions(value))
               {
                  boValidTrigger = true;
                  break;
               }
            }
         }

         if (boValidTrigger)
         {
            imageTrigger.Source = (BitmapImage)Resources["Triggered"];

            /* Trigger each FUNCTION within the Active Group */
            foreach (UIElement c in Container.Children)
            {
               if (c is IFunctionGUI)
               {
                  TriggerFunctions(c as IFunctionGUI);
               }
            }
         }
         return false;
      }

      /// <summary>
      /// Processes handling of assigned functions on detection of TRIGGER event.
      /// </summary>
      /// <param name="c">Triggering FUNCTION.</param>
      private void TriggerFunctions(IFunctionGUI c)
      {
         c.Func.boProcessRequest((char)0, (char)0, (char)0, (uint)0);
      }

      /// <summary>
      /// Handling of Trigger End event to check if all Triggered function have completed.
      /// </summary>
      public void CheckTriggerEnd()
      {
         /* Go through all Panel Group controls and check if control of used functions has completed */
         foreach (UIElement c in Container.Children)
         {
            if (c is IFunctionGUI)
            {
               if ((c as IFunctionGUI).Func.TriggerActive == true)
               {
                  return;
               }
            }
         }

         this.imageTrigger.Source = (BitmapImage)Resources["Blank"];
      }

      public XmlSchema GetSchema()
      {
         throw new NotImplementedException();
      }

      public void ReadXml(XmlReader reader)
      {
         throw new NotImplementedException();
      }

      public void WriteXml(System.Xml.XmlWriter writer)
      {
         foreach (UIElement c in Container.Children)
         {
            if (c is IFunctionGUI)
            {
               /* Store the FUNCTION type */
               writer.WriteStartElement("Function", (c as IFunctionGUI).Func.GetType().ToString());

               if (c is IXmlSerializable)
                  (c as IXmlSerializable).WriteXml(writer);

               writer.WriteEndElement();
            }
         }
      }

      public void AddFunctionToGroup(Control ctl)
      {
         if (ctl is IFunctionGUI)
         {
            Container.Children.Add(ctl);

            (ctl as IFunctionGUI).OnRemove += GroupContainerTriggered_OnRemove;
         }
         else
         {
            throw new Exception("Only controls of type IFunctionGUI can be added.");
         }
      }

      private void GroupContainerTriggered_OnRemove(object sender, EventArgs e)
      {
         Container.Children.Remove((sender as UIElement));
      }
   }
}
