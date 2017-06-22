﻿using HalloweenControllerRPi.Container;
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
using Windows.UI.Xaml.Media;

namespace HalloweenControllerRPi
{
   public partial class MainPage : IXmlSerializable
   {
      private StorageFile fileToLoad;
      private StorageFile fileToSave;
      private StorageFile fileSettings;

      public object DriveInfo { get; private set; }

      #region /* XML Loading */
      private async void loadSettingsFile()
      {
         this.checkBox_LoadOnStart.IsChecked = false;

         try
         {
            fileToLoad = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("HWControllerSequence.sqn");
         }
         catch
         {
            fileToLoad = null;
         }

         try
         {
            fileSettings = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("HWController.cfg");
         }
         catch
         {
            fileSettings = null;
         }

         if (fileSettings != null)
         {
            StreamReader settingsFile = new StreamReader(await fileSettings.OpenStreamForReadAsync());
            XmlReader xmlReader = XmlReader.Create(settingsFile);

            //XDocument xDoc = XDocument.Load(XmlReader.Create(new StreamReader(await fileSettings.OpenStreamForReadAsync())));

            if (xmlReader.ReadToFollowing("HalloweenControllerRPi.MainPage") == true)
            {
               if (xmlReader.GetAttribute("Version") == "1.0")
               {
                  if (xmlReader.ReadToFollowing("Settings") == true)
                  {
                     this.checkBox_LoadOnStart.IsChecked = (bool)(xmlReader.GetAttribute("LoadOnStart") == "True" ? true : false);

                     if (this.checkBox_LoadOnStart.IsChecked == true)
                     {
                        this.buttonLoadSequence_Click(this, null);

                        await Task.Delay(12000);

                        this.buttonStart_Click(this, null);
                     }
                  }
               }
            }

            settingsFile.Dispose();
         }
         else
         {
            fileSettings = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("HWController.cfg", CreationCollisionOption.ReplaceExisting);

            /* File doesnt exist */
            saveSettingsFile();
         }
      }

      private async void saveSettingsFile()
      {
         XmlWriterSettings xmlSettings = new XmlWriterSettings();
         Stream settingsfile = await fileSettings.OpenStreamForWriteAsync();

         xmlSettings.Indent = true;

         using (XmlWriter xmlWriter = XmlWriter.Create(settingsfile, xmlSettings))
         {
            xmlWriter.WriteStartDocument();

            /* Store Version */
            xmlWriter.WriteStartElement(this.GetType().ToString());
            xmlWriter.WriteAttributeString("Version", "1.0");
               xmlWriter.WriteStartElement("Settings");
               xmlWriter.WriteAttributeString("LoadOnStart", this.checkBox_LoadOnStart.IsChecked.ToString());
               xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
         }

         settingsfile.Dispose();
      }

      private async void buttonLoadSequence_Click(object sender, RoutedEventArgs e)
      {
         (sender as Button).IsEnabled = false;

         if (fileToLoad != null)
         {
            StreamReader loadfile = new StreamReader(await fileToLoad.OpenStreamForReadAsync());
            XmlReader xmlReader = XmlReader.Create(loadfile);

            XDocument xDoc = XDocument.Load(XmlReader.Create(new StreamReader(await fileToLoad.OpenStreamForReadAsync())));

            foreach(GroupContainer gc in lGroupContainers)
            {
               gc.ClearAllFunctions();
            }

            new ContentDialog()
            {
               Title = "Please Wait",
               IsPrimaryButtonEnabled = false,
               IsSecondaryButtonEnabled = false,
               HorizontalContentAlignment = HorizontalAlignment.Center,
               VerticalContentAlignment = VerticalAlignment.Center,
               Content = new TextBlock() { Text = "Loading XML file..." }
            }.ShowAsync();

            //pbControl.MaxValue = xDoc.Descendants().Count();
            //pbControl.Show(this);

            this.ReadXml(xmlReader);

            //DrawingControl.ResumeDrawing(groupContainer_AlwaysActive);
            //DrawingControl.ResumeDrawing(groupContainer_Trigger);

            loadfile.Dispose();
         }

         (sender as Button).IsEnabled = true;
      }

      private void checkBox_LoadOnStart_Checked(object sender, RoutedEventArgs e)
      {
         saveSettingsFile();
      }

      private void checkBox_LoadOnStart_Unchecked(object sender, RoutedEventArgs e)
      {
         saveSettingsFile();
      }

      public void ReadXml(System.Xml.XmlReader reader)
      {
         //TabPage tabPage = null;
         string data = "";
         uint groupIdx = 0;
         //int noOfElements = 1;
         
         if (reader.ReadToFollowing("HalloweenControllerRPi.MainPage") == true)
         {
            data = reader.GetAttribute("Version");
            //pbControl.Progress = noOfElements++;

            if (data == "1.0")
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

               if (GetVisibleContentDialog() != null)
                  GetVisibleContentDialog().Hide();

               //      pbControl.Progress = noOfElements++;
               //      pbControl.Remove();
            }
            else
            {
               if( GetVisibleContentDialog() != null)
                  GetVisibleContentDialog().Hide();

               new ContentDialog()
               {
                  Title = "Error",
                  IsSecondaryButtonEnabled = false,
                  PrimaryButtonText = "Exit",
                  HorizontalContentAlignment = HorizontalAlignment.Center,
                  VerticalContentAlignment = VerticalAlignment.Center,
                  Content = new TextBlock() { Text = "File version mismatch (" + data + ")." }
               }.ShowAsync();
            }
         }
      }

      private static ContentDialog GetVisibleContentDialog()
      {
         var currentDialogs = VisualTreeHelper.GetOpenPopups(Window.Current);

         foreach (var p in currentDialogs)
         {
            if (p.Child is ContentDialog)
               return (p.Child as ContentDialog);
         }

         return null;
      }
      #endregion

      #region /* XML Saving */
      private async void buttonSaveSequence_Click(object sender, RoutedEventArgs e)
      {
         (sender as Button).IsEnabled = false;

         fileToSave = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("HWControllerSequence.sqn", CreationCollisionOption.ReplaceExisting);
         
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
         (sender as Button).IsEnabled = true;
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
         writer.WriteAttributeString("Version", "1.0");

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
