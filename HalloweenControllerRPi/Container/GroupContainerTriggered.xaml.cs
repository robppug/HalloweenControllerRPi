using Gregstoll;
using HalloweenControllerRPi.Function_GUI;
using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
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
      }
      public GroupContainerTriggered(uint idx) : this()
      {
         GroupIndex = idx;
      }

      private void Panel_Loaded(object sender, RoutedEventArgs e)
      {
         Container.AllowDrop = true;
         Container.Drop += this.Panel_DragDrop;
         Container.DragEnter += this.Panel_DragEnter;
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
                     foreach (UIElement c in (sender as UniversalWrapPanel).Children)
                     {
                        if (c is Function_Button_INPUT)
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
                  FuncGUI = (Control)Activator.CreateInstance(draggedItem.GUIType, (this.Parent as IHostApp), draggedItem.Index, Function.tenTYPE.TYPE_TRIGGER);

                  Container.Children.Add(FuncGUI);
                  return;
               }
            }
         }
      }

      private async void Panel_DragEnter(object sender, DragEventArgs e)
      {
         var items = await e.DataView.GetTextAsync("Type");

         if (items != null)
         {
            if (items.ToString() == typeof(Function_Button).ToString())
               e.AcceptedOperation = DataPackageOperation.Copy;
         }
      }
   }
}
