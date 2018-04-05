////////////////////////////////////////////////////////////////////////////////
// ExportBatchDVHs.cs
//
//  A ESAPI v11+ script that demonstrates Batch DVH export.
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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ExportBatchDVHs
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                using (Application app = Application.CreateApplication(null, null))
                {
                    Execute(app);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
        // this method works through the entire Eclipse database, opening each patient,
        // looking through each course for a plan with valid dose.  A DHV csv file
        // is dumped for each plan's target structure in the directory c:\temp\dvhdump.
        static void Execute(Application app)
        {
            string outputdir = @"c:\temp\dvhdump";
            System.IO.Directory.CreateDirectory(outputdir);

            var patSummaries = app.PatientSummaries;
            foreach (PatientSummary ps in patSummaries)
            {
                Patient p = app.OpenPatient(ps);
                foreach (Course c in p.Courses)
                {
                    // select plans from the course that have valid dose
                    foreach (PlanSetup plan in c.PlanSetups.Where(x => x.IsDoseValid))
                    {
                        // find the planning target
                        Structure target = plan.StructureSet.Structures.Where(x => x.Id == plan.TargetVolumeID).FirstOrDefault();
                        // extract the DVH of the planning target, dump to a file in c:\temp\dvhdump directory.
                        if (plan.Dose != null && target != null)
                        {
                            DVHData dvh = plan.GetDVHCumulativeData(target, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.1);
                            string filename = string.Format(@"{0}\{1}_{2}_{3}_{4}-dvh.csv",
                                outputdir, p.Id, c.Id, plan.Id, target.Id);
                            DumpDVH(filename, dvh);
                            Console.WriteLine(filename);
                        }
                    }
                }
                app.ClosePatient();
            }
        }
        static void DumpDVH(string filename, DVHData dvh)
        {
            System.IO.StreamWriter dvhFile = new System.IO.StreamWriter(filename);
            // write a header
            dvhFile.WriteLine("Dose,Volume");
            // write all dvh points for the PTV.
            foreach (DVHPoint pt in dvh.CurveData)
            {
                string line = string.Format("{0},{1}", pt.DoseValue.Dose, pt.Volume);
                dvhFile.WriteLine(line);
            }
            dvhFile.Close();
        }
    }
}
