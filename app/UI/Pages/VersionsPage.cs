using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Management.Versions;
using Flarial.Launcher.UI.Controls;
using Flarial.Launcher.UI.Theme;
using Windows.ApplicationModel.Store.Preview.InstallControl;

namespace Flarial.Launcher.UI.Pages;

sealed class VersionsPage : Grid
{
    readonly StackPanel _versionsList;
    readonly ScrollViewer _scrollViewer;
    readonly FlarialButton _installButton;
    readonly FlarialProgressBar _progressBar;
    readonly TextBlock _statusText;
    readonly TextBlock _buttonText;
    readonly TextBlock _buttonIcon;
    readonly Grid _statusContainer;
    readonly VersionCatalog _catalog;

    VersionItem? _selectedItem;
    InstallRequest? _request = null;
    string? _installedVersion;

    internal string? SelectedVersion => _selectedItem?.Version;

    internal VersionsPage(VersionCatalog catalog)
    {
        _catalog = catalog;

        try { if (Minecraft.IsInstalled) _installedVersion = Minecraft.Version; }
        catch { _installedVersion = null; }

        Background = Brushes.Transparent;
        Margin = new Thickness(FlarialTheme.SpacingXl);

        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition());
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var headerContainer = new StackPanel
        {
            Margin = new Thickness(0, 0, 0, FlarialTheme.SpacingLg)
        };

        var header = new TextBlock
        {
            Text = "Game Versions",
            FontSize = FlarialTheme.FontSizeTitle,
            FontWeight = FontWeights.Bold,
            Foreground = FlarialTheme.TextPrimaryBrush
        };
        headerContainer.Children.Add(header);

        var subtitle = new TextBlock
        {
            Text = _installedVersion != null
                ? $"Currently installed: {_installedVersion}"
                : "Select a version to install",
            FontSize = FlarialTheme.FontSizeBody,
            Foreground = FlarialTheme.TextSecondaryBrush,
            Margin = new Thickness(0, FlarialTheme.SpacingXs, 0, 0)
        };
        headerContainer.Children.Add(subtitle);

        SetRow(headerContainer, 0);
        Children.Add(headerContainer);

        _versionsList = new StackPanel
        {
            Margin = new Thickness(0)
        };

        _scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _versionsList,
            Padding = new Thickness(0)
        };

        SetRow(_scrollViewer, 1);
        Children.Add(_scrollViewer);

        var footer = new Grid
        {
            Margin = new Thickness(0, FlarialTheme.SpacingLg, 0, 0)
        };
        footer.ColumnDefinitions.Add(new ColumnDefinition());
        footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _statusContainer = new Grid
        {
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = Visibility.Collapsed
        };

        var statusStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        var spinnerIcon = new TextBlock
        {
            Text = "\uE895",
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 14,
            Foreground = FlarialTheme.AccentBrush,
            VerticalAlignment = VerticalAlignment.Center,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new RotateTransform(0)
        };
        statusStack.Children.Add(spinnerIcon);

        _statusText = new TextBlock
        {
            Text = "Preparing...",
            FontSize = FlarialTheme.FontSizeBody,
            Foreground = FlarialTheme.TextSecondaryBrush,
            Margin = new Thickness(FlarialTheme.SpacingSm, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        statusStack.Children.Add(_statusText);

        _statusContainer.Children.Add(statusStack);

        // Progress bar
        _progressBar = new FlarialProgressBar
        {
            Height = 4,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, FlarialTheme.SpacingSm, FlarialTheme.SpacingLg, 0),
            Visibility = Visibility.Collapsed
        };

        var statusWrapper = new StackPanel();
        statusWrapper.Children.Add(_statusContainer);
        statusWrapper.Children.Add(_progressBar);

        SetColumn(statusWrapper, 0);
        footer.Children.Add(statusWrapper);

        var buttonContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        _buttonIcon = new TextBlock
        {
            Text = "\uE896", // Download icon
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 14,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };
        buttonContent.Children.Add(_buttonIcon);

        _buttonText = new TextBlock
        {
            Text = "Install Version",
            FontSize = FlarialTheme.FontSizeBody,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            Margin = new Thickness(FlarialTheme.SpacingSm, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        buttonContent.Children.Add(_buttonText);

        _installButton = new FlarialButton
        {
            Content = buttonContent,
            Padding = new Thickness(FlarialTheme.SpacingXl, FlarialTheme.SpacingMd, FlarialTheme.SpacingXl, FlarialTheme.SpacingMd)
        };
        SetColumn(_installButton, 1);
        footer.Children.Add(_installButton);

        SetRow(footer, 2);
        Children.Add(footer);

        _installButton.Click += async (_, _) =>
        {
            if (_selectedItem == null) return;

            if (_selectedItem.IsInstalled)
            {
                return;
            }

            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
            System.Diagnostics.Debug.WriteLine($"[VersionsPage] Install button clicked for version: {_selectedItem.Version}");

            try
            {
                IsEnabled = false;

                if (!Minecraft.IsInstalled)
                {
                    System.Diagnostics.Debug.WriteLine($"[VersionsPage] Minecraft not installed, showing dialog");
                    await MessageDialog.ShowAsync(MessageDialogContent._notInstalled);
                    return;
                }

                if (!Minecraft.IsSigned)
                {
                    System.Diagnostics.Debug.WriteLine($"[VersionsPage] Minecraft not signed, showing dialog");
                    await MessageDialog.ShowAsync(MessageDialogContent._notSigned);
                    return;
                }

                _installButton.Visibility = Visibility.Collapsed;
                _statusContainer.Visibility = Visibility.Visible;
                _progressBar.Visibility = Visibility.Visible;
                _progressBar.IsIndeterminate = true;
                _statusText.Text = "Preparing...";

                System.Diagnostics.Debug.WriteLine($"[VersionsPage] UI set to 'Preparing...' state");

                try
                {
                    var entry = _catalog[_selectedItem.Version];
                    System.Diagnostics.Debug.WriteLine($"[VersionsPage] Got catalog entry, calling InstallAsync (this fetches download URL first)...");

                    var installAsyncStopwatch = System.Diagnostics.Stopwatch.StartNew();

                    _request = await entry.InstallAsync((sender, args) => Dispatcher.Invoke(() =>
                    {
                        if (_progressBar.Value == args) return;

                        if (sender is AppInstallState.Downloading)
                        {
                            if (_statusText.Text.StartsWith("Preparing"))
                            {
                                System.Diagnostics.Debug.WriteLine($"[VersionsPage] First download progress received after {installAsyncStopwatch.ElapsedMilliseconds}ms in 'Preparing' state");
                            }
                            _statusText.Text = $"Downloading... {args:F0}%";
                        }
                        else if (sender is AppInstallState.Installing)
                        {
                            if (!_statusText.Text.StartsWith("Installing"))
                            {
                                System.Diagnostics.Debug.WriteLine($"[VersionsPage] Transitioned to Installing state after {installAsyncStopwatch.ElapsedMilliseconds}ms");
                            }
                            _statusText.Text = $"Installing... {args:F0}%";
                        }

                        _progressBar.Value = args;
                        _progressBar.IsIndeterminate = false;
                    }));

                    System.Diagnostics.Debug.WriteLine($"[VersionsPage] InstallAsync returned InstallRequest after {installAsyncStopwatch.ElapsedMilliseconds}ms");

                    var result = await _request;
                    System.Diagnostics.Debug.WriteLine($"[VersionsPage] Installation completed with result: {result}");

                    if (!result) Application.Current.Shutdown();
                }
                finally
                {
                    _request?.Dispose();
                    _request = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VersionsPage] ERROR: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[VersionsPage] Stack trace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                totalStopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[VersionsPage] Total install flow time: {totalStopwatch.ElapsedMilliseconds}ms");

                _progressBar.IsIndeterminate = false;
                _progressBar.Value = 0;
                _progressBar.Visibility = Visibility.Collapsed;
                _statusContainer.Visibility = Visibility.Collapsed;
                _installButton.Visibility = Visibility.Visible;

                IsEnabled = true;
            }
        };

        Application.Current.MainWindow.Closing += (sender, args) =>
        {
            args.Cancel = _request?.Cancel() ?? false;
            if (args.Cancel) ((Window)sender).Hide();
        };
    }

    private void OnVersionSelected(object? sender, EventArgs e)
    {
        if (sender is not VersionItem newItem) return;

        if (_selectedItem != null && _selectedItem != newItem)
        {
            _selectedItem.IsSelected = false;
        }

        _selectedItem = newItem;
        newItem.IsSelected = true;

        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (_selectedItem == null) return;

        if (_selectedItem.IsInstalled)
        {
            _buttonIcon.Text = "\uE73E"; // Checkmark
            _buttonText.Text = "Already Installed";
            _installButton.IsEnabled = false;
            _installButton.Opacity = 0.5;
        }
        else
        {
            _buttonIcon.Text = "\uE896"; // Download
            _buttonText.Text = "Install Version";
            _installButton.IsEnabled = true;
            _installButton.Opacity = 1.0;
        }
    }

    /// <summary>
    /// Adds a version to the list.
    /// </summary>
    internal void AddVersion(string version)
    {
        bool isFirst = _versionsList.Children.Count == 0;
        bool isInstalled = version == _installedVersion;

        var item = new VersionItem(version, isFirst, isInstalled);
        item.Selected += OnVersionSelected;
        _versionsList.Children.Add(item);

        if (isInstalled || (isFirst && _selectedItem == null))
        {
            if (_selectedItem != null)
            {
                _selectedItem.IsSelected = false;
            }
            _selectedItem = item;
            item.IsSelected = true;
            UpdateButtonState();
        }
    }

    /// <summary>
    /// Selects the first version in the list.
    /// </summary>
    internal void SelectFirst()
    {
        if (_selectedItem != null) return;

        if (_versionsList.Children.Count > 0 && _versionsList.Children[0] is VersionItem first)
        {
            _selectedItem = first;
            first.IsSelected = true;
            UpdateButtonState();
        }
    }
}

sealed class VersionItem : Border
{
    readonly TextBlock _versionText;
    readonly SolidColorBrush _backgroundBrush;

    bool _isSelected;

    public string Version { get; }
    public bool IsLatest { get; }
    public bool IsInstalled { get; }

    public event EventHandler? Selected;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            UpdateVisualState();
        }
    }

    public VersionItem(string version, bool isLatest, bool isInstalled)
    {
        Version = version;
        IsLatest = isLatest;
        IsInstalled = isInstalled;

        var transparentHover = Color.FromArgb(0, FlarialTheme.SurfaceHover.R, FlarialTheme.SurfaceHover.G, FlarialTheme.SurfaceHover.B);
        _backgroundBrush = FlarialTheme.CreateAnimatableBrush(transparentHover);
        Background = _backgroundBrush;
        CornerRadius = new CornerRadius(FlarialTheme.SmallRadius);
        BorderThickness = new Thickness(0);
        Margin = new Thickness(0, 2, 0, 2);
        Padding = new Thickness(FlarialTheme.SpacingMd, FlarialTheme.SpacingSm, FlarialTheme.SpacingMd, FlarialTheme.SpacingSm);
        Cursor = Cursors.Hand;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        var contentGrid = new Grid();
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition());
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _versionText = new TextBlock
        {
            Text = version,
            FontSize = FlarialTheme.FontSizeBody,
            FontWeight = FontWeights.Normal,
            Foreground = FlarialTheme.TextPrimaryBrush,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_versionText, 0);
        contentGrid.Children.Add(_versionText);

        var badgesPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (isInstalled)
        {
            var installedBadge = CreateBadge("INSTALLED",
                Color.FromArgb(25, 76, 175, 80),  // Subtle green background
                Color.FromRgb(76, 175, 80));
            badgesPanel.Children.Add(installedBadge);
        }

        if (isLatest && !isInstalled)
        {
            var latestBadge = CreateBadge("LATEST",
                Color.FromArgb(25, 255, 36, 56),   // Subtle accent background
                FlarialTheme.AccentPrimary);
            badgesPanel.Children.Add(latestBadge);
        }

        if (!isInstalled && !isLatest)
        {
            var availableIcon = new TextBlock
            {
                Text = "\uE896", // Download icon
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 12,
                Foreground = FlarialTheme.TextDisabledBrush,
                VerticalAlignment = VerticalAlignment.Center
            };
            badgesPanel.Children.Add(availableIcon);
        }

        Grid.SetColumn(badgesPanel, 1);
        contentGrid.Children.Add(badgesPanel);

        Child = contentGrid;

        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
        MouseLeftButtonUp += OnMouseUp;
    }

    private static Border CreateBadge(string text, Color bgColor, Color textColor)
    {
        var badgeBrush = new SolidColorBrush(bgColor);
        badgeBrush.Freeze();

        var textBrush = new SolidColorBrush(textColor);
        textBrush.Freeze();

        var badge = new Border
        {
            Background = badgeBrush,
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(FlarialTheme.SpacingSm, 2, FlarialTheme.SpacingSm, 2),
            Margin = new Thickness(FlarialTheme.SpacingSm, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        badge.Child = new TextBlock
        {
            Text = text,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = textBrush
        };

        return badge;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (!_isSelected)
        {
            AnimateBackground(FlarialTheme.SurfaceHover);
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (!_isSelected)
        {
            var transparentHover = Color.FromArgb(0, FlarialTheme.SurfaceHover.R, FlarialTheme.SurfaceHover.G, FlarialTheme.SurfaceHover.B);
            AnimateBackground(transparentHover);
        }
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        Selected?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateVisualState()
    {
        if (_isSelected)
        {
            AnimateBackground(FlarialTheme.SurfaceSelected);
        }
        else
        {
            var transparentHover = Color.FromArgb(0, FlarialTheme.SurfaceHover.R, FlarialTheme.SurfaceHover.G, FlarialTheme.SurfaceHover.B);
            AnimateBackground(transparentHover);
        }
    }

    private void AnimateBackground(Color target)
    {
        var anim = new ColorAnimation(target, FlarialTheme.FastDuration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        _backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
    }
}
