using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVHPlot.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel(DVHViewModel dVHViewModel,
            DVHSelectionViewModel dVHSelectionViewModel)
        {
            DVHViewModel = dVHViewModel;
            DVHSelectionViewModel = dVHSelectionViewModel;
        }

        public DVHViewModel DVHViewModel { get; }
        public DVHSelectionViewModel DVHSelectionViewModel { get; }
    }
}
