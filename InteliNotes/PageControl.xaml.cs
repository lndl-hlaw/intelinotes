using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Ink;

namespace InteliNotes
{
    /// <summary>
    /// Logika interakcji dla klasy PageControl.xaml
    /// </summary>
    public partial class PageControl : UserControl, INotifyPropertyChanged
    {
        Notebook notebook;

        private StrokeCollection PrivStrokes;
        public StrokeCollection DisplayedStrokes
        {
            get { return PrivStrokes; }
            set
            {
                PrivStrokes = value;
                OnPropertyChanged("DisplayedStrokes");
            }
        }

        #region Constructors
        public PageControl(Notebook notebook)
        {
            InitializeComponent();
            this.notebook = notebook;
            DisplayedStrokes = new StrokeCollection();
            Loaded += (s, e) =>
            {
                this.Height = 1.41428 * this.ActualWidth;
                this.MinHeight = this.MaxHeight = this.ActualHeight;
                this.MinWidth = this.MaxWidth = this.ActualWidth;
            };
        }

        #endregion

        #region Control Actions
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Height = 1.41428 * this.ActualWidth;
        }
        private void DrawingCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            notebook.lastClickedPt = e.GetPosition(this.DrawingCanvas);
            notebook.lastClickedPage = this;
        }

        private void DrawingCanvas_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void DrawingCanvas_SelectionMoving(object sender, InkCanvasSelectionEditingEventArgs e)
        {
            Rect rect = e.NewRectangle;
            Rect prevRect = e.OldRectangle;
            Point prevPt = new Point() { X = prevRect.X, Y = prevRect.Y };
            var strokes = this.DrawingCanvas.GetSelectedStrokes();
            var elems = this.DrawingCanvas.GetSelectedElements();
            if(this != notebook.pages.First() && rect.Top < -1*rect.Height/2)
            {
                List<(UIElement, Point)> im = new List<(UIElement, Point)>();
                for (int i = 0; i < elems.Count; ++i)
                {
                    UIElement image = elems[i];
                    im.Add((image, new Point() { X = InkCanvas.GetLeft(image), Y = InkCanvas.GetTop(image) }));
                }
                CustomClipboard clip = new CustomClipboard(prevRect, strokes, im);
                this.DrawingCanvas.RemoveSelection();
                InkCanvas upper = notebook.pages[notebook.pages.IndexOf(this) - 1].DrawingCanvas;
                Point nextPt = new Point()
                {
                    X = rect.X,
                    Y = Math.Min(upper.ActualHeight - prevRect.Height - 30,
                    upper.ActualHeight + rect.Top - this.DrawingCanvas.Margin.Top)
                };

                clip.Paste(upper, nextPt);

                notebook.monitor.AddLastAction(new MoveStrokesAndControlsAction(this.DrawingCanvas, upper, prevPt, nextPt, strokes, elems.ToList()));
            }
            else if(this != notebook.pages.Last() && rect.Bottom > this.DrawingCanvas.ActualHeight + rect.Height/2)
            {
                List<(UIElement, Point)> im = new List<(UIElement, Point)>();
                for (int i = 0; i < elems.Count; ++i)
                {
                    UIElement image = elems[i];
                    im.Add((image, new Point() { X = InkCanvas.GetLeft(image), Y = InkCanvas.GetTop(image) }));
                }
                CustomClipboard clip = new CustomClipboard(prevRect, strokes, im);
                Point nextPt = new Point() { X = rect.X, Y = Math.Max(30, rect.Top - this.DrawingCanvas.ActualHeight) };
                this.DrawingCanvas.RemoveSelection();
                InkCanvas downer = notebook.pages[notebook.pages.IndexOf(this) + 1].DrawingCanvas;
                clip.Paste(downer, nextPt);

                notebook.monitor.AddLastAction(new MoveStrokesAndControlsAction(this.DrawingCanvas, downer, prevPt, nextPt, strokes, elems.ToList()));
            }
            else
            {
                Point nextPt = new Point() { X = rect.X, Y = rect.Y };
                notebook.monitor.AddLastAction(new MoveStrokesAndControlsAction(this.DrawingCanvas, this.DrawingCanvas, prevPt, nextPt, strokes, elems.ToList()));
            }
        }
        #endregion

        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void DrawingCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            notebook.monitor.AddLastAction(new AddStrokeAction(DrawingCanvas, e.Stroke));
        }
    }
}
