﻿using HalloweenControllerRPi.Device;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace HalloweenControllerRPi.Functions
{
   public class Func_SOUND : Function
   {
      private string _fileSound;
      private int _SoundDuration_s;
      private uint _Repeats;
      private uint RepeatCount;
      private bool? _Looping;
      public List<StorageFile> lSoundFiles;
      public MediaElement activePlaybackDevice;
      public IRandomAccessStream sSoundStream;
      public EventHandler TimerTick;

      private DispatcherTimer tickTimer;

      #region Parameters
      public int SoundDuration_ms
      {
         get { return _SoundDuration_s; }
         set { _SoundDuration_s = value; }
      }

      public bool? Loop
      {
         get { return _Looping; }
         set { _Looping = value; }
      }

      public uint Repeats
      {
         get { return _Repeats; }
         set { _Repeats = value; }
      }

      public string fileSOUND
      {
         get { return _fileSound; }
         set { _fileSound = value; }
      }
      #endregion

      public Func_SOUND() { }

      public Func_SOUND(IHostApp host, tenTYPE entype) : base(host, entype)
      {
         activePlaybackDevice = new MediaElement();
         activePlaybackDevice.RealTimePlayback = true;

         lSoundFiles = new List<StorageFile>();

         FunctionKeyCommand = new Command("SOUND", 'S');

         evOnDelayEnd += new EventHandler(OnTrigger);
         evOnDurationEnd += new EventHandler(OnDurationEnd);
         
         tickTimer = new DispatcherTimer();
         tickTimer.Tick += tickTimer_Elapsed;
         vSetTimerInterval(tickTimer, 1000);
      }

      void vSetTimerInterval(DispatcherTimer t, uint value)
      {
         if (value > 0)
         {
            t.Interval = TimeSpan.FromMilliseconds(value);
         }
      }

      void tickTimer_Elapsed(object sender, object e)
      {
         if (TimerTick != null)
         {
            TimerTick.Invoke(sender, EventArgs.Empty);
         }

         tickTimer.Start();
      }

      public async Task<int> GetAvailableSounds()
      {
         var files = await ApplicationData.Current.LocalCacheFolder.GetFilesAsync();

         lSoundFiles.Clear();
         
         foreach (StorageFile file in files)
         {
            if (file.DisplayType.Contains("WAV File") || file.DisplayType.Contains("MP3 File"))
            {
               lSoundFiles.Add(file);
            }
         }

         return lSoundFiles.Count;
      }

      public async void OpenFile(int index)
      {
         if (activePlaybackDevice != null)
         {
            CloseFile();

            sSoundStream = await lSoundFiles[index].OpenAsync(Windows.Storage.FileAccessMode.Read);

            activePlaybackDevice.AutoPlay = false;
            activePlaybackDevice.SetSource(sSoundStream, lSoundFiles[index].ContentType);
         }
      }

      public void CloseFile()
      {
         if (sSoundStream != null)
         {
            sSoundStream.Dispose();
            sSoundStream = null;
         }
      }

      public void Play()
      {
         if (activePlaybackDevice != null && sSoundStream != null && activePlaybackDevice.CurrentState != MediaElementState.Playing)
         {
            activePlaybackDevice.Play();

            SoundDuration_ms = (int)activePlaybackDevice.NaturalDuration.TimeSpan.TotalMilliseconds;

            if (TimerTick != null)
            {
               TimerTick.Invoke(this, EventArgs.Empty);
            }
            
            tickTimer.Start();
         }
      }

      public void Pause()
      {
         if (activePlaybackDevice != null)
         {
            activePlaybackDevice.Pause();
         }
      }

      public void Stop()
      {
         if (activePlaybackDevice != null)
         {
            activePlaybackDevice.Stop();
            tickTimer.Stop();

            if (TimerTick != null)
            {
               TimerTick.Invoke(this, EventArgs.Empty);
            }
         }
      }

      /// <summary>
      /// This does the actual processing of the onTrigger EVENT.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnTrigger(object sender, EventArgs e)
      {
         if (activePlaybackDevice.CurrentState != MediaElementState.Playing)
         {
            Play();

            RepeatCount = _Repeats;
         }
         else if ((_Looping == true) && (RepeatCount > 0))
         {
            RepeatCount--;
            Stop();
            Play();
         }
      }

      private void OnDurationEnd(object sender, EventArgs e)
      {
         if ((_Looping == true) && (RepeatCount > 0))
         {
            base.evOnTrigger.Invoke(sender, EventArgs.Empty);
         }
         else
         {
            Stop();
         }
      }

      public override void WriteXml(System.Xml.XmlWriter writer)
      {
         base.WriteXml(writer);

         writer.WriteAttributeString("ActivePBDevice", (activePlaybackDevice != null ? activePlaybackDevice.ToString() : ""));
         writer.WriteAttributeString("Duration", Duration_ms.ToString());
         writer.WriteAttributeString("Delay", Duration_ms.ToString());
         writer.WriteAttributeString("File", (fileSOUND != null ? fileSOUND.ToString() : ""));
         writer.WriteAttributeString("Loop", Loop.ToString());
         writer.WriteAttributeString("Repeats", Repeats.ToString());
         writer.WriteAttributeString("Volume", (activePlaybackDevice != null ? activePlaybackDevice.Volume.ToString() : "1"));
      }

      public override List<char> SerializeSequence()
      {
         return null;
      }
   }
}