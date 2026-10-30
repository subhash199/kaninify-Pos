using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Maui.ApplicationModel;

namespace EposRetail.Services;

/// <summary>
/// Opens the customer-facing display window and (on Windows) places it on the secondary
/// monitor when one is present.
/// </summary>
public static class CustomerDisplayWindow
{
    public static void Open()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        // Only ever keep a single customer display window open.
        if (app.Windows.Any(window => window.Page is CustomerDisplayPage))
        {
            return;
        }

#if WINDOWS
        // Only show the customer display when a second monitor is actually connected;
        // otherwise it would open on top of the POS window.
        if (GetSecondaryMonitorBounds() is null)
        {
            return;
        }
#endif

        var displayWindow = new Window(new CustomerDisplayPage())
        {
            Title = "Powered by Kaninify"
        };

#if WINDOWS
        // Position once, the first time the window is activated (which happens after the
        // shared window-created lifecycle has run).
        bool positioned = false;
        displayWindow.Activated += (sender, _) =>
        {
            if (positioned || sender is not Window window)
            {
                return;
            }

            positioned = true;
            MainThread.BeginInvokeOnMainThread(() => PositionOnSecondaryDisplay(window));
        };
#endif

        app.OpenWindow(displayWindow);
    }

    public static void CloseAll()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        foreach (var window in app.Windows.Where(w => w.Page is CustomerDisplayPage).ToList())
        {
            app.CloseWindow(window);
        }
    }

#if WINDOWS
    private static void PositionOnSecondaryDisplay(Window window)
    {
        try
        {
            if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window platformWindow)
            {
                return;
            }

            var secondary = GetSecondaryMonitorBounds();
            if (secondary is null)
            {
                // Single-monitor setups keep the default position.
                return;
            }

            var appWindow = platformWindow.AppWindow;

            // A maximized window cannot be moved, so restore before repositioning.
            if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
            {
                presenter.Restore();
            }

            appWindow.MoveAndResize(secondary.Value);

            if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter restoredPresenter)
            {
                restoredPresenter.Maximize();
            }

            // Keep input focus on the POS window so the display does not steal the keyboard.
            var posWindow = Application.Current?.Windows.FirstOrDefault(w => w.Page is MainPage);
            if (posWindow?.Handler?.PlatformView is Microsoft.UI.Xaml.Window posPlatformWindow)
            {
                posPlatformWindow.Activate();
            }
        }
        catch (Exception)
        {
            // Positioning is best-effort; the window still opens if a second monitor is unavailable.
        }
    }

    private static Windows.Graphics.RectInt32? GetSecondaryMonitorBounds()
    {
        var monitors = new List<MonitorInfo>();

        bool Callback(IntPtr hMonitor, IntPtr hdc, ref MonitorRect rect, IntPtr data)
        {
            var info = new MonitorInfo { CbSize = Marshal.SizeOf<MonitorInfo>() };
            if (GetMonitorInfo(hMonitor, ref info))
            {
                monitors.Add(info);
            }

            return true;
        }

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Callback, IntPtr.Zero);

        if (monitors.Count < 2)
        {
            return null;
        }

        var index = monitors.FindIndex(m => (m.Flags & MonitorInfo.PrimaryFlag) == 0);
        var bounds = monitors[index >= 0 ? index : 1].Monitor;
        return new Windows.Graphics.RectInt32(
            bounds.Left,
            bounds.Top,
            bounds.Right - bounds.Left,
            bounds.Bottom - bounds.Top);
    }

    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref MonitorRect rect, IntPtr data);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clip, MonitorEnumProc callback, IntPtr data);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo info);

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public const uint PrimaryFlag = 0x1;

        public int CbSize;
        public MonitorRect Monitor;
        public MonitorRect Work;
        public uint Flags;
    }
#endif
}
