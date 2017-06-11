using System;

namespace HalloweenControllerRPi.Device.Controllers.Providers
{
   public interface ISoundProvider
   {
      event EventHandler<SoundProviderEventArgs> StateChanged;

      void Play(byte track, bool loop);
      void Stop();
      void Volume(byte vol);
      void Next();
      void Previous();
   }

   public class SoundProviderEventArgs : EventArgs
   {
      public enum State
      {
         SoundFinished,
         SoundStopped
      }

      public State NewState {get; set;}

      public SoundProviderEventArgs(State newState)
      {
         NewState = newState;
      }
   }
}