using System.Collections.Generic;
using VMS.TPS.Common.Model.API;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// Container for Courses listed in a TreeView.
    /// </summary>
    public class CourseForTree
    {
        // Fields
        readonly List<PlanForTree> _planList = new List<PlanForTree>();

        // Properties
        public Course CourseObject { get; private set; }
        public string CourseName { get; private set; }
        public List<PlanForTree> PlanList
        {
            get { return _planList; }
        }

        // Constructors
        public CourseForTree(Course course, IEnumerable<PlanSetup> planSetupsInScope, IEnumerable<PlanSum> planSumsInScope)
        {
            this.CourseObject = course;
            this.CourseName = course.Id.ToString();
        }
    }
}
