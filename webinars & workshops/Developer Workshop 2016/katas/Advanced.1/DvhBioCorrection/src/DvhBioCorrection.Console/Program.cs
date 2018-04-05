////////////////////////////////////////////////////////////////////////////////
// DVHBioCorrection project
//
//  A ESAPI v11+ script that demonstrates biocorrected DVHs
//
// Applies to:
//      Eclipse Scripting API
//          11, 13.6, 13.7, 15.0,15.1
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
using System;
using System.IO;
using DvhBioCorrection.DvhMetric;
using VMS.TPS.Common.Model.API;

namespace DvhBioCorrection.Console
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                PrintUsage();
                return;
            }

            var inputLines = File.ReadAllLines(args[0]);

            using (var app = Application.CreateApplication())
            {
                Patient patient = null;

                foreach (var inputLine in inputLines)
                {
                    var inputParts = inputLine.Split('\t');

                    var patientId = inputParts[0];
                    var courseId = inputParts[1];
                    var planSetupId = inputParts[2];
                    var structureId = inputParts[3];
                    var metricName = inputParts[4];

                    // Open a patient if the patient is null or different
                    if (patient == null || patient.Id != patientId)
                    {
                        app.ClosePatient();
                        patient = app.OpenPatientById(patientId);
                    }

                    var metricValue = DoseMetricCalculator.Calculate(patient,
                        courseId, planSetupId, structureId, metricName);

                    System.Console.WriteLine(metricValue);
                }
            }
        }

        private static void PrintUsage()
        {
            System.Console.WriteLine("Arguments: <input-file-name>");
        }
    }
}
