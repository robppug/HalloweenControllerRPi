using HalloweenControllerRPi.Device.Controllers.BusDevices;
using HalloweenControllerRPi.Device.Controllers.Providers;
using HalloweenControllerRPi.UI;
using HalloweenControllerRPi.UI.ExternalDisplay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace HalloweenControllerRPi.Device.Drivers
{
   class SSD1306<T> : IDeviceCommsProvider<T>, IDriverDisplayProvider where T : IDeviceComms
   {
      /* This driver is intended to be used with Driver_SSD1306 based OLED displays connected via I2c */
      private byte[] DisplayBuffer;     
      private int Rotation = 1;
      private T _stream;
      private ThreadPoolTimer displayRefreshTimer;
      private object _Lock = new object();

      public bool RefreshDisplay { get; set; } = true;

      public bool Initialised { get; private set; }

      public T BusDeviceComms
      {
         get { return _stream; }
         private set { _stream = value; }
      }

      public int Width { get; set; } = 128;
      public int Height { get; set; } = 64;

      public SSD1306(int w, int h)
      {
         Initialised = false;

         Width = w;
         Height = h;

         DisplayBuffer = new byte[Width * Height / 8];

         DisplayBuffer.Initialize();
      }

      public void Open(T stream)
      {
         if (Initialised == false)
         {
            _stream = stream;
            Initialised = true;
         }
         else
         {
            throw new Exception("Driver (" + this + ") is already Open.");
         }
      }

      public void Close()
      {
         if (Initialised == true)
         {
            _stream = default(T);

            Initialised = false;
         }
      }


      /* Display commands. See the datasheet for details on commands: http://www.adafruit.com/datasheets/Driver_SSD1306.pdf                        */
      private static readonly byte[] CMD_DISPLAY_OFF           = { 0xAE };             /* Turns the display off                                    */
      private static readonly byte[] CMD_DISPLAY_ON            = { 0xAF };             /* Turns the display on                                     */
      private static readonly byte[] CMD_DISPLAYALLON_RESUME   = { 0xA4 };
      private static readonly byte[] CMD_NORM_DISPLAY          = { 0xA6 };
      private static readonly byte[] CMD_DISPLAY_CLKDIV        = { 0xD5, 0x80 };
      private static readonly byte[] CMD_MULTIPLEX             = { 0xA8, 0x3F };       /* 1F = 32H, 3F = 64H */
      private static readonly byte[] CMD_DISPLAY_OFFSET        = { 0xD3, 0x00 };
      private static readonly byte[] CMD_STARTLINE             = { 0x40 };
      private static readonly byte[] CMD_SETPRECHARGE          = { 0xD9, 0xF1 };
      private static readonly byte[] CMD_SETVCOMDETECT         = { 0xDB, 0x40 };
      private static readonly byte[] CMD_SETCOMPINS            = { 0xDA, 0x12 };       /* 0x02 = 32H, 0x12 = 64H */
      private static readonly byte[] CMD_SETCONTRAST           = { 0x81, 0xCF };
      private static readonly byte[] CMD_CHARGEPUMP_ON         = { 0x8D, 0x14 };       /* Turn on internal charge pump to supply power to display  */
      private static readonly byte[] CMD_MEMADDRMODE           = { 0x20, 0x00 };       /* Horizontal memory mode                                   */
      private static readonly byte[] CMD_SEGREMAP              = { 0xA0 };             /* Remaps the segments, which has the effect of mirroring the display horizontally */
      private static readonly byte[] CMD_COMSCANDIR            = { 0xC8 };             /* Set the COM scan direction to inverse, which flips the screen vertically        */
      private static readonly byte[] CMD_RESETCOLADDR          = { 0x21, 0x00, 0x7F }; /* Reset the column address pointer                         */
      private static readonly byte[] CMD_RESETPAGEADDR         = { 0x22, 0x00, 0x07 }; /* Reset the page address pointer                           */


      private static readonly byte[][] CommandInitSequence = new byte[][] 
         { CMD_DISPLAY_OFF,
           CMD_DISPLAY_CLKDIV,
           CMD_MULTIPLEX,
           CMD_DISPLAY_OFFSET,
           CMD_STARTLINE,
           CMD_CHARGEPUMP_ON,
           CMD_MEMADDRMODE,
           CMD_SEGREMAP,
           CMD_COMSCANDIR,
           CMD_SETCOMPINS,
           CMD_SETCONTRAST,
           CMD_SETPRECHARGE,
           CMD_SETVCOMDETECT,
           CMD_DISPLAYALLON_RESUME,
           CMD_NORM_DISPLAY,
           CMD_DISPLAY_ON };

      /* Initialize GPIO, I2C, and the display 
         The device may not respond to multiple Init calls without being power cycled
         so we allow an optional boolean to excuse failures which is useful while debugging
         without power cycling the display */
      public void InitialiseDriver(bool proceedOnFail = false)
      {
         try
         {
            List<byte> commandString = new List<byte>();

            foreach(byte[] b in CommandInitSequence)
            {
               commandString.AddRange(b);
            }

            DisplaySendCommand(commandString.ToArray());

            Update();
         }
         catch (Exception e)
         {
            Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);

            if (!proceedOnFail)
            {
               throw new Exception("Display Initialization Failed", e);
            }
         }

         displayRefreshTimer = ThreadPoolTimer.CreatePeriodicTimer((s) => { if(RefreshDisplay) Update(); }, TimeSpan.FromMilliseconds(200));
      }

      /* Send graphics data to the screen */
      private void DisplaySendData(byte[] Data)
      {
         byte[] commandBuffer = new byte[Data.Length + 1];
         Data.CopyTo(commandBuffer, 1);
         commandBuffer[0] = 0x40; // display buffer register

         _stream.Write(commandBuffer);
      }

      /* Send commands to the screen */
      private void DisplaySendCommand(byte[] Command)
      {
         byte[] commandBuffer = new byte[Command.Length + 1];
         Command.CopyTo(commandBuffer, 1);
         commandBuffer[0] = 0x00; // control register

         _stream.Write(commandBuffer);
      }

      /* Writes the Display Buffer out to the physical screen for display */
      public void Update()
      {
         //System.Diagnostics.Debug.WriteLine("   WRITING TO DISPLAY START");

         List<byte> commandString = new List<byte>();

         commandString.AddRange(CMD_RESETCOLADDR);    /* Reset the column address pointer back to 0 */
         commandString.AddRange(CMD_RESETPAGEADDR);   /* Reset the page address pointer back to 0   */
         DisplaySendCommand(commandString.ToArray());

         lock (_Lock)
         {
            DisplaySendData(DisplayBuffer);               /* Send the data over i2c                     */
         }
         //System.Diagnostics.Debug.WriteLine("   WRITING TO DISPLAY END");
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="bitmap">In 1BPP format</param>
      /// <param name="w"></param>
      /// <param name="h"></param>
      /// <param name="color">White = ON, Black = OFF, Transparent = Inverted</param>
      public void DrawBitmap(short x, short y, byte[] bitmap, short w, short h, Color color)
      {
         short byteWidth = (short)((w + 7) / 8); // Bitmap scanline pad = whole byte
         byte data = 0;

         lock (_Lock)
         {
            for (short j = 0; j < h; j++, y++)
            {
               for (short i = 0; i < w; i++)
               {
                  if ((i & 7) > 0)
                     data <<= 1;
                  else
                     data = bitmap[j * byteWidth + i / 8];

                  if ((data & 0x80) > 0)
                     DrawPixel((short)(x + i), y, Colors.White);
                  else
                     DrawPixel((short)(x + i), y, Colors.Black);
               }
            }
         }
      }

      public void DrawPixel(short x, short y, Color color)
      {
         if ((x < 0) || (x >= Width) || (y < 0) || (y >= Height))
            return;

         // check rotation, move pixel around if necessary
         switch (Rotation)
         {
            case 1:
               GraphicsHelper.SwapValues(x, y);
               x = (short)(Width - x - 1);
               break;
            case 2:
               x = (short)(Width - x - 1);
               y = (short)(Height - y - 1);
               break;
            case 3:
               GraphicsHelper.SwapValues(x, y);
               y = (short)(Height - y - 1);
               break;
         }

         // x is which column
         if (color == Colors.White)
            DisplayBuffer[x + (y / 8) * Width] = (byte)(DisplayBuffer[x + (y / 8) * Width] | (1 << (y & 7)));
         else if (color == Colors.Black)
            DisplayBuffer[x + (y / 8) * Width] = (byte)(DisplayBuffer[x + (y / 8) * Width] & ~(1 << (y & 7)));
         else if (color == Colors.Transparent)
            DisplayBuffer[x + (y / 8) * Width] = (byte)(DisplayBuffer[x + (y / 8) * Width] ^ (1 << (y & 7)));
      }
   }
}
