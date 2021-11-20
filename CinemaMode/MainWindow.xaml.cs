using Alex.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfScreenHelper;

namespace CinemaMode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            WindowRectange = new();

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            this.LocationChanged += MainWindow_LocationChanged;
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) => ApplyCurrentSize();

        private void MainWindow_LocationChanged(object? sender, EventArgs e) => ApplyCurrentLocation();

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var window in Windows)
            {
                window.Close();
            }
        }

        private readonly Rectangle WindowRectange;

        private void ApplyCurrentLocation()
        {
            WindowRectange.Visibility = Visibility.Collapsed;
            Canvas.SetLeft(WindowRectange, Left / 10 + VirtualScreenWidthHalfScaled + 1);
            Canvas.SetTop(WindowRectange, Top / 10 + VirtualScreenHeightHalfScaled + 1);
            WindowRectange.Visibility = Visibility.Visible;
        }

        private void ApplyCurrentSize()
        {
            WindowRectange.Width = Width / 10 - 2;
            WindowRectange.Height = Height / 10 - 2;
        }

        private double VirtualScreenWidthHalfScaled;
        private double VirtualScreenHeightHalfScaled;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            int margin = 1;
            VirtualScreenWidthHalfScaled = SystemParameters.VirtualScreenWidth / 20;
            VirtualScreenHeightHalfScaled = SystemParameters.VirtualScreenHeight / 20;

            int i = 0;

            foreach (var screen in Screen.AllScreens)
            {
                Rectangle r = new();
                r.RadiusX = 10;
                r.RadiusY = 10;
                Canvas.SetLeft(r, screen.Bounds.Left / 10 + VirtualScreenWidthHalfScaled + margin);
                Canvas.SetTop(r, screen.Bounds.Top / 10 + VirtualScreenHeightHalfScaled + margin);
                r.Width = screen.Bounds.Width / 10 - 2 * margin;
                r.Height = screen.Bounds.Height / 10 - 2 * margin;
                
                r.InputBindings.Add(new MouseBinding(new MyCommand<Shape>(DisableAllOtherScreens, r), new(MouseAction.LeftClick)));
                r.Tag = screen.DeviceName;
                r.Fill = Brushes.Green;
                r.Stroke = Brushes.Red;
                r.Visibility = Visibility.Visible;

                Canvas.Children.Add(r);
            }

            WindowRectange.RadiusX = 10;
            WindowRectange.RadiusY = 10;
            WindowRectange.Fill = Brushes.White;
            WindowRectange.Stroke = Brushes.Black;
            WindowRectange.Visibility = Visibility.Visible;
            ApplyCurrentLocation();
            ApplyCurrentSize();
            Canvas.Children.Add(WindowRectange);

            foreach (var screen in Screen.AllScreens)
            {
                TextBlock textBlock = new();
                textBlock.Text = (++i).ToString();// screen.DeviceName.Split("DISPLAY")[1];
                textBlock.Width = 30;
                textBlock.Height = 15;
                textBlock.TextAlignment = TextAlignment.Center;
                textBlock.Visibility = Visibility.Visible;
                textBlock.FontWeight = FontWeights.UltraBold;

                Canvas.Children.Add(textBlock);
                Canvas.SetLeft(textBlock, screen.Bounds.Left / 10 + VirtualScreenWidthHalfScaled + screen.Bounds.Width / 20 - textBlock.Width / 2);
                Canvas.SetTop(textBlock, screen.Bounds.Top / 10 + VirtualScreenHeightHalfScaled + screen.Bounds.Height / 20 - textBlock.Height / 2);
            }

        }

        //private void DisableAllOtherScreens(Shape shape) => shape.Fill = shape.Fill == Brushes.Yellow ? Brushes.Black : Brushes.Yellow;

        private void DisableAllOtherScreens(Shape shape)
        {
            foreach (var screen in WpfScreenHelper.Screen.AllScreens.ToList())
            {
                var deviceName = (string)shape.Tag;
                if (screen.DeviceName != deviceName)
                {
                    DisableScreen(screen);
                }
            }
        }

        const int DM_POSITION = 0x20;

        const int DM_PELSWIDTH = 0x80000;
        const int DM_PELSHEIGHT = 0x100000;

        private List<Window> Windows = new();


        private void DisableScreen(Screen screen)
        {
            var screenWin = new Window();
            Windows.Add(screenWin);
            screenWin.Background = Brushes.Black;
            screenWin.Width = screen.Bounds.Width;
            screenWin.Height = screen.Bounds.Height;
            screenWin.Left = screen.Bounds.Left;
            screenWin.Top = screen.Bounds.Top;
            screenWin.Topmost = true;
            screenWin.BorderBrush = Brushes.Black;
            screenWin.BorderThickness = new(0);
            screenWin.ShowInTaskbar = false;
            screenWin.WindowStyle = WindowStyle.None;
            screenWin.ResizeMode = ResizeMode.NoResize;
            //screenWin.bord
            screenWin.Show();
        }

        private void DisableScreenByAttaching(string deviceName)
        {
            DEVMODE devMode = new();
            devMode.dmSize = (short)Marshal.SizeOf<DEVMODE>();
            devMode.dmDriverExtra = 0;
            devMode.dmFields = DM_POSITION | DM_PELSHEIGHT | DM_PELSWIDTH;
            devMode.dmPelsWidth = 0;
            devMode.dmPelsHeight = 0;

            //POINTL delete = new();
            //delete.x = 0;
            //delete.y = 0;
            //devMode.dmPosition = delete;

            DEVMODE devModeZero = new();

            //A pointer to a DEVMODE structure that describes the new graphics mode.If lpDevMode is NULL, all the values currently in the registry will be used for the display setting.Passing NULL for the lpDevMode parameter and 0 for the dwFlags parameter is the easiest way to return to the default mode after a dynamic mode change.
            DEVMODE dmTemp;
            var res = User32.EnumDisplaySettings(deviceName, ENUM_IMODENUM.ENUM_REGISTRY_SETTINGS, out dmTemp);

            dmTemp.dmFields = DM_POSITION | DM_PELSHEIGHT | DM_PELSWIDTH;
            dmTemp.dmPelsWidth = 0;
            dmTemp.dmPelsHeight = 0;


            User32.ChangeDisplaySettingsEx(deviceName, ref dmTemp, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET, IntPtr.Zero);

            User32.ChangeDisplaySettingsEx(null, ref devModeZero, IntPtr.Zero, 0, IntPtr.Zero);
            //ChangeDisplaySettingsEX
            //PInvoke.User32.changedis
        }

        private class MyCommand<T> : ICommand
        {
            public MyCommand(Action<T> action, T parameter) => (Action, Parameter) = (action, parameter);

            public Action<T> Action { get; }
            public T Parameter { get; }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter) => true;

            public void Execute(object? parameter)
            {
                Action(Parameter);
            }
        }
    }
}
