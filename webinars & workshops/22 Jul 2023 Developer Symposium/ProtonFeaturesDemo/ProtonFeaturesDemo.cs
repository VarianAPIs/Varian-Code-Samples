// System libraries for basic functionality
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Serilog for monitoring the script progress in console and log file
using Serilog;

// Eclipse Scripting libraries for ESAPI functionalitites
using TP = VMS.TPS.Common.Model.API;
using TPTypes = VMS.TPS.Common.Model.Types;

// We want to modify patient data, so script has to be approved in clinical mode
[assembly: TP.ESAPIScript(IsWriteable = true)]
namespace ProtonfeaturesDemo
{
  class ProtonFeaturesDemo
  {
    /* When set true, the script changes are saved and the script is paused at every step.
     * Reload patient data in Eclipse to see the changes.
     * Press <Enter> in console to continue script execution. (Works best in command prompt.)*/ 
    private const bool pauseAfterEachStep_ = false;

    // Save logs and data files in these location
    private const string rootDir_ = @"C:\temp\ProtonFeaturesDemo\";
    private const string logFileDir_ = rootDir_ + @"log\";

    [STAThread]
    static void Main(string[] args)
    {
      // Making sure the script works before accessing ESAPI features
      Console.WriteLine("Hello, World.");

      StartLogging();

      (string patientId, string courseId,
       string planId,    string ssetId
      ) = ParseInputs(args);

      Log.Information($"Inputs: \n Patient: {patientId} \n Course: {courseId} \n " +
                                    $"Plan: {planId} \n SSet: {ssetId}");

      try
      {
        Log.Information("Creating VMS.TPS Application");
        using (var App = TP.Application.CreateApplication())
        {
          Log.Information("App created");

          // Open patient for modifications
          Log.Information("Opening patient data");

          TP.Patient patient = App.OpenPatientById(patientId);
          if (patient is null)
            throw new ArgumentException($"Could not find patient with ID \"{patientId}\".");
          patient.BeginModifications();

          // Find the structure set
          TP.StructureSet sset = patient.StructureSets.FirstOrDefault(x => x.Id == ssetId);
          if(sset is null)
            throw new ArgumentException($"Could not find structure set with ID \"{ssetId}\".");

          // Make sure the structure set contains PTVB
          TP.Structure ptvStructure = sset.Structures.FirstOrDefault(x => x.Id == "PTVB_7000");
          if(ptvStructure is null)
            throw new ArgumentException($"Could not find structure with ID \"PTVB_7000\".");

          #region Create Course and Proton Plan

          Log.Information("Creating Course and Proton Plan");

          // Only create course if it does not exist already
          TP.Course course = patient.Courses.FirstOrDefault(x => x.Id == courseId);
          if (course is null)
          {
            course = patient.AddCourse();
            course.Id = courseId;
          }

          // Only create plan if it does not exist
          TP.IonPlanSetup plan = course.IonPlanSetups.FirstOrDefault(x => x.Id == planId);
          if (plan is null)
          {
            Log.Information(" Creating New Proton Plan");

            var planParameters =
              new
              {
                targetId = "PTVB_7000",
                patientSupportDeviceId = "Table",
                dosePerFraction = 2.0, // Gy
                doseUnit = TPTypes.DoseValue.DoseUnit.Gy,
                numberOfFractions = 35,
                treatmentPercentage = 1.0, // =100%
              };

            plan = course.AddIonPlanSetup(sset, planParameters.patientSupportDeviceId);
            plan.Id = planId;
            
            var errorHint = new StringBuilder();
            plan.SetTargetStructureIfNoDose(ptvStructure, errorHint);
            if(errorHint.Length > 0)
              throw new ApplicationException("Setting plan target structure failed:\n" + errorHint.ToString());

            SaveAndPause(App);

            Log.Information(" Setting Prescription");
            plan.SetPrescription(planParameters.numberOfFractions,
              new TPTypes.DoseValue(planParameters.dosePerFraction, planParameters.doseUnit),
              planParameters.treatmentPercentage);

            SaveAndPause(App);

            Log.Information(" Setting Machine & Beam Parameters");
            var machineParams =
              new
              {
                machineId = "ProBeam_RH",
                techniqueId = "MODULAT_SCANNING",
                toleranceId = "T1"
              };

            var beamParams =
              new
              {
                nBeams = 3,
                beamIds = new string[] { "Field 1", "Field 2", "Field 3" },
                targetId = "FieldTarget",
                snoutId = "S1",
                snoutPositions = new double[] { 17.0, 23.0, 23.0 },
                gantryAngles = new double[] { 180.0, 45.0, 315.0 },
                patientSupportAngle = 0.0,
                rangeShifterId = "RS_5CM",
                rangeShifterSetting = "IN",
              };

            TP.Structure tgtStructure = sset.Structures.FirstOrDefault(x => x.Id == beamParams.targetId);

            for (int i = 0; i < beamParams.nBeams; i++)
            {
              Log.Information($"  {beamParams.beamIds[i]}");
              TP.IonBeam beam = plan.AddModulatedScanningBeam(
                new TPTypes.ProtonBeamMachineParameters(machineParams.machineId,
                machineParams.techniqueId, machineParams.toleranceId),
                beamParams.snoutId, beamParams.snoutPositions[i], beamParams.gantryAngles[i],
                beamParams.patientSupportAngle, tgtStructure.CenterPoint) as TP.IonBeam;

              beam.Id = beamParams.beamIds[i];

              TP.IonBeamParameters beamEditableParams = beam.GetEditableParameters();
              beamEditableParams.TargetStructure = tgtStructure;
              beamEditableParams.PreSelectedRangeShifter1Id = beamParams.rangeShifterId;
              beamEditableParams.PreSelectedRangeShifter1Setting = beamParams.rangeShifterSetting;
              beam.ApplyParameters(beamEditableParams);

              #region Set Target Margins

              beam.ProximalTargetMargin = 2.0; // mm
              beam.DistalTargetMargin = 3.0; // mm
              beam.LateralMargins = new TPTypes.VRect<double>(5.0, 5.0, 5.0, 5.0); // mm

              #endregion
            }

            SaveAndPause(App);
          }
          else
          {
            Log.Information(" Proton Plan already exists");
          }


          #region Set Calculation Models

          Log.Information(" Setting Calculation Models");
          var calcModelDictionary = new Dictionary<TPTypes.CalculationType, string>()
            {
              { TPTypes.CalculationType.ProtonBeamLineModifiers, "PCS_18" },
              { TPTypes.CalculationType.ProtonDVHEstimation, "DVH Estimation Algorithm [18.0.0]" },
              { TPTypes.CalculationType.ProtonMSPostProcessing, "PCS_18" },
              { TPTypes.CalculationType.ProtonOptimization, "NUPO_18" },
              { TPTypes.CalculationType.ProtonVolumeDose, "PCS_18"},
              { TPTypes.CalculationType.ProtonBeamDeliveryDynamics, "NUPO_18" } // In 18.0
            };

          foreach (var entry in calcModelDictionary)
          {
            Log.Information($"  {entry.Key} : {entry.Value}");
            plan.SetCalculationModel(entry.Key, entry.Value);
          }

          SaveAndPause(App);

          #endregion

          #endregion // Create Course and Proton Plan

          #region Add Optimization Objectives

          Log.Information("Adding Optimization Objectives");

          #region Remove Existing Objectives

          Log.Information(" Removing Existing Objectives");
          foreach (var objective in plan.OptimizationSetup.Objectives.ToList())
            plan.OptimizationSetup.RemoveObjective(objective);

          #endregion

          #region Add New Objectives

          Log.Information(" Adding New Objectives");
          TP.Structure oarStructure = sset.Structures.FirstOrDefault(x => x.Id == "BrainStem");
          if (oarStructure is null)
            throw new ApplicationException($"Could not find structure with ID \"{oarStructure}\"");

          plan.OptimizationSetup.AddPointObjective(ptvStructure, TPTypes.OptimizationObjectiveOperator.Upper,
          new TPTypes.DoseValue(71.0, TPTypes.DoseValue.DoseUnit.Gy), 0.0, 200);

          plan.OptimizationSetup.AddPointObjective(ptvStructure, TPTypes.OptimizationObjectiveOperator.Lower,
          new TPTypes.DoseValue(69.0, TPTypes.DoseValue.DoseUnit.Gy), 100.0, 150);

          plan.OptimizationSetup.AddMeanDoseObjective(oarStructure,
          new TPTypes.DoseValue(10.0, TPTypes.DoseValue.DoseUnit.Gy), 100);

          plan.OptimizationSetup.AddProtonNormalTissueObjective(50, 4.3, 98, 80);

          SaveAndPause(App);

          #endregion

          #region Add Robust Optimization Uncertainties

          Log.Information(" Adding Uncertainty Scenarios");
          var uncertaintyParams = new
          {
            planUncertaintyType = new TPTypes.PlanUncertaintyType[] {
              TPTypes.PlanUncertaintyType.RobustOptimizationUncertainty,
              TPTypes.PlanUncertaintyType.RangeUncertainty
            },
            planSpecificUncertainty = true, // The plan uncertainty dose is either plan specific (true) or field specific (false).
            curveErrors = new double[] { 3.0, -3.0 }, // %
            uncertaintyShifts = new TPTypes.VVector[] {
              new TPTypes.VVector(0, 0, 0), // cm
              new TPTypes.VVector(-0.5, 0, 0),
              new TPTypes.VVector(0.5, 0, 0),
            }
          };

          foreach (var uncertaintyType in uncertaintyParams.planUncertaintyType)
          {
            foreach (var curveError in uncertaintyParams.curveErrors)
            {
              foreach (var uncertaintyShift in uncertaintyParams.uncertaintyShifts)
              {
                plan.AddPlanUncertaintyWithParameters(uncertaintyType, uncertaintyParams.planSpecificUncertainty,
                  curveError, uncertaintyShift);
              }
            }
          }

          SaveAndPause(App);

          #endregion

          #endregion Add Optimization Objectives

          #region Apply RapidPlan

          Log.Information("Applying RapidPlan");

          #region Remove Existing Objectives

          Log.Information(" Removing Existing Objectives & Parameters");
          foreach (var objective in plan.OptimizationSetup.Objectives.ToList())
            plan.OptimizationSetup.RemoveObjective(objective);

          foreach (var parameter in plan.OptimizationSetup.Parameters)
          {
            try
            {
              plan.OptimizationSetup.RemoveParameter(parameter);
            }
            catch (Exception)
            {
              // Do nothing, some parameters can't / shouldn't be removed
            }
          }

          #endregion

          SaveAndPause(App);

          #region Run RapidPlan

          Log.Information(" Running RapidPlan");
          var rapidPlanParams = new
          {
            modelId = "20180313_rv-VUMC Model_PTV_1",
            targetDoseLevels = new Dictionary<string, TPTypes.DoseValue>()
            {
              { "PTVB", new TPTypes.DoseValue(70.0, TPTypes.DoseValue.DoseUnit.Gy) }, // Structure ID ; Dose Level
              { "PTVE", new TPTypes.DoseValue(54.25, TPTypes.DoseValue.DoseUnit.Gy) },
              { "PTVO", new TPTypes.DoseValue(54.25, TPTypes.DoseValue.DoseUnit.Gy) }
            },

            structureMatches = new Dictionary<string, string>()
            {
              {"PTVB", "PTVB"}, // ID in RapidPlan model ; Structure ID 
              {"PTVE", "PTVE"},
              {"PTVO", "PTVO"},
              {"BrainStem", "HERSENSTAM"},
              {"C.PAROTID", "C.PAROTID"},
              {"I.PAROTID", "I.PAROTID" },
              {"Esophagus1", "ESOPHAGUS"},
              {"LARYNX COMP", "LARYNX COMP"},
              {"Oral Cavity1", "MONDHOLTE"},
              {"PCM COMP", "PCM COMP"},
              {"Ring Boost", "RING BOOST"},
              {"Ring ELEKTIEF", "RING ELEKTIEF"},
              {"Spinal Cord1", "MYELUM"}
            }
          };

          TP.CalculationResult rapidPlanCalcRes = plan.CalculateDVHEstimates(
            rapidPlanParams.modelId,
            rapidPlanParams.targetDoseLevels,
            rapidPlanParams.structureMatches);

          // We can't proceed without the objectives from RapidPlan
          if (!rapidPlanCalcRes.Success)
            throw new ApplicationException($"RapidPlan Calculation Failed.");

          Log.Information($" Calculation completed. " +
            $"DVH Estimates for {plan.DVHEstimates.Count() / 2} structures available.");

          SaveAndPause(App);

          #endregion

          #endregion Apply RapidPlan

          #region Optimize and Calculate Plan

          Log.Information("Optimizing and Calculating Plan");

          Log.Information(" Setting Calculation Options");
          string calculationModelId = plan.GetCalculationModel(TPTypes.CalculationType.ProtonVolumeDose);
          if (calculationModelId.Contains("APT"))
          {
            plan.SetCalculationOption(calculationModelId, "CalculationGridSizeInCM", "0.4");
            plan.SetCalculationOption(calculationModelId, "UseGPU", "Yes");
            plan.SetCalculationOption(calculationModelId, "UseFastParticleGeneration", "Yes");
          }

          plan.SetOptimizationMode(TPTypes.IonPlanOptimizationMode.MultiFieldOptimization);

          SaveAndPause(App);

          Log.Information(" Calculating Beam Line");
          var beamLineCalcRes = plan.CalculateBeamLine();
          if (!beamLineCalcRes.Success)
            throw new ApplicationException($"Beam Line Calculation Failed.");

          SaveAndPause(App);

          Log.Information(" Optimizing");
          var optimizationRes = plan.OptimizeIMPT(
            new TPTypes.OptimizationOptionsIMPT(200, TPTypes.OptimizationOption.RestartOptimization));
          if (!optimizationRes.Success)
            throw new ApplicationException($"Optimization Failed.");
          else
            Log.Information($" Optimization finished in {optimizationRes.NumberOfIMRTOptimizerIterations} iterations.");

          SaveAndPause(App);

            Log.Information(" Calculating Dose");
          var doseCalcRes = plan.PostProcessAndCalculateDose();
          if (!doseCalcRes.Success)
            throw new ApplicationException($"Dose Calculation Failed.");

          SaveAndPause(App);

          Log.Information(" Normalizing");
          var normalizationParams = new
          {
            dose = 95.0, // %
            volume = 98.0 // %
          };
          var currentDose = plan.GetDoseAtVolume(ptvStructure, normalizationParams.volume,
                TPTypes.VolumePresentation.Relative, TPTypes.DoseValuePresentation.Relative);

          plan.PlanNormalizationValue = 100 * (currentDose.Dose / normalizationParams.dose);

          SaveAndPause(App);

          Log.Information(" Recalculating Dose");
          doseCalcRes = plan.PostProcessAndCalculateDose();
          if (!doseCalcRes.Success)
            throw new ApplicationException($"Dose Recalculation Failed.");

          SaveAndPause(App);

          Log.Information(" Calculating Uncertainty Doses");
          var robustnessCalcRes = plan.CalculatePlanUncertaintyDoses();
          if (!robustnessCalcRes.Success)
            Log.Warning(" Uncertainty Dose Calculation Failed.");

          SaveAndPause(App);

          Log.Information(" Calculating Delivery Times");
          var deliveryTimeCalcRes = plan.CalculateBeamDeliveryDynamics();
          if (!deliveryTimeCalcRes.Success)
            Log.Warning(" Delivery Time Calculation Failed.");

          SaveAndPause(App);

          Log.Information(" Calculating DECT Dose");
          TP.Image rhoImage = plan.Series.Images.FirstOrDefault(
            x => x.ImageType.Contains(@"DERHOZ\RHO"));
          TP.Image zeffImage = plan.Series.Images.FirstOrDefault(
            x => x.ImageType.ToLower().Contains(@"DERHOZ\Z"));
          if (rhoImage != null && zeffImage != null)
          {
            TP.IonPlanSetup dectPlan = plan.CreateDectVerificationPlan(rhoImage, zeffImage);
            Log.Information($"  Created DECT Verification Plan \"{dectPlan.Id}\".");
          }
          else
          {
            Log.Warning(" DECT Dose Calculation skipped. No DECT images found.");
          }

          #endregion

          #region Export Spot Data

          using (var writer = new StreamWriter(Path.Combine(rootDir_, "spot_data.csv"), append: false))
          {
            writer.WriteLine("Beam, Energy (MeV), Weight (MU), X (mm), Y (mm), Z (mm)");
            foreach (var beam in plan.IonBeams)
            {
              // Converting spot weights to MU's similarly as done with DICOM plan
              double totMeterset = beam.Meterset.Value; // total MU of the beam
              double totWeight = beam.IonControlPoints.Last().MetersetWeight; // total weight of the beam
              double conversionFactor = totMeterset / totWeight;

              foreach (var controlPoint in beam.IonControlPoints.Where(x => x.Index % 2 == 0))
              {
                foreach (var spot in controlPoint.FinalSpotList)
                {
                  double spotMU = spot.Weight * conversionFactor;
                  writer.WriteLine($"{beam.BeamNumber}, "+
                    $"{controlPoint.NominalBeamEnergy:F3}, {spotMU:F2}, " + 
                    $"{spot.Position.x:F1}, {spot.Position.y:F1}, {spot.Position.z:F1}");
                }
              }
            }
          }

          #endregion Export Spot Data

          #region Export Target Structure Mesh

          using (var writer = new StreamWriter(Path.Combine(rootDir_, "target_nodes.csv"), append: false))
          {
            writer.WriteLine("X (mm), Y (mm), Z (mm)");
            foreach (var point in plan.IonBeams.First().TargetStructure.MeshGeometry.Positions)
            {
              writer.WriteLine($"{point.X:F1}, {point.Y:F1}, {point.Z:F1}");
            }
          }

          using (var writer = new StreamWriter(Path.Combine(rootDir_, "target_triangles.csv"), append: false))
          {
            var triangles = plan.IonBeams.First().TargetStructure.MeshGeometry.TriangleIndices;
            writer.WriteLine(String.Join(",", triangles));
          }

          #endregion Export Target Structure Mesh

          #region Export Beam Coordinates

          using (var writer = new StreamWriter(Path.Combine(rootDir_, "beamDirs.csv"), append: false))
          {
            writer.WriteLine("Beam, Source X (mm), Source Y (mm), Source Z (mm), " +
                           "Target X (mm), Target Y (mm), Target Z (mm)");
            foreach (var beam in plan.IonBeams)
            {
              var beamSourceLocation = beam.GetSourceLocation(beam.IonControlPoints.First().GantryAngle);
              var beamTargetLocation = beam.IsocenterPosition;
              writer.WriteLine($"{beam.BeamNumber}, " +
                $"{beamSourceLocation.x:F1}, {beamSourceLocation.y:F1}, {beamSourceLocation.z:F1}, " +
                $"{beamTargetLocation.x:F1}, {beamTargetLocation.y:F1}, {beamTargetLocation.z:F1}");
            }
          }

          #endregion Export Beam Coordinates
        }
      }
      catch (Exception e)
      {
        Log.Error(e, "Exception caught");
      }
      finally
      {
        Log.Information("Exiting");
        Log.CloseAndFlush();
      }
    }

    /// <summary>
    /// Extract Patient, Course, and Plan Ids from command line arguments
    /// </summary>
    /// <param name="args">Expected size = 4</param>
    /// <returns>Fours strings corresponding to the patient, course, plan, and structure set IDs.</returns>
    private static (string, string, string, string) ParseInputs(string[] args)
    {
      Log.Information("Parsing inputs");

      if (args.Length != 4)
      {
        string errorMsg = "Invalid Input!\n" +
                  "Usage: ProtonFeaturesDemo.exe <PatientId> <CourseId> <ProtonPlanId> <StructureSetId>\n" +
                  "Exiting.";
        Log.Error(errorMsg);
        Environment.Exit(-1);
      }

      return (args[0], args[1], args[2], args[3]);
    }

    public static void StartLogging()
    {
      var logFilePath = Path.Combine(logFileDir_, $"log_{DateTime.Now.ToString(@"yyy-mm-dd@hh.mm.ss")}.txt");
      var logFlushInterval = new TimeSpan(0, 0, 5);

#if DEBUG
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.File(logFilePath, flushToDiskInterval: logFlushInterval)
        .WriteTo.Console()
        .CreateLogger();
#else
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.File(logFilePath, flushToDiskInterval: logFlushInterval)
        .WriteTo.Console()
        .CreateLogger();
#endif

      Log.Information($"Logging started at {logFilePath}");
    }

    private static void SaveAndPause(TP.Application App)
    {
      if (!pauseAfterEachStep_)
        return;

      Log.Information("Saving...");
      App.SaveModifications();
      Log.Information("Paused - press enter to continue");
      Console.ReadLine();
    }
  }
}
