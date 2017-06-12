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
      private PointerPoint currentPoint;
      private TextBlock coordsText;

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

         if (x > (int)mouseDraw.ActualWidth)
            x = (int)mouseDraw.ActualWidth;
         else if (x < 0)
            x = 0;
         if (y > (int)mouseDraw.ActualHeight)
            y = (int)mouseDraw.ActualHeight;
         else if (y < 0)
            y = 0;

         coordsText.Text = "(" + x + "," + ((int)mouseDraw.ActualHeight - y) + ")";

         Canvas.SetLeft(coordsText, p.X + 30);
         Canvas.SetTop(coordsText, p.Y - 5);
      }

      private bool CheckPosition()
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
            else if ((currentPoint.Position.X >= l.X1) && (currentPoint.Position.X <= l.X2))
            {
               l.X2 = currentPoint.Position.X;
               l.Y2 = currentPoint.Position.Y;

               found = true;
            }
         }

         return found;
      }

      private void AddPoint(Line line)
      {
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
         double distance;

         if (e.Pointer.IsInContact == true)
         {
            distance = e.GetCurrentPoint(mouseDraw).Position.X - currentPoint.Position.X;

            if (distance > 0)
            {
               if (distance >= Resolution)
               {
                  Line line = new Line();

                  line.Stroke = new SolidColorBrush(Colors.Red);
                  line.X1 = currentPoint.Position.X;
                  line.Y1 = currentPoint.Position.Y;
                  line.X2 = e.GetCurrentPoint(mouseDraw).Position.X;
                  line.Y2 = e.GetCurrentPoint(mouseDraw).Position.Y;

                  currentPoint = e.GetCurrentPoint(mouseDraw);

                  AddPoint(line);
               }
            }
         }
      }


      private void mouseDraw_PointerPressed(object sender, PointerRoutedEventArgs e)
      {
         bool found = false;
         Line[] currentCurve = new Line[mouseDraw.Children.Count];

         mouseDraw.Children.CopyTo(currentCurve, 0);

         if (e.Pointer.IsInContact)
         {
            currentPoint = e.GetCurrentPoint(mouseDraw);

            found = CheckPosition();

            //This is to prevent GAPS
            if ((found == false) && (mouseDraw.Children.Count != 0))
            {
               Line lastLine = (Line)mouseDraw.Children.Last();

               if (currentPoint.Position.X > lastLine.X2)
               {
                  lastLine.X2 = currentPoint.Position.X;
                  lastLine.Y2 = currentPoint.Position.Y;
               }
            }
         }
      }

      private void mouseDraw_PointerReleased(object sender, PointerRoutedEventArgs e)
      {
      }

      private void mouseDraw_PointerEntered(object sender, PointerRoutedEventArgs e)
      {
         Line line = null;

         if (e.Pointer.IsInContact == true)
         {
            if (e.GetCurrentPoint(mouseDraw).Position.X <= Resolution)
            {
               //Left
               mouseDraw.Children.Clear();
               CapturedPoints.Clear();

               line = new Line();

               line.X1 = 0;
               line.Y1 = e.GetCurrentPoint(mouseDraw).Position.Y;
            }
            else if (e.GetCurrentPoint(mouseDraw).Position.Y >= mouseDraw.ActualHeight - Resolution)
            {
               line = new Line();

               //Bottom
               line.X1 = e.GetCurrentPoint(mouseDraw).Position.X;
               line.Y1 = mouseDraw.ActualHeight;
            }
            else if (e.GetCurrentPoint(mouseDraw).Position.Y <= Resolution)
            {
               line = new Line();

               //Top
               line.X1 = e.GetCurrentPoint(mouseDraw).Position.X;
               line.Y1 = 0;
            }

            if (line != null)
            {
               currentPoint = e.GetCurrentPoint(mouseDraw);

               line.Stroke = new SolidColorBrush(Colors.Red);
               line.X2 = currentPoint.Position.X;
               line.Y2 = currentPoint.Position.Y;

               AddPoint(line);

               bool found = CheckPosition();
            }
         }

         //mouseDraw_PointerPressed(sender, e);
      }

      private void mouseDraw_PointerExited(object sender, PointerRoutedEventArgs e)
      {
         Line line = null;

         if (e.Pointer.IsInContact == true)
         {
            //Detect where the mouse left
            if (e.GetCurrentPoint(mouseDraw).Position.X >= mouseDraw.ActualWidth - Resolution)
            {
               line = new Line();

               //Right
               line.X2 = mouseDraw.ActualWidth;
               line.Y2 = e.GetCurrentPoint(mouseDraw).Position.Y;
            }
            else if (e.GetCurrentPoint(mouseDraw).Position.Y >= mouseDraw.ActualHeight - Resolution)
            {
               line = new Line();

               //Bottom
               line.X2 = e.GetCurrentPoint(mouseDraw).Position.X;
               line.Y2 = mouseDraw.ActualHeight;
            }
            else if (e.GetCurrentPoint(mouseDraw).Position.Y <= Resolution)
            {
               line = new Line();

               //Top
               line.X2 = e.GetCurrentPoint(mouseDraw).Position.X;
               line.Y2 = 0;
            }

            if (line != null)
            {
               line.Stroke = new SolidColorBrush(Colors.Red);
               line.X1 = currentPoint.Position.X;
               line.Y1 = currentPoint.Position.Y;

               currentPoint = e.GetCurrentPoint(mouseDraw);

               AddPoint(line);
            }
         }
      }
   }
}
