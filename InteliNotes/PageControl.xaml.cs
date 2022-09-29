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
    public partial class PageControl : UserControl
    {
        public PageViewModel model;
        Notebook notebook;
        public PageControl(Notebook notebook)
        {
            InitializeComponent();
            model = new PageViewModel();
            DataContext = model;
            this.notebook = notebook;
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

        public PageControl(Notebook nt, Color color, PenStates state, double size): this(nt)
        {
            model.DrawingAttributes.Color = color;
            model.DrawingAttributes.Width = size;
            model.DrawingAttributes.Height = size;
            ChangeState(state);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Height = 1.41428 * this.ActualWidth;
        }

        public void ChangeColor(Color color)
        {
            model.DrawingAttributes.Color = color;
        }
        public void ChangeHighlighter(bool value)
        {
            model.HighlighterMode = value;
        }

        public void ChangeState(PenStates state)
        {
            switch(state)
            {
                case PenStates.Pencil:
                    model.EditingMode = InkCanvasEditingMode.Ink;
                    break;
                case PenStates.EraserStroke:
                    model.EditingMode = InkCanvasEditingMode.EraseByStroke;
                    break;
                case PenStates.EraserPoint:
                    model.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    break;
                case PenStates.Selection:
                    model.EditingMode = InkCanvasEditingMode.Select;
                    break;
            }
        }

        public void ChangeSize(double size)
        {
            //model.DrawingAttributes.Height = size;
            //model.DrawingAttributes.Width = size;
            model.SetNormalAttrSize(size);
        }


        private void DrawingCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            notebook.lastClickedPt = e.GetPosition(this.DrawingCanvas);
            notebook.lastClickedPage = this.DrawingCanvas;
        }
    }

    public class PageViewModel: INotifyPropertyChanged
    {
        #region Display definitions

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

        private InkCanvasEditingMode PrivMode;

        public InkCanvasEditingMode EditingMode
        {
            get {
                if (HighlighterMode == true)
                {
                    return InkCanvasEditingMode.Ink;
                }
                else
                {
                    return PrivMode;
                }
            }
            
            set
            {
                if (PrivMode != value)
                {
                    PrivMode = value;
                    OnPropertyChanged("HighlighterMode");
                    OnPropertyChanged("DrawingAttributes");
                    OnPropertyChanged("EditingMode");
                }
            }
        }



        private DrawingAttributes highlighterDrawingAttr;
        private DrawingAttributes PrivAttributes;

        public DrawingAttributes DrawingAttributes
        {
            get {

                if (HighlighterMode == true)
                {
                    return highlighterDrawingAttr;
                }
                else
                {
                    return PrivAttributes;
                }
            }
            
            set
            {
                if (PrivAttributes != value)
                {
                    PrivAttributes = value;
                    highlighterDrawingAttr.Color = value.Color;
                    OnPropertyChanged("HighlighterMode");
                    OnPropertyChanged("DrawingAttributes");
                    OnPropertyChanged("EditingMode");
                }
            }
        }

        public void SetNormalAttrSize(double size)
        {
            PrivAttributes.Width = size;
            PrivAttributes.Height = size;
        }

        #endregion
        private bool highmode;
        public bool HighlighterMode
        {
            get { return highmode; }
            set
            {
                highmode = value;
                OnPropertyChanged("HighlighterMode");
                OnPropertyChanged("DrawingAttributes");
                OnPropertyChanged("EditingMode");
            }
        }

        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public PageViewModel()
        {
            EditingMode = InkCanvasEditingMode.Ink;
            highlighterDrawingAttr = new DrawingAttributes();
            highlighterDrawingAttr.Color = (Color)ColorConverter.ConvertFromString("Black");
            highlighterDrawingAttr.Width = 4;
            highlighterDrawingAttr.Height = 10;

            DrawingAttributes = new DrawingAttributes();
            DrawingAttributes.Color = (Color)ColorConverter.ConvertFromString("Black");
            DrawingAttributes.Width = 2;
            DrawingAttributes.Height = 2;
            DisplayedStrokes = new StrokeCollection();
            HighlighterMode = false;
        }
    }
}
