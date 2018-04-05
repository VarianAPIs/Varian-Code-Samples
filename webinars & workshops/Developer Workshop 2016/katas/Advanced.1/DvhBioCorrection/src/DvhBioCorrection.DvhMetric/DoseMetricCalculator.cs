using System;
using System.Linq;
using System.Text.RegularExpressions;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DvhBioCorrection.DvhMetric
{
    public class DoseMetricCalculator
    {
        public static double Calculate(Patient patient,
            string courseId, string planSetupId, string structureId, string metricName)
        {
            var dose = GetDose(patient, courseId, planSetupId, structureId, metricName);
            var doseMetric = GetDoseMetric(metricName);
            return doseMetric.Calculate(dose);
        }

        private static double[] GetDose(Patient patient,
            string courseId, string planSetupId, string structureId, string metricName)
        {
            var dose = GetPhysicalDoseForStructure(patient, courseId, planSetupId, structureId);

            var lqMatch = Regex.Match(metricName, @"LQ, a/b=([0-9.]+)");

            if (lqMatch.Success)
            {
                var alphaBeta = Convert.ToDouble(lqMatch.Groups[1].Value);
                var fractions = GetPlanSetupFractions(patient, courseId, planSetupId);

                var doseConverter = new LqBioDoseConverter(fractions, alphaBeta);
                return doseConverter.Convert(dose);
            }
            else
            {
                return dose;
            }
        }

        private static double[] GetPhysicalDoseForStructure(Patient patient,
            string courseId, string planSetupId, string structureId)
        {
            var planSetup = GetPlanSetup(patient, courseId, planSetupId);
            planSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
            var structure = GetStructure(planSetup, structureId);
            var doseExtractor = new DoseExtractor(planSetup.Dose);
            return doseExtractor.GetDoseForStructure(structure);
        }

        private static PlanSetup GetPlanSetup(Patient patient, string courseId, string planSetupId)
        {
            return patient.Courses.First(c => c.Id == courseId)
                .PlanSetups.First(p => p.Id == planSetupId);
        }

        private static Structure GetStructure(PlanSetup planSetup, string structureId)
        {
            return planSetup.StructureSet.Structures.First(s => s.Id == structureId);
        }

        private static int GetPlanSetupFractions(Patient patient, string courseId, string planSetupId)
        {
            return GetFractions(GetPlanSetup(patient, courseId, planSetupId));
        }

        private static int GetFractions(PlanSetup planSetup)
        {
            return planSetup.NumberOfFractions.Value;
        }

        private static IDoseMetric GetDoseMetric(string metricName)
        {
            if (metricName.StartsWith("Mean"))
            {
                return new MeanDoseMetric();
            }

            throw new ArgumentException("Invalid metric name");
        }
    }
}
