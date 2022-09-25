using HalloweenControllerRPi.Device.Drivers;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml.Shapes;

namespace HalloweenControllerRPi.Device.Controllers.Providers
{
    public interface IDriverDisplayProvider
    {
        int Width { get; set; }
        int Height { get; set; }
        bool RefreshDisplay { get; set; }

        void DrawPixel(short x, short y, Color color);
        void DrawBitmap(short x, short y, byte[] bitmap, short w, short h, Color color);
        void Update();
    }
}