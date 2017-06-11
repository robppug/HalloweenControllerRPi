using HalloweenControllerRPi.Attributes;
using HalloweenControllerRPi.Device.Controllers;
using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Extentions;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using static HalloweenControllerRPi.Device.HWController;

namespace HalloweenControllerRPi.Device
{
   public abstract class HWController : IHWController
   {
      #region Declarations
      public delegate void DataEventHandler<T>(T data);

      public event HostedMessageDelegate CommandReceived;
      public event DataEventHandler<uint> DiscoveryProgress;
      public event EventHandler ControllerInitialised;

      public class HWInterfaceException : Exception { public HWInterfaceException(String msg) : base(msg) { } }

      public const char commandTerminator = '\n';
      #endregion

      protected virtual void OnCommandReceived(CommandEventArgs args)
      {
         CommandReceived?.Invoke(this, args);
      }

      protected virtual void OnControllerInitialised()
      {
         ControllerInitialised?.Invoke(this, EventArgs.Empty);
      }

      protected virtual void OnDiscoveryProgressUpdated(uint percentage)
      {
         DiscoveryProgress?.Invoke(percentage);
      }

      public abstract void OnChannelNotification(object sender, CommandEventArgs e);

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

      public virtual void TransmitCommand(CommandEventArgs args)
      {
         /* Execute TRIGGER command */
         CommandReceived?.Invoke(this, args);
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
