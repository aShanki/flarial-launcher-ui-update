using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Flarial.Launcher.App;
using Flarial.Launcher.UI.Controls;
using Flarial.Launcher.UI.Theme;
using Flarial.Launcher.UI.Animations;
using System.Windows.Controls;
using Flarial.Launcher.Services.SDK;
using Flarial.Launcher.Services.Management.Versions;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Core;
using System;
using Flarial.Launcher.Services.Client;
using Flarial.Launcher.Services.Modding;
using System.Windows.Threading;
namespace Flarial.Launcher.UI.Pages;

sealed class HomePage : Grid
{
    readonly FlarialProgressBar _progressBar;
    readonly TextBlock _statusText;
    readonly FlarialButton _playButton;
    readonly FlarialCard? _bannerCard;

    internal HomePage(Configuration configuration, VersionCatalog catalog, Image? banner)
    {
        Background = FlarialTheme.BackgroundMediumBrush;

        var contentStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 100)
        };

        _statusText = new TextBlock
        {
            Text = "Preparing...",
            FontSize = FlarialTheme.FontSizeMedium,
            Foreground = FlarialTheme.TextSecondaryBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            Visibility = Visibility.Hidden
        };
        contentStack.Children.Add(_statusText);

        _progressBar = new FlarialProgressBar
        {
            Width = 450,
            Margin = new Thickness(0, FlarialTheme.SpacingMd, 0, 0),
            Visibility = Visibility.Hidden
        };
        contentStack.Children.Add(_progressBar);

        var playContent = new Grid
        {
            Width = 450
        };

        var playIcon = new TextBlock
        {
            Text = "\uE768",
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 36,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(FlarialTheme.SpacingXl, 0, 0, 0)
        };
        playContent.Children.Add(playIcon);

        var playText = new TextBlock
        {
            Text = "PLAY",
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        playContent.Children.Add(playText);

        _playButton = new FlarialButton
        {
            Content = playContent,
            Padding = new Thickness(0, FlarialTheme.SpacingXl, 0, FlarialTheme.SpacingXl)
        };
        contentStack.Children.Add(_playButton);

        Children.Add(contentStack);

        if (banner != null)
        {
            banner.Stretch = Stretch.UniformToFill;
            banner.HorizontalAlignment = HorizontalAlignment.Center;

            var bannerBorder = new Border
            {
                CornerRadius = new CornerRadius(FlarialTheme.MediumRadius),
                ClipToBounds = true,
                Child = banner
            };

            _bannerCard = new FlarialCard
            {
                Child = bannerBorder,
                Width = 320,
                Height = 80,
                Padding = new Thickness(0),
                HasBackground = false,
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, FlarialTheme.SpacingXl),
                IsClickable = true,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            Children.Add(_bannerCard);
        }

        _playButton.Click += async (_, _) =>
        {
            try
            {
                _playButton.Visibility = Visibility.Hidden;

                _progressBar.IsIndeterminate = true;
                _progressBar.Visibility = Visibility.Visible;

                _statusText.Visibility = Visibility.Visible;

                if (!Minecraft.IsInstalled)
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._notInstalled);
                    return;
                }

                if (!Minecraft.IsSigned)
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._notSigned);
                    return;
                }

                var beta = configuration.DllBuild is DllBuild.Beta;
                var custom = configuration.DllBuild is DllBuild.Custom;

                beta = beta || Minecraft.UsingGameDevelopmentKit;
                var client = beta ? FlarialClient.Beta : FlarialClient.Release;

                if (!custom && !beta && !catalog.IsSupported)
                {
                    var unsupportedVersion = new UnsupportedVersion(Minecraft.Version, catalog.LatestSupportedVersion);
                    await MessageDialog.ShowAsync(unsupportedVersion);
                    return;
                }

                if (custom)
                {
                    if (string.IsNullOrWhiteSpace(configuration.CustomDllPath))
                    {
                        await MessageDialog.ShowAsync(MessageDialogContent._invalidCustomDll);
                        return;
                    }

                    ModificationLibrary library = new(configuration.CustomDllPath!);

                    if (!library.IsValid)
                    {
                        await MessageDialog.ShowAsync(MessageDialogContent._invalidCustomDll);
                        return;
                    }

                    _statusText.Text = "Launching...";

                    if (await Task.Run(() => Injector.Launch(configuration.WaitForInitialization, library)) is null)
                    {
                        await MessageDialog.ShowAsync(MessageDialogContent._launchFailure);
                        return;
                    }

                    return;
                }

                _statusText.Text = "Verifying...";

                if (!await client.DownloadAsync(_ => Dispatcher.Invoke(() =>
                {
                    if (_progressBar.Value == _) return;

                    _statusText.Text = "Downloading...";

                    _progressBar.Value = _;
                    _progressBar.IsIndeterminate = false;
                })))
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._updateFailure);
                    return;
                }

                _statusText.Text = "Launching...";
                _progressBar.IsIndeterminate = true;

                if (beta && await MessageDialog.ShowAsync(MessageDialogContent._betaUsage))
                    return;

                if (!await Task.Run(() => client.Launch(configuration.WaitForInitialization)))
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._launchFailure);
                    return;
                }
            }
            finally
            {
                _progressBar.IsIndeterminate = false;
                _progressBar.Visibility = Visibility.Hidden;

                _statusText.Text = "Preparing...";
                _statusText.Visibility = Visibility.Hidden;

                _playButton.Visibility = Visibility.Visible;
            }
        };

        Loaded += (s, e) =>
        {
            _playButton.Opacity = 0;

            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, FlarialTheme.NormalDuration)
            {
                BeginTime = TimeSpan.FromMilliseconds(100)
            };
            _playButton.BeginAnimation(OpacityProperty, fadeIn);
        };
    }
}
