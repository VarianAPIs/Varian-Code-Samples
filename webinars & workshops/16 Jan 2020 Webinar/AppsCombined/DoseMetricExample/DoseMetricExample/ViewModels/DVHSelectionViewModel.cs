using DVHPlot.Events;
using DVHPlot.Models;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace DVHPlot.ViewModels
{
    public class DVHSelectionViewModel : BindableBase
    {
        private PlanSetup _plan;
        private IEventAggregator _eventAggregator;
        public ObservableCollection<StructureSelectionModel> SelectionStructures {get; private set;}
        public DVHSelectionViewModel(PlanSetup plan,
            IEventAggregator eventAggregator)
        {
            _plan = plan;
            _eventAggregator = eventAggregator;
            SelectionStructures = new ObservableCollection<StructureSelectionModel>();
            SetInitialStructures();
        }

        private void SetInitialStructures()
        {
            foreach(Structure s in _plan.StructureSet.Structures.Where(x=>!x.IsEmpty && x.DicomType!="MARKER" && x.DicomType != "SUPPORT"))
            {
                SelectionStructures.Add(new StructureSelectionModel(_eventAggregator)
                {
                    Id = s.Id,
                    bIsChecked = _plan.StructuresSelectedForDvh.Contains(s)
                });
            }
        }
    }
}
