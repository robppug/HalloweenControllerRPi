using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using System;
using System.Collections.Generic;
using static HalloweenControllerRPi.Functions.Func_PWM;

namespace HalloweenControllerRPi.Device.Controllers.Channels
{
   internal class ChannelFunction_PWM : IChannel, IProcessTick
   {
      private IInterpolation curve = Interpolate.Common(new double[] { 0, 455, 910, 1365, 1820, 2275, 2730, 3185, 3640, 4095 },  /* 4095 / 9 points  */
                                                        new double[] { 0, 40, 140, 320, 620, 1000, 1450, 2200, 3100, 4095 }); /* y = x * x / 4095 */

      private tenFUNCTION _enFunction;
      private uint _channelIdx;
      private uint _minLevel;
      private uint _maxLevel;
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
         Level = 0;
         UpdateCount = 0;
         Function = tenFUNCTION.FUNC_OFF;
         toggle = false;
         updateTick = 0;
         _customLevelIdx = 0;

         Index = chan;
         ChannelHost = host;
         CustomLevel = new List<uint>();
      }

      public uint Index
      {
         set { _channelIdx = value; }
         get { return _channelIdx; }
      }

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

      public tenFUNCTION Function
      {
         get { return _enFunction; }
         set { _enFunction = value; Tick(); }
      }

      public uint UpdateCount
      {
         get { return _updateCnt; }
         set { _updateCnt = value; Tick(); }
      }

      public List<uint> CustomLevel
      {
         get; set;
      }

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

         if (Function == tenFUNCTION.FUNC_OFF)
         {
            _functionLevel = 0;
         }
         else if (Function == tenFUNCTION.FUNC_ON)
         {
            _functionLevel = MaxLevel;
         }
         else if (boUpdateTick() == true)
         {
            switch (Function)
            {
               case tenFUNCTION.FUNC_SWEEP_UP:
                  value = (_functionLevel + 16);
                  if (value > MaxLevel)
                  {
                     value = MinLevel;
                  }
                  _functionLevel = value;
                  break;

               case tenFUNCTION.FUNC_SWEEP_DOWN:
                  if (_functionLevel < (MinLevel + 16))
                  {
                     value = MinLevel;
                  }
                  else
                  {
                     value = (_functionLevel - 16);
                  }

                  _functionLevel = (value <= MinLevel ? MaxLevel : value);
                  break;

               case tenFUNCTION.FUNC_SIGNWAVE:
                  if (toggle && (_functionLevel < (MinLevel + 16)))
                  {
                     _functionLevel = MinLevel;
                  }
                  else
                  {
                     _functionLevel = (_functionLevel + (uint)(toggle ? -16 : 16));
                  }

                  if ((toggle == true && _functionLevel <= MinLevel)
                      || (toggle == false && _functionLevel >= MaxLevel))
                  {
                     toggle = (toggle ? false : true);
                  }
                  break;

               case tenFUNCTION.FUNC_FLICKER_OFF:
                  count = (uint)(new Random().Next((int)PWMResolution));
                  if (count > (MaxLevel / 9))
                  {
                     _functionLevel = count;
                  }
                  else
                  {
                     _functionLevel = MinLevel;
                  }
                  break;

               case tenFUNCTION.FUNC_FLICKER_ON:
                  count = (uint)(new Random().Next((int)PWMResolution));
                  if (count < (MaxLevel - (MaxLevel / 9)))
                  {
                     _functionLevel = MinLevel;
                  }
                  else
                  {
                     _functionLevel = count;
                  }
                  break;

               case tenFUNCTION.FUNC_RANDOM:
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

               case tenFUNCTION.FUNC_STROBE:
                  if (_functionLevel != MinLevel)
                  {
                     _functionLevel = MinLevel;
                  }
                  else
                  {
                     _functionLevel = MaxLevel;
                  }
                  break;


               case tenFUNCTION.FUNC_RAMP_ON:
                  break;

               case tenFUNCTION.FUNC_RAMP_OFF:
                  break;

               case tenFUNCTION.FUNC_RAMP_BOTH:
                  break;

               case tenFUNCTION.FUNC_CUSTOM:
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


         if (Function != tenFUNCTION.FUNC_CUSTOM)
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

      public object GetValue()
      {
         return Level;
      }
   }
}