using HalloweenControllerRPi.Device.Controllers.Channels;
using System;
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Function
{
   public class InputPinValueChangedEventArgs
   {
      public GpioPinEdge Edge { get; private set; }

      public InputPinValueChangedEventArgs(GpioPinEdge edge)
      {
         Edge = edge;
      }
   }

   class IOPin_PCA9501 : IIOPin
   {
      private GpioPinValue m_lastValue;
      private GpioPinValue m_value;
      private GpioPinDriveMode m_driveMode;
      private uint m_pin;

      public event TypedEventHandler<IIOPin, InputPinValueChangedEventArgs> ValueChanged;

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
         m_value = GpioPinValue.High;
         m_lastValue =  GpioPinValue.High;
      }

      protected void OnValueChanged(InputPinValueChangedEventArgs e)
      {
         ValueChanged?.Invoke(this, e);
      }

      public bool IsDriveModeSupported(GpioPinDriveMode driveMode)
      {
         bool boSupported = false;

         switch(driveMode)
         {
            case GpioPinDriveMode.InputPullUp:
            case GpioPinDriveMode.Output:
               boSupported = true;
               break;
            case GpioPinDriveMode.Input:
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
         /* If configured as an INPUT, we have some additional processing */
         if (m_driveMode == GpioPinDriveMode.InputPullUp)
         {
            if (m_lastValue != m_value)
            {
               GpioPinEdge edge;

               switch (m_value)
               {
                  case GpioPinValue.High:
                     edge = GpioPinEdge.RisingEdge;
                     break;
                  case GpioPinValue.Low:
                  default:
                     edge = GpioPinEdge.FallingEdge;
                     break;
               }

               m_lastValue = m_value;

               OnValueChanged(new InputPinValueChangedEventArgs(edge));
            }
         }

         m_value = value;
      }
   }
}
