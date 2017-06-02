using HalloweenControllerRPi.Device.Controllers.Providers;
using System;
using static HalloweenControllerRPi.Device.Controllers.Channels.SoundEventArgs;

namespace HalloweenControllerRPi.Device.Controllers.Channels
{

   public class SoundEventArgs
   {
      public enum State
      {
         Play,
         Stop
      };

      public State NewState { get; set; }
      public byte? Track { get; set; }
      public byte? Volume { get; set; }

      public SoundEventArgs(State state, byte? track, byte? volume)
      {
         NewState = state;
         Track = track;
         Volume = volume;
      }
   }

   public class ChannelFunction_SOUND : IChannel
   {
      private IChannelHost _channelHost;
      private State _state;
      private uint _channelIdx;

      public event EventHandler<SoundEventArgs> StateChange;

      public IChannelHost ChannelHost
      {
         get { return _channelHost; }
         private set { _channelHost = value; }
      }

      public byte? Track { get; set; }
      public byte? Volume { get; set; }

      public uint Index
      {
         set { _channelIdx = value; }
         get { return _channelIdx; }
      }

      public ChannelFunction_SOUND(IChannelHost host, uint chan)
      {
         Index = chan;
         ChannelHost = host;
      }

      public void Tick()
      {
         
      }

      public void Play()
      {
         StateChange?.Invoke(this, new SoundEventArgs(State.Play, Track, Volume));
      }

      public void Stop()
      {
         Track = null;
         Volume = null;

         StateChange?.Invoke(this, new SoundEventArgs(State.Stop, null, null));
      }

      public object GetValue()
      {
         return Volume;
      }
   }
}
