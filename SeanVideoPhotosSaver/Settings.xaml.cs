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
using Microsoft.Win32;

namespace SeanVideoPhotosSaver
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>

    public partial class Settings : System.Windows.Window
    {
        public const string KEY_PATH = @"Software\SeanVideoPhotosSaver";
        public const string IMAGE_PATH = "ImageAndVideoFolder";
        public const string LAST_THEME = "LastThemeIndex";

        public Settings()
        {
            InitializeComponent();

            RegistryKey settingsKey = Registry.CurrentUser.OpenSubKey(Settings.KEY_PATH, true);
            if (settingsKey != null)
            {
                FileNameTextBox.Text = (string)(settingsKey.GetValue(Settings.IMAGE_PATH));
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.SelectedPath = FileNameTextBox.Text;
            string folder = null;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folder = fbd.SelectedPath;
            }

            FileNameTextBox.Text = folder;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FileNameTextBox.Text))
            {
                RegistryKey settingsKey = Registry.CurrentUser.OpenSubKey(KEY_PATH, true);
                if (settingsKey == null)
                {
                    settingsKey = Registry.CurrentUser.CreateSubKey(KEY_PATH);
                }

                settingsKey.SetValue(IMAGE_PATH, FileNameTextBox.Text, RegistryValueKind.String);
            }
            Application.Current.Shutdown(0);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

    }
}