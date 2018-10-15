using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
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
    public sealed partial class DrawCanvas : ContentDialog, IXmlFunction
    {
        private enum DirectionInfo
        {
            Left,
            Right,
            Top,
            Bottom
        };
        private TextBlock coordsText;
        private Point lastLineEndPoint;
        private Line nextLine;
        private Line editingLine;

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

        public Color LineColor { get; set; } = Colors.Red;

        public bool DisplayCoordinates { get; set; } = false;

        public DrawCanvas()
        {
            this.InitializeComponent();

            editingLine = null;

            CapturedPoints = new List<Line>();
            coordsText = new TextBlock()
            {
                FontSize = 8,
            };

            grid.Children.Add(coordsText);

            if (DisplayCoordinates)
            {
                PointerMoved += DrawCanvas_PointerMoved;
            }
        }

        private void AddLine(Line line)
        {
            CapturedPoints.Add(line);
            mouseDraw.Children.Add(line);
        }

        private void RemoveLine(Line line)
        {
            CapturedPoints.Remove(line);
            mouseDraw.Children.Remove(line);
        }

        private Line GetNextLine(Line currentLine)
        {
            Line next = null;

            if (CapturedPoints.IndexOf(currentLine) < (CapturedPoints.Count - 1))
            {
                next = CapturedPoints[CapturedPoints.IndexOf(currentLine) + 1];
            }

            return next;
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
                    RemoveLine(l);
                }
                else if ((p.X >= l.X1) && (p.X <= l.X2))
                {
                    //RPUGLIESE - Should NOT set the mouse X position, MUST use RESOLUTION property
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

        private void ClearAllCapturedLines()
        {
            mouseDraw.Children.Clear();
            CapturedPoints.Clear();
        }
        private void UpdateEndPoint(Line l)
        {
            lastLineEndPoint.X = l.X2;
            lastLineEndPoint.Y = l.Y2;
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

        private void mouseDraw_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Point p = e.GetCurrentPoint(mouseDraw).Position;
            double distance_X;

            if (e.Pointer.IsInContact == true)
            {
                if (editingLine != null)
                    distance_X = p.X - editingLine.X1;
                else
                    distance_X = p.X - lastLineEndPoint.X;

                //Only allow MOVING forward
                if (distance_X >= Resolution)
                {
                    if (editingLine != null)
                    {
                        //Set the current LINE to the new Y position
                        editingLine.Y2 = p.Y;

                        //Update the Last End Point to the current editing LINE
                        lastLineEndPoint.X = editingLine.X2;
                        lastLineEndPoint.Y = editingLine.Y2;

                        //Check if we are at the END of the curve, stop EDITING
                        if (GetNextLine(editingLine) == null)
                        {
                            editingLine = null;
                        }
                        else
                        {
                            nextLine.Y1 = p.Y;

                            //Move the editing LINE to the NEXT LINE
                            editingLine = nextLine;

                            //Get the NEXT LINE
                            nextLine = GetNextLine(editingLine);
                        }
                    }
                    else
                    {
                        Line line = new Line();

                        line.Stroke = new SolidColorBrush(LineColor);
                        line.X1 = lastLineEndPoint.X;
                        line.Y1 = lastLineEndPoint.Y;
                        line.X2 = lastLineEndPoint.X + Resolution;
                        line.Y2 = p.Y;

                        lastLineEndPoint.X = line.X2;
                        lastLineEndPoint.Y = line.Y2;

                        AddLine(line);
                    }
                }
            }
        }

        private void mouseDraw_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Point p = e.GetCurrentPoint(mouseDraw).Position;
            double distance_X;

            if (e.Pointer.IsInContact)
            {
                Line[] currentLines = new Line[CapturedPoints.Count];

                CapturedPoints.CopyTo(currentLines);

                foreach (Line l in currentLines)
                {
                    if ((p.X >= l.X1) && (p.X <= l.X2))
                    {
                        //Ensure they are not editing the LAST line
                        if (CapturedPoints.Last() != l)
                        {
                            int index = CapturedPoints.IndexOf(l);

                            editingLine = l;
                            nextLine = CapturedPoints[index + 1];
                            return;
                        }
                        else
                        {
                            RemoveLine(l);

                            lastLineEndPoint.X = CapturedPoints.Last().X2;
                            lastLineEndPoint.Y = CapturedPoints.Last().Y2;
                        }
                    }
                }

                if (mouseDraw.Children.Count > 0)
                {
                    distance_X = p.X - lastLineEndPoint.X;

                    while (distance_X > Resolution)
                    {
                        Line line = new Line();

                        line.Stroke = new SolidColorBrush(LineColor);
                        line.X1 = lastLineEndPoint.X;
                        line.Y1 = lastLineEndPoint.Y;
                        line.X2 = lastLineEndPoint.X + Resolution;
                        line.Y2 = p.Y;

                        lastLineEndPoint.X = line.X2;
                        lastLineEndPoint.Y = line.Y2;

                        distance_X -= Resolution;

                        AddLine(line);
                    }
                }
            }
        }

        private void mouseDraw_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            editingLine = null;

            lastLineEndPoint.X = CapturedPoints.Last().X2;
            lastLineEndPoint.Y = CapturedPoints.Last().Y2;
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

                            line.Stroke = new SolidColorBrush(LineColor);
                            line.X1 = lastLineEndPoint.X;
                            line.Y1 = lastLineEndPoint.Y;
                            line.X2 = lastLineEndPoint.X + Resolution;
                            line.Y2 = p.Y;

                            lastLineEndPoint.X = line.X2;
                            lastLineEndPoint.Y = line.Y2;

                            distance_X -= Resolution;

                            AddLine(line);
                        }
                    }
                }
            }
        }

        private void mouseDraw_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Point p = e.GetCurrentPoint(mouseDraw).Position;
            DirectionInfo? exitDirection = null;

            if (e.Pointer.IsInContact == true)
            {
                //Get the EXIT direction
                if (p.X >= XMax - Resolution)
                    exitDirection = DirectionInfo.Right;
                else if (p.Y >= YMax - Resolution)
                    exitDirection = DirectionInfo.Bottom;
                else if (p.Y <= Resolution)
                    exitDirection = DirectionInfo.Top;

                if (editingLine != null)
                {
                    editingLine.Y2 = p.Y;

                    UpdateEndPoint(editingLine);

                    editingLine = null;
                }
                else
                {
                    Line line = new Line();

                    line.Stroke = new SolidColorBrush(LineColor);
                    line.X1 = lastLineEndPoint.X;
                    line.Y1 = lastLineEndPoint.Y;

                    //EXIT from the RIGHT
                    if (exitDirection == DirectionInfo.Right)
                    {
                        line.X2 = XMax;
                        line.Y2 = p.Y;
                    }
                    //EXIT from the BOTTOM
                    else if (exitDirection == DirectionInfo.Bottom)
                    {
                        line.X2 = lastLineEndPoint.X + Resolution;
                        line.Y2 = YMax;
                    }
                    //EXIT from the TOP
                    else if (exitDirection == DirectionInfo.Top)
                    {
                        line.X2 = lastLineEndPoint.X + Resolution;
                        line.Y2 = 0;
                    }

                    if (exitDirection != null)
                    {
                        UpdateEndPoint(line);

                        AddLine(line);
                    }
                }
            }
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new Exception("Deprecated");
        }

        public void ReadXML(XElement element)
        {
            String customData = Convert.ToString(element.Attribute("CustomLevel").Value);

            // ?<x1>\d+(?:\.\d+)?)\s+
            // ?<x1> = GroupName
            // \d+ = Match all digits (any length)
            // ( = capture group (open)
            //   ?: = non-capturing group
            //      \. = Match '.' character
            //      \d+ = Match all digits (any length)
            // ) = capture group (close)
            // ? = Match 0 or 1 of the above
            // \s+ = Match all whitespaces (any length)
            Regex line = new Regex(@"(?<x1>\d+(?:\.\d+)?)\s+(?<x2>\d+(?:\.\d+)?)\s+(?<y1>\d+(?:\.\d+)?)\s+(?<y2>\d+(?:\.\d+)?)", RegexOptions.Compiled);
            MatchCollection match = line.Matches(customData);

            foreach (Match m in match)
            {
                Line l = new Line();

                l.Stroke = new SolidColorBrush(Colors.Red);
                l.X1 = Convert.ToDouble(m.Groups["x1"].Value);
                l.X2 = Convert.ToDouble(m.Groups["x2"].Value);
                l.Y1 = Convert.ToDouble(m.Groups["y1"].Value);
                l.Y2 = Convert.ToDouble(m.Groups["y2"].Value);

                AddLine(l);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            String data = null;

            if (CapturedPoints != null)
            {
                foreach (Line l in CapturedPoints)
                {
                    data = String.Join(" ", data, l.X1.ToString("0.0000"), l.X2.ToString("0.0000"), l.Y1.ToString("0.0000"), l.Y2.ToString("0.0000"));
                }

                if (data != null)
                {
                    data = data.TrimStart(' ');
                }
                writer.WriteAttributeString("CustomLevel", data);
            }
        }
    }
}
