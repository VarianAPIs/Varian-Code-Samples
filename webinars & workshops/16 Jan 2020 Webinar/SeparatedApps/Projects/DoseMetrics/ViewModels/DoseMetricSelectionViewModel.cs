using DoseMetrics.Events;
using DoseMetrics.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DoseMetrics.ViewModels
{
    public class DoseMetricSelectionViewModel : BindableBase
    {
        private PlanSetup _plan;
        private IEventAggregator _eventAggregator;
        #region properties
        public List<Structure> Structures { get; private set; }
        private Structure selectedStructure;

        public Structure SelectedStructure
        {
            get { return selectedStructure; }
            set { SetProperty(ref selectedStructure, value);
                AddMetricCommand.RaiseCanExecuteChanged();
            }
        }

        public List<DoseMetricModel> DoseMetrics { get; private set; }
        private DoseMetricModel selectedMetric;
        public DoseMetricModel SelectedMetric
        {
            get { return selectedMetric; }
            set
            {
                SetProperty(ref selectedMetric, value);
                SelectedMetric.InputUnit = SelectedMetric.InputUnits.First();
                SelectedMetric.OutputUnit = SelectedMetric.OutputUnits.First();
                AddMetricCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion properties
        public DelegateCommand AddMetricCommand { get; private set; }
        public DoseMetricSelectionViewModel(PlanSetup plan, 
            IEventAggregator eventAggregator)
        {
            _plan = plan;
            _eventAggregator = eventAggregator;
            Structures = new List<Structure>(plan.StructureSet.Structures);
            DoseMetrics = new List<DoseMetricModel>();
            SetDoseMetricModels();
            AddMetricCommand = new DelegateCommand(OnAddMetric, CanAddMetric);
        }

        private void OnAddMetric()
        {
            if (SelectedMetric != null && SelectedStructure!=null)
            {
                SelectedMetric.Structure = SelectedStructure.Id;
                SelectedMetric.GetOutputValue();
                _eventAggregator.GetEvent<AddDoseMetricEvent>().Publish(new DoseMetricModel(_plan)
                {
                    Structure = SelectedMetric.Structure,
                    Metric = SelectedMetric.Metric,
                    InputUnit = SelectedMetric.InputUnit,
                    OutputUnit = SelectedMetric.OutputUnit,
                    InputValue = SelectedMetric.InputValue,
                    OutputValue = SelectedMetric.OutputValue,
                    Tolerance = SelectedMetric.Tolerance,
                    ToleranceMet = SelectedMetric.ToleranceMet
                });
                //MessageBox.Show($"{SelectedMetric.Metric} for {SelectedMetric.Structure.Id} at {SelectedMetric.InputValue.ToString("F2")}{SelectedMetric.InputUnit} = {SelectedMetric.OutputValue.ToString("F2")}{SelectedMetric.OutputUnit}");
            }
        }

        private bool CanAddMetric()
        {
            return SelectedStructure != null && SelectedMetric != null;
        }

        private void SetDoseMetricModels()
        {
            DoseMetrics.Add(new DoseMetricModel(_plan)
            {
                Metric = "Dose At Volume",
                InputUnits = new List<string> { "cc", "%" },
                OutputUnits = new List<string> { "cGy", "%" },
            });
            DoseMetrics.Add(new DoseMetricModel(_plan)
            {
                Metric = "Volume At Dose",
                InputUnits = new List<string> { "cGy", "%" },
                OutputUnits = new List<string> { "cc", "%" },
            });
        }
    }

   
}
