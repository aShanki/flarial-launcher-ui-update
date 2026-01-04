using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Flarial.Launcher.App;
using Flarial.Launcher.UI.Controls;
using Flarial.Launcher.UI.Theme;
using Microsoft.Win32;
using ModernWpf.Controls;

namespace Flarial.Launcher.UI.Pages;

sealed class SettingsPage : ScrollViewer
{
    readonly FlarialToggle _hardwareAcceleration;
    readonly FlarialToggle _waitForInitialization;
    readonly Border _customDllSection;
    TextBox _customDllPath = null!;
    int _selectedBuild = 0;

    internal SettingsPage(Configuration configuration)
    {
        Background = FlarialTheme.BackgroundMediumBrush;
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        Padding = new Thickness(FlarialTheme.SpacingLg);

        var mainStack = new StackPanel();

        var header = new TextBlock
        {
            Text = "Settings",
            FontSize = FlarialTheme.FontSizeTitle,
            FontWeight = FontWeights.Bold,
            Foreground = FlarialTheme.TextPrimaryBrush,
            Margin = new Thickness(0, 0, 0, FlarialTheme.SpacingSm)
        };
        mainStack.Children.Add(header);

        var buildCard = new FlarialCard { Margin = new Thickness(0, 0, 0, FlarialTheme.SpacingMd), EnableHover = false };
        var buildStack = new StackPanel();

        buildStack.Children.Add(new TextBlock
        {
            Text = "CLIENT BUILD",
            FontSize = FlarialTheme.FontSizeSmall,
            FontWeight = FontWeights.SemiBold,
            Foreground = FlarialTheme.TextSecondaryBrush,
            Margin = new Thickness(0, 0, 0, FlarialTheme.SpacingSm)
        });

        var releaseOption = CreateRadioOption("Release", "Stable version for everyday use", 0, configuration);
        var betaOption = CreateRadioOption("Beta", "Preview features, may be unstable", 1, configuration);
        var customOption = CreateRadioOption("Custom", "Use your own DLL file", 2, configuration);

        buildStack.Children.Add(releaseOption);
        buildStack.Children.Add(betaOption);
        buildStack.Children.Add(customOption);

        buildCard.Child = buildStack;
        mainStack.Children.Add(buildCard);

        _customDllSection = CreateCustomDllSection(configuration);
        _customDllSection.Visibility = configuration.DllBuild == DllBuild.Custom ? Visibility.Visible : Visibility.Collapsed;
        mainStack.Children.Add(_customDllSection);

        var perfCard = new FlarialCard { EnableHover = false };
        var perfStack = new StackPanel();

        perfStack.Children.Add(new TextBlock
        {
            Text = "PERFORMANCE",
            FontSize = FlarialTheme.FontSizeSmall,
            FontWeight = FontWeights.SemiBold,
            Foreground = FlarialTheme.TextSecondaryBrush,
            Margin = new Thickness(0, 0, 0, FlarialTheme.SpacingSm)
        });

        var hardwareAccelOption = new Grid();
        hardwareAccelOption.ColumnDefinitions.Add(new ColumnDefinition());
        hardwareAccelOption.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var hardwareAccelText = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        hardwareAccelText.Children.Add(new TextBlock
        {
            Text = "Hardware Acceleration",
            FontSize = FlarialTheme.FontSizeBody,
            Foreground = FlarialTheme.TextPrimaryBrush
        });
        hardwareAccelText.Children.Add(new TextBlock
        {
            Text = "Improve launcher responsiveness using GPU",
            FontSize = FlarialTheme.FontSizeSmall,
            Foreground = FlarialTheme.TextSecondaryBrush,
            Margin = new Thickness(0, 2, 0, 0)
        });
        Grid.SetColumn(hardwareAccelText, 0);
        hardwareAccelOption.Children.Add(hardwareAccelText);

        _hardwareAcceleration = new FlarialToggle();
        _hardwareAcceleration.SetState(configuration.HardwareAcceleration, animate: false);
        _hardwareAcceleration.Toggled += (s, e) => configuration.HardwareAcceleration = _hardwareAcceleration.IsOn;
        Grid.SetColumn(_hardwareAcceleration, 1);
        hardwareAccelOption.Children.Add(_hardwareAcceleration);

        perfStack.Children.Add(hardwareAccelOption);

        perfStack.Children.Add(new Border
        {
            Height = 1,
            Background = FlarialTheme.BorderSubtleBrush,
            Margin = new Thickness(0, FlarialTheme.SpacingMd, 0, FlarialTheme.SpacingMd)
        });

        var initOption = new Grid();
        initOption.ColumnDefinitions.Add(new ColumnDefinition());
        initOption.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var initText = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        initText.Children.Add(new TextBlock
        {
            Text = "Wait for Initialization",
            FontSize = FlarialTheme.FontSizeBody,
            Foreground = FlarialTheme.TextPrimaryBrush
        });
        initText.Children.Add(new TextBlock
        {
            Text = "Reduce crashes at the cost of injection speed",
            FontSize = FlarialTheme.FontSizeSmall,
            Foreground = FlarialTheme.TextSecondaryBrush,
            Margin = new Thickness(0, 2, 0, 0)
        });
        Grid.SetColumn(initText, 0);
        initOption.Children.Add(initText);

        _waitForInitialization = new FlarialToggle();
        _waitForInitialization.SetState(configuration.WaitForInitialization, animate: false);
        _waitForInitialization.Toggled += (s, e) => configuration.WaitForInitialization = _waitForInitialization.IsOn;
        Grid.SetColumn(_waitForInitialization, 1);
        initOption.Children.Add(_waitForInitialization);

        perfStack.Children.Add(initOption);

        perfCard.Child = perfStack;
        mainStack.Children.Add(perfCard);

        Content = mainStack;

        _selectedBuild = (int)configuration.DllBuild;
        UpdateRadioSelection((int)configuration.DllBuild);
    }

    private Border CreateRadioOption(string label, string description, int index, Configuration configuration)
    {
        bool isSelected = (int)configuration.DllBuild == index;

        var transparentHover = Color.FromArgb(0, FlarialTheme.SurfaceHover.R, FlarialTheme.SurfaceHover.G, FlarialTheme.SurfaceHover.B);
        var option = new Border
        {
            CornerRadius = new CornerRadius(FlarialTheme.SmallRadius),
            Padding = new Thickness(FlarialTheme.SpacingMd),
            Margin = new Thickness(0, 0, 0, FlarialTheme.SpacingXs),
            Background = FlarialTheme.CreateAnimatableBrush(isSelected ? FlarialTheme.SurfaceSelected : transparentHover),
            Cursor = System.Windows.Input.Cursors.Hand,
            Tag = index
        };

        var content = new Grid();
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        content.ColumnDefinitions.Add(new ColumnDefinition());

        var radioOuter = new Border
        {
            Width = 20,
            Height = 20,
            CornerRadius = new CornerRadius(10),
            BorderBrush = isSelected ? FlarialTheme.AccentBrush : FlarialTheme.BorderMediumBrush,
            BorderThickness = new Thickness(2),
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 2, FlarialTheme.SpacingMd, 0)
        };

        var radioInner = new Border
        {
            Width = 10,
            Height = 10,
            CornerRadius = new CornerRadius(5),
            Background = FlarialTheme.AccentBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed
        };
        radioOuter.Child = radioInner;

        Grid.SetColumn(radioOuter, 0);
        content.Children.Add(radioOuter);

        var textStack = new StackPanel();
        textStack.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = FlarialTheme.FontSizeBody,
            Foreground = FlarialTheme.TextPrimaryBrush
        });
        textStack.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = FlarialTheme.FontSizeSmall,
            Foreground = FlarialTheme.TextSecondaryBrush,
            Margin = new Thickness(0, 2, 0, 0)
        });

        Grid.SetColumn(textStack, 1);
        content.Children.Add(textStack);

        option.Child = content;

        option.MouseLeftButtonUp += (s, e) =>
        {
            if (_selectedBuild == index) return;

            _selectedBuild = index;
            configuration.DllBuild = (DllBuild)index;
            UpdateRadioSelection(index);
            _customDllSection.Visibility = index == 2 ? Visibility.Visible : Visibility.Collapsed;
        };

        option.MouseEnter += (s, e) =>
        {
            if (_selectedBuild != index)
            {
                var anim = new System.Windows.Media.Animation.ColorAnimation(FlarialTheme.SurfaceHover, FlarialTheme.FastDuration);
                ((SolidColorBrush)option.Background).BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
        };

        option.MouseLeave += (s, e) =>
        {
            if (_selectedBuild != index)
            {
                var transparentBg = Color.FromArgb(0, FlarialTheme.SurfaceHover.R, FlarialTheme.SurfaceHover.G, FlarialTheme.SurfaceHover.B);
                var anim = new System.Windows.Media.Animation.ColorAnimation(transparentBg, FlarialTheme.FastDuration);
                ((SolidColorBrush)option.Background).BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
        };

        return option;
    }

    private void UpdateRadioSelection(int selectedIndex)
    {
        var mainStack = (StackPanel)Content;
        var buildCard = (FlarialCard)mainStack.Children[1];
        var buildStack = (StackPanel?)buildCard.Child;
        if (buildStack == null) return;

        for (int i = 1; i < 4; i++) // Skip header (index 0)
        {
            var option = (Border)buildStack.Children[i];
            var grid = (Grid)option.Child;
            var radioOuter = (Border)grid.Children[0];
            var radioInner = (Border)radioOuter.Child;

            int optionIndex = (int)option.Tag;
            bool isSelected = optionIndex == selectedIndex;

            radioOuter.BorderBrush = isSelected ? FlarialTheme.AccentBrush : FlarialTheme.BorderMediumBrush;
            radioInner.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;

            var transparentBg = Color.FromArgb(0, FlarialTheme.SurfaceHover.R, FlarialTheme.SurfaceHover.G, FlarialTheme.SurfaceHover.B);
            var bgColor = isSelected ? FlarialTheme.SurfaceSelected : transparentBg;
            var anim = new System.Windows.Media.Animation.ColorAnimation(bgColor, FlarialTheme.FastDuration);
            ((SolidColorBrush)option.Background).BeginAnimation(SolidColorBrush.ColorProperty, anim);
        }
    }

    private Border CreateCustomDllSection(Configuration configuration)
    {
        var card = new FlarialCard();
        var stack = new StackPanel();

        stack.Children.Add(new TextBlock
        {
            Text = "CUSTOM DLL PATH",
            FontSize = FlarialTheme.FontSizeSmall,
            FontWeight = FontWeights.SemiBold,
            Foreground = FlarialTheme.TextSecondaryBrush,
            Margin = new Thickness(0, 0, 0, FlarialTheme.SpacingSm)
        });

        var inputGrid = new Grid();
        inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
        inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _customDllPath = new TextBox
        {
            Text = configuration.CustomDllPath ?? "",
            IsReadOnly = true,
            Background = FlarialTheme.SurfaceBrush,
            Foreground = FlarialTheme.TextPrimaryBrush,
            BorderBrush = FlarialTheme.BorderSubtleBrush,
            Padding = new Thickness(FlarialTheme.SpacingMd, FlarialTheme.SpacingSm, FlarialTheme.SpacingMd, FlarialTheme.SpacingSm)
        };
        Grid.SetColumn(_customDllPath, 0);
        inputGrid.Children.Add(_customDllPath);

        var browseButton = new FlarialButton
        {
            Variant = FlarialButton.ButtonVariant.Secondary,
            Margin = new Thickness(FlarialTheme.SpacingSm, 0, 0, 0),
            Padding = new Thickness(FlarialTheme.SpacingMd, FlarialTheme.SpacingSm, FlarialTheme.SpacingMd, FlarialTheme.SpacingSm)
        };

        var browseContent = new StackPanel { Orientation = Orientation.Horizontal };
        browseContent.Children.Add(new TextBlock
        {
            Text = "\uE8E5", // Folder icon
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            Foreground = FlarialTheme.TextPrimaryBrush,
            VerticalAlignment = VerticalAlignment.Center
        });
        browseContent.Children.Add(new TextBlock
        {
            Text = "Browse",
            Foreground = FlarialTheme.TextPrimaryBrush,
            Margin = new Thickness(FlarialTheme.SpacingSm, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        });
        browseButton.Content = browseContent;

        browseButton.Click += (s, e) =>
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true,
                Filter = "Dynamic-Link Libraries (*.dll)|*.dll"
            };

            if (dialog.ShowDialog() == true)
            {
                _customDllPath.Text = dialog.FileName;
                configuration.CustomDllPath = dialog.FileName;
            }
        };

        Grid.SetColumn(browseButton, 1);
        inputGrid.Children.Add(browseButton);

        stack.Children.Add(inputGrid);
        card.Child = stack;

        return card;
    }
}
