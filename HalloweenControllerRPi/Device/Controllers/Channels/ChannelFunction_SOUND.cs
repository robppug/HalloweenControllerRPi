using HalloweenControllerRPi.Device.Controllers.Providers;
using System;
using static HalloweenControllerRPi.Device.Controllers.Channels.SoundChannelEventArgs;

namespace HalloweenControllerRPi.Device.Controllers.Channels
{

   public class SoundChannelEventArgs
   {
      public enum SoundState
      {
         Play,
         Stop, 
         Volume
      };

      public SoundState NewState { get; set; }

      public SoundChannelEventArgs(SoundState state)
      {
         NewState = state;
      }
   }

   public class ChannelFunction_SOUND : IChannel
   {
      private IChannelHost _channelHost;
      private uint _channelIdx;
      private byte _volume;

      public event EventHandler<SoundChannelEventArgs> ChannelUpdated;

      public IChannelHost ChannelHost
      {
         get { return _channelHost; }
         private set { _channelHost = value; }
      }

      public byte Track { get; set; }
      public bool Loop { get; set; }
      public byte Volume
      {
         get { return _volume; }
         set
         {
            _volume = value;
            ChannelUpdated?.Invoke(this, new SoundChannelEventArgs(SoundState.Volume));
         }
      }

      public uint Index
      {
         set { _channelIdx = value; }
         get { return _channelIdx; }
      }

      public ushort AvailableTracks
      {
         get;
         set;
      }

      public ChannelFunction_SOUND(IChannelHost host, uint chan)
      {
         Index = chan;
         ChannelHost = host;
         Loop = false;
      }

      public void Tick()
      {
         
      }

      public void Play()
      {
         ChannelUpdated?.Invoke(this, new SoundChannelEventArgs(SoundState.Play));
      }

      public void Stop()
      {
         ChannelUpdated?.Invoke(this, new SoundChannelEventArgs(SoundState.Stop));
      }

      public object GetValue()
      {
         return Volume;
      }
   }
}
