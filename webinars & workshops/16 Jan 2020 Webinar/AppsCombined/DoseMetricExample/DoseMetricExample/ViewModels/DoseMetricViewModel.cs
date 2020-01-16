using DoseMetricExample.Events;
using DoseMetricExample.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoseMetricExample.ViewModels
{
    public class DoseMetricViewModel
    {
        private IEventAggregator _eventAggregator;

        public ObservableCollection<DoseMetricModel> DoseMetrics { get; private set; }
        public DoseMetricViewModel(IEventAggregator eventAggregator )
        {
            _eventAggregator = eventAggregator;
            DoseMetrics = new ObservableCollection<DoseMetricModel>();
            _eventAggregator.GetEvent<AddDoseMetricEvent>().Subscribe(OnAddMetric);
        }

        private void OnAddMetric(DoseMetricModel obj)
        {
            DoseMetrics.Add(obj);
        }
    }
}
