using HalloweenControllerRPi.Device;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.UI.Xaml;

namespace HalloweenControllerRPi.Functions
{
    abstract public class Function : IFunction, IXmlFunction
    {
        public enum tenTYPE
        {
            TYPE_CONSTANT,
            TYPE_TRIGGER
        }

        public IHostApp _HostApp;
        private DispatcherTimer _timerDuration;
        private DispatcherTimer _timerDelay;
        public Type FuncButtonType;

        public class ProcessFunctionArgs : EventArgs
        {
            char _cFunc;
            char _cSubFunc;
            char _cFuncIndex;
            uint _u32FuncValue;

            public ProcessFunctionArgs(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue)
            {
                _cFunc = cFunc;
                _cSubFunc = subFunc;
                _cFuncIndex = cFuncIndex;
                _u32FuncValue = u32FuncValue;
            }

            public bool UserStopped
            {
                get
                {
                    return (_cFunc == 0 && _cFuncIndex == 0 && _u32FuncValue == 0);
                }
            }
        }

        public Function()
        {
            Data = new List<char>();
            TriggerActive = false;
        }

        public Function(IHostApp host, tenTYPE entype) : this()
        {
            _HostApp = host;
            Type = entype;

            _timerDuration = new DispatcherTimer();
            _timerDuration.Tick += ev_TimerTick_Duration;
            SetTimerInterval(_timerDuration, Duration_ms);

            _timerDelay = new DispatcherTimer();
            _timerDelay.Tick += ev_TimerTick_Delay;
            SetTimerInterval(_timerDelay, MinDelay_ms);

            evOnTrigger += ev_OnTrigger;
        }

        #region Parameters
        public uint Duration_ms
        {
            get { return (uint)new Random().Next((int)MinDuration_ms, (int)MaxDuration_ms); }
        }
        public uint MinDuration_ms { get; set; } = 1000;
        public uint MaxDuration_ms { get; set; } = 2000;

        public uint Delay_ms
        {
            get { return (uint)new Random().Next((int)MinDelay_ms, (int)MaxDelay_ms); }
        }
        public uint MinDelay_ms { get; set; } = 0;
        public uint MaxDelay_ms { get; set; } = 1000;

        public uint Index { get; set; }
        public tenTYPE Type { get; set; }
        public List<char> Data { get; set; }
        public Command FunctionKeyCommand { get; set; }
        public bool TriggerActive { get; set; }

        public EventHandler evOnTrigger { get; set; }
        public EventHandler evOnDelayEnd { get; set; }
        public EventHandler evOnDurationEnd { get; set; }
        public EventHandler evOnFunctionUpdated { get; set; }

        #endregion

        private void SetTimerInterval(DispatcherTimer t, uint value)
        {
            if (value > 0)
            {
                t.Interval = TimeSpan.FromMilliseconds(value);
            }
        }
        public void SendCommand(string cmd)
        {
            List<string> data = new List<string>
            {
                Index.ToString("00"),
                "  0"
            };

            SendCommandToHost(cmd, data.ToArray());
        }

        public void SendCommand<T>(string cmd, T[] val) where T : IConvertible
        {
            List<string> data = new List<string>
            {
                Index.ToString("00")
            };

            foreach (T v in val)
            {
                data.Add(v.ToString().PadLeft(3));
            }

            SendCommandToHost(cmd, data.ToArray());
        }

        public void SendCommand<T>(string cmd, T val) where T : IConvertible
        {
            List<string> data = new List<string>
            {
                Index.ToString("00"),
                val.ToString().PadLeft(3)
            };

            SendCommandToHost(cmd, data.ToArray());
        }

        /// <summary>
        /// Checks the requested COMMAND and then sends it to the HWInterface for processing and transmission.
        /// </summary>
        /// <param name="commandKey"></param>
        /// <param name="lData"></param>
        protected void SendCommandToHost(string commandKey, params string[] lData)
        {
            string data;

            if (_HostApp != null)
            {
                List<Command> availSubFuncCommands;
                Command subFunc = null;

                /* Check if the requested SUBFUNCTION is available/supported on the connected HW */
                availSubFuncCommands = _HostApp.GetSubFunctionCommandsList(this.FunctionKeyCommand);

                if (availSubFuncCommands != null)
                {
                    foreach (Command c in availSubFuncCommands)
                    {
                        if (c.Key == commandKey)
                        {
                            subFunc = c;
                            break;
                        }
                    }
                }
                else
                {
                    throw new Exception("Function not supported/available.");
                }

                if (subFunc != null)
                {
                    /* Build the command */
                    data = _HostApp.BuildCommand(this.FunctionKeyCommand.Key, commandKey, lData);

                    /* TX the command */
                    _HostApp.TransmitCommandToDevice(data);
                }
                else
                {
                    throw new Exception("Sub-Function not supported/available.");
                }
            }
        }

        protected void FireTriggerEnd(Function func)
        {
            /* Check if the HOST (base) form has a callback configured */
            TriggerActive = false;

            if (_HostApp != null)
            {
                _HostApp.TriggerEnd(func);
            }
        }

        /// <summary>
        /// Processes Trigger EVENTs (Serial, Other)
        /// </summary>
        /// <param name="cFunc"></param>
        /// <param name="cFuncIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        virtual public bool ProcessRequest(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue)
        {
            evOnTrigger?.Invoke(this, new ProcessFunctionArgs(cFunc, subFunc, cFuncIndex, u32FuncValue));

            return true;
        }

        public void StopFunction(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue)
        {
            evOnDurationEnd?.Invoke(this, new ProcessFunctionArgs(cFunc, subFunc, cFuncIndex, u32FuncValue));
        }

        /// <summary>
        /// Handles checking for Trigger conditions.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        virtual public bool CheckTriggerConditions(uint value)
        {
            return true;
        }

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ev_OnTrigger(object sender, EventArgs e)
        {
            if (Type == tenTYPE.TYPE_TRIGGER)
            {
                TriggerActive = true;

                if (Delay_ms > 0)
                {
                    SetTimerInterval(_timerDelay, Delay_ms);
                    _timerDelay.Start();
                }
                else
                    PostDelay();
            }
            else
                PostDelay();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ev_TimerTick_Delay(object sender, object e)
        {
            _timerDelay.Stop();

            if (sender is DispatcherTimer)
            {
                PostDelay();
            }
        }

        private void PostDelay()
        {
            evOnDelayEnd?.Invoke(this, EventArgs.Empty);

            if (Type == tenTYPE.TYPE_TRIGGER)
            {
                SetTimerInterval(_timerDuration, Duration_ms);
                _timerDuration.Start();
            }
        }

        private void ev_TimerTick_Duration(object sender, object e)
        {
            _timerDuration.Stop();

            if (sender is DispatcherTimer)
            {
                PostDuration();
            }
        }

        private void PostDuration()
        {
            if (evOnDurationEnd != null)
                evOnDurationEnd.Invoke(this, EventArgs.Empty);

            FireTriggerEnd(this);
        }
        #endregion

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public virtual void ReadXML(XElement element)
        {
            MinDuration_ms = Convert.ToUInt16(element.Attribute("MinDuration").Value);
            MaxDuration_ms = Convert.ToUInt16(element.Attribute("MaxDuration").Value);
            MinDelay_ms = Convert.ToUInt16(element.Attribute("MinDelay").Value);
            MaxDelay_ms = Convert.ToUInt16(element.Attribute("MaxDelay").Value);
        }

        public void ReadXml(XmlReader reader)
        {
            throw new Exception("Deprecated");
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Index", this.Index.ToString("00"));
            writer.WriteAttributeString("MinDuration", MinDuration_ms.ToString());
            writer.WriteAttributeString("MaxDuration", MaxDuration_ms.ToString());
            writer.WriteAttributeString("MinDelay", MinDelay_ms.ToString());
            writer.WriteAttributeString("MaxDelay", MaxDelay_ms.ToString());

        }
    }
}
