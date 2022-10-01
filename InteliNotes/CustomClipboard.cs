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
        Dictionary<UIElement, Point> images;
        public CustomClipboard(Rect selectedRect, StrokeCollection strokes, Dictionary<UIElement, Point> images)
        {
            this.images = images;
            this.strokes = strokes.Clone();
            windowRect = selectedRect;
        }

        public void Paste(InkCanvas canvas, Point position)
        {
            strokes = strokes.Clone();
            //canvas.Strokes.Add(CopyAndMoveStrokes(strokes, position.X - windowRect.Left, position.Y - windowRect.Top));
            canvas.Strokes.Add(strokes);
            MoveStrokes(strokes, position.X - windowRect.Left, position.Y - windowRect.Top);
            List<UIElement> newElems = new List<UIElement>();
            foreach(var img in images)
            {
                Image pic = new Image();
                pic.Source = (img.Key as Image).Source.Clone();
                newElems.Add(pic);
                canvas.Children.Add(pic);
                InkCanvas.SetTop(pic, position.Y + img.Value.Y - windowRect.Top);
                InkCanvas.SetLeft(pic, position.X + img.Value.X - windowRect.Left);
            }
            canvas.Select(strokes, newElems);
        }

        private void MoveStrokes(StrokeCollection strokes, double x, double y)
        {
            Matrix mat = new Matrix();
            mat.Translate(x, y);
            strokes.Transform(mat, false);
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
