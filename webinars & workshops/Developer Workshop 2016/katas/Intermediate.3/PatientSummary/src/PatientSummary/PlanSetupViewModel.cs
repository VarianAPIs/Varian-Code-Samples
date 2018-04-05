using System;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public class PlanSetupViewModel
    {
        public PlanSetupViewModel(PlanSetup planSetup)
        {
            PlanSetupId = planSetup.Id;
            CourseId = planSetup.Course.Id;
            CreationDate = planSetup.CreationDateTime;
            TargetId = planSetup.TargetVolumeID;
            Prescription = planSetup.TotalPrescribedDose.ToString();
            ApprovalStatus = planSetup.ApprovalStatus.ToString();
        }

        public string PlanSetupId { get; private set; }
        public string CourseId { get; private set; }
        public DateTime? CreationDate { get; private set; }
        public string TargetId { get; private set; }
        public string Prescription { get; private set; }
        public string ApprovalStatus { get; private set; }
    }
}