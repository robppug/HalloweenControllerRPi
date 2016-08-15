using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HalloweenControllerRPi.Device
{
   public abstract class HWInterface : IHWInterface
   {
      #region Declarations
      public delegate void DataEventHandler<T>(T data);

      public event HostedMessageDelegate CommandReceived;
      public event DataEventHandler<Type> FunctionAdded;
      public event DataEventHandler<string> VersionInfoUpdated;

      public class HWInterfaceException : Exception { public HWInterfaceException(String msg) : base(msg) { } }

      private UInt16 devicePID = 0x0000;

      public UInt16 DevicePID
      {
         get { return devicePID; }
         set { devicePID = value; }
      }

      public const char commandTerminator = '\n';
      #endregion

      protected virtual void OnCommandRecieved(CommandEventArgs args)
      {
         if (CommandReceived != null)
         {
            CommandReceived(this, args);
         }
      }

      protected virtual void OnFunctionAdded(Type funcType)
      {
         if (FunctionAdded != null)
         {
            FunctionAdded(funcType);
         }
      }

      protected virtual void OnVersionInfoUpdated(string sVersion)
      {
         if (this.VersionInfoUpdated != null)
         {
            this.VersionInfoUpdated(sVersion);
         }
      }

      public abstract Dictionary<Command, List<Command>> Commands { get; }
      public abstract string BuildCommand(string func, string subFunc, params string[] data);
      public abstract void DecodeCommand(List<char> fullCmd, out Command function, out Command subFunction, ref char[] data);
      public abstract void FireCommand(string cmd);

      /// <summary>
      /// Command to request HW Board Type information.
      /// </summary>
      public abstract void GetBoardType();

      /// <summary>
      /// Connect HW interface device (ie. Serial Port).
      /// </summary>
      public abstract void Connect();

      /// <summary>
      /// Disconnect HW interface device (ie. Serial Port).
      /// </summary>
      public abstract void Disconnect();

      public virtual void FireCommandReceived(char cmd, char par1, char par2)
      {
         /* Execute TRIGGER command */
         if (this.CommandReceived != null)
            this.CommandReceived.BeginInvoke(this, new CommandEventArgs(cmd, par1, par2), EndAsyncEvent, null);
      }

      public virtual void EndAsyncEvent(IAsyncResult iar)
      {
         //var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
         //var invokedMethod = (HostedMessageDelegate)ar.AsyncDelegate;

         try
         {
            //invokedMethod.EndInvoke(iar);
         }
         catch
         {
            // Handle any exceptions that were thrown by the invoked method
            //Console.WriteLine("An event listener went kaboom!");
         }

      }
      /// <summary>
      /// Handling of Rx SERIAL command interpretation
      /// </summary>
      /// <param name="data"></param>
      /// <returns></returns>
      public abstract bool ProcessCommandRecieved(List<char> data);

      public virtual UserControl GetUIPanel()
      {
         return null;
      }
   }
}
