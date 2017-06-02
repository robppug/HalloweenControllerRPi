using HalloweenControllerRPi.Device.Controllers.BusDevices;

namespace HalloweenControllerRPi.Device.Controllers.Providers
{
   internal interface IDeviceCommsProvider<T> where T : IDeviceComms
   {
      T BusDeviceComms { get; }

      void Open(T stream);
      void Close();
      void InitialiseDriver(bool proceedOnFail = false);
   }
}