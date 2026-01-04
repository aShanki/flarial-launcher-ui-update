using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Flarial.Launcher.App;
using Flarial.Launcher.UI.Theme;
using ModernWpf;

namespace Flarial.Launcher.UI;

sealed class MainWindow : Window
{
    readonly Border _windowBorder;
    readonly Grid _titleBar;

    internal MainWindow(Configuration configuration)
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;

        ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);

        Icon = ApplicationManifest.Icon;
        Title = "Flarial Launcher";
        Width = 960;
        Height = 540;
        UseLayoutRounding = SnapsToDevicePixels = true;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _windowBorder = new Border
        {
            CornerRadius = new CornerRadius(FlarialTheme.WindowRadius),
            Background = FlarialTheme.BackgroundGradient,
            BorderBrush = FlarialTheme.BorderSubtleBrush,
            BorderThickness = new Thickness(1),
            Effect = FlarialTheme.CreateShadowEffect(30, 0.6),
            ClipToBounds = true
        };

        var clipBorder = new Border
        {
            CornerRadius = new CornerRadius(FlarialTheme.WindowRadius),
            Background = Brushes.Transparent
        };

        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(44) });
        mainGrid.RowDefinitions.Add(new RowDefinition());

        _titleBar = CreateTitleBar();
        Grid.SetRow(_titleBar, 0);
        mainGrid.Children.Add(_titleBar);

        var content = new MainWindowContent(configuration) { IsEnabled = false };
        Grid.SetRow(content, 1);
        mainGrid.Children.Add(content);

        clipBorder.Child = mainGrid;
        _windowBorder.Child = clipBorder;
        Content = _windowBorder;

        Loaded += OnLoaded;

        Opacity = 0;
        _windowBorder.RenderTransform = new ScaleTransform(0.95, 0.95);
        _windowBorder.RenderTransformOrigin = new Point(0.5, 0.5);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var fadeIn = new DoubleAnimation(0, 1, FlarialTheme.NormalDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var scaleIn = new DoubleAnimation(0.95, 1, FlarialTheme.NormalDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        BeginAnimation(OpacityProperty, fadeIn);
        ((ScaleTransform)_windowBorder.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleIn);
        ((ScaleTransform)_windowBorder.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleIn);
    }

    private Grid CreateTitleBar()
    {
        var titleBar = new Grid
        {
            Background = FlarialTheme.TransparentBrush
        };

        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(FlarialTheme.SpacingLg) });
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        titleBar.ColumnDefinitions.Add(new ColumnDefinition());
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var iconImage = new Image
        {
            Source = ApplicationManifest.Icon,
            Width = 18,
            Height = 18,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, FlarialTheme.SpacingSm, 0)
        };
        Grid.SetColumn(iconImage, 1);
        titleBar.Children.Add(iconImage);

        var titleText = new TextBlock
        {
            Text = "Flarial Launcher",
            FontSize = FlarialTheme.FontSizeBody,
            Foreground = FlarialTheme.TextSecondaryBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0)
        };
        Grid.SetColumn(titleText, 2);
        titleBar.Children.Add(titleText);

        var dragArea = new Border
        {
            Background = Brushes.Transparent
        };
        dragArea.MouseLeftButtonDown += (s, e) =>
        {
            if (e.ClickCount == 1)
                DragMove();
        };
        Grid.SetColumn(dragArea, 2);
        Grid.SetColumnSpan(dragArea, 1);
        titleBar.Children.Add(dragArea);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, FlarialTheme.SpacingSm, 0)
        };

        buttonPanel.Children.Add(CreateWindowButton(CreateMinimizeIcon(), OnMinimize, false));
        buttonPanel.Children.Add(CreateWindowButton(CreateCloseIcon(), OnClose, true));

        Grid.SetColumn(buttonPanel, 3);
        titleBar.Children.Add(buttonPanel);

        return titleBar;
    }

    private Border CreateWindowButton(Path icon, Action onClick, bool isClose)
    {
        var normalBg = FlarialTheme.TransparentBrush;
        var hoverBg = isClose
            ? new SolidColorBrush(Color.FromRgb(232, 17, 35))
            : FlarialTheme.SurfaceHoverBrush;

        var button = new Border
        {
            Width = 46,
            Height = 30,
            Background = normalBg,
            CornerRadius = new CornerRadius(0),
            Cursor = Cursors.Hand,
            Child = icon
        };

        button.MouseEnter += (s, e) =>
        {
            button.Background = hoverBg;
            if (isClose)
                icon.Stroke = Brushes.White;
        };

        button.MouseLeave += (s, e) =>
        {
            button.Background = normalBg;
            icon.Stroke = FlarialTheme.TextSecondaryBrush;
        };

        button.MouseLeftButtonUp += (s, e) => onClick();

        return button;
    }

    private Path CreateMinimizeIcon()
    {
        return new Path
        {
            Data = Geometry.Parse("M 0,0 L 10,0"),
            Stroke = FlarialTheme.TextSecondaryBrush,
            StrokeThickness = 1,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private Path CreateCloseIcon()
    {
        return new Path
        {
            Data = Geometry.Parse("M 0,0 L 10,10 M 10,0 L 0,10"),
            Stroke = FlarialTheme.TextSecondaryBrush,
            StrokeThickness = 1,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private void OnMinimize() => WindowState = WindowState.Minimized;

    private void OnClose()
    {
        var fadeOut = new DoubleAnimation(1, 0, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        fadeOut.Completed += (s, e) => Close();

        var scaleOut = new DoubleAnimation(1, 0.95, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        BeginAnimation(OpacityProperty, fadeOut);
        ((ScaleTransform)_windowBorder.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleOut);
        ((ScaleTransform)_windowBorder.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleOut);
    }
}
