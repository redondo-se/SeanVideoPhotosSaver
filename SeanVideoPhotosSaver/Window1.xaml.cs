using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SeanVideoPhotosSaver.Themes;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace SeanVideoPhotosSaver
{
    /// <summary>
    /// Interaction logic for Window1
    /// </summary>

    public partial class Window1 : System.Windows.Window
    {
        private ThemeBase _theme;

        public Window1()
        {
            InitializeComponent();
        }

        public void CleanShutdown()
        {
#if !DEBUG
            Gma.UserActivityMonitor.HookManager.MouseMove -= new System.Windows.Forms.MouseEventHandler(HookManager_MouseMove);
            Gma.UserActivityMonitor.HookManager.MouseDown -= new System.Windows.Forms.MouseEventHandler(HookManager_MouseDown);
#endif
            Gma.UserActivityMonitor.HookManager.KeyDown -= new System.Windows.Forms.KeyEventHandler(HookManager_KeyDown);

            if (_theme != null)
            {
                _theme.CleanShutdown();
            }

            FileNameProvider.Instance.SaveFileList();

            Application.Current.Shutdown(0);
        }

        void OnLoaded(object sender, EventArgs e)
        {

#if !DEBUG
            Topmost = true;
            Gma.UserActivityMonitor.HookManager.MouseMove += new System.Windows.Forms.MouseEventHandler(HookManager_MouseMove);
            Gma.UserActivityMonitor.HookManager.MouseDown += new System.Windows.Forms.MouseEventHandler(HookManager_MouseDown);
#endif

            Gma.UserActivityMonitor.HookManager.KeyDown += new System.Windows.Forms.KeyEventHandler(HookManager_KeyDown);

            // Get last theme from registry
            int lastTheme = 0;
            RegistryKey settingsKey = Registry.CurrentUser.OpenSubKey(Settings.KEY_PATH, true);
            if (settingsKey == null)
            {
                settingsKey = Registry.CurrentUser.CreateSubKey(Settings.KEY_PATH);
            }

            try
            {
                int tempval = (int)settingsKey.GetValue(Settings.LAST_THEME);
                lastTheme = tempval;
            }
            catch (Exception)
            {
                // do nothing
            }

            switch (lastTheme)
            {
                case 0:
                    _theme = new ThemeAllRandom(MainContainer);
                    lastTheme = 1;
                    break;
                case 1:
                    _theme = new ThemeCentered(MainContainer);
                    lastTheme = 2;
                    break;
                case 2:
                    _theme = new ThemeFullScreen(MainContainer);
                    lastTheme = 3;
                    break;
                case 3:
                    _theme = new ThemeTwoDiagonal(MainContainer);
                    lastTheme = 0;
                    break;
            }

            settingsKey.SetValue(Settings.LAST_THEME, lastTheme, RegistryValueKind.DWord);
        }

        void HookManager_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Left)
            {
                _theme.MoveBackward();
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.Right)
            {
                _theme.MoveForward();
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.M)
            {
                _theme.ToggleMute();
            }
            else
            {
                CleanShutdown();
            }
        }

        void HookManager_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            CleanShutdown();
        }

        void HookManager_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            CleanShutdown();
        }

    }
}