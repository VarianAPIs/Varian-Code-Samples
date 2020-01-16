using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DoseMetricExample.Models
{
    public class DoseMetricModel
    {
        private PlanSetup _plan;

        public string Structure { get; set; }
        public string Metric { get; set; }
        public double InputValue { get; set; }
        public List<string> InputUnits { get; set; }
        public string InputUnit { get; set; }
        public List<string> OutputUnits { get; set; }
        public string OutputUnit { get; set; }
        public double OutputValue { get; set; }
        public string Tolerance { get; set; }
        public bool ToleranceMet { get; set; }
        public DoseMetricModel(PlanSetup plan)
        {
            _plan = plan;
        }

        internal void GetOutputValue()
        {
            //put methods here.
            if (Metric == "Dose At Volume")
            {
                OutputValue = _plan.GetDoseAtVolume(_plan.StructureSet.Structures.FirstOrDefault(x => x.Id == Structure),
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
