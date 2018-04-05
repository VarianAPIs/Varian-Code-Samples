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
        double doseSum, eqd2Sum ;
        foreach (PlanSum plansum in context.PlanSumsInScope)
        {
            doseSum = eqd2Sum=0;
            foreach (PlanSetup plan in plansum.PlanSetups)
            {
                double EQD2Dose = CalculateEQD2Dose(plan, alphabeta);
                message += plan.Id + "\t" + plan.TotalPrescribedDose.Dose.ToString("0.0000") + "\t" + EQD2Dose.ToString("0.0000") + "\n";
                doseSum += plan.TotalPrescribedDose.Dose;
                eqd2Sum += EQD2Dose;
            }
            message += plansum.Id + "\t" + doseSum.ToString("0.0000") + "\t" + eqd2Sum.ToString("0.0000") + "\n";
        }
        MessageBox.Show(message);
    }
    double CalculateEQD2Dose(PlanSetup ps, double targetAB)
    {
        if (ps is BrachyPlanSetup && ((BrachyPlanSetup)ps).NumberOfPdrPulses != null)
        {
            return CalculateEQ2DoseForPDR(ps, targetAB);
        }
        double dosePerFraction = ps.UniqueFractionation.PrescribedDosePerFraction.Dose;
        double dosePerFractionInGy = dosePerFraction;
        if (ps.UniqueFractionation.PrescribedDosePerFraction.Unit == VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.cGy)
        {
            dosePerFractionInGy = dosePerFraction * 0.01;
        }
        int numberOfFractions = ps.UniqueFractionation.NumberOfFractions.Value;
        double bed = numberOfFractions * dosePerFraction * (1 + dosePerFractionInGy / targetAB);
        double eq2 = bed / (1 + 2 / targetAB);
        return eq2;
    }

    double CalculateEQ2DoseForPDR(PlanSetup ps, double targetAB)
    {
        double dosePerFraction = ps.UniqueFractionation.PrescribedDosePerFraction.Dose;
        double dosePerFractionInGy = dosePerFraction;
        if (ps.UniqueFractionation.PrescribedDosePerFraction.Unit == VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.cGy)
        {
            dosePerFractionInGy = dosePerFraction * 0.01;
        }
        int numberOfFractions = ps.UniqueFractionation.NumberOfFractions.Value;
        double dosePrescription = dosePerFractionInGy * ps.UniqueFractionation.NumberOfFractions.Value;
        int numberOfPDRPulses = (ps as BrachyPlanSetup).NumberOfPdrPulses.Value;
        double dosePerPulse = dosePrescription / numberOfPDRPulses;

        //constants in hours.
        double timeOfRepair = 1.5;
        double pulseTime = 0.1;
        double pulsePeriod = 1;

        //-----
        double mu = Math.Log(2) / timeOfRepair;
        double z = Math.Exp(-mu * pulseTime);
        double k = Math.Exp(-mu * (pulsePeriod - pulseTime));
        double s = (k / ((1 - k * z) * (1 - k * z))) * (numberOfPDRPulses * (1 - k * z) - 1 + Math.Pow(k, numberOfPDRPulses) * Math.Pow(z, numberOfPDRPulses));
        double y = 1 - Math.Exp(-mu * pulseTime);
        double fp = 1 - ((1 / (numberOfPDRPulses * mu * pulseTime)) * (numberOfPDRPulses * y - s * Math.Pow(y, 2)));

        double bed = numberOfFractions * dosePerFraction * (1 + 2 * (1 / targetAB) * (timeOfRepair / Math.Log(2)) * (dosePerPulse / pulseTime) * fp);
        double eq2 = bed / (1 + 2 / targetAB);
        return eq2;
    }

  }
}
