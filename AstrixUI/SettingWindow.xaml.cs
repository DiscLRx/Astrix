using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HcWindow = HandyControl.Controls.Window;

namespace AstrixUI
{
    public partial class SettingWindow : HcWindow
    {
        public SettingWindow()
        {
            InitializeComponent();
            InitStartAtSystemBootCheckBox();
        }

        public void InitStartAtSystemBootCheckBox()
        {
            var exePath = Environment.ProcessPath;
            var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (registryKey is null || string.IsNullOrWhiteSpace(exePath))
            {
                return;
            }
            startAtSystemBootCheckBox.IsChecked = registryKey.GetValue("Astrix") as string == exePath;
        }

        private void StartAtSystemBootChecked(object sender, RoutedEventArgs e)
        {
            var exePath = Environment.ProcessPath;
            var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (registryKey is null || string.IsNullOrWhiteSpace(exePath))
            {
                return;
            }
            registryKey.SetValue("Astrix", exePath);
        }

        private void StartAtSystemBootUnchecked(object sender, RoutedEventArgs e)
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (registryKey is null)
            {
                return;
            }
            registryKey.DeleteValue("Astrix");
        }
    }
}
