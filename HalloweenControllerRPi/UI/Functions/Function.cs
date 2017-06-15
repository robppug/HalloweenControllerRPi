using HalloweenControllerRPi.Device;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Windows.UI.Xaml;

namespace HalloweenControllerRPi.Functions
{
   abstract public class Function : IFunction, IXmlSerializable
   {
      public enum tenTYPE
      {
         TYPE_CONSTANT,
         TYPE_TRIGGER
      }

      public IHostApp _HostApp;

      private uint _Duration_ms;
      private uint _Delay_ms;
      private uint _Index;
      private bool _TriggerActive;
      private tenTYPE _enType;
      private List<char> _Data;
      private Command _FunctionKeyCommand;

      private DispatcherTimer _timerDuration;
      private DispatcherTimer _timerDelay;
 
      private EventHandler _evOnTrigger;
      private EventHandler _evOnDelayEnd;
      private EventHandler _evOnDurationEnd;
      private EventHandler _evOnFunctionUpdated;

      public Type FuncButtonType;
      
      public class ProcessFunctionArgs : EventArgs
      {
         char _cFunc;
         char _cSubFunc;
         char _cFuncIndex;
         uint  _u32FuncValue;

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
         _Data = new List<char>();
         _TriggerActive = false;
      }

      public Function(IHostApp host, tenTYPE entype) : this()
      {
         _HostApp = host;
         _enType = entype;

         _timerDuration = new DispatcherTimer();
         _timerDuration.Tick += ev_TimerTick_Duration;
         this.vSetTimerInterval(_timerDuration, this._Duration_ms);

         _timerDelay = new DispatcherTimer();
         _timerDelay.Tick += ev_TimerTick_Delay;
         this.vSetTimerInterval(_timerDelay, this._Delay_ms);

         _evOnTrigger += ev_OnTrigger;
      }

      #region Parameters
      public uint Duration_ms
      {
         get { return _Duration_ms; }
         set { _Duration_ms = value; }
      }
      public uint Delay_ms
      {
         get { return _Delay_ms; }
         set { _Delay_ms = value; }
      }
      public uint Index
      {
         get { return _Index; }
         set { _Index = value; }
      }
      public EventHandler evOnTrigger
      {
         get { return _evOnTrigger; }
         set { _evOnTrigger = value; }
      }
      public EventHandler evOnDelayEnd
      {
         get { return _evOnDelayEnd; }
         set { _evOnDelayEnd = value; }
      }
      public EventHandler evOnDurationEnd
      {
         get { return _evOnDurationEnd; }
         set { _evOnDurationEnd = value; }
      }
      public EventHandler evOnFunctionUpdated
      {
         get { return _evOnFunctionUpdated; }
         set { _evOnFunctionUpdated = value; }
      }
      public tenTYPE Type
      {
         get { return _enType; }
         set { _enType = value; }
      }
      public List<char> Data
      {
         get { return _Data; }
         set { _Data = value; }
      }
      public Command FunctionKeyCommand
      {
         get { return _FunctionKeyCommand; }
         set { _FunctionKeyCommand = value; }
      }

      public bool TriggerActive
      {
         get { return _TriggerActive; }
         set { _TriggerActive = value; }
      }
      #endregion

      private void vSetTimerInterval(DispatcherTimer t, uint value)
      {
         if (value > 0)
         {
            t.Interval = TimeSpan.FromMilliseconds(value);
         }
      }
      public void SendCommand(string cmd)
      {
         List<string> data = new List<string>();

         data.Add(Index.ToString("00"));
         data.Add("  0");

         SendCommandToHost(cmd, data.ToArray());
      }

      public void SendCommand<T>(string cmd, T[] val) where T : IConvertible
      {
         List<string> data = new List<string>();

         data.Add(Index.ToString("00"));
         foreach(T v in val)
         {
            data.Add(v.ToString().PadLeft(3));
         }

         SendCommandToHost(cmd, data.ToArray());
      }

      public void SendCommand<T>(string cmd, T val) where T : IConvertible
      {
         List<string> data = new List<string>();

         data.Add(Index.ToString("00"));
         data.Add(val.ToString().PadLeft(3));

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

         if (this._HostApp != null)
         {
            List<Command> availSubFuncCommands;
            Command subFunc = null;

            /* Check if the requested SUBFUNCTION is available/supported on the connected HW */
            availSubFuncCommands = this._HostApp.GetSubFunctionCommandsList(this.FunctionKeyCommand);

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
               data = this._HostApp.BuildCommand(this.FunctionKeyCommand.Key, commandKey, lData);

               /* TX the command */
               this._HostApp.TransmitCommandToDevice(data);
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

         if (this._HostApp != null)
         {
            this._HostApp.TriggerEnd(func);
         }
      }
      
      /// <summary>
      /// Processes Trigger EVENTs (Serial, Other)
      /// </summary>
      /// <param name="cFunc"></param>
      /// <param name="cFuncIndex"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      virtual public bool boProcessRequest(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue)
      {
         if (evOnTrigger != null)
         {
            evOnTrigger.Invoke(this, new ProcessFunctionArgs(cFunc, subFunc, cFuncIndex, u32FuncValue));
         }

         return true;
      }

      public void vStopFunction(char cFunc, char subFunc, char cFuncIndex, uint u32FuncValue)
      {
         if (evOnDurationEnd != null)
         {
            evOnDurationEnd.Invoke(this, new ProcessFunctionArgs(cFunc, subFunc, cFuncIndex, u32FuncValue));
         }
      }

      /// <summary>
      /// Handles checking for Trigger conditions.
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      virtual public bool boCheckTriggerConditions(uint value)
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
         if (_enType == tenTYPE.TYPE_TRIGGER)
         {
            TriggerActive = true;

            if (Delay_ms > 0)
            {
               vSetTimerInterval(_timerDelay, _Delay_ms);
               _timerDelay.Start();
            }
            else
               t_PostDelay();
         }
         else
            t_PostDelay();
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
            t_PostDelay();
         }
      }

      private void t_PostDelay()
      {
         if (evOnDelayEnd != null)
            evOnDelayEnd.Invoke(this, EventArgs.Empty);

         if (_enType == tenTYPE.TYPE_TRIGGER)
         {
            vSetTimerInterval(_timerDuration, _Duration_ms);
            _timerDuration.Start();
         }
      }

      private void ev_TimerTick_Duration(object sender, object e)
      {
         _timerDuration.Stop();

         if (sender is DispatcherTimer)
         {
            t_PostDuration();
         }
      }

      private void t_PostDuration()
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

      virtual public void ReadXml(System.Xml.XmlReader reader)
      {
         
      }

      virtual public void WriteXml(System.Xml.XmlWriter writer)
      {
         writer.WriteAttributeString("Index", this.Index.ToString("00"));
      }
   }
}
