using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Flarial.Launcher.Interface.Theme;

namespace Flarial.Launcher.Interface.Controls;

class FlarialCard : Border
{
    readonly DropShadowEffect _shadowEffect;
    readonly SolidColorBrush _backgroundBrush;

    bool _isSelected;
    bool _isClickable;
    bool _hasBackground = true;
    bool _enableHover = true;

    public event RoutedEventHandler? Click;

    public bool EnableHover
    {
        get => _enableHover;
        set => _enableHover = value;
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            ApplySelectedState();
        }
    }

    public bool IsClickable
    {
        get => _isClickable;
        set
        {
            _isClickable = value;
            Cursor = value ? Cursors.Hand : Cursors.Arrow;
        }
    }

    public bool HasBackground
    {
        get => _hasBackground;
        set
        {
            _hasBackground = value;
            if (!value)
            {
                _backgroundBrush.Color = Colors.Transparent;
                Effect = null;
            }
        }
    }

    public new UIElement? Child
    {
        get => base.Child;
        set => base.Child = value;
    }

    public FlarialCard()
    {
        CornerRadius = new CornerRadius(FlarialTheme.MediumRadius);
        _backgroundBrush = FlarialTheme.CreateAnimatableBrush(FlarialTheme.Surface);
        Background = _backgroundBrush;
        BorderBrush = FlarialTheme.BorderSubtleBrush;
        BorderThickness = new Thickness(1);
        Padding = new Thickness(FlarialTheme.SpacingLg);
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        _shadowEffect = new DropShadowEffect
        {
            BlurRadius = 0,
            ShadowDepth = 2,
            Color = Colors.Black,
            Opacity = 0,
            Direction = 270
        };
        Effect = _shadowEffect;

        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
        MouseLeftButtonUp += OnMouseUp;
    }

    private void ApplySelectedState()
    {
        if (_isSelected)
        {
            var colorAnim = new ColorAnimation(FlarialTheme.SurfaceSelected, FlarialTheme.FastDuration);
            _backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            BorderBrush = FlarialTheme.AccentBrush;
        }
        else
        {
            var colorAnim = new ColorAnimation(FlarialTheme.Surface, FlarialTheme.FastDuration);
            _backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            BorderBrush = FlarialTheme.BorderSubtleBrush;
        }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (!_hasBackground || !_enableHover) return;

        AnimateShadow(true);

        if (!_isSelected)
        {
            var colorAnim = new ColorAnimation(FlarialTheme.SurfaceHover, FlarialTheme.FastDuration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            _backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (!_hasBackground || !_enableHover) return;

        AnimateShadow(false);

        Color targetColor = _isSelected ? FlarialTheme.SurfaceSelected : FlarialTheme.Surface;
        var colorAnim = new ColorAnimation(targetColor, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        _backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isClickable)
        {
            Click?.Invoke(this, new RoutedEventArgs());
        }
    }

    private void AnimateShadow(bool show)
    {
        double targetBlur = show ? 15 : 0;
        double targetOpacity = show ? 0.3 : 0;

        var blurAnim = new DoubleAnimation(targetBlur, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var opacityAnim = new DoubleAnimation(targetOpacity, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        _shadowEffect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, blurAnim);
        _shadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnim);
    }

    public static FlarialCard CreateWithHeader(string header, UIElement content)
    {
        var card = new FlarialCard();

        var stack = new StackPanel();

        var headerText = new TextBlock
        {
            Text = header,
            FontSize = FlarialTheme.FontSizeBody,
            FontWeight = FontWeights.SemiBold,
            Foreground = FlarialTheme.TextSecondaryBrush
        };
        stack.Children.Add(headerText);
        stack.Children.Add(content);

        card.Child = stack;
        return card;
    }
}
