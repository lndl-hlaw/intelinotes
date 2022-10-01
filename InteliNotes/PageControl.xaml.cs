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
            };
        }

        public PageControl(Notebook nt, int w, int h): this(nt)
        {
            Width = w;
            Height = h;
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
            notebook.lastClickedPage = this.DrawingCanvas;
        }

        private void DrawingCanvas_SelectionChanged(object sender, EventArgs e)
        {
        }

        private void DrawingCanvas_SelectionMoving(object sender, InkCanvasSelectionEditingEventArgs e)
        {
            Rect rect = e.NewRectangle;
            Rect prevRect = e.OldRectangle;
            var strokes = this.DrawingCanvas.GetSelectedStrokes();
            var elems = this.DrawingCanvas.GetSelectedElements();
            if(this != notebook.pages.First() && rect.Top < -1*rect.Height/2)
            {
                Dictionary<UIElement, Point> im = new Dictionary<UIElement, Point>();
                for (int i = 0; i < elems.Count; ++i)
                {
                    UIElement image = elems[i];
                    im[image] = new Point() { X = InkCanvas.GetLeft(image), Y = InkCanvas.GetTop(image) };
                    this.DrawingCanvas.Children.Remove(image);
                }
                CustomClipboard clip = new CustomClipboard(prevRect, strokes, im);
                this.DrawingCanvas.Strokes.Remove(strokes);
                this.DrawingCanvas.Select(null, null);
                InkCanvas upper = notebook.pages[notebook.pages.IndexOf(this) - 1].DrawingCanvas;
                clip.Paste(upper, new Point() { X = rect.X, Y = Math.Min(upper.ActualHeight - prevRect.Height - 30, 
                    upper.ActualHeight + rect.Top - this.DrawingCanvas.Margin.Top) });
            }
            else if(this != notebook.pages.Last() && rect.Bottom > this.DrawingCanvas.ActualHeight + rect.Height/2)
            {
                Dictionary<UIElement, Point> im = new Dictionary<UIElement, Point>();
                for(int i = 0; i < elems.Count; ++i)
                {
                    UIElement image = elems[i];
                    im[image] = new Point() { X = InkCanvas.GetLeft(image), Y = InkCanvas.GetTop(image) };
                    this.DrawingCanvas.Children.Remove(image);
                }
                CustomClipboard clip = new CustomClipboard(prevRect, strokes, im);
                this.DrawingCanvas.Strokes.Remove(strokes);
                this.DrawingCanvas.Select(null, null);
                InkCanvas upper = notebook.pages[notebook.pages.IndexOf(this) + 1].DrawingCanvas;
                clip.Paste(upper, new Point() { X = rect.X, Y = Math.Max(30, rect.Top - this.DrawingCanvas.ActualHeight) });
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
    }
}
