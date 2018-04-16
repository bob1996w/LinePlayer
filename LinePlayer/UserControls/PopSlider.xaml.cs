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

namespace LinePlayer.UserControls
{
    /// <summary>
    /// PopSlider.xaml 的互動邏輯
    /// </summary>
    public partial class PopSlider : UserControl
    {
        public PopSlider()
        {
            InitializeComponent();
        }
        public SolidColorBrush BarColor { get { return GetValue(BarColorProperty) as SolidColorBrush; } set { SetValue(BarColorProperty, value); } }
        public static readonly DependencyProperty BarColorProperty = DependencyProperty.Register(nameof(BarColor), typeof(SolidColorBrush), typeof(PopSlider), new PropertyMetadata(
            Brushes.Black, (DependencyObject obj, DependencyPropertyChangedEventArgs args) => {
                (obj as PopSlider).slider_border.Background = args.NewValue as SolidColorBrush;
            }));
        public SolidColorBrush BackgroundColor { get { return GetValue(BackgroundColorProperty) as SolidColorBrush; } set { SetValue(BackgroundColorProperty, value); } }
        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(nameof(BackgroundColor), typeof(SolidColorBrush), typeof(PopSlider), new PropertyMetadata(
            Brushes.White, (DependencyObject obj, DependencyPropertyChangedEventArgs args) => {
                (obj as PopSlider).background_border.Background = args.NewValue as SolidColorBrush;
            }));
        public double MinValue { get { return (double)GetValue(MinValueProperty); } set { SetValue(MinValueProperty, value); } }
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(PopSlider), new PropertyMetadata(
            0d));
        public double MaxValue { get { return (double)GetValue(MaxValueProperty); } set { SetValue(MaxValueProperty, value); } }
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(PopSlider), new PropertyMetadata(
            100d));
        public double NowValue { get { return (double)GetValue(NowValueProperty); } set { SetValue(NowValueProperty, value); } }
        public static readonly DependencyProperty NowValueProperty = DependencyProperty.Register(nameof(NowValue), typeof(double), typeof(PopSlider), new FrameworkPropertyMetadata(
            50d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,(DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
            {
                var slider = (obj as PopSlider);
                if (!slider.IsSliderPreview) { Watch_Border_Size_Change(slider, (double)args.NewValue); }
            }));
        public double TempValue { get { return (double)GetValue(TempValueProperty); } set { SetValue(TempValueProperty, value); } }
        public static readonly DependencyProperty TempValueProperty = DependencyProperty.Register(nameof(TempValue), typeof(double), typeof(PopSlider), new FrameworkPropertyMetadata(
            50d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public bool IsSliderPreview { get { return (bool)GetValue(IsSliderPreviewProperty); } set { SetValue(IsSliderPreviewProperty, value); } }
        public static readonly DependencyProperty IsSliderPreviewProperty = DependencyProperty.Register(nameof(IsSliderPreview), typeof(bool), typeof(PopSlider), new PropertyMetadata(
            false));
        public string LabelText { get { return (string)GetValue(SliderLabelProperty); } set { SetValue(SliderLabelProperty, value); } }
        public static readonly DependencyProperty SliderLabelProperty = DependencyProperty.Register(nameof(LabelText), typeof(string), typeof(PopSlider), new PropertyMetadata(
            "",  (DependencyObject obj, DependencyPropertyChangedEventArgs args) => {
                (obj as PopSlider).slider_label.Content = (string)args.NewValue;
            }));
        public SolidColorBrush LabelTextColor { get { return GetValue(LabelTextColorProperty) as SolidColorBrush; } set { SetValue(LabelTextColorProperty, value); } }
        public static readonly DependencyProperty LabelTextColorProperty = DependencyProperty.Register(nameof(LabelTextColor), typeof(SolidColorBrush), typeof(PopSlider), new PropertyMetadata(
            Brushes.Black, (DependencyObject obj, DependencyPropertyChangedEventArgs args) => {
                (obj as PopSlider).slider_label.Foreground = args.NewValue as SolidColorBrush;
            }));
        public static void Watch_Border_Size_Change(PopSlider slider, double newValue)
        {
            var border = slider.slider_border;
            var val = Math.Max(Math.Min(newValue, slider.MaxValue), slider.MinValue);
            var rightMargin = (slider.MaxValue - val) / (slider.MaxValue - slider.MinValue) * slider.ActualWidth;
            border.Margin = new Thickness(0, 0, rightMargin, 0);
        }


        private void slider_border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void slider_border_MouseEnter(object sender, MouseEventArgs e)
        {
            IsSliderPreview = true;
        }

        private void slider_border_MouseLeave(object sender, MouseEventArgs e)
        {
            IsSliderPreview = false;
            Watch_Border_Size_Change(this, NowValue);
        }

        private void slider_border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Watch_Border_Size_Change(this, NowValue);
        }

        private void slider_border_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePos = e.GetPosition(this).X;
            TempValue = (mousePos / ActualWidth) * (MaxValue - MinValue) + MinValue;
            Watch_Border_Size_Change(this, TempValue);
            HoverChangedEvent?.Invoke(this, TempValue);
        }

        private void slider_border_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(this).X;
            NowValue = (mousePos / ActualWidth) * (MaxValue - MinValue) + MinValue;
            ValueChangedEvent?.Invoke(this, NowValue);
        }

        // value changed event
        public delegate void ValueChangedEventHandler(object sender, double value);
        public event ValueChangedEventHandler ValueChangedEvent;

        public delegate void HoverChangedEventHandler(object sender, double value);
        public event HoverChangedEventHandler HoverChangedEvent;
    }
}
