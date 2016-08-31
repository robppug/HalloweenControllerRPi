using HalloweenControllerRPi.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Functions
{
   public class Func_INPUT : Function
   {
      public enum tenTriggerLvl
      {
         tLow = 0,
         tHigh = 1
      };

      private uint _debounceTime_ms;
      private tenTriggerLvl _triggerLevel;

      public tenTriggerLvl TriggerLevel
      {
         get { return _triggerLevel; }
         set { _triggerLevel = value; }
      }

      public uint DebounceTime_ms
      {
         get { return _debounceTime_ms; }
         set
         {
            List<string> data = new List<string>();

            _debounceTime_ms = value;

            data.Add(Index.ToString("00"));
            data.Add(_debounceTime_ms.ToString());

            /* Notify the HW to configure its input debouncing time */
            this.SendCommand("DEBTIME", data.ToArray());
         }
      }
      
      public Func_INPUT()
      {

      }

      public Func_INPUT(IHostApp host, tenTYPE entype)
         : base(host, entype)
      {
         FunctionKeyCommand = new Command("INPUT", 'I');

         evOnTrigger += new EventHandler(OnTrigger);
      }

      private void OnTrigger(object sender, EventArgs e)
      {
         //throw new NotImplementedException();
      }

      public override bool boCheckTriggerConditions(uint u32value)
      {
         return (u32value == (uint)_triggerLevel);
      }

      public override void WriteXml(System.Xml.XmlWriter writer)
      {
         base.WriteXml(writer);

         writer.WriteAttributeString("TriggerLevel", _triggerLevel.GetHashCode().ToString());
         writer.WriteAttributeString("DebounceTime", _debounceTime_ms.GetHashCode().ToString());
      }
      public override void ReadXml(System.Xml.XmlReader reader)
      {
         base.ReadXml(reader);

         //RPUGLIESE - Is this needed??  Check it!
         if (reader.GetAttribute("TriggerLevel") != null)
         {
            this._triggerLevel = (tenTriggerLvl)Convert.ToUInt16(reader.GetAttribute("TriggerLevel"));
         }

         if (reader.GetAttribute("DebounceTime") != null)
         {
            this._debounceTime_ms = Convert.ToUInt16(reader.GetAttribute("DebounceTime"));
         }
      }

      public override bool boProcessRequest(char cFunc, char cFuncIndex, uint u32FuncValue)
      {
         bool boValid = false;

         if (boCheckTriggerConditions(u32FuncValue))
         {
            boValid = base.boProcessRequest(cFunc, cFuncIndex, u32FuncValue);
         }

         return boValid;
      }
   }
}
