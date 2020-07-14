using VMS.TPS.Common.Model.API;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// Container for Plans in a TreeView
    /// </summary>
    public class PlanForTree
    {
        // Properties
        public PlanningItem PlanningItemObject { get; private set; }
        public string PlanName { get; private set; }
        public string PlanType { get; private set; }

        // Constructors. Enables use with PlanSetups, PlanSums, and PlanningItems
        public PlanForTree(PlanSetup plan) : this(plan as PlanningItem) { }
        public PlanForTree(PlanSum plan) : this(plan as PlanningItem) { }
        public PlanForTree(PlanningItem plan)
        {
            this.PlanningItemObject = plan;
            this.PlanName = plan.Id.ToString();
        }
    }
}
