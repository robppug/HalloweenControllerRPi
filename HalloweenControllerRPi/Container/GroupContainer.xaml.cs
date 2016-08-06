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

   public sealed partial class GroupContainer: UserControl
   {
      private bool _FuncAlwaysActive;
      /// <summary>
      /// Parameter defining wheither the GROUP contains 'always active' functions (ie. not triggered).
      /// </summary>
      public bool FuncAlwaysActive
      {
         get
         {
            return _FuncAlwaysActive;
         }
         set
         {
            _FuncAlwaysActive = value;

         }
      }

      public FlowDirection PanelFlowDirection
      {
         get
         {
            return Container.FlowDirection;
         }
         set
         {
            Container.FlowDirection = value;

         }
      }

      public GroupContainer()
      {
         this.InitializeComponent();
      }

      private void Panel_Loaded(object sender, RoutedEventArgs e)
      {
         if (FuncAlwaysActive == true)
         {
            Container.AllowDrop = true;
            Container.Drop += this.Panel_DragDrop;
            Container.DragEnter += this.Panel_DragEnter;
         }
         else
         {
            Container.AllowDrop = false;
            Container.Drop -= this.Panel_DragDrop;
            Container.DragEnter -= this.Panel_DragEnter;
         }
      }

      private async void Panel_DragDrop(object sender, DragEventArgs e)
      {
         Control FuncGUI;
         Function_Button draggedItem;
         Type[] allowedTypes = new Type[] { typeof(Function_Button_RELAY),
                                            typeof(Function_Button_PWM),
                                            typeof(Function_Button_SOUND)};
         /* Check if the dragged item is one of the allowed dragged item TYPES. */
         foreach (Type type in allowedTypes)
         {
            var items = await e.DataView.GetTextAsync(StandardDataFormats.Text);

            if (items != null)
            {
               if (type.ToString() == items.ToString())
               {
                  /* Only allow ONE of each function in the Always Actives group */
                  draggedItem = (Function_Button)Activator.CreateInstance(type, (uint)0/*draggedItem.Index*/, Function.tenTYPE.TYPE_CONSTANT);

                  //Container.Children.Add(draggedItem);
                  
                  /* Create an instance of the FUNCTION GUI */
                  FuncGUI = (Control)Activator.CreateInstance(draggedItem.GUIType, (this.Parent as IHostApp), draggedItem.Index, Function.tenTYPE.TYPE_CONSTANT);

                  Container.Children.Add(FuncGUI); 
                  return;
               }
            }
         }
      }

      private async void Panel_DragEnter(object sender, DragEventArgs e)
      {
         var items = await e.DataView.GetTextAsync(StandardDataFormats.Text);

         if (items != null)
         {
            if (items.ToString() != typeof(Function_Button_INPUT).ToString())
               e.AcceptedOperation = DataPackageOperation.Copy;
         }
      }
   }
}
