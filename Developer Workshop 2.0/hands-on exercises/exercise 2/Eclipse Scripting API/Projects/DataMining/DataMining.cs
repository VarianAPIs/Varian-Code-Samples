////////////////////////////////////////////////////////////////////////////////
// DataMining.cs
//
//  A ESAPI v11+ standalone executable script that demonstrates data mining.
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

namespace DataMining
{
  class Program
  {
    [STAThread]
    static void Main(string[] args)
    {
      try
      {
          using (Application app = Application.CreateApplication("allrights", "allrights"))
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
        // loop through all patients in the database, open each one and iterate 
        // through all courses. For each course with Id == "Varian", print
        // info and approval status.
        foreach (PatientSummary ps in app.PatientSummaries)
        {
            Patient p = app.OpenPatient(ps);
            foreach (Course c in p.Courses)
            {
                if (c.Id == "Varian")
                {
                    foreach (PlanSetup plan in c.PlanSetups)
                    {
                        Console.WriteLine("{0}/{1}/{2} ({3})",
                            p.Id, c.Id, plan.Id, plan.ApprovalStatus.ToString());
                        // print the max dose for each plan 
                        if (plan.Dose != null)
                        {
                            plan.DoseValuePresentation = DoseValuePresentation.Absolute;
                            DoseValue dv = plan.Dose.DoseMax3D;
                            Console.WriteLine("  ->max dose = {0}", dv.ToString());
                        }
                        // code for Step 4 follows this comment:
                        foreach (Beam beam in plan.Beams)
                        {
                            Console.WriteLine("  {0}: {1} control points.", beam.Id, beam.ControlPoints.Count());

                            foreach (ControlPoint cp in beam.ControlPoints)
                            {
                                Console.WriteLine("\tControl pt meterset = {0}", cp.MetersetWeight);
                            }
                        }
                    }
                }
            }
            app.ClosePatient();
        }
    }
  }
}
