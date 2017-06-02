using HalloweenControllerRPi.Device;
using System;
using System.Collections.Generic;
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
      public EventHandler TimerTick;

      private DispatcherTimer tickTimer;

      #region Parameters
      public uint Volume { get; set; }

      public uint Track { get; set; }
      #endregion

      public Func_SOUND() { }

      public Func_SOUND(IHostApp host, tenTYPE entype) : base(host, entype)
      {
         FunctionKeyCommand = new Command("SOUND", 'S');

         evOnDelayEnd += OnTrigger;
         evOnDurationEnd += OnDurationEnd;

         tickTimer = new DispatcherTimer();
         tickTimer.Tick += tickTimer_Elapsed;
         vSetTimerInterval(tickTimer, 1000);

         Track = 1;
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

      public void Play()
      {
         if (TimerTick != null)
         {
            TimerTick.Invoke(this, EventArgs.Empty);
         }

         tickTimer.Start();
      }

      public void Stop()
      {
         tickTimer.Stop();

         if (TimerTick != null)
         {
            TimerTick.Invoke(this, EventArgs.Empty);
         }
      }

      /// <summary>
      /// This does the actual processing of the onTrigger EVENT.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnTrigger(object sender, EventArgs e)
      {
         List<string> data = new List<string>();

         data.Add(Index.ToString("00"));
         data.Add(Volume.ToString());

         this.SendCommand("VOLUME", data.ToArray());

         data.Clear();

         data.Add(Index.ToString("00"));
         data.Add(Track.ToString());

         this.SendCommand("TRACK", data.ToArray());

         data.Clear();

         data.Add(Index.ToString("00"));
         data.Add(" 0");
         this.SendCommand("PLAY", data.ToArray());
      }

      private void OnDurationEnd(object sender, EventArgs e)
      {
         List<string> data = new List<string>();

         data.Add(Index.ToString("00"));
         data.Add(" 0");
         this.SendCommand("STOP", data.ToArray());

         if ((e as ProcessFunctionArgs) != null)
         {
            if ((e as ProcessFunctionArgs).UserStopped == true)
            {
               Stop();
            }
         }
         else
         {
            Stop();
         }
      }

      public override void WriteXml(System.Xml.XmlWriter writer)
      {
         base.WriteXml(writer);

         writer.WriteAttributeString("Duration", Duration_ms.ToString());
         writer.WriteAttributeString("Delay", Duration_ms.ToString());
         writer.WriteAttributeString("Volume", Volume.ToString());
      }
   }
}
