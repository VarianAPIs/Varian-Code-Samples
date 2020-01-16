using DoseMetricExample.Events;
using DoseMetricExample.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DoseMetricExample.ViewModels
{
    public class DoseMetricSelectionViewModel : BindableBase
    {
        private PlanSetup _plan;
        private IEventAggregator _eventAggregator;

        public List<Structure> Structures{ get; set; }
        private Structure selectedStructure;

        //public event PropertyChangedEventHandler PropertyChanged;

        public Structure SelectedStructure
        {
            get { return selectedStructure; }
            set { 
                SetProperty(ref selectedStructure, value);
                AddMetricCommand.RaiseCanExecuteChanged();
                //RaisePropertyChanged("SelectedStructure");
            }
        }

        //private void RaisePropertyChanged(string v)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(v));
        //    }
        //}

        public List<DoseMetricModel> DoseMetrics { get; set; }
        private DoseMetricModel selectedMetric;

        public DoseMetricModel SelectedMetric
        {
            get { return selectedMetric; }
            set 
            { 
                SetProperty(ref selectedMetric, value);
                AddMetricCommand.RaiseCanExecuteChanged();
                //RaisePropertyChanged("SelectedMetric");
            }
        }
        public DelegateCommand AddMetricCommand{ get; set; }
        public DoseMetricSelectionViewModel(PlanSetup plan,
            IEventAggregator eventAggregator)
        {
            _plan = plan;
            _eventAggregator = eventAggregator;
            DoseMetrics = new List<DoseMetricModel>();
            Structures = new List<Structure>(plan.StructureSet.Structures);
            AddMetricCommand = new DelegateCommand(OnAddMetric, CanAddMetric);
            SetDoseMetrics();

        }

        private void OnAddMetric()
        {
            SelectedMetric.Structure = SelectedStructure.Id;
            SelectedMetric.GetOutputValue();
            //MessageBox.Show($"{SelectedMetric.Metric} for {SelectedMetric.Structure} at {SelectedMetric.InputValue.ToString("F2")}{SelectedMetric.InputUnit} = {SelectedMetric.OutputValue.ToString("F2")}{SelectedMetric.OutputUnit}");
            _eventAggregator.GetEvent<AddDoseMetricEvent>().Publish(new DoseMetricModel(_plan)
            {
                Structure = SelectedMetric.Structure,
                Metric = SelectedMetric.Metric,
                InputValue = SelectedMetric.InputValue,
                InputUnit = SelectedMetric.InputUnit,
                OutputUnit = SelectedMetric.OutputUnit,
                OutputValue = SelectedMetric.OutputValue,
                Tolerance = SelectedMetric.Tolerance,
                ToleranceMet = SelectedMetric.ToleranceMet
            });
        }

        private bool CanAddMetric()
        {
            return SelectedStructure != null && SelectedMetric != null;
        }

        private void SetDoseMetrics()
        {
            DoseMetrics.Add(new DoseMetricModel(_plan)
            {
                Metric = "Dose At Volume",
                InputUnits = new List<string> { "cc", "%" },
                OutputUnits = new List<string> { "cGy", "%" },
                InputUnit = "cc",
                OutputUnit = "cGy"
            });
            DoseMetrics.Add(new DoseMetricModel(_plan)
            {
                Metric = "Volume At Dose"
                ,
                OutputUnits = new List<string> { "cc", "%" },
                InputUnits = new List<string> { "cGy", "%" },
                OutputUnit = "cc",
                InputUnit = "cGy"
            });
        }
    }

   
}
