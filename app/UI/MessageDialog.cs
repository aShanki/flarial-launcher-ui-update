using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.UI.Controls;

namespace Flarial.Launcher.UI;

static class MessageDialog
{
    static FlarialDialog? s_currentDialog;
    static Grid? s_dialogContainer;
    static readonly SemaphoreSlim s_semaphore = new(1, 1);

    public static bool IsShown => s_semaphore.CurrentCount <= 0;

    private static Grid GetDialogContainer()
    {
        if (s_dialogContainer != null) return s_dialogContainer;

        // Create a container grid that sits on top of all content
        s_dialogContainer = new Grid
        {
            Visibility = Visibility.Collapsed
        };

        // Find the main window and add the dialog container
        var mainWindow = Application.Current.MainWindow;
        Grid? targetGrid = null;

        if (mainWindow?.Content is Grid rootGrid)
        {
            targetGrid = rootGrid;
        }
        else if (mainWindow?.Content is Border border)
        {
            // Handle nested borders (Border > Border > Grid)
            var child = border.Child;
            while (child is Border nestedBorder)
            {
                child = nestedBorder.Child;
            }
            if (child is Grid innerGrid)
            {
                targetGrid = innerGrid;
            }
        }

        if (targetGrid != null)
        {
            Grid.SetRowSpan(s_dialogContainer, 10);
            Grid.SetColumnSpan(s_dialogContainer, 10);
            Panel.SetZIndex(s_dialogContainer, 1000);
            targetGrid.Children.Add(s_dialogContainer);
        }

        return s_dialogContainer;
    }

    internal static async Task<bool> ShowAsync(string title, string content, string primary, [Optional] string? close)
    {
        await s_semaphore.WaitAsync();
        try
        {
            var tcs = new TaskCompletionSource<bool>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var container = GetDialogContainer();

                s_currentDialog = FlarialDialog.Create(title, content, primary, close);
                s_currentDialog.Closed += result =>
                {
                    container.Children.Remove(s_currentDialog);
                    container.Visibility = Visibility.Collapsed;
                    s_currentDialog = null;
                    tcs.SetResult(result);
                };

                container.Children.Clear();
                container.Children.Add(s_currentDialog);
                container.Visibility = Visibility.Visible;
                s_currentDialog.Show();
            });

            return await tcs.Task;
        }
        finally
        {
            s_semaphore.Release();
        }
    }

    internal static async Task<bool> ShowAsync(MessageDialogContent content)
    {
        return await ShowAsync(content.Title, content.Content, content.Primary, content.Close);
    }
}
