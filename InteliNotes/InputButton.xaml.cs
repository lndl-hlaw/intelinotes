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
using MahApps.Metro.IconPacks;

namespace InteliNotes
{
    /// <summary>
    /// Logika interakcji dla klasy InputButton.xaml
    /// </summary>
    public partial class InputButton : UserControl, INotifyPropertyChanged
    {
        public InputButton()
        {
            InitializeComponent();
            PreviewMouseLeftButtonUp += (sender, args) => OnClick();
        }

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
                                "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InputButton));

        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(InputButton.ClickEvent);
            RaiseEvent(newEventArgs);
        }

        void OnClick()
        {
            RaiseClickEvent();
        }

        public PackIconMaterialKind Icon
        {
            get { return (PackIconMaterialKind)GetValue(IconProperty); }
            set
            {
                SetValue(IconProperty, value);
                OnPropertyChanged("Icon");
            }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(PackIconMaterialKind), typeof(InputButton));

        public Brush IconColor
        {
            get { return (Brush)GetValue(ColorProperty); }
            set
            {
                SetValue(ColorProperty, value);
                OnPropertyChanged("IconColor");
            }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("IconColor", typeof(Brush), typeof(InputButton));


        public Brush IconBackground
        {
            get { return (Brush)GetValue(BackgroundPropertyCustom); }
            set
            {
                SetValue(BackgroundPropertyCustom, value);
                OnPropertyChanged("IconColor");
            }
        }

        public static readonly DependencyProperty BackgroundPropertyCustom = DependencyProperty.Register("IconBackground", typeof(Brush), typeof(InputButton));

        #region Property Change
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
