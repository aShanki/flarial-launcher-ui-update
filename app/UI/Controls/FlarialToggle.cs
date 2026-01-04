using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Flarial.Launcher.UI.Theme;

namespace Flarial.Launcher.UI.Controls;

/// <summary>
/// Discord/Spotify-style toggle switch with smooth animations.
/// </summary>
class FlarialToggle : Border
{
    readonly Border _track;
    readonly Border _thumb;
    readonly TranslateTransform _thumbTransform;
    readonly SolidColorBrush _trackBrush;

    bool _isOn;

    public event EventHandler? Toggled;

    public bool IsOn
    {
        get => _isOn;
        set
        {
            if (_isOn != value)
            {
                _isOn = value;
                AnimateToState(_isOn);
                Toggled?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public FlarialToggle()
    {
        Width = 44;
        Height = 24;
        Cursor = Cursors.Hand;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        _trackBrush = FlarialTheme.CreateAnimatableBrush(FlarialTheme.Surface);
        _track = new Border
        {
            CornerRadius = new CornerRadius(12),
            Background = _trackBrush,
            BorderBrush = FlarialTheme.BorderSubtleBrush,
            BorderThickness = new Thickness(1)
        };

        _thumbTransform = new TranslateTransform(2, 0);

        _thumb = new Border
        {
            Width = 18,
            Height = 18,
            CornerRadius = new CornerRadius(9),
            Background = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(2, 0, 0, 0),
            RenderTransform = _thumbTransform,
            Effect = new DropShadowEffect
            {
                BlurRadius = 4,
                ShadowDepth = 1,
                Opacity = 0.3,
                Color = Colors.Black
            }
        };

        var grid = new Grid();
        grid.Children.Add(_track);
        grid.Children.Add(_thumb);
        Child = grid;

        MouseLeftButtonUp += OnClick;
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        IsOn = !_isOn;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (!_isOn)
        {
            var colorAnim = new ColorAnimation(FlarialTheme.SurfaceHover, FlarialTheme.FastDuration);
            _trackBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (!_isOn)
        {
            var colorAnim = new ColorAnimation(FlarialTheme.Surface, FlarialTheme.FastDuration);
            _trackBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
        }
    }

    private void AnimateToState(bool isOn)
    {
        double targetX = isOn ? 20 : 0;

        var thumbAnim = new DoubleAnimation(targetX, FlarialTheme.FastDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        _thumbTransform.BeginAnimation(TranslateTransform.XProperty, thumbAnim);

        Color targetColor = isOn ? FlarialTheme.AccentPrimary : FlarialTheme.Surface;

        var colorAnim = new ColorAnimation(targetColor, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        _trackBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

        _track.BorderBrush = isOn ? FlarialTheme.TransparentBrush : FlarialTheme.BorderSubtleBrush;
    }

    /// <summary>
    /// Sets the toggle state without triggering the Toggled event.
    /// </summary>
    public void SetState(bool isOn, bool animate = true)
    {
        if (_isOn != isOn)
        {
            _isOn = isOn;
            if (animate)
            {
                AnimateToState(_isOn);
            }
            else
            {
                _thumbTransform.X = isOn ? 20 : 0;
                _trackBrush.Color = isOn ? FlarialTheme.AccentPrimary : FlarialTheme.Surface;
                _track.BorderBrush = isOn ? FlarialTheme.TransparentBrush : FlarialTheme.BorderSubtleBrush;
            }
        }
    }
}

class FlarialToggleOption : Grid
{
    readonly FlarialToggle _toggle;
    readonly TextBlock _label;
    readonly TextBlock _description;

    public event EventHandler? Toggled;

    public bool IsOn
    {
        get => _toggle.IsOn;
        set => _toggle.IsOn = value;
    }

    public string Label
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    public string Description
    {
        get => _description.Text;
        set
        {
            _description.Text = value;
            _description.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public FlarialToggleOption()
    {
        ColumnDefinitions.Add(new ColumnDefinition());
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var textStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, FlarialTheme.SpacingLg, 0)
        };

        _label = new TextBlock
        {
            FontSize = FlarialTheme.FontSizeBody,
            Foreground = FlarialTheme.TextPrimaryBrush
        };
        textStack.Children.Add(_label);

        _description = new TextBlock
        {
            FontSize = FlarialTheme.FontSizeSmall,
            Foreground = FlarialTheme.TextSecondaryBrush,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 0),
            Visibility = Visibility.Collapsed
        };
        textStack.Children.Add(_description);

        SetColumn(textStack, 0);
        Children.Add(textStack);

        _toggle = new FlarialToggle
        {
            VerticalAlignment = VerticalAlignment.Center
        };
        _toggle.Toggled += (s, e) => Toggled?.Invoke(this, e);

        SetColumn(_toggle, 1);
        Children.Add(_toggle);
    }

    public FlarialToggleOption(string label, string description, bool isOn) : this()
    {
        Label = label;
        Description = description;
        _toggle.SetState(isOn, animate: false);
    }
}
