using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DoseMetrics.Models
{
    public class DoseMetricModel : BindableBase
    {
        public string Structure { get; set; }
        public string Metric { get; set; }
        public double InputValue { get; set; }
        public List<string> InputUnits { get; set; }

        private string inputUnit;

        public string InputUnit
        {
            get { return inputUnit; }
            set { SetProperty(ref inputUnit, value); }
        }

        public List<string> OutputUnits { get; set; }
        private string outputUnit;
        public string OutputUnit
        {
            get { return outputUnit; }
            set { SetProperty(ref outputUnit, value); }
        }

        public double OutputValue { get; set; }
        private string tolerance;

        public string Tolerance
        {
            get { return tolerance; }
            set { SetProperty(ref tolerance, value); }
        }

        public bool ToleranceMet { get; set; }
        private PlanSetup _plan;
        public DoseMetricModel(PlanSetup plan)
        {
            _plan = plan;
        }

        internal void GetOutputValue()
        {
            //put methods here.
            if (Metric == "Dose At Volume")
            {
                OutputValue = _plan.GetDoseAtVolume(_plan.StructureSet.Structures.FirstOrDefault(x=>x.Id == Structure),
                    InputValue,
                    InputUnit == "%" ? VolumePresentation.Relative : VolumePresentation.AbsoluteCm3,
                    OutputUnit == "%" ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute).Dose;

            }
            else if (Metric == "Volume At Dose")
            {
                OutputValue = _plan.GetVolumeAtDose(_plan.StructureSet.Structures.FirstOrDefault(x => x.Id == Structure),
                    new DoseValue(InputValue, (InputUnit == "%" ? DoseValue.DoseUnit.Percent : DoseValue.DoseUnit.cGy)),
                    OutputUnit == "%" ? VolumePresentation.Relative : VolumePresentation.AbsoluteCm3);
            }
            else { throw new ApplicationException("Could not determine metric"); }
            if (Tolerance.Contains("<"))
            {
                ToleranceMet = OutputValue < Convert.ToDouble(Tolerance.TrimStart('<'));
            }
            else if (Tolerance.Contains(">"))
            {
                ToleranceMet = OutputValue > Convert.ToDouble(Tolerance.TrimStart('>'));
            }
            else if (Tolerance.Contains("="))
            {
                ToleranceMet = OutputValue == Convert.ToDouble(Tolerance.TrimStart('='));
            }
            else { throw new ApplicationException("No Tolerance Specified"); }
        }
    }
}
