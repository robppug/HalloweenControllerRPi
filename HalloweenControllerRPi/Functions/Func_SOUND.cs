using HalloweenControllerRPi.Device;
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
      private bool _Looping;

      public MediaElement activePlaybackDevice;
      public EventHandler evOnTimerTick;

      private DispatcherTimer tickTimer;

      #region Parameters
      public int SoundDuration_s
      {
         get { return _SoundDuration_s; }
         set { _SoundDuration_s = value; }
      }

      public bool Loop
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

         FunctionKeyCommand = new Command("SOUND", 'S');

         evOnDelayEnd += new EventHandler(OnTrigger);
         evOnDurationEnd += new EventHandler(OnDurationEnd);

         tickTimer = new DispatcherTimer();
         tickTimer.Tick += tickTimer_Elapsed;
         this.vSetTimerInterval(tickTimer, 1000);
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
         if (evOnTimerTick != null)
         {
            //evOnTimerTick.Invoke(sender, e);
         }
      }

      public void OpenFile(string fileName)
      {
         this.InitSoundDevice(fileName);
      }

      public void CloseFile()
      {
         //if (soundStream != null)
         {
            //soundStream.Dispose();
            //soundStream = null;
         }
      }

      public void InitSoundDevice(string fileName)
      {
         if (activePlaybackDevice != null)
         {
            //StorageFile file = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.GetFileAsync("background.wav");

            /* Create a SOUND stream */
            //var soundStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            //activePlaybackDevice.SetSource(soundStream, file.ContentType);
         }
         else
         {
            /* ERROR - No channel selected */
         }
      }

      public void Play()
      {
         if (activePlaybackDevice != null && /*soundStream != null &&*/ activePlaybackDevice.CurrentState != MediaElementState.Playing)
         {
            activePlaybackDevice.Play();
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

            if (evOnTimerTick != null)
            {
               evOnTimerTick.Invoke(this, EventArgs.Empty);
            }
         }
         if (activePlaybackDevice != null)
         {
            activePlaybackDevice.Position = new TimeSpan(0);
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
