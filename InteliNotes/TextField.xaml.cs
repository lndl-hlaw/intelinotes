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

namespace InteliNotes
{
    /// <summary>
    /// Logika interakcji dla klasy TextField.xaml
    /// </summary>
    public partial class TextField : UserControl
    {
        Notebook notebook;
        bool unsetVisibility = false;
        public TextField(Notebook notebook, bool visibility = true)
        {
            this.notebook = notebook;
            unsetVisibility = !visibility;
            InitializeComponent();
        }


        private void tekst_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            notebook.lastTextField = this;
            tekst.BorderThickness = new Thickness(1);
            SetGrid(Visibility.Visible);
            e.Handled = false;
        }
        private void tekst_Loaded(object sender, RoutedEventArgs e)
        {
            if(unsetVisibility)
            {
                DisableView();
            }
        }

        public void DisableView()
        {
            tekst.BorderThickness = new Thickness(0);
            SetGrid(Visibility.Hidden);
        }

        private void SetGrid(Visibility visibility)
        {
            var template = tekst.Template;
            if(template != null)
            {
                var resizeGrid = (Grid)template.FindName("resizeGripGrid", tekst);
                if(resizeGrid != null)
                {
                    resizeGrid.Visibility = visibility;
                }
            }
        }

        #region Resize Data

        private double prevWidth, prevHeight;
        private Point prevPoint;

        private void resizeGripGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                e.Handled = true;
                Grid grid = (Grid)sender;
                grid.CaptureMouse();

                prevWidth = tekst.ActualWidth;
                prevHeight = tekst.ActualHeight;

                prevPoint = e.GetPosition(null);
            }
            catch (Exception)
            {
            }
        }

        private void resizeGripGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point point = e.GetPosition(null);
                    var xDiff = point.X - prevPoint.X;
                    var yDiff = point.Y - prevPoint.Y;

                    tekst.Width = prevWidth + xDiff;
                    tekst.Height = prevHeight + yDiff;


                    prevWidth = tekst.Width;
                    prevHeight = tekst.Height;
                    prevPoint = e.GetPosition(null);
                }
            }
            catch (Exception)
            {
            }
        }


        private void resizeGripGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                e.Handled = true;
                Grid grid = (Grid)sender;
                grid.ReleaseMouseCapture();

                prevWidth = tekst.ActualWidth;
                prevHeight = tekst.ActualHeight;
            }
            catch (Exception)
            {
            }
        }
        #endregion

    }

    //public class TextFieldViewModel
    //{
    //    public TextFieldViewModel()
    //    {
    //        TextFieldDisabled = false;
    //    }

    //    private bool disbled;
    //    public bool TextFieldDisabled
    //    {
    //        get { return disbled; }
    //        set
    //        {
    //            disbled = value;
    //            OnPropertyChanged("TextFieldDisabled");
    //        }
    //    }

    //    #region Property Changed
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected virtual void OnPropertyChanged(string propertyName)
    //    {
    //        PropertyChangedEventHandler handler = PropertyChanged;
    //        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion
    //}

}
