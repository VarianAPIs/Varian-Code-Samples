using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamDataVisualization.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel(PatientNavigationViewModel patientNavigationViewModel,
            ScanPlotViewModel scanPlotViewModel)
        {
            PatientNavigationViewModel = patientNavigationViewModel;
            ScanPlotViewModel = scanPlotViewModel;
        }

        public PatientNavigationViewModel PatientNavigationViewModel { get; }
        public ScanPlotViewModel ScanPlotViewModel { get; }
    }
}
