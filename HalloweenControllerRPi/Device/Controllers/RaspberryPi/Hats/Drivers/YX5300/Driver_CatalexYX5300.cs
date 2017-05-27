using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.SC16IS752;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.Interfaces.BusDevices.Drivers
{
   public class Catalex_YX5300 : IDriverSoundProvider
   {
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
         PLAYING_N = 0x4C,
         QUERY_STATUS = 0x42,
         QUERY_VOLUME = 0x43,
         QUERY_FLDR_TRACKS = 0x4e,
         QUERY_TOT_TRACKS = 0x48,
         QUERY_FLDR_COUNT = 0x4f
      }

      public void InitialiseDriver()
      {
         //throw new NotImplementedException();
      }

      public void BuildCommand(ref List<byte> buffer, COMMANDS command, uint dat = 0)
      {
         if (buffer != null)
         {
            buffer.Add(0x7e);   //
            buffer.Add(0xff);   //
            buffer.Add(0x06);   // Len
            buffer.Add((byte)command);//
            buffer.Add(0x01);   // 0x00 NO, 0x01 feedback
            buffer.Add((byte)(dat >> 8));  //datah
            buffer.Add((byte)(dat));       //datal
            buffer.Add(0xef);   //
         }
      }
   }
}
