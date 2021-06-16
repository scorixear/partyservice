using PartyService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace PartyService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<PlayerData> PlayerSource = new ObservableCollection<PlayerData>();

        private List<string> Log = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(0);
        }

        private void RefreshMaximizeRestoreButton()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.maximizeButton.Visibility = Visibility.Collapsed;
                this.restoreButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.maximizeButton.Visibility = Visibility.Visible;
                this.restoreButton.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            this.RefreshMaximizeRestoreButton();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
        }

        public static IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                // We need to tell the system what our size should be when maximized. Otherwise it will cover the whole screen,
                // including the task bar.
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                // Adjust the maximized size and position to fit the work area of the correct monitor
                IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = new MONITORINFO();
                    monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    GetMonitorInfo(monitor, ref monitorInfo);
                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;
                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                    mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }

            return IntPtr.Zero;
        }

        private const int WM_GETMINMAXINFO = 0x0024;

        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Config config = Config.instance;
            PartyLeaver logger = null;
            try
            {
                logger = new PartyLeaver();
                logger.PartyService.PlayerLeftEvent += PartyService_PlayerLeftEvent;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.StackTrace);
            }
            PlayerDataGrid.ItemsSource = PlayerSource;
        }

        private void PartyService_PlayerLeftEvent(string playerName)
        {
            DateTime now = DateTime.UtcNow;
            PlayerSource.Add(new PlayerData { PlayerName = now.ToString("[HH:mm] ") + playerName });
            Log.Add(now.ToString("[yyyy/MM/dd HH:mm:ss] ") + playerName);
            Dispatcher.Invoke(() =>
            {
                PlayerDataGrid.ItemsSource = null;
                PlayerDataGrid.ItemsSource = PlayerSource;
            });
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Topmost = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PlayerSource.Remove((PlayerData)((Button)sender).DataContext);
            PlayerDataGrid.ItemsSource = null;
            PlayerDataGrid.ItemsSource = PlayerSource;
        }

        private void SaveLog_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(Path.Combine(Assembly.GetExecutingAssembly().Location, "Logs")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Logs"));
            }
            if (Log.Count > 0)
            {
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"PartyLog_{DateTime.UtcNow.ToString("HH-mm-ss_yyyy-MM-dd")}.txt"),
                Log.Aggregate((a, b) => a + "\n" + b));
            }
            Log.Clear();
            PlayerSource.Clear();
            PlayerDataGrid.ItemsSource = null;
            PlayerDataGrid.ItemsSource = PlayerSource;
        }
        public class PlayerData
        {
            public string PlayerName { get; set; }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PartyService_PlayerLeftEvent("Test " + PlayerSource.Count);
        }
    }
}
