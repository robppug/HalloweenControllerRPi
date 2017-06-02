using HalloweenControllerRPi.Device;
using HalloweenControllerRPi.Function_GUI;
using HalloweenControllerRPi.Functions;
using HalloweenControllerRPi.UI.Functions.Function_Button;
using System;
using System.Xml.Serialization;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.Container
{
   /// <summary>
   /// Class which defines the handling of GROUPs (panels) which contains Control FUNCTIONS (Trigger/AlwaysActive)
   /// </summary>
   public partial class GroupContainer : UserControl, IXmlSerializable
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

      /// <summary>
      /// FUNCTION DROP event handling when the user drags an Available Function Button into the GroupContainer.
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
            if (draggedItem.TriggerOnly == false)
            {
               /* Only allow ONE of each function in the Always Actives group */
               draggedItem = (Function_Button)Activator.CreateInstance(draggedItem.GetType(), draggedItem.Index, Function.tenTYPE.TYPE_CONSTANT);

               //Container.Children.Add(draggedItem);

               /* Create an instance of the FUNCTION GUI */
               FuncGUI = (Control)Activator.CreateInstance(draggedItem.GUIType, MainPage.HostApp, draggedItem.Index, Function.tenTYPE.TYPE_CONSTANT);

               Container.Children.Add(FuncGUI);
               return;
            }
         }
      }

      private void Panel_DragEnter(object sender, DragEventArgs e)
      {
         Function_Button draggedObect = (e.DataView.Properties["Object"] as Function_Button);

         e.AcceptedOperation = DataPackageOperation.None;

         if (draggedObect != null)
         {
            if(draggedObect.TriggerOnly == false)
               e.AcceptedOperation = DataPackageOperation.Copy;
         }
      }

      public void ProcessAlwaysActives(bool boStart)
      {
         foreach (Control c in Container.Children)
         {
            TriggerFunctions(c, boStart);
         }
      }

      private void TriggerFunctions(Control c, bool boStart)
      {
         if (c is IFunctionGUI)
         {
            Function func = (c as IFunctionGUI).Func;

            if (boStart)
               func.boProcessRequest((char)0, (char)0, (uint)0);
            else
               func.vStopFunction((char)0, (char)0, (uint)0);
         }
         else
         {
            //RPUGLIESE - TODO
            //foreach (Control sub in c.Controls)
               TriggerFunctions(c, boStart);
         }
      }

      public void CheckTriggerEnd()
      {
         foreach (GroupContainerTriggered gt in this.Container.Children)
         {
            gt.CheckTriggerEnd();
         }
      }

      public void TriggerEnd(Function func)
      {
         /* Go through all Always Actives and check if control of used functions has completed */
         foreach (Control f in Container.Children)
         {
            if (f is IFunctionGUI)
            {
               if(   ((f as IFunctionGUI).Func.GetType() == func.GetType())
                  && ((f as IFunctionGUI).Func.Index == func.Index))
               {
                  TriggerFunctions(f, true);
               }
            }
            //else if (f is GroupContainerTriggered)
            //{
            //   (f as GroupContainerTriggered).TriggerEnd(func);
            //}
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
      public void ProcessTrigger(CommandEventArgs args)
      {
         foreach (GroupContainerTriggered gt in this.Container.Children)
         {
            gt.boProcessRequest(args.Commamd, args.Index, args.Value);
         }
      }

      public void ClearAllFunctions()
      {
         Container.Children.Clear();
      }

      /// <summary>
      /// Adds a FUNCTION to a USER created Trigger GROUP.
      /// </summary>
      /// <param name="groupIdx">Trigger Group INDEX</param>
      /// <param name="ctl"></param>
      public void AddFunctionToTriggerGroup(uint groupIdx, Control ctl)
      {
         if (FuncAlwaysActive == true)
         {
            if (ctl is IFunctionGUI)
            {
               Container.Children.Add(ctl);
            }
         }
         else
         {
            foreach (GroupContainerTriggered gt in Container.Children)
            {
               if (gt.GroupIndex == groupIdx)
               {
                  gt.AddFunctionToGroup(ctl);
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

      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="writer"></param>
      public void WriteXml(System.Xml.XmlWriter writer)
      {
         uint groupCountIdx = 0;

         /* Store all FUNCTIONS in this group */
         foreach (Control c in Container.Children)
         {
            /* ALWAYS ACTIVE FUNCTIONS */
            if (FuncAlwaysActive == true)
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
            /* TRIGGER FUNCTIONS */
            else
            {
               if (c is GroupContainerTriggered)
               {
                  writer.WriteStartElement("Group");
                  writer.WriteAttributeString("Index", groupCountIdx.ToString());

                  if (c is IXmlSerializable)
                     (c as IXmlSerializable).WriteXml(writer);

                  groupCountIdx++;
                  writer.WriteEndElement();
               }
            }
         }
      }
   }
}
