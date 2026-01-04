using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Flarial.Launcher.UI.Theme;

namespace Flarial.Launcher.UI.Controls;

/// <summary>
/// A modern modal dialog with scale/fade animations.
/// </summary>
class FlarialDialog : Grid
{
    readonly Border _overlay;
    readonly Border _dialogCard;
    readonly TextBlock _titleText;
    readonly TextBlock _messageText;
    readonly StackPanel _buttonPanel;

    FlarialButton? _primaryButton;
    FlarialButton? _secondaryButton;

    public event Action<bool>? Closed;

    public string Title
    {
        get => _titleText.Text;
        set => _titleText.Text = value;
    }

    public string Message
    {
        get => _messageText.Text;
        set => _messageText.Text = value;
    }

    public FlarialDialog()
    {
        _overlay = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
            Opacity = 0
        };
        Children.Add(_overlay);

        _dialogCard = new Border
        {
            CornerRadius = new CornerRadius(FlarialTheme.LargeRadius),
            Background = FlarialTheme.SurfaceBrush,
            BorderBrush = FlarialTheme.BorderSubtleBrush,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(FlarialTheme.SpacingXl),
            MaxWidth = 400,
            MinWidth = 300,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Effect = new DropShadowEffect
            {
                BlurRadius = 40,
                ShadowDepth = 0,
                Opacity = 0.5,
                Color = Colors.Black
            },
            RenderTransform = new ScaleTransform(0.9, 0.9),
            RenderTransformOrigin = new Point(0.5, 0.5),
            Opacity = 0
        };

        var contentStack = new StackPanel();

        _titleText = new TextBlock
        {
            FontSize = FlarialTheme.FontSizeLarge,
            FontWeight = FontWeights.SemiBold,
            Foreground = FlarialTheme.TextPrimaryBrush,
            TextWrapping = TextWrapping.Wrap
        };
        contentStack.Children.Add(_titleText);

        _messageText = new TextBlock
        {
            FontSize = FlarialTheme.FontSizeBody,
            Foreground = FlarialTheme.TextSecondaryBrush,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 22
        };
        contentStack.Children.Add(_messageText);

        _buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, FlarialTheme.SpacingSm, 0, 0)
        };
        contentStack.Children.Add(_buttonPanel);

        _dialogCard.Child = contentStack;
        Children.Add(_dialogCard);
    }

    public void SetButtons(string primaryText, string? secondaryText = null)
    {
        _buttonPanel.Children.Clear();

        if (!string.IsNullOrEmpty(secondaryText))
        {
            _secondaryButton = new FlarialButton
            {
                Variant = FlarialButton.ButtonVariant.Ghost,
                Margin = new Thickness(0, 0, FlarialTheme.SpacingSm, 0)
            };
            _secondaryButton.Content = new TextBlock
            {
                Text = secondaryText,
                Foreground = FlarialTheme.TextSecondaryBrush
            };
            _secondaryButton.Click += (s, e) => Close(false);
            _buttonPanel.Children.Add(_secondaryButton);
        }

        _primaryButton = new FlarialButton
        {
            Variant = FlarialButton.ButtonVariant.Primary
        };
        _primaryButton.Content = new TextBlock
        {
            Text = primaryText,
            Foreground = Brushes.White
        };
        _primaryButton.Click += (s, e) => Close(true);
        _buttonPanel.Children.Add(_primaryButton);
    }

    public void Show()
    {
        Visibility = Visibility.Visible;

        var overlayFade = new DoubleAnimation(0, 1, FlarialTheme.FastDuration);
        _overlay.BeginAnimation(OpacityProperty, overlayFade);

        var cardFade = new DoubleAnimation(0, 1, FlarialTheme.NormalDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        _dialogCard.BeginAnimation(OpacityProperty, cardFade);

        var cardScale = new DoubleAnimation(0.9, 1, FlarialTheme.NormalDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        ((ScaleTransform)_dialogCard.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, cardScale);
        ((ScaleTransform)_dialogCard.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, cardScale);
    }

    public void Close(bool result)
    {
        var overlayFade = new DoubleAnimation(0, FlarialTheme.FastDuration);
        _overlay.BeginAnimation(OpacityProperty, overlayFade);

        var cardFade = new DoubleAnimation(0, FlarialTheme.FastDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        cardFade.Completed += (s, e) =>
        {
            Visibility = Visibility.Collapsed;
            Closed?.Invoke(result);
        };
        _dialogCard.BeginAnimation(OpacityProperty, cardFade);

        var cardScale = new DoubleAnimation(0.9, FlarialTheme.FastDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        ((ScaleTransform)_dialogCard.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, cardScale);
        ((ScaleTransform)_dialogCard.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, cardScale);
    }

    /// <summary>
    /// Creates a dialog and adds it to the main window.
    /// </summary>
    public static FlarialDialog Create(string title, string message, string primaryButton, string? secondaryButton = null)
    {
        var dialog = new FlarialDialog
        {
            Title = title,
            Message = message,
            Visibility = Visibility.Collapsed
        };
        dialog.SetButtons(primaryButton, secondaryButton);
        return dialog;
    }
}
