using HalloweenControllerRPi.Attributes;
using HalloweenControllerRPi.Device;
using HalloweenControllerRPi.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Xml;
using static HalloweenControllerRPi.Attributes.FunctionAttribute;

namespace HalloweenControllerRPi.Functions
{
   public enum PWMFunctions
   {
      [FunctionAttribute(FunctionType.CONSTANT | FunctionType.TRIGGERED)]
      FUNC_OFF,
      [FunctionAttribute(FunctionType.CONSTANT | FunctionType.TRIGGERED)]
      FUNC_ON,
      [FunctionAttribute(FunctionType.TRIGGERED)]
      FUNC_RAMP_ON,
      [FunctionAttribute(FunctionType.TRIGGERED)]
      FUNC_RAMP_OFF,
      [FunctionAttribute(FunctionType.TRIGGERED)]
      FUNC_RAMP_BOTH,
      [FunctionAttribute(FunctionType.CONSTANT | FunctionType.TRIGGERED)]
      FUNC_SWEEP_UP,
      [FunctionAttribute(FunctionType.CONSTANT | FunctionType.TRIGGERED)]
      FUNC_SWEEP_DOWN,
      [FunctionAttribute(FunctionType.CONSTANT | FunctionType.TRIGGERED)]
      FUNC_SIGNWAVE,
      [FunctionAttribute(FunctionType.CONSTANT | FunctionType.TRIGGERED)]
      FUNC_FLICKER_OFF,
      [FunctionAttribute(FunctionType.CONSTANT | FunctionType.TRIGGERED)]
      FUNC_FLICKER_ON,
      [FunctionAttribute(FunctionType.CONSTANT | FunctionType.TRIGGERED)]
      FUNC_RANDOM,
      [FunctionAttribute(FunctionType.CONSTANT | FunctionType.TRIGGERED)]
      FUNC_STROBE,
      [FunctionAttribute(FunctionType.CONSTANT | FunctionType.TRIGGERED)]
      FUNC_CUSTOM,
      FUNC_NO_OF_FUNCTIONS
   };

   internal static class PWMFunctionsEnumExtensions
   {
      public static bool IsConstant(this PWMFunctions func)
      {
         FunctionAttribute mde = EnumExtension<FunctionAttribute, PWMFunctions>.GetModeAttribute(func);
         if (mde != null)
            return mde.IsConstant;
         else
            return true;
      }

      public static bool IsTriggered(this PWMFunctions func)
      {
         FunctionAttribute mde = EnumExtension<FunctionAttribute, PWMFunctions>.GetModeAttribute(func);
         if (mde != null)
            return mde.IsTriggered;
         else
            return true;
      }
   }

   public class Func_PWM : Function
   {
      public List<uint> CustomLevels { get; set; }

      public uint MinLevel { get; set; } = 0;

      public uint MaxLevel { get; set; } = 100;

      public uint UpdateRate { get; set; } = 1;

      public uint RampRate { get; set; } = 1;

      public PWMFunctions Function { get; set; } = PWMFunctions.FUNC_OFF;

      public Func_PWM()
      {
      }

      public Func_PWM(IHostApp host, tenTYPE entype)
         : base(host, entype)
      {
         CustomLevels = new List<uint>();

         FunctionKeyCommand = new Command("PWM", 'T');

         evOnDelayEnd += OnTrigger;
         evOnDurationEnd += OnDurationEnd;
      }

      /// <summary>
      /// EVENT to fire when the set DURATION expires.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnDurationEnd(object sender, EventArgs e)
      {
         if (Type == tenTYPE.TYPE_TRIGGER)
         {
            if (Function == PWMFunctions.FUNC_OFF)
            {
               SendCommand("SET");
            }
            else
            {
               SendCommand("FUNCTION");
            }
         }
         else
         {
            SendCommand("FUNCTION");
         }
      }

      /// <summary>
      /// EVENT to fire when TRIGGERED.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnTrigger(object sender, EventArgs e)
      {
         List<string> data = new List<string>();

         SendCommand("MINLEVEL", MinLevel);
         SendCommand("MAXLEVEL", MaxLevel);
         SendCommand("RATE", UpdateRate);
         SendCommand("RAMPRATE", RampRate);

         switch (Function)
         {
            case PWMFunctions.FUNC_CUSTOM:
               SendCommand("DATA", CustomLevels.ToArray());
               break;

            default:
               break;
         }

         SendCommand("FUNCTION", (uint)Function);
      }

      public override bool boProcessRequest(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue)
      {
         if (cFunc == (char)0)
            return base.boProcessRequest(cFunc, subFunc, cFuncIndex, u32FuncValue);

         return false;
      }

      public override void ReadXml(XmlReader reader)
      {
         base.ReadXml(reader);

         Duration_ms = Convert.ToUInt16(reader.GetAttribute("Duration"));
         Delay_ms = Convert.ToUInt16(reader.GetAttribute("Delay"));
         RampRate = Convert.ToUInt16(reader.GetAttribute("RampRate"));
         UpdateRate = Convert.ToUInt16(reader.GetAttribute("UpdateRate"));
      }

      public override void WriteXml(System.Xml.XmlWriter writer)
      {
         base.WriteXml(writer);

         writer.WriteAttributeString("Duration", Duration_ms.ToString());
         writer.WriteAttributeString("Delay", Delay_ms.ToString());
         writer.WriteAttributeString("MinLevel", MinLevel.ToString());
         writer.WriteAttributeString("MaxLevel", MaxLevel.ToString());
         writer.WriteAttributeString("RampRate", RampRate.ToString());
         writer.WriteAttributeString("UpdateRate", UpdateRate.ToString());
         writer.WriteAttributeString("Function", Function.ToString());
      }
   }
}