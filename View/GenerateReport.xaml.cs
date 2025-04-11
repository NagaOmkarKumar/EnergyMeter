using Project_K.ViewModel;
using System.Windows;

namespace Project_K.View
{
    /// <summary>
    /// Interaction logic for GenerateReport.xaml
    /// </summary>
    public partial class GenerateReport : Window
    {
        public GenerateReport()
        {
            InitializeComponent();
            DataContext = new GenerateReportVM();
        }
    }
}
