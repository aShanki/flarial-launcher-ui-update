using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Flarial.Launcher.Interface.Theme;

namespace Flarial.Launcher.Interface.Controls;

class FlarialProgressBar : Grid
{
    readonly Border _track;
    readonly Border _fill;
    readonly LinearGradientBrush _fillGradient;
    readonly DropShadowEffect _glowEffect;

    double _value;
    bool _isIndeterminate;

    public double Value
    {
        get => _value;
        set
        {
            _value = Math.Max(0, Math.Min(100, value));
            AnimateToValue(_value);
        }
    }

    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        set
        {
            _isIndeterminate = value;
            if (value)
                StartIndeterminateAnimation();
            else
                StopIndeterminateAnimation();
        }
    }

    public FlarialProgressBar()
    {
        Height = 6;
        MinWidth = 100;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        _track = new Border
        {
            CornerRadius = new CornerRadius(3),
            Background = FlarialTheme.SurfaceBrush
        };
        Children.Add(_track);

        _fillGradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5),
            GradientStops = new GradientStopCollection
            {
                new(FlarialTheme.AccentPrimary, 0),
                new(Color.FromRgb(255, 100, 120), 1)
            }
        };

        _glowEffect = new DropShadowEffect
        {
            Color = FlarialTheme.AccentPrimary,
            BlurRadius = 8,
            ShadowDepth = 0,
            Opacity = 0.5,
            Direction = 0
        };

        _fill = new Border
        {
            CornerRadius = new CornerRadius(3),
            Background = _fillGradient,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = 0,
            Effect = _glowEffect
        };
        Children.Add(_fill);

        SizeChanged += (s, e) => UpdateFillWidth();
    }

    private void UpdateFillWidth()
    {
        if (!_isIndeterminate)
        {
            _fill.Width = ActualWidth * (_value / 100);
        }
    }

    private void AnimateToValue(double value)
    {
        if (_isIndeterminate || ActualWidth == 0) return;

        double targetWidth = ActualWidth * (value / 100);

        var widthAnim = new DoubleAnimation(targetWidth, FlarialTheme.NormalDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        _fill.BeginAnimation(WidthProperty, widthAnim);

        if (value > 0 && value < 100)
        {
            PulseGlow();
        }
    }

    private void PulseGlow()
    {
        var glowAnim = new DoubleAnimation
        {
            From = 8,
            To = 15,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            EasingFunction = new QuadraticEase()
        };

        _glowEffect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, glowAnim);
    }

    private void StartIndeterminateAnimation()
    {
        _fill.Width = ActualWidth * 0.3;
        _fill.HorizontalAlignment = HorizontalAlignment.Left;

        var animation = new DoubleAnimation
        {
            From = -_fill.Width,
            To = ActualWidth,
            Duration = TimeSpan.FromMilliseconds(1500),
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var transform = new TranslateTransform();
        _fill.RenderTransform = transform;
        transform.BeginAnimation(TranslateTransform.XProperty, animation);
    }

    private void StopIndeterminateAnimation()
    {
        _fill.RenderTransform = null;
        _fill.Width = 0;
        _fill.HorizontalAlignment = HorizontalAlignment.Left;
    }

    public void SetProgress(double value, TimeSpan? duration = null)
    {
        _value = Math.Max(0, Math.Min(100, value));

        if (_isIndeterminate || ActualWidth == 0) return;

        double targetWidth = ActualWidth * (_value / 100);

        var widthAnim = new DoubleAnimation(targetWidth, duration ?? FlarialTheme.NormalDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        _fill.BeginAnimation(WidthProperty, widthAnim);
    }

    public void Complete()
    {
        SetProgress(100, TimeSpan.FromMilliseconds(300));

        var glowAnim = new DoubleAnimation
        {
            From = 8,
            To = 25,
            Duration = TimeSpan.FromMilliseconds(200),
            AutoReverse = true,
            EasingFunction = new QuadraticEase()
        };

        _glowEffect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, glowAnim);
    }
}
