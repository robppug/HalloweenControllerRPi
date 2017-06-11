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
      private uint _postTriggerDelay_ms;
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
            _debounceTime_ms = value;
            SendCommand("DEBTIME", _debounceTime_ms);
         }
      }

      public uint PostTriggerDelay_ms
      {
         get { return _postTriggerDelay_ms; }
         set
         {
            _postTriggerDelay_ms = value;

            SendCommand("POSTDEBTIME", _debounceTime_ms);
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
         writer.WriteAttributeString("PostTriggerTime", _postTriggerDelay_ms.GetHashCode().ToString());
      }

      public override bool boProcessRequest(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue)
      {
         bool boValid = false;

         if (boCheckTriggerConditions(u32FuncValue))
         {
            boValid = base.boProcessRequest(cFunc, subFunc, cFuncIndex, u32FuncValue);
         }

         return boValid;
      }
   }
}
