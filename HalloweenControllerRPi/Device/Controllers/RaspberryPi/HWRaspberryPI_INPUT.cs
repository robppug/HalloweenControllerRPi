﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using static HalloweenControllerRPi.Functions.Func_INPUT;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi
{
   class HWRaspberryPI_INPUT
   {
      public class EventArgsINPUT : EventArgs
      {
         private TriggerLvl _trgLvl;

         public EventArgsINPUT(TriggerLvl trgLvl)
         {
            _trgLvl = trgLvl;
         }

         public TriggerLvl TriggerLevel { get { return _trgLvl; } }
      }

      private uint _channelIdx;
      private TriggerLvl _triggerLevel;
      private TimeSpan _debTime;
      private GpioPin _Pin;
      private GpioPin _LastPin;

      public delegate void EventHandlerInput(object sender, EventArgsINPUT e);
      public event EventHandlerInput InputLevelChanged;

      public HWRaspberryPI_INPUT(uint chan, GpioPin pin)
      {
         _triggerLevel = TriggerLvl.tLow;

         Channel = chan;

         _Pin = pin;
         _LastPin = _Pin;

         _Pin.ValueChanged += Pin_ValueChanged;
      }

      private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
      {
         GpioPinEdge gpEdge = args.Edge;

         if (   ((gpEdge == GpioPinEdge.RisingEdge) && (TriggerLevel == TriggerLvl.tHigh))
             || ((gpEdge == GpioPinEdge.FallingEdge) && (TriggerLevel == TriggerLvl.tLow)))
         {
            InputLevelChanged.Invoke(this, new EventArgsINPUT(TriggerLevel));
         }
      }

      public uint Channel
      {
         private set { _channelIdx = value;  }
         get { return _channelIdx; }
      }

      public TriggerLvl TriggerLevel
      {
         get { return _triggerLevel; }
         set { _triggerLevel = value; }
      }

      public GpioPinValue CurrentPinLevel
      {
         get { return _Pin.Read(); }
         set { _Pin.Write(value); }
      }

      public void Tick()
      {
      }
   }
}
