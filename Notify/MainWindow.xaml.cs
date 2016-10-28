using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Notify
{
    public partial class MainWindow
    {
        private readonly DispatcherTimer _timer;
        private NotifyIcon _notifyIcon;
        private int _time;

        public MainWindow()
        {
            Process proc = Process.GetCurrentProcess();
            int count = Process.GetProcesses().Where(p =>
                             p.ProcessName == proc.ProcessName).Count();
            if (count > 1)
            {
                System.Windows.MessageBox.Show("Already an instance is running...");
                Close();
            }


            InitializeComponent();
            CenterWindow();
            InitializeNotifyIcon();

            TimeLabel.Content = 0;

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _timer.Tick += Timer_Tick;
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

            var stop = new MenuItem("Stop Notify");
            stop.Click += StopButton_Click;
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

        private void DeleteNumber()
        {
            if ((int)TimeLabel.Content < 10)
            {
                TimeLabel.Content = 0;
                StartButton.IsEnabled = false;
            }
            else
                TimeLabel.Content = (int)TimeLabel.Content / 10;
        }

        private void AddNumber(int number)
        {
            if (TimeLabel.Content.ToString().Length >= 2 || (TimeLabel.Content.Equals(0) && number >= 6) ||
                (TimeLabel.Content.Equals(0) && number.Equals(0)))
                return;

            TimeLabel.Content = (int)TimeLabel.Content * 10 + number;
            StartButton.IsEnabled = true;
            StartButton.IsDefault = true;
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
                DisableStop();
                Notify();
            }
        }

        private void DisableStop()
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
                StopButton.IsEnabled = false;
                TimeLabel.Content = 0;
            });

            _notifyIcon.Text = Properties.Resources.App_Name;
            _notifyIcon.ShowBalloonTip(5000, Properties.Resources.App_Name, "Time is up", ToolTipIcon.Info);
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            _timer.Stop();
            DisableStop();
            StopButton.IsEnabled = false;
            TimeLabel.Content = 0;
            _notifyIcon.Text = Properties.Resources.App_Name;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            StopButton.IsDefault = true;
            EnableStop();

            _time = (int)TimeLabel.Content * 60;
            TimeLabel.Content += ":00";

            _timer.Start();

            WindowState = WindowState.Minimized;
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_timer.IsEnabled)
                return;

            switch (e.Key)
            {
                case Key.NumPad0:
                case Key.D0:
                    AddNumber(0);
                    break;
                case Key.NumPad1:
                case Key.D1:
                    AddNumber(1);
                    break;
                case Key.NumPad2:
                case Key.D2:
                    AddNumber(2);
                    break;
                case Key.NumPad3:
                case Key.D3:
                    AddNumber(3);
                    break;
                case Key.NumPad4:
                case Key.D4:
                    AddNumber(4);
                    break;
                case Key.NumPad5:
                case Key.D5:
                    AddNumber(5);
                    break;
                case Key.NumPad6:
                case Key.D6:
                    AddNumber(6);
                    break;
                case Key.NumPad7:
                case Key.D7:
                    AddNumber(7);
                    break;
                case Key.NumPad8:
                case Key.D8:
                    AddNumber(8);
                    break;
                case Key.NumPad9:
                case Key.D9:
                    AddNumber(9);
                    break;
                case Key.Back:
                    DeleteNumber();
                    break;
            }
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