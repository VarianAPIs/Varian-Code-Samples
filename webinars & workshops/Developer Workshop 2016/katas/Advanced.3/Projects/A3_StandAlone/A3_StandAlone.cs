using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// TODO: uncomment the line below if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]



////////////////////////////////////////////////////////////////////////////////
// A3_StandAlone.cs - Extract plan details from control points
//
//  A ESAPI v11+ script that demonstrates DVH extraction.
//
// Kata Advanced.3)    
//  Extract plan details from control points and display.
//
// Applies to:
//      Eclipse Scripting API
//      v11, 13, 13.5, 13.6, 13.7, 15.0
//
// Copyright (c) 2016 Charles Mayo, University of Michigan
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


namespace A3_StandAlone
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
        Console.Error.WriteLine(e.ToString());
        Console.WriteLine();
        Console.WriteLine("Press any key to continue");
        Console.ReadKey();
      }
    }
    static void Execute(Application app)
    {
      // TODO: add here your code
        Patient curpat = app.OpenPatientById("002441");
        Course curcourse = curpat.Courses.Where(x => x.Id == "advanced.3").Single();
        ExternalPlanSetup cureps = curcourse.ExternalPlanSetups.Where(x => x.Id == "NCPTestScript").Single();

        PlanInfoFromControlPoints(cureps);
    }
    static void PlanInfoFromControlPoints(ExternalPlanSetup cureps)
    {
        StringBuilder sb = new StringBuilder();

        //Create single list of control points in plan to simplify queries

        List<ControlPoint> cplist = new List<ControlPoint>();
        foreach (ControlPointCollection curcpc in cureps.Beams.Select(x => x.ControlPoints))
        {
            foreach (ControlPoint curcp in curcpc) cplist.Add(curcp);
        }

        sb.AppendLine(cureps.Course.Patient.Id + "\t" + cureps.Course.Id + "\t" + cureps.Id);
        sb.AppendLine();
        sb.AppendLine("N Beams: " + cplist.Select(x => x.Beam.Id).Distinct().Count().ToString());
        sb.AppendLine("Total MU: " + cplist.Select(x => new { ID = x.Beam.Id, MU = x.Beam.Meterset.Value }).Distinct().Select(x => x.MU).Sum().ToString());
        sb.AppendLine("N Gantry Angles: " + cplist.Select(x => x.GantryAngle).Distinct().Count().ToString());
        sb.AppendLine("N Collimator Angles: " + cplist.Select(x => x.CollimatorAngle).Distinct().Count().ToString());
        sb.AppendLine("N Table Angles: " + cplist.Select(x => x.PatientSupportAngle).Distinct().Count().ToString());
        sb.AppendLine("N Jaw Positions:" + cplist.Select(x => x.JawPositions).Distinct().Count().ToString());
        sb.AppendLine("N Control Points : " + cplist.Count().ToString());

        //For bonus, list beams and jaw postions
        sb.AppendLine();
        sb.AppendLine("For bonus, list beams and jaw postions");
        sb.AppendLine();

        var q = cplist.Select(x => new { b = x.Beam.Id, jaw = x.JawPositions }).Distinct();
        foreach (var qq in q) sb.AppendLine(qq.b + "\t" + qq.jaw);

        System.Windows.MessageBox.Show(sb.ToString());
    }
  }
}
