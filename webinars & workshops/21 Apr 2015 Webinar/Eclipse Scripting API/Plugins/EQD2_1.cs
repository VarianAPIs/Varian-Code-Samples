////////////////////////////////////////////////////////////////////////////////
//  
// Copyright (c) 2015 Varian Medical Systems, Inc.
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
        string message = "plan\tdose\tEQD2 Dose\n---------------------------------------------\n";
        double alphabeta = 10.0;
        foreach (PlanSetup plan in context.PlansInScope)
        {
            double EQD2Dose = CalculateEQD2Dose(plan, alphabeta);
            message += plan.Id + "\t" + plan.TotalPrescribedDose.Dose.ToString("0.0000") + "\t" + EQD2Dose.ToString("0.0000") + "\n";
        }
        MessageBox.Show(message);
    }
    double CalculateEQD2Dose(PlanSetup plan, double targetAB)
    {
        double dosePerFraction = plan.UniqueFractionation.PrescribedDosePerFraction.Dose;
        double dosePerFractionInGy = dosePerFraction;
        int numberOfFractions = plan.UniqueFractionation.NumberOfFractions.Value;
        double bed = numberOfFractions * dosePerFraction * (1 + dosePerFractionInGy / targetAB);
        double eq2 = bed / (1 + 2 / targetAB);
        return eq2;
    }
  }
}
