////////////////////////////////////////////////////////////////////////////////
// DVHExtract.cs
//
//  A ESAPI v11+ script that demonstrates DVH extraction.
//
// Kata Intermediate.1)	
//  Extract the DVH for the Bladder for your loaded case and write it to a CSV file.
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
        // declare local variables that reference the objects we need.
        PlanSetup plan = context.PlanSetup;
        if (plan == null)
        {
            MessageBox.Show("Please load a plan before running this script.");
            return;
        }
        StructureSet ss = context.StructureSet;
        if (ss == null)
        {
            MessageBox.Show("Please load a structure set before running this script.");
            return;
        }
        var listStructures = context.StructureSet.Structures;
        // 'listStructures' if of type IEnumerable<Structure>


        // loop through structure list and find a structure named "Bladder"
        Structure bladder = null;
        foreach (Structure scan in listStructures)
        {
            if (scan.Id == "Bladder")
            {
                bladder = scan;
            }
        }
        if (bladder == null)
        {
            MessageBox.Show("Bladder not found!");
            return;
        }

        string msg = string.Format("bladder volume = {0}", bladder.Volume);
        MessageBox.Show(msg, "Varian Developer");
        
        // extract DVH data for bladder using bin width of 0.1.
        DVHData dvh = plan.GetDVHCumulativeData(bladder, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.1);
        
        string filename = string.Format(@"c:\temp\bladder_dvh-{0}.csv", context.CurrentUser.Name);
        System.IO.StreamWriter dvhFile = new System.IO.StreamWriter(filename);
        // write a header
        dvhFile.WriteLine("Dose,Volume");
        // write all dvh points for the PTV.
        foreach (DVHPoint pt in dvh.CurveData)
        {
            string line = string.Format("{0},{1}", pt.DoseValue.Dose, pt.Volume);
            dvhFile.WriteLine(line);
        }
        dvhFile.Close();
        msg = string.Format("dvh file written to {0}", filename);
        MessageBox.Show(msg, "Varian Developer");
   }
  }
}
