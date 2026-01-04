using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Flarial.Launcher.Management;
using Flarial.Launcher.Services.Client;
using Flarial.Launcher.Services.Management;
using Flarial.Launcher.Services.Management.Versions;
using Flarial.Launcher.Services.Networking;
using Flarial.Launcher.Interface.Animations;
using Flarial.Launcher.Interface.Pages;
using Flarial.Launcher.Interface.Theme;
using ModernWpf.Controls;
using System.Windows.Interop;

namespace Flarial.Launcher.Interface;

sealed class MainWindowContent : Grid
{
    HomePage? _homePage = null;
    VersionsPage? _versionsPage = null;
    readonly SettingsPage _settingsPage;
    readonly Configuration _configuration;

    Border _navRail = null!;
    StackPanel _navItems = null!;
    readonly ContentPresenter _contentArea;
    readonly LoadingSpinner _loadingSpinner;

    Border? _selectedNavItem;
    int _selectedIndex = 0;

    internal MainWindowContent(Configuration configuration)
    {
        _configuration = configuration;
        _settingsPage = new(configuration);
        IsEnabled = false;

        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(FlarialTheme.NavRailWidth) });
        ColumnDefinitions.Add(new ColumnDefinition());

        _navRail = CreateNavRail();
        SetColumn(_navRail, 0);
        Children.Add(_navRail);

        _contentArea = new ContentPresenter
        {
            Margin = new Thickness(0)
        };
        SetColumn(_contentArea, 1);
        Children.Add(_contentArea);

        _loadingSpinner = new LoadingSpinner
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _contentArea.Content = _loadingSpinner;

        Application.Current.MainWindow.ContentRendered += OnContentRendered;
    }

    private Border CreateNavRail()
    {
        var rail = new Border
        {
            Background = FlarialTheme.SurfaceBrush,
            BorderBrush = FlarialTheme.BorderSubtleBrush,
            BorderThickness = new Thickness(0, 0, 1, 0)
        };

        var mainStack = new Grid();
        mainStack.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainStack.RowDefinitions.Add(new RowDefinition());
        mainStack.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var logoSection = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(FlarialTheme.SpacingLg, FlarialTheme.SpacingLg, FlarialTheme.SpacingLg, FlarialTheme.SpacingXl)
        };

        var logoImage = new Image
        {
            Source = ApplicationManifest.Icon,
            Width = 32,
            Height = 32
        };
        logoSection.Children.Add(logoImage);

        var titleText = new TextBlock
        {
            Text = "Flarial",
            FontSize = FlarialTheme.FontSizeLarge,
            FontWeight = FontWeights.Bold,
            Foreground = FlarialTheme.TextPrimaryBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(FlarialTheme.SpacingMd, 0, 0, 0)
        };
        logoSection.Children.Add(titleText);

        SetRow(logoSection, 0);
        mainStack.Children.Add(logoSection);

        var navSection = new Grid { Margin = new Thickness(FlarialTheme.SpacingSm, 0, FlarialTheme.SpacingSm, 0) };

        _navItems = new StackPanel();

        var homeItem = CreateNavItem("Home", Symbol.Home, 0);
        var versionsItem = CreateNavItem("Versions", Symbol.BrowsePhotos, 1);
        var settingsItem = CreateNavItem("Settings", Symbol.Setting, 2);

        _navItems.Children.Add(homeItem);
        _navItems.Children.Add(versionsItem);

        _navItems.Children.Add(new Border
        {
            Height = 1,
            Background = FlarialTheme.BorderSubtleBrush,
            Margin = new Thickness(FlarialTheme.SpacingMd, FlarialTheme.SpacingSm, FlarialTheme.SpacingMd, FlarialTheme.SpacingSm)
        });

        _navItems.Children.Add(settingsItem);

        navSection.Children.Add(_navItems);
        SetRow(navSection, 1);
        mainStack.Children.Add(navSection);

        var footer = new TextBlock
        {
            Text = ApplicationManifest.Version,
            FontSize = FlarialTheme.FontSizeSmall,
            Foreground = FlarialTheme.TextDisabledBrush,
            Margin = new Thickness(FlarialTheme.SpacingLg, FlarialTheme.SpacingLg, 0, FlarialTheme.SpacingLg)
        };
        SetRow(footer, 2);
        mainStack.Children.Add(footer);

        rail.Child = mainStack;
        return rail;
    }

    private Border CreateNavItem(string label, Symbol icon, int index)
    {
        var isSelected = index == 0;

        var item = new Border
        {
            CornerRadius = new CornerRadius(FlarialTheme.SmallRadius),
            Padding = new Thickness(FlarialTheme.SpacingMd, FlarialTheme.SpacingSm, FlarialTheme.SpacingMd, FlarialTheme.SpacingSm),
            Cursor = Cursors.Hand,
            Background = isSelected ? FlarialTheme.SurfaceHoverBrush : FlarialTheme.TransparentBrush,
            Tag = index
        };

        var content = new StackPanel { Orientation = Orientation.Horizontal };

        var symbolIcon = new SymbolIcon(icon)
        {
            Foreground = isSelected ? FlarialTheme.TextPrimaryBrush : FlarialTheme.TextSecondaryBrush
        };
        content.Children.Add(symbolIcon);

        var labelText = new TextBlock
        {
            Text = label,
            FontSize = FlarialTheme.FontSizeBody,
            Foreground = isSelected ? FlarialTheme.TextPrimaryBrush : FlarialTheme.TextSecondaryBrush,
            Margin = new Thickness(FlarialTheme.SpacingMd, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        content.Children.Add(labelText);

        item.Child = content;

        if (isSelected)
            _selectedNavItem = item;

        item.MouseEnter += (s, e) =>
        {
            if ((int)item.Tag != _selectedIndex)
            {
                var colorAnim = new ColorAnimation(FlarialTheme.SurfaceHover, FlarialTheme.FastDuration);
                ((SolidColorBrush)item.Background).BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
        };

        item.MouseLeave += (s, e) =>
        {
            if ((int)item.Tag != _selectedIndex)
            {
                var transparentHover = Color.FromArgb(0, FlarialTheme.SurfaceHover.R, FlarialTheme.SurfaceHover.G, FlarialTheme.SurfaceHover.B);
                var colorAnim = new ColorAnimation(transparentHover, FlarialTheme.FastDuration);
                ((SolidColorBrush)item.Background).BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
        };

        item.MouseLeftButtonUp += (s, e) => SelectNavItem(item);

        var initialColor = isSelected ? FlarialTheme.SurfaceHover : Color.FromArgb(0, FlarialTheme.SurfaceHover.R, FlarialTheme.SurfaceHover.G, FlarialTheme.SurfaceHover.B);
        item.Background = FlarialTheme.CreateAnimatableBrush(initialColor);

        return item;
    }

    private void SelectNavItem(Border item)
    {
        int index = (int)item.Tag;
        if (index == _selectedIndex) return;

        int previousIndex = _selectedIndex;

        if (_selectedNavItem != null)
        {
            var prevStack = (StackPanel)_selectedNavItem.Child;
            var prevIcon = (SymbolIcon)prevStack.Children[0];
            var prevLabel = (TextBlock)prevStack.Children[1];

            prevIcon.Foreground = FlarialTheme.TextSecondaryBrush;
            prevLabel.Foreground = FlarialTheme.TextSecondaryBrush;

            var transparentHover = Color.FromArgb(0, FlarialTheme.SurfaceHover.R, FlarialTheme.SurfaceHover.G, FlarialTheme.SurfaceHover.B);
            var colorAnim = new ColorAnimation(transparentHover, FlarialTheme.FastDuration);
            ((SolidColorBrush)_selectedNavItem.Background).BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
        }

        var stack = (StackPanel)item.Child;
        var iconEl = (SymbolIcon)stack.Children[0];
        var labelEl = (TextBlock)stack.Children[1];

        iconEl.Foreground = FlarialTheme.TextPrimaryBrush;
        labelEl.Foreground = FlarialTheme.TextPrimaryBrush;

        var bgAnim = new ColorAnimation(FlarialTheme.SurfaceHover, FlarialTheme.FastDuration);
        ((SolidColorBrush)item.Background).BeginAnimation(SolidColorBrush.ColorProperty, bgAnim);

        _selectedNavItem = item;
        _selectedIndex = index;

        NavigateToPage(index, previousIndex);
    }

    private void NavigateToPage(int index, int previousIndex)
    {
        FrameworkElement? page = index switch
        {
            0 => _homePage,
            1 => _versionsPage,
            2 => _settingsPage,
            _ => null
        };

        if (page == null) return;

        bool goingDown = index > previousIndex;
        var outDirection = goingDown ? PageTransitions.SlideDirection.Up : PageTransitions.SlideDirection.Down;
        var inDirection = goingDown ? PageTransitions.SlideDirection.Down : PageTransitions.SlideDirection.Up;

        var currentPage = _contentArea.Content as FrameworkElement;
        if (currentPage != null && currentPage != page)
        {
            PageTransitions.SlideOut(currentPage, outDirection, () =>
            {
                _contentArea.Content = page;
                PageTransitions.SlideIn(page, inDirection);
            });
        }
        else
        {
            _contentArea.Content = page;
            PageTransitions.SlideIn(page, inDirection);
        }
    }

    private async void OnContentRendered(object? sender, System.EventArgs e)
    {
        try
        {
            if (!await HttpService.IsAvailableAsync() &&
                !await MessageDialog.ShowAsync(MessageDialogContent._connectionFailure))
                Application.Current.Shutdown();

            if (await LauncherUpdater.CheckAsync() &&
                await MessageDialog.ShowAsync(MessageDialogContent._launcherUpdateAvailable))
            {
                await LauncherUpdater.DownloadAsync(delegate { });
                return;
            }

            var catalog = VersionEntries.CreateAsync();
            var bannerTask = Sponsorship.GetAsync(new WindowInteropHelper(Application.Current.MainWindow));
            var entries = await catalog;
            var banner = await bannerTask;

            _versionsPage = new(entries);
            _homePage = new(_configuration, entries, banner);

            foreach (var entry in entries)
            {
                if (entry.Value is null) continue;
                _versionsPage.AddVersion(entry.Key);
                await System.Windows.Threading.Dispatcher.Yield();
            }

            _versionsPage.SelectFirst();

            _contentArea.Content = _homePage;
            PageTransitions.SlideIn(_homePage, PageTransitions.SlideDirection.Up);

            IsEnabled = true;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Load error: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
