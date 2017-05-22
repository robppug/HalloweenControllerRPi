namespace HalloweenControllerRPi.Device.Drivers
{
   internal interface IDriverProvider
   {
      void InitialiseDriver(bool proceedOnFail = false);
   }
}