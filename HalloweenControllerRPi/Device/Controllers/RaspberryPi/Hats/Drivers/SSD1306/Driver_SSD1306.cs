using HalloweenControllerRPi.Device.Controllers.RaspberryPi.Hats.BusDevices;
using HalloweenControllerRPi.Device.Drives;
using System;
using System.Diagnostics;
using Windows.Devices.I2c;

namespace HalloweenControllerRPi.Device.Drivers
{
   class Driver_SSD1306 : II2CBusDevice, IDriverProvider, IDriverDisplayProvider
   {
      /* This driver is intended to be used with Driver_SSD1306 based OLED displays connected via I2c */

      private const UInt32 SCREEN_WIDTH_PX = 128;                                               /* Number of horizontal pixels on the display */
      private const UInt32 SCREEN_HEIGHT_PX = 64;                                               /* Number of vertical pixels on the display   */
      private const UInt32 SCREEN_HEIGHT_PAGES = SCREEN_HEIGHT_PX / 8;                          /* The vertical pixels on this display are arranged into 'pages' of 8 pixels each */
      private byte[,] DisplayBuffer = new byte[SCREEN_WIDTH_PX, SCREEN_HEIGHT_PAGES];           /* A local buffer we use to store graphics data for the screen                    */
      private byte[] SerializedDisplayBuffer = new byte[SCREEN_WIDTH_PX * SCREEN_HEIGHT_PAGES]; /* A temporary buffer used to prepare graphics data for sending over i2c          */
      
      private I2cDevice m_i2cDevice;

      public bool Initialised { get; private set; }

      public I2cDevice _i2cDevice
      {
         get { return m_i2cDevice; }
      }

      public Driver_SSD1306()
      {
         Initialised = false;
      }

      public void Open(I2cDevice i2cDevice)
      {
         if (Initialised == false)
         {
            m_i2cDevice = i2cDevice;
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
            m_i2cDevice = null;

            Initialised = false;
         }
      }


      /* Display commands. See the datasheet for details on commands: http://www.adafruit.com/datasheets/Driver_SSD1306.pdf                      */
      private static readonly byte[] CMD_DISPLAY_OFF = { 0xAE };              /* Turns the display off                                    */
      private static readonly byte[] CMD_DISPLAY_ON = { 0xAF };               /* Turns the display on                                     */
      private static readonly byte[] CMD_DISPLAYALLON_RESUME = { 0xA4 };
      private static readonly byte[] CMD_NORM_DISPLAY = { 0xA6 };
      private static readonly byte[] CMD_DISPLAY_CLKDIV = { 0xD5, 0x80 };
      private static readonly byte[] CMD_MULTIPLEX = { 0xA8, 0x3F };          /* 1F = 32H, 3F = 64H */
      private static readonly byte[] CMD_DISPLAY_OFFSET = { 0xD3, 0x00 };
      private static readonly byte[] CMD_STARTLINE = { 0x40 };
      private static readonly byte[] CMD_SETPRECHARGE = { 0xD9, 0xF1 };
      private static readonly byte[] CMD_SETVCOMDETECT = { 0xDB, 0x40 };
      private static readonly byte[] CMD_SETCOMPINS = { 0xDA, 0x12 };         /* 0x02 = 32H, 0x12 = 64H */
      private static readonly byte[] CMD_SETCONTRAST = { 0x81, 0xCF };
      private static readonly byte[] CMD_CHARGEPUMP_ON = { 0x8D, 0x14 };      /* Turn on internal charge pump to supply power to display  */
      private static readonly byte[] CMD_MEMADDRMODE = { 0x20, 0x00 };        /* Horizontal memory mode                                   */
      private static readonly byte[] CMD_SEGREMAP = { 0xA1 };                 /* Remaps the segments, which has the effect of mirroring the display horizontally */
      private static readonly byte[] CMD_COMSCANDIR = { 0xC8 };               /* Set the COM scan direction to inverse, which flips the screen vertically        */
      private static readonly byte[] CMD_RESETCOLADDR = { 0x21, 0x00, 0x7F }; /* Reset the column address pointer                         */
      private static readonly byte[] CMD_RESETPAGEADDR = { 0x22, 0x00, 0x07 };/* Reset the page address pointer                           */

      /* Initialize GPIO, I2C, and the display 
         The device may not respond to multiple Init calls without being power cycled
         so we allow an optional boolean to excuse failures which is useful while debugging
         without power cycling the display */
      public void InitialiseDriver(bool proceedOnFail = false)
      {
         try
         {
            DisplaySendCommand(CMD_DISPLAY_OFF);
            DisplaySendCommand(CMD_DISPLAY_CLKDIV);
            DisplaySendCommand(CMD_MULTIPLEX);
            DisplaySendCommand(CMD_DISPLAY_OFFSET);
            DisplaySendCommand(CMD_STARTLINE);
            DisplaySendCommand(CMD_CHARGEPUMP_ON);  /* Turn on the internal charge pump to provide power to the screen          */
            DisplaySendCommand(CMD_MEMADDRMODE);    /* Set the addressing mode to "horizontal"                                  */
            DisplaySendCommand(CMD_SEGREMAP);       /* Flip the display horizontally, so it's easier to read on the breadboard  */
            DisplaySendCommand(CMD_COMSCANDIR);     /* Flip the display vertically, so it's easier to read on the breadboard    */
            DisplaySendCommand(CMD_SETCOMPINS);
            DisplaySendCommand(CMD_SETCONTRAST);
            DisplaySendCommand(CMD_SETPRECHARGE);
            DisplaySendCommand(CMD_SETVCOMDETECT);
            DisplaySendCommand(CMD_DISPLAYALLON_RESUME);
            DisplaySendCommand(CMD_NORM_DISPLAY);
            DisplaySendCommand(CMD_DISPLAY_ON);     /* Turn the display on                                                      */
         }
         catch (Exception e)
         {
            Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);

            if (!proceedOnFail)
            {
               throw new Exception("Display Initialization Failed", e);
            }
         }
      }

      /* Send graphics data to the screen */
      private void DisplaySendData(byte[] Data)
      {
         byte[] commandBuffer = new byte[Data.Length + 1];
         Data.CopyTo(commandBuffer, 1);
         commandBuffer[0] = 0x40; // display buffer register

         m_i2cDevice.Write(commandBuffer);
      }

      /* Send commands to the screen */
      private void DisplaySendCommand(byte[] Command)
      {
         byte[] commandBuffer = new byte[Command.Length + 1];
         Command.CopyTo(commandBuffer, 1);
         commandBuffer[0] = 0x00; // control register

         m_i2cDevice.Write(commandBuffer);
      }

      /* Writes the Display Buffer out to the physical screen for display */
      public void Update()
      {
         int Index = 0;
         /* We convert our 2-dimensional array into a serialized string of bytes that will be sent out to the display */
         for (int PageY = 0; PageY < SCREEN_HEIGHT_PAGES; PageY++)
         {
            for (int PixelX = 0; PixelX < SCREEN_WIDTH_PX; PixelX++)
            {
               SerializedDisplayBuffer[Index] = DisplayBuffer[PixelX, PageY];
               Index++;
            }
         }

         /* Write the data out to the screen */
         DisplaySendCommand(CMD_RESETCOLADDR);         /* Reset the column address pointer back to 0 */
         DisplaySendCommand(CMD_RESETPAGEADDR);        /* Reset the page address pointer back to 0   */
         DisplaySendData(SerializedDisplayBuffer);     /* Send the data over i2c                     */
      }

      /* 
         * NAME:        WriteLineDisplayBuf
         * DESCRIPTION: Writes a string to the display screen buffer (DisplayUpdate() needs to be called subsequently to output the buffer to the screen)
         * INPUTS:
         *
         * Line:      The string we want to render. In this sample, special characters like tabs and newlines are not supported.
         * Col:       The horizontal column we want to start drawing at. This is equivalent to the 'X' axis pixel position.
         * Row:       The vertical row we want to write to. The screen is divided up into 4 rows of 16 pixels each, so valid values for Row are 0,1,2,3.
         *
         * RETURN VALUE:
         * None. We simply return when we encounter characters that are out-of-bounds or aren't available in the font.
         */
      public void WriteLine(String Line, UInt32 Col, UInt32 Row)
      {
         UInt32 CharWidth = 0;
         foreach (Char Character in Line)
         {
            CharWidth = WriteCharDisplayBuf(Character, Col, Row);
            Col += CharWidth;   /* Increment the column so we can track where to write the next character   */
            if (CharWidth == 0) /* Quit if we encounter a character that couldn't be printed                */
            {
               return;
            }
         }
      }

      /* 
         * NAME:        WriteCharDisplayBuf
         * DESCRIPTION: Writes one character to the display screen buffer (DisplayUpdate() needs to be called subsequently to output the buffer to the screen)
         * INPUTS:
         *
         * Character: The character we want to draw. In this sample, special characters like tabs and newlines are not supported.
         * Col:       The horizontal column we want to start drawing at. This is equivalent to the 'X' axis pixel position.
         * Row:       The vertical row we want to write to. The screen is divided up into 4 rows of 16 pixels each, so valid values for Row are 0,1,2,3.
         *
         * RETURN VALUE:
         * We return the number of horizontal pixels used. This value is 0 if Row/Col are out-of-bounds, or if the character isn't available in the font.
         */
      public UInt32 WriteCharDisplayBuf(Char Chr, UInt32 Col, UInt32 Row)
      {
         /* Check that we were able to find the font corresponding to our character */
         FontCharacterDescriptor CharDescriptor = DisplayFontTable.GetCharacterDescriptor(Chr);
         if (CharDescriptor == null)
         {
            return 0;
         }

         /* Make sure we're drawing within the boundaries of the screen buffer */
         UInt32 MaxRowValue = (SCREEN_HEIGHT_PAGES / DisplayFontTable.FontHeightBytes) - 1;
         UInt32 MaxColValue = SCREEN_WIDTH_PX;
         if (Row > MaxRowValue)
         {
            return 0;
         }
         if ((Col + CharDescriptor.CharacterWidthPx + DisplayFontTable.FontCharSpacing) > MaxColValue)
         {
            return 0;
         }

         UInt32 CharDataIndex = 0;
         UInt32 StartPage = Row * 2;                                              //0
         UInt32 EndPage = StartPage + CharDescriptor.CharacterHeightBytes;        //2
         UInt32 StartCol = Col;
         UInt32 EndCol = StartCol + CharDescriptor.CharacterWidthPx;
         UInt32 CurrentPage = 0;
         UInt32 CurrentCol = 0;

         /* Copy the character image into the display buffer */
         for (CurrentPage = StartPage; CurrentPage < EndPage; CurrentPage++)
         {
            for (CurrentCol = StartCol; CurrentCol < EndCol; CurrentCol++)
            {
               DisplayBuffer[CurrentCol, CurrentPage] = CharDescriptor.CharacterData[CharDataIndex];
               CharDataIndex++;
            }
         }

         /* Pad blank spaces to the right of the character so there exists space between adjacent characters */
         for (CurrentPage = StartPage; CurrentPage < EndPage; CurrentPage++)
         {
            for (; CurrentCol < EndCol + DisplayFontTable.FontCharSpacing; CurrentCol++)
            {
               DisplayBuffer[CurrentCol, CurrentPage] = 0x00;
            }
         }

         /* Return the number of horizontal pixels used by the character */
         return CurrentCol - StartCol;
      }

      public UInt32 WriteImage(DisplayImage img, UInt32 Col, UInt32 Row)
      {
         /* Make sure we're drawing within the boundaries of the screen buffer */
         UInt32 MaxRowValue = (SCREEN_HEIGHT_PAGES / img.ImageHeightBytes) - 1;
         UInt32 MaxColValue = SCREEN_WIDTH_PX;
         if (Row > MaxRowValue)
         {
            return 0;
         }

         if ((Col + img.ImageWidthPx + DisplayFontTable.FontCharSpacing) > MaxColValue)
         {
            return 0;
         }

         UInt32 CharDataIndex = 0;
         UInt32 StartPage = Row * 2;                                              //0
         UInt32 EndPage = StartPage + img.ImageHeightBytes;        //2
         UInt32 StartCol = Col;
         UInt32 EndCol = StartCol + img.ImageWidthPx;
         UInt32 CurrentPage = 0;
         UInt32 CurrentCol = 0;

         /* Copy the character image into the display buffer */
         for (CurrentCol = StartCol; CurrentCol < EndCol; CurrentCol++)
         {
            for (CurrentPage = StartPage; CurrentPage < EndPage; CurrentPage++)
            {
               DisplayBuffer[CurrentCol, CurrentPage] = img.ImageData[CharDataIndex];
               CharDataIndex++;
            }
         }

         /* Return the number of horizontal pixels used by the character */
         return CurrentCol - StartCol;
      }

      /* Sets all pixels in the screen buffer to 0 */
      public void ClearDisplayBuf()
      {
         Array.Clear(DisplayBuffer, 0, DisplayBuffer.Length);
      }
   }
}
