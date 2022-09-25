using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device
{
    public class CommandEventArgs : EventArgs
    {
        public char Commamd
        {
            get;
            private set;
        }
        public char SubCommamd
        {
            get;
            private set;
        }

        public uint Index
        {
            get;
            private set;
        }

        public uint Value
        {
            get;
            private set;
        }

        public CommandEventArgs(char cmd, char subcmd, uint par1, uint par2)
        {
            this.Commamd = cmd;
            this.SubCommamd = subcmd;
            this.Index = par1;
            this.Value = par2;
        }
    }
}
