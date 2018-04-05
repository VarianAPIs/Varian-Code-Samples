////////////////////////////////////////////////////////////////////////////////
// PlanGeneration.cs
//
// Collection of static methods to used in the automatic plan generation.
//  
// Applies to: ESAPI v13.6.
//
// Copyright (c) 2015 Varian Medical Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{

  public enum ModelStructureType
  {
    Target, OAR
  }

  public struct ModelStructure
  {
    public string ModelId { get; private set; }
    public ModelStructureType StructureType { get; private set; }

    public ModelStructure(string id, ModelStructureType type)
      : this()
    {
      ModelId = id;
      StructureType = type;
    }
  }

  public class PlanGeneration
  {
    ////////////////////////////////////////////////////////////////////////////////
    // Algorithm specifications.
    // Change this part to match the algorithms installed on your local machine.
    ////////////////////////////////////////////////////////////////////////////////
    private const string DVHEstimationAlgorithm = "DVH Estimation Algorithm [13.6.15]";
    private const string DVHEstimationModel = "WUSTL Prostate Model";
    private const string OptimizationAlgorithm = "Photon Optimizer [13.6.15]";
    private const string DoseCalculationAlgorithm = "AAA_13615";
    private const string MlcId = "Millennium_120";
    private const string LeafMotionCalculator = "Varian Leaf Motion Calculator [13.6.15]";

    public const int DefaultNumberOfFractions = 44;
    public const double DefaultDosePerFraction = 1.8;
    public const string PTVSubOARSId = "PTV";
    public const string ExpandedCTVId = "CTV+margin";

    private const int NumberOfIterationsForIMRTOptimization = 2500;
    private const double MarginForJawFittingInMM = 5.0;
    private const double CollimatorAngle = 0.0;
    private const double PatientSupportAngle = 0.0;

    // Specifications for the treatment machine.
    public static readonly ExternalBeamMachineParameters MachineParameters = new ExternalBeamMachineParameters("23EX_Varian", "6X", 600, "STATIC", string.Empty);
    private static readonly List<double> GantryAngles = new List<double> { 39.0, 90.0, 141.0, 195.0, 246.0, 297.0, 348.0 };
    
    // In the DVH estimation, we need to match the structure Ids in the treatment plan to the structures 
    // in the RapidPlan model. Here we use a simple regex match. The keys in the dictionary are the structure Ids
    // in the RapidPlan model and the values in the dictionary contain the rule used in the regex match.
    private static readonly Dictionary<string, string> StructureMatchRules = new Dictionary<string, string>
    {
      {"PTV", @"^ctv\+margin$"},
      {"Bladder", @"^bladder$"},
      {"Rectum", @"^rectum$"},
      {"Femur_R", @"^femur\s*\w*r\s*\w*"},
      {"Femur_L", @"^femur\s*\w*l\s*\w*"},
    };

    /// <summary>
    /// Create PTV from CTV and fit collimator jaws to the target.
    /// </summary>
    public static void GenerateBeamGeometry(ExternalPlanSetup plan, double dosePerFraction, int numberOfFractions, double ptvMargin, string ctvId)
    {
      // Prescription
      const double prescribedPercentage = 1.0; // Note: 100% corresponds to 1.0
      var dose = new DoseValue(dosePerFraction, DoseValue.DoseUnit.Gy);
      plan.UniqueFractionation.SetPrescription(numberOfFractions, dose, prescribedPercentage);

      Trace.WriteLine("\nCreating PTV from CTV...");
      CreatePTVFromCTV(plan, ptvMargin, ctvId);

      // Match plan structures to model structures
      var structureMatches = GetStructureMatches(plan);
      if (!structureMatches.Any())
      {
        MessageBox.Show("Structure match could not be found", "No model structures found", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      // Generate PTV that does not overlap with OARs.
      // Identify the structures to be spared by their IDs in the RapidPlan model.
      Trace.WriteLine("Subtracting OARs from PTV...");
      var sparedOrgans = new List<string> { "Rectum" };
      structureMatches = SubtractOARsFromPTV(plan, structureMatches, sparedOrgans);

      var ptvId = structureMatches.Single(x => x.Value.StructureType == ModelStructureType.Target).Key;
      var ptv = plan.StructureSet.Structures.Single(x => x.Id == ptvId);
      Trace.WriteLine("PTV successfully generated.\n");

      // Fit jaws to target and add treatment fields.
      const double collRtn = 0.0;
      var jawPositions = GantryAngles.ToDictionary(angle => angle, angle => FitJawsToTarget(plan, ptv, angle, collRtn, MarginForJawFittingInMM));
      var isocenter = ptv.CenterPoint;
      foreach (var item in jawPositions)
      {
        plan.AddStaticBeam(MachineParameters, item.Value, CollimatorAngle, item.Key, PatientSupportAngle, isocenter);
      }

      Trace.WriteLine("\nJaws successfully fitted to target.\n");
    }

    /// <summary>
    /// Create PTV from CTV by adding a margin.
    /// </summary>
    private static void CreatePTVFromCTV(ExternalPlanSetup plan, double ptvMargin, string ctvId)
    {
      var ctvs = plan.StructureSet.Structures.Where(structure => structure.Id == ctvId).ToList();
      if (ctvs.Count() == 1)
      {
        const string dicomType = "ORGAN";
        var ctv = ctvs.Single();
        var ptv = plan.StructureSet.AddStructure(dicomType, ExpandedCTVId);
        ptv.SegmentVolume = ctv.Margin(ptvMargin);
      }
    }

    /// <summary>
    /// Calculate DVH estimates for a given plan and structure matches.
    /// </summary>
    public static void CalculateDVHEstimates(ExternalPlanSetup plan, Dictionary<string, ModelStructure> structureMatches)
    {
      var prescribedDose = plan.TotalPrescribedDose;
      var matchedStructures = structureMatches.ToDictionary(x => x.Key, x => x.Value.ModelId);
      var targetStructureDoseLevels = structureMatches.Where(x => x.Value.StructureType == ModelStructureType.Target).ToDictionary(x => x.Key, x => prescribedDose);
      Trace.WriteLine("\nCalculating DVH estimates...\n");
      plan.SetCalculationModel(CalculationType.DVHEstimation, DVHEstimationAlgorithm);
      var res = plan.CalculateDVHEstimates(DVHEstimationModel, targetStructureDoseLevels, matchedStructures);

      if (!res.Success)
      {
        throw new Exception("DVH estimation failed.");
      }

      Trace.WriteLine("\nDVH estimation succeeded!\n");
    }

    /// <summary>
    /// Run IMRT optimization for a given plan. 
    /// </summary>
    public static void Optimize(ExternalPlanSetup plan)
    {
      plan.SetCalculationModel(CalculationType.PhotonIMRTOptimization, OptimizationAlgorithm);
      var opt = new OptimizationOptionsIMRT(NumberOfIterationsForIMRTOptimization,
        OptimizationOption.RestartOptimization, OptimizationConvergenceOption.TerminateIfConverged, MlcId);

      Trace.WriteLine("\nOptimizing...\n");
      var res = plan.Optimize(opt);
      if (!res.Success)
      {
        var message = string.Format("Optimization failed for plan '{0}'", plan.Id);
        throw new Exception(message);
      }

      plan.SetCalculationModel(CalculationType.PhotonVolumeDose, DoseCalculationAlgorithm);
      plan.SetCalculationModel(CalculationType.PhotonLeafMotions, LeafMotionCalculator);

      Trace.WriteLine("\nCalculating leaf motions...\n");

      var calcRes = plan.CalculateLeafMotions();
      if (!res.Success)
      {
        var message = string.Format("Leaf motion calculation failed for plan '{0}'. Output:\n{1}", plan.Id, calcRes);
        throw new Exception(message);
      }
    }

    /// <summary>
    /// Calculate dose for a given plan.
    /// </summary>
    public static void CalculateDose(ExternalPlanSetup plan)
    {
      plan.SetCalculationModel(CalculationType.PhotonVolumeDose, DoseCalculationAlgorithm);
      Trace.WriteLine("\nCalculating dose...\n");
      var res = plan.CalculateDose();
      if (!res.Success)
      {
        var message = string.Format("Dose calculation failed for plan '{0}'. Output:\n{1}", plan.Id, res);
        Trace.WriteLine(message);
      }
    }

    /// <summary>
    /// Fit jaw positions to a given target.
    /// </summary>
    public static VRect<double> FitJawsToTarget(ExternalPlanSetup plan, Structure ptv, double gantryAngleInDeg, double collimatorRotationInDeg, double margin)
    {
      var isocenter = ptv.CenterPoint;
      var gantryAngleInRad = DegToRad(gantryAngleInDeg);
      var collimatorRotationInRad = DegToRad(collimatorRotationInDeg);

      double xMin = 0;
      double yMin = 0;
      double xMax = 0;
      double yMax = 0;

      var nPlanes = plan.StructureSet.Image.ZSize;
      for (int z = 0; z < nPlanes; z++)
      {
        var contoursOnImagePlane = ptv.GetContoursOnImagePlane(z);
        if (contoursOnImagePlane != null && contoursOnImagePlane.Length > 0)
        {
          foreach (var contour in contoursOnImagePlane)
          {
            AdjustJawSizeForContour(ref xMin, ref xMax, ref yMin, ref yMax, isocenter, contour, gantryAngleInRad, collimatorRotationInRad);
          }
        }
      }
      return new VRect<double>(xMin - margin, yMin - margin, xMax + margin, yMax + margin);
    }

    private static void AdjustJawSizeForContour(ref double xMin, ref double xMax, ref double yMin, ref double yMax, VVector isocenter, IEnumerable<VVector> contour, double gantryRtnInRad, double collRtnInRad)
    {
      foreach (var point in contour)
      {
        var projection = ProjectToBeamEyeView(point, isocenter, gantryRtnInRad, collRtnInRad);
        var xCoord = projection.Item1;
        var yCoord = projection.Item2;

        // Update the coordinates for jaw positions.
        if (xCoord < xMin)
        {
          xMin = xCoord;
        }

        if (xCoord > xMax)
        {
          xMax = xCoord;
        }

        if (yCoord < yMin)
        {
          yMin = yCoord;
        }

        if (yCoord > yMax)
        {
          yMax = yCoord;
        }
      }
    }


    /// <summary>
    /// Project a given point to beam's eye view. Assumes head first supine treatment orientation.
    /// </summary>
    private static Tuple<double, double> ProjectToBeamEyeView(VVector point, VVector isocenter, double gantryRtnInRad, double collRtnInRad)
    {
      // Calculate coordinates with respect to isocenter location.
      var p = point - isocenter;

      // Calculate the components of a vector corresponding to beam direction (from isocenter toward source).
      var nx = Math.Cos(gantryRtnInRad - Math.PI / 2.0);
      var ny = Math.Sin(gantryRtnInRad - Math.PI / 2.0);

      // Calculate the projection of a contour point p on the plane orthogonal to beam direction such that collimator rotation is taken into account.
      var cosCollRtn = Math.Cos(collRtnInRad);
      var sinCollRtn = Math.Sin(collRtnInRad);
      var xCoord = cosCollRtn * (nx * p.y - ny * p.x) + sinCollRtn * p.z;
      var yCoord = sinCollRtn * (ny * p.x - nx * p.y) + cosCollRtn * p.z;

      return new Tuple<double, double>(xCoord, yCoord);
    }

    /// <summary>
    /// Subtract a given set of OARS from the PTV.
    /// </summary>
    private static Dictionary<string, ModelStructure> SubtractOARsFromPTV(ExternalPlanSetup plan, Dictionary<string, ModelStructure> structureMatches, List<string> sparedOrgans)
    {
      // Remove the old PTV - OARs structure if the script was already run before.
      if (plan.StructureSet.Structures.Any(x => x.Id == PTVSubOARSId))
      {
        var oldPtv = plan.StructureSet.Structures.Single(x => x.Id == PTVSubOARSId);
        plan.StructureSet.RemoveStructure(oldPtv);
      }

      var ptvId = structureMatches.Single(x => x.Value.StructureType == ModelStructureType.Target).Key;
      var ptv = plan.StructureSet.Structures.Single(st => st.Id == ptvId);
      var ptvSegmentVolume = ptv.SegmentVolume;

      // Remove all parts of PTV that overlap with OARs
      var oars = plan.StructureSet.Structures.Where(x => structureMatches.ContainsKey(x.Id) && structureMatches[x.Id].StructureType == ModelStructureType.OAR);
      foreach (var oar in oars)
      {
        if (sparedOrgans.Contains(structureMatches[oar.Id].ModelId))
        {
          ptvSegmentVolume = ptvSegmentVolume.Sub(oar.SegmentVolume);  
        }
      }

      const string dicomType = "PTV";
      var newPtv = plan.StructureSet.AddStructure(dicomType, PTVSubOARSId);
      newPtv.SegmentVolume = ptvSegmentVolume;

      // Replace the old PTV with new PTV in the structure matches.
      structureMatches.Remove(ptvId);
      structureMatches.Add(PTVSubOARSId, new ModelStructure("PTV", ModelStructureType.Target));

      return structureMatches;
    }

    /// <summary>
    /// Match plan structures to the RapidPlan model structures.
    /// </summary>
    public static Dictionary<string, ModelStructure> GetStructureMatches(PlanSetup plan)
    {
      var res = new Dictionary<string, ModelStructure>();
      var structures = plan.StructureSet.Structures;
      foreach (var st in structures)
      {
        foreach (var rule in StructureMatchRules)
        {
          var regex = new Regex(rule.Value);
          if (regex.IsMatch(st.Id.ToLower()))
          {
            res.Add(st.Id, new ModelStructure(rule.Key, rule.Key == "PTV" ? ModelStructureType.Target : ModelStructureType.OAR));
          }
        }
      }
      return res;
    }

    private static double DegToRad(double angle)
    {
      const double degToRad = Math.PI / 180.0D;
      return angle * degToRad;
    }

    /// <summary>
    /// Create verifications plans for a given treatment plan.
    /// </summary>
    public static void CreateVerificationPlan(Course course, IEnumerable<Beam> beams, ExternalPlanSetup verifiedPlan, StructureSet verificationStructures,
                                               string planId, bool calculateDose)
    {
      var verificationPlan = course.AddExternalPlanSetupAsVerificationPlan(verificationStructures, verifiedPlan);
      Helpers.RemoveOldPlan(course, planId);
      verificationPlan.Id = planId;

      // Put isocenter to the center of the body.
      var isocenter = verificationStructures.Structures.Single(st => st.Id.ToLower().StartsWith("body")).CenterPoint;

      // Copy the given beams to the verification plan and the meterset values.
      var getCollimatorAndGantryAngleFromBeam = beams.Count() > 1;
      var presetValues = (from beam in beams 
                          let newBeamId = CopyBeam(beam, verificationPlan, isocenter, getCollimatorAndGantryAngleFromBeam) 
                          select new KeyValuePair<string, MetersetValue>(newBeamId, beam.Meterset)).ToList();

      // Set presciption
      const int numberOfFractions = 1;
      verificationPlan.UniqueFractionation.SetPrescription(numberOfFractions, verifiedPlan.UniqueFractionation.PrescribedDosePerFraction, prescribedPercentage: 1.0);

      if (calculateDose)
      {
        verificationPlan.SetCalculationModel(CalculationType.PhotonVolumeDose, DoseCalculationAlgorithm);
        Trace.WriteLine("\nCalculating dose for verification plan...\n");
        var res = verificationPlan.CalculateDoseWithPresetValues(presetValues);
        if (!res.Success)
        {
          var message = string.Format("Dose calculation failed for verification plan. Output:\n{0}", res);
          Trace.WriteLine(message);
          throw new Exception(message);
        }
      }

    }

    /// <summary>
    /// Create a copy of an existing beam (beams are unique to plans).
    /// </summary>
    private static string CopyBeam(Beam originalBeam, ExternalPlanSetup plan, VVector isocenter, bool getCollimatorAndGantryFromBeam)
    {
      // Create a new beam.
      var collimatorAngle = getCollimatorAndGantryFromBeam ? originalBeam.ControlPoints.First().CollimatorAngle : 0.0;
      var gantryAngle = getCollimatorAndGantryFromBeam ? originalBeam.ControlPoints.First().GantryAngle : 0.0;
      var metersetWeights = originalBeam.ControlPoints.Select(cp => cp.MetersetWeight);
      var beam = plan.AddSlidingWindowBeam(MachineParameters, metersetWeights, collimatorAngle, gantryAngle,
        PatientSupportAngle, isocenter);

      // Copy control points from the original beam.
      var editableParams = beam.GetEditableParameters();
      for (var i = 0; i < editableParams.ControlPoints.Count(); i++)
      {
        editableParams.ControlPoints.ElementAt(i).LeafPositions = originalBeam.ControlPoints.ElementAt(i).LeafPositions;
        editableParams.ControlPoints.ElementAt(i).JawPositions = originalBeam.ControlPoints.ElementAt(i).JawPositions;
      }
      beam.ApplyParameters(editableParams);
      return beam.Id;
    }

    /// <summary>
    /// Add normal tissue objectives. The NTO values are taken from the WUSTL Prostate Model datasheet.
    /// </summary>
    public static void AddNTO(ExternalPlanSetup plan)
    {
      const double priority = 100.0;
      const double distanceFromTargetBorderInMM = 3.0;
      const double startDosePersentage = 100.0;
      const double endDosePercentage = 40.0;
      const double fallOff = 0.05;
      plan.OptimizationSetup.AddNormalTissueObjective(priority, distanceFromTargetBorderInMM, startDosePersentage, endDosePercentage, fallOff);
    }

    /// <summary>
    /// Normalize the plan such that V100%Rx is at least 98%.
    /// </summary>
    public static void Normalize(ExternalPlanSetup plan, Dictionary<string, ModelStructure> structureMatches)
    {
      var ptvId = structureMatches.Single(x => x.Value.ModelId == "PTV").Key;
      var ptv = plan.StructureSet.Structures.Single(st => st.Id == ptvId);
      plan.PlanNormalizationValue = 100.0;
      const double relativeDose = 100;
      const double volTarget = 98.0;
      var targetDose = plan.TotalPrescribedDose.Dose;
      var dv = new DoseValue((relativeDose / 100.0) * targetDose, "Gy");
      var vol = plan.GetVolumeAtDose(ptv, dv, VolumePresentation.Relative);
      if (vol < volTarget)
      {
        dv = plan.GetDoseAtVolume(ptv, volTarget, VolumePresentation.Relative, DoseValuePresentation.Absolute);
        plan.PlanNormalizationValue = 100.0 * dv.Dose / ((relativeDose / 100.0) * targetDose);
      }
    }
  }
}
