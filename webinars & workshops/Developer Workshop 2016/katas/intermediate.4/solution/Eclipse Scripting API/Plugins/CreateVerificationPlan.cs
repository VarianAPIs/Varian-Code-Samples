////////////////////////////////////////////////////////////////////////////////
// CreateVerificationPlan.cs
//
//  A ESAPI v13.6+ script that demonstrates creation of verification plans 
//  from a clinical plan.
//
// Kata Intermediate.5)    
//  Program an ESAPI automation script that creates a new QA course, a new set 
//  of verification plans for the selected clinical plan 
//  (1 composite and 1 verification plan per beam), and calculates dose for all 
//  of the new verification plans.
//
// Applies to:
//      Eclipse Scripting API for Research Users
//          13.6, 13.7, 15.0,15.1
//      Eclipse Scripting API
//          15.1
//
// Copyright (c) 2016 Varian Medical Systems, Inc.
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
// #define v136 // uncomment this for v13.6 or v13.7
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

#if !v136
// for 15.1 script approval so approval wizard knows this is a writeable script.
[assembly: ESAPIScript(IsWriteable = true)]
#endif

namespace VMS.TPS
{
  public class Script
  {
      // these three strings define the patient/study/image id for the image phantom that will be copied into the active patient.
      public static string QAPatientID = "$QAGeometry";
      public static string QAStudyID = "none";
      public static string QAImageID = "CT MATRIXXEVO";

    public Script()
    {
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
    {
        Patient p = context.Patient;
        if (p == null)
            throw new ApplicationException("Please load a patient");

        ExternalPlanSetup plan = context.ExternalPlanSetup;
        if (plan == null)
            throw new ApplicationException("Please load an external beam plan that will be verified.");

        p.BeginModifications();
        // TODO: look whether the phantom scan exists in this patient before copying it
        StructureSet ssQA = p.CopyImageFromOtherPatient(QAPatientID, QAStudyID, QAImageID);

        // Get or create course with Id 'IMRTQA'
        const string courseId = "IMRTQA";
        Course course = p.Courses.Where(o => o.Id == courseId).SingleOrDefault();
        if (course == null)
        {
            course = p.AddCourse();
            course.Id = courseId;
        }
#if false
        // Create an individual verification plan for each field.
        foreach (var beam in plan.Beams)
        {
            CreateVerificationPlan(course, new List<Beam> { beam }, plan, ssQA, beam.Id, calculateDose: false);
        }
#endif
        // Create a verification plan that contains all fields (Composite).
        ExternalPlanSetup verificationPlan = CreateVerificationPlan(course, plan.Beams, plan, ssQA, "Composite", calculateDose: true);

        //ExternalPlanSetup verificationPlan = course.AddExternalPlanSetupAsVerificationPlan(ssQA, plan);

        // nagivate back from verificationPlan to verified plan
        PlanSetup verifiedPlan = verificationPlan.VerifiedPlan;
        if (plan != verifiedPlan)
        {
            MessageBox.Show(string.Format("ERROR! verified plan {0} != loaded plan {1}", verifiedPlan.Id
                , plan.Id));
        }
        MessageBox.Show(string.Format("Success - verification plan {0} created in course {1}.", verificationPlan.Id, course.Id));

    }
    /// <summary>
    /// Create verifications plans for a given treatment plan.
    /// </summary>
    public static ExternalPlanSetup CreateVerificationPlan(Course course, IEnumerable<Beam> beams, ExternalPlanSetup verifiedPlan, StructureSet verificationStructures,
                                               string planId, bool calculateDose)
    {
        var verificationPlan = course.AddExternalPlanSetupAsVerificationPlan(verificationStructures, verifiedPlan);
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
        verificationPlan.SetPrescription(numberOfFractions, verifiedPlan.DosePerFraction, treatmentPercentage: 1.0);

        if (calculateDose)
        {

            verificationPlan.SetCalculationModel(CalculationType.PhotonVolumeDose, verifiedPlan.GetCalculationModel(CalculationType.PhotonVolumeDose));
//            Trace.WriteLine("\nCalculating dose for verification plan...\n");
            var res = verificationPlan.CalculateDoseWithPresetValues(presetValues);
            if (!res.Success)
            {
                var message = string.Format("Dose calculation failed for verification plan. Output:\n{0}", res);
//                Trace.WriteLine(message);
                throw new Exception(message);
            }
        }
        return verificationPlan;
    }

    /// <summary>
    /// Create a copy of an existing beam (beams are unique to plans).
    /// </summary>
    private static string CopyBeam(Beam originalBeam, ExternalPlanSetup plan, VVector isocenter, bool getCollimatorAndGantryFromBeam)
    {
        ExternalBeamMachineParameters MachineParameters = 
            new ExternalBeamMachineParameters(originalBeam.TreatmentUnit.Id, originalBeam.EnergyModeDisplayName, originalBeam.DoseRate, originalBeam.Technique.Id, string.Empty);
        
        // Create a new beam.
        var collimatorAngle = getCollimatorAndGantryFromBeam ? originalBeam.ControlPoints.First().CollimatorAngle : 0.0;
        var gantryAngle = getCollimatorAndGantryFromBeam ? originalBeam.ControlPoints.First().GantryAngle : 0.0;
        var couchAngle = getCollimatorAndGantryFromBeam ? originalBeam.ControlPoints.First().PatientSupportAngle : 0.0;
        var metersetWeights = originalBeam.ControlPoints.Select(cp => cp.MetersetWeight);
        var beam = plan.AddSlidingWindowBeam(MachineParameters, metersetWeights, collimatorAngle, gantryAngle,
          couchAngle, isocenter);

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



  }
}
