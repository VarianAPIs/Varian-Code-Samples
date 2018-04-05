////////////////////////////////////////////////////////////////////////////////
// VolumeAtPercentDose.cs
//
//  A ESAPI v11+ script that calculate the total volume at given percent prescription doses.
//
// Kata Intermediate.8)	
//  Calculate the total volume at 50% prescripted doses and disply the result in a message box.
//  method CalculateVolAtPercentPresDose(...)  can be used for any given percent dose
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
using System.Drawing;
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
            Patient patient = context.Patient;
            if (context.PlanSetup == null)
            {
                System.Windows.MessageBox.Show("Please load a patient plan before the calculation.", "Volume Calculation Results");
                return;
            }
            if (!context.PlanSetup.IsDoseValid)
            {
                System.Windows.MessageBox.Show("This plan has no dose calculated, Please load plan with 3D dose.", "Volume Calculation Results");
                return;
            }

            // set precentage to 50 for this excercise
            const double percentVal = 50.0;

            //call calculation method for the given dose level      
            var totalVol = CalculateVolAtPercentPresDose(context.PlanSetup, percentVal);

            // display the total volume at 50 percent dose in message box
            string msgFormat =
            " Patient ID          Plan ID         Total Vol. at 50% Pres. Dose\n" +
            "------------       ----------       ------------------------------\n" +
            "{0}                {1}                    {2} [cm3]\n";
           

            var msg = String.Format(msgFormat, patient.Id, context.PlanSetup.Id, totalVol.ToString());
            System.Windows.MessageBox.Show(msg, "Volume Calculation Results");

        }
        /// <summary>
        /// Calculate a total volume for the loaded patient plan on given percent level of prescitoion dose
        /// and display the result in message box.
        /// </summary>
        /// <param name="curPlan"> the current loaded patient plan</param>
        /// <param name="percentVal"> the percent level of prescition dose</param>
        /// <returns></returns>
        public double CalculateVolAtPercentPresDose(PlanSetup curPlan, double percentVal)
        {

            double totalVol = 0.0;
            int totalNum = 0;

            // get total prescribed dose, only consider on single plan for this excercise
            var presDose = curPlan.TotalDose.Dose;
            curPlan.DoseValuePresentation = DoseValuePresentation.Absolute;
            var curPlanDose = curPlan.Dose;

            int xDoseCount = curPlanDose.XSize;
            // allocated buffer for dose array in x direction
            double[] xDoseVals = new double[xDoseCount];

            var percentDose = presDose * percentVal / 100;

            // search through 3D dose matrix 
            for (double z = 0.0; z < curPlanDose.ZSize * curPlanDose.ZRes; z += curPlanDose.ZRes)
            {
                for (double y = 0.0; y < curPlanDose.YSize * curPlanDose.YRes; y += curPlanDose.YRes)
                {
                    // get first and last point to extract dose profile with given z and y
                    VVector start = curPlanDose.Origin + curPlanDose.YDirection * y + curPlanDose.ZDirection * z;
                    VVector stop = start + curPlanDose.XDirection * curPlanDose.XRes * (curPlanDose.XSize - 1);

                    DoseProfile profile = null;
                    profile = curPlanDose.GetDoseProfile(start, stop, xDoseVals);

                    // compare doses in x direction
                    foreach (var profilePoint in profile)
                    {
                        if (profilePoint.Value >= percentDose)
                        {
                            totalNum++;

                        }
                    }

                }
            }

            // calculate total Volume in cm
            totalVol = curPlanDose.XRes * curPlanDose.YRes * curPlanDose.ZRes * totalNum / 1000.0;

            return Math.Round(totalVol, 2);

        }

    }
}