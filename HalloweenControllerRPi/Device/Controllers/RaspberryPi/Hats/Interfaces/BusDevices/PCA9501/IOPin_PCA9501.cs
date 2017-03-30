using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function
{
   class IOPin_PCA9501 : IIOPin
   {
      private GpioPinValue m_value;
      private GpioPinDriveMode m_driveMode;
      private uint m_pin;

      public event TypedEventHandler<IIOPin, GpioPinValueChangedEventArgs> ValueChanged;

      public TimeSpan DebounceTimeout
      {
         get; set;
      }

      public uint PinNumber
      {
         get
         {
            return m_pin;
         }
      }

      public IOPin_PCA9501(uint pin)
      {
         m_pin = pin;
      }

      public bool IsDriveModeSupported(GpioPinDriveMode driveMode)
      {
         bool boSupported = false;

         switch(driveMode)
         {
            case GpioPinDriveMode.Input:
            case GpioPinDriveMode.Output:
               boSupported = true;
               break;
            case GpioPinDriveMode.InputPullUp:
            case GpioPinDriveMode.InputPullDown:
            case GpioPinDriveMode.OutputOpenDrain:
            case GpioPinDriveMode.OutputOpenDrainPullUp:
            case GpioPinDriveMode.OutputOpenSource:
            case GpioPinDriveMode.OutputOpenSourcePullDown:
            default:
               break;
         }
         return boSupported;
      }

      public GpioPinValue Read()
      {
         return m_value;
      }

      public void SetDriveMode(GpioPinDriveMode value)
      {
         if (IsDriveModeSupported(value) == true)
         {
            m_driveMode = value;
         }
         else
         {
            throw new Exception("Drive mode (" + value.ToString() + ") is not supported.");
         }
      }

      public GpioPinDriveMode GetDriveMode()
      {
         return m_driveMode;
      }


      public void Write(GpioPinValue value)
      {
         m_value = value;
      }
   }
}
