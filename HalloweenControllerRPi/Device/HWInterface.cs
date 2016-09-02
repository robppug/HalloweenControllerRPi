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

      protected virtual void OnCommandReceived(CommandEventArgs args)
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
      public abstract uint Inputs { get; }
      public abstract uint PWMs { get; }
      public abstract uint Relays { get; }

      public abstract string BuildCommand(string func, string subFunc, params string[] data);
      public abstract void DecodeCommand(List<char> fullCmd, out Command function, out Command subFunction, ref char[] data);

      /// <summary>
      /// Command has been requested that needs processing (eg. TX Serial)
      /// </summary>
      /// <param name="cmd"></param>
      public abstract void TransmitCommand(string cmd);

      /// <summary>
      /// Connect HW interface device (ie. Serial Port).
      /// </summary>
      public abstract void Connect();

      /// <summary>
      /// Disconnect HW interface device (ie. Serial Port).
      /// </summary>
      public abstract void Disconnect();

      public virtual void TriggerCommandReceived(CommandEventArgs args)
      {
         /* Execute TRIGGER command */
         if (this.CommandReceived != null)
         {
            this.CommandReceived.Invoke(this, args);
         }
      }

      /// <summary>
      /// Command has been received that needs processing (eg. RX Serial)
      /// </summary>
      /// <param name="data"></param>
      /// <returns>True if COMMAND was successfully handled</returns>
      public virtual bool ReceivedCommand(List<char> data)
      {
         return false;
      }

      public virtual UserControl GetUIPanel()
      {
         return null;
      }
   }
}
