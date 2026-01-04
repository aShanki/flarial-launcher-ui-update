using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Flarial.Launcher.Interface.Theme;

static class FlarialTheme
{
    public static readonly Color BackgroundDark = Color.FromRgb(13, 13, 13);
    public static readonly Color BackgroundMedium = Color.FromRgb(18, 18, 18);
    public static readonly Color BackgroundLight = Color.FromRgb(24, 24, 24);

    public static readonly Color Surface = Color.FromRgb(30, 30, 30);
    public static readonly Color SurfaceHover = Color.FromRgb(40, 40, 40);
    public static readonly Color SurfaceActive = Color.FromRgb(50, 50, 50);
    public static readonly Color SurfaceSelected = Color.FromRgb(45, 45, 45);

    public static readonly Color AccentPrimary = Color.FromRgb(255, 36, 56);
    public static readonly Color AccentHover = Color.FromRgb(255, 60, 80);
    public static readonly Color AccentPressed = Color.FromRgb(200, 28, 44);
    public static readonly Color AccentGlow = Color.FromArgb(100, 255, 36, 56);

    public static readonly Color TextPrimary = Color.FromArgb(222, 255, 255, 255);
    public static readonly Color TextSecondary = Color.FromArgb(153, 255, 255, 255);
    public static readonly Color TextDisabled = Color.FromArgb(97, 255, 255, 255);
    public static readonly Color TextOnAccent = Colors.White;

    public static readonly Color BorderSubtle = Color.FromRgb(51, 51, 51);
    public static readonly Color BorderMedium = Color.FromRgb(68, 68, 68);

    public static readonly SolidColorBrush BackgroundDarkBrush;
    public static readonly SolidColorBrush BackgroundMediumBrush;
    public static readonly SolidColorBrush BackgroundLightBrush;
    public static readonly SolidColorBrush SurfaceBrush;
    public static readonly SolidColorBrush SurfaceHoverBrush;
    public static readonly SolidColorBrush SurfaceActiveBrush;
    public static readonly SolidColorBrush SurfaceSelectedBrush;
    public static readonly SolidColorBrush AccentBrush;
    public static readonly SolidColorBrush AccentHoverBrush;
    public static readonly SolidColorBrush AccentPressedBrush;
    public static readonly SolidColorBrush TextPrimaryBrush;
    public static readonly SolidColorBrush TextSecondaryBrush;
    public static readonly SolidColorBrush TextDisabledBrush;
    public static readonly SolidColorBrush TextOnAccentBrush;
    public static readonly SolidColorBrush BorderSubtleBrush;
    public static readonly SolidColorBrush BorderMediumBrush;
    public static readonly SolidColorBrush TransparentBrush;

    public static readonly LinearGradientBrush BackgroundGradient;
    public static readonly LinearGradientBrush AccentGradient;

    public static readonly Duration FastDuration = new(TimeSpan.FromMilliseconds(100));
    public static readonly Duration NormalDuration = new(TimeSpan.FromMilliseconds(200));
    public static readonly Duration SlowDuration = new(TimeSpan.FromMilliseconds(350));

    public const double SmallRadius = 4;
    public const double MediumRadius = 8;
    public const double LargeRadius = 12;
    public const double WindowRadius = 12;

    public const double SpacingXs = 4;
    public const double SpacingSm = 8;
    public const double SpacingMd = 12;
    public const double SpacingLg = 16;
    public const double SpacingXl = 24;

    public const double FontSizeSmall = 12;
    public const double FontSizeBody = 14;
    public const double FontSizeMedium = 16;
    public const double FontSizeLarge = 20;
    public const double FontSizeTitle = 24;

    public const double NavRailWidth = 200;
    public const double NavItemHeight = 40;

    static FlarialTheme()
    {
        BackgroundDarkBrush = CreateFrozenBrush(BackgroundDark);
        BackgroundMediumBrush = CreateFrozenBrush(BackgroundMedium);
        BackgroundLightBrush = CreateFrozenBrush(BackgroundLight);
        SurfaceBrush = CreateFrozenBrush(Surface);
        SurfaceHoverBrush = CreateFrozenBrush(SurfaceHover);
        SurfaceActiveBrush = CreateFrozenBrush(SurfaceActive);
        SurfaceSelectedBrush = CreateFrozenBrush(SurfaceSelected);
        AccentBrush = CreateFrozenBrush(AccentPrimary);
        AccentHoverBrush = CreateFrozenBrush(AccentHover);
        AccentPressedBrush = CreateFrozenBrush(AccentPressed);
        TextPrimaryBrush = CreateFrozenBrush(TextPrimary);
        TextSecondaryBrush = CreateFrozenBrush(TextSecondary);
        TextDisabledBrush = CreateFrozenBrush(TextDisabled);
        TextOnAccentBrush = CreateFrozenBrush(TextOnAccent);
        BorderSubtleBrush = CreateFrozenBrush(BorderSubtle);
        BorderMediumBrush = CreateFrozenBrush(BorderMedium);
        TransparentBrush = CreateFrozenBrush(Colors.Transparent);

        BackgroundGradient = new LinearGradientBrush
        {
            StartPoint = new Point(0.5, 0),
            EndPoint = new Point(0.5, 1),
            GradientStops = new GradientStopCollection
            {
                new(BackgroundLight, 0),
                new(BackgroundDark, 1)
            }
        };
        BackgroundGradient.Freeze();

        AccentGradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5),
            GradientStops = new GradientStopCollection
            {
                new(AccentPrimary, 0),
                new(Color.FromRgb(255, 80, 100), 1)
            }
        };
        AccentGradient.Freeze();
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    public static SolidColorBrush CreateAnimatableBrush(Color color) => new(color);

    public static DropShadowEffect CreateGlowEffect(Color color, double blurRadius = 15, double opacity = 0.5)
    {
        return new DropShadowEffect
        {
            Color = color,
            BlurRadius = blurRadius,
            ShadowDepth = 0,
            Opacity = opacity,
            Direction = 0
        };
    }

    public static DropShadowEffect CreateShadowEffect(double blurRadius = 20, double opacity = 0.4)
    {
        return new DropShadowEffect
        {
            Color = Colors.Black,
            BlurRadius = blurRadius,
            ShadowDepth = 2,
            Opacity = opacity,
            Direction = 270
        };
    }

    public static DropShadowEffect CreateAccentGlow(double blurRadius = 20)
    {
        return CreateGlowEffect(AccentPrimary, blurRadius, 0.6);
    }
}
