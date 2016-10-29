using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace Notify
{
    public partial class MainWindow
    {
        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _breakTimer;
        private NotifyIcon _notifyIcon;
        private int _time;
        private int _breakTime = 0;
        private int _breakCount = 0;

        public MainWindow()
        {
            var proc = Process.GetCurrentProcess();
            var count = Process.GetProcesses().Count(p => p.ProcessName == proc.ProcessName);

            if (count > 1)
            {
                System.Windows.MessageBox.Show("Already an instance is running...");
                Close();
            }


            InitializeComponent();
            CenterWindow();
            InitializeNotifyIcon();

            TimeLabel.Content = "25:00";

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _breakTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _timer.Tick += Timer_Tick;
            _breakTimer.Tick += BreakTimer_Tick;
        }

        private void CenterWindow()
        {
            Left = (SystemParameters.PrimaryScreenWidth / 2) - (Width / 2);
            Top = (SystemParameters.PrimaryScreenHeight / 2) - (Height / 2);
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                ContextMenu = GetMenu(),
                Text = Properties.Resources.App_Name,
                Visible = true,
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)
            };


            _notifyIcon.MouseDoubleClick +=
                (sender, e) =>
                {

                    WindowState = (WindowState == WindowState.Normal ? WindowState.Minimized : WindowState.Normal);

                    if (WindowState == WindowState.Normal)
                    {
                        Activate();
                    }
                };
        }

        private ContextMenu GetMenu()
        {
            var open = new MenuItem("Open Notify");
            open.Click += (sender, e) => { WindowState = WindowState.Normal; };

            var stop = new MenuItem("Reset Notify");
            stop.Click += ResetButton_Click;
            stop.Enabled = false;

            var separator = new MenuItem("-");

            var exit = new MenuItem("Exit");

            exit.Click += (sender, e) =>
            {
                _notifyIcon.Visible = false;
                Application.Current.Shutdown();
            };

            var menu = new ContextMenu(new[] { open, stop, separator, exit });
            return menu;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_time > 0)
            {
                --_time;

                string time = $"{_time / 60}:{_time % 60:D2}";

                Dispatcher.Invoke(() => TimeLabel.Content = time);
                _notifyIcon.Text = Properties.Resources.App_Name + @" " + time;
            }
            else
            {
                _timer.Stop();
                DisableReset();
                Notify();
            }
        }

        private void BreakTimer_Tick(object sender, EventArgs e)
        {
            _breakTime++;
            string time = $"{_breakCount} break {_breakTime / 60}:{_breakTime % 60:D2}";
            Dispatcher.Invoke(() => BreakLabel.Content = time);
        }

        private void DisableReset()
        {
            _notifyIcon.ContextMenu.MenuItems[1].Enabled = false;
        }

        private void EnableStop()
        {
            _notifyIcon.ContextMenu.MenuItems[1].Enabled = true;
        }

        private void Notify()
        {
            Dispatcher.Invoke(() =>
            {
                TimeLabel.Content = "25:00";
                StartButton.IsEnabled = true;
                StartButton.Content = "Continue";
            });

            _breakCount++;
            _breakTime = 0;
            _breakTimer.Start();

            _notifyIcon.Text = Properties.Resources.App_Name;
            _notifyIcon.ShowBalloonTip(5000, Properties.Resources.App_Name, "Time is up", ToolTipIcon.Info);

            WindowState = WindowState.Normal;
            Activate();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            _timer.Stop();
            _breakTimer.Stop();
            _breakTime = 0;
            _breakCount = 0;
            BreakLabel.Content = "";
            DisableReset();
            StartButton.IsEnabled = true;
            StartButton.Content = "Start";
            ResetButton.IsEnabled = false;
            TimeLabel.Content = "25:00";
            _notifyIcon.Text = Properties.Resources.App_Name;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            ResetButton.IsEnabled = true;
            ResetButton.IsDefault = true;
            EnableStop();

            _time = 60 * 25;

            _breakTimer.Stop();
            _timer.Start();

            BreakLabel.Content = "";

            WindowState = WindowState.Minimized;
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            ShowInTaskbar = !WindowState.Equals(WindowState.Minimized);
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
        }
    }
}