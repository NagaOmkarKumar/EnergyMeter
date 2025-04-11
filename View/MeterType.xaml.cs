using Project_K.ViewModel;
using System.Windows;

namespace Project_K.View
{
    /// <summary>
    /// Interaction logic for MeterType.xaml
    /// </summary>
    public partial class MeterType : Window
    {
        public MeterType()
        {
            InitializeComponent();
            DataContext = new MeterVM();
        }
    }
}
