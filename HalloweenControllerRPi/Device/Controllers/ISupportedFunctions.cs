namespace HalloweenControllerRPi.Device.Controllers
{
    public interface ISupportedFunctions
    {
        uint Inputs { get; }
        uint PWMs { get; }
        uint Relays { get; }
        uint SoundChannels { get; }
    }
}