using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using System;
using static HalloweenControllerRPi.Functions.Func_PWM;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi
{
   class HWRaspberryPI_PWM : IFunctionHandler, IProcessTick
   {
      private IInterpolation curve = Interpolate.Common(new double[] { 0, 455, 910, 1365, 1820, 2275, 2730, 3185, 3640, 4095 },  /* 4095 / 9 points  */
                                                        new double[] { 0, 51,  202, 455,  809,  1264, 1820, 2477, 3236, 4095 }); /* y = x * x / 4095 */
      private tenFUNCTION _enFunction;
      private uint _channelIdx;
      private uint _minLevel;
      private uint _maxLevel;
      private uint _updateCnt;
      private uint _func_value;
      private uint updateTick;
      private bool toggle;
      private uint _functionLevel;

      public const uint PWMResolution = 4095;

      public HWRaspberryPI_PWM(uint chan)
      {
         MinLevel = 0;
         MaxLevel = PWMResolution;
         Level = 0;
         UpdateCount = 0;
         Function = tenFUNCTION.FUNC_OFF;
         toggle = false;
         updateTick = 0;

         Channel = chan;
      }

      public uint Channel
      {
         set { _channelIdx = value;  }
         get { return _channelIdx; }
      }

      public uint Level
      {
         set { _func_value = value;  }
         get { return _func_value; }
      }

      public uint MinLevel
      {
         set { _minLevel = (PWMResolution * value) / 100; }
         get { return _minLevel; }
      }

      public uint MaxLevel
      {
         set { _maxLevel = (PWMResolution * value) / 100; }
         get { return _maxLevel; }
      }

      public tenFUNCTION Function
      {
         get { return _enFunction; }
         set { _enFunction = value; }
      }

      public uint UpdateCount
      {
         get { return _updateCnt; }
         set { _updateCnt = value; }
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

         switch (Function)
         {
            case tenFUNCTION.FUNC_OFF:
               _functionLevel = 0;
               break;
            case tenFUNCTION.FUNC_CONSTANT:
               _functionLevel = MaxLevel;
               break;
            case tenFUNCTION.FUNC_SWEEP_UP:
               if (boUpdateTick() == true)
               {
                  value = (_functionLevel + 16);
                  _functionLevel = (value >= MaxLevel ? MinLevel : value);
               }
               break;
            case tenFUNCTION.FUNC_SWEEP_DOWN:
               if (boUpdateTick() == true)
               {
                  value = (_functionLevel - 16);
                  _functionLevel = (value <= (MinLevel + 16) ? MaxLevel : value);
               }
               break;
            case tenFUNCTION.FUNC_SIGNWAVE:
               if (boUpdateTick() == true)
               {
                  _functionLevel = (_functionLevel + (uint)(toggle ? -16 : 16));

                  if ((toggle == true && _functionLevel <= (MinLevel + 16)) || (toggle == false && _functionLevel >= MaxLevel))
                     toggle = (toggle ? false : true);
               }
               break;
            case tenFUNCTION.FUNC_FLICKER_OFF:
               if (boUpdateTick() == true)
               {
                  uint count;

                  count = (uint)(new Random().Next((int)PWMResolution));
                  if (count > (MaxLevel / 9))
                     _functionLevel = count;
                  else
                     _functionLevel = MinLevel;
               }
               break;
            case tenFUNCTION.FUNC_FLICKER_ON:
               if (boUpdateTick() == true)
               {
                  uint count;

                  count = (uint)(new Random().Next((int)PWMResolution));
                  if (count < (MaxLevel - (MaxLevel / 9)))
                     _functionLevel = MinLevel;
                  else
                     _functionLevel = count;
               }
               break;
            case tenFUNCTION.FUNC_RANDOM:
               if (boUpdateTick() == true)
               {
                  value = (uint)(new Random().Next((int)PWMResolution));
                  _functionLevel = (value > MaxLevel ? MaxLevel : value);
               }
               break;
            case tenFUNCTION.FUNC_STROBE:
               if (boUpdateTick() == true)
               {
                  if (_functionLevel != MinLevel)
                     _functionLevel = MinLevel;
                  else
                     _functionLevel = MaxLevel;
               }
               break;

            default:
               break;
         }

         Level = (uint)curve.Interpolate(_functionLevel);
      }
   }
}
