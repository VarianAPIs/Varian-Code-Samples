
////////////////////////////////////////////////////////////////////////////////
// PlanIndices.cs
//
//  A ESAPI v11+ script that demonstrates calculating  Conformity index (CI), Gradient index (GI), Heterogeneity index (HI) for a plan and display the information in a message box.
//      a.CI: V100/TV
//      b.GI: V50/V100
//      c.HI: Dmax/Dp
//
// Kata Advanced.7    
//   Calaculate CI, GI and HI and display them in a message box.
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

// TODO: uncomment the line below if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]


namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
    {
      // TODO : Add here your code that is called when the script is launched from Eclipse
        // check if the plan has dose
        if (!context.PlanSetup.IsDoseValid)
        {
            MessageBox.Show("The plan selected has no valid dose.");
            return;
        }
        // search for PTV structure
        Structure ptv = context.PlanSetup.StructureSet.Structures.Where(o => o.Id == context.PlanSetup.TargetVolumeID).FirstOrDefault();
        if (ptv == null)
        {
            MessageBox.Show("Target volume is not defined.");
            return;
        }
        // make sure the volume is non-zero
        if (ptv.Volume < double.Epsilon)
        {
            MessageBox.Show("Target Volume has no contours.");
            return;
        }

        // --- calc Conformity index (CI)
        DoseValue dose100 = new DoseValue(100, DoseValue.DoseUnit.Percent);
        double v100 = context.PlanSetup.GetVolumeAtDose(ptv, dose100, VolumePresentation.AbsoluteCm3);
        double CI = v100 / ptv.Volume;

        // --- calc Gradient index (GI)
        DoseValue dose50 = new DoseValue(50, DoseValue.DoseUnit.Percent);
        double v50 = context.PlanSetup.GetVolumeAtDose(ptv, dose50, VolumePresentation.AbsoluteCm3);
        double GI = v50/v100; // C# can handle divide-by-zero (double.infinity) so not need to check the situation

        // --- calc Heterogeneity index (HI)
        // get prescription dose
        double Dp = context.PlanSetup.DosePerFraction.Dose * context.PlanSetup.NumberOfFractions.Value;
        double Dmax = context.PlanSetup.GetDoseAtVolume(ptv, 0, VolumePresentation.Relative, DoseValuePresentation.Absolute).Dose;
        double HI = Dmax / Dp; // C# can handle divide-by-zero (double.infinity) so not need to check the situation

        MessageBox.Show(string.Format("{0}\rConformity Index:{1:0.00}\rGradient Index:{2:0.00}\rHeterogenity Index:{3:0.00}", ptv.Id, CI, GI, HI));

       
    }
  }
}
