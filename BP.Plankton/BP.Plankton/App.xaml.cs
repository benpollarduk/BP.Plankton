using System;
using System.Windows;
using Plankton.IO;

namespace Plankton
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var window = new MainWindow();

            if (e.Args.Length > 0)
            {
                try
                {
                    var arg = e.Args[0];
                    var file = PlanktonSettingsFile.Open(arg);
                    window.StartupFile = file;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"The startup argument provided was not a valid Plankton Settings File: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            MainWindow = window;
            MainWindow.Show();
        }
    }
}