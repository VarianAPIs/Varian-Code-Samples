using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// Class to hold generic (non-patient specific) Objective information
    /// </summary>
    public class StructureObjective
    {
        // Properties read in from the CSV
        public string ID { get; set; }
        public string Code { get; set; }
        public string[] Aliases { get; set; }
        public string DVHObjective { get; set; }
        public string Evaluator { get; set; }
        public string Priority { get; set; }
        public string Variation { get; set; }

        // Properties set by analyzing the CSV properties
        // e.g.: D95%[cGy] >6000
        private string DVHType { get; set; }        // e.g.: Dose at Volume
        private string DVHEvalPt { get; set; }      // e.g.: 95
        private string DVHUnit { get; set; }        // e.g.: %
        private string DVHEvalUnit { get; set; }    // e.g.: cGy
        private string DVHEvalType { get; set; }    // e.g.: >
        private string DVHEvalValue { get; set; }   // e.g.: 6000
        private string DVHVariation { get; set; }   // Value for the variation parameter

        // Dictionary to aid in determining the type of objective
        // @ symbol evaluates as string literal so you don't have to use escape characters (which cause problems with the Regex)
        // (?i) matches case insensitive
        const string evalpt = @"(?<evalpt>\d+\p{P}\d+|\d+)";           //matches e.g.: 3000 in 3000cGy 
        const string unitVol = @"(?<unit>(%|cc))";                     //matches e.g.: cc in 200cc
        const string unitDose = @"(?<unit>(%|Gy|cGy))";                //matches e.g.: cGy in 3000cGy
        const string evalunitVol = @"(\[(?<evalunit>(%|cc))\])";       //matches [%] or [cc] for volume metrics
        const string evalunitDose = @"(\[(?<evalunit>(%|cGy|Gy))\])";  //matches [%] or [cGy] or [Gy] for dose metrics
        private readonly Dictionary<Regex, string> objectiveDictionary = new Dictionary<Regex, string>()
        {
            {new Regex(@"^(?i)(Min)"+evalunitDose+"$"),                 "Min"},                     //matches Min
            {new Regex(@"^(?i)(Max)"+evalunitDose+"$"),                 "Max"},                     //matches Max
            {new Regex(@"^(?i)(Mean)"+evalunitDose+"$"),                "Mean"},                    //matches Mean
            {new Regex(@"^(?i)(Volume)"),                               "Volume"},                  //matches Volume
            {new Regex(@"^(?i)D"+evalpt+unitVol+evalunitDose+"$"),      "Dose at Volume"},          //matches D95%, D2cc
            {new Regex(@"^(?i)V"+evalpt+unitDose+evalunitVol+"$"),      "Volume at Dose"},          //matches V98%, V40Gy
            {new Regex(@"^(?i)DC"+evalpt+unitVol+evalunitDose+"$"),     "Covered Dose at Volume"},  //matches DC95%, DC700cc
            {new Regex(@"^(?i)CV"+evalpt+unitDose+evalunitVol+"$"),     "Covered Volume at Dose"},  //matches CV98%, CV40Gy
            {new Regex(@"^(?i)(CI)"+evalpt+unitDose+"$"),               "Conformality Index"}       //matches CI30Gy, CI30%
        };

        // Use regular expressions to determine the type of the metric
        public void DetermineMetricType()
        {
            // Parse the objective
            if (!String.IsNullOrEmpty(DVHObjective))
            {
                var testMatch = objectiveDictionary.Where(x => x.Key.IsMatch(DVHObjective));
                if (testMatch.Count() == 1)
                {
                    // Found a match for a known objective type.
                    var match = testMatch.First().Key.Matches(DVHObjective)[0];
                    DVHType = testMatch.First().Value;
                    DVHEvalUnit = match.Groups["evalunit"].Value;
                    DVHEvalPt = match.Groups["evalpt"].Value;
                    DVHUnit = match.Groups["unit"].Value;
                    Console.WriteLine("expression {0} => type = {1}, evalPoint = {2}, unit = {3}, evalUnit = {4}", DVHObjective, DVHType, DVHEvalPt, DVHUnit, DVHEvalUnit);

                    // Fix case if needed.
                    List<string> fixcase = new List<string> { "cGy", "Gy", "cc", "%" };
                    DVHEvalUnit = fixcase.Where(x => x.ToUpper().CompareTo(DVHEvalUnit.ToUpper()) == 0).FirstOrDefault();
                    DVHUnit = fixcase.Where(x => x.ToUpper().CompareTo(DVHUnit.ToUpper()) == 0).FirstOrDefault();
                }
            }

            // Parse the Evaluator
            string eval_pattern = @"^(?<type><|<=|=|>=|>)(?<goal>\d+\p{P}\d+|\d+)$";
            if (!String.IsNullOrEmpty(Evaluator)) //Variables remain null if empty
            {
                var matches = Regex.Matches(Evaluator, eval_pattern);
                if (matches.Count == 1)
                {
                    DVHEvalValue = matches[0].Groups["goal"].ToString();
                    DVHEvalType = matches[0].Groups["type"].ToString();
                }
                else //If no match, set to not recognized
                {
                    DVHEvalValue = "Not recognized";
                    DVHEvalType = "Not recognized";
                }
            }

            // Parse the Variation
            string variation_pattern = @"^(\d+\p{P}\d+|\d+)$";
            if (!String.IsNullOrEmpty(Variation)) //Variables remain null if empty or no match
            {
                var matches = Regex.Matches(Variation, variation_pattern);
                if (matches.Count == 1)
                {
                    DVHVariation = matches[0].Value;
                }
                else
                {
                    DVHVariation = "Not recognized";
                }
            }
        }

        // Evaulate the DVH with a given structure and plan and return a string indicating what value was achieved.
        public string EvaluateObjectiveAchieved(Structure evalStructure, PlanningItem plan, ref List<string> warnings)
        {
            string achieved = "";

            // Call separate functions depending on type. Return value and units.
            switch (DVHType)
			{
				case "Dose at Volume":
                    achieved = EvaluateDoseAtVolume(evalStructure, plan, ref warnings);
                    break;
				case "Volume at Dose":
                    achieved = EvaluateVolumeAtDose(evalStructure, plan, ref warnings);
                    break;
				case "Covered Dose at Volume":
                    achieved = EvaluateCoveredDoseAtVolume(evalStructure, plan, ref warnings);
                    break;
				case "Covered Volume at Dose":
                    achieved = EvaluateCoveredVolumeAtDose(evalStructure, plan, ref warnings);
                    break;
				case "Min":
                    achieved = EvaluateMinMaxMean(evalStructure, plan, ref warnings, "Min");
                    break;
				case "Mean":
                    achieved = EvaluateMinMaxMean(evalStructure, plan, ref warnings, "Mean");
                    break;
                case "Max":
                    achieved = EvaluateMinMaxMean(evalStructure, plan, ref warnings, "Max");
                    break;
                case "Volume":
                    achieved = EvaluateVolume(evalStructure, plan, ref warnings);
                    break;
                case "Conformality Index":
                    achieved = EvaluateConformalityIndex(evalStructure, plan, ref warnings);
                    break;
                default:
                    warnings.Add(string.Format("DVHObjective not recognized.", DVHObjective));
                    break;
            }
            return achieved;
        }

        // Determine if the objective has been met
        public string EvaluateObjectiveMet(string achieved, ref List<string> warnings)
        {
            string met = "";

            // Search for numbers in the achieved string
            string pattern_achieved = @"\d+\p{P}\d+|\d+";
            achieved = Regex.Match(achieved, pattern_achieved).Value.ToString();

            // Check if evaluator is empty
            if (string.IsNullOrEmpty(DVHEvalValue) || string.IsNullOrEmpty(DVHEvalType))
            {
                return "Not evaluated";
            }

            // Check if evaluator is unknown
            if (DVHEvalValue.CompareTo("Not recognized") == 0 || DVHEvalType.CompareTo("Not recognized") == 0)
            {
                warnings.Add("Evaluator not recognized.");
                return "Not evaluated";
            }

            // Check if Achieved was calculated incorrectly
            if (string.IsNullOrEmpty(achieved))
            {
                return "Not evaluated";
            }

            // Calculate if goal is met. (e.g.: achieved=4000[cGy], DVHEvalType=<, DVHEvalValue=5000[cGy] ---> "4000[cGy]<5000[cGy]" = True)
            // If Goal is met.
            if (EvaluateLogicalExpression(achieved + DVHEvalType + DVHEvalValue))
            {
                met = "Goal";
            }

            // If Variation exists.
            else if (!String.IsNullOrEmpty(DVHVariation))
            {
                // If variation is not a recognized format.
                if (DVHVariation == "Not recognized")
                {
                    met = "Not evaluated";
                    warnings.Add("Variation not recognized.");
                }

                // If it passes the Variation Acceptable.
                else if (EvaluateLogicalExpression(achieved + DVHEvalType + DVHVariation))
                {
                    met = "Variation";
                }

                // If it doesn't pass the Variation Acceptable.
                else
                {
                    met = "Not met";
                }
            }

            // If goal isn't met and no variation to check.
            else
            {
                met = "Not met";
            }

            // If there are warnings.
            if (warnings.Any())
            {
                met = "Not evaluated";
            }

            return met;
        }

        /////////////////////////////////
        // Evaluation helper functions //
        /////////////////////////////////

        // Calculate the dose at volume
        private string EvaluateDoseAtVolume(Structure evalStructure, PlanningItem plan, ref List<string> warnings)
        {
            string achieved;

            // Calculate results
            VolumePresentation vp = (DVHUnit == "%") ? VolumePresentation.Relative : VolumePresentation.AbsoluteCm3;
            double volume = double.Parse(DVHEvalPt);
            DoseValuePresentation dvpFinal = (DVHEvalUnit == "%" && plan.GetType().Name != "PlanSum") ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute;
            DoseValue dvAchieved = plan.GetDoseAtVolume(evalStructure, volume, vp, dvpFinal);

            // Check if dose is undefined
            if (dvAchieved.IsUndefined())
            {
                // Check for sufficient sampling and dose coverage
                DVHData dvh = plan.GetDVHCumulativeData(evalStructure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.1);
                if (dvh.SamplingCoverage < 0.9)
                {
                    warnings.Add("Insufficient sampling coverage.");
                }

                if (dvh.Coverage < 1.0)
                {
                    warnings.Add("Insufficient dose coverage.");
                }

                if (warnings.Count == 0)
                {
                    warnings.Add("Dose value undefined.");
                }

                return "";
            }

            // Check dose output unit and adapting to template
            if (dvAchieved.UnitAsString.CompareTo(DVHEvalUnit.ToString()) != 0)
            {
                // Convert the acheived dose value to Gy because it wants Gy units.
                if ((DVHEvalUnit.CompareTo("Gy") == 0) && (dvAchieved.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0))
                {
                    //this is valid for V15 because we should return Gy if the user wants the value in Gy.
                    dvAchieved = new DoseValue(dvAchieved.Dose / 100, DoseValue.DoseUnit.Gy);
                }
                else
                {
                    if (plan.GetType().Name == "PlanSum" && DVHEvalUnit.CompareTo("%") == 0)
                    {
                        if ((DVHEvalUnit.CompareTo("%") == 0) && (dvAchieved.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0))
                        {
                            //this is valid for V15 because we should return Gy if the user wants the value in Gy.
                            dvAchieved = new DoseValue(dvAchieved.Dose / 100, DoseValue.DoseUnit.Gy);
                        }
                    }
                    else
                    {
                        throw new ApplicationException("Internal error: Inside else loop");
                    }
                }
            }

            // Return result
            achieved = !dvAchieved.IsUndefined() ? dvAchieved.ToString() : "";
            if (plan is PlanSum && DVHEvalUnit.CompareTo("%") == 0)
            {
                warnings.Add("Relative dose changed to Gy.");
            }

            return achieved;
        }

        // Calculate Volume at Dose
        private string EvaluateVolumeAtDose(Structure evalStructure, PlanningItem plan, ref List<string> warnings)
        {
            string achieved;

            // Plan sums don't support relative dose.
            if (plan is PlanSum && DVHUnit.CompareTo("%") == 0)
            {
                warnings.Add("Relative dose not supported for plan sums.");
                return "";
            }

            // Determine Units
            DoseValue.DoseUnit du = (DVHUnit.CompareTo("%") == 0) ? DoseValue.DoseUnit.Percent :
                (DVHUnit.CompareTo("Gy") == 0) ? DoseValue.DoseUnit.Gy :
                (DVHUnit.CompareTo("cGy") == 0) ? DoseValue.DoseUnit.cGy : DoseValue.DoseUnit.Unknown;
            string doseunit = (plan is PlanSum) ? (plan as PlanSum).PlanSetups.First().DosePerFraction.UnitAsString :
                (plan as PlanSetup).DosePerFraction.UnitAsString;

            // If plan sum and relative dose then convert to Gy
            if (plan is PlanSum && DVHUnit.CompareTo("%") == 0)
            {
                du = DoseValue.DoseUnit.Gy;
            }

            // For version 15, we must handle all doses in the unit they are supposed to be presented in.
            double dose_value = double.Parse(DVHEvalPt);
            if (du.ToString() == "Gy" && doseunit == "cGy")
            {
                dose_value = dose_value * 100;
                du = DoseValue.DoseUnit.cGy;
            }
            else if (du.ToString() == "cGy" && doseunit == "Gy")
            {
                dose_value = dose_value / 100;
                du = DoseValue.DoseUnit.Gy;
            }

            // Calculate results
            DoseValue dv = new DoseValue(dose_value, du);
            VolumePresentation vpFinal = (DVHEvalUnit.CompareTo("%") == 0) ? VolumePresentation.Relative : VolumePresentation.AbsoluteCm3;
            double volumeAchieved = plan.GetVolumeAtDose(evalStructure, dv, vpFinal);

            // Check if dose is undefined
            if (double.IsNaN(volumeAchieved))
            {
                // Check for sufficient sampling and dose coverage
                DVHData dvh = plan.GetDVHCumulativeData(evalStructure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.1);
                if (dvh.SamplingCoverage < 0.9)
                {
                    warnings.Add("Insufficient sampling coverage.");
                }

                if (dvh.Coverage < 1.0)
                {
                    warnings.Add("Insufficient dose coverage.");
                }

                if (warnings.Count == 0)
                {
                    warnings.Add("Dose value undefined.");
                }

                return "";
            }
           
            achieved = string.Format("{0:0.00} {1}", volumeAchieved, DVHEvalUnit);
            return achieved;
        }

        // Evaluate covered dose at Volume.
        private string EvaluateCoveredDoseAtVolume(Structure evalStructure, PlanningItem plan, ref List<string> warnings)
        {
            // Covered Dose at Volume DCxcc is equivalent to D(V_tot - x)cc
            double Vtot = evalStructure.Volume;
            double doseEvalPt = (DVHUnit.CompareTo("cc") == 0) ? Vtot - double.Parse(DVHEvalPt) : 100 - double.Parse(DVHEvalPt);
            StructureObjective tmp = new StructureObjective(){
                DVHUnit = DVHUnit,
                DVHEvalPt = string.Format("{0:0.00}",doseEvalPt),
                DVHEvalUnit = DVHEvalUnit
            };

            return tmp.EvaluateDoseAtVolume(evalStructure, plan, ref warnings);
        }

        // Evaluate covered volume at dose.
        private string EvaluateCoveredVolumeAtDose(Structure evalStructure, PlanningItem plan, ref List<string> warnings)
        {
            // Covered Volume at Dose DVxGy is equivalent to V_tot - VxGy
            double Vtot = evalStructure.Volume;
            StructureObjective tmp = new StructureObjective()
            {
                DVHUnit = DVHUnit,
                DVHEvalPt = DVHEvalPt,
                DVHEvalUnit = DVHEvalUnit
            };
            string achieved = tmp.EvaluateVolumeAtDose(evalStructure, plan, ref warnings);
            if (string.IsNullOrEmpty(achieved))
            {
                return achieved;
            }

            double achievedDouble = double.Parse(Regex.Match(achieved, @"^\d*\.?\d*").ToString());
            achievedDouble = (DVHEvalUnit.CompareTo("cc") == 0) ? Vtot - achievedDouble : 100 - achievedDouble;
            achieved = string.Format("{0:0.00} {1}", achievedDouble, DVHEvalUnit);

            return achieved;
        }

        // Evaluate min, mean, or max.
        private string EvaluateMinMaxMean(Structure evalStructure, PlanningItem plan, ref List<string> warnings, string type)
        {
            string achieved;

            // Compute different value based on Min / Max / Mean
            DoseValue dose;
            DoseValuePresentation dvp = (DVHEvalUnit.CompareTo("%") == 0 && plan.GetType().Name != "PlanSum") ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute;
            switch (type)
            {
                case "Min":
                    dose = plan.GetDVHCumulativeData(evalStructure, dvp, VolumePresentation.Relative, 0.1).MinDose;
                    break;
                case "Max":
                    dose = plan.GetDVHCumulativeData(evalStructure, dvp, VolumePresentation.Relative, 0.1).MaxDose;
                    break;
                case "Mean":
                    dose = plan.GetDVHCumulativeData(evalStructure, dvp, VolumePresentation.Relative, 0.1).MeanDose;
                    break;
                default:
                    return "Not supported";
            }

            // Checking dose output unit and adapting to template
            if ((DVHEvalUnit.CompareTo("Gy") == 0) && (dose.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0)) //Gy to cGy
            {
                achieved = new DoseValue(dose.Dose / 100, DoseValue.DoseUnit.Gy).ToString();
            }
            else //Gy to Gy or % to %
            {
                if (plan is PlanSum && DVHEvalUnit.CompareTo("%") == 0)
                {
                    warnings.Add("Relative dose changed to Gy.");
                    if ((dose.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0))
                    {
                        //this is valid for V15 because we should return Gy if the user wants the value in Gy.
                        achieved = new DoseValue(dose.Dose / 100, DoseValue.DoseUnit.Gy).ToString();
                    }
                    else
                    {
                        achieved = dose.ToString();
                    }
                }
                else
                {
                    achieved = dose.ToString();
                }
            }

            return achieved;
        }

        // Evaluate absolute volume.
        private string EvaluateVolume(Structure evalStructure, PlanningItem plan, ref List<string> warnings)
        {
            string achieved = string.Format("{0:0.00} cc", evalStructure.Volume);
            return achieved;
        }

        // Evaluate conformality index.
        private string EvaluateConformalityIndex(Structure evalStructure, PlanningItem plan, ref List<string> warnings)
        {
            // Conformality index = (Volume of Isodose line) / (Volume of Target)
            string achieved = "";

            // Find body contour. Search first for DicomType == "Body", then DicomType == "External"
            StructureSet ss = plan.StructureSet;
            Structure body = (from s in ss.Structures
                              where s.DicomType.ToUpper().CompareTo("Body".ToUpper()) == 0
                              select s).FirstOrDefault();
            if ((body == null) || (body.IsEmpty))
            {
                body = (from s in ss.Structures
                        where s.DicomType.ToUpper().CompareTo("External".ToUpper()) == 0
                        select s).FirstOrDefault();
            }
            if ((body == null) || (body.IsEmpty))
            {
                warnings.Add("Body not defined or not found.");
                return achieved;
            }

            // Create a temporary StructureObjective object to evaluate the volume at dose in the body
            double V_target = evalStructure.Volume;
            StructureObjective tmp = new StructureObjective()
            {
                DVHUnit = DVHUnit,
                DVHEvalPt = DVHEvalPt,
                DVHEvalUnit = "cc"
            };
            string V_isodose = tmp.EvaluateVolumeAtDose(body, plan, ref warnings);
            if (string.IsNullOrEmpty(V_isodose))
            {
                return V_isodose;
            }

            double V_isodose_double = double.Parse(Regex.Match(V_isodose, @"^\d*\.?\d*").ToString());
            double achievedDouble = V_isodose_double / V_target;
            achieved = string.Format("{0:0.00}", achievedDouble);
            return achieved;
        }

        // Helper function. Evaluates a logical expression in a string and returns a bool.
        public static bool EvaluateLogicalExpression(string logicalExpression)
        {
            System.Data.DataTable table = new System.Data.DataTable();
            table.Columns.Add("", typeof(bool));
            table.Columns[0].Expression = logicalExpression;

            System.Data.DataRow r = table.NewRow();
            table.Rows.Add(r);
            bool result = (Boolean)r[0];
            return result;
        }

    }
}



