using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.UI.Popups;

namespace YOURSOLUTION.Controls
{
    // Enum to define the picker's opening direction
    public enum PickerPlacement { Top, Bottom, Left, Right }

    public partial class AdvancedColorPickerControl : UserControl, INotifyPropertyChanged
    {
        // Private fields
        private double _hue;
        private double _saturation;
        private double _value;
        private double _alpha;
        private bool _isUpdatingFromColor;
        private bool _isPrimaryActive = true;

        public AdvancedColorPickerControl()
        {
            InitializeComponent();
            UpdateComponentsFromColor(SelectedColor);
            PopulateSwatches();

        }
        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            // This helper tells us exactly if ExternalTrigger was written in the XAML 
            // (either bound to something or explicitly set), bypassing the CLR setter issue.
            var valueSource = DependencyPropertyHelper.GetValueSource(this, ExternalTriggerProperty);

            // BaseValueSource.Default means it was NEVER touched (like your Avatar picker).
            // Anything else (Local, ParentTemplate, etc.) means it was set/bound.
            if (valueSource.BaseValueSource != BaseValueSource.Default)
            {
                HideButton = true;
            }
        }
        private void ColorPickerPopup_Closed(object sender, EventArgs e)
        {
            // Reset the trigger so the parent can set it to 'true' again later
            ExternalTrigger = false;
        }


        private bool _hideButton;

        public bool HideButton
        {
            get => _hideButton;
            private set
            {
                if (_hideButton != value)
                {
                    _hideButton = value;
                    OnPropertyChanged(nameof(HideButton));
                }
            }
        }
        #region Dependency Properties

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(AdvancedColorPickerControl),
                new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public static readonly DependencyProperty SecondaryColorProperty =
            DependencyProperty.Register(nameof(SecondaryColor), typeof(Color), typeof(AdvancedColorPickerControl),
                new FrameworkPropertyMetadata(Colors.Gray, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

        public Color SecondaryColor
        {
            get => (Color)GetValue(SecondaryColorProperty);
            set => SetValue(SecondaryColorProperty, value);
        }

        public static readonly DependencyProperty EnableSecondaryProperty =
            DependencyProperty.Register(nameof(EnableSecondary), typeof(bool), typeof(AdvancedColorPickerControl), new PropertyMetadata(false));

        public bool EnableSecondary
        {
            get => (bool)GetValue(EnableSecondaryProperty);
            set => SetValue(EnableSecondaryProperty, value);
        }

        public static readonly DependencyProperty IsDarkModeProperty =
            DependencyProperty.Register(nameof(IsDarkMode), typeof(bool), typeof(AdvancedColorPickerControl), new PropertyMetadata(false));

        public bool IsDarkMode
        {
            get => (bool)GetValue(IsDarkModeProperty);
            set => SetValue(IsDarkModeProperty, value);
        }

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register(nameof(Placement), typeof(PickerPlacement), typeof(AdvancedColorPickerControl), new PropertyMetadata(PickerPlacement.Bottom));

        public PickerPlacement Placement
        {
            get => (PickerPlacement)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        public static readonly DependencyProperty CenterAlignProperty =
            DependencyProperty.Register(nameof(CenterAlign), typeof(bool), typeof(AdvancedColorPickerControl), new PropertyMetadata(false));

        public bool CenterAlign
        {
            get => (bool)GetValue(CenterAlignProperty);
            set => SetValue(CenterAlignProperty, value);
        }

        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.Register(nameof(HorizontalOffset), typeof(double), typeof(AdvancedColorPickerControl), new PropertyMetadata(0.0));

        public double HorizontalOffset
        {
            get => (double)GetValue(HorizontalOffsetProperty);
            set => SetValue(HorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register(nameof(VerticalOffset), typeof(double), typeof(AdvancedColorPickerControl), new PropertyMetadata(0.0));

        public double VerticalOffset
        {
            get => (double)GetValue(VerticalOffsetProperty);
            set => SetValue(VerticalOffsetProperty, value);
        }

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register(nameof(Offset), typeof(double), typeof(AdvancedColorPickerControl), new PropertyMetadata(0.0, OnOffsetChanged));

        public double Offset
        {
            get => (double)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }

        private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedColorPickerControl control && e.NewValue is double offset)
            {
                control.HorizontalOffset = offset;
                control.VerticalOffset = offset;
            }
        }

        public static readonly DependencyProperty ExternalTriggerProperty =
    DependencyProperty.Register(
        nameof(ExternalTrigger),
        typeof(bool),
        typeof(AdvancedColorPickerControl),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnExternalTriggerChanged));

        public bool ExternalTrigger
        {
            get => (bool)GetValue(ExternalTriggerProperty);
            set => SetValue(ExternalTriggerProperty, value);
        }

        private static void OnExternalTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedColorPickerControl control && e.NewValue is bool isOpen)
            {
                control.HideButton = true;
                if (isOpen)
                {
                    // Lock the button visibility to hidden when using external triggers
                    control.HideButton = true;
                    control._isPrimaryActive = true;
                    control.OnPropertyChanged(nameof(ActiveColor));

                    // Open the popup
                    control.ColorPickerPopup.IsOpen = true;
                }
            }
        }


        // --- Swatches Property ---

        public static readonly DependencyProperty SwatchesProperty =
            DependencyProperty.Register(
                nameof(Swatches),
                typeof(IList<Color>),
                typeof(AdvancedColorPickerControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSwatchesChanged));

        /// <summary>
        /// Bindable list of colors to display as swatches.
        /// When null or empty the built-in default palette is shown.
        /// Supports <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>:
        /// adding a color (e.g. a recently-used color) will automatically refresh the panel.
        /// </summary>
        public IList<Color> Swatches
        {
            get => (IList<Color>)GetValue(SwatchesProperty);
            set => SetValue(SwatchesProperty, value);
        }

        private static void OnSwatchesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AdvancedColorPickerControl control) return;

            // Unsubscribe from the old collection if it was observable
            if (e.OldValue is INotifyCollectionChanged oldObservable)
                oldObservable.CollectionChanged -= control.Swatches_CollectionChanged;

            // Subscribe to the new collection if it is observable
            if (e.NewValue is INotifyCollectionChanged newObservable)
                newObservable.CollectionChanged += control.Swatches_CollectionChanged;

            control.PopulateSwatches();
        }

        private void Swatches_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Re-render the swatch panel any time the bound collection changes
            // (e.g. the parent appends a recently-used / favorited color)
            PopulateSwatches();
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion

        #region Internal Properties

        public Color ActiveColor => _isPrimaryActive ? SelectedColor : SecondaryColor;

        public double Hue { get => _hue; set { _hue = value; UpdateColorFromComponents(); OnPropertyChanged(nameof(Hue)); } }
        public double Saturation { get => _saturation; set { _saturation = value; UpdateColorFromComponents(); OnPropertyChanged(nameof(Saturation)); } }
        public double Value { get => _value; set { _value = value; UpdateColorFromComponents(); OnPropertyChanged(nameof(Value)); } }
        public double Alpha { get => _alpha; set { _alpha = value; UpdateColorFromComponents(); OnPropertyChanged(nameof(Alpha)); } }
        public string HexValue
        {
            get => $"#{ActiveColor.A:X2}{ActiveColor.R:X2}{ActiveColor.G:X2}{ActiveColor.B:X2}";
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                // Must start with # and be a valid length before attempting parse
                var cleaned = value.Trim();
                if (!cleaned.StartsWith("#")) cleaned = "#" + cleaned;
                if (cleaned.Length != 7 && cleaned.Length != 9) return;

                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(cleaned);
                    if (_isPrimaryActive) SelectedColor = color;
                    else SecondaryColor = color;
                }
                catch { }
            }
        }
        #endregion

        #region Event Handlers
        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var slider = sender as Slider;
            double relativePosition;
            double newValue;
            if (slider != null)
            {
                var point = e.GetPosition(slider);
                // Calculate the clicked value based on the slider's width and range
                relativePosition = point.X / slider.ActualWidth;
                newValue = slider.Minimum + (relativePosition * (slider.Maximum - slider.Minimum));
                slider.Value = newValue;
            }
        }
        private void HexTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            // Delay selection until focus settles
            textBox.Dispatcher.BeginInvoke((Action)(() =>
            {
                textBox.SelectAll();
            }), DispatcherPriority.ContextIdle);
        }
        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedColorPickerControl picker && e.NewValue is Color color)
            {
                if (!picker._isUpdatingFromColor)
                {
                    var isActiveColor = (picker._isPrimaryActive && e.Property == SelectedColorProperty) ||
                                        (!picker._isPrimaryActive && e.Property == SecondaryColorProperty);

                    if (isActiveColor)
                    {
                        picker.UpdateComponentsFromColor(color);
                    }
                }
                // We notify these here as well to catch changes from external bindings
                picker.OnPropertyChanged(nameof(HexValue));
                picker.OnPropertyChanged(nameof(ActiveColor));
            }
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            _isPrimaryActive = true;
            OnPropertyChanged(nameof(ActiveColor));
            ColorPickerPopup.IsOpen = true;
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            _isPrimaryActive = false;
            OnPropertyChanged(nameof(ActiveColor));
            ColorPickerPopup.IsOpen = true;
        }

        private void ColorPickerPopup_Opened(object sender, EventArgs e)
        {
            var colorToLoad = _isPrimaryActive ? SelectedColor : SecondaryColor;
            UpdateComponentsFromColor(colorToLoad);
            Dispatcher.InvokeAsync(() => UpdateSVSelectorPosition(),
        System.Windows.Threading.DispatcherPriority.Loaded);
        }

        public CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            double pointX = 0;
            double pointY = 0;

            switch (Placement)
            {
                case PickerPlacement.Bottom:
                    pointY = targetSize.Height + VerticalOffset;
                    if (CenterAlign) pointX = (targetSize.Width - popupSize.Width) / 2;
                    break;
                case PickerPlacement.Top:
                    pointY = -popupSize.Height - VerticalOffset;
                    if (CenterAlign) pointX = (targetSize.Width - popupSize.Width) / 2;
                    break;
                case PickerPlacement.Left:
                    pointX = -popupSize.Width - HorizontalOffset;
                    if (CenterAlign) pointY = (targetSize.Height - popupSize.Height) / 2;
                    break;
                case PickerPlacement.Right:
                    pointX = targetSize.Width + HorizontalOffset;
                    if (CenterAlign) pointY = (targetSize.Height - popupSize.Height) / 2;
                    break;
            }

            return new[] { new CustomPopupPlacement(new Point(pointX, pointY), PopupPrimaryAxis.Horizontal) };
        }

        private void SVCanvas_MouseDown(object sender, MouseButtonEventArgs e) => UpdateSV(e.GetPosition(SVCanvas));
        private void SVCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) UpdateSV(e.GetPosition(SVCanvas));
        }

        private void Swatch_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Background is SolidColorBrush brush)
            {
                if (_isPrimaryActive) SelectedColor = brush.Color;
                else SecondaryColor = brush.Color;
            }
        }
        #endregion

        #region Methods
        private void UpdateSV(Point position)
        {
            Saturation = Math.Clamp(position.X / SVCanvas.ActualWidth, 0, 1);
            Value = 1 - Math.Clamp(position.Y / SVCanvas.ActualHeight, 0, 1);
            OnPropertyChanged(nameof(ActiveColor));
            OnPropertyChanged(nameof(HexValue));

            // This updates the SVCanvas gradient when the Hue slider is moved.
            HueGradientStop.Color = FromHSVA(Hue, 1, 1, 1);
            _isUpdatingFromColor = false;
        }

        private void UpdateSVSelectorPosition()
        {
            // Wait for the canvas to have actual dimensions
            if (SVCanvas.ActualWidth == 0 || SVCanvas.ActualHeight == 0)
            {
                SVCanvas.Loaded -= SVCanvas_Loaded;
                SVCanvas.Loaded += SVCanvas_Loaded;
                return;
            }

            double x = Saturation * SVCanvas.ActualWidth;
            double y = (1 - Value) * SVCanvas.ActualHeight;
            SVSelectorPosition.X = x - (SVSelector.ActualWidth / 2);
            SVSelectorPosition.Y = y - (SVSelector.ActualHeight / 2);
        }
        private void SVCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSVSelectorPosition();
        }

        private static readonly IReadOnlyList<Color> DefaultSwatches = new List<Color>
        {
            (Color)ColorConverter.ConvertFromString("#c0392b"),
            (Color)ColorConverter.ConvertFromString("#e74c3c"),
            (Color)ColorConverter.ConvertFromString("#9b59b6"),
            (Color)ColorConverter.ConvertFromString("#8e44ad"),
            (Color)ColorConverter.ConvertFromString("#2980b9"),
            (Color)ColorConverter.ConvertFromString("#3498db"),
            (Color)ColorConverter.ConvertFromString("#1abc9c"),
            (Color)ColorConverter.ConvertFromString("#16a085"),
            (Color)ColorConverter.ConvertFromString("#27ae60"),
            (Color)ColorConverter.ConvertFromString("#2ecc71"),
            (Color)ColorConverter.ConvertFromString("#f1c40f"),
            (Color)ColorConverter.ConvertFromString("#f39c12"),
            (Color)ColorConverter.ConvertFromString("#e67e22"),
            (Color)ColorConverter.ConvertFromString("#d35400"),
            (Color)ColorConverter.ConvertFromString("#ffffff"),
            (Color)ColorConverter.ConvertFromString("#bdc3c7"),
            (Color)ColorConverter.ConvertFromString("#95a5a6"),
            (Color)ColorConverter.ConvertFromString("#7f8c8d"),
            (Color)ColorConverter.ConvertFromString("#34495e"),
            (Color)ColorConverter.ConvertFromString("#2c3e50"),
            (Color)ColorConverter.ConvertFromString("#000000"),
        };

        private void PopulateSwatches()
        {
            // Use the bound list when it has items; otherwise fall back to the built-in palette
            IEnumerable<Color> colors = (Swatches != null && Swatches.Count > 0)
                ? Swatches
                : DefaultSwatches;

            SwatchPanel.Children.Clear();

            foreach (var color in colors)
            {
                var borderColor = DarkenColor(color, 0.8f);

                var border = new Border
                {
                    Width = 24,
                    Height = 24,
                    CornerRadius = new CornerRadius(12),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(borderColor),
                    Margin = new Thickness(0, 0, 4, 6),
                    Background = new SolidColorBrush(color),
                    Cursor = Cursors.Hand
                };
                border.MouseLeftButtonUp += Swatch_Click;
                SwatchPanel.Children.Add(border);
            }
        }

        private void UpdateColorFromComponents()
        {
            if (_isUpdatingFromColor) return;
            _isUpdatingFromColor = true;

            Color newColor = FromHSVA(_hue, _saturation, _value, _alpha);

            if (_isPrimaryActive) SelectedColor = newColor;
            else SecondaryColor = newColor;

            // FIX: Explicitly notify that properties dependent on the color have changed.
            // This ensures the hex box, alpha slider, and color preview all update instantly.
            OnPropertyChanged(nameof(ActiveColor));
            OnPropertyChanged(nameof(HexValue));

            // This updates the SVCanvas gradient when the Hue slider is moved.
            HueGradientStop.Color = FromHSVA(Hue, 1, 1, 1);
            UpdateSVSelectorPosition();
            _isUpdatingFromColor = false;
        }

        private void UpdateComponentsFromColor(Color color)
        {
            _isUpdatingFromColor = true;

            ToHSVA(color, out _hue, out _saturation, out _value, out _alpha);

            OnPropertyChanged(nameof(Hue));
            OnPropertyChanged(nameof(Saturation));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(Alpha));
            UpdateSVSelectorPosition();

            _isUpdatingFromColor = false;
        }
        #endregion

        #region Color Conversion (HSVA)
        public static Color FromHSVA(double hue, double saturation, double value, double alpha)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));
            byte a = Convert.ToByte(alpha * 255);

            return hi switch
            {
                0 => Color.FromArgb(a, v, t, p),
                1 => Color.FromArgb(a, q, v, p),
                2 => Color.FromArgb(a, p, v, t),
                3 => Color.FromArgb(a, p, q, v),
                4 => Color.FromArgb(a, t, p, v),
                _ => Color.FromArgb(a, v, p, q),
            };
        }
        private Color DarkenColor(Color color, float factor)
        {
            factor = Math.Clamp(factor, 0, 1);
            return Color.FromArgb(
                color.A,
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor)
            );
        }
        public static void ToHSVA(Color color, out double hue, out double saturation, out double value, out double alpha)
        {
            double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;
            alpha = color.A / 255.0;
            double min = Math.Min(r, Math.Min(g, b)), max = Math.Max(r, Math.Max(g, b));
            value = max;
            double delta = max - min;

            if (delta < 0.00001) { hue = 0; saturation = 0; return; }
            if (max > 0.0) { saturation = (delta / max); }
            else { saturation = 0.0; hue = 0.0; return; }

            if (r >= max) hue = (g - b) / delta;
            else if (g >= max) hue = 2.0 + (b - r) / delta;
            else hue = 4.0 + (r - g) / delta;

            hue *= 60.0;
            if (hue < 0.0) hue += 360.0;
        }
        #endregion

    }




    #region Converters
    // A simple converter to turn a Color into a SolidColorBrush
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
                return new SolidColorBrush(color);
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // A multi-value converter to calculate the popup's centered offset
    public class CenteredOffsetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Expected values:
            // values[0]: (bool) PickerCentered
            // values[1]: (double) Target element size (width or height)
            // values[2]: (double) Popup size (width or height)
            // values[3]: (PlacementMode) PickerPosition
            if (values.Length < 4 ||
                !(values[0] is bool isCentered) ||
                !(values[1] is double targetSize) ||
                !(values[2] is double popupSize) ||
                !(values[3] is PlacementMode position))
            {
                return 0.0;
            }

            if (!isCentered || targetSize == 0 || popupSize == 0) return 0.0;

            string axis = parameter as string;

            // Calculate horizontal offset only for Top or Bottom placement
            if (axis == "X" && (position == PlacementMode.Top || position == PlacementMode.Bottom || position == PlacementMode.Center))
            {
                return (targetSize - popupSize) / 2;
            }

            // Calculate vertical offset only for Left or Right placement
            if (axis == "Y" && (position == PlacementMode.Left || position == PlacementMode.Right))
            {
                return (targetSize - popupSize) / 2;
            }

            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = false;
            if (value is bool b)
            {
                boolValue = b;
            }

            // Invert visibility if "invert" parameter is passed
            if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
            {
                boolValue = !boolValue;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool boolValue = (visibility == Visibility.Visible);
                if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
                {
                    boolValue = !boolValue;
                }
                return boolValue;
            }
            return false;
        }
    }

    public class OpaqueColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                // Return the same color but with full alpha (255)
                return Color.FromArgb(255, color.R, color.G, color.B);
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

}
