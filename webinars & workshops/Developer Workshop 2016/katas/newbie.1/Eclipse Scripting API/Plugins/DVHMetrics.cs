////////////////////////////////////////////////////////////////////////////////
// DVHMetrics.cs
//
//  A ESAPI v11+ script that demonstrates DVH Metric calculation.
//
// Kata newbie.1)	
//  Newbie.1) Extract and display relevant DVH metrics for your loaded case 
//  using an ESAPI script.
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

    public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
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

        // first find PTV, Rectum, and Bladder structures
        Structure ptv68 = null, ptv56 = null, rectum = null, bladder = null;
        foreach (Structure scan in ss.Structures)
        {
            if (scan.Id == "PTV_6800")
            {
                ptv68 = scan;
            }
            if (scan.Id == "PTV_5600")
            {
                ptv56 = scan;
            }
            else if (scan.Id == "Rectum")
            {
                rectum = scan;
            }
            else if (scan.Id == "Bladder")
            {
                bladder = scan;
            }
        }
#if false
        // LINQ equivalents
        Structure ptv68 = (from s in ss.Structures
                        where s.Id.CompareTo("PTV_6800") == 0
                        select s).FirstOrDefault();

        Structure rectum = (from s in ss.Structures
                        where s.Id.CompareTo("Rectum") == 0
                        select s).FirstOrDefault();

        Structure bladder = (from s in ss.Structures
                        where s.Id.CompareTo("Bladder") == 0
                        select s).FirstOrDefault();
#endif
        if (ptv68 == null)
        {
            MessageBox.Show("Couldn't find ptv 'PTV_6800'!");
            return;
        }
        if (ptv56 == null)
        {
            MessageBox.Show("Couldn't find ptv 'PTV_5600'!");
            return;
        }
        if (rectum == null)
        {
            MessageBox.Show("Couldn't find normal structure 'Rectum'!");
            return;
        }
        if (bladder == null)
        {
            MessageBox.Show("Couldn't find  normal structure 'Bladder'!");
            return;
        }
        // evaluate the metrics
        DoseValue d03cc = plan.GetDoseAtVolume(ptv68, 0.03, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute);
        double v56Gy = plan.GetVolumeAtDose(rectum, new DoseValue(56, DoseValue.DoseUnit.Gy), VolumePresentation.Relative);
        double v40Gy = plan.GetVolumeAtDose(bladder, new DoseValue(40, DoseValue.DoseUnit.Gy), VolumePresentation.Relative);

        DVHData dvh56 = plan.GetDVHCumulativeData(ptv56, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.1);
        DVHData dvh56Rel = plan.GetDVHCumulativeData(ptv56, DoseValuePresentation.Relative, VolumePresentation.AbsoluteCm3, 0.1);
        double ptv56MeanGy = dvh56.MeanDose.Dose;
        double ptv56MeanRel = dvh56Rel.MeanDose.Dose;

        // format string and present to user (See string.Format help for more sophisticated ways to format a string).
        string format = 
            " Structure    Metric         Goal        Actual\n"+
            "---------  -----------     --------    ---------\n" +
            "PTV_68     D0.03cc[Gy]     <71.5 Gy    {0:0.000}\n" +
            "PTV_56     Mean[Gy]        Report      {1:0.000}\n" +
            "PTV_56     Mean[%]         Report      {2:0.0}\n" +
            "RECTUM     V56Gy[%]        <5 %        {3:0.0}\n" +
            "BLADDER    V40Gy[%]        <40 %       {4:0.0}\n";
        string message = string.Format(format, d03cc.Dose, ptv56MeanGy, ptv56MeanRel, v56Gy, v40Gy);
        MessageBox.Show(message, "Varian Developer");
    }
  }
}
