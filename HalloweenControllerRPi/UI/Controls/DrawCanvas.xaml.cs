using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace HalloweenControllerRPi.UI.Controls
{
   public sealed partial class DrawCanvas : ContentDialog
   {
      PointerPoint currentPoint;

      public DrawCanvas()
      {
         this.InitializeComponent();
      }


      private void ContentDialog_DoneClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
      {
      }

      private void ContentDialog_CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
      {
      }

      private void mouseDraw_PointerMoved(object sender, PointerRoutedEventArgs e)
      {
         if (e.Pointer.IsInContact == true)
         {
            if (e.GetCurrentPoint(mouseDraw).Position.X > currentPoint.Position.X)
            {
               Line line = new Line();

               line.Stroke = new SolidColorBrush(Colors.Black);
               line.X1 = currentPoint.Position.X;
               line.Y1 = currentPoint.Position.Y;
               line.X2 = e.GetCurrentPoint(mouseDraw).Position.X;
               line.Y2 = e.GetCurrentPoint(mouseDraw).Position.Y;

               currentPoint = e.GetCurrentPoint(mouseDraw);

               mouseDraw.Children.Add(line);
            }
         }
      }

      private void mouseDraw_PointerPressed(object sender, PointerRoutedEventArgs e)
      {
         currentPoint = e.GetCurrentPoint(mouseDraw);
      }
   }
}
