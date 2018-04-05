using System.Windows.Controls;
using VMS.TPS;

namespace PatientSummary
{
    public partial class MainView : UserControl
    {
        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
