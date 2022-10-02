using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;

namespace InteliNotes
{
    class CustomClipboard
    {
        Rect windowRect;
        StrokeCollection strokes;
        StrokeCollection pasteStrokes;
        List<(UIElement, Point)> images;
        List<(UIElement, Point)> newImages;
        public CustomClipboard(Rect selectedRect, StrokeCollection strokes, List<(UIElement, Point)> images)
        {
            this.images = images;
            this.strokes = strokes;
            windowRect = selectedRect;
        }

        public void Paste(InkCanvas canvas, Point position)
        {
            pasteStrokes = strokes.Clone();
            //canvas.Strokes.Add(CopyAndMoveStrokes(strokes, position.X - windowRect.Left, position.Y - windowRect.Top));
            canvas.Strokes.Add(pasteStrokes);
            pasteStrokes.Move(position.X - windowRect.Left, position.Y - windowRect.Top);
            List<UIElement> newElems = new List<UIElement>();
            newImages = new List<(UIElement, Point)>();
            foreach(var img in images)
            {
                Image pic = new Image();
                pic.Source = (img.Item1 as Image).Source.Clone();
                newElems.Add(pic);
                //canvas.Children.Add(pic);
                //InkCanvas.SetTop(pic, position.Y + img.Value.Y - windowRect.Top);
                //InkCanvas.SetLeft(pic, position.X + img.Value.X - windowRect.Left);
                Point pt = new Point()
                {
                    X = position.X + img.Item2.X - windowRect.Left,
                    Y = position.Y + img.Item2.Y - windowRect.Top
                };
                canvas.AddUIElementAtPosition(pic, pt);
                newImages.Add((pic, pt));
            }
            canvas.Select(pasteStrokes, newElems);
        }

        public StrokeCollection GetStrokes()
        {
            return pasteStrokes == null ? strokes : pasteStrokes;
        }

        public List<(UIElement, Point)> GetElements()
        {
            return newImages == null ? images : newImages;
        }

        private static StrokeCollection CopyAndMoveStrokes(StrokeCollection strokes, double x, double y)
        {
            StrokeCollection moved = new StrokeCollection();
            for(int i = 0; i < strokes.Count; ++i)
            {
                Stroke str = strokes[i];
                StylusPointCollection stylusPoints = new StylusPointCollection();
                List<StylusPoint> prevList = str.StylusPoints.ToList();
                for (int j = 0; j < prevList.Count; ++j)
                {
                    StylusPoint pt = prevList[j];
                    pt.X += x;
                    pt.Y += y;
                    stylusPoints.Add(pt);
                }
                Stroke newStroke = new Stroke(stylusPoints);
                newStroke.DrawingAttributes = str.DrawingAttributes.Clone();
                moved.Add(newStroke);
            }
            return moved;
        }
    }
}
