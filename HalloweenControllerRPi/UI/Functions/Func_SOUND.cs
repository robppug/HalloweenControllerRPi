using HalloweenControllerRPi.Device;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Xml;

namespace HalloweenControllerRPi.Functions
{
   public class Func_SOUND : Function
   {
      private uint _volume;

      #region Parameters
      public uint AvailableTracks { get; set; } = 0;

      public uint Volume
      {
         get { return _volume; }
         set
         {
            _volume = value;
            SendCommand("VOLUME", Volume);
         }
      }

      public uint Track { get; set; } = 0;

      public bool Loop { get; set; } = false;
      #endregion

      public Func_SOUND() { }

      public Func_SOUND(IHostApp host, tenTYPE entype) : base(host, entype)
      {
         FunctionKeyCommand = new Command("SOUND", 'S');

         evOnDelayEnd += OnTrigger;
         evOnDurationEnd += OnDurationEnd;
      }

      internal void Initialise()
      {
         SendCommand("AVAILABLE TRACKS");
      }

      /// <summary>
      /// This does the actual processing of the onTrigger EVENT.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnTrigger(object sender, EventArgs e)
      {
         SendCommand("TRACK", Track);
         SendCommand("LOOP", Loop);
         SendCommand("PLAY");
      }

      private void OnDurationEnd(object sender, EventArgs e)
      {
         SendCommand("STOP");
      }

      public override void ReadXml(XmlReader reader)
      {
         base.ReadXml(reader);

         Duration_ms = Convert.ToUInt16(reader.GetAttribute("Duration"));
         Delay_ms = Convert.ToUInt16(reader.GetAttribute("Delay"));
         Volume = Convert.ToUInt32(reader.GetAttribute("Volume"));
         Track = Convert.ToUInt32(reader.GetAttribute("Track"));
         Loop = Convert.ToBoolean(reader.GetAttribute("Loop"));
      }

      public override void WriteXml(System.Xml.XmlWriter writer)
      {
         base.WriteXml(writer);

         writer.WriteAttributeString("Duration", Duration_ms.ToString());
         writer.WriteAttributeString("Delay", Duration_ms.ToString());
         writer.WriteAttributeString("Volume", Volume.ToString());
         writer.WriteAttributeString("Track", Track.ToString());
         writer.WriteAttributeString("Loop", Loop.ToString());
      }

      public override bool boProcessRequest(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue)
      {
         if(cFunc == (char)0)
            return base.boProcessRequest(cFunc, subFunc, cFuncIndex, u32FuncValue);
         else 
         {
            switch (subFunc)
            {
               case 'I':
                  if (cFuncIndex == Index)
                  {
                     return base.boProcessRequest(cFunc, subFunc, cFuncIndex, u32FuncValue);
                  }
                  break;

               case 'A':
                  AvailableTracks = u32FuncValue;
                  evOnFunctionUpdated?.Invoke(this, EventArgs.Empty);
                  return true;

               case 'F':
                  if (Loop == true)
                  {
                     SendCommand("PLAY");
                     return true;
                  }
                  break;

               default:
                  break;
            }
         }

         return false;
      }
   }
}
