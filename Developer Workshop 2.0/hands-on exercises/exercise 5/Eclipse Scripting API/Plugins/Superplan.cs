////////////////////////////////////////////////////////////////////////////////
// Superplan.cs
//
//  A ESAPI v13+ research mode script that demonstrates plan automation
//  features.
//
// Copyright (c) 2014 Varian Medical Systems, Inc.
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
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window*/)
    {
        Patient patient = context.Patient;
        if (patient == null)
            throw new ApplicationException("Please load a patient.");

        // Get active structureset
        StructureSet ss = context.StructureSet;
        if (ss == null)
            throw new ApplicationException("Please load a structure set.");

        // Check that unique body structure, PTV and Rectum structures exist
        Structure body = ss.Structures.Where(o => o.Id == "BODY").FirstOrDefault();
        Structure ptv = ss.Structures.Where(o => o.Id == "PTV").FirstOrDefault();
        Structure rectum = ss.Structures.Where(o => o.Id == "Rectum1").FirstOrDefault();
        if (body == null || ptv == null || rectum == null)
            throw new ApplicationException(string.Format("Cannot find required structures (BODY, PTV, Rectum) from Structureset '{0}'", ss.Id));

        // Enable modifications 
        patient.BeginModifications();

        // Get or create course with Id 'Superplan'
        const string courseId = "Superplan";
        Course course = patient.Courses.Where(o => o.Id == courseId).SingleOrDefault();
        if (course == null)
        {
            course = patient.AddCourse();
            course.Id = courseId;
        }

        // Create a new plan
        ExternalPlanSetup plan = course.AddExternalPlanSetup(ss);
        int fractions = 20;
        double prescribedPercentage = 1.0;
        DoseValue fractiondose = new DoseValue(2.50, DoseValue.DoseUnit.Gy);  // TODO: if needed change to cGy to match your system configuration
        plan.UniqueFractionation.SetPrescription(fractions, fractiondose, prescribedPercentage);

        // Add fields
        const int nfields = 5;
        ExternalBeamMachineParameters parameters = new ExternalBeamMachineParameters("Varian 23EX", "6X", 600, "STATIC", null); // TODO: change machine id to yours
        VVector isocenter = ptv.CenterPoint;
        for (int n = 0; n < nfields; n++)
        {   // add a 10 cm x 10 cm field
            Beam beam = plan.AddStaticBeam(parameters, new VRect<double>(-50, -50, 50, 50), 0, Math.Round(360.0 / nfields * n, 0), 0, isocenter);
        }
        // end of first part.
        MessageBox.Show("Plan created, open course SuperPlan, Choose Plan1 to view results.", "Varian Developer");
        
        // Set optimization constraints
        OptimizationSetup optimizationSetup = plan.OptimizationSetup;
        optimizationSetup.AddPointObjective(ptv, OptimizationObjectiveOperator.Lower, new DoseValue(49.50, DoseValue.DoseUnit.Gy), 100, 100.0);
        optimizationSetup.AddPointObjective(ptv, OptimizationObjectiveOperator.Upper, new DoseValue(52.00, DoseValue.DoseUnit.Gy), 0, 100.0);
        optimizationSetup.AddPointObjective(rectum, OptimizationObjectiveOperator.Upper, new DoseValue(20.00, DoseValue.DoseUnit.Gy), 40.0, 100.0);
        MessageBox.Show("Plan '" + plan.Id + "' in course '" + course.Id + "' for patient '" + patient.Id + "' has been created");

        // optimize for 30 iterations
        if (!plan.Optimize(30).Success)
        {
            MessageBox.Show("Optimization failed", "Varian Developer", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // run LMC
        if (!plan.CalculateLeafMotions().Success)
        {
            MessageBox.Show("LMC failed", "Varian Developer", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // calculate final dose
        CalculationResult doseCalc = plan.CalculateDose();
        if (!doseCalc.Success)
        {
            MessageBox.Show("Dose calculation failed, logs shown next.", "Varian Developer", MessageBoxButton.OK, MessageBoxImage.Error);
            // write calculate logs to a file and show them to the user.
            string filename = writeCalculationLogs(plan);
            // 'Start' generated TXT file to launch Notepad window
            System.Diagnostics.Process.Start(filename);
            // Sleep for a few seconds to let Excel window start
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            return;
        }

        MessageBox.Show("Done");
    }

    public string writeCalculationLogs(PlanSetup plan)
    {
        // write calculate logs to a file
        string temp = System.Environment.GetEnvironmentVariable("TEMP");
        string filename = temp + "\\calculcationlog.txt";
        using (TextWriter writer = new StreamWriter(filename))
        {
            foreach (Beam beam in plan.Beams)
            {
                writer.WriteLine(string.Format("Calculation log for beam {0}", beam.Id));
                foreach (BeamCalculationLog log in beam.CalculationLogs)
                {
                    string logCategory = log.Category;

                    writer.WriteLine("Category: " + logCategory);
                    foreach (string line in log.MessageLines)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }
        return filename;
    }
  }
}
