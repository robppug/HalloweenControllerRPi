using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevices;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevices
{
   internal interface IDeviceCommsProvider<T> where T : IDeviceComms
   {
      T BusDeviceComms { get; }

      void Open(T stream);
      void Close();
      void InitialiseDriver(bool proceedOnFail = false);
   }
}