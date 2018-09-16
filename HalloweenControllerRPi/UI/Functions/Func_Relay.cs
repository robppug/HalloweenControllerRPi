using HalloweenControllerRPi.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HalloweenControllerRPi.Functions
{
    public class Func_RELAY : Function
    {
        public enum tenOutputLevel
        {
            tLow = 0,
            tHigh = 1
        };

        public Func_RELAY()
        {
        }

        public Func_RELAY(IHostApp host, tenTYPE entype)
           : base(host, entype)
        {
            FunctionKeyCommand = new Command("RELAY", 'R');

            evOnDelayEnd += OnTrigger;
            evOnDurationEnd += OnDurationEnd;
        }

        /// <summary>
        /// This does the actual processing of the onTrigger EVENT.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrigger(object sender, EventArgs e)
        {
            SendCommand("SET", (uint)tenOutputLevel.tHigh);
        }

        private void OnDurationEnd(object sender, EventArgs e)
        {
            SendCommand("SET", (uint)tenOutputLevel.tLow);
        }
    }
}