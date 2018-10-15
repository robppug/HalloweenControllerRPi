using HalloweenControllerRPi.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

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

        public tenTriggerLvl TriggerLevel { get; set; }

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
                SendCommand("POSTDEBTIME", _postTriggerDelay_ms);
            }
        }

        public bool Enabled { get; set; } = false;

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

        public override bool CheckTriggerConditions(uint u32value)
        {
            if (Enabled)
            {
                return (u32value == (uint)TriggerLevel);
            }
            else
                return false;
        }

        public override void ReadXML(XElement element)
        {
            base.ReadXML(element);

            Enabled = Convert.ToBoolean(element.Attribute("Enabled").Value);
            TriggerLevel = (Func_INPUT.tenTriggerLvl)Convert.ToUInt16(element.Attribute("TriggerLevel").Value);
            DebounceTime_ms = Convert.ToUInt16(element.Attribute("DebounceTime").Value);
            PostTriggerDelay_ms = Convert.ToUInt16(element.Attribute("PostTriggerTime").Value);
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteAttributeString("Enabled", Enabled.ToString());
            writer.WriteAttributeString("TriggerLevel", TriggerLevel.GetHashCode().ToString());
            writer.WriteAttributeString("DebounceTime", _debounceTime_ms.ToString());
            writer.WriteAttributeString("PostTriggerTime", _postTriggerDelay_ms.ToString());
        }

        public override bool ProcessRequest(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue)
        {
            bool boValid = false;

            if (CheckTriggerConditions(u32FuncValue))
            {
                boValid = base.ProcessRequest(cFunc, subFunc, cFuncIndex, u32FuncValue);
            }

            return boValid;
        }
    }
}
