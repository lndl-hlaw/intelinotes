using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Controls;

namespace InteliNotes
{
    public enum PenStates
    {
        Pencil,
        EraserPoint,
        EraserStroke,
        Selection
    }
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Notebooks
        private Notebook _dispNotebook;
        public Notebook DisplayedNotebook
        {
            get { return _dispNotebook; }
            set
            {
                _dispNotebook = value;
                RecomposePages();
                OnPropertyChanged("DisplayedNotebook");
            }
        }

        private ObservableCollection<Notebook> nots;
        public ObservableCollection<Notebook> Notebooks
        {
            get { return nots; }
            set
            {
                nots = value;
                OnPropertyChanged("Notebooks");
            }
        }
        #endregion

        #region Drawing Attributes
        public bool isHighlighter = false;

        private InkCanvasEditingMode PrivMode;
        public InkCanvasEditingMode EditingMode
        {
            get
            {
                return PrivMode;
            }

            set
            {
                if (PrivMode != value)
                {
                    PrivMode = value;
                    OnPropertyChanged("DrawingAttributes");
                    OnPropertyChanged("EditingMode");
                }
            }
        }

        private DrawingAttributes PrivAttributes;
        public DrawingAttributes DrawingAttributes
        {
            get
            {
                return PrivAttributes;
            }

            set
            {
                if (PrivAttributes != value)
                {
                    PrivAttributes = value;
                    OnPropertyChanged("DrawingAttributes");
                    OnPropertyChanged("EditingMode");
                }
            }
        }
        #endregion

        #region Sample Stroke
        private ColorPicker.Models.NotifyableColor _pickCol;
        public ColorPicker.Models.NotifyableColor PickerColor
        {
            get { return _pickCol; }
            set
            {
                _pickCol = value;
                _penColor = Color.FromArgb((byte)value.A, (byte)value.RGB_R,
              (byte)value.RGB_G, (byte)value.RGB_B);
                OnPropertyChanged("PickerColor");
            }
        }

        private Color _penColor = Color.FromArgb(255, 0, 0, 0);
        public System.Windows.Media.Color PenColor
        {
            get
            {
                return _penColor;
            }
            set
            {
                _penColor = value;
                //PenToBrush = new SolidColorBrush(value);
                OnPropertyChanged("PenColor");
            }
        }

        //private Brush _penBr = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        //public Brush PenToBrush
        //{
        //    get { return _penBr; }
        //    set
        //    {
        //        _penBr = value;
        //        OnPropertyChanged("PenToBrush");
        //    }
        //}

        private double _penSize = 2;
        public double penSize
        {
            get { return _penSize; }
            set
            {
                _penSize = value;
                OnPropertyChanged("penSize");
            }
        }
        #endregion

        private MainWindow window;
        public MainViewModel(MainWindow window)
        {
            DrawingAttributes = new DrawingAttributes();
            DrawingAttributes.Color = (Color)ColorConverter.ConvertFromString("Black");
            DrawingAttributes.Width = 2;
            DrawingAttributes.Height = 2;
            EditingMode = InkCanvasEditingMode.Ink;

            this.window = window;
            DisplayedNotebook = new Notebook("Notes Pierwszy");
            Notebooks = new ObservableCollection<Notebook>();
            Notebooks.Add(DisplayedNotebook);
            //window.AddPage();
            Notebooks.Add(new Notebook("Notes Drugi"));
        }


        public void RemovePage(int index)
        {
            if (DisplayedNotebook.pages != null && index > -1 && index < DisplayedNotebook.pages.Count)
            {
                DisplayedNotebook.pages.RemoveAt(index);
                RecomposePages();
            }
        }

        private void RecomposePages()
        {
            window.ConcretePages.Children.RemoveRange(0, window.ConcretePages.Children.Count);
            foreach (var page in DisplayedNotebook.pages)
            {
                window.ConcretePages.Children.Add(page);
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

    }
}
