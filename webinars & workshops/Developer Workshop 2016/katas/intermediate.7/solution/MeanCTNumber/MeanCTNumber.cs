////////////////////////////////////////////////////////////////////////////////
// MeanCTNumbers.cs
//
//  A ESAPI v11+ script that demonstrates ct number extraction.
//
// Kata Intermediate.7)    
//	Program an ESAPI automation script that calculates the mean and 
//  std CT number within a rt-structure
//
// Applies to:
//      Eclipse Scripting API
//      v11, 13, 13.5, 13.6, 13.7, 15.0
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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Media.Media3D;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MeanCTNumber
{
    class Program
    {
        [STAThread] // Do not remove this attribute, otherwise the script will not work
        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Usage: id(can be null) pw pt structure_set rtstruct");
                return;
            }

            Console.WriteLine("Starting:");
            DateTime s_time = DateTime.Now;
            Console.WriteLine(s_time.ToString());

            try
            {
                Console.WriteLine("Logging in...");

                if (args[0].CompareTo("null") == 0)
                {
                    using (Application app = Application.CreateApplication(null, null))
                    {
                        Console.WriteLine("Running script...");
                        Execute(app, args[2], args[3], args[4]);
                    }
                }
                else
                {
                    using (Application app = Application.CreateApplication(args[0], args[1]))
                    {
                        Console.WriteLine("Running script...");
                        Execute(app, args[2], args[3], args[4]);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception was thrown:" + exception.Message);
            }


            Console.WriteLine("Ending:");
            DateTime e_time = DateTime.Now;
            Console.WriteLine(e_time.ToString());

            Console.WriteLine("Elapsed:");
            TimeSpan span = e_time - s_time;
            Console.WriteLine(span.ToString());

            Console.WriteLine("Execution finished. Press any key to exit.");
            Console.ReadKey();
        }

        // check each point on the image grid, and store the indices only
        static List<int> GetInterior(VMS.TPS.Common.Model.API.Image dose, VMS.TPS.Common.Model.API.Structure st)
        {
            List<int> result = new List<int>();
            VVector p = new VVector();
            Rect3D st_bounds = st.MeshGeometry.Bounds;

            for (int z = 0; z < dose.ZSize; ++z)
            {
                for (int y = 0; y < dose.YSize; ++y)
                {
                    for (int x = 0; x < dose.XSize; ++x)
                    {
                        p.x = x * dose.XRes;
                        p.y = y * dose.YRes;
                        p.z = z * dose.ZRes;

                        p = p + dose.Origin;

                        if (st_bounds.Contains(p.x, p.y, p.z) // trimming
                            && st.IsPointInsideSegment(p)) // this is an expensive call
                        {
                            int[,] voxels = new int[dose.XSize, dose.YSize];
                            dose.GetVoxels(z, voxels);
                            result.Add(voxels[x, y]);
                        }
                    }
                }
                GC.Collect(); // do this to avoid time out
                GC.WaitForPendingFinalizers();
            }

            return result;
        }

        static void Execute(Application app, string pid, string structure_set, string rtstruct)
        {
            // Retrieve patient information
            Patient patient = app.OpenPatientById(pid);
            if (patient == null)
                throw new ApplicationException("Cannot open patient ");

            // Iterate through all patient courses...
            foreach (var st in patient.StructureSets)
            {
                if (st.Id == structure_set)
                {
                    List<int> voxels = GetInterior(st.Image, st.Structures.First(s => s.Id == rtstruct));
                    double avg = voxels.Average();
                    Console.WriteLine("Mean:" + avg);
                    double sum = voxels.Sum(d => Math.Pow(d - avg, 2));
                    //Put it all together      
                    double std = Math.Sqrt((sum) / (voxels.Count() - 1));
                    Console.WriteLine("STD:" + std);
                }
            }

            // Close the current patient, otherwise we will not be able to open another patient
            app.ClosePatient();
        }
    }
}