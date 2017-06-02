namespace HalloweenControllerRPi.Device.Controllers.Providers
{
   public interface ISoundProvider
   {
      void Play(byte? track, byte? volume = 30);
      void Stop();
   }
}