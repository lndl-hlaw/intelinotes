using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using IronPdf;
using System.IO;
using System.Windows.Interop;

namespace InteliNotes
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        MainViewModel model;
        public MainWindow()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Window));
            model = new MainViewModel(this);
            DataContext = model;
        }

        public void AddPage()
        {
            ConcretePages.Children.Add(model.DisplayedNotebook.AddPage());
        }
        private void ButtonAddPage_Click(object sender, RoutedEventArgs e)
        {
            AddPage();
        }

        private void ButtonRemoveLastPage_Click(object sender, RoutedEventArgs e)
        {
            if (model.DisplayedNotebook.pages.Count > 1)
            {
                model.RemovePage(model.DisplayedNotebook.pages.Count - 1);
            }
        }


        private void RemoveTabClick(object sender, RoutedEventArgs args)
        {
            if (Tabs.Items.Count > 1)
            {
                int index = Tabs.Items.IndexOf(((System.Windows.Controls.Button)sender).DataContext as Notebook);
                Tabs.SelectedItem = Tabs.Items[index == 0 ? index + 1 : index - 1];
                model.Notebooks.Remove(((System.Windows.Controls.Button)sender).DataContext as Notebook);
            }
        }

        public void FillPages()
        {
            foreach(var page in model.DisplayedNotebook.pages)
            {
                ConcretePages.Children.Add(page);
            }
        }

        private void StandardColorPicker_ColorChanged(object sender, RoutedEventArgs e)
        {
            ColorPicker.StandardColorPicker picker = sender as ColorPicker.StandardColorPicker;
            Color color = Color.FromArgb((byte)picker.Color.A, (byte)picker.Color.RGB_R,
                (byte)picker.Color.RGB_G, (byte)picker.Color.RGB_B);
            if(model.isHighlighter)
            {
                color.A = 130;
            }
            model.PenColor = color;
            model.DrawingAttributes.Color = color;
        }

        private void SelectPenClick(object sender, RoutedEventArgs args)
        {
            model.isHighlighter = false;
            model.DrawingAttributes.Width = model.penSize;
            model.DrawingAttributes.Height = model.penSize;
            model.EditingMode = InkCanvasEditingMode.Ink;
        }
        private void SelectEraserClick(object sender, RoutedEventArgs args)
        {
            model.EditingMode = InkCanvasEditingMode.EraseByPoint;
        }
        private void SelectEraserStrokeClick(object sender, RoutedEventArgs args)
        {
            model.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }
        private void SelectSelectionClick(object sender, RoutedEventArgs args)
        {
            model.EditingMode = InkCanvasEditingMode.Select;
        }

        private void GetDefaultColor_Click(object sender, RoutedEventArgs args)
        {
            Color color = ((SolidColorBrush)(sender as PaintColorControl).IconColor).Color;
            model.isHighlighter = false;
            model.DrawingAttributes.Width = model.penSize;
            model.DrawingAttributes.Height = model.penSize;
            model.PenColor = color;
            model.DrawingAttributes.Color = color;
        }

        private void GetHighlighterColor_Click(object sender, RoutedEventArgs args)
        {
            Color color = ((SolidColorBrush)(sender as InputButton).IconColor).Color;
            color.A = 130;
            model.isHighlighter = true;
            model.PenColor = color;
            model.DrawingAttributes.Color = color;
            model.DrawingAttributes.Width = 4;
            model.DrawingAttributes.Height = 10;
        }

        private void slValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (model != null && model.isHighlighter == false)
            {
                double size = Math.Sqrt(e.NewValue);
                model.penSize = size;
                model.DrawingAttributes.Width = size;
                model.DrawingAttributes.Height = size;
            }
        }


        private void OpenMediaInNotebook(object sender, RoutedEventArgs args)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Files (*.*)|*.*|Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...|Pdf files|*.pdf";
            if (dialog.ShowDialog() == true)
            {
                string file = dialog.FileName;
                if (file.EndsWith(".pdf"))
                {
                    IronPdf.PdfDocument pdf = IronPdf.PdfDocument.FromFile(file);
                    System.Drawing.Bitmap[] pageImages = pdf.ToBitmap();
                    int pagesCnt = pdf.PageCount;
                    int index = model.DisplayedNotebook.pages.Count - 1;
                    for (; index >= 0; --index)
                    {
                        var page = model.DisplayedNotebook.pages[index];
                        if (page.DisplayedStrokes.Count != 0 ||
                            page.DrawingCanvas.Children.OfType<Image>().ToList().Count > 0)
                        {
                            break;
                        }
                    }
                    int pagesToAdd = pagesCnt - (model.DisplayedNotebook.pages.Count - index - 1);
                    for (int i = 0; i < pagesToAdd; ++i)
                    {
                        AddPage();
                    }
                    for (int i = index + 1, j = 0; i < model.DisplayedNotebook.pages.Count; ++i, ++j)
                    {
                        Image image = new Image();
                        image.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(pageImages[j].GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        model.DisplayedNotebook.pages[i].DrawingCanvas.Children.Add(image);
                    }
                }
                else
                {
                    System.Drawing.Bitmap bitmap;
                    using (Stream bmpStream = System.IO.File.Open(file, System.IO.FileMode.Open))
                    {
                        System.Drawing.Image image = System.Drawing.Image.FromStream(bmpStream);
                        bitmap = new System.Drawing.Bitmap(image);
                        AddImageToCanvas(bitmap, true);
                    }
                }
            }
        }


        private void SaveNotebookAsPdf(object sender, RoutedEventArgs args)
        {
            PdfSharp.Pdf.PdfDocument doc = new PdfSharp.Pdf.PdfDocument();
            for (int i = 0; i < model.DisplayedNotebook.pages.Count; ++i)
            {
                InkCanvas inkCanvas = model.DisplayedNotebook.pages[i].DrawingCanvas;
                int width = (int)inkCanvas.ActualWidth;
                int height = (int)inkCanvas.ActualHeight;
                int left = (int)inkCanvas.Margin.Left;
                int top = (int)inkCanvas.Margin.Top;
                RenderTargetBitmap rtb = new RenderTargetBitmap(width + left, height + top, 96d, 96d, PixelFormats.Pbgra32);
                rtb.Render(inkCanvas);
                CroppedBitmap crop = new CroppedBitmap(rtb, new Int32Rect(left, top, width, height));

                PdfPage page = new PdfPage();

                System.IO.MemoryStream outStream = new System.IO.MemoryStream();
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(crop));
                enc.Save(outStream);
                XImage img = XImage.FromStream(outStream);

                img.Interpolate = false;

                doc.Pages.Add(page);

                XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[i]);
                xgr.DrawImage(img, 0, 0, 595, 842);
                outStream.Close();
            }
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Pdf files|*.pdf";
            if (dialog.ShowDialog() == true)
            {
                doc.Save(dialog.FileName);
                doc.Close();
            }
        }


        public void AddImageFromClipboard()
        {
            if (Clipboard.ContainsImage())
            {
                // ImageUIElement.Source = Clipboard.GetImage(); // does not work
                System.Windows.Forms.IDataObject clipboardData = System.Windows.Forms.Clipboard.GetDataObject();
                if (clipboardData != null)
                {
                    if (clipboardData.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
                    {
                        System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)clipboardData.GetData(System.Windows.Forms.DataFormats.Bitmap);
                        AddImageToCanvas(bitmap);
                    }
                }
            }
        }

        private void AddImageToCanvas(System.Drawing.Bitmap bitmap, bool startPoint = false)
        {
            double width = bitmap.Width;
            double height = bitmap.Height;
            double ratio = width / height;
            double pageWidth = model.DisplayedNotebook.lastClickedPage.ActualWidth;
            double pageHeight = model.DisplayedNotebook.lastClickedPage.ActualHeight;
            Point position = startPoint ? new Point(0,0) :model.DisplayedNotebook.lastClickedPt;

            if (width + position.X > pageWidth)
            {
                position.X = Math.Max(0, pageWidth - width);
                if (width > pageWidth)
                {
                    width = pageWidth;
                    height = width / ratio;
                    bitmap = ResizeImage(bitmap, (int)width, (int)height);
                }
            }

            if (height + position.Y > pageHeight)
            {
                position.Y = Math.Max(0, pageHeight - height);
                if (height > pageHeight)
                {
                    height = pageHeight;
                    width = height * ratio;
                    bitmap = ResizeImage(bitmap, (int)width, (int)height);
                }
            }

            Image image = new Image();
            image.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            if (startPoint)
            {
                model.DisplayedNotebook.pages[0].DrawingCanvas.Children.Add(image);
            }
            else
            {
                model.DisplayedNotebook.lastClickedPage.Children.Add(image);
            }

            InkCanvas.SetTop(image, position.Y);
            InkCanvas.SetLeft(image, position.X);
        }

        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new System.Drawing.Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        #region Key Actions

        private CustomClipboard clipboard;
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                switch(e.Key)
                {
                    case Key.V:
                        
                        if(clipboard != null)
                        {
                            clipboard.Paste(model.DisplayedNotebook.lastClickedPage, model.DisplayedNotebook.lastClickedPt);
                            e.Handled = true;
                        }
                        else if (Clipboard.ContainsImage())
                        {
                            AddImageFromClipboard();
                            e.Handled = true;
                        }
                        break;
                    case Key.X:
                        CopyOrCut(true);
                        e.Handled = true;
                        break;
                    case Key.C:
                        CopyOrCut(false);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void CopyOrCut(bool cut)
        {
            InkCanvas page = model.DisplayedNotebook.lastClickedPage;
            var selectedElements = page.GetSelectedElements();
            var selectedStrokes = page.GetSelectedStrokes().Clone();
            int imgCnt = selectedElements.Where(n => n is System.Windows.Controls.Image).ToList().Count;
            int strCnt = selectedStrokes.Count;
            if (imgCnt == 1 && strCnt == 0)
            {
                Image image = selectedElements[0] as Image;
                Clipboard.Clear();
                Clipboard.SetDataObject(image);
                if(cut)
                {
                    page.Children.Remove(image);
                }
                clipboard = null;
            }
            else
            {
                InkCanvas canvas = model.DisplayedNotebook.lastClickedPage;
                Rect selectionR = canvas.GetSelectionBounds();
                var stroks = canvas.GetSelectedStrokes();
                var imgs = canvas.GetSelectedElements();
                Dictionary<UIElement, Point> im = new Dictionary<UIElement, Point>();
                foreach (var image in imgs)
                {
                    im[image] = new Point() { X = InkCanvas.GetLeft(image), Y = InkCanvas.GetTop(image) };
                }
                clipboard = new CustomClipboard(selectionR, stroks, im);
                if (cut)
                {
                    for(int i = 0; i < imgs.Count; ++i)
                    {
                        Image image = imgs[i] as Image;
                        canvas.Children.Remove(image);
                    }
                    canvas.Strokes.Remove(stroks);
                }

            }
        }
        #endregion
    }

    public class Notebook : INotifyPropertyChanged
    {

        #region Last Clicked Canvas
        public InkCanvas lastClickedPage;
        public Point lastClickedPt;
        #endregion

        private List<PageControl> _pages;
        public List<PageControl> pages
        {
            get { return _pages; }
            set
            {
                _pages = value;
                OnPropertyChanged("pages");
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        public Notebook(string name)
        {
            Name = name;
            pages = new List<PageControl>();
            PageControl page = new PageControl(this);
            page.RequestBringIntoView += (s, e) => { e.Handled = true; };
            pages.Add(page);
            lastClickedPage = page.DrawingCanvas;
            lastClickedPt = new Point(0, 0);

        }

        public PageControl AddPage()
        {
            PageControl page = new PageControl(this);
            page.RequestBringIntoView += (s, e) => { e.Handled = true; };
            pages.Add(page);
            return page;
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
