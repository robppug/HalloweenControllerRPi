using Gregstoll;
using HalloweenControllerRPi.Function_GUI;
using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
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
   public sealed partial class GroupContainerTriggered : UserControl
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
                     foreach (UIElement c in (sender as GroupContainerTriggered).Container.Children)
                     {
                        if (c is Func_Input_GUI)
                           return;
                     }
                  }

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
         var items = await e.DataView.GetTextAsync("Type");

         if (items != null)
         {
            if (Type.GetType(items).GetTypeInfo().IsSubclassOf(typeof(Function_Button)))
            {
               bool onlyOne = (bool)await e.DataView.GetDataAsync("OnlyOne");

               /* Check if the dragged Function_Button has a count restriction. */
               if (onlyOne == true)
               {
                  foreach (UIElement c in (sender as GroupContainerTriggered).Container.Children)
                  {
                     if (c is Func_Input_GUI)
                     {
                        e.DragUIOverride.Caption = "Input already assigned...";
                        e.DragUIOverride.IsCaptionVisible = true;
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

         foreach (UIElement f in this.Container.Children)
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
            foreach (UIElement c in this.Container.Children)
            {
               TriggerFunctions(c);
            }
         }

         return false;
      }

      /// <summary>
      /// Processes handling of assigned functions on detection of TRIGGER event.
      /// </summary>
      /// <param name="c">Triggering FUNCTION.</param>
      private void TriggerFunctions(UIElement c)
      {
         if (c is IFunctionGUI)
         {
            (c as IFunctionGUI).Func.boProcessRequest((char)0, (char)0, (uint)0);
         }
         else
         {
            //foreach (UIElement sub in c.Controls)
            //   TriggerFunctions(sub);
         }
      }
   }
}
