using HalloweenControllerRPi.Device;
using HalloweenControllerRPi.Functions;
using System.Collections.Generic;

namespace HalloweenControllerRPi
{
   /// <summary>
   /// Event type for stuff and things
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="args"></param>
   public delegate void HostedMessageDelegate(object sender, CommandEventArgs args);

   /// <summary>
   /// Host for processings
   /// </summary>
   public interface IHostApp
   {
      /// <summary>
      /// Transmit the COMMAND to the HW Device
      /// </summary>
      /// <param name="cmd"></param>
      void TransmitCommandToDevice(string cmd);

      /// <summary>
      /// Builds a command based on the information provided.
      /// </summary>
      /// <param name="function"></param>
      /// <param name="subFunc"></param>
      /// <param name="data"></param>
      /// <returns></returns>
      string BuildCommand(string function, string subFunc, params string[] data);

      /// <summary>
      /// Get a List of available command based on requested Function Key.
      /// </summary>
      /// <param name="functionKey"></param>
      /// <returns></returns>
      List<Command> GetSubFunctionCommandsList(Command functionKey);

      /// <summary>
      /// Called when a Trigger sequence has completed.
      /// </summary>
      /// <param name="func"></param>
      void TriggerEnd(Function func);
   }
}
