using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HalloweenControllerRPi.Functions.Func_PWM;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi
{
   class HWRaspberryPI_PWM
   {
      private IInterpolation curve = Interpolate.Common(new double[] { 0, 512, 1024, 1536, 2048, 2560, 3072, 3584, 4095 },
                                                        new double[] { 0, 100, 250,  400,  1200, 2000, 3000, 4000, 4095 } );
      private tenFUNCTION _enFunction;
      private uint _channelIdx;
      private uint _maxLevel;
      private uint _updateCnt;
      private uint _func_value;
      private uint updateTick;
      private bool toggle;

      public const uint PWMResolution = 4095;

      public HWRaspberryPI_PWM(uint chan)
      {
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
         private set { _channelIdx = value;  }
         get { return _channelIdx; }
      }

      public uint Level
      {
         set { _func_value = value;  }
         get { return _func_value; }
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
               Level = 0;
               break;
            case tenFUNCTION.FUNC_CONSTANT:
               Level = MaxLevel;
               break;
            case tenFUNCTION.FUNC_SWEEP_UP:
               if (boUpdateTick() == true)
               {
                  value = (Level + 16);
                  Level = (value >= MaxLevel ? 0 : value);
               }
               break;
            case tenFUNCTION.FUNC_SWEEP_DOWN:
               if (boUpdateTick() == true)
               {
                  value = (Level - 16);
                  Level = (value <= 16 ? MaxLevel : 0);
               }
               break;
            case tenFUNCTION.FUNC_SIGNWAVE:
               if (boUpdateTick() == true)
               {
                  Level = (Level + (uint)(toggle ? -16 : 16));

                  if ((toggle == true && Level <= 16) || (toggle == false && Level >= MaxLevel))
                     toggle = (toggle ? false : true);
               }
               break;
            case tenFUNCTION.FUNC_FLICKER_OFF:
               if (boUpdateTick() == true)
               {
                  uint count;

                  count = (uint)(new Random().Next((int)PWMResolution));
                  if (count < (MaxLevel - (MaxLevel / 10)))
                     Level = 0;
                  else
                     Level = count;
               }
               break;
            case tenFUNCTION.FUNC_FLICKER_ON:
               if (boUpdateTick() == true)
               {
                  uint count;

                  count = (uint)(new Random().Next((int)PWMResolution));
                  if (count > (MaxLevel / 9))
                     Level = count;
                  else
                     Level = 0;
               }
               break;
            case tenFUNCTION.FUNC_RANDOM:
               if (boUpdateTick() == true)
               {
                  value = (uint)(new Random().Next((int)PWMResolution));
                  Level = (value > MaxLevel ? MaxLevel : value);
               }
               break;
            case tenFUNCTION.FUNC_STROBE:
               if (boUpdateTick() == true)
               {
                  if (Level != 200)
                     Level = 200;
                  else
                     Level = MaxLevel;
               }
               break;

            default:
               break;
         }

         Level = (uint)curve.Interpolate(Level);
      }
   }
}
