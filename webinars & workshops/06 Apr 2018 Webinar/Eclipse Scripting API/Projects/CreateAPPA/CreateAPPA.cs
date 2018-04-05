////////////////////////////////////////////////////////////////////////////////
// CreateAPPA.cs
//
//  A ESAPI v15.5+ script that demonstrates simple plan creation.
//
// Applies to:
//      Eclipse Scripting API
//          15.5
//
// Copyright (c) 2018 Varian Medical Systems, Inc.
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
[assembly: ESAPIScript(IsWriteable = true)]

namespace CreateAPPA
{
  class Program
  {
    static ExternalBeamMachineParameters MachineParameters =
        new ExternalBeamMachineParameters("D_Varian23EX", "6X", 600, "STATIC", string.Empty);

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
        Patient pat = app.OpenPatientById("exercise5-0");
        try
        {
            pat.BeginModifications();

            const string courseId = "AutoPlanned";
            Course course = pat.Courses.Where(o => o.Id == courseId).SingleOrDefault();
            if (course == null)

            if (course == null)
            {
                course = pat.AddCourse();
                course.Id = courseId;
            }
            StructureSet ss = pat.StructureSets.First(x => x.Id == "CT_1");
            if (course.CanAddPlanSetup(ss))
            {

                // find the PTV
                Structure ptv = ss.Structures.First(x => x.Id == "PTV");
                // Put isocenter to the center of the ptv.
                var isocenter = ptv.CenterPoint;
                //add plan and beams
                ExternalPlanSetup plan = course.AddExternalPlanSetup(ss);
                plan.SetPrescription(5, new DoseValue(2, DoseValue.DoseUnit.Gy), 1.0);
                Beam g0 = plan.AddMLCBeam(MachineParameters, null, new VRect<double>(-10, -10, 10, 10), 0, 0, 0, isocenter);
                Beam g180 = plan.AddMLCBeam(MachineParameters, null, new VRect<double>(-10, -10, 10, 10), 0, 180.0, 0, isocenter);

                    // fit beam jaws and MLC
                bool useAsymmetricXJaw = true, useAsymmetricYJaws = true, optimizeCollimatorRotation = true;
                g0.FitCollimatorToStructure(new FitToStructureMargins(0), ptv, useAsymmetricXJaw, useAsymmetricYJaws, optimizeCollimatorRotation);

                FitToStructureMargins margins = new FitToStructureMargins(1);
	            JawFitting jawFit = JawFitting.FitToRecommended;
	            OpenLeavesMeetingPoint olmp = OpenLeavesMeetingPoint.OpenLeavesMeetingPoint_Middle;
                ClosedLeavesMeetingPoint clmp = ClosedLeavesMeetingPoint.ClosedLeavesMeetingPoint_BankOne;
                g0.FitMLCToStructure(margins, ptv, optimizeCollimatorRotation, jawFit, olmp, clmp);
                g180.FitMLCToStructure(margins, ptv, optimizeCollimatorRotation, jawFit, olmp, clmp);

                // format the field ids
                g0.Id = string.Format("g{0}c{1}", 
                        g0.GantryAngleToUser(g0.ControlPoints[0].GantryAngle),
                        g0.CollimatorAngleToUser(g0.ControlPoints[0].CollimatorAngle)
                        );
                g180.Id = string.Format("g{0}c{1}",
                        g180.GantryAngleToUser(g180.ControlPoints[0].GantryAngle),
                        g180.CollimatorAngleToUser(g180.ControlPoints[0].CollimatorAngle)
                        );
                app.SaveModifications();
            }
        }
        finally {
            app.ClosePatient();
        }
    }
  }
}
