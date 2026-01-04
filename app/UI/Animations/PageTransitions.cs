using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Flarial.Launcher.UI.Theme;

namespace Flarial.Launcher.UI.Animations;

/// <summary>
/// Page transition animations for smooth navigation effects.
/// </summary>
static class PageTransitions
{
    public enum SlideDirection { Left, Right, Up, Down }

    /// <summary>
    /// Slides an element into view with fade effect.
    /// </summary>
    public static void SlideIn(FrameworkElement element, SlideDirection direction = SlideDirection.Right, Action? onComplete = null)
    {
        if (element == null) return;

        element.Visibility = Visibility.Visible;

        // Setup transform
        var transform = new TranslateTransform();
        element.RenderTransform = transform;

        // Calculate start offset
        double startX = 0, startY = 0;
        switch (direction)
        {
            case SlideDirection.Right:
                startX = 30;
                break;
            case SlideDirection.Left:
                startX = -30;
                break;
            case SlideDirection.Up:
                startY = -30;
                break;
            case SlideDirection.Down:
                startY = 30;
                break;
        }

        transform.X = startX;
        transform.Y = startY;
        element.Opacity = 0;

        // Slide animation
        var slideX = new DoubleAnimation(startX, 0, FlarialTheme.NormalDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var slideY = new DoubleAnimation(startY, 0, FlarialTheme.NormalDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        // Fade animation
        var fade = new DoubleAnimation(0, 1, FlarialTheme.NormalDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        if (onComplete != null)
        {
            fade.Completed += (s, e) => onComplete();
        }

        transform.BeginAnimation(TranslateTransform.XProperty, slideX);
        transform.BeginAnimation(TranslateTransform.YProperty, slideY);
        element.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    /// <summary>
    /// Slides an element out of view with fade effect.
    /// </summary>
    public static void SlideOut(FrameworkElement element, SlideDirection direction = SlideDirection.Left, Action? onComplete = null)
    {
        if (element == null) return;

        // Setup transform
        var transform = element.RenderTransform as TranslateTransform ?? new TranslateTransform();
        element.RenderTransform = transform;

        // Calculate end offset
        double endX = 0, endY = 0;
        switch (direction)
        {
            case SlideDirection.Right:
                endX = 30;
                break;
            case SlideDirection.Left:
                endX = -30;
                break;
            case SlideDirection.Up:
                endY = -30;
                break;
            case SlideDirection.Down:
                endY = 30;
                break;
        }

        // Slide animation
        var slideX = new DoubleAnimation(endX, FlarialTheme.FastDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var slideY = new DoubleAnimation(endY, FlarialTheme.FastDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        // Fade animation
        var fade = new DoubleAnimation(0, FlarialTheme.FastDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        fade.Completed += (s, e) =>
        {
            element.Visibility = Visibility.Collapsed;
            onComplete?.Invoke();
        };

        transform.BeginAnimation(TranslateTransform.XProperty, slideX);
        transform.BeginAnimation(TranslateTransform.YProperty, slideY);
        element.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    /// <summary>
    /// Fades out the current element and fades in the new element.
    /// </summary>
    public static void FadeSwitch(FrameworkElement outElement, FrameworkElement inElement, SlideDirection direction = SlideDirection.Right)
    {
        SlideOut(outElement, SlideDirection.Left, () =>
        {
            SlideIn(inElement, direction);
        });
    }

    /// <summary>
    /// Simple fade in animation.
    /// </summary>
    public static void FadeIn(FrameworkElement element, Action? onComplete = null)
    {
        if (element == null) return;

        element.Visibility = Visibility.Visible;
        element.Opacity = 0;

        var fade = new DoubleAnimation(0, 1, FlarialTheme.NormalDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        if (onComplete != null)
        {
            fade.Completed += (s, e) => onComplete();
        }

        element.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    /// <summary>
    /// Simple fade out animation.
    /// </summary>
    public static void FadeOut(FrameworkElement element, Action? onComplete = null)
    {
        if (element == null) return;

        var fade = new DoubleAnimation(0, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        fade.Completed += (s, e) =>
        {
            element.Visibility = Visibility.Collapsed;
            onComplete?.Invoke();
        };

        element.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    /// <summary>
    /// Scale and fade in animation (for dialogs).
    /// </summary>
    public static void ScaleIn(FrameworkElement element, Action? onComplete = null)
    {
        if (element == null) return;

        element.Visibility = Visibility.Visible;
        element.Opacity = 0;

        var scale = new ScaleTransform(0.9, 0.9);
        element.RenderTransform = scale;
        element.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnim = new DoubleAnimation(0.9, 1, FlarialTheme.NormalDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var fade = new DoubleAnimation(0, 1, FlarialTheme.NormalDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        if (onComplete != null)
        {
            fade.Completed += (s, e) => onComplete();
        }

        scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        element.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    /// <summary>
    /// Scale and fade out animation (for dialogs).
    /// </summary>
    public static void ScaleOut(FrameworkElement element, Action? onComplete = null)
    {
        if (element == null) return;

        var scale = element.RenderTransform as ScaleTransform ?? new ScaleTransform(1, 1);
        element.RenderTransform = scale;
        element.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnim = new DoubleAnimation(0.9, FlarialTheme.FastDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var fade = new DoubleAnimation(0, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        fade.Completed += (s, e) =>
        {
            element.Visibility = Visibility.Collapsed;
            onComplete?.Invoke();
        };

        scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        element.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    /// <summary>
    /// Staggered animation for multiple elements.
    /// </summary>
    public static void StaggerSlideIn(FrameworkElement[] elements, SlideDirection direction = SlideDirection.Up, int delayMs = 50)
    {
        for (int i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            var delay = TimeSpan.FromMilliseconds(i * delayMs);

            element.Visibility = Visibility.Visible;
            element.Opacity = 0;

            var transform = new TranslateTransform { Y = 20 };
            element.RenderTransform = transform;

            var slide = new DoubleAnimation(20, 0, FlarialTheme.NormalDuration)
            {
                BeginTime = delay,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var fade = new DoubleAnimation(0, 1, FlarialTheme.NormalDuration)
            {
                BeginTime = delay,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            transform.BeginAnimation(TranslateTransform.YProperty, slide);
            element.BeginAnimation(UIElement.OpacityProperty, fade);
        }
    }
}
