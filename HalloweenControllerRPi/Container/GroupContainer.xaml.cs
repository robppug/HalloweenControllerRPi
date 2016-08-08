﻿using HalloweenControllerRPi.Function_GUI;
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
            this.AllowDrop = true;
            this.Drop += this.Panel_DragDrop;
            this.DragEnter += this.Panel_DragEnter;
         }
         else
         {
            this.AllowDrop = false;
            this.Drop -= this.Panel_DragDrop;
            this.DragEnter -= this.Panel_DragEnter;
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
            if (e.DataView.Contains("Type"))
            {
               var items = await e.DataView.GetDataAsync("Type");

               if (type.ToString() == items.ToString())
               {
                  uint idx = 0;

                  if(e.DataView.Contains("Index"))
                     idx = (uint)await e.DataView.GetDataAsync("Index");

                  /* Only allow ONE of each function in the Always Actives group */
                  draggedItem = (Function_Button)Activator.CreateInstance(type, idx, Function.tenTYPE.TYPE_CONSTANT);

                  //Container.Children.Add(draggedItem);
                  
                  /* Create an instance of the FUNCTION GUI */
                  FuncGUI = (Control)Activator.CreateInstance(draggedItem.GUIType, MainPage.HostApp, draggedItem.Index, Function.tenTYPE.TYPE_CONSTANT);

                  Container.Children.Add(FuncGUI); 
                  return;
               }
            }
         }
      }

      private async void Panel_DragEnter(object sender, DragEventArgs e)
      {
         if (e.DataView.Contains("Type"))
         {
            var items = await e.DataView.GetTextAsync("Type");

            if (items.ToString() != typeof(Function_Button_INPUT).ToString())
            {
               e.DragUIOverride.Caption = "Add to Always Active Group...";
               e.DragUIOverride.IsCaptionVisible = true;
               e.AcceptedOperation = DataPackageOperation.Copy;
            }
         }
      }

      /// <summary>
      /// Adds a new Trigger group with the specified INDEX.
      /// </summary>
      /// <param name="idx"></param>
      public void AddTriggerGroup(uint idx)
      {
         GroupContainerTriggered groupContainerTriggered = new GroupContainerTriggered(idx);
         groupContainerTriggered.HorizontalAlignment = HorizontalAlignment.Stretch;

         Container.Children.Add(groupContainerTriggered);
      }

      /// <summary>
      /// Adds a new Trigger group with the next availble INDEX.
      /// </summary>
      public void AddTriggerGroup()
      {
         AddTriggerGroup((uint)Container.Children.Count);
      }

      /// <summary>
      /// Trigger received, parse through all Trigger groups and fire the trigger command.
      /// </summary>
      /// <param name="cFunc"></param>
      /// <param name="cIndex"></param>
      /// <param name="u32Value"></param>
      public void ProcessTrigger(char cFunc, char cIndex, uint u32Value)
      {
         foreach (GroupContainerTriggered gt in this.Container.Children)
         {
            gt.boProcessRequest(cFunc, cIndex, u32Value);
         }
      }
   }
}
