using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi;
using Windows.UI.Xaml.Controls;

namespace HalloweenControllerRPi.Device
{
   public abstract class HWController : IHWController
   {
      #region Declarations
      public delegate void DataEventHandler<T>(T data);

      public event HostedMessageDelegate CommandReceived;
      public event DataEventHandler<Type> FunctionAdded;
      public event DataEventHandler<string> VersionInfoUpdated;
      public event DataEventHandler<uint> DiscoveryProgress;
      public event EventHandler ControllerInitialised;

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
         CommandReceived?.Invoke(this, args);
      }

      protected virtual void OnFunctionAdded(Type funcType)
      {
         FunctionAdded?.Invoke(funcType);
      }

      protected virtual void OnVersionInfoUpdated(string sVersion)
      {
         this.VersionInfoUpdated?.Invoke(sVersion);
      }

      protected virtual void OnControllerInitialised()
      {
         this.ControllerInitialised?.Invoke(this, EventArgs.Empty);
      }

      protected virtual void OnDiscoveryProgressUpdated(uint percentage)
      {
         this.DiscoveryProgress?.Invoke(percentage);
      }

      public void OnInputChannelNotification(object sender, ChannelFunction_INPUT.EventArgsINPUT e)
      {
         this.TriggerCommandReceived(new CommandEventArgs('I', e.Index + 1, (uint)e.TriggerLevel));
      }

      public abstract Dictionary<Command, List<Command>> Commands { get; }
      public abstract uint Inputs { get; }
      public abstract uint PWMs { get; }
      public abstract uint Relays { get; }
      public abstract uint SoundChannels { get; }

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
