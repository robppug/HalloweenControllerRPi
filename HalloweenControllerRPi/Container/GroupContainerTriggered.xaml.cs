using Gregstoll;
using HalloweenControllerRPi.Function_GUI;
using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
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
      private async void Panel_DragDrop(object sender, DragEventArgs e)
      {
         Control FuncGUI;
         Function_Button draggedItem;
         Type[] allowedTypes = new Type[] { typeof(Function_Button_RELAY),
                                            typeof(Function_Button_PWM),
                                            typeof(Function_Button_SOUND),
                                            typeof(Function_Button_INPUT) };

         /* Check if the dragged item is one of the allowed dragged item TYPES. */
         foreach (Type type in allowedTypes)
         {
            if (e.DataView.Contains("Type"))
            {
               var items = await e.DataView.GetDataAsync("Type");

               if (type.ToString() == items.ToString())
               {
                  bool onlyOne = (bool)await e.DataView.GetDataAsync("OnlyOne");
                  uint idx = 0;

                  if (e.DataView.Contains("Index"))
                     idx = (uint)await e.DataView.GetDataAsync("Index");

                  /* Check if the dragged Function_Button has a count restriction. */
                  if (onlyOne == true)
                  {
                     foreach (UIElement c in Container.Children)
                     {
                        if (c is Func_Input_GUI)
                           return;
                     }
                  }

                  //RPUGLIESE - TODO
                  /* If Function Button is not STATIC, remove from sending flow panel (availables). */
                  //if (draggedItem.IsRemoveable == true)
                  //   draggedItem.Dispose();

                  /* Only allow ONE of each function in the Always Actives group */
                  draggedItem = (Function_Button)Activator.CreateInstance(type, idx, Function.tenTYPE.TYPE_TRIGGER);

                  //Container.Children.Add(draggedItem);

                  /* Create an instance of the FUNCTION GUI */
                  FuncGUI = (Control)Activator.CreateInstance(draggedItem.GUIType, MainPage.HostApp, draggedItem.Index, Function.tenTYPE.TYPE_TRIGGER);

                  Container.Children.Add(FuncGUI);
                  return;
               }
            }
         }
      }

      /// <summary>
      /// UIElement DRAG Enter event, check if the dragged item is;
      ///   - Of type Function Button
      ///   - Has a Function that is configured as Only One
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private async void Panel_DragEnter(object sender, DragEventArgs e)
      {
         if (e.DataView.Contains("Type"))
         { 
            var items = await e.DataView.GetTextAsync("Type");

            if (Type.GetType(items).GetTypeInfo().IsSubclassOf(typeof(Function_Button)))
            {
               bool onlyOne = (bool)await e.DataView.GetDataAsync("OnlyOne");

               /* Check if the dragged Function_Button has a count restriction. */
               if (onlyOne == true)
               {
                  foreach (UIElement c in Container.Children)
                  {
                     if (c is Func_Input_GUI)
                     {
                        e.AcceptedOperation = DataPackageOperation.None;
                        return;
                     }
                  }
               }

               e.AcceptedOperation = DataPackageOperation.Copy;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cFunc"></param>
      /// <param name="cFuncIndex"></param>
      /// <param name="u32FuncValue"></param>
      /// <returns></returns>
      public bool boProcessRequest(char cFunc, char cFuncIndex, uint u32FuncValue)
      {
         bool boValidTrigger = false;

         foreach (UIElement f in Container.Children)
         {
            /* Triggered - Compare triggering INPUT to assigned FUNCTION INPUTS and execute TRIGGER on matching INPUT number */
            if (f is Func_Input_GUI)
            {
               if (((f as Func_Input_GUI).Func.Index == cFuncIndex) && (f as Func_Input_GUI).Func.boCheckTriggerConditions(u32FuncValue))
               {
                  boValidTrigger = true;
                  break;
               }
            }
         }

         if (boValidTrigger)
         {
            this.imageTrigger.Source = new BitmapImage(new Uri("ms-appx:///Assets/trigger.png"));

            /* Trigger each FUNCTION within the Active Group */
            foreach(UIElement c in Container.Children)
            {
               if(c is IFunctionGUI)
                  TriggerFunctions(c as IFunctionGUI);
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
         c.Func.boProcessRequest((char)0, (char)0, (uint)0);
      }

      /// <summary>
      /// Handling of Trigger End event callback from ending FUNCTION.
      /// </summary>
      /// <param name="func"></param>
      public void TriggerEnd(Function func)
      {
         /* Go through all Panel Group controls and check if control of used functions has completed */
         foreach (UIElement c in Container.Children)
         {
            if (c is IFunctionGUI)
            {
               if ((c as IFunctionGUI).Func == func)
               {
                  this.imageTrigger.Source = null;
                  return;
               }
            }
         }
      }

      public System.Xml.Schema.XmlSchema GetSchema()
      {
         throw new NotImplementedException();
      }

      public void ReadXml(System.Xml.XmlReader reader)
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
         //Function_Button funcButton;

         if (ctl is IFunctionGUI)
         {
            /* Create and instance of the Function_Button */
            //funcButton = (Function_Button)Activator.CreateInstance(  (ctl as IFunctionGUI).Func.FuncButtonType, 
            //                                                         (ctl as IFunctionGUI).Func.Index, 
            //                                                         Function.tenTYPE.TYPE_TRIGGER);

            //funcButton.Height = ctl.Height;

            /* Add the Function_Button and FUNCTION_GUI to the group Panel */
            //Panel.Controls.Add(funcButton);

            Container.Children.Add(ctl);

            //this.Panel.SetFlowBreak(ctl, true);

            //Panel.AutoSize = true;
         }
         else
         {
            throw new Exception("Only controls of type IFunctionGUI can be added.");
         }
      }
   }
}
