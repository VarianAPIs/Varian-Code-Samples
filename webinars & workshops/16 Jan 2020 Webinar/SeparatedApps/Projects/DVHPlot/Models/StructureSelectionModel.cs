using DVHPlot.Events;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVHPlot.Models
{
    public class StructureSelectionModel:BindableBase
    {
        public string Id { get; set; }
        private bool bisChecked;
        private IEventAggregator _eventAggregator;

        public bool bIsChecked
        {
            get { return bisChecked; }
            set 
            { 
                SetProperty(ref bisChecked, value);
                _eventAggregator.GetEvent<StructureSelectionEvent>().Publish(this);
            }
        }
        public StructureSelectionModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

    }
}
