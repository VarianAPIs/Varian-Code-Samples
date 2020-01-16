using DoseMetrics.Events;
using DoseMetrics.Models;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoseMetrics.ViewModels
{
    public class DoseMetricViewModel
    {
        private IEventAggregator _eventAggregator;

        public ObservableCollection<DoseMetricModel> DoseMetrics { get; private set; }
        public DoseMetricViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DoseMetrics = new ObservableCollection<DoseMetricModel>();
            _eventAggregator.GetEvent<AddDoseMetricEvent>().Subscribe(OnAddDoseMetric);
        }

        private void OnAddDoseMetric(DoseMetricModel dm)
        {
            DoseMetrics.Add(dm);
        }
    }
}
