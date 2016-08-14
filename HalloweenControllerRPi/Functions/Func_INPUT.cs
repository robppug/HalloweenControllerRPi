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
      public enum TriggerLvl
      {
         tLow = 0,
         tHigh = 1
      };
      public TriggerLvl triggerLevel;

      public Func_INPUT() { }

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
         return (u32value == (uint)triggerLevel);
      }

      public override void WriteXml(System.Xml.XmlWriter writer)
      {
         base.WriteXml(writer);

         writer.WriteAttributeString("TriggerLevel", triggerLevel.GetHashCode().ToString());
      }
      public override void ReadXml(System.Xml.XmlReader reader)
      {
         base.ReadXml(reader);

         if (reader.GetAttribute("TriggerLevel") != null)
         {
            this.triggerLevel = (TriggerLvl)Convert.ToUInt16(reader.GetAttribute("TriggerLevel"));
         }
      }

      public override List<char> SerializeSequence()
      {
         /* Create the serialised data:   
          *    "I (type) (index) (triglvl)"   */
         this.Data.AddRange("I" + ' ' + (char)((int)this.Type + 0x30) + ' ' + (char)(this.Index + 0x30) + ' ' + (char)((int)triggerLevel + 0x30));

         return this.Data;
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
