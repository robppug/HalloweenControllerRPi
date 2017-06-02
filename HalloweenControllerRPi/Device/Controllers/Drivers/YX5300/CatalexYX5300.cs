using HalloweenControllerRPi.Attributes;
using HalloweenControllerRPi.Device.Controllers.BusDevices;
using HalloweenControllerRPi.Device.Controllers.Providers;
using HalloweenControllerRPi.Extentions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.Drivers
{

   public enum Commands
   {
      [AccessType("W")]
      NEXT_SONG = 0X01,  // Play next song.
      [AccessType("W")]
      PREV_SONG = 0X02,  // Play previous song.
      [AccessType("W")]
      PLAY_W_INDEX = 0X03,
      [AccessType("W")]
      VOLUME_UP = 0X04,
      [AccessType("W")]
      VOLUME_DOWN = 0X05,
      [AccessType("W")]
      SET_VOLUME = 0X06,
      [AccessType("W")]
      SNG_CYCL_PLAY = 0X08,  // Single Cycle Play.
      [AccessType("W")]
      SEL_DEV = 0X09,
      [AccessType("W")]
      SLEEP_MODE = 0X0A,
      [AccessType("W")]
      WAKE_UP = 0X0B,
      [AccessType("W")]
      RESET = 0X0C,
      [AccessType("W")]
      PLAY = 0X0D,
      [AccessType("W")]
      PAUSE = 0X0E,
      [AccessType("W")]
      PLAY_FOLDER_FILE = 0X0F,
      [AccessType("W")]
      STOP_PLAY = 0X16,
      [AccessType("W")]
      FOLDER_CYCLE = 0X17,
      [AccessType("W")]
      SHUFFLE_PLAY = 0x18,
      [AccessType("W")]
      SET_SNGL_CYCL = 0X19, // Set single cycle.
      [AccessType("W")]
      SET_DAC = 0X1A,
      [AccessType("W")]
      PLAY_W_VOL = 0X22,
      [AccessType("R")]
      QUERY_CURRENT_STATUS = 0X42,
      [AccessType("R")]
      QUERY_CURRENT_VOLUME = 0X43,
      [AccessType("R")]
      QUERY_CURRENT_EQ = 0X44,
      [AccessType("R")]
      QUERY_TOTAL_TRACKS_TF = 0X48,
      [AccessType("R")]
      QUERY_CURRENT_TRACKS_TF = 0X4C,
   }

   /// <summary>
   /// Register ENUM extension class
   /// </summary>
   internal static class CommandEnumExtensions
   {
      public static bool CanRead(this Commands cmd)
      {
         AccessTypeAttribute mde = EnumExtension<AccessTypeAttribute, Commands>.GetModeAttribute(cmd);
         if (mde != null)
            return mde.Read;
         else
            return true;
      }

      public static bool CanWrite(this Commands cmd)
      {
         AccessTypeAttribute mde = EnumExtension<AccessTypeAttribute, Commands>.GetModeAttribute(cmd);
         if (mde != null)
            return mde.Write;
         else
            return true;
      }
   }

   internal class CataLexCommand
   {
      public Commands Command;
      public ushort Data;
   }

   public class Catalex_YX5300<T> : ISoundProvider, IDeviceCommsProvider<T> where T : IDeviceComms
   {
      private bool _initialised = false;
      private T _stream;
      private List<byte> _dataBuffer = new List<byte>();
      private const byte MAX_VOLUME = 30;

      public T BusDeviceComms
      {
         get { return _stream; }
         private set { _stream = value; }
      }

      private static void BuildCommand(ref List<byte> buffer, Commands command, bool feedback, ushort dat = 0)
      {
         if (buffer != null)
         {
            buffer.Add(0x7e);
            buffer.Add(0xff);
            buffer.Add(0x06);   // Len
            buffer.Add((byte)command);
            buffer.Add((byte)(feedback ? 0x01 : 0x00));   // 0x00 NO, 0x01 feedback
            buffer.Add((byte)(dat >> 8));  //datah
            buffer.Add((byte)(dat));       //datal
            buffer.Add(0xef);
         }
      }

      private static CataLexCommand DecodeCommand(ref List<byte> buffer)
      {
         CataLexCommand result = new CataLexCommand()
         {
            Command = (Commands)buffer[3],
            Data = (ushort)((buffer[5] << 8) | buffer[6])
         };

         return result;
      }
      

      public Catalex_YX5300()
      {
         _initialised = false;
      }

      public void InitialiseDriver(bool proceedOnFail = false)
      {
         if(_initialised == true)
         {
            _stream.DataReceived += _stream_DataReceived;
            BuildCommand(ref _dataBuffer, Commands.SEL_DEV, false, 0x02);

            _stream.Write(_dataBuffer.ToArray());
            _dataBuffer.Clear();
         }
         else
         {
            throw new Exception("Driver needs to be Opened first.");
         }
      }

      private void _stream_DataReceived(object sender, EventArgs e)
      {
         
      }

      public void Open(T stream)
      {
         if (_initialised == false)
         {
            _stream = stream;
            _initialised = true;
         }
         else
         {
            throw new Exception("Driver (" + this + ") is already Open.");
         }
      }

      public void Close()
      {
         _initialised = false;
         _stream = default(T);
      }

      public void Play(byte? track, byte? volume = MAX_VOLUME)
      {
         ushort reg = 0x0000;

         if (volume == null)
            volume = MAX_VOLUME;

         volume /= (100 / MAX_VOLUME);

         if (volume < 0)
            volume = 0;
         else if (volume > MAX_VOLUME)
            volume = MAX_VOLUME;

         reg = (ushort)((volume << 8) | track);

         if(track == null)
            BuildCommand(ref _dataBuffer, Commands.PLAY, false, reg);
         else
         {
            if (track == 0)
               throw new Exception("Track 0 is invalid (Range 1 to 255)");

            BuildCommand(ref _dataBuffer, Commands.PLAY_W_VOL, false, reg);
         }

         _stream.Write(_dataBuffer.ToArray());
         _dataBuffer.Clear();
      }

      public void Stop()
      {
         BuildCommand(ref _dataBuffer, Commands.STOP_PLAY, false);

         _stream.Write(_dataBuffer.ToArray());
         _dataBuffer.Clear();
      }

      public async void GetNumberOfTracks()
      {
         List<byte> readData = new List<byte>();

         BuildCommand(ref _dataBuffer, Commands.QUERY_TOTAL_TRACKS_TF, true);

         _stream.Write(_dataBuffer.ToArray());
         _dataBuffer.Clear();

         await Task.Delay(1000);

         readData.AddRange(_stream.Read(14));

         //return (readData[0]);
      }

      public int GetCurrentTrack()
      {
         return 0;
      }

   }

}
