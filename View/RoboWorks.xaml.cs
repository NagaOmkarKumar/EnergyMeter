using Project_K.ViewModel;
using System.Timers;
using System.Windows;

namespace Project_K.View
{
    /// <summary>
    /// Interaction logic for RoboWorks.xaml
    /// </summary>
    /// 

    public partial class RoboWorks : Window
    {
       
        public RoboWorks()
        {
            InitializeComponent();
            DataContext = new RoboWorksVM();
          
        }

        private void MeterComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Cancel the close event to hide the window instead
            e.Cancel = true;

            // Minimize the window to the tray
            TrayIcon.MinimizeToTray(this);
            base.OnClosing(e);
        }

    }
}
