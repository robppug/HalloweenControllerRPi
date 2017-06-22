using HalloweenControllerRPi.Attributes;
using HalloweenControllerRPi.Device.Controllers;
using HalloweenControllerRPi.Device.Controllers.Channels;
using HalloweenControllerRPi.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;
using static HalloweenControllerRPi.Device.HWController;

namespace HalloweenControllerRPi.Device
{
   public abstract class HWController : IHWController
   {
      protected const string GetFunctionRegexPattern = @"^(?<Function>\w)";
      protected const string GetSubFunctionRegexPattern = @"^\w*\:\s+(?<SubFunction>\w)";
      protected const string GetChannelIndexRegexPattern = @"^(?<ChannelIndex>\d+)";
      protected const string GetValueRegexPattern = @"^\d+\s+(?<Value>\d+)";
      protected const string GetValuesRegexPattern = @"^\d+(?<Value>(?:\s+\d+)+)";

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

      private static Match GetRegexMatch(string pattern, string decodedData)
      {
         Regex regex = new Regex(pattern, RegexOptions.Compiled);
         Match match = regex.Match(decodedData);
         return match;
      }

      protected static char GetFunction(string decodedData)
      {
         //Get the FIRST group of CHAR on a new line
         Match match = GetRegexMatch(GetFunctionRegexPattern, decodedData);

         /* The CHANNEL of the request */
         return Char.Parse(match.Groups["Function"].Value);
      }


      protected static char GetSubFunction(string decodedData)
      {
         //Get the SECOND group of CHAR on a new line
         Match match = GetRegexMatch(GetSubFunctionRegexPattern, decodedData);

         /* The CHANNEL of the request */
         return Char.Parse(match.Groups["SubFunction"].Value);
      }

      protected static uint GetChannelIndex(string decodedData)
      {
         //Get the FIRST group of DIGITS on a new line
         Match match = GetRegexMatch(GetChannelIndexRegexPattern, decodedData);

         /* The CHANNEL of the request */
         return UInt32.Parse(match.Groups["ChannelIndex"].Value);
      }

      protected static uint GetValue(string decodedData)
      {
         //Get the SECOND group of DIGITS on a new line
         Match match = GetRegexMatch(GetValueRegexPattern, decodedData);

         return UInt32.Parse(match.Groups["Value"].Value);
      }

      protected static uint[] GetValues(string decodedData)
      {
         List<uint> values = new List<uint>();

         Match match = GetRegexMatch(GetValuesRegexPattern, decodedData);
         Regex valuesRegex = new Regex(@"(?<Value>(?:\s*)\d+)", RegexOptions.Compiled);
         MatchCollection matches = valuesRegex.Matches(match.Groups["Value"].Value);

         foreach (Match m in matches)
         {
            values.Add(UInt32.Parse(m.Groups["Value"].Value));
         }

         return values.ToArray();
      }

      protected Command GetSubFunctionCommand(Command function, string subFunc)
      {
         Command command = null;

         foreach (Command c in this.Commands[function].ToList())
         {
            if (c.Key == subFunc)
            {
               command = c;
            }
         }

         return command;
      }

      protected Command GetFunctionCommand(string p)
      {
         Command command = null;

         foreach (Command c in this.Commands.Keys)
         {
            if (c.Key == p)
            {
               command = c;
               break;
            }
         }

         return command;
      }

      public virtual string BuildCommand(string func, string subFunc, params string[] data)
      {
         StringBuilder fullCommand = new StringBuilder();

         Command function = GetFunctionCommand(func);
         Command subFunction = GetSubFunctionCommand(function, subFunc);

         if (function == null)
         {
            throw new HWInterfaceException("Function " + func + "  not available.");
         }

         fullCommand.Append(function.Value.ToString() + ": ");

         if (subFunc != null)
            fullCommand.Append(subFunction.Value.ToString());

         if (data.Length != 0)
         {
            foreach (string s in data)
               fullCommand.Append(" " + s);
         }

         fullCommand.Append(commandTerminator);

         return fullCommand.ToString();
      }

      /// <summary>
      /// Processed RX'ed commands and decodes the byte array, returning the Function, Sub-Function and Data (if any).
      /// </summary>
      /// <param name="command">List of bytes (actual RX'ed data)</param>
      /// <param name="function">Decoded FUNCTION (out param nullable)</param>
      /// <param name="subFunction">Decoded SUBFUNCTION (out param nullable)</param>
      /// <param name="data">Decoded Data Array (ref param)</param>
      public virtual void DecodeCommand(string fullCmd, out Command function, out Command subFunction, ref char[] data)
      {
         char l_FuncCommand = GetFunction(fullCmd);
         char l_SubCommand = GetSubFunction(fullCmd);

         function = null;
         foreach (Command c in Commands.Keys)
         {
            if (c.Value == l_FuncCommand)
            {
               function = c;
               break;
            }
         }

         subFunction = null;
         foreach (Command c in Commands[function].ToList())
         {
            if (c.Value == l_SubCommand)
            {
               subFunction = c;
            }
         }

         fullCmd.CopyTo(5, data, 0, fullCmd.Length - 5);
      }

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
