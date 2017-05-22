﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloweenControllerRPi.Device.Drives
{
    // column major, little endian.
    class DisplayImage
    {
        public readonly UInt32 ImageWidthPx;
        public readonly UInt32 ImageHeightBytes;
        public readonly byte[] ImageData;

        public DisplayImage(UInt32 imageHeightBytes, byte[] imageData)
        {
            ImageWidthPx = (UInt32)imageData.Length / imageHeightBytes;
            ImageHeightBytes = imageHeightBytes;
            ImageData = imageData;
        }
    }

    // Images were generated with http://dotmatrixtool.com
    // Find the source at https://github.com/stefangordon/dotmatrixtool
    // Column Major, Little Endian, for 2 byte tall images use "16px" height in dotmatrixtool.
    static class DisplayImages
    {
        public static DisplayImage Connected = new DisplayImage(2, new byte[]
           { 0x00, 0x00, 0x30, 0x00, 0x30, 0x00, 0x18, 0x01, 0x98, 0x01, 0xcc, 0x08, 0xcc, 0x0c, 0xcc, 0x6c, 0xcc, 0x6c, 0xcc, 0x0c, 0xcc, 0x08, 0x98, 0x01, 0x18, 0x01, 0x30, 0x00, 0x30, 0x00, 0x00, 0x00 } );

        public static DisplayImage ClockUp = new DisplayImage(2, new byte[]
          { 0x10, 0x0e, 0x98, 0x31, 0x9c, 0x20, 0x5e, 0x40, 0x5f, 0x47, 0x5e, 0x44, 0x9c, 0x24, 0x98, 0x31, 0x10, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

        public static DisplayImage ClockDown = new DisplayImage(2, new byte[]
          { 0x01, 0x0e, 0x83, 0x31, 0x87, 0x20, 0x4f, 0x40, 0x5f, 0x47, 0x4f, 0x44, 0x87, 0x24, 0x83, 0x31, 0x01, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
    }
}
