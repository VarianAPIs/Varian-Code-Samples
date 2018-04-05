////////////////////////////////////////////////////////////////////////////////
// Intermediate_10.cs - Using Structure.MeshGeometry objects to check distances relevant to clearance 
//
//  A ESAPI v11+ script that demonstrates DVH extraction.
//
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


using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Media.Media3D;//You will need to add this namespace  to be able to access the Point3D objects

// TODO: uncomment the line below if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]

namespace Intermediate_10
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

        GetDistances(cureps);
    }
    static void GetDistances(ExternalPlanSetup cureps)
    {

        //You will need to add a namespace up top to be able to access the Point3D objects using System.Windows.Media.Media3D;
        StringBuilder sb = new StringBuilder();

        Structure s_Body = cureps.StructureSet.Structures.Where(x => x.Id.Contains("BODY")).First();
        Structure s_Couch = cureps.StructureSet.Structures.Where(x => x.Id.Contains("CouchSurface")).First();

        Point3D origin = new Point3D(cureps.Beams.First().IsocenterPosition.x, cureps.Beams.First().IsocenterPosition.y, cureps.Beams.First().IsocenterPosition.z);

        double distance = s_Couch.MeshGeometry.Positions.Select(x => (x - origin).Length).Min() / 10.0f;

        sb.AppendLine("Distance Isocenter to Table (cm): " + distance.ToString("F1"));

        Point3D FaceCenter = new Point3D();
        double gantryangle;
        double tableangle;

        double disttoface = 350.0f;//Assume distance from isocenter to the face of the collimator head is 35 cm. 

        sb.AppendLine();
        sb.AppendLine("Distance From Collimator Face Center (cm)");
        sb.AppendLine("Beam\t\tBody\tCouch");

        foreach (Beam curbeam in cureps.Beams)
        {
            gantryangle = curbeam.ControlPoints.First().GantryAngle <= 180.0f ? (Math.PI / 180.0f) * curbeam.ControlPoints.First().GantryAngle : (Math.PI / 180.0f) * (curbeam.ControlPoints.First().GantryAngle - 360.0f);
            tableangle = curbeam.ControlPoints.First().PatientSupportAngle <= 180.0f ? (Math.PI / 180.0f) * curbeam.ControlPoints.First().PatientSupportAngle : (Math.PI / 180.0f) * (curbeam.ControlPoints.First().PatientSupportAngle - 360.0f);

            FaceCenter.X = origin.X + disttoface * Math.Cos(tableangle) * Math.Sin(gantryangle);
            FaceCenter.Y = origin.Y - disttoface * Math.Cos(gantryangle);
            FaceCenter.Z = origin.Z + disttoface * Math.Sin(tableangle) * Math.Sin(gantryangle);

            distance = s_Body.MeshGeometry.Positions.Select(x => (x - FaceCenter).Length).Min() / 10.0f;
            sb.Append(curbeam.Id + "\t\t" + distance.ToString("F1"));

            distance = s_Couch.MeshGeometry.Positions.Select(x => (x - FaceCenter).Length).Min() / 10.0f;
            sb.AppendLine("\t" + distance.ToString("F1"));
        }

        System.Windows.MessageBox.Show(sb.ToString());
    }
  }
}
