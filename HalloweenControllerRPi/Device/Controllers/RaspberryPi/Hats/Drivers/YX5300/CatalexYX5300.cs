using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevices;
using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.SC16IS752;
using HalloweenControllerRPi.Device.Drivers;
using System;
using System.Collections.Generic;
using System.IO;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.Drivers
{
   public class Catalex_YX5300<T> : IDriverSoundProvider, IDeviceCommsProvider<T> where T : IDeviceComms
   {
      private bool _initialised = false;
      private T _stream;
      private List<byte> _dataBuffer = new List<byte>();

      public T BusDeviceComms
      {
         get { return _stream; }
         private set { _stream = value; }
      }

      public enum COMMANDS
      {
         NEXT_SONG = 0X01,  // Play next song.
         PREV_SONG = 0X02,  // Play previous song.
         PLAY_W_INDEX = 0X03,
         VOLUME_UP = 0X04,
         VOLUME_DOWN = 0X05,
         SET_VOLUME = 0X06,
         SNG_CYCL_PLAY = 0X08,  // Single Cycle Play.
         SEL_DEV = 0X09,
         SLEEP_MODE = 0X0A,
         WAKE_UP = 0X0B,
         RESET = 0X0C,
         PLAY = 0X0D,
         PAUSE = 0X0E,
         PLAY_FOLDER_FILE = 0X0F,
         STOP_PLAY = 0X16,
         FOLDER_CYCLE = 0X17,
         SHUFFLE_PLAY = 0x18,
         SET_SNGL_CYCL = 0X19, // Set single cycle.
         SET_DAC = 0X1A,
         PLAY_W_VOL = 0X22,
      }

      private static void BuildCommand(ref List<byte> buffer, COMMANDS command, ushort dat = 0)
      {
         if (buffer != null)
         {
            buffer.Add(0x7e);
            buffer.Add(0xff);
            buffer.Add(0x06);   // Len
            buffer.Add((byte)command);
            buffer.Add(0x00);   // 0x00 NO, 0x01 feedback
            buffer.Add((byte)(dat >> 8));  //datah
            buffer.Add((byte)(dat));       //datal
            buffer.Add(0xef);
         }
      }

      public Catalex_YX5300()
      {
         _initialised = false;
      }

      public void InitialiseDriver(bool proceedOnFail = false)
      {
         if(_initialised == true)
         {
            BuildCommand(ref _dataBuffer, COMMANDS.SEL_DEV, 0x02);

            _stream.Write(_dataBuffer.ToArray());
            _dataBuffer.Clear();
         }
         else
         {
            throw new Exception("Driver needs to be Opened first.");
         }
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

      public void Play(byte track, byte volume = 30)
      {
         List<byte> data = new List<byte>();
         ushort reg = 0x0000;

         if (track == 0)
            throw new Exception("Track 0 is invalid (Range 1 to 255)");
         if (volume < 0)
            volume = 0;
         else if (volume > 30)
            volume = 30;

         reg = (ushort)((volume << 8) | track);

         BuildCommand(ref data, COMMANDS.PLAY_W_VOL, reg);

         _stream.Write(data.ToArray());
      }

      public void Stop()
      {
         List<byte> data = new List<byte>();

         BuildCommand(ref data, COMMANDS.STOP_PLAY);

         _stream.Write(data.ToArray());
      }

   }
}
