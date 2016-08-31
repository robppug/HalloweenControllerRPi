using HalloweenControllerRPi.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Functions
{
   public class Func_PWM : Function
   {
      public enum tenFUNCTION
      {
         FUNC_OFF,
         FUNC_CONSTANT,
         FUNC_SWEEP_UP,
         FUNC_SWEEP_DOWN,
         FUNC_SIGNWAVE,
         FUNC_FLICKER_OFF,
         FUNC_FLICKER_ON,
         FUNC_RANDOM,
         FUNC_STROBE,
         FUNC_NO_OF_FUNCTIONS
      };

      private uint _MaxLevel;
      private uint _UpdateRate;
      private tenFUNCTION _Function;

      public uint MaxLevel
      {
         get { return _MaxLevel; }
         set { _MaxLevel = value; }
      }
      public uint UpdateRate
      {
         get { return _UpdateRate; }
         set { _UpdateRate = value; }
      }
      public tenFUNCTION Function
      {
         get { return _Function; }
         set { _Function = value; }
      }

      public Func_PWM() { }

      public Func_PWM(IHostApp host, tenTYPE entype)
         : base(host, entype)
      {
         _Function = tenFUNCTION.FUNC_OFF;

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
         List<string> data = new List<string>();

         data.Add(Index.ToString("00"));
         if (base.Type == tenTYPE.TYPE_TRIGGER)
         {
            if (this._Function == tenFUNCTION.FUNC_OFF)
            {
               data.Add(" 0");
               this.SendCommand("SET", data.ToArray());
            }
            else
            {
               data.Add("0");
               this.SendCommand("FUNCTION", data.ToArray());
            }
         }
         else
         {
            data.Add(" 0");
            this.SendCommand("FUNCTION", data.ToArray());
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

         if (this._Function != tenFUNCTION.FUNC_OFF)
         {
            data.Add(Index.ToString("00"));
            data.Add(MaxLevel.ToString());

            this.SendCommand("MAXLEVEL", data.ToArray());

            data.Clear();

            data.Add(Index.ToString("00"));
            data.Add(UpdateRate.ToString());

            this.SendCommand("RATE", data.ToArray());

            data.Clear();

            data.Add(Index.ToString("00"));
            data.Add(((uint)_Function).ToString());

            this.SendCommand("FUNCTION", data.ToArray());
         }
      }

      public override void WriteXml(System.Xml.XmlWriter writer)
      {
         base.WriteXml(writer);

         writer.WriteAttributeString("Duration", Duration_ms.ToString());
         writer.WriteAttributeString("Delay", Delay_ms.ToString());
         writer.WriteAttributeString("MaxLevel", MaxLevel.ToString());
         writer.WriteAttributeString("UpdateRate", UpdateRate.ToString());
         writer.WriteAttributeString("Function", ((int)Function).ToString());
      }
   }
}
