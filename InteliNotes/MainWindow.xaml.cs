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
using System.Windows.Interop;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using IronPdf;
using System.IO;
using System.IO.Compression;

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

        public void RemoveLastPage()
        {
            if (model.DisplayedNotebook.pages.Count > 1)
            {
                if(model.DisplayedNotebook.lastClickedPage == model.DisplayedNotebook.pages.Last())
                {
                    model.DisplayedNotebook.lastClickedPage = model.DisplayedNotebook.pages[model.DisplayedNotebook.pages.Count - 2];
                    model.DisplayedNotebook.lastClickedPt = new Point(0, 0);
                }
                model.RemovePage(model.DisplayedNotebook.pages.Count - 1);
                
            }
        }
        private void ButtonAddPage_Click(object sender, RoutedEventArgs e)
        {
            AddPage();
        }

        private void ButtonRemoveLastPage_Click(object sender, RoutedEventArgs e)
        {
            RemoveLastPage();
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

        private void OpenNotebook(object sender, RoutedEventArgs args)
        {
            OpenNotebook();
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
                        model.DisplayedNotebook.pages[i].DrawingCanvas.AddUIElementAtPosition(image, 0, 0);
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


        public Image AddImageFromClipboard()
        {
            Image image = null;
            if (Clipboard.ContainsImage())
            {
                // ImageUIElement.Source = Clipboard.GetImage(); // does not work
                System.Windows.Forms.IDataObject clipboardData = System.Windows.Forms.Clipboard.GetDataObject();
                if (clipboardData != null)
                {
                    if (clipboardData.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
                    {
                        System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)clipboardData.GetData(System.Windows.Forms.DataFormats.Bitmap);
                        image = AddImageToCanvas(bitmap);
                    }
                }
            }
            return image;
        }

        private Image AddImageToCanvas(System.Drawing.Bitmap bitmap, bool startPoint = false)
        {
            double width = bitmap.Width;
            double height = bitmap.Height;
            double ratio = width / height;
            double pageWidth = model.DisplayedNotebook.lastClickedPage.DrawingCanvas.ActualWidth;
            double pageHeight = model.DisplayedNotebook.lastClickedPage.DrawingCanvas.ActualHeight;
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
                model.DisplayedNotebook.pages[0].DrawingCanvas.AddUIElementAtPosition(image, position.X, position.Y);
            }
            else
            {
                model.DisplayedNotebook.lastClickedPage.DrawingCanvas.AddUIElementAtPosition(image, position.X, position.Y);
            }
            return image;
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
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    if (e.Key == Key.N)
                    {
                        RemoveLastPage();
                    }
                }
                else switch (e.Key)
                    {
                        case Key.V:

                            if (clipboard != null)
                            {
                                clipboard.Paste(model.DisplayedNotebook.lastClickedPage.DrawingCanvas, model.DisplayedNotebook.lastClickedPt);

                                StrokeCollection strokes = clipboard.GetStrokes();
                                var elems = clipboard.GetElements();
                                model.DisplayedNotebook.monitor.AddLastAction(new AddCombinedAction(
                                    model.DisplayedNotebook.lastClickedPage.DrawingCanvas, strokes, elems));
                                e.Handled = true;
                            }
                            else if (Clipboard.ContainsImage())
                            {
                                Image img = AddImageFromClipboard();
                                if(img != null)
                                {
                                    model.DisplayedNotebook.monitor.AddLastAction(new AddControlAction(
                                        model.DisplayedNotebook.lastClickedPage.DrawingCanvas, model.DisplayedNotebook.lastClickedPt, img));
                                }
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
                        case Key.D:
                            model.DisplayedNotebook.lastClickedPage.DrawingCanvas.Select(null, null);
                            e.Handled = true;
                            break;
                        case Key.E:
                            model.EditingMode = InkCanvasEditingMode.EraseByStroke;
                            break;
                        case Key.Z:
                            // todo undo
                            model.DisplayedNotebook.monitor.Undo();
                            break;
                        case Key.Y:
                            // todo redo
                            model.DisplayedNotebook.monitor.Redo();
                            break;
                    }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.B:
                        model.isHighlighter = false;
                        model.DrawingAttributes.Width = model.penSize;
                        model.DrawingAttributes.Height = model.penSize;
                        model.EditingMode = InkCanvasEditingMode.Ink;
                        break;
                    case Key.E:
                        model.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        break;
                    case Key.L:
                        model.EditingMode = InkCanvasEditingMode.Select;
                        break;
                    case Key.Delete:
                        model.DisplayedNotebook.lastClickedPage.DrawingCanvas.RemoveSelection(model.DisplayedNotebook.monitor);
                        break;
                    case Key.Back:
                        model.DisplayedNotebook.lastClickedPage.DrawingCanvas.RemoveSelection(model.DisplayedNotebook.monitor);
                        break;
                }
            }
        }

        private void CopyOrCut(bool cut)
        {
            InkCanvas page = model.DisplayedNotebook.lastClickedPage.DrawingCanvas;
            var selectedElements = page.GetSelectedElements();
            var selectedStrokes = page.GetSelectedStrokes().Clone();
            int imgCnt = selectedElements.Where(n => n is System.Windows.Controls.Image).ToList().Count;
            int strCnt = selectedStrokes.Count;
            Rect selection = page.GetSelectionBounds();
            Point fromPt = new Point() { X = selection.X, Y = selection.Y };
            if (imgCnt == 1 && strCnt == 0)
            {
                Image image = selectedElements[0] as Image;
                Clipboard.Clear();
                Clipboard.SetDataObject(image);
                if(cut)
                {
                    page.Children.Remove(image);
                    model.DisplayedNotebook.monitor.AddLastAction(new RemoveControlAction(page, fromPt, image));
                }
                clipboard = null;
            }
            else
            {
                InkCanvas canvas = model.DisplayedNotebook.lastClickedPage.DrawingCanvas;
                Rect selectionR = canvas.GetSelectionBounds();
                var stroks = canvas.GetSelectedStrokes();
                var imgs = canvas.GetSelectedElements();
                List<(UIElement, Point)> im = new List<(UIElement, Point)>();
                foreach (var image in imgs)
                {
                    im.Add((image,new Point() { X = InkCanvas.GetLeft(image), Y = InkCanvas.GetTop(image) }));
                }
                clipboard = new CustomClipboard(selectionR, stroks, im);
                if (cut)
                {
                    canvas.RemoveSelection();
                    model.DisplayedNotebook.monitor.AddLastAction(new RemoveCombinedAction(canvas, selectedStrokes, im));
                }

            }
        }
        #endregion

        #region File Menu

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            SaveNotebook();
        }

        private void SaveNotebook()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "InteliNote files | *.inote";
            if (dialog.ShowDialog() == true)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        for (int i = 0; i < model.DisplayedNotebook.pages.Count; ++i)
                        {
                            InkCanvas canvas = model.DisplayedNotebook.pages[i].DrawingCanvas;

                            var strokesFile = archive.CreateEntry($"page{i + 1}/strokes.bin");

                            using (var entryStream = strokesFile.Open())
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    canvas.Strokes.Save(ms);
                                    ms.WriteTo(entryStream);
                                }
                            }
                            foreach (var element in canvas.Children)
                            {
                                if (element is Image)
                                {
                                    Image image = element as Image;
                                    var encoder = new JpegBitmapEncoder();
                                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image.Source));
                                    using (MemoryStream stream = new MemoryStream())
                                    {
                                        encoder.Save(stream);
                                        int x = (int)InkCanvas.GetLeft(image);
                                        int y = (int)InkCanvas.GetTop(image);
                                        var imageEntry = archive.CreateEntry($"page{i + 1}/image_{x}_{y}.jpg");
                                        using (var entrStr = imageEntry.Open())
                                        {
                                            stream.WriteTo(entrStr);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                    }
                }
            }
        }
        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenNotebook();
        }

        private void OpenNotebook()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "InteliNote files | *.inote";
            if (dialog.ShowDialog() == true)
            {
                Notebook notebook = new Notebook(System.IO.Path.GetFileName(dialog.FileName), false);

                using (var file = File.Open(dialog.FileName, FileMode.Open))
                using (var archive = new ZipArchive(file, ZipArchiveMode.Read, true))
                {
                    foreach (var entry in archive.Entries)
                    {
                        using (var str = entry.Open())
                        {
                            using (var memory = new MemoryStream())
                            {
                                str.CopyTo(memory);
                                if (entry.Name.Contains("stroke"))
                                {
                                    PageControl page = notebook.AddPage();
                                    memory.Position = 0;
                                    page.DrawingCanvas.Strokes = new StrokeCollection(memory);
                                }
                                else if (entry.Name.Contains("image"))
                                {
                                    string name = entry.FullName;
                                    int pageNr = int.Parse(name.Substring(4, name.IndexOf("/") - 4));
                                    int firstNr = name.IndexOf("_") + 1;
                                    int lastNr = name.LastIndexOf("_") + 1;
                                    int left = int.Parse(name.Substring(firstNr, lastNr - firstNr - 1));
                                    int top = int.Parse(name.Substring(lastNr, name.IndexOf(".jpg") - lastNr));
                                    BitmapImage image = new BitmapImage();
                                    image.BeginInit();
                                    image.CacheOption = BitmapCacheOption.OnLoad;
                                    memory.Position = 0;
                                    image.StreamSource = memory;
                                    image.EndInit();
                                    Image img = new Image();
                                    img.Source = image;
                                    notebook.pages[pageNr - 1].DrawingCanvas.AddUIElementAtPosition(img, left, top);
                                }
                            }
                        }
                    }
                }
                model.Notebooks.Add(notebook);
                model.DisplayedNotebook = notebook;
            }
        }

        #endregion

    }

    public class Notebook : INotifyPropertyChanged
    {

        #region Last Clicked Canvas

        private PageControl _lastPage;
        public PageControl lastClickedPage
        {
            get
            {
                if(_lastPage == null && pages != null && pages.Count > 0 && pages[0] != null)
                {
                    return pages[0];
                }
                else
                {
                    return _lastPage;
                }
            }
            set
            {
                if(_lastPage != value)
                {
                    _lastPage = value;
                }
            }
        }

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

        public StateMonitor monitor;
        public Notebook(string name, bool addpage = true)
        {
            Name = name;
            pages = new List<PageControl>();
            monitor = new StateMonitor(50);

            if(addpage)
            {
                PageControl page = new PageControl(this);
                page.RequestBringIntoView += (s, e) => { e.Handled = true; };
                pages.Add(page);
                lastClickedPage = page;
                lastClickedPt = new Point(0, 0);
            }
        }

        public PageControl AddPage()
        {
            PageControl page = new PageControl(this);
            page.RequestBringIntoView += (s, e) => { e.Handled = true; };
            pages.Add(page);
            if(lastClickedPage == null)
            {
                lastClickedPage = page;
                lastClickedPt = new Point(0, 0);
            }
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
