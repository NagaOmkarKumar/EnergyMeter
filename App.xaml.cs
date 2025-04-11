using Microsoft.Win32;
using System.Windows;
using System.Diagnostics;
using Project_K.View;

namespace Project_K
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //public static string UserId = string.Empty;

        static String databaseName = "MeterDetails0.db";
        static String folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static String databasePath = System.IO.Path.Combine(folderPath, databaseName);

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    // Initialize tray icon on application startup
        //    TrayIcon.Instance.InitializeTrayIcon();

        //    // Ensure the app starts with the main window visible and maximized
        //    //var mainWindow = new RoboWorks();
        //    //this.MainWindow = mainWindow;
        //    //mainWindow.WindowState = WindowState.Maximized;  // Set to maximized state
        //    //mainWindow.Show();
        //}

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize tray icon on application startup
            TrayIcon.Instance.InitializeTrayIcon();
            //var mainWindow = new RoboWorks();

            var mainWindow = System.Windows.Application.Current.MainWindow ?? new RoboWorks();
            // Ensure that the application does not shut down when the main window is closed
            //this.MainWindow.Closing += (sender, args) =>
            this.MainWindow = mainWindow;
            mainWindow.Closing += (sender, args) =>
            {
                args.Cancel = true; // Cancel the closing event
                TrayIcon.MinimizeToTray(this.MainWindow); // Minimize the window to tray
            };

            // Show the main window
           
           // this.MainWindow = mainWindow;
            //mainWindow.Show();
        }
    }

}
