using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// ViewModel for CourseForTreeView.
    /// Defines interaction logic with PlanChooserView.
    /// </summary>
    public class CourseForTreeViewModel : TreeViewItemViewModel
    {
        // Fields.
        readonly CourseForTree _course;
        private IEnumerable<PlanSetup> _planSetupsInScope { get; set; }
        private IEnumerable<PlanSum> _planSumsInScope { get; set; }

        // Constructors.
        public CourseForTreeViewModel(CourseForTree course, IEnumerable<PlanSetup> planSetupsInScope, IEnumerable<PlanSum> planSumsInScope) : base(null, false)
        {
            _course = course;
            _planSetupsInScope = planSetupsInScope;
            _planSumsInScope = planSumsInScope;
            this.LoadChildren();
            this.IsEnabled = this.Children.Any(x => x.IsEnabled);
            this.IsExpanded = this.Children.Any(x => x.IsChecked == true);
        }

        // Properties.
        public string CourseName
        {
            get { return _course.CourseName; }
        }

        public Course CourseObject
        {
            get { return _course.CourseObject; }
        }

        // Methods
        protected override void LoadChildren()
        {
            if (CourseObject.PlanSetups.Count() == 0 && CourseObject.PlanSums.Count() == 0)
            {
                return;
            }
            // Create list of plans in course and re-order
            List<PlanHelper> planHelpers = (from PlanSetup plan in CourseObject.PlanSetups
                                            select new PlanHelper(plan)).ToList();
            planHelpers.AddRange(from PlanSum plan in CourseObject.PlanSums
                                 select new PlanHelper(plan));
            planHelpers = planHelpers.OrderBy(x => x.PlanType == "Other").
                                      ThenBy(x => x.PlanType == "PlanSum").
                                      ThenBy(x => x.PlanType == "CD").
                                      ThenBy(x => x.PlanType == "Initial").ToList();

            // Add plans to children
            foreach (PlanHelper planHelper in planHelpers)
            {
                PlanningItem plan = planHelper.Plan;
                PlanForTree planForTree = new PlanForTree(plan);
                bool isInContext = _planSetupsInScope.ToList().Contains(plan) || _planSumsInScope.ToList().Contains(plan);
                base.Children.Add(new PlanForTreeViewModel(planForTree, this, isInContext));
            }

            // Verify check state
            this.IsChecked = (Children.All(x => x.IsChecked == true)) ? true :
                             (Children.All(x => x.IsChecked == false)) ? false : (bool?)null;
        }
    }

    // Helper class for sorting planTypes
    public class PlanHelper
    {
        // Properties
        public PlanningItem Plan { get; set; }
        public string PlanType { get; set; }

        // Constructors
        public PlanHelper(PlanSum plan) : this(plan as PlanningItem) { }
        public PlanHelper(PlanSetup plan) : this(plan as PlanningItem) { }
        public PlanHelper(PlanningItem plan)
        {
            Plan = plan;
            PlanType = DeterminePlanType(plan);
        }

        // Determine plan type as best as we can (Initial, CD, PlanSum, Other)
        private string DeterminePlanType(PlanningItem plan)
        {
            if (plan is PlanSum)
            {
                return "PlanSum";
            }
            else if (plan.Id.ToUpper().Contains("INI"))
            {
                return "Initial";
            }
            else if (plan.Id.ToUpper().Contains("CD"))
            {
                return "CD";
            }
            else
            {
                return "Other";
            }
        }
    }
}
