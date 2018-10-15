using System;

namespace HalloweenControllerRPi.Device.Controllers.Providers
{
    public interface ISoundProvider
    {
        event EventHandler<SoundProviderEventArgs> StateChanged;
        event EventHandler<SoundProviderStatusEventArgs> StatusChanged;

        void Play(byte track, bool loop);
        void Stop();
        void Volume(byte vol);
        void Status();
        void Next();
        void Previous();
    }

    public class SoundProviderEventArgs : EventArgs
    {
        public enum State
        {
            SoundPlaying,
            SoundFinished,
            SoundStopped
        }

        public State NewState { get; set; }

        public SoundProviderEventArgs(State newState)
        {
            NewState = newState;
        }
    }

    public class SoundProviderStatusEventArgs : EventArgs
    {
        public ushort Status { get; set; }

        public SoundProviderStatusEventArgs(ushort status)
        {
            Status = status;
        }
    }
}