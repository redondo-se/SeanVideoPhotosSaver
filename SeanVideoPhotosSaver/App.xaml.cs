using System;
using System.Windows;
using System.Data;
using System.Xml;
using System.Configuration;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Reflection;

namespace SeanVideoPhotosSaver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : System.Windows.Application
    {
        //[DllImport("faultrep.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        //private static extern bool AddERExcludedApplication(string name);
        [DllImport("kernel32.dll")]
        static extern ErrorModes SetErrorMode(ErrorModes uMode);

        [Flags]
        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }

        private Window1 _mainWindow;

        void OnStartup(Object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            System.Windows.Forms.Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.Current.Dispatcher.UnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Dispatcher_UnhandledException);

            SetErrorMode(ErrorModes.SEM_FAILCRITICALERRORS | ErrorModes.SEM_NOGPFAULTERRORBOX);

            //string fullExeName = System.Windows.Forms.Application.ExecutablePath;
            //string[] nameComponenents = fullExeName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            //string exeName = nameComponenents[nameComponenents.Length - 1];
            //bool exclusionWorked = AddERExcludedApplication(exeName);
            //int errorCode = Marshal.GetLastWin32Error();

            string[] args = e.Args;
            if (args.Length > 0)
            {
                // Get the 2 character command line argument
                string arg = args[0].ToLower(CultureInfo.InvariantCulture).Trim().Substring(0, 2);
                switch (arg)
                {
                    case "/c":
                        // Show the options dialog
                        Settings settings = new Settings();
                        settings.Show();
                        break;
                    case "/p":
                        // Don't do anything for preview
                        Application.Current.Shutdown(0);
                        break;
                    case "/s":
                        // Show screensaver form
                        ShowScreensaver();
                        break;
                    default:
                        Application.Current.Shutdown(0);
                        break;
                }
            }
            else
            {
                // If no arguments were passed in, show the screensaver
                ShowScreensaver();
            }

        }

        void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _mainWindow.CleanShutdown();
        }

        void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            _mainWindow.CleanShutdown();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _mainWindow.CleanShutdown();
        }

        void ShowScreensaver()
        {
            //creates window on primary screen
            _mainWindow = new Window1();
            _mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            System.Drawing.Rectangle location = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            _mainWindow.WindowState = WindowState.Maximized;
            //_mainWindow.WindowState = WindowState.Normal;

            //creates window on other screens
            //foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            //{
            //    if (screen == System.Windows.Forms.Screen.PrimaryScreen)
            //        continue;

            //    Window1 window = new Window1();
            //    window.WindowStartupLocation = WindowStartupLocation.Manual;
            //    location = screen.Bounds;

            //    //covers entire monitor
            //    window.Left = location.X - 7;
            //    window.Top = location.Y - 7;
            //    window.Width = location.Width + 14;
            //    window.Height = location.Height + 14;

            //}

            ////show non-primary screen windows
            //foreach (Window window in System.Windows.Application.Current.Windows)
            //{
            //    if (window != primaryWindow)
            //        window.Show();
            //}

            ///shows primary screen window last
            _mainWindow.Show();
        }




    }


}