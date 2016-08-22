using HalloweenControllerRPi.Container;
using HalloweenControllerRPi.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HalloweenControllerRPi
{
   public partial class MainPage : IXmlSerializable
   {
      private StorageFile fileToLoad;
      private StorageFile fileToSave;

      #region "XML Loading"
      private async void buttonLoadSequence_Click(object sender, RoutedEventArgs e)
      {
         try
         {
            StorageFile fileToLoad = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.GetFileAsync("HWControllerSequence.sqn");
         }
         catch { fileToLoad = null; }

         if (fileToLoad != null)
         {
            StreamReader loadfile = new StreamReader(await fileToLoad.OpenStreamForReadAsync());
            XmlReader xmlReader = XmlReader.Create(loadfile);

            XDocument xDoc = XDocument.Load(XmlReader.Create(new StreamReader(await fileToLoad.OpenStreamForReadAsync())));

            foreach(GroupContainer gc in lGroupContainers)
            {
               gc.ClearAllFunctions();
            }

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
         string data = "";
         uint groupIdx = 0;
         int noOfElements = 1;
         
         if (reader.ReadToFollowing("HalloweenControllerRPi.MainPage") == true)
         {
            data = reader.GetAttribute("Version");
            //pbControl.Progress = noOfElements++;

            if (data == "0.5")
            {
               string groupType = null;

               /* Reload available FUNCTION_BUTTONS */
               //Available_Board.Items.Clear();

               /* Detect connected BOARD */
               //GetBoardType();

               //Thread.Sleep(1000);

               //Application.DoEvents();

               //Thread.Sleep(1000);

               /* Read and create FUNCTIONS in the order stored in the XML */
               while (reader.Read())
               {
                  if (reader.NodeType == XmlNodeType.Element)
                  {
                     //pbControl.Progress = noOfElements++;

                     /* Allow the GUI to update */
                     //Application.DoEvents();

                     switch (reader.Name)
                     {
                        /* Group Container found */
                        case "GroupContainer":
                           if (reader.NamespaceURI == "groupContainer_AlwaysActive")
                           {
                              groupType = "AlwaysActive";
                              groupContainer_AlwaysActive.ClearAllFunctions();
                           }
                           else if (reader.NamespaceURI == "groupContainer_Triggered")
                           {
                              groupType = "Trigger";
                              groupContainer_Triggered.ClearAllFunctions();
                           }
                           break;

                        /* GROUP Containter/Trigger found */
                        case "Group":
                           /* Get Index */
                           groupIdx = Convert.ToUInt32(reader.GetAttribute("Index"));

                           if (groupType == "Trigger")
                           {
                              groupContainer_Triggered.AddTriggerGroup(groupIdx);
                           }
                           break;

                        /* FUNCTION GUI found */
                        case "Function":
                           if (reader.MoveToNextAttribute())
                           {
                              Control ctl;

                              /* Check NODE - Function Type */
                              if (reader.Name == "Type")
                              {
                                 /* Create and INSTANCE of the function */
                                 Type cType = Type.GetType(reader.GetAttribute("Type"));
                                 UInt16 cIndex = Convert.ToUInt16(reader.GetAttribute("Index"));

                                 ctl = (Control)Activator.CreateInstance(cType,
                                                                         MainPage.HostApp,
                                                                         cIndex,
                                                                         ((groupType == "Trigger") ? Function.tenTYPE.TYPE_TRIGGER : Function.tenTYPE.TYPE_CONSTANT));

                                 /* Call FUNCTION instances XML deserialiser */
                                 (ctl as IXmlSerializable).ReadXml(reader);

                                 /* Add Function to GROUP */
                                 if(groupType == "Trigger")
                                    groupContainer_Triggered.AddFunctionToTriggerGroup(groupIdx, ctl);
                                 else
                                    groupContainer_AlwaysActive.AddFunctionToTriggerGroup(groupIdx, ctl);

                              }
                           }
                           break;
                     }
                  }
               }

         //      pbControl.Progress = noOfElements++;
         //      pbControl.Remove();
            }
            else
            {
         //      MessageBox.Show("File version mismatch (" + data + ") found.", "Loading error...");
            }
         }
      }
      #endregion

      #region "XML Saving"
      private async void buttonSaveSequence_Click(object sender, RoutedEventArgs e)
      {
         StorageFile fileToSave = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFileAsync("HWControllerSequence.sqn", CreationCollisionOption.ReplaceExisting);
         
         if (fileToSave != null)
         {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            Stream savefile = await fileToSave.OpenStreamForWriteAsync();
            
            xmlSettings.Indent = true;

            using (XmlWriter xmlWriter = XmlWriter.Create(savefile, xmlSettings))
            {
               WriteXml(xmlWriter);
            }

            savefile.Dispose();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="writer"></param>
      public void WriteXml(System.Xml.XmlWriter writer)
      {
         writer.WriteStartDocument();

         /* Store Version */
         writer.WriteStartElement(this.GetType().ToString());
         writer.WriteAttributeString("Version", "0.5");

         /* Iterates through all the GroupConters in the PIVOT control. */
         foreach (GroupContainer gc in lGroupContainers)
         {
            /* Store the GroupContainer NAME */
            writer.WriteStartElement("GroupContainer", gc.Name);

            gc.WriteXml(writer);

            writer.WriteEndElement();
         }

         writer.WriteEndElement();
         writer.WriteEndDocument();
      }
      #endregion

      public XmlSchema GetSchema()
      {
         throw new NotImplementedException();
      }
   }
}
