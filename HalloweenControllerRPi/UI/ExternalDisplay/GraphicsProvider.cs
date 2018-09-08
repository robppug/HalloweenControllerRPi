using HalloweenControllerRPi.Device.Controllers.Providers;
using HalloweenControllerRPi.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace HalloweenControllerRPi.UI.ExternalDisplay
{
   public static class GraphicsHelper
   {
      public static void SwapValues<Type>(Type a, Type b)
      {
         Type t = a;
         a = b;
         b = t;
      }
   }

   public class GraphicsProvider
   {
      private IDriverDisplayProvider _device;
      private Canvas _activeCanvas;

      private DispatcherTimer _refreshTimer;

      public Canvas ActiveCanvas { get; set; }

      public Image OutputImage { get; internal set; }

      public MenuControl Menu { get; set; }

      public GraphicsProvider(IDriverDisplayProvider dev)
      {
         _device = dev;
      }
    
      public async Task Draw()
      {
         //System.Diagnostics.Debug.WriteLine("DISPLAY UPDATE START: \n     " + Environment.StackTrace);

         //ActiveCanvas.UseLayoutRounding = true;
         ActiveCanvas.UpdateLayout();
         ActiveCanvas.Measure(ActiveCanvas.DesiredSize);
         ActiveCanvas.Arrange(new Rect(new Point(0, 0), ActiveCanvas.DesiredSize));

         // Create a render bitmap and push the surface to it
         RenderTargetBitmap renderBitmap = new RenderTargetBitmap();
         await renderBitmap.RenderAsync(ActiveCanvas, (int)ActiveCanvas.DesiredSize.Width, (int)ActiveCanvas.DesiredSize.Height);

         DataReader bitmapStream = DataReader.FromBuffer(await renderBitmap.GetPixelsAsync());

         if (_device != null)
         {
            byte[] BGRA8 = new byte[4];
            byte[] pixelBuffer_1BPP = new byte[(int)(renderBitmap.PixelWidth * renderBitmap.PixelHeight) / 8];

            using (bitmapStream)
            {
               while (bitmapStream.UnconsumedBufferLength > 0)
               {
                  uint index = (uint)(((renderBitmap.PixelWidth * renderBitmap.PixelHeight * 4) - bitmapStream.UnconsumedBufferLength) / 32);

                  for (int bit = 0; bit < 8; bit++)
                  {
                     bitmapStream.ReadBytes(BGRA8);

                     pixelBuffer_1BPP[index] |= (byte)((byte)(((BGRA8[0] & 0x80) | (BGRA8[1] & 0x80) | (BGRA8[2] & 0x80)) == 0x80 ? 1 : 0) << (7 - bit));
                  }
               }

               _device.RefreshDisplay = false;

               _device.DrawBitmap(0, 0, pixelBuffer_1BPP, (short)renderBitmap.PixelWidth, (short)renderBitmap.PixelHeight, Colors.White);

               _device.RefreshDisplay = true;
            }
         }

         //System.Diagnostics.Debug.WriteLine("DISPLAY UPDATE END");

         var stream = new InMemoryRandomAccessStream();
         var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);

         IBuffer buffer = await renderBitmap.GetPixelsAsync();

         byte[] pixelBuffer = buffer.ToArray(0, (int)(renderBitmap.PixelWidth * renderBitmap.PixelHeight * 4));

         encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)ActiveCanvas.DesiredSize.Width, (uint)ActiveCanvas.DesiredSize.Height, 96, 96, pixelBuffer);

         await encoder.FlushAsync();
         stream.Seek(0);

         WriteableBitmap bmp = new WriteableBitmap(128, 64);
         bmp.SetSource(stream);

         OutputImage.Source = bmp;
      }

      public void SuspendLayout()
      {
         if (_device != null)
            _device.RefreshDisplay = false;
      }

      public void ResumeLayout()
      {
         if (_device != null)
            _device.RefreshDisplay = true;
      }
   }
}
