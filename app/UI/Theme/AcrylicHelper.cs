using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Flarial.Launcher.UI.Theme;

/// <summary>
/// Helper class for enabling acrylic/blur effects on WPF windows using Windows APIs.
/// Supports Windows 10 1809+ for acrylic and Windows 11 for Mica.
/// </summary>
static class AcrylicHelper
{
    #region Win32 Interop

    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public uint GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    private enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    private enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5
    }

    // DWM attributes for Windows 11
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_MICA_EFFECT = 1029;
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

    #endregion

    /// <summary>
    /// Enables the acrylic blur effect on a window.
    /// Falls back to transparent gradient on older systems.
    /// </summary>
    public static void EnableAcrylic(Window window, Color tintColor, double tintOpacity = 0.7)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            window.SourceInitialized += (s, e) => EnableAcrylic(window, tintColor, tintOpacity);
            return;
        }

        // Enable dark mode for title bar
        EnableDarkMode(hwnd);

        // Try Windows 11 Mica first
        if (TryEnableMica(hwnd))
            return;

        // Fall back to acrylic blur (Windows 10 1809+)
        if (TryEnableAcrylicBlur(hwnd, tintColor, tintOpacity))
            return;

        // Final fallback: just use transparent gradient
        TryEnableTransparentGradient(hwnd, tintColor, tintOpacity);
    }

    /// <summary>
    /// Enables a simple blur effect (less intensive than acrylic).
    /// </summary>
    public static void EnableBlur(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            window.SourceInitialized += (s, e) => EnableBlur(window);
            return;
        }

        EnableDarkMode(hwnd);

        var accent = new AccentPolicy
        {
            AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND,
            AccentFlags = 2,
            GradientColor = 0x00000000
        };

        SetAccentPolicy(hwnd, accent);
    }

    /// <summary>
    /// Disables any blur/acrylic effects on the window.
    /// </summary>
    public static void DisableEffects(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;

        var accent = new AccentPolicy
        {
            AccentState = AccentState.ACCENT_DISABLED
        };

        SetAccentPolicy(hwnd, accent);
    }

    /// <summary>
    /// Extends the window frame into the client area for borderless windows.
    /// </summary>
    public static void ExtendFrameIntoClientArea(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            window.SourceInitialized += (s, e) => ExtendFrameIntoClientArea(window);
            return;
        }

        var margins = new MARGINS { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        DwmExtendFrameIntoClientArea(hwnd, ref margins);
    }

    private static void EnableDarkMode(IntPtr hwnd)
    {
        var darkMode = 1;
        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
    }

    private static bool TryEnableMica(IntPtr hwnd)
    {
        try
        {
            // Windows 11 22H2+ uses DWMWA_SYSTEMBACKDROP_TYPE
            var backdropType = 2; // DWMSBT_MAINWINDOW (Mica)
            var result = DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));

            if (result == 0) return true;

            // Try older Windows 11 attribute
            var micaValue = 1;
            result = DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref micaValue, sizeof(int));
            return result == 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryEnableAcrylicBlur(IntPtr hwnd, Color tintColor, double tintOpacity)
    {
        try
        {
            var alpha = (byte)(tintOpacity * 255);
            var gradientColor = (uint)((alpha << 24) | (tintColor.B << 16) | (tintColor.G << 8) | tintColor.R);

            var accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                AccentFlags = 2,
                GradientColor = gradientColor
            };

            return SetAccentPolicy(hwnd, accent);
        }
        catch
        {
            return false;
        }
    }

    private static bool TryEnableTransparentGradient(IntPtr hwnd, Color tintColor, double tintOpacity)
    {
        try
        {
            var alpha = (byte)(tintOpacity * 255);
            var gradientColor = (uint)((alpha << 24) | (tintColor.B << 16) | (tintColor.G << 8) | tintColor.R);

            var accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT,
                AccentFlags = 2,
                GradientColor = gradientColor
            };

            return SetAccentPolicy(hwnd, accent);
        }
        catch
        {
            return false;
        }
    }

    private static bool SetAccentPolicy(IntPtr hwnd, AccentPolicy accent)
    {
        var accentPtr = Marshal.AllocHGlobal(Marshal.SizeOf(accent));
        try
        {
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = Marshal.SizeOf(accent),
                Data = accentPtr
            };

            var result = SetWindowCompositionAttribute(hwnd, ref data);
            return result == 0;
        }
        finally
        {
            Marshal.FreeHGlobal(accentPtr);
        }
    }
}
