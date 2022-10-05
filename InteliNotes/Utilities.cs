using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;

namespace InteliNotes
{
    public static class Constants
    {
        public readonly static string MainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"InteliNote");
        public readonly static string OpenedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"InteliNote\opened.txt");
        public readonly static string NotebooksPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"InteliNote\Notebooks");
    }

    public static class InkCanvasExtender
    {
        public static void Move(this StrokeCollection strokes, double offsetX, double offsetY)
        {
            Matrix matrix = new Matrix();
            matrix.Translate(offsetX, offsetY);
            strokes.Transform(matrix, false);
        }
        public static void Move(this Stroke stroke, double offsetX, double offsetY)
        {
            Matrix matrix = new Matrix();
            matrix.Translate(offsetX, offsetY);
            stroke.Transform(matrix, false);
        }
        public static void RemoveSelection(this InkCanvas canvas, StateMonitor monitor = null)
        {
            var elems = canvas.GetSelectedElements();
            var strokes = canvas.GetSelectedStrokes();
            canvas.Strokes.Remove(strokes);
            List<(UIElement, Point)> monList = new List<(UIElement, Point)>();
            for(int i = 0; i < elems.Count; ++i)
            {
                UIElement elem = elems[i];

                if(monitor != null)
                {
                    monList.Add((elem, new Point() { X = InkCanvas.GetLeft(elem), Y = InkCanvas.GetTop(elem) }));
                }

                canvas.Children.Remove(elem);
            }
            canvas.Select(null, null);

            if(monitor != null)
            {
                monitor.AddLastAction(new RemoveCombinedAction(canvas, strokes, monList));
            }
        }
        public static void AddUIElementAtPosition(this InkCanvas canvas, UIElement element, Point point)
        {
            canvas.AddUIElementAtPosition(element, point.X, point.Y);
        }

        public static void AddUIElementAtPosition(this InkCanvas canvas, UIElement element, double x, double y)
        {
            canvas.Children.Add(element);

            if (element is Image)
            {
                element.MouseLeftButtonDown += (sender, e) =>
                {
                    if (e.ClickCount == 2)
                    {
                        if (canvas.GetSelectedElements().Contains(sender))
                        {
                            canvas.Select(null, null);
                        }
                    }
                };
            }
            else if(element is TextField)
            {
                (element as TextField).tekst.MouseDoubleClick += (sender, e) =>
                {
                    if (canvas.GetSelectedElements().Contains(sender))
                    {
                        canvas.Select(null, null);
                    }
                };
            }

            InkCanvas.SetTop(element, y);
            InkCanvas.SetLeft(element, x);
        }

        public static T DeepClone<T>(this T from)
        {
            if(!typeof(T).IsSerializable)
            {
                throw new InvalidOperationException($"Typ {typeof(T)} nie jest serializowalny");
            }

            using (MemoryStream s = new MemoryStream())
            {
                BinaryFormatter f = new BinaryFormatter();
                f.Serialize(s, from);
                s.Position = 0;
                object clone = f.Deserialize(s);
                return (T)clone;
            }
        }
    }
    public class DropOutStack<T>
    {
        internal class Item
        {
            public T Value;
            public Item next;
            public Item prev;
            public Item(T v, Item next = null, Item prev = null)
            {
                Value = v;
                this.next = next;
                this.prev = prev;
            }
        }

        private Item Top;
        private Item Bottom;
        public int Count = 0;
        public int MaxCount;


        public DropOutStack(int capacity)
        {
            MaxCount = capacity;
        }

        public void Push(T value)
        {
            Item item = new Item(value);
            Push(item);
        }

        private void Push(Item item)
        {
            if(Count == 0)
            {
                Top = Bottom = item;
                Count++;
            }
            else if(Count == MaxCount)
            {
                Bottom = Bottom.next;
                Bottom.prev.next = null;
                Bottom.prev = null;

                Top.next = item;
                item.prev = Top;
                Top = Top.next;
            }
            else
            {
                Top.next = item;
                item.prev = Top;
                Top = Top.next;
                Count++;
            }
        }

        public T Pop()
        {
            if(Count == 0)
            {
                throw new ArgumentOutOfRangeException("Stos jest pusty");
            }
            else
            {
                T item = Top.Value;
                Top = Top.prev;
                Count--;
                return item;
            }
        }
        private Item Remove(Item current)
        {
            Item next = current.next;
            Item prev = current.prev;

            current.next = null;
            current.prev = null;

            if (current != Top)
            {
                next.prev = prev;
            }
            else
            {
                Top = prev;
            }

            if (current != Bottom)
            {
                prev.next = next;
            }
            else
            {
                Bottom = next;
            }
            Count--;
            return next;
        }
        private Item RemoveOnCondition(Item current, Func<T, bool> remove)
        {
            Item next = current.next;
            if (remove(current.Value) == true)
            {
                next = Remove(current);
            }
            return next;
        }
        public void RemoveMatching(Func<T, bool> remove)
        {
            Item current = Bottom;
            while(current != null)
            {
                current = RemoveOnCondition(current, remove);
            }
        }
    }
    public class PageCountConverter: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string actualPage = values[0].ToString();
            string totalPages = values[1].ToString();
            return $"Strona {actualPage}/{totalPages}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value == true ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
