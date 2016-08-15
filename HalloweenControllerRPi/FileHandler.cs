using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;

namespace HalloweenControllerRPi
{
   public partial class MainPage
   {
      private StorageFile fileToLoad;

      #region "XML Loading"
      private async void buttonLoadSequence_Click(object sender, RoutedEventArgs e)
      {
         FileOpenPicker fileDialog = new FileOpenPicker();

         fileDialog.ViewMode = PickerViewMode.List;
         fileDialog.FileTypeFilter.Add(".sqn");

         fileToLoad = await fileDialog.PickSingleFileAsync();

         if (fileToLoad != null)
         {
            StreamReader loadfile = new StreamReader(await fileToLoad.OpenStreamForReadAsync());
            XmlReader xmlReader = XmlReader.Create(loadfile);

            XDocument xDoc = XDocument.Load(XmlReader.Create(new StreamReader(await fileToLoad.OpenStreamForReadAsync())));

            //foreach (TabPage tab in tabControl_Groups.Controls.OfType<TabPage>())
            //{
            //   foreach (Control c in tab.Controls)
            //   {
            //      if (c is GroupContainer)
            //         (c as GroupContainer).ClearAllFunctions();
            //   }
            //}

            //DrawingControl.SuspendDrawing(groupContainer_AlwaysActive);
            //DrawingControl.SuspendDrawing(groupContainer_Trigger);

            //pbControl = new ProgressBarControl();
            //pbControl.Title = "Loading XML file...";

            //pbControl.MaxValue = xDoc.Descendants().Count();
            //pbControl.Show(this);

            this.ReadXml(xmlReader);

            //DrawingControl.ResumeDrawing(groupContainer_AlwaysActive);
            //DrawingControl.ResumeDrawing(groupContainer_Trigger);

            loadfile.Dispose();
                  
         }
      }

      public void ReadXml(System.Xml.XmlReader reader)
      {
         //TabPage tabPage = null;
         //string data = "";
         //uint groupIdx = 0;
         //int noOfElements = 1;

         //if (reader.ReadToFollowing("HalloweenController.Form_MAIN") == true)
         //{
         //   data = reader.GetAttribute("Version");
         //   pbControl.Progress = noOfElements++;

         //   if (data == "0.4")
         //   {
         //      string groupType = null;

         //      /* Reload available FUNCTION_BUTTONS */
         //      //this.flowLayoutPanel_Available.Controls.Clear();

         //      /* Detect connected BOARD */
         //      //GetBoardType();

         //      //Thread.Sleep(1000);

         //      //Application.DoEvents();

         //      //Thread.Sleep(1000);

         //      /* Read and create FUNCTIONS in the order stored in the XML */
         //      while (reader.Read())
         //      {
         //         if (reader.NodeType == XmlNodeType.Element)
         //         {
         //            pbControl.Progress = noOfElements++;

         //            /* Allow the GUI to update */
         //            Application.DoEvents();

         //            switch (reader.Name)
         //            {
         //               /* Tab Page found */
         //               case "TabPage":
         //                  if (reader.NamespaceURI == "AlwaysActive")
         //                  {
         //                     groupType = "AlwaysActive";
         //                     tabPage = (TabPage)tabControl_Groups.GetControl(0);
         //                  }
         //                  else if (reader.NamespaceURI == "Trigger")
         //                  {
         //                     groupType = "Trigger";
         //                     tabPage = (TabPage)tabControl_Groups.GetControl(1);
         //                  }

         //                  foreach (Control c in tabPage.Controls)
         //                  {
         //                     if (c is GroupContainer)
         //                        (c as GroupContainer).ClearAllFunctions();
         //                  }
         //                  break;

         //               /* GROUP Containter/Trigger found */
         //               case "Group":
         //                  /* Get Index */
         //                  groupIdx = Convert.ToUInt32(reader.GetAttribute("Index"));

         //                  if (groupType == "Trigger")
         //                  {
         //                     foreach (Control c in tabPage.Controls)
         //                     {
         //                        if (c is GroupContainer)
         //                           (c as GroupContainer).AddTriggerGroup(groupIdx);
         //                     }
         //                  }
         //                  break;

         //               /* FUNCTION GUI found */
         //               case "Function":
         //                  if (reader.MoveToNextAttribute())
         //                  {
         //                     Control ctl;

         //                     /* Check NODE - Function Type */
         //                     if (reader.Name == "Type")
         //                     {
         //                        /* Create and INSTANCE of the function */
         //                        Type cType = Type.GetType(reader.GetAttribute("Type"));
         //                        UInt16 cIndex = Convert.ToUInt16(reader.GetAttribute("Index"));

         //                        if (cType.BaseType == typeof(Function_Button))
         //                           ctl = (Control)Activator.CreateInstance(cType,
         //                                                                    cIndex,
         //                                                                    ((groupType == "Trigger") ? Function.tenTYPE.TYPE_TRIGGER : Function.tenTYPE.TYPE_CONSTANT));
         //                        else
         //                           ctl = (Control)Activator.CreateInstance(cType,
         //                                                                    this as IHostForm,
         //                                                                    cIndex,
         //                                                                    ((groupType == "Trigger") ? Function.tenTYPE.TYPE_TRIGGER : Function.tenTYPE.TYPE_CONSTANT));

         //                        /* Call FUNCTION instances XML deserialiser */
         //                        (ctl as IXmlSerializable).ReadXml(reader);

         //                        /* Add Function to GROUP */
         //                        foreach (Control c in tabPage.Controls)
         //                        {
         //                           if (c is GroupContainer)
         //                           {
         //                              (c as GroupContainer).AddFunctionToTriggerGroup(groupIdx, ctl);
         //                           }
         //                        }
         //                     }
         //                  }
         //                  break;
         //            }
         //         }
         //      }

         //      pbControl.Progress = noOfElements++;
         //      pbControl.Remove();
         //   }
         //   else
         //   {
         //      MessageBox.Show("File version mismatch (" + data + ") found.", "Loading error...");
         //   }
         //}
      }
      #endregion

      //#region "XML Saving"
      //private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
      //{
      //   this.Activate();

      //   /* Get the file name */
      //   string fileName = (sender as FileDialog).FileName;

      //   XmlWriterSettings xmlSettings = new XmlWriterSettings();

      //   xmlSettings.Indent = true;

      //   using (XmlWriter xmlWriter = XmlWriter.Create(fileName, xmlSettings))
      //   {
      //      this.WriteXml(xmlWriter);
      //   }
      //}

      ///// <summary>
      ///// Saves the Sequence to an XML file.
      ///// </summary>
      ///// <param name="sender"></param>
      ///// <param name="e"></param>
      //private void button_SaveCfg_Click(object sender, EventArgs e)
      //{
      //   if (this.loadedFileName != null)
      //   {
      //      this.saveFileDialog.FileName = Path.GetFileNameWithoutExtension(this.loadedFileName);
      //   }
      //   this.saveFileDialog.ShowDialog();
      //}

      ///// <summary>
      ///// 
      ///// </summary>
      ///// <param name="writer"></param>
      //public void WriteXml(System.Xml.XmlWriter writer)
      //{
      //   writer.WriteStartDocument();

      //   /* Store Version */
      //   writer.WriteStartElement(this.GetType().ToString());
      //   writer.WriteAttributeString("Version", "0.4");

      //   /* Iterates through all the tabs in the tab control. */
      //   foreach (Control tab in tabControl_Groups.Controls)
      //   {
      //      TabPage tabPage = (TabPage)tab;

      //      /* Store the TAB index */
      //      writer.WriteStartElement("TabPage", tabPage.Text);

      //      /* Iterates through all GroupContainers in the tab page. */
      //      foreach (Control c in tabPage.Controls)
      //      {
      //         if (c is GroupContainer)
      //            (c as GroupContainer).WriteXml(writer);
      //      }

      //      writer.WriteEndElement();
      //   }

      //   writer.WriteEndElement();
      //   writer.WriteEndDocument();
      //}
      //#endregion

      //public XmlSchema GetSchema()
      //{
      //   throw new NotImplementedException();
      //}
   }
}
