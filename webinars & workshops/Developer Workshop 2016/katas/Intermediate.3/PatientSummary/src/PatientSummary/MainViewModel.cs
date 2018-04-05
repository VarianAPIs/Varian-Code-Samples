using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public class MainViewModel
    {
        public MainViewModel(Patient patient)
        {
            var planSetups = GetPlanSetups(patient);
            PlanSetups = planSetups.Select(p => new PlanSetupViewModel(p));
        }

        public IEnumerable<PlanSetupViewModel> PlanSetups { get; private set; }

        private IEnumerable<PlanSetup> GetPlanSetups(Patient patient)
        {
            List<PlanSetup> planSetups = new List<PlanSetup>();

            if (patient.Courses != null)
            {
                foreach (var course in patient.Courses)
                {
                    if (course.PlanSetups != null)
                    {
                        foreach (var planSetup in course.PlanSetups)
                        {
                            planSetups.Add(planSetup);
                        }
                    }
                }
            }

            return planSetups;
        }
    }
}