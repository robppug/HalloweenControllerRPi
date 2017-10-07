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

      public bool Randomise { get; set; } = false;
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
         uint track = Track;

         if (Randomise == true)
         {
            track = (uint)(new Random().Next(1, (int)AvailableTracks));
         }
         SendCommand("TRACK", track);
         SendCommand("LOOP", (Loop ? 1 : 0));
         SendCommand("PLAY");
      }

      private void OnDurationEnd(object sender, EventArgs e)
      {

         if (Type == tenTYPE.TYPE_TRIGGER)
         {
            //If duration is 0, just play the sound to end.
            if (Duration_ms > 0)
            {
               SendCommand("STOP");
            }
         }
         else
         {
            SendCommand("STOP");
         }
      }

      public override void ReadXml(XmlReader reader)
      {
         base.ReadXml(reader);

         Duration_ms = Convert.ToUInt16(reader.GetAttribute("Duration"));
         Delay_ms = Convert.ToUInt16(reader.GetAttribute("Delay"));
         Volume = Convert.ToUInt32(reader.GetAttribute("Volume"));
         Track = Convert.ToUInt32(reader.GetAttribute("Track"));
         Loop = Convert.ToBoolean(reader.GetAttribute("Loop"));
         Randomise = Convert.ToBoolean(reader.GetAttribute("Randomise"));
      }

      public override void WriteXml(System.Xml.XmlWriter writer)
      {
         base.WriteXml(writer);

         writer.WriteAttributeString("Duration", Duration_ms.ToString());
         writer.WriteAttributeString("Delay", Delay_ms.ToString());
         writer.WriteAttributeString("Volume", Volume.ToString());
         writer.WriteAttributeString("Track", Track.ToString());
         writer.WriteAttributeString("Loop", Loop.ToString());
         writer.WriteAttributeString("Randomise", Randomise.ToString());
      }

      public override bool boProcessRequest(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue)
      {
         if(cFunc == (char)0)
            return base.boProcessRequest(cFunc, subFunc, cFuncIndex, u32FuncValue);
         else 
         {
            if (cFuncIndex == Index)
            {
               switch (subFunc)
               {
                  case 'I':
                     return base.boProcessRequest(cFunc, subFunc, cFuncIndex, u32FuncValue);

                  case 'A':
                     AvailableTracks = u32FuncValue;
                     evOnFunctionUpdated?.Invoke(this, EventArgs.Empty);
                     return true;

                  case 'F':
                     if (Loop == true)
                     {
                        uint track = Track;
                        if (Randomise == true)
                        {
                           track = (uint)(new Random().Next((int)AvailableTracks));
                        }
                        SendCommand("TRACK", track);
                        SendCommand("PLAY");
                        return true;
                     }
                     break;

                  default:
                     break;
               }
            }
         }

         return false;
      }
   }
}
