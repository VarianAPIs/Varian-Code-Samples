////////////////////////////////////////////////////////////////////////////////
// FindLargestVolume.cs
//
//  A ESAPI v11+ script that demonstrates data mining.
//
// Kata Intermediate.4)    
//  Create an ESAPI data mining script that finds the normal structure with 
//  the largest volume for all patients in Eclipse.
//
// Applies to:
//      Eclipse Scripting API
//          v11, 13.6, 13.7, 15.0,15.1
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

// TODO: uncomment the line below if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]

namespace FindLargestVolume
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
        int structureCount = 0;
        string structureName = "";
        double maxVolume = -1;
        foreach (var ps in app.PatientSummaries)
        {
            Patient patient = app.OpenPatient(ps);
            try
            {
                System.Console.WriteLine("searching patient {0}", patient.Id);
                foreach (StructureSet ss in patient.StructureSets)
                {
                    foreach (Structure structure in ss.Structures)
                    {
                        structureCount++;

                        if((structure.DicomType == "ORGAN" || structure.DicomType == "AVOIDANCE") &&
                            structure.Volume > maxVolume)
                        {
                            maxVolume = structure.Volume;
                            structureName = string.Format("{0}/{1}/{2}", patient.Name, ss.Id, structure.Id);
                        }
                    }
                }
            }
            finally
            {
                app.ClosePatient();
            }
        }
        System.Console.WriteLine("Found {0} total structures.", structureCount);
        System.Console.WriteLine("The one with the largest volume is {0}.", structureName);
        System.Console.WriteLine("Volume is {0} cc.", maxVolume);
    }
  }
}
