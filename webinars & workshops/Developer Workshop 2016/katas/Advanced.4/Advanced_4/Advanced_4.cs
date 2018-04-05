////////////////////////////////////////////////////////////////////////////////
// Advanced_4.cs
//
//  A ESAPI v11+ script that demonstrates extraction of DVH metrics.
//
// Kata Advanced.4)    
//  Create a data mining script that finds all patients having a specific structure
//  and report mean dose, max dose, and a DVH metric. 
//
// Applies to:
//      Eclipse Scripting API
//      v11, 13, 13.5, 13.6, 13.7, 15.0
//
// Copyright (c) 2016 Richard Popple, The University of Alabama at Birmingham
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
using System.IO;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Advanced_4
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                using (Application app = Application.CreateApplication())
                {
                    Execute(app);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
        static void Execute(Application app)
        {
            // Hard coded parameters. For a real-world application, some or all of these would be input by the user 
            string structureOfInterest = "Bladder";
            double doseOfInterestGy = 50.0;
            DoseValue doseValueOfInterest = new DoseValue(doseOfInterestGy, DoseValue.DoseUnit.Gy);
            VolumePresentation volumePresentation = VolumePresentation.Relative;

            // Output file. Again hard coded - in a real world application, we would likely use a file selection
            // dialog or some other method of getting the output destination from the user.
            string outputFileName = String.Format("Advanced_4_{0}.csv", structureOfInterest);
            string outputDestinationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // String builder to contain result
            StringBuilder result = new StringBuilder();
            result.AppendFormat("ID,Course,Plan,Mean dose,Max dose, V{0}\n", doseValueOfInterest.Dose);

            // Loop through the patients in the database. WARNING: In a typical clinical database, this
            // will require a long time.
            foreach (var patientSummary in app.PatientSummaries)
            {
                // Retrieve patient information
                Patient patient = app.OpenPatient(patientSummary);
                if (patient == null)
                    throw new ApplicationException("Cannot open patient " + patientSummary.Id);

                string message = string.Format("Patient: {0}", patient.Id);
                Console.WriteLine(message);

                // Iterate through all completed patient courses and planning approved plans
                var courses = patient.Courses.Where(c => c.CompletedDateTime != null).ToList();
                foreach (var course in courses)
                {
                    var planSetups = course.PlanSetups.Where(p => (p.Dose != null) & (p.ApprovalStatus == PlanSetupApprovalStatus.PlanningApproved)).ToList();
                    foreach (var planSetup in planSetups)
                    {
                        try
                        {
                            // Check for structure of interest and compute DVH metrics
                            var roi = planSetup.StructureSet.Structures.FirstOrDefault(o => o.Id.Equals(structureOfInterest));
                            if (roi != null)
                            {
                                planSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
                                double volumeAtDose = planSetup.GetVolumeAtDose(roi, doseValueOfInterest, volumePresentation);
                                var dvh = planSetup.GetDVHCumulativeData(roi, DoseValuePresentation.Absolute, volumePresentation, planSetup.TotalDose.Dose / 1000.0);
                                result.AppendFormat("{0},{1},{2},{3:0.0000},{4:0.0000},{5:0.00}\n", patient.Id, course.Id, planSetup.Id, dvh.MeanDose.Dose, dvh.MaxDose.Dose, volumeAtDose);

                                // Display message
                                message = string.Format("   --> Found {0} in course {1}, plan {2}", structureOfInterest, course.Id, planSetup.Id);
                                Console.WriteLine(message);
                            }
                        }
                        catch (ApplicationException exception)
                        {
                            // In case of any error we will display some useful information...
                            string errorMsg = string.Format("Exception was thrown. Patient Id: {0}, Course: {1}, Exception: {2}", patient.Id, course.Id, exception.Message);
                            Console.WriteLine(errorMsg);
                            // ... and then move to the next patient
                            continue;
                        }
                    }
                }
                // Close the current patient, otherwise we will not be able to open another patient
                app.ClosePatient();
            }

            // Write results to file
            string filePath = Path.Combine(outputDestinationDirectory, outputFileName);
            File.WriteAllText(filePath, result.ToString());

            Console.WriteLine("Done. Results written to");
            Console.WriteLine(filePath);
            Console.WriteLine("Press enter to close.");
            Console.ReadLine();
        }
    }
}
