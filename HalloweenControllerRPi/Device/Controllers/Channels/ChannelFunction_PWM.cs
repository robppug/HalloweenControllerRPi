using HalloweenControllerRPi.Functions;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using System;
using System.Collections.Generic;

namespace HalloweenControllerRPi.Device.Controllers.Channels
{
   internal class ChannelFunction_PWM : IChannel, IProcessTick
   {
      private IInterpolation curve = Interpolate.Common(new double[] { 0, 455, 910, 1365, 1820, 2275, 2730, 3185, 3640, 4095 },  /* 4095 / 9 points  */
                                                        //new double[] { 0, 40, 140, 320, 620, 1000, 1450, 2200, 3100, 4095 }); /* y = x * x / 4095 */
                                                        new double[] { 0, 6,   45,  152,  360,  703,  1215, 1929, 2879, 4095 }); /* y = 0.0000000597 * x ^ 3 */

      private PWMFunctions _enRampingFunction;
      private PWMFunctions _enFunction;
      private uint _minLevel;
      private uint _maxLevel;
      private uint _rampRate;
      private uint _updateCnt;
      private uint _func_value;
      private uint updateTick;
      private bool toggle;
      private uint _functionLevel;
      private IChannelHost _channelHost;
      private int _customLevelIdx;

      public const uint PWMResolution = 4095;

      public ChannelFunction_PWM(IChannelHost host, uint chan)
      {
         MinLevel = 0;
         MaxLevel = PWMResolution;
         RampRate = 1;
         Level = 0;
         UpdateCount = 0;
         Function = PWMFunctions.FUNC_OFF;
         _enRampingFunction = PWMFunctions.FUNC_OFF;
         toggle = false;
         updateTick = 0;
         _customLevelIdx = 0;

         Index = chan;
         ChannelHost = host;
         CustomLevel = new List<uint>();
      }

      public uint Index { get; set; }

      public uint Level
      {
         get { return _func_value; }
         set { _func_value = value; Tick(); }
      }

      public uint MinLevel
      {
         get { return _minLevel; }
         set { _minLevel = (PWMResolution * value) / 100; Tick(); }
      }

      public uint MaxLevel
      {
         get { return _maxLevel; }
         set { _maxLevel = (PWMResolution * value) / 100; Tick(); }
      }

      public uint RampRate
      {
         get { return _rampRate; }
         set { _rampRate = value; Tick(); }
      }

      public PWMFunctions Function
      {
         get { return _enFunction; }
         set { _enFunction = value; Tick(); }
      }

      public uint UpdateCount
      {
         get { return _updateCnt; }
         set { _updateCnt = value; Tick(); }
      }

      public List<uint> CustomLevel { get; set; }

      public IChannelHost ChannelHost
      {
         get { return _channelHost; }
         private set { _channelHost = value; }
      }

      private bool boUpdateTick()
      {
         updateTick++;
         if (updateTick >= UpdateCount)
         {
            updateTick = 0;
            return true;
         }
         return false;
      }

      public void Tick()
      {
         uint value;
         uint count;

         if (Function == PWMFunctions.FUNC_OFF)
         {
            if ((_enRampingFunction == PWMFunctions.FUNC_RAMP_BOTH) || (_enRampingFunction == PWMFunctions.FUNC_RAMP_OFF))
            {
               if (boUpdateTick() == true)
               {
                  if (_functionLevel < (MinLevel + _rampRate))
                  {
                     value = MinLevel;
                     _enRampingFunction = PWMFunctions.FUNC_OFF;
                  }
                  else
                  {
                     value = (_functionLevel - _rampRate);
                  }

                  _functionLevel = value;
               }
            }
            else
            {
               _enRampingFunction = PWMFunctions.FUNC_OFF;
               _functionLevel = 0;
            }
         }
         else if (boUpdateTick() == true)
         {
            switch (Function)
            {
               case PWMFunctions.FUNC_SWEEP_UP:
                  value = (_functionLevel + _rampRate);
                  if (value > MaxLevel)
                  {
                     value = MinLevel;
                  }
                  _functionLevel = value;
                  break;

               case PWMFunctions.FUNC_SWEEP_DOWN:
                  if (_functionLevel < (MinLevel + _rampRate))
                  {
                     value = MinLevel;
                  }
                  else
                  {
                     value = (_functionLevel - _rampRate);
                  }

                  _functionLevel = (value <= MinLevel ? MaxLevel : value);
                  break;

               case PWMFunctions.FUNC_SIGNWAVE:
                  if (toggle && (_functionLevel < (MinLevel + _rampRate)))
                  {
                     _functionLevel = MinLevel;
                  }
                  else
                  {
                     _functionLevel = (_functionLevel + (uint)(toggle ? -_rampRate : _rampRate));
                  }

                  if ((toggle == true && _functionLevel <= MinLevel)
                      || (toggle == false && _functionLevel >= MaxLevel))
                  {
                     toggle = (toggle ? false : true);
                  }
                  break;

               case PWMFunctions.FUNC_FLICKER_OFF:
                  count = (uint)(new Random().Next((int)PWMResolution));
                  if (count > (MaxLevel / _rampRate))
                  {
                     _functionLevel = count;
                  }
                  else
                  {
                     _functionLevel = MinLevel;
                  }
                  break;

               case PWMFunctions.FUNC_FLICKER_ON:
                  count = (uint)(new Random().Next((int)PWMResolution));
                  if (count < (MaxLevel - (MaxLevel / _rampRate)))
                  {
                     _functionLevel = MinLevel;
                  }
                  else
                  {
                     _functionLevel = count;
                  }
                  break;

               case PWMFunctions.FUNC_RANDOM:
                  value = (uint)(new Random().Next((int)PWMResolution));
                  if (value > MaxLevel)
                  {
                     _functionLevel = MaxLevel;
                  }
                  else if (value < MinLevel)
                  {
                     _functionLevel = MinLevel;
                  }
                  else
                  {
                     _functionLevel = value;
                  }
                  break;

               case PWMFunctions.FUNC_STROBE:
                  if (_functionLevel != MinLevel)
                  {
                     _functionLevel = MinLevel;
                  }
                  else
                  {
                     _functionLevel = MaxLevel;
                  }
                  break;


               case PWMFunctions.FUNC_RAMP_ON:
               case PWMFunctions.FUNC_RAMP_BOTH:
                  _enRampingFunction = _enFunction;

                  value = (_functionLevel + _rampRate);
                  if (value > MaxLevel)
                  {
                     value = MaxLevel;
                  }
                  _functionLevel = value;
                  break;

               case PWMFunctions.FUNC_ON:
                  _functionLevel = MaxLevel;
                  break;

               case PWMFunctions.FUNC_RAMP_OFF:
                  _enRampingFunction = _enFunction;
                  _functionLevel = MaxLevel;
                  break;

               case PWMFunctions.FUNC_CUSTOM:
                  if (_customLevelIdx < CustomLevel.Count)
                  {
                     _functionLevel = CustomLevel[_customLevelIdx];

                     _customLevelIdx++;
                  }
                  else
                  {
                     _customLevelIdx = 0;
                  }
                  break;

               default:
                  _customLevelIdx = 0;
                  break;
            }
         }


         if (Function != PWMFunctions.FUNC_CUSTOM)
         {
            if (_functionLevel > MaxLevel)
            {
               _functionLevel = MaxLevel;
            }
            else if (_functionLevel < MinLevel)
            {
               _functionLevel = MinLevel;
            }
         }

         _func_value = (uint)curve.Interpolate(_functionLevel);
      }

      public uint GetValue()
      {
         return Level;
      }
   }
}