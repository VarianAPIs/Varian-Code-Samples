////////////////////////////////////////////////////////////////////////////////
// ShowControlPointInfo.cs
//
//  A ESAPI v11+ script that demonstrates data extraction using a plugin script.
//
// Kata Newbie.4)	
//  Extract and display MLC control point information for beam 1 of the first 
//  Planning Approved plan using either an ESAPI script or Visual Scripting.
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
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
    {
        Patient patient = context.Patient;
        if (patient == null)
        {
            MessageBox.Show("Load a patient!");
            return;
        }
        foreach (Course c in patient.Courses)
        {
            foreach (ExternalPlanSetup ps in c.ExternalPlanSetups)
            {
                if (ps.ApprovalStatus == PlanSetupApprovalStatus.PlanningApproved)
                {
                    // found first approved plan.  Select the first beam that's not a setup field.
                    Beam first = ps.Beams.Where(b => b.IsSetupField == false).First();
                    if (first != null) 
                    {
                        ControlPoint firstCP = first.ControlPoints[0];
                        float[,] lp = firstCP.LeafPositions;
                        double msw = firstCP.MetersetWeight;
                        double gantry = firstCP.GantryAngle;
                        string leafPosns = formatLeafPositions(lp);
                        string msg = string.Format("patient {0}, course {1}, plan {2}, beam {3}\n", patient.Id, c.Id, ps.Id, first.Id);
                        msg += string.Format("Control point 1 :\n \tmeterset={0}\n, \tgantry={1}\n, \tleaf positions=\n{2}", msw, gantry, leafPosns);
                        MessageBox.Show(msg, "Varian Developer");
                        return;
                    }
                        

                }
            }
        }
        MessageBox.Show("No approved plan found", "Varian Developer");
    }
    string formatLeafPositions(float[,] lp)
    {
        // 2 dimensional array of leaf positions. Each bank (carriage)
        // of leaves has an array of leaf positions. There are 2 banks 
        // of opposing leaves.
        
        
        //create the header
        char bank = 'A';
        string header = "";
        for (int bankIndex = 0; bankIndex <= lp.GetUpperBound(0); bankIndex++)
        {
            header += "Bank " + bank + "\t";
            bank++;
        }
        string formatted = header + "\n";

        for (int leafIndex = 0; leafIndex <= lp.GetUpperBound(1); leafIndex++)
        {
            for (int bankIndex = 0; bankIndex <= lp.GetUpperBound(0); bankIndex++)
            {
                formatted += lp[bankIndex, leafIndex].ToString("0.0") + "\t";
            }
            formatted += "\n";
        }
        return formatted;
    }
  }
}
