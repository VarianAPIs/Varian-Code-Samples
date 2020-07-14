using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// Class to hold patient- and plan-specific DVH criteria and evaluations
    /// </summary>
    public class StructureObjectiveResult
    {
        public StructureObjective Objective { get; set; }
        public PlanningItem Plan { get; set; }
        public List<string> Warnings { get; set; }
        public string Met { get; set; }
        public string Achieved { get; set; }
        public string FoundStructureID { get; set; }
        
        // Evaluate the attached objective for the given structure.
        public void EvaluateObjective(Dictionary<string, Structure> ssDict)
        {
            List<string> warnings = new List<string>();

            // Can't evaluate if parsing error
            if (Objective.DVHObjective == "Parsing Error")
            {
                warnings.Add("Unable to parse CSV file, likely due to commas in EBRT");
                Achieved = "";
                Met = "Not evaluated";
            }

            // If this is an empty objective, don't evaluate.
            else if (string.IsNullOrEmpty(Objective.ID) && string.IsNullOrEmpty(Objective.DVHObjective))
            {
                Achieved = "";
                Met = "";
                FoundStructureID = "";
            }
            else
            {
                // First find the structure for this objective.
                Structure evalStructure = ssDict[Objective.ID];
                if (evalStructure == null)
                {
                    Achieved = "";
                    warnings.Add("Structure not found or empty.");
                    Met = "Not evaluated";
                    FoundStructureID = "";
                }
                else
                {
                    FoundStructureID = evalStructure.Id;
                    Achieved = Objective.EvaluateObjectiveAchieved(evalStructure, Plan, ref warnings);
                    Met = Objective.EvaluateObjectiveMet(Achieved, ref warnings);
                }
            }

            // Deal with any warnings we found
            Warnings = warnings.Distinct().ToList();
        }
    }
}



