using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading.Tasks;
using Flarial.Launcher.Management;
using Flarial.Launcher.Interface.Controls;
using Flarial.Launcher.Interface.Theme;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Client;
using Flarial.Launcher.Services.Modding;
using Flarial.Launcher.Services.Management.Versions;
using static Flarial.Launcher.Interface.MessageDialogContent;

namespace Flarial.Launcher.Interface.Pages;

sealed class HomePage : Grid
{
    readonly FlarialProgressBar _progressBar;
    readonly TextBlock _statusText;
    readonly FlarialButton _playButton;

    readonly VersionEntries _entries;

    sealed class UnsupportedVersion(string packageVersion, string supportedVersion) : MessageDialogContent
    {
        public override string Title => "Unsupported Version";
        public override string Primary => "Back";
        public override string Content => $@"Minecraft {packageVersion} isn't compatible with Flarial Client.

Please switch to Minecraft {supportedVersion} for the best experience.
You may switch versions by going to the Versions page in the launcher.

If you need help, join our Discord.";
    }

    internal HomePage(Configuration configuration, VersionEntries entries, Image? banner)
    {
        _entries = entries;
        Background = Brushes.Transparent;

        var contentStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, -22, 0, 0)
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
            Children.Add(banner);
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
                    await MessageDialog.ShowAsync(_notInstalled);
                    return;
                }

                if (Minecraft.UsingGameDevelopmentKit && !Minecraft.IsPackaged)
                {
                    await MessageDialog.ShowAsync(_unsignedInstallation);
                    return;
                }

                var beta = configuration.DllBuild is DllBuild.Beta;
                var custom = configuration.DllBuild is DllBuild.Custom;

                var client = beta ? FlarialClient.Beta : FlarialClient.Release;

                if (!custom && !beta && !_entries.IsSupported)
                {
                    var unsupportedVersion = new UnsupportedVersion(Minecraft.PackageVersion, _entries.First().Key);
                    await MessageDialog.ShowAsync(unsupportedVersion);
                    return;
                }

                if (custom)
                {
                    if (string.IsNullOrWhiteSpace(configuration.CustomDllPath))
                    {
                        await MessageDialog.ShowAsync(_invalidCustomDll);
                        return;
                    }

                    ModificationLibrary library = new(configuration.CustomDllPath!);

                    if (!library.IsValid)
                    {
                        await MessageDialog.ShowAsync(_invalidCustomDll);
                        return;
                    }

                    _statusText.Text = "Launching...";

                    if (await Task.Run(() => Injector.Launch(configuration.WaitForInitialization, library)) is null)
                    {
                        await MessageDialog.ShowAsync(_launchFailure);
                        return;
                    }

                    return;
                }

                if (beta && await MessageDialog.ShowAsync(_betaDllEnabled))
                    return;

                _statusText.Text = "Verifying...";

                if (!await client.DownloadAsync(_ => Dispatcher.Invoke(() =>
                {
                    if (_progressBar.Value == _) return;

                    _statusText.Text = "Downloading...";

                    _progressBar.Value = _;
                    _progressBar.IsIndeterminate = false;
                })))
                {
                    await MessageDialog.ShowAsync(_clientUpdateFailure);
                    return;
                }

                _statusText.Text = "Launching...";
                _progressBar.IsIndeterminate = true;

                if (!await Task.Run(() => client.Launch(configuration.WaitForInitialization)))
                {
                    await MessageDialog.ShowAsync(_launchFailure);
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

            var fadeIn = new DoubleAnimation(0, 1, FlarialTheme.NormalDuration)
            {
                BeginTime = TimeSpan.FromMilliseconds(100)
            };
            _playButton.BeginAnimation(OpacityProperty, fadeIn);
        };
    }
}
