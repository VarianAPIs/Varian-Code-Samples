using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM = VMS.TPS.Common.Model.API;
using D = VMS.TPS.Common.Model.Types.DoseValuePresentation;
using V = VMS.TPS.Common.Model.Types.VolumePresentation;
using ESAPIX.Common;
using ESAPIX.Common.Args;
using ESAPIX.Extensions;

namespace DiggingIntoDVH
{
    /// <summary>
    /// These three methods demonstrate the very basic functionality of pulling DVH data
    /// from the Eclipse Scripting API
    /// </summary>
    public class StandardFunctionsDemo
    {
        public static void CumulativeDVHExample()
        {
            using (var app = VM.Application.CreateApplication())
            {
                var patient = app.OpenPatientById("DA00005");
                var allPlans = patient.Courses.SelectMany(c => c.PlanSetups);
                var plan = allPlans.FirstOrDefault(p => p.Id == "ABD ARC2");
                var ptv = plan.StructureSet.Structures
                    .FirstOrDefault(s => s.Id == "PTV_3000");
                var dvhData = plan.GetDVHCumulativeData(ptv, D.Absolute, V.Relative, 0.1);

                Console.WriteLine(dvhData.MinDose);
                Console.WriteLine(dvhData.MeanDose);
                Console.WriteLine(dvhData.MaxDose);
                foreach(var pt in dvhData.CurveData)
                {
                    Console.WriteLine($"{pt.DoseValue} @ {pt.Volume:N2} {pt.VolumeUnit}");
                }
            }
        }

        public static void DoseAtVolumeExample()
        {
            using (var app = VM.Application.CreateApplication())
            {
                var patient = app.OpenPatientById("DA00005");
                var allPlans = patient.Courses.SelectMany(c => c.PlanSetups);
                var plan = allPlans.FirstOrDefault(p => p.Id == "ABD ARC1");
                var ptv = plan.StructureSet.Structures
                    .FirstOrDefault(s => s.Id == "PTV_3000");
                var coverage_95 = plan.GetDoseAtVolume(ptv, 95, V.Relative, D.Absolute);
                Console.WriteLine(coverage_95);
            }
        }

        public static void VolumeAtDoseExample()
        {
            using (var app = VM.Application.CreateApplication())
            {
                var patient = app.OpenPatientById("DA00005");
                var allPlans = patient.Courses.SelectMany(c => c.PlanSetups);
                var plan = allPlans.FirstOrDefault(p => p.Id == "ABD ARC1");
                var ptv = plan.StructureSet.Structures
                    .FirstOrDefault(s => s.Id == "PTV_3000");
                var volumeAt100 = plan.GetVolumeAtDose(ptv, plan.TotalDose, V.Relative);
                //With ESAPIX
                var volumeAt100x = plan.ExecuteQuery("V100%[%]", ptv);
                var volumeAt100_ = plan.ExecuteQuery("V100%[%]", "PTV_3000");
                Console.WriteLine(volumeAt100);
            }
        }
    }
}
