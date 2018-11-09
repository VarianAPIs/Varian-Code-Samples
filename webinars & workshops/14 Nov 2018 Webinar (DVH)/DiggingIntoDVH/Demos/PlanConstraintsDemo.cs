using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM = VMS.TPS.Common.Model.API;
using D = VMS.TPS.Common.Model.Types.DoseValuePresentation;
using V = VMS.TPS.Common.Model.Types.VolumePresentation;
using ESAPIX.Extensions;
using ESAPIX.Constraints;
using ESAPIX.Constraints.DVH;

namespace DiggingIntoDVH
{
    public class PlanConstraintsDemo
    {
        public static void GetAndRunPlanConstraints()
        {
            using (var app = VM.Application.CreateApplication())
            {
                var patient = app.OpenPatientById("DA00005");
                var allPlans = patient.Courses.SelectMany(c => c.PlanSetups);
                var plan = allPlans.FirstOrDefault(p => p.Id == "ABD ARC1");
                //Example of what is available in ESAPI default
                var (rxs, measures) = plan.GetProtocolPrescriptionsAndMeasures();
                //Using ESAPIX to get more 
                var constraints = plan.GetConstraints();
                foreach(var con in constraints)
                {
                    Console.WriteLine(con?.FullName);
                }
            }
        }

        public static void CreateESAPIXConstraints()
        {
            using (var app = VM.Application.CreateApplication())
            {
                var patient = app.OpenPatientById("DA00005");
                var allPlans = patient.Courses.SelectMany(c => c.PlanSetups);
                var plan = allPlans.FirstOrDefault(p => p.Id == "ABD ARC1");

                //Create some constraints
                var constraints = new List<IConstraint>()
                {
                    ConstraintBuilder.Build("PTV_3000", "D95%[cGy] >= 2800", PriorityType.PRIORITY_2),
                    ConstraintBuilder.Build("PTV_3000", "Max[cGy] <= 3500", PriorityType.PRIORITY_1),
                    ConstraintBuilder.Build("PTV_3000", "Max[cGy] <= 3500", PriorityType.PRIORITY_1),
                    ConstraintBuilder.Build("PTV_NA", "Max[cGy] <= 3500", PriorityType.PRIORITY_1),
                };

                //Evalulate constraints
                foreach (var con in constraints)
                {
                    //Check to make sure this constraint is possible
                    var canConstrain = con.CanConstrain(plan);
                    if (canConstrain.IsSuccess)
                    {
                        Console.WriteLine($"{con?.FullName} => {con.Constrain(plan).ResultType}");
                    }
                    else
                    {
                        Console.WriteLine($"{con?.FullName} => {canConstrain.Message}");
                    }
                }
            }
        }
    }
}
