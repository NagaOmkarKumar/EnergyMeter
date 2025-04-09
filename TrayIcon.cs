using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using Project_K.View;
using System.Diagnostics;

namespace Project_K
{
    public class TrayIcon
    {
        private static NotifyIcon? _trayIcon;
        private System.Windows.Forms.ContextMenuStrip _trayMenu;
        public static TrayIcon Instance { get; } = new TrayIcon();
        // Constructor
        public TrayIcon()
        {
            InitializeTrayIcon();
            SetAppAutoStart();
        }
     
        // Initialize Tray Icon and menu
        public void InitializeTrayIcon()
        {
            if (_trayIcon != null) return;
            // Create the context menu for the tray icon
            _trayMenu = new System.Windows.Forms.ContextMenuStrip();
            _trayMenu.Items.Add("Open", null, OnOpenClicked);
            _trayMenu.Items.Add("Exit", null, OnExitClicked);
            
            // Initialize the tray icon with an icon and menu
            _trayIcon = new NotifyIcon
            {
                //Icon = new Icon("D:\\WPF_Projects\\Project K\\Debug\\Project K\\View\\assets\\JBM_ICON.ico"),
                Icon = new Icon("View/assets/JBM_ICON.ico"),
                //Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Project_K.View.assets.JBM_ICON.ico")),

                // Set tray icon
                ContextMenuStrip = _trayMenu,
                Visible = true,
                Text = "EnergyMeter"
            };

            // Handle double-click event to show window
            _trayIcon.DoubleClick += TrayIcon_DoubleClick;
        }

        // Set the app to auto-start with Windows
        private void SetAppAutoStart()
        {
            string appName = "Project_K";  // Your app name
            string appPath = Assembly.GetEntryAssembly().Location;
            string appExePath = appPath.Substring(0, appPath.LastIndexOf("\\") + 1) + "Project K.exe";
            Trace.WriteLine(appPath);
            Trace.WriteLine(appExePath);

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            // Check if the entry already exists
            if (registryKey.GetValue(appName) == null)
            {
                registryKey.SetValue(appName, appExePath); // Set the auto-start registry key
            }
        }

        // Event handler for double-clicking the tray icon (show window)
        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        // Event handler for clicking "Open" in the tray menu (show window)
        private void OnOpenClicked(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        // Event handler for clicking "Exit" in the tray menu (exit app)
        private void OnExitClicked(object sender, EventArgs e)
        {
            ExitApplication();
        }

        // Show the main window
        private void ShowMainWindow()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow;

            if (mainWindow == null)
            {
                mainWindow = new RoboWorks();
                System.Windows.Application.Current.MainWindow = mainWindow; // Set the main window of the app
                mainWindow.Show();
            }
           // var mainWindow1 = new RoboWorks();
           // mainWindow1.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
            mainWindow.Show();
        }

        // Exit the application
        private void ExitApplication()
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose(); // Properly dispose of the tray icon
            }// Hide tray icon
            System.Windows.Application.Current.Shutdown(); // Shut down the application
        }

        // Minimize the window to the tray
        public static void MinimizeToTray(Window window)
        {
            window.WindowState = WindowState.Minimized;
            window.Hide();
            if (_trayIcon == null)
            {
                Instance.InitializeTrayIcon(); // Ensure the tray icon is initialized
            }

           // _trayIcon.Icon = View/asset;
            // _trayIcon.Visible = true;
        }
    }
}
