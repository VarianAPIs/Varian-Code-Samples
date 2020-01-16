using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DoseParameters
{
    public class DoseParametersViewModel:BindableBase
    {
        private PlanSetup _plan;

        public ObservableCollection<TableDisplayModel> RxData { get; set; }
        public ObservableCollection<TableDisplayModel> CalculationParameters { get; set; }
        public DoseParametersViewModel(PlanSetup planSetup)
        {
            _plan = planSetup;
            RxData = new ObservableCollection<TableDisplayModel>();
            CalculationParameters = new ObservableCollection<TableDisplayModel>();
            GetRxData();
            GetCalcData();
        }

        private void GetRxData()
        {
            RxData.Add(new TableDisplayModel { Property = "Dose Per Fraction", Value = $"{_plan.DosePerFraction}" });
            RxData.Add(new TableDisplayModel { Property = "Number of Fractions", Value = $"{_plan.NumberOfFractions}" });
            RxData.Add(new TableDisplayModel { Property = "Total Dose", Value = $"{_plan.TotalDose}" });
            RxData.Add(new TableDisplayModel { Property = "Treatment %", Value = $"{_plan.TreatmentPercentage * 100.0}" });
        }

        private void GetCalcData()
        {
            CalculationParameters.Add(new TableDisplayModel { Property = "Caluclation Model", Value = $"{_plan.GetCalculationModel(CalculationType.PhotonVolumeDose)}" });
            CalculationParameters.Add(new TableDisplayModel { Property = "Calculation Grid Size", Value = GetFromLogs("CalculationGridSizeInCM") });
            CalculationParameters.Add(new TableDisplayModel { Property = "Field Normalization Type", Value = GetFromLogs("FieldNormalizationType") });
            CalculationParameters.Add(new TableDisplayModel { Property = "Heterogeneity Correction", Value = GetFromLogs("HeterogeneityCorrection") });
            CalculationParameters.Add(new TableDisplayModel { Property = "Dosimetric leaf gap", Value = GetFromLogs("Dosimetric leaf gap") });
            CalculationParameters.Add(new TableDisplayModel { Property = "Leaf transmission factor", Value = GetFromLogs("Leaf transmission factor") });
        }

        private string GetFromLogs(string v)
        {
            var beamLogs = _plan.Beams.FirstOrDefault(x => !x.IsSetupField).CalculationLogs;
            if(beamLogs.Any(x=>x.Category == "Dose"))
            {
                return beamLogs.FirstOrDefault(x => x.Category == "Dose").MessageLines.FirstOrDefault(x => x.Contains(v)).Split('=').Last();
            }
            else
            {
                return "Not Found in Logs";
            }
        }
    }
}
