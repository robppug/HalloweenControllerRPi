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
      private TextBlock coordsText;
      private Point lastLineEndPoint;

      public double Resolution { get; set; } = 10.0;

      public List<Line> CapturedPoints { get; }
      public double YMax
      {
         get { return mouseDraw.ActualHeight; }
      }

      public double XMax
      {
         get { return mouseDraw.ActualWidth; }
      }


      public DrawCanvas()
      {
         this.InitializeComponent();

         CapturedPoints = new List<Line>();
         coordsText = new TextBlock()
         {
            FontSize = 8,
         };

         grid.Children.Add(coordsText);
         PointerMoved += DrawCanvas_PointerMoved;
      }

      private void DrawCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
      {
         Point p = e.GetCurrentPoint(mouseDraw).Position;
         int x, y;

         x = (int)p.X;
         y = (int)p.Y;

         if (x > (int)XMax)
            x = (int)XMax;
         else if (x < 0)
            x = 0;
         if (y > (int)YMax)
            y = (int)YMax;
         else if (y < 0)
            y = 0;

         coordsText.Text = "(" + x + "," + ((int)YMax - y) + ")";

         Canvas.SetLeft(coordsText, p.X + 30);
         Canvas.SetTop(coordsText, p.Y - 5);
      }

      private bool RemoveAllFromPoint(Point p)
      {
         bool found = false;
         Line[] currentLines = new Line[CapturedPoints.Count];

         CapturedPoints.CopyTo(currentLines);

         foreach (Line l in currentLines)
         {
            if (found)
            {
               RemovePoint(l);
            }
            else if ((p.X >= l.X1) && (p.X <= l.X2))
            {
               l.X2 = p.X;
               l.Y2 = p.Y;

               found = true;
            }
         }

         //Update the LAST line end point to the new LAST line.
         if (CapturedPoints.Count > 0)
         {
            lastLineEndPoint.X = CapturedPoints.Last().X2;
            lastLineEndPoint.Y = CapturedPoints.Last().Y2;
         }

         return found;
      }

      private void AddPoint(Line line)
      {
         //if (line.X1 < 0)
         //   line.X1 = 0;
         //if (line.X2 > XMax)
         //   line.X2 = XMax;
         //if (line.Y1 < 0)
         //   line.Y1 = 0;
         //if (line.Y2 > YMax)
         //   line.Y2 = YMax;

         CapturedPoints.Add(line);
         mouseDraw.Children.Add(line);
      }
      private void RemovePoint(Line line)
      {
         CapturedPoints.Remove(line);
         mouseDraw.Children.Remove(line);
      }

      private void mouseDraw_PointerMoved(object sender, PointerRoutedEventArgs e)
      {
         Point p = e.GetCurrentPoint(mouseDraw).Position;
         double distance_X;

         if (e.Pointer.IsInContact == true)
         {
            distance_X = p.X - lastLineEndPoint.X;

            //Only allow MOVING forward
            if (distance_X >= Resolution)
            {
               Line line = new Line();

               line.Stroke = new SolidColorBrush(Colors.Red);
               line.X1 = lastLineEndPoint.X;
               line.Y1 = lastLineEndPoint.Y;
               line.X2 = lastLineEndPoint.X + Resolution;
               line.Y2 = p.Y;

               lastLineEndPoint.X = line.X2;
               lastLineEndPoint.Y = line.Y2;

               AddPoint(line);
            }
         }
      }

      private void mouseDraw_PointerPressed(object sender, PointerRoutedEventArgs e)
      {
         Point p = e.GetCurrentPoint(mouseDraw).Position;
         double distance_X;

         if (e.Pointer.IsInContact)
         {
            //Check if the new LINE is before existing LINEs
            RemoveAllFromPoint(p);

            if (mouseDraw.Children.Count > 0)
            {
               distance_X = p.X - lastLineEndPoint.X;

               while (distance_X > Resolution)
               {
                  Line line = new Line();

                  line.Stroke = new SolidColorBrush(Colors.Red);
                  line.X1 = lastLineEndPoint.X;
                  line.Y1 = lastLineEndPoint.Y;
                  line.X2 = lastLineEndPoint.X + Resolution;
                  line.Y2 = p.Y;

                  lastLineEndPoint.X = line.X2;
                  lastLineEndPoint.Y = line.Y2;

                  distance_X -= Resolution;

                  AddPoint(line);
               }
            }
         }
      }

      private void mouseDraw_PointerReleased(object sender, PointerRoutedEventArgs e)
      {
      }

      private void mouseDraw_PointerEntered(object sender, PointerRoutedEventArgs e)
      {
         Point p = e.GetCurrentPoint(mouseDraw).Position;
         double distance_X;

         if (e.Pointer.IsInContact == true)
         {
            //Have we ENTERED from the LEFT?
            if (p.X <= Resolution)
            {
               ClearAllCapturedLines();

               lastLineEndPoint = new Point(0, p.Y);
            }
            else
            {
               RemoveAllFromPoint(p);

               if (mouseDraw.Children.Count > 0)
               {
                  distance_X = p.X - lastLineEndPoint.X;

                  while (distance_X > Resolution)
                  {
                     Line line = new Line();

                     line.Stroke = new SolidColorBrush(Colors.Red);
                     line.X1 = lastLineEndPoint.X;
                     line.Y1 = lastLineEndPoint.Y;
                     line.X2 = lastLineEndPoint.X + Resolution;
                     line.Y2 = p.Y;

                     lastLineEndPoint.X = line.X2;
                     lastLineEndPoint.Y = line.Y2;

                     distance_X -= Resolution;

                     AddPoint(line);
                  }
               }
            }
         }
      }

      private void ClearAllCapturedLines()
      {
         mouseDraw.Children.Clear();
         CapturedPoints.Clear();
      }

      private void mouseDraw_PointerExited(object sender, PointerRoutedEventArgs e)
      {
         Point p = e.GetCurrentPoint(mouseDraw).Position;

         if (e.Pointer.IsInContact == true)
         {
            Line line = new Line();

            line.Stroke = new SolidColorBrush(Colors.Red);
            line.X1 = lastLineEndPoint.X;
            line.Y1 = lastLineEndPoint.Y;

            //EXIT from the RIGHT
            if (p.X >= XMax - Resolution)
            {
               line.X2 = XMax;
               line.Y2 = p.Y;

               lastLineEndPoint.X = line.X2;
               lastLineEndPoint.Y = line.Y2;

               AddPoint(line);
            }
            //EXIT from the BOTTOM
            else if (p.Y >= YMax - Resolution)
            {
               line.X2 = lastLineEndPoint.X + Resolution;
               line.Y2 = YMax;

               lastLineEndPoint.X = line.X2;
               lastLineEndPoint.Y = line.Y2;

               AddPoint(line);
            }
            //EXIT from the TOP
            else if (p.Y <= Resolution)
            {
               line.X2 = lastLineEndPoint.X + Resolution;
               line.Y2 = 0;

               lastLineEndPoint.X = line.X2;
               lastLineEndPoint.Y = line.Y2;

               AddPoint(line);
            }
         }
      }
   }
}
