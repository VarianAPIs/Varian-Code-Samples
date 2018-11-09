using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM = VMS.TPS.Common.Model.API;
using D = VMS.TPS.Common.Model.Types.DoseValuePresentation;
using V = VMS.TPS.Common.Model.Types.VolumePresentation;
using ESAPIX.Extensions;

namespace DiggingIntoDVH
{
    class ESAPIXFunctionsDemo
    {
        public static void DifferentialDVHExample()
        {
            using (var app = VM.Application.CreateApplication())
            {
                var patient = app.OpenPatientById("DA00005");
                var allPlans = patient.Courses.SelectMany(c => c.PlanSetups);
                var plan = allPlans.FirstOrDefault(p => p.Id == "ABD ARC2");
                var ptv = plan.StructureSet.Structures
                    .FirstOrDefault(s => s.Id == "PTV_3000");
                var cumulative = plan.GetDVHCumulativeData(ptv, D.Absolute, V.Relative, 0.1).CurveData;
                //ESAPIX
                var diffDVH = cumulative.Differential();
            }
        }

        public static void DoseAtVolumeExample()
        {
            using (var app = VM.Application.CreateApplication())
            {
                var patient = app.OpenPatientById("DA00005");
                var allPlans = patient.Courses.SelectMany(c => c.PlanSetups);
                var plan = allPlans.FirstOrDefault(p => p.Id == "ABD ARC1");
                //ESAPIX
                var coverage_95 = plan.ExecuteQuery("D95%[Gy]", "PTV_3000");
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
                //With ESAPIX
                var volumeAt100 = plan.ExecuteQuery("V100%[%]", "PTV_3000");
                Console.WriteLine(volumeAt100);
            }
        }
    }
}
