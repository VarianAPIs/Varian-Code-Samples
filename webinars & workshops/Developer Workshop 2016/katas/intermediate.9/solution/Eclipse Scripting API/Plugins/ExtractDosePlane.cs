////////////////////////////////////////////////////////////////////////////////
// ExtractDosePlane.cs
//
//  A ESAPI v11+ script that demonstrates dose plane extraction.
//
// Kata Intermediate.9)	
//  Program an ESAPI script that extracts planar dose for the first beam of 
//  the selected verification plan and writes it out as a CSV file 
//  (or bitmap, or MapCheck2 format, or Matrixx file format).
//
// Applies to:
//      Eclipse Scripting API
//          11, 13.6, 13.7, 15.0,15.1
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
        static double MMtoCM(double dMM)
        {
            return dMM / 10.0;
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
    {
        PlanSetup plan = context.PlanSetup;
        StructureSet ss = context.StructureSet;
        if (plan == null)
        {
            MessageBox.Show("Load a plan!");
            return;
        }
        if (plan.IsDoseValid == false)
        {
            MessageBox.Show("plan has no dose calculated!");
            return;
        }
        //User places point called "TopLeft" in top left corner where scan will start before running script
        Structure refMarker = (from s in ss.Structures
                               where
                                 s.Id == "TopLeft"
                               select s).FirstOrDefault();
        if (refMarker == null)
        {
            MessageBox.Show("No marker point called TopLeft found.");
            return;
        }
        VVector topLeft = refMarker.CenterPoint;
        
        //User places point called "BottomRight" in bottom right corner where scan will stop before running script
        Structure refMarker2 = (from s in ss.Structures
                               where
                                 s.Id == "BottomRight"
                               select s).FirstOrDefault();
        if (refMarker == null)
        {
            MessageBox.Show("No marker point called BottomRight found.");
            return;
        }
        VVector bottomRight = refMarker2.CenterPoint;
        VVector topRight = new VVector(bottomRight.x, topLeft.y, topLeft.z);
        VVector bottomLeft = new VVector(topLeft.x, topLeft.y, bottomRight.z);

        int column = 255; // arbitrarily make 255x255 (since that's what Excel can handle)
        int row = 255;
        double[] buffer = new double[column];

        double xDist = (VVector.Distance(topLeft, topRight)) / (double)column;
        string filename = string.Format(@"c:\temp\doseplane.csv");
        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filename, false, Encoding.ASCII))
        {
            sw.Write("Y[cm]...X[cm],");
            for (int i = 0; i < column; i++)
            {
                sw.Write("{0},", MMtoCM(Math.Round(topLeft.x + (xDist * i), 3)));
            }
            sw.WriteLine("");
            double zDist = (VVector.Distance(topLeft, bottomLeft)) / (double)row;
            for (int j = 0; j < row; j++)
            {
                // figure out new start and stop points for the row we are scanning, then get the dose profile (scan it)
                double newZ = topLeft.z - (zDist * j);
                VVector newRowStart = topLeft;
                VVector newRowEnd = topRight;
                newRowStart.z = newZ;
                newRowEnd.z = newZ;
                // scan the row
                DoseProfile dp = plan.Dose.GetDoseProfile(newRowStart, newRowEnd, buffer);

                sw.Write("{0},", MMtoCM(Math.Round(newZ, 3)));
                foreach (var profilePt in dp)
                {
                    sw.Write("{0},", Math.Round(profilePt.Value, 6));
                }
                sw.WriteLine("");
            }

            sw.Flush();
            sw.Close();

            MessageBox.Show(string.Format(@"File written to '{0}'", filename), "Varian Developer");
        }
        // 'Start' generated CSV file to launch Excel window
        System.Diagnostics.Process.Start(filename);
        // Sleep for a few seconds to let Excel to start
        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
    }
    }
}
