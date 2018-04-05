////////////////////////////////////////////////////////////////////////////////
// StructureIdFrequency.cs
//
//  An ESAPI v11+ standalone executable datamining script that works through 
//  all patients in the Aria database and generates a histogram of structure id 
//  naming frequency and exports that to a CSV file.
//
//  This script was developed to help Eclipse users understand the variability 
//  of structure naming in their enterprise.
//
// To use this script:
//  1) Compile in Visual Studio with the Release build configuration.
//  2) Copy ".\bin\release\StructureIdFrequency.exe" to the computer where you want 
//    to run the StructureIdFrequency.exe program - must have Eclipse v11+ installed.
//  3) On that same computer, create directory "C:\temp", or change generated 
//    CSV path as needed below (variable ReportPath).
//  4) Open a DOS command prompt, navigate to the directory where 
//    StructureIdFrequency.exe was copied to, and run the program.  Sign in with 
//    a valid Aria userid.
//  5) Import 'c:\temp\structureids-nomarkers-sinceJune2013.csv' into Excel to
//    view results.
//
// Known issues:  If a structure ID has a comma (,) in it, the frequency will 
//                be reported incorrectly in the exported CSV file.
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
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Collections;

namespace StructureIdFrequency
{
    class Program
    {
        // change the following two items to match the file name you want to use and the search range you want to use.
        const string ReportPath = @"c:\temp\structureids-nomarkers-sinceJune2013.csv";
        static DateTime searchSince = new DateTime(2013, 6, 1);   // look for structures created since June 1st 2013

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
        static void Execute(Application app)
        {
            // store structure id and usage frequency in 'structureIdHistogram'
            Dictionary<string, int> structureIdHistogram = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            var patSummaries = app.PatientSummaries;
            System.Console.WriteLine("collecting ids for {0} patients", patSummaries.Count());

            int iIndex = 0;
            double searchDays = (DateTime.Now - searchSince).TotalDays;
            foreach (PatientSummary ps in patSummaries)
            {
                Patient p = app.OpenPatient(ps);

                // Loop only through structure sets created after specified 'searchSince' date.  This could be a relevant filter
                // since treatment planning system changes / process changes / naming changes occur over time.
                // if no date filter is needed, just change loop to 'foreach (StructureSet ss in p.StructureSets)'
                foreach (StructureSet ss in p.StructureSets.Where(sst => (DateTime.Now - sst.HistoryDateTime).TotalDays <= searchDays))
                {
                    // loop through structures, filter out markers.
                    foreach (Structure s in ss.Structures.Where(st => st.DicomType != "MARKER"))
                    {
                        // add the structure id to the histogram or increase the usage count if already there.
                        int count = 0;
                        if (structureIdHistogram.TryGetValue(s.Id, out count))
                        {
                            structureIdHistogram[s.Id] = count + 1;
                        }
                        else
                        {
                            structureIdHistogram.Add(s.Id, 1);
                        }
                    }
                }
                app.ClosePatient();

                // print out a status message to the console every 100th patient.
                if (++iIndex % 100 == 0)
                    Console.WriteLine("{0} % completed.", (iIndex / (double)patSummaries.Count()) * 100);

            }
            System.Console.WriteLine("DONE collecting ids, dumping file.");
            List<KeyValuePair<string, int>> myList = structureIdHistogram.ToList();

            // sort to put the structures with highest frequency usage first.
            myList.Sort((firstPair, nextPair) =>
            {
                return nextPair.Value.CompareTo(firstPair.Value);
            }
            );
            // print out structure id and count
            using (System.IO.StreamWriter idFile = new System.IO.StreamWriter(ReportPath))
            {
                // write a header
                idFile.WriteLine("Structure Id,Count");
                // write structure ids and frequency to the CSV file
                foreach (var kvp in myList)
                {
                    string line = string.Format("{0},{1}", kvp.Key, kvp.Value);
                    idFile.WriteLine(line);
                }
                idFile.Close();
            }
            System.Console.WriteLine("id file written to {0}", ReportPath);
        }
    }
}
