using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Flarial.Launcher.Interface.Theme;

namespace Flarial.Launcher.Interface.Controls;

class FlarialButton : Border
{
    public enum ButtonVariant { Primary, Secondary, Ghost }

    readonly ContentPresenter _content;
    readonly DropShadowEffect _glowEffect;
    readonly ScaleTransform _scaleTransform;
    readonly SolidColorBrush _backgroundBrush;

    ButtonVariant _variant = ButtonVariant.Primary;
    bool _isPressed;

    public event RoutedEventHandler? Click;

    public ButtonVariant Variant
    {
        get => _variant;
        set
        {
            _variant = value;
            ApplyVariantStyle();
        }
    }

    public object? ButtonContent
    {
        get => _content.Content;
        set => _content.Content = value;
    }

    public object? Content
    {
        get => ButtonContent;
        set => ButtonContent = value;
    }

    public FlarialButton()
    {
        _scaleTransform = new ScaleTransform(1, 1);
        RenderTransform = _scaleTransform;
        RenderTransformOrigin = new Point(0.5, 0.5);

        _glowEffect = new DropShadowEffect
        {
            Color = FlarialTheme.AccentPrimary,
            BlurRadius = 0,
            ShadowDepth = 0,
            Opacity = 0,
            Direction = 0
        };
        Effect = _glowEffect;

        _backgroundBrush = FlarialTheme.CreateAnimatableBrush(FlarialTheme.AccentPrimary);
        Background = _backgroundBrush;
        CornerRadius = new CornerRadius(FlarialTheme.MediumRadius);
        Padding = new Thickness(FlarialTheme.SpacingLg, FlarialTheme.SpacingMd, FlarialTheme.SpacingLg, FlarialTheme.SpacingMd);
        Cursor = Cursors.Hand;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        _content = new ContentPresenter
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Child = _content;

        ApplyVariantStyle();

        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
        MouseLeftButtonDown += OnMouseDown;
        MouseLeftButtonUp += OnMouseUp;
    }

    private void ApplyVariantStyle()
    {
        switch (_variant)
        {
            case ButtonVariant.Primary:
                _backgroundBrush.Color = FlarialTheme.AccentPrimary;
                BorderBrush = null;
                BorderThickness = new Thickness(0);
                _glowEffect.Color = FlarialTheme.AccentPrimary;
                break;

            case ButtonVariant.Secondary:
                _backgroundBrush.Color = FlarialTheme.Surface;
                BorderBrush = FlarialTheme.BorderSubtleBrush;
                BorderThickness = new Thickness(1);
                _glowEffect.Color = FlarialTheme.BorderMedium;
                break;

            case ButtonVariant.Ghost:
                _backgroundBrush.Color = Color.FromArgb(0, FlarialTheme.SurfaceHover.R, FlarialTheme.SurfaceHover.G, FlarialTheme.SurfaceHover.B);
                BorderBrush = null;
                BorderThickness = new Thickness(0);
                _glowEffect.Color = FlarialTheme.BorderMedium;
                break;
        }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        AnimateGlow(true);

        Color hoverColor = _variant switch
        {
            ButtonVariant.Primary => FlarialTheme.AccentHover,
            ButtonVariant.Secondary => FlarialTheme.SurfaceHover,
            ButtonVariant.Ghost => FlarialTheme.SurfaceHover,
            _ => FlarialTheme.AccentHover
        };

        AnimateBackgroundColor(hoverColor);
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        _isPressed = false;

        AnimateGlow(false);

        Color normalColor = _variant switch
        {
            ButtonVariant.Primary => FlarialTheme.AccentPrimary,
            ButtonVariant.Secondary => FlarialTheme.Surface,
            ButtonVariant.Ghost => Color.FromArgb(0, FlarialTheme.SurfaceHover.R, FlarialTheme.SurfaceHover.G, FlarialTheme.SurfaceHover.B),
            _ => FlarialTheme.AccentPrimary
        };

        AnimateBackgroundColor(normalColor);
        AnimateScale(1.0);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isPressed = true;

        AnimateScale(0.96);

        Color pressedColor = _variant switch
        {
            ButtonVariant.Primary => FlarialTheme.AccentPressed,
            ButtonVariant.Secondary => FlarialTheme.SurfaceActive,
            ButtonVariant.Ghost => FlarialTheme.SurfaceActive,
            _ => FlarialTheme.AccentPressed
        };

        AnimateBackgroundColor(pressedColor);
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isPressed) return;
        _isPressed = false;

        AnimateScale(1.0);

        Color hoverColor = _variant switch
        {
            ButtonVariant.Primary => FlarialTheme.AccentHover,
            ButtonVariant.Secondary => FlarialTheme.SurfaceHover,
            ButtonVariant.Ghost => FlarialTheme.SurfaceHover,
            _ => FlarialTheme.AccentHover
        };

        AnimateBackgroundColor(hoverColor);

        Click?.Invoke(this, new RoutedEventArgs());
    }

    private void AnimateGlow(bool show)
    {
        if (_variant != ButtonVariant.Primary)
        {
            return;
        }

        double targetBlur = show ? 20 : 0;
        double targetOpacity = show ? 0.6 : 0;

        var blurAnim = new DoubleAnimation(targetBlur, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var opacityAnim = new DoubleAnimation(targetOpacity, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        _glowEffect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, blurAnim);
        _glowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnim);
    }

    private void AnimateScale(double targetScale)
    {
        var scaleAnim = new DoubleAnimation(targetScale, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        _scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        _scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
    }

    private void AnimateBackgroundColor(Color targetColor)
    {
        var colorAnim = new ColorAnimation(targetColor, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        _backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
    }
}
