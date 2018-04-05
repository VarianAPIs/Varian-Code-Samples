////////////////////////////////////////////////////////////////////////////////
// DoseProfiles.cs
//
//  A ESAPI v11+ script that demonstrates dose profile extraction.
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

    public void Execute(ScriptContext context /*, System.Windows.Window window*/)
    {
        // Show the coordinates of isocenter of first beam
        var isoc = context.PlanSetup.Beams.First().IsocenterPosition;

        string msg = isoc.x.ToString() + ", " +
                     isoc.y.ToString() + ", " +
                     isoc.z.ToString();
        MessageBox.Show("Isocenter (x,y,z in mm): " + msg);

        // Show dose at isocenter
        context.PlanSetup.DoseValuePresentation = DoseValuePresentation.Absolute;

        var dose = context.PlanSetup.Dose.GetDoseToPoint(isoc);
        MessageBox.Show("Dose at isocenter: " + dose.ToString());

        // Get dose values between isocenter and location of dose maximum
        var dosemax = context.PlanSetup.Dose.DoseMax3DLocation;

        double[] values = new double[20];
        var profile = context.PlanSetup.Dose.GetDoseProfile(isoc, dosemax, values);

        msg = String.Empty;
        foreach (var profilePoint in profile)
        {
            msg = msg + profilePoint.Value.ToString();
            msg = msg + "\n";
        }
        MessageBox.Show(msg);

        // Save values to a file
        using (System.IO.TextWriter writer = new System.IO.StreamWriter("c:\\temp\\profile.txt"))
        {
            writer.WriteLine("X, Y, Z, Dose");

            foreach (var profilePoint in profile)
            {
                writer.WriteLine(profilePoint.Position.x + "," +
                                 profilePoint.Position.y + "," +
                                 profilePoint.Position.z + "," +
                                 profilePoint.Value);
            }
        }   
        

        
    }
  }
}
