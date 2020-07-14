using VMS.TPS.Common.Model.API;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// ViewModel for PlanForTreeView.
    /// Defines interaction logic with PlanChooserView.
    /// </summary>
    public class PlanForTreeViewModel : TreeViewItemViewModel
    {
        readonly PlanForTree _plan;

        public PlanForTreeViewModel(PlanForTree plan, CourseForTreeViewModel parentCourse, bool isInContext) : base(parentCourse, false)
        {
            _plan = plan;
            this.IsEnabled = _plan.PlanningItemObject.IsDoseValid();
            this.IsChecked = isInContext;
        }

        public string PlanName
        {
            get { return _plan.PlanName; }
        }

        public PlanningItem PlanningItemObject
        {
            get { return _plan.PlanningItemObject; }
        }

        public string PlanType
        {
            get { return _plan.PlanType; }
        }

    }


}
